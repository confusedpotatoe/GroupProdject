using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BrickBreaker.Storage
{
    public sealed class StorageConfiguration
    {
        private readonly IConfiguration _config;
        private readonly string _root;

        public StorageConfiguration()
        {
            _root = LocateSolutionRoot() ?? Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(_root)
                .AddJsonFile(Path.Combine("BrickBreaker.Storage", "Properties", "appsettings.json"),
                optional: false,
                reloadOnChange: true);

            _config = builder.Build();
        }

        public string GetConnectionString()
        {
            var ConnectionString = _config.GetConnectionString("Supabase")
            ?? _config["Supabase"]
            ?? _config["ConnectionString:Supabase"]
            ?? _config["SupabaseConnection"];

            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                return ConnectionString;
            }
            else
            throw new InvalidOperationException ("Missing Supabase connection string. Add `ConnectionString:Supabase` to BrickBreaker.Storage/Properties/appsettings.json.");
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
}
