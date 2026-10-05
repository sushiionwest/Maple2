using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Maple2.Database.Context;
using Maple2.Tools;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Mods;

/// <summary>
/// Applies JSON patch files from a mods folder to the metadata database.
///
/// Each *.json file holds one patch object or an array of them:
/// <code>
/// { "table": "item", "key": { "Id": 50400001 }, "set": { "Property": { "SlotMax": 9999 } } }
/// </code>
/// - "table": metadata table name (item, npc, map, quest, skill, table, server-table, ...).
/// - "key": column values that identify the row.
/// - "set": columns to change. An object written to a JSON column is deep-merged (RFC 7396 merge patch,
///   so null removes a key and arrays are replaced whole); anything else replaces the column value.
/// - "clone": optional key of an existing row to copy into "key" before "set" is applied.
/// - "delete": true removes the row instead.
///
/// Files and folders whose name starts with '_' or '.' are skipped. Every table a patch touches has its
/// ingest checksum reset, so the next Maple2.File.Ingest run restores it from client data and re-applies mods.
/// </summary>
public class MetadataModApplier {
    public const string ModsFolderEnv = "MODS_FOLDER";

    private readonly MetadataContext context;

    public MetadataModApplier(MetadataContext context) {
        this.context = context;
    }

    public static string DefaultModsFolder() {
        string? folder = Environment.GetEnvironmentVariable(ModsFolderEnv);
        return string.IsNullOrWhiteSpace(folder) ? Path.Combine(Paths.SOLUTION_DIR, "mods") : Path.GetFullPath(folder);
    }

    public record Result(int Files, int Patches, IReadOnlyCollection<string> Tables, IReadOnlyList<string> Errors) {
        public bool Success => Errors.Count == 0;
    }

