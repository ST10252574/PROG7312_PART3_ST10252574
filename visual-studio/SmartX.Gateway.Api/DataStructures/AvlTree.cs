namespace SmartX.Gateway.Api.DataStructures;

public sealed class AvlTree<TKey, TValue> where TKey : notnull
{
    private sealed class Node(TKey key, TValue value)
    {
        public TKey Key { get; } = key;
        public List<TValue> Values { get; } = [value];
        public int Height { get; set; } = 1;
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }

    private readonly IComparer<TKey> comparer;
    private Node? root;

    public AvlTree(IComparer<TKey>? comparer = null) =>
        this.comparer = comparer ?? Comparer<TKey>.Default;

    public int Count { get; private set; }

    public void Add(TKey key, TValue value)
    {
        root = Insert(root, key, value);
        Count++;
    }

    public IReadOnlyList<TValue> Range(TKey minimum, TKey maximum)
    {
        if (comparer.Compare(minimum, maximum) > 0)
            throw new ArgumentException("The start of the range must not be later than its end.");

        var results = new List<TValue>();
        CollectRange(root, minimum, maximum, results);
        return results;
    }

    private Node Insert(Node? node, TKey key, TValue value)
    {
        if (node is null) return new Node(key, value);
        var order = comparer.Compare(key, node.Key);
        if (order == 0)
        {
            node.Values.Add(value);
            return node;
        }
        if (order < 0) node.Left = Insert(node.Left, key, value);
        else node.Right = Insert(node.Right, key, value);

        UpdateHeight(node);
        return Rebalance(node);
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

    private static int Height(Node? node) => node?.Height ?? 0;

    private static void UpdateHeight(Node node) =>
        node.Height = 1 + Math.Max(Height(node.Left), Height(node.Right));

    private static int Balance(Node node) => Height(node.Left) - Height(node.Right);

    private static Node RotateRight(Node root)
    {
        var pivot = root.Left ?? throw new InvalidOperationException("A right rotation requires a left child.");
        root.Left = pivot.Right;
        pivot.Right = root;
        UpdateHeight(root);
        UpdateHeight(pivot);
        return pivot;
    }

    private static Node RotateLeft(Node root)
    {
        var pivot = root.Right ?? throw new InvalidOperationException("A left rotation requires a right child.");
        root.Right = pivot.Left;
        pivot.Left = root;
        UpdateHeight(root);
        UpdateHeight(pivot);
        return pivot;
    }

    private static Node Rebalance(Node node)
    {
        var balance = Balance(node);
        if (balance > 1)
        {
            if (node.Left is not null && Balance(node.Left) < 0)
                node.Left = RotateLeft(node.Left);
            return RotateRight(node);
        }
        if (balance < -1)
        {
            if (node.Right is not null && Balance(node.Right) > 0)
                node.Right = RotateRight(node.Right);
            return RotateLeft(node);
        }
        return node;
    }
}
