using System.Text.Json;

namespace InertiaSharp.Cli.Wizard;

public static class ProjectService
{
    public const string ProjectInertiaSharpCli = "inertiasharp.cli";
    
    public static async Task<string>GetLatestVersionAsync(string package = "inertiasharp")
    {
        var url = $"https://api.nuget.org/v3-flatcontainer/{package.ToLower()}/index.json";

        var json = await new HttpClient().GetStringAsync(url);

        using var doc = JsonDocument.Parse(json);
        var versions = doc.RootElement.GetProperty("versions");

        return versions[versions.GetArrayLength() - 1].GetString() ?? throw new Exception("No version available");
    }
}