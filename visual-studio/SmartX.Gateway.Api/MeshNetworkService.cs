using SmartX.Gateway.Api.DataStructures;

namespace SmartX.Gateway.Api;

public sealed class MeshNetworkService(GatewayStore store)
{
    public const string CentralGatewayId = "gateway-central";

    public MeshTopology GetTopology()
    {
        var graph = BuildGraph();
        var minimumSpanningTree = graph.MinimumSpanningTree(CentralGatewayId);
        return new MeshTopology(
            CentralGatewayId,
            graph.Nodes,
            graph.Links,
            minimumSpanningTree.Links,
            minimumSpanningTree.TotalCost);
    }

    public MeshRoute? GetRoute(string identifier)
    {
        var graph = BuildGraph();
        var normalized = identifier.Trim();
        var sensor = graph.Nodes.FirstOrDefault(node =>
            node.Type == "sensor" &&
            (string.Equals(node.Id, normalized, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(node.MacAddress, normalized, StringComparison.OrdinalIgnoreCase)));
        if (sensor is null) return null;

        var path = graph.LowestCostPath(sensor.Id, CentralGatewayId);
        return new MeshRoute(
            sensor.Id,
            CentralGatewayId,
            path.Reachable,
            path.Path,
            path.Hops,
            path.TotalCost);
    }

    public MeshConnectivity? GetConnectivity(string gatewayId, string traversal)
    {
        var graph = BuildGraph();
        if (!graph.ContainsNode(gatewayId)) return null;
        var traversalOrder = graph.Traverse(gatewayId, traversal);
        var reachable = traversalOrder.ToHashSet(StringComparer.Ordinal);
        var allIds = graph.Nodes.Select(node => node.Id).ToList();
        return new MeshConnectivity(
            gatewayId,
            traversal.Trim().ToLowerInvariant(),
            traversalOrder,
            traversalOrder,
            allIds.Where(id => !reachable.Contains(id)).ToList());
    }

    private MeshGraph BuildGraph()
    {
        List<Sensor> sensors;
        lock (store.SyncRoot)
            sensors = store.Sensors.ToList();

        var graph = new MeshGraph();
        graph.AddNode(new MeshNode(CentralGatewayId, "Smart-X Central Gateway", "gateway", null, "online", 50, 52));
        var relays = new[]
        {
            new MeshNode("router-north", "North Relay", "router", null, "online", 50, 22),
            new MeshNode("router-east", "East Relay", "router", null, "online", 80, 56),
            new MeshNode("router-west", "West Relay", "router", null, "online", 20, 56)
        };
        foreach (var relay in relays) graph.AddNode(relay);

        for (var index = 0; index < sensors.Count; index++)
        {
            var sensor = sensors[index];
            var angle = (index / (double)Math.Max(1, sensors.Count)) * Math.PI * 2 - Math.PI / 2;
            graph.AddNode(new MeshNode(
                sensor.Id,
                sensor.Location,
                "sensor",
                sensor.MacAddress,
                sensor.Status.ToString().ToLowerInvariant(),
                50 + 42 * Math.Cos(angle),
                52 + 41 * Math.Sin(angle)));
        }

        graph.AddLink(new MeshLink(CentralGatewayId, "router-north", 1.1, 9));
        graph.AddLink(new MeshLink(CentralGatewayId, "router-east", 1.6, 14));
        graph.AddLink(new MeshLink(CentralGatewayId, "router-west", 1.8, 16));
        graph.AddLink(new MeshLink("router-north", "router-east", 1.2, 11));
        graph.AddLink(new MeshLink("router-east", "router-west", 1.4, 13));
        graph.AddLink(new MeshLink("router-west", "router-north", 1.3, 12));

        for (var index = 0; index < sensors.Count; index++)
        {
            var firstRelay = relays[index % relays.Length].Id;
            var secondRelay = relays[(index + 1) % relays.Length].Id;
            graph.AddLink(new MeshLink(sensors[index].Id, firstRelay, 0.8 + (index % 4) * 0.15, 6 + (index % 4) * 2));
            graph.AddLink(new MeshLink(sensors[index].Id, secondRelay, 1.1 + (index % 3) * 0.2, 8 + (index % 3) * 2));
        }
        return graph;
    }
}
