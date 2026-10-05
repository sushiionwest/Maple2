using System;
using System.IO;
using System.Linq;
using Maple2.Database.Mods;
using Maple2.Tools;

namespace Maple2.Server.Tests.Database;

[TestFixture]
public class ModPatchTests {
    [Test]
    public void Parse_SinglePatch() {
        ModPatch patch = ModPatch.Parse("""{ "table": "item", "key": { "Id": 50400001 }, "set": { "Property": { "SlotMax": 9999 } } }""").Single();

        Assert.That(patch.Table, Is.EqualTo("item"));
        Assert.That(patch.Key["Id"]!.GetValue<int>(), Is.EqualTo(50400001));
        Assert.That(patch.Set["Property"]!["SlotMax"]!.GetValue<int>(), Is.EqualTo(9999));
        Assert.That(patch.CloneFrom, Is.Null);
        Assert.That(patch.Delete, Is.False);
    }

    [Test]
    public void Parse_ArrayWithCommentsAndTrailingCommas() {
        var patches = ModPatch.Parse("""
            // Custom items
            [
              { "table": "item", "key": { "Id": 2 }, "clone": { "Id": 1 }, "comment": "copy of 1" },
              { "table": "npc", "key": { "Id": 3 }, "delete": true },
            ]
            """);

        Assert.That(patches, Has.Count.EqualTo(2));
        Assert.That(patches[0].CloneFrom!["Id"]!.GetValue<int>(), Is.EqualTo(1));
        Assert.That(patches[1].Delete, Is.True);
    }

    [TestCase("""{ "key": { "Id": 1 }, "set": { "Name": "x" } }""", "missing \"table\"")]
    [TestCase("""{ "table": "item", "set": { "Name": "x" } }""", "missing \"key\"")]
    [TestCase("""{ "table": "item", "key": { "Id": 1 } }""", "does nothing")]
    [TestCase("""{ "table": "item", "key": { "Id": 1 }, "delete": true, "set": { "Name": "x" } }""", "cannot be combined")]
    [TestCase("""{ "table": "item", "key": { "Id": 1 }, "sett": { "Name": "x" } }""", "unknown property \"sett\"")]
    [TestCase("""{ "table": "item", "key": 1, "set": { "Name": "x" } }""", "must be an object")]
    [TestCase("""42""", "patch object or an array")]
    public void Parse_RejectsInvalidPatches(string json, string message) {
        var ex = Assert.Throws<FormatException>(() => ModPatch.Parse(json));
        Assert.That(ex!.Message, Does.Contain(message));
    }

    [Test]
    public void FindModFiles_SkipsUnderscoreAndDotPaths() {
        string root = Path.Combine(Path.GetTempPath(), "ms2-mods-" + Guid.NewGuid());
        try {
            Directory.CreateDirectory(Path.Combine(root, "items"));
            Directory.CreateDirectory(Path.Combine(root, "_examples"));
            Directory.CreateDirectory(Path.Combine(root, ".git"));
            File.WriteAllText(Path.Combine(root, "b.json"), "{}");
            File.WriteAllText(Path.Combine(root, "items", "a.json"), "{}");
            File.WriteAllText(Path.Combine(root, "_off.json"), "{}");
            File.WriteAllText(Path.Combine(root, "_examples", "x.json"), "{}");
            File.WriteAllText(Path.Combine(root, ".git", "y.json"), "{}");
            File.WriteAllText(Path.Combine(root, "notes.txt"), "");

            string[] files = MetadataModApplier.FindModFiles(root).Select(file => Path.GetRelativePath(root, file).Replace('\\', '/')).ToArray();

            Assert.That(files, Is.EqualTo(new[] { "b.json", "items/a.json" }));
        } finally {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public void Examples_AreValidPatches() {
        string[] examples = Directory.GetFiles(Path.Combine(Paths.SOLUTION_DIR, "mods", "_examples"), "*.json");

        Assert.That(examples, Is.Not.Empty);
        foreach (string example in examples) {
            Assert.DoesNotThrow(() => ModPatch.Parse(File.ReadAllText(example)), example);
        }
    }

    [Test]
    public void FindModFiles_MissingFolderIsEmpty() {
        Assert.That(MetadataModApplier.FindModFiles(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())), Is.Empty);
    }
}
