namespace Wiped.Shared.VFS;

internal static class OSPaths
{
	public static string GetConfigDir(string appName)
	{
		if (OperatingSystem.IsLinux())
		{
			if (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") is not { } xdgDataHome)
			{
				xdgDataHome = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
					".config"
				);
			}

			return Path.Combine(xdgDataHome, appName);
		}
		else
		{
			throw new PlatformNotSupportedException();
		}
	}

	public static string GetCacheDir(string appName)
	{
		if (OperatingSystem.IsLinux())
		{
			if (Environment.GetEnvironmentVariable("XDG_CACHE_HOME") is not { } xdgDataHome)
			{
				xdgDataHome = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
					".cache"
				);
			}

			return Path.Combine(xdgDataHome, appName);
		}
		else
		{
			throw new PlatformNotSupportedException();
		}
	}

	public static string GetDataDir(string appName)
	{
		if (OperatingSystem.IsLinux())
		{
			if (Environment.GetEnvironmentVariable("XDG_DATA_HOME") is not { } xdgDataHome)
			{
				xdgDataHome = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
					".local", "share"
				);
			}

			return Path.Combine(xdgDataHome, appName);
		}
		else
		{
			throw new PlatformNotSupportedException();
		}
	}
}
