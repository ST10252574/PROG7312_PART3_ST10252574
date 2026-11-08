namespace SmartX.Gateway.Api.DataStructures;

public sealed class BinarySearchTree<TKey, TValue> where TKey : notnull
{
    private sealed class Node(TKey key, TValue value)
    {
        public TKey Key { get; } = key;
        public List<TValue> Values { get; } = [value];
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }

    private readonly IComparer<TKey> comparer;
    private Node? root;

    public BinarySearchTree(IComparer<TKey>? comparer = null) =>
        this.comparer = comparer ?? Comparer<TKey>.Default;

    public void Add(TKey key, TValue value)
    {
        root = Add(root, key, value);
    }

    public bool TryGetValues(TKey key, out IReadOnlyList<TValue> values)
    {
        var current = root;
        while (current is not null)
        {
            var order = comparer.Compare(key, current.Key);
            if (order == 0)
            {
                values = current.Values;
                return true;
            }
            current = order < 0 ? current.Left : current.Right;
        }
        values = Array.Empty<TValue>();
        return false;
    }

    public IReadOnlyList<KeyValuePair<TKey, IReadOnlyList<TValue>>> InOrder()
    {
        var result = new List<KeyValuePair<TKey, IReadOnlyList<TValue>>>();
        var pending = new Stack<Node>();
        var current = root;
        while (current is not null || pending.Count > 0)
        {
            while (current is not null)
            {
                pending.Push(current);
                current = current.Left;
            }
            current = pending.Pop();
            result.Add(new KeyValuePair<TKey, IReadOnlyList<TValue>>(current.Key, current.Values));
            current = current.Right;
        }
        return result;
    }

    private Node Add(Node? node, TKey key, TValue value)
    {
        if (node is null) return new Node(key, value);
        var order = comparer.Compare(key, node.Key);
        if (order == 0) node.Values.Add(value);
        else if (order < 0) node.Left = Add(node.Left, key, value);
        else node.Right = Add(node.Right, key, value);
        return node;
    }
}
