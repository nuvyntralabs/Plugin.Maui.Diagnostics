namespace Plugin.Maui.Diagnostics;

sealed class TimelineStore
{
    readonly int _capacity;
    readonly List<TimelineEntry> _entries = [];

    public TimelineStore(int capacity) =>
        _capacity = Math.Max(1, capacity);

    public int Count => _entries.Count;

    public void Add(TimelineEntry entry)
    {
        _entries.Add(entry);
        if (_entries.Count > _capacity)
        {
            _entries.RemoveRange(0, _entries.Count - _capacity);
        }
    }

    public IReadOnlyList<TimelineEntry> Snapshot() => _entries.ToArray();

    public void Replace(IEnumerable<TimelineEntry> entries)
    {
        _entries.Clear();
        foreach (var entry in entries)
        {
            Add(entry);
        }
    }

    public void Clear() => _entries.Clear();
}
