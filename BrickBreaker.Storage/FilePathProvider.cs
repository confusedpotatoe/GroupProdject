using Microsoft.Extensions.Configuration;

namespace BrickBreaker.Storage;

public sealed class FilePathProvider
{
    private readonly IConfiguration _config;
    private readonly string _root;

    public FilePathProvider()
    {
        _root = LocateSolutionRoot() ?? Directory.GetCurrentDirectory();
        _config = new ConfigurationBuilder()
            .SetBasePath(_root)
            .AddJsonFile(Path.Combine("BrickBreaker.Storage", "Properties", "appsettings.json"), optional: false, reloadOnChange: true)
            .Build();
    }

    public string GetUserPath() => ResolvePath(_config["FilePaths:UserPath"]);
    public string GetLeaderboardPath() => ResolvePath(_config["FilePaths:LeaderboardPath"]);

    private string ResolvePath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new InvalidOperationException("File path configuration is missing.");

        return Path.GetFullPath(Path.Combine(_root, relativePath));
    }

    private static string? LocateSolutionRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(dir))
        {
            if (File.Exists(Path.Combine(dir, "BrickBreaker.sln")))
            {
                return dir;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        return null;
    }
}