    public static IEnumerable<string> FindModFiles(string folder) {
        if (!Directory.Exists(folder)) {
            return [];
        }

        return Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories)
            .Where(file => !Path.GetRelativePath(folder, file)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(part => part.StartsWith('_') || part.StartsWith('.')))
            .OrderBy(file => file, StringComparer.Ordinal);
    }

    /// <summary>
    /// Applies every patch in the folder in one transaction. Nothing is written if any patch fails.
    /// </summary>
    public Result Apply(string folder) {
        var errors = new List<string>();
        var tables = new SortedSet<string>(StringComparer.Ordinal);
        var patches = new List<(string File, ModPatch Patch)>();

        List<string> files = FindModFiles(folder).ToList();
        foreach (string file in files) {
            string name = Path.GetRelativePath(folder, file);
            try {
                patches.AddRange(ModPatch.Parse(System.IO.File.ReadAllText(file)).Select(patch => (name, patch)));
            } catch (Exception ex) when (ex is JsonException or FormatException) {
                errors.Add($"{name}: {ex.Message}");
            }
        }

        if (errors.Count > 0 || patches.Count == 0) {
            return new Result(files.Count, 0, tables, errors);
        }

        lock (context) {
            DbConnection connection = context.Database.GetDbConnection();
            bool opened = connection.State != ConnectionState.Open;
            if (opened) {
                connection.Open();
            }

            try {
                using DbTransaction transaction = connection.BeginTransaction();
                var schema = new SchemaCache(connection, transaction);
                for (int i = 0; i < patches.Count; i++) {
                    (string file, ModPatch patch) = patches[i];
                    try {
                        ApplyPatch(connection, transaction, schema, patch);
                        tables.Add(patch.Table);
                    } catch (Exception ex) when (ex is FormatException or DbException) {
                        errors.Add($"{file} (patch {i + 1}, table '{patch.Table}'): {ex.Message}");
                    }
                }

                if (errors.Count > 0) {
                    transaction.Rollback();
                    return new Result(files.Count, 0, new SortedSet<string>(), errors);
                }

                foreach (string table in tables) {
                    Execute(connection, transaction, "UPDATE `checksum` SET `Crc32C` = 0 WHERE `TableName` = @p0", table);
                }
                transaction.Commit();
            } finally {
                if (opened) {
                    connection.Close();
                }
            }
        }

        return new Result(files.Count, patches.Count, tables, errors);
    }

    /// <summary>
    /// Returns the current row as a ready-to-edit patch, so modders can see the exact column and field names.
    /// </summary>
    public string Dump(string table, IReadOnlyDictionary<string, JsonNode?> key) {
        lock (context) {
            DbConnection connection = context.Database.GetDbConnection();
            bool opened = connection.State != ConnectionState.Open;
            if (opened) {
                connection.Open();
            }

            try {
                var schema = new SchemaCache(connection, null);
                TableSchema tableSchema = schema.Get(table);
                using DbCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM {Quote(table)} WHERE {WhereClause(command, tableSchema, key, "k")} LIMIT 1";
                using DbDataReader reader = command.ExecuteReader();
                if (!reader.Read()) {
                    throw new FormatException($"No row in '{table}' matches {FormatKey(key)}");
                }

                var set = new JsonObject();
                var keyObject = new JsonObject();
                for (int i = 0; i < reader.FieldCount; i++) {
                    string column = reader.GetName(i);
                    if (reader.IsDBNull(i)) {
                        continue;
                    }

                    JsonNode? value = tableSchema.JsonColumns.Contains(column)
                        ? JsonNode.Parse(reader.GetString(i))
                        : JsonSerializer.SerializeToNode(reader.GetValue(i));
                    if (key.Keys.Contains(column, StringComparer.OrdinalIgnoreCase)) {
                        keyObject[column] = value;
                    } else {
                        set[column] = value;
                    }
                }

                var patch = new JsonObject {
                    ["table"] = table,
                    ["key"] = keyObject,
                    ["set"] = set,
                };
                return patch.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            } finally {
                if (opened) {
                    connection.Close();
                }
            }
        }
    }

    private static void ApplyPatch(DbConnection connection, DbTransaction transaction, SchemaCache schema, ModPatch patch) {
        TableSchema table = schema.Get(patch.Table);
        foreach (string column in patch.Key.Keys.Concat(patch.Set.Keys).Concat(patch.CloneFrom?.Keys ?? [])) {
            table.RequireColumn(column);
        }

        if (patch.Delete) {
            int deleted = ExecuteWhere(connection, transaction, $"DELETE FROM {Quote(patch.Table)} WHERE ", table, patch.Key);
            if (deleted == 0) {
                throw new FormatException($"No row matches {FormatKey(patch.Key)}");
            }
            return;
        }

        if (patch.CloneFrom != null) {
            ExecuteWhere(connection, transaction, $"DELETE FROM {Quote(patch.Table)} WHERE ", table, patch.Key);

            using DbCommand clone = connection.CreateCommand();
            clone.Transaction = transaction;
            var select = new List<string>();
            foreach (string column in table.Columns) {
                string? keyColumn = patch.Key.Keys.FirstOrDefault(k => k.Equals(column, StringComparison.OrdinalIgnoreCase));
                select.Add(keyColumn == null ? Quote(column) : AddParameter(clone, $"n{select.Count}", patch.Key[keyColumn]));
            }
            clone.CommandText = $"INSERT INTO {Quote(patch.Table)} ({string.Join(", ", table.Columns.Select(Quote))}) " +
                                $"SELECT {string.Join(", ", select)} FROM {Quote(patch.Table)} WHERE {WhereClause(clone, table, patch.CloneFrom, "c")}";
            if (clone.ExecuteNonQuery() == 0) {
                throw new FormatException($"Clone source not found: {FormatKey(patch.CloneFrom)}");
            }
        }

        if (patch.Set.Count == 0) {
            return;
        }

        using DbCommand update = connection.CreateCommand();
        update.Transaction = transaction;
        var assignments = new List<string>();
        foreach ((string column, JsonNode? value) in patch.Set) {
            string name = $"s{assignments.Count}";
            if (value == null) {
                assignments.Add($"{Quote(column)} = NULL");
            } else if (table.JsonColumns.Contains(column)) {
                string json = AddParameter(update, name, value.ToJsonString());
                assignments.Add(value is JsonObject
                    ? $"{Quote(column)} = JSON_MERGE_PATCH(COALESCE({Quote(column)}, JSON_OBJECT()), CAST({json} AS JSON))"
                    : $"{Quote(column)} = CAST({json} AS JSON)");
            } else {
                assignments.Add($"{Quote(column)} = {AddParameter(update, name, value)}");
            }
        }

        update.CommandText = $"UPDATE {Quote(patch.Table)} SET {string.Join(", ", assignments)} WHERE {WhereClause(update, table, patch.Key, "k")}";
        if (update.ExecuteNonQuery() == 0 && !RowExists(connection, transaction, table, patch.Key)) {
            throw new FormatException($"No row matches {FormatKey(patch.Key)}. Use \"clone\" to create a new row.");
        }
    }

    private static bool RowExists(DbConnection connection, DbTransaction transaction, TableSchema table, IReadOnlyDictionary<string, JsonNode?> key) {
        using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT COUNT(*) FROM {Quote(table.Name)} WHERE {WhereClause(command, table, key, "e")}";
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static int ExecuteWhere(DbConnection connection, DbTransaction transaction, string sql, TableSchema table, IReadOnlyDictionary<string, JsonNode?> key) {
        using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql + WhereClause(command, table, key, "w");
        return command.ExecuteNonQuery();
    }

    private static void Execute(DbConnection connection, DbTransaction transaction, string sql, params object[] args) {
        using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        for (int i = 0; i < args.Length; i++) {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = $"@p{i}";
            parameter.Value = args[i];
            command.Parameters.Add(parameter);
        }
        command.ExecuteNonQuery();
    }

    private static string WhereClause(DbCommand command, TableSchema table, IReadOnlyDictionary<string, JsonNode?> key, string prefix) {
        if (key.Count == 0) {
            throw new FormatException("\"key\" must name at least one column");
        }

        var conditions = new List<string>();
        foreach ((string column, JsonNode? value) in key) {
            table.RequireColumn(column);
            conditions.Add($"{Quote(column)} = {AddParameter(command, $"{prefix}{conditions.Count}", value)}");
        }
        return string.Join(" AND ", conditions);
    }

    private static string AddParameter(DbCommand command, string name, JsonNode? value) {
        object? raw = value switch {
            null => null,
            JsonValue v when v.TryGetValue(out long l) => l,
            JsonValue v when v.TryGetValue(out double d) => d,
            JsonValue v when v.TryGetValue(out bool b) => b,
            JsonValue v when v.TryGetValue(out string? s) => s,
            _ => value.ToJsonString(),
        };
        return AddParameter(command, name, raw);
    }

    private static string AddParameter(DbCommand command, string name, object? value) {
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = $"@{name}";
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
        return parameter.ParameterName;
    }

    private static string Quote(string identifier) => $"`{identifier.Replace("`", "``")}`";

    private static string FormatKey(IReadOnlyDictionary<string, JsonNode?> key) =>
        string.Join(", ", key.Select(entry => $"{entry.Key}={entry.Value?.ToJsonString() ?? "null"}"));

    private record TableSchema(string Name, IReadOnlyList<string> Columns, ISet<string> JsonColumns) {
        public void RequireColumn(string column) {
            if (!Columns.Contains(column, StringComparer.OrdinalIgnoreCase)) {
                throw new FormatException($"Unknown column '{column}' in table '{Name}'. Columns: {string.Join(", ", Columns)}");
            }
        }
    }

    private class SchemaCache(DbConnection connection, DbTransaction? transaction) {
        private readonly Dictionary<string, TableSchema> tables = new(StringComparer.Ordinal);

        public TableSchema Get(string table) {
            if (tables.TryGetValue(table, out TableSchema? schema)) {
                return schema;
            }

            var columns = new List<string>();
            var jsonColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (DbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM information_schema.COLUMNS " +
                                      "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t ORDER BY ORDINAL_POSITION";
                AddParameter(command, "t", (object) table);
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    string column = reader.GetString(0);
                    columns.Add(column);
                    if (reader.GetString(1).Equals("json", StringComparison.OrdinalIgnoreCase)) {
                        jsonColumns.Add(column);
                    }
                }
            }

            if (columns.Count == 0) {
                throw new FormatException($"Unknown table '{table}'");
            }

            schema = new TableSchema(table, columns, jsonColumns);
            tables[table] = schema;
            return schema;
        }
    }
}

