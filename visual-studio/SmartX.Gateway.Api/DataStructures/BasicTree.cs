namespace SmartX.Gateway.Api.DataStructures;

public sealed class GeneralTreeNode<T>(T value)
{
    public T Value { get; } = value;
    public List<GeneralTreeNode<T>> Children { get; } = [];

    public GeneralTreeNode<T> AddChild(T childValue)
    {
        var child = new GeneralTreeNode<T>(childValue);
        Children.Add(child);
        return child;
    }
}

public sealed class GeneralTree<T>(T rootValue)
{
    public GeneralTreeNode<T> Root { get; } = new(rootValue);

    public IReadOnlyList<T> DepthFirst()
    {
        var result = new List<T>();
        var pending = new Stack<GeneralTreeNode<T>>();
        pending.Push(Root);
        while (pending.TryPop(out var node))
        {
            result.Add(node.Value);
            for (var index = node.Children.Count - 1; index >= 0; index--)
                pending.Push(node.Children[index]);
        }
        return result;
    }
}
