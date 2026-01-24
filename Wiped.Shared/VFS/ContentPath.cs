namespace Wiped.Shared.VFS;

public readonly struct ContentPath : IEquatable<ContentPath>
{
	// use ordinal for comparisons
	// creation will convert to be case insensitive
	// minor perf saved

	private readonly string _path;

    public static readonly ContentPath Root = new("");

	private readonly string Normalize(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return string.Empty;

		// forward slashes pls
		path = path.Replace('\\', '/');

		Span<char> buffer = stackalloc char[path.Length];
		var len = 0;
		var segmentStart = 0;
		for (var i = 0; i <= path.Length; i++)
		{
			if (i != path.Length && path[i] != '/')
				continue;

			var segment = path.AsSpan(segmentStart, i - segmentStart);
			segmentStart = i + 1;

			if (segment.Length == 0)
				continue;

			if (segment.SequenceEqual("."))
				continue;

			// content may try to be sneaky
			if (segment.SequenceEqual(".."))
				continue;

			if (len > 0)
				buffer[len++] = '/';

			for (var j = 0; j < segment.Length; j++)
				buffer[len++] = char.ToLowerInvariant(segment[j]);
		}

		return new string(buffer[..len]);
	}

	public bool IsDescendentOf(ContentPath parent)
	{
		// root is always a descendent unless we ourselves are also root
		if (parent._path.Length == 0)
			return _path.Length > 0;

		// our path can never be shorter if a descendent as we must also include the parents exact path
		if (_path.Length <= parent._path.Length)
			return false;

		// actual check
		if (!_path.StartsWith(parent._path, StringComparison.Ordinal))
			return false;

		return true;
	}

	public bool IsDirectChildOf(ContentPath parent)
	{
		if (!IsDescendentOf(parent))
			return false;

		return !_path.AsSpan(parent._path.Length + 1).Contains('/');
	}

	public ContentPath(string path)
	{
		_path = Normalize(path);
	}

	public override string ToString() => _path;

    public bool Equals(ContentPath other) => string.Equals(_path, other._path, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is ContentPath cp && Equals(cp);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(_path);
}