public record ModPatch(
    string Table,
    IReadOnlyDictionary<string, JsonNode?> Key,
    IReadOnlyDictionary<string, JsonNode?> Set,
    IReadOnlyDictionary<string, JsonNode?>? CloneFrom,
    bool Delete) {

    public static List<ModPatch> Parse(string json) {
        JsonNode? root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        });

        return root switch {
            JsonArray array => array.Select((node, i) => FromNode(node, $"patch {i + 1}")).ToList(),
            JsonObject obj => [FromNode(obj, "patch")],
            _ => throw new FormatException("A mod file must contain a patch object or an array of patches"),
        };
    }

    private static ModPatch FromNode(JsonNode? node, string label) {
        if (node is not JsonObject obj) {
            throw new FormatException($"{label} must be an object");
        }

        foreach (string property in obj.Select(entry => entry.Key)) {
            if (property is not ("table" or "key" or "set" or "clone" or "delete" or "comment")) {
                throw new FormatException($"{label}: unknown property \"{property}\" (expected table, key, set, clone, delete, comment)");
            }
        }

        string table = obj["table"]?.GetValue<string>() ?? throw new FormatException($"{label} is missing \"table\"");
        IReadOnlyDictionary<string, JsonNode?> key = ToMap(obj["key"], $"{label}.key") ?? throw new FormatException($"{label} is missing \"key\"");
        IReadOnlyDictionary<string, JsonNode?>? clone = ToMap(obj["clone"], $"{label}.clone");
        IReadOnlyDictionary<string, JsonNode?> set = ToMap(obj["set"], $"{label}.set") ?? new Dictionary<string, JsonNode?>();
        bool delete = obj["delete"]?.GetValue<bool>() ?? false;

        if (delete && (set.Count > 0 || clone != null)) {
            throw new FormatException($"{label}: \"delete\" cannot be combined with \"set\" or \"clone\"");
        }
        if (!delete && clone == null && set.Count == 0) {
            throw new FormatException($"{label} does nothing: add \"set\", \"clone\" or \"delete\"");
        }

        return new ModPatch(table, key, set, clone, delete);
    }

    private static IReadOnlyDictionary<string, JsonNode?>? ToMap(JsonNode? node, string label) {
        return node switch {
            null => null,
            JsonObject obj => obj.ToDictionary(entry => entry.Key, entry => entry.Value?.DeepClone()),
            _ => throw new FormatException($"{label} must be an object"),
        };
    }
}

public static class ModResultExtensions {
    public static string Describe(this MetadataModApplier.Result result) {
        var builder = new StringBuilder();
        if (result.Success) {
            builder.Append($"Applied {result.Patches} patch(es) from {result.Files} file(s)");
            if (result.Tables.Count > 0) {
                builder.Append($" to: {string.Join(", ", result.Tables)}");
            }
        } else {
            builder.AppendLine($"Mods not applied ({result.Errors.Count} error(s)):");
            foreach (string error in result.Errors) {
                builder.AppendLine($"  {error}");
            }
        }
        return builder.ToString().TrimEnd();
    }
}
