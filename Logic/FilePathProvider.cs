using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace BrickBreaker.Storage;

public sealed class FilePathProvider
{
    private readonly IConfiguration _config;
    private readonly string _root;

    public FilePathProvider()
    {
        _root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."));
        _config = new ConfigurationBuilder()
            .SetBasePath(_root)
            .AddJsonFile("Properties/appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
    public string GetUserPath() => Path.Combine(_root, _config["FilePaths:UserPath"]);
    public string GetLeaderboardPath() => Path.Combine(_root, _config["FilePaths:LeaderboardPath"]);
}