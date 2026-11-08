namespace SmartX.Gateway.Api.DataStructures;

public sealed class RedBlackTree<TKey, TValue> where TKey : notnull
{
    private enum NodeColor { Red, Black }

    private sealed class Node(TKey key, TValue value)
    {
        public TKey Key { get; } = key;
        public List<TValue> Values { get; } = [value];
        public NodeColor Color { get; set; } = NodeColor.Red;
        public Node? Parent { get; set; }
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }

    private readonly IComparer<TKey> comparer;
    private Node? root;

    public RedBlackTree(IComparer<TKey>? comparer = null) =>
        this.comparer = comparer ?? Comparer<TKey>.Default;

    public void Add(TKey key, TValue value)
    {
        Node? parent = null;
        var current = root;
        while (current is not null)
        {
            parent = current;
            var order = comparer.Compare(key, current.Key);
            if (order == 0)
            {
                current.Values.Add(value);
                return;
            }
            current = order < 0 ? current.Left : current.Right;
        }

        var inserted = new Node(key, value) { Parent = parent };
        if (parent is null) root = inserted;
        else if (comparer.Compare(key, parent.Key) < 0) parent.Left = inserted;
        else parent.Right = inserted;
        RestoreRedBlackProperties(inserted);
    }

    public bool TryGetValues(TKey key, out IReadOnlyList<TValue> values)
    {
        var current = FindNode(key);
        if (current is not null)
        {
            values = current.Values;
            return true;
        }
        values = Array.Empty<TValue>();
        return false;
    }

    public IReadOnlyList<TValue> Range(TKey minimum, TKey maximum)
    {
        if (comparer.Compare(minimum, maximum) > 0)
            throw new ArgumentException("The start of the range must not be later than its end.");
        var results = new List<TValue>();
        CollectRange(root, minimum, maximum, results);
        return results;
    }

    private Node? FindNode(TKey key)
    {
        var current = root;
        while (current is not null)
        {
            var order = comparer.Compare(key, current.Key);
            if (order == 0) return current;
            current = order < 0 ? current.Left : current.Right;
        }
        return null;
    }

    private void CollectRange(Node? node, TKey minimum, TKey maximum, List<TValue> results)
    {
        if (node is null) return;
        var lower = comparer.Compare(node.Key, minimum);
        var upper = comparer.Compare(node.Key, maximum);
        if (lower > 0) CollectRange(node.Left, minimum, maximum, results);
        if (lower >= 0 && upper <= 0) results.AddRange(node.Values);
        if (upper < 0) CollectRange(node.Right, minimum, maximum, results);
    }

    private void RestoreRedBlackProperties(Node node)
    {
        var current = node;
        while (!ReferenceEquals(current, root) && current.Parent?.Color == NodeColor.Red)
        {
            var parent = current.Parent;
            var grandparent = parent?.Parent;
            if (parent is null || grandparent is null) break;
            if (ReferenceEquals(parent, grandparent.Left))
            {
                var uncle = grandparent.Right;
                if (uncle?.Color == NodeColor.Red)
                {
                    parent.Color = NodeColor.Black;
                    uncle.Color = NodeColor.Black;
                    grandparent.Color = NodeColor.Red;
                    current = grandparent;
                }
                else
                {
                    if (ReferenceEquals(current, parent.Right))
                    {
                        current = parent;
                        RotateLeft(current);
                        parent = current.Parent;
                        grandparent = parent?.Parent;
                    }
                    if (parent is not null && grandparent is not null)
                    {
                        parent.Color = NodeColor.Black;
                        grandparent.Color = NodeColor.Red;
                        RotateRight(grandparent);
                    }
                }
            }
            else
            {
                var uncle = grandparent.Left;
                if (uncle?.Color == NodeColor.Red)
                {
                    parent.Color = NodeColor.Black;
                    uncle.Color = NodeColor.Black;
                    grandparent.Color = NodeColor.Red;
                    current = grandparent;
                }
                else
                {
                    if (ReferenceEquals(current, parent.Left))
                    {
                        current = parent;
                        RotateRight(current);
                        parent = current.Parent;
                        grandparent = parent?.Parent;
                    }
                    if (parent is not null && grandparent is not null)
                    {
                        parent.Color = NodeColor.Black;
                        grandparent.Color = NodeColor.Red;
                        RotateLeft(grandparent);
                    }
                }
            }
        }
        if (root is not null) root.Color = NodeColor.Black;
    }

    private void RotateLeft(Node pivot)
    {
        var child = pivot.Right ?? throw new InvalidOperationException("Left rotation requires a right child.");
        pivot.Right = child.Left;
        if (child.Left is not null) child.Left.Parent = pivot;
        child.Parent = pivot.Parent;
        if (pivot.Parent is null) root = child;
        else if (ReferenceEquals(pivot, pivot.Parent.Left)) pivot.Parent.Left = child;
        else pivot.Parent.Right = child;
        child.Left = pivot;
        pivot.Parent = child;
    }

    private void RotateRight(Node pivot)
    {
        var child = pivot.Left ?? throw new InvalidOperationException("Right rotation requires a left child.");
        pivot.Left = child.Right;
        if (child.Right is not null) child.Right.Parent = pivot;
        child.Parent = pivot.Parent;
        if (pivot.Parent is null) root = child;
        else if (ReferenceEquals(pivot, pivot.Parent.Right)) pivot.Parent.Right = child;
        else pivot.Parent.Left = child;
        child.Right = pivot;
        pivot.Parent = child;
    }
}
