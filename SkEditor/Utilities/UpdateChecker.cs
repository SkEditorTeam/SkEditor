using Octokit;
using SkEditor.API;
using System.Reflection;

namespace SkEditor.Utilities;
public class UpdateChecker
{
	private static readonly int _major = Assembly.GetExecutingAssembly().GetName().Version.Major;
	private static readonly int _minor = Assembly.GetExecutingAssembly().GetName().Version.Minor;
	private static readonly int _build = Assembly.GetExecutingAssembly().GetName().Version.Build;

	private const long RepoId = 679628726;
	private static readonly GitHubClient _gitHubClient = new(new ProductHeaderValue("SkEditor"));

	public static async void Check()
	{
		try
		{
			Release release = await _gitHubClient.Repository.Release.GetLatest(RepoId);
			(int, int, int) version = GetVersion(release.TagName);
			if (IsNewerVersion(version))
			{
				ApiVault.Get().ShowMessage("hihihiha", "nowa wersja");
			}
			else
			{
				ApiVault.Get().ShowMessage("hihihiha", "nie ma nowej wersji");
			}
		}
		catch
		{
			ApiVault.Get().Log("catch");
		}
	}

	private static (int, int, int) GetVersion(string tagName)
	{
		tagName = tagName.TrimStart('v');
		string[] versionParts = tagName.Split('.');
		return (int.Parse(versionParts[0]), int.Parse(versionParts[1]), int.Parse(versionParts[2]));
	}

	private static bool IsNewerVersion((int, int, int) version)
	{
		if (version.Item1 > _major)
		{
			return true;
		}
		else if (version.Item1 == _major)
		{
			if (version.Item2 > _minor)
			{
				return true;
			}
			else if (version.Item2 == _minor)
			{
				if (version.Item3 > _build)
				{
					return true;
				}
			}
		}
		return false;
	}
}
