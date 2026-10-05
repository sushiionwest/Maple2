# Mods

Server-side game data overrides. Every `*.json` file in this folder is a patch against the
metadata database (`maple-data`) that `Maple2.File.Ingest` builds from the client files. Patches
let you change items, NPCs, maps, quests, skills, shops and other tables without repacking any
`.m2d` archives.

Anything whose name starts with `_` or `.` is skipped, so `_examples/` is ignored and you can
turn a mod off by renaming it to `_whatever.json`.

## The loop

1. Find the row you want to change and dump it as a starting point:

   ```
   dotnet run --project Maple2.File.Ingest -- --dump item Id=50400001
   ```

   This prints the row as a ready-made patch. Keep only the fields you want to change.
   In game, `find item <name>` / `find npc <name>` gives you ids.

2. Save it as a file in this folder, e.g. `mods/potions.json`.

3. Apply it and hot reload, from in game (needs the Debug admin permission):

   ```
   reload --mods            # apply mods, then reload everything on every channel
   reload --mods item npc   # apply mods, then reload only item and npc data
   reload item              # reload without applying (e.g. after editing the DB by hand)
   reload --local item      # only this channel
   ```

   Or from a terminal, without a running server:

   ```
   dotnet run --project Maple2.File.Ingest -- --mods-only
   ```

Reload targets: `all` (default), `item`, `npc`, `map`, `quest`, `skill`, `ride`, `achievement`,
`script`, `ai`, `cube`, `table`, `server-table`, `trigger`.

A full ingest (`dotnet run --project Maple2.File.Ingest`) also applies this folder at the end,
so your mods survive re-ingesting client data.

## Patch format

A file holds one patch object or an array of them. Comments (`//`) and trailing commas are allowed.

```jsonc
{
  "table": "item",                 // metadata table: item, npc, map, quest, skill, table, server-table, ...
  "key": { "Id": 50400001 },       // columns that identify the row
  "set": {                         // columns to change
    "Name": "Mega Potion",         // plain column: replaced
    "Property": { "SlotMax": 9999 } // JSON column + object: deep-merged into what's there
  },
  "comment": "optional note"
}
```

- **Deep merge.** Objects written to JSON columns are merged with
  [JSON merge patch](https://www.rfc-editor.org/rfc/rfc7396) rules: keys you leave out are kept,
  `null` removes a key, and **arrays are replaced whole** (copy the full array from `--dump`).
  A non-object value replaces the column outright.
- **New rows.** `"clone": { "Id": 50400001 }` copies an existing row to the `key` row (replacing it
  if it already exists), then applies `set`. New item, NPC or map ids are only server-side: the game
  client also needs matching entries in its own data files to show them.
- **Removing rows.** `"delete": true` removes the `key` row.
- Field names are the C# property names in `Maple2.Model/Metadata/*.cs`. The JSON omits fields at
  their default value (0, false), so a field can be missing from `--dump` and still be settable.
- `table` and `server-table` rows hold a whole table in one `Table` column, keyed by `Name`
  (for example `{ "Name": "shop_item.xml" }`). Use `--dump` to find the exact name.

## Safety

- Every run is one transaction. If any patch fails (unknown table or column, missing row, bad
  JSON) nothing is written and the errors are listed.
- Patches are idempotent, so re-running is always safe.
- Patching marks the table as changed, so the next full ingest restores it from client data and
  re-applies this folder. **Deleted a patch?** Run a full ingest to put the original data back.

## Limits of hot reload

`reload` makes the server re-read data on next use. Things built from data before the reload keep
the old values until they are rebuilt: NPCs already spawned in a loaded map, items already in
inventories (their stats were rolled at creation), and loaded map instances (leave and re-enter, or
wait for the map to unload). Values copied at server start-up (some constants) still need a restart.
