namespace SmartX.Gateway.Api.DataStructures;

public sealed class BinaryMinHeap<T>
{
    private readonly List<(double Priority, long Sequence, T Value)> items = [];
    private long sequence;

    public int Count => items.Count;

    public void Enqueue(T value, double priority)
    {
        if (!double.IsFinite(priority))
            throw new ArgumentOutOfRangeException(nameof(priority), "Heap priorities must be finite.");
        items.Add((priority, sequence++, value));
        BubbleUp(items.Count - 1);
    }

    public bool TryDequeue(out T? value)
    {
        if (items.Count == 0)
        {
            value = default;
            return false;
        }
        value = items[0].Value;
        var last = items[^1];
        items.RemoveAt(items.Count - 1);
        if (items.Count > 0)
        {
            items[0] = last;
            BubbleDown(0);
        }
        return true;
    }

    private void BubbleUp(int index)
    {
        while (index > 0)
        {
            var parent = (index - 1) / 2;
            if (Compare(items[index], items[parent]) >= 0) break;
            (items[index], items[parent]) = (items[parent], items[index]);
            index = parent;
        }
    }

    private void BubbleDown(int index)
    {
        while (true)
        {
            var left = index * 2 + 1;
            var right = left + 1;
            var smallest = index;
            if (left < items.Count && Compare(items[left], items[smallest]) < 0) smallest = left;
            if (right < items.Count && Compare(items[right], items[smallest]) < 0) smallest = right;
            if (smallest == index) return;
            (items[index], items[smallest]) = (items[smallest], items[index]);
            index = smallest;
        }
    }

    private static int Compare(
        (double Priority, long Sequence, T Value) left,
        (double Priority, long Sequence, T Value) right)
    {
        var priorityOrder = left.Priority.CompareTo(right.Priority);
        return priorityOrder != 0 ? priorityOrder : left.Sequence.CompareTo(right.Sequence);
    }
}
