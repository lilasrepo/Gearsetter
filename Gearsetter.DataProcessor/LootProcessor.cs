using System.Text.Json;
using System.Text.RegularExpressions;
using LLib.GameData;
using Lumina.Excel.Sheets;

namespace Gearsetter.DataProcessor;

public sealed partial class LootProcessor
{
    [GeneratedRegex(@"(.*) Coffer \(IL (\d+)\)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex CofferRegex();

    [GeneratedRegex(@"Required level: (\d+)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RequiredLevelRegex();

    private static readonly Dictionary<string, uint> ItemNamesToIds = [];
    private static readonly Dictionary<uint, string> ItemIdsToNames = [];
    private static readonly Dictionary<uint, List<uint>> ItemSourcesById = [];
    private static Lumina.GameData _lumina = null!;

    public static void Main(string[] args)
    {
        _lumina =
            new("C:/Program Files (x86)/steam/steamapps/common/FINAL FANTASY XIV Online/game/sqpack");
        foreach (var item in _lumina.GetExcelSheet<Item>()!
                     .Where(x => x.RowId > 0 && !string.IsNullOrEmpty(x.Name.ExtractText()))
                     .Where(x => x.ItemAction.RowId is 388 or 1085 || x.EquipSlotCategory.RowId > 0))
        {
            string itemName = item.Name.ExtractText();
            ItemNamesToIds.Add(itemName, item.RowId);
            ItemIdsToNames.Add(item.RowId, itemName);

            // idk what happens in these sheets
            if (itemName.Contains(" (IL 415)"))
                ItemNamesToIds.Add(itemName.Replace("Dwarven Mythril", "High Mythril"), item.RowId);
        }

        foreach (var item in ItemNamesToIds.Where(x => x.Key.Contains(" (IL ")).ToList())
        {
            string itemName = item.Key.Substring(0, item.Key.IndexOf(" (IL ", StringComparison.OrdinalIgnoreCase));
            ItemNamesToIds[itemName] = item.Value;
        }

        ItemNamesToIds["Level 70 Weapon Coffer"] = ItemNamesToIds["Level 50 Weapon Coffer (IL 70)"];
        ItemNamesToIds["Level 90 Weapon Coffer"] = ItemNamesToIds["Level 50 Weapon Coffer (IL 90)"];
        ItemNamesToIds["Level 110 Weapon Coffer"] = ItemNamesToIds["Level 50 Weapon Coffer (IL 110)"];

        ProcessCoffersWithManifests();
        ProcessDownloadedFiles();
        ProcessItemlevelCoffers();

        File.WriteAllText("../../../Gearsetter/lootSources.json",
            JsonSerializer.Serialize(ItemSourcesById.SelectMany(x => x.Value.Select(y => (ItemId: x.Key, SourceId: y)))
                    .GroupBy(x => x.SourceId)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.ItemId).ToList()),
                new JsonSerializerOptions { WriteIndented = true }));

        /*
        foreach (var (itemId, sourceIds) in ItemSourcesById)
            Console.WriteLine(
                $"Item: {ItemIdsToNames[itemId]} ({itemId}): {string.Join(", ", sourceIds.Select(sourceId => $"{ItemIdsToNames[sourceId]} ({sourceId})"))}");
        */
    }

    private static void ProcessCoffersWithManifests()
    {
        foreach (var coffer in _lumina.GetExcelSheet<Item>()!
                     .Where(x => x is { RowId: > 0, FilterGroup: 45, ItemAction.RowId: 1085 }))
        {
            var archiveItems = _lumina.GetSubrowExcelSheet<ArchiveItem>()!.GetRow(coffer.AdditionalData.RowId);
            foreach (var archiveItem in archiveItems)
                ItemSourcesById[(uint)archiveItem.Item.RowId] = [coffer.RowId];
        }
    }

    private static void ProcessDownloadedFiles()
    {
        var itemSources =
            JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText("itemSources.json"))!;
        itemSources = itemSources.Where(x => !x.Value.Any(y => y.EndsWith(" Sack") || y.EndsWith(" Map")))
            .ToDictionary(x => x.Key, x => x.Value);
        foreach (var (itemName, sources) in itemSources)
        {
            if (itemName.EndsWith(" Card") || itemName.EndsWith(" Shard") || itemName.EndsWith(" Crystal") ||
                itemName.EndsWith(" Cluster") || itemName.Contains(" Materia ") || itemName.EndsWith(" Potion") ||
                itemName.EndsWith("Fantasia") || itemName.EndsWith(" Barding") || itemName.EndsWith(" Corsage"))
            {
                Console.WriteLine($"Skipping {itemName}");
                continue;
            }

            if (!ItemNamesToIds.TryGetValue(itemName, out var itemId))
            {
                Console.WriteLine($"Item not found: {itemName}");
                continue;
            }

            if (ItemSourcesById.ContainsKey(itemId))
                continue;

            List<uint> sourceIds = sources
                .Select(x => ItemNamesToIds.TryGetValue(x, out var y) ? y : 0)
                .Where(x => x != 0)
                .ToList();
            if (sourceIds.Count > 0)
                ItemSourcesById[itemId] = sourceIds;
            else
                Console.WriteLine($"No sources: {itemName} ({itemId}); expected {string.Join(", ", sources)}");
        }
    }

    private static void ProcessItemlevelCoffers()
    {
        foreach (var (itemId, itemName) in ItemIdsToNames.Where(x => x.Value.Contains(" (IL ")))
        {
            // we have data for this coffer already?
            if (ItemSourcesById.Any(x => x.Value.Contains(itemId)))
                continue;

            ProcessItemlevelCoffer(itemId, itemName);
        }
    }

    private static readonly string[] ExcludedItems =
    [
        "Sardine", "Gentlemage's", "Blue-eyes", "Shinryu's", "Tsukuyomi's", "Suzaku's", "Seiryu's", "Byakko's",
        "Susano's", "Kinna"
    ];

    private static void ProcessItemlevelCoffer(uint itemId, string itemName)
    {
        var cofferMatch = CofferRegex().Match(itemName);
        if (!cofferMatch.Success)
        {
            Console.WriteLine("Couldn't parse coffer: {itemName}");
            return;
        }

        var sheetItem = _lumina.GetExcelSheet<Item>()!.GetRow(itemId);
        var descriptionMatch = RequiredLevelRegex().Match(sheetItem.Description.ExtractText());
        if (!descriptionMatch.Success)
        {
            Console.WriteLine($"Couldn't parse required level: {sheetItem.Description.ExtractText()}");
            return;
        }

        string cofferName = cofferMatch.Groups[1].Value;
        foreach (var (search, replace) in Mappings.AlternateCofferNames)
            cofferName = cofferName.Replace(search, replace);

        byte level = byte.Parse(descriptionMatch.Groups[1].Value);
        int itemLevel = int.Parse(cofferMatch.Groups[2].Value);

        if (Mappings.ArtifactGear.TryGetValue(itemId, out EClassJob classJob))
        {
            Console.WriteLine($"Artifact Gear: {classJob} ({itemId}) @ {itemLevel}");
            var possibleItems = _lumina.GetExcelSheet<Item>()!
                .Where(x => x is { RowId: > 0, EquipSlotCategory.RowId: > 0, Rarity: not 4 })
                .Where(x => x.LevelEquip == level && x.LevelItem.RowId == itemLevel)
                .Where(x => Mappings.ArtifactJobToCategory[classJob] == x.ClassJobCategory.RowId)
                .Where(x => ExcludedItems.All(y => !x.Name.ExtractText().Contains(y)))
                .Where(x => itemLevel <= 290 || x.EquipSlotCategory.Value.MainHand == 0)
                .ToList();
            foreach (var item in possibleItems)
            {
                Console.WriteLine($"  {item.Name.ExtractText()}");

                if (ItemSourcesById.TryGetValue(item.RowId, out var sources))
                    sources.Add(itemId);
                else
                    ItemSourcesById.Add(item.RowId, [itemId]);
            }

            return;
        }

        foreach (var (suffix, equipSlotCategories) in Mappings.EquipSlotCategories)
        {
            if (cofferName.EndsWith($" {suffix}"))
            {
                var setName = cofferName.Substring(0, cofferName.Length - suffix.Length - 1);
                List<string> setNames = [setName];
                if (Mappings.AlternateSetNames.TryGetValue(setName, out var alternateNames))
                    setNames.AddRange(alternateNames);

                var possibleItems = _lumina.GetExcelSheet<Item>()!
                    .Where(x => x.LevelEquip == level && x.LevelItem.RowId == itemLevel &&
                                equipSlotCategories.Contains(x.EquipSlotCategory.RowId));

                // we have multiple gearsets with different names at level caps
                possibleItems = possibleItems
                    .Where(x => x.LevelEquip % 10 != 0 || setNames.Any(n => x.Name.ExtractText().Contains(n)));

                possibleItems = possibleItems.ToList();
                if (possibleItems.Any())
                {
                    /*
                    Console.WriteLine($"Set: {setName}, Type: {suffix}, Item Level: {itemLevel}");
                    foreach (var item in possibleItems)
                        Console.WriteLine($"  {item.Name.ExtractText()}");
                    */
                    foreach (var item in possibleItems)
                    {
                        if (ItemSourcesById.TryGetValue(item.RowId, out var sources))
                            sources.Add(itemId);
                        else
                            ItemSourcesById.Add(item.RowId, [itemId]);
                    }
                }
                else
                    Console.WriteLine($"Set: {setName}, Type: {suffix}, Item Level: {itemLevel}");

                return;
            }
        }

        Console.WriteLine($"Coffer: {itemName} ({itemId})");
    }
}
