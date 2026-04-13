namespace Wiped.Tools;

// ring buffer
internal sealed class ProgramLauncherHistory
{
	private string[] _buffer;
	private int _index;
    private int _count;

	private int _maxEntries;

	public int Capacity => _maxEntries;
	public int Count => _count;

	public void Add(string entry)
	{
		_buffer[_index] = entry;
		_index = (_index + 1) % _maxEntries;

		// not full yet
		// increment the count
		if (_count < _maxEntries)
			_count++;
	}

	public void Add(IEnumerable<string> entries)
	{
		foreach (var entry in entries)
			Add(entry);
	}

	public IEnumerable<string> GetEntries()
	{
		for (var i = 0; i < _count; i++)
		{
			var baseIndex = _index - _count; // oldest item
			var offset = baseIndex + i;
			var index = (offset + _maxEntries) % _maxEntries;
			yield return _buffer[index];
		}
	}

	// not a hot path so idc
	// only done once on user input (if ever)
	public void Resize(int newSize)
	{
		if (newSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(newSize));

		var newBuffer = new string[newSize];

    	var itemsToCopy = _count < newSize
			? _count
			: newSize;

		// find oldest item to keep
		var baseIndex = _index - itemsToCopy;
		var start = (baseIndex + _maxEntries) % _maxEntries;

		for (var i = 0; i < itemsToCopy; i++)
		{
			var offset = start + i;
			var oldIndex = offset % _maxEntries;
			newBuffer[i] = _buffer[oldIndex];
		}

		_buffer = newBuffer;
		_maxEntries = newSize;
		_count = itemsToCopy;
		_index = itemsToCopy % newSize;
	}

	public void Clear()
	{
		_index = 0;
		_count = 0;
		_buffer = new string[_maxEntries];
	}

	public bool IsValidIndex(int index)
	{
		if (index < 0)
			return false;

		if (index >= _count)
			return false;

		return true;
	}

	public string PeekIndex(int index)
	{
		var offsetFromLatest = _index - 1 - index;
		var actualIndex = (offsetFromLatest + _maxEntries) % _maxEntries;
		return _buffer[actualIndex];
	}

	internal ProgramLauncherHistory(int maxEntries)
	{
		if (maxEntries < 0)
			throw new ArgumentOutOfRangeException();

		_index = 0;
		_count = 0;
		_maxEntries = maxEntries;
		_buffer = new string[maxEntries];
	}
}
