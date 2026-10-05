using System.Text.Json.Nodes;
using Maple2.Database.Context;
using Maple2.Database.Mods;
using Maple2.Tools.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Maple2.File.Ingest.Helpers;

/// <summary>
/// Command line entry points for the mods folder: apply patches, or dump a row as a patch to start from.
/// </summary>
public static class ModTool {
    public static MetadataContext Connect() {
        string? server = Environment.GetEnvironmentVariable("DB_IP");
        string? port = Environment.GetEnvironmentVariable("DB_PORT");
        string? database = Environment.GetEnvironmentVariable("DATA_DB_NAME");
        string? user = Environment.GetEnvironmentVariable("DB_USER");
        string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (server == null || port == null || database == null || user == null || password == null) {
            throw new ArgumentException("Database connection information was not set");
        }

        string dataDbConnection = $"Server={server};Port={port};Database={database};User={user};Password={password};oldguids=true";
        DbContextOptions options = new DbContextOptionsBuilder()
            .UseMySql(dataDbConnection, ServerVersion.AutoDetect(dataDbConnection)).Options;
        return new MetadataContext(options);
    }

    public static int Apply(MetadataContext context) {
        string folder = MetadataModApplier.DefaultModsFolder();
        if (!Directory.Exists(folder)) {
            Console.WriteLine($"No mods folder at {folder}, skipping mods.");
            return 0;
        }

        Console.WriteLine($"Applying mods from {folder}...");
        MetadataModApplier.Result result = new MetadataModApplier(context).Apply(folder);
        Console.WriteLine(result.Success ? result.Describe().ColorGreen() : result.Describe().ColorRed());
        return result.Success ? 0 : 1;
    }

    public static int Dump(MetadataContext context, string[] args) {
        if (args.Length < 2) {
            Console.Error.WriteLine("Usage: --dump <table> <column>=<value> [<column>=<value>...]   e.g. --dump item Id=50400001");
            return 1;
        }

        var key = new Dictionary<string, JsonNode?>();
        foreach (string pair in args[1..]) {
            int split = pair.IndexOf('=');
            if (split <= 0) {
                Console.Error.WriteLine($"Expected <column>=<value>, got '{pair}'");
                return 1;
            }

            string value = pair[(split + 1)..];
            key[pair[..split]] = long.TryParse(value, out long number) ? JsonValue.Create(number) : JsonValue.Create(value);
        }

        try {
            Console.WriteLine(new MetadataModApplier(context).Dump(args[0], key));
            return 0;
        } catch (FormatException ex) {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
