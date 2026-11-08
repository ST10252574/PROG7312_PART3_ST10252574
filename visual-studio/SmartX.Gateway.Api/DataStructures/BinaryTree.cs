namespace SmartX.Gateway.Api.DataStructures;

public sealed class BinaryTreeNode<T>(T value)
{
    public T Value { get; } = value;
    public BinaryTreeNode<T>? Left { get; set; }
    public BinaryTreeNode<T>? Right { get; set; }
}

public sealed class BinaryTree<T>
{
    public BinaryTreeNode<T>? Root { get; private set; }

    public void InsertLevelOrder(T value)
    {
        if (Root is null)
        {
            Root = new BinaryTreeNode<T>(value);
            return;
        }

        var pending = new Queue<BinaryTreeNode<T>>();
        pending.Enqueue(Root);
        while (pending.TryDequeue(out var node))
        {
            if (node.Left is null)
            {
                node.Left = new BinaryTreeNode<T>(value);
                return;
            }
            pending.Enqueue(node.Left);
            if (node.Right is null)
            {
                node.Right = new BinaryTreeNode<T>(value);
                return;
            }
            pending.Enqueue(node.Right);
        }
    }

    public IReadOnlyList<T> InOrder()
    {
        var result = new List<T>();
        var pending = new Stack<BinaryTreeNode<T>>();
        var current = Root;
        while (current is not null || pending.Count > 0)
        {
            while (current is not null)
            {
                pending.Push(current);
                current = current.Left;
            }
            current = pending.Pop();
            result.Add(current.Value);
            current = current.Right;
        }
        return result;
    }
}
