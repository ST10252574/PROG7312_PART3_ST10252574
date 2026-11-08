using SmartX.Gateway.Api;

namespace SmartX.Gateway.Api.DataStructures;

public sealed record MeshPathResult(
    bool Reachable,
    IReadOnlyList<string> Path,
    int Hops,
    double TotalCost);

public sealed record MeshMstResult(IReadOnlyList<MeshLink> Links, double TotalCost);

public sealed class MeshGraph
{
    private readonly Dictionary<string, MeshNode> nodes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<MeshLink>> adjacency = new(StringComparer.Ordinal);
    private readonly List<MeshLink> links = [];
    private readonly HashSet<string> linkKeys = new(StringComparer.Ordinal);

    public IReadOnlyList<MeshNode> Nodes => nodes.Values.OrderBy(node => node.Id).ToList();
    public IReadOnlyList<MeshLink> Links => links.ToList();
    public bool ContainsNode(string nodeId) => nodes.ContainsKey(nodeId);

    public void AddNode(MeshNode node)
    {
        if (string.IsNullOrWhiteSpace(node.Id) || !double.IsFinite(node.X) || !double.IsFinite(node.Y))
            throw new ArgumentException("A mesh node must have an ID and finite display coordinates.", nameof(node));
        if (!nodes.TryAdd(node.Id, node))
            throw new ArgumentException($"Mesh node '{node.Id}' is already registered.", nameof(node));
        adjacency.Add(node.Id, []);
    }

    public void AddLink(MeshLink link)
    {
        if (!nodes.ContainsKey(link.FromId) || !nodes.ContainsKey(link.ToId) ||
            link.FromId == link.ToId || !double.IsFinite(link.Cost) || link.Cost <= 0 ||
            !double.IsFinite(link.LatencyMs) || link.LatencyMs < 0)
            throw new ArgumentException("A mesh link must join two registered nodes and have valid costs.", nameof(link));

        var key = LinkKey(link.FromId, link.ToId);
        if (!linkKeys.Add(key)) return;
        links.Add(link);
        adjacency[link.FromId].Add(link);
        adjacency[link.ToId].Add(new MeshLink(link.ToId, link.FromId, link.Cost, link.LatencyMs));
    }

    public MeshPathResult LowestCostPath(string startId, string destinationId)
    {
        if (!nodes.TryGetValue(startId, out var start) ||
            !nodes.TryGetValue(destinationId, out var destination) ||
            IsOffline(start) || IsOffline(destination))
            return new MeshPathResult(false, Array.Empty<string>(), 0, 0);

        var distances = new Dictionary<string, double>(StringComparer.Ordinal) { [startId] = 0 };
        var previous = new Dictionary<string, string>(StringComparer.Ordinal);
        var pending = new BinaryMinHeap<(string NodeId, double Distance)>();
        pending.Enqueue((startId, 0), 0);

        while (pending.TryDequeue(out var current))
        {
            if (current.Distance > distances.GetValueOrDefault(current.NodeId, double.PositiveInfinity) + 1e-9)
                continue;
            if (current.NodeId == destinationId) break;
            foreach (var edge in SortedEdges(current.NodeId))
            {
                var neighbor = nodes[edge.ToId];
                if (IsOffline(neighbor)) continue;
                var proposed = current.Distance + edge.Cost;
                if (proposed >= distances.GetValueOrDefault(neighbor.Id, double.PositiveInfinity) - 1e-9)
                    continue;
                distances[neighbor.Id] = proposed;
                previous[neighbor.Id] = current.NodeId;
                pending.Enqueue((neighbor.Id, proposed), proposed);
            }
        }

        if (!distances.TryGetValue(destinationId, out var totalCost))
            return new MeshPathResult(false, Array.Empty<string>(), 0, 0);
        var path = new List<string> { destinationId };
        while (path[0] != startId)
        {
            if (!previous.TryGetValue(path[0], out var prior))
                return new MeshPathResult(false, Array.Empty<string>(), 0, 0);
            path.Insert(0, prior);
        }
        return new MeshPathResult(true, path, Math.Max(0, path.Count - 1), totalCost);
    }

    public IReadOnlyList<string> Traverse(string startId, string traversal)
    {
        if (!nodes.TryGetValue(startId, out var start) || IsOffline(start))
            return Array.Empty<string>();
        var normalized = traversal.Trim().ToLowerInvariant();
        if (normalized is not ("bfs" or "dfs"))
            throw new ArgumentException("Traversal must be 'bfs' or 'dfs'.", nameof(traversal));

        var visited = new HashSet<string>(StringComparer.Ordinal) { startId };
        var order = new List<string>();
        if (normalized == "bfs")
        {
            var pending = new Queue<string>();
            pending.Enqueue(startId);
            while (pending.TryDequeue(out var current))
            {
                order.Add(current);
                foreach (var neighbor in SortedEdges(current).Select(edge => edge.ToId))
                {
                    if (IsOffline(nodes[neighbor]) || !visited.Add(neighbor)) continue;
                    pending.Enqueue(neighbor);
                }
            }
        }
        else
        {
            var pending = new Stack<string>();
            pending.Push(startId);
            while (pending.TryPop(out var current))
            {
                order.Add(current);
                var neighbors = SortedEdges(current).Select(edge => edge.ToId).Reverse();
                foreach (var neighbor in neighbors)
                {
                    if (IsOffline(nodes[neighbor]) || !visited.Add(neighbor)) continue;
                    pending.Push(neighbor);
                }
            }
        }
        return order;
    }

    public MeshMstResult MinimumSpanningTree(string preferredRootId)
    {
        if (!nodes.ContainsKey(preferredRootId) || IsOffline(nodes[preferredRootId]))
            return new MeshMstResult(Array.Empty<MeshLink>(), 0);

        var activeIds = nodes.Values
            .Where(node => !IsOffline(node))
            .Select(node => node.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var roots = new[] { preferredRootId }.Concat(activeIds.Where(id => id != preferredRootId));
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var selected = new List<MeshLink>();
        var candidates = new BinaryMinHeap<(MeshLink Link, string TargetId)>();

        foreach (var rootId in roots)
        {
            if (!visited.Add(rootId)) continue;
            EnqueueFrontier(rootId);
            while (candidates.TryDequeue(out var candidate))
            {
                if (!visited.Add(candidate.TargetId)) continue;
                selected.Add(candidate.Link);
                EnqueueFrontier(candidate.TargetId);
            }
        }

        return new MeshMstResult(selected, selected.Sum(link => link.Cost));

        void EnqueueFrontier(string nodeId)
        {
            foreach (var edge in SortedEdges(nodeId))
            {
                if (IsOffline(nodes[edge.ToId]) || visited.Contains(edge.ToId)) continue;
                candidates.Enqueue((edge, edge.ToId), edge.Cost);
            }
        }
    }

    private IReadOnlyList<MeshLink> SortedEdges(string nodeId) =>
        adjacency[nodeId]
            .OrderBy(edge => edge.Cost)
            .ThenBy(edge => edge.ToId, StringComparer.Ordinal)
            .ToList();

    private static bool IsOffline(MeshNode node) =>
        string.Equals(node.Status, "offline", StringComparison.OrdinalIgnoreCase);

    private static string LinkKey(string fromId, string toId) =>
        string.CompareOrdinal(fromId, toId) < 0 ? $"{fromId}::{toId}" : $"{toId}::{fromId}";
}
