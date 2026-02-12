using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public static class AppVersion
{
    public static string Current =>
        System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version
            ?.ToString(3) // Gets "1.0.0" (3 parts)
        ?? "0.0.0";

    private const string GithubApiUrl = "https://api.github.com/repos/StarcraftPlayer/Roblox-Settings-Editor/releases/latest";

    public static async Task<(bool hasUpdate, string latestVersion, string downloadUrl)> CanUpdate()
    {
        try
        {
            using var client = new HttpClient();

            // GitHub API requires a user agent
            client.DefaultRequestHeaders.Add("User-Agent", "Roblox-Settings-Editor");

            var response = await client.GetStringAsync(GithubApiUrl);
            var json = JsonDocument.Parse(response);

            // Get latest version tag (e.g. "v1.0.1")
            var latestTag = json.RootElement
                .GetProperty("tag_name")
                .GetString()
                ?.TrimStart('v') ?? "0.0.0";

            // Get download URL
            var downloadUrl = json.RootElement
                .GetProperty("html_url")
                .GetString() ?? "";

            return (latestTag != Current, latestTag, downloadUrl);
        }
        catch
        {
            return (false, Current, "");
        }
    }
}