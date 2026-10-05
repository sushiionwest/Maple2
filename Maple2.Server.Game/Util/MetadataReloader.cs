using Maple2.Database.Context;
using Maple2.Database.Mods;
using Maple2.Database.Storage;

namespace Maple2.Server.Game.Util;

/// <summary>
/// Drops cached metadata so edits to the metadata database (for example from the mods folder) take effect
/// without restarting the channel. Already-loaded fields keep what they built at creation, such as their NPC spawns.
/// </summary>
public class MetadataReloader {
    private readonly Dictionary<string, Action> targets;
    private readonly MetadataContext context;

    public MetadataReloader(MetadataContext context, ItemMetadataStorage item, NpcMetadataStorage npc, MapMetadataStorage map,
                            MapEntityStorage mapEntity, MapDataStorage mapData, QuestMetadataStorage quest, SkillMetadataStorage skill,
                            RideMetadataStorage ride, AchievementMetadataStorage achievement, ScriptMetadataStorage script,
                            AiMetadataStorage ai, FunctionCubeMetadataStorage functionCube, TableMetadataStorage table,
                            ServerTableMetadataStorage serverTable, TriggerScriptMetadata triggerScript, TriggerCache triggerCache) {
        this.context = context;
        targets = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase) {
            ["item"] = item.InvalidateCache,
            ["npc"] = npc.InvalidateCache,
            ["map"] = () => {
                map.InvalidateCache();
                mapEntity.InvalidateCache();
                mapData.InvalidateCache();
            },
            ["quest"] = quest.InvalidateCache,
            ["skill"] = skill.InvalidateCache,
            ["ride"] = ride.InvalidateCache,
            ["achievement"] = achievement.InvalidateCache,
            ["script"] = script.InvalidateCache,
            ["ai"] = ai.InvalidateCache,
            ["cube"] = functionCube.InvalidateCache,
            ["table"] = table.InvalidateCache,
            ["server-table"] = serverTable.InvalidateCache,
            ["trigger"] = () => {
                triggerScript.InvalidateCache();
                triggerCache.Clear();
            },
        };
    }

    public IEnumerable<string> Targets => targets.Keys;

    /// <summary>
    /// Reloads the named targets, or everything when none (or "all") are given.
    /// </summary>
    /// <returns>The targets that were reloaded.</returns>
    public IReadOnlyList<string> Reload(IEnumerable<string> names) {
        List<string> requested = names.Select(name => name.Trim()).Where(name => name.Length > 0).ToList();
        if (requested.Count == 0 || requested.Contains("all", StringComparer.OrdinalIgnoreCase)) {
            requested = targets.Keys.ToList();
        }

        List<string> unknown = requested.Where(name => !targets.ContainsKey(name)).ToList();
        if (unknown.Count > 0) {
            throw new ArgumentException($"Unknown reload target(s): {string.Join(", ", unknown)}. Valid: all, {string.Join(", ", targets.Keys)}");
        }

        foreach (string name in requested) {
            targets[name]();
        }
        return requested;
    }

    public MetadataModApplier.Result ApplyMods(string? folder = null) {
        return new MetadataModApplier(context).Apply(folder ?? MetadataModApplier.DefaultModsFolder());
    }
}
