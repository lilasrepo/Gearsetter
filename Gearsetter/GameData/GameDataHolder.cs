using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Gearsetter.Model;
using LLib.GameData;
using LLib.Gear;
using Lumina.Excel.Sheets;

namespace Gearsetter.GameData;

internal sealed class GameDataHolder
{
    private readonly Configuration _configuration;
    private readonly GearStatsCalculator _gearStatsCalculator;
    private readonly ReadOnlyDictionary<uint,List<uint>> _itemsToItemSources;
    private readonly ReadOnlyDictionary<uint, List<EClassJob>> _classJobCategories;
    private readonly IReadOnlyList<ItemList> _allItemLists;

    public GameDataHolder(IDataManager dataManager, Configuration configuration,
        GearStatsCalculator gearStatsCalculator)
    {
        _configuration = configuration;
        _gearStatsCalculator = gearStatsCalculator;

        var itemSourcesToItems = JsonSerializer.Deserialize<Dictionary<uint, List<uint>>>(
            typeof(GameDataHolder).Assembly.GetManifestResourceStream("Gearsetter.LootSources")!)!;
        _itemsToItemSources = itemSourcesToItems.SelectMany(x => x.Value.Select(y => (SourceId: x.Key, ItemId: y)))
            .GroupBy(x => x.ItemId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.SourceId).ToList())
            .AsReadOnly();

        _classJobCategories = dataManager.GetExcelSheet<ClassJobCategory>()
            .ToDictionary(x => x.RowId, x =>
                new Dictionary<EClassJob, bool>
                    {
                        { EClassJob.Adventurer, x.ADV },
                        { EClassJob.Gladiator, x.GLA },
                        { EClassJob.Pugilist, x.PGL },
                        { EClassJob.Marauder, x.MRD },
                        { EClassJob.Lancer, x.LNC },
                        { EClassJob.Archer, x.ARC },
                        { EClassJob.Conjurer, x.CNJ },
                        { EClassJob.Thaumaturge, x.THM },
                        { EClassJob.Carpenter, x.CRP },
                        { EClassJob.Blacksmith, x.BSM },
                        { EClassJob.Armorer, x.ARM },
                        { EClassJob.Goldsmith, x.GSM },
                        { EClassJob.Leatherworker, x.LTW },
                        { EClassJob.Weaver, x.WVR },
                        { EClassJob.Alchemist, x.ALC },
                        { EClassJob.Culinarian, x.CUL },
                        { EClassJob.Miner, x.MIN },
                        { EClassJob.Botanist, x.BTN },
                        { EClassJob.Fisher, x.FSH },
                        { EClassJob.Paladin, x.PLD },
                        { EClassJob.Monk, x.MNK },
                        { EClassJob.Warrior, x.WAR },
                        { EClassJob.Dragoon, x.DRG },
                        { EClassJob.Bard, x.BRD },
                        { EClassJob.WhiteMage, x.WHM },
                        { EClassJob.BlackMage, x.BLM },
                        { EClassJob.Arcanist, x.ACN },
                        { EClassJob.Summoner, x.SMN },
                        { EClassJob.Scholar, x.SCH },
                        { EClassJob.Rogue, x.ROG },
                        { EClassJob.Ninja, x.NIN },
                        { EClassJob.Machinist, x.MCH },
                        { EClassJob.DarkKnight, x.DRK },
                        { EClassJob.Astrologian, x.AST },
                        { EClassJob.Samurai, x.SAM },
                        { EClassJob.RedMage, x.RDM },
                        { EClassJob.BlueMage, x.BLU },
                        { EClassJob.Gunbreaker, x.GNB },
                        { EClassJob.Dancer, x.DNC },
                        { EClassJob.Reaper, x.RPR },
                        { EClassJob.Sage, x.SGE },
                        { EClassJob.Viper, x.VPR },
                        { EClassJob.Pictomancer, x.PCT }
                    }
                    .Where(y => y.Value)
                    .Select(y => y.Key)
                    .ToList())
            .AsReadOnly();
        ClassJobNames = dataManager.GetExcelSheet<ClassJob>()
            .Where(x => x.RowId > 0 && Enum.IsDefined(typeof(EClassJob), x.RowId))
            .OrderBy(x => x.UIPriority)
            .Select(x => ((EClassJob)x.RowId,
                dataManager.Language == ClientLanguage.English ? x.NameEnglish.ToString() : x.Name.ToString()))
            .ToList();
        PrimaryStats = dataManager.GetExcelSheet<ClassJob>()
            .Where(x => x.RowId > 0 && Enum.IsDefined(typeof(EClassJob), x.RowId))
            .Where(x => x.PrimaryStat > 0)
            .ToDictionary(x => (EClassJob)x.RowId, x => (EBaseParam)x.PrimaryStat);
        ItemUiCategoryNames = dataManager.GetExcelSheet<ItemUICategory>()
            .Where(x => x.RowId > 0)
            .OrderBy(x => x.OrderMajor)
            .ThenBy(x => x.OrderMinor)
            .Select(x => (x.RowId, x.Name.ToString()))
            .ToList();

        _allItemLists =
            dataManager.GetExcelSheet<Item>()
                .Where(x => x.RowId > 1600) // exclude outdated names
                .Where(x => x.EquipSlotCategory.RowId > 0 &&
                            Enum.IsDefined(typeof(EEquipSlotCategory), x.EquipSlotCategory.RowId))
                .Where(x => x.LevelItem.RowId > 1) // ignore ilvl 1 glamour items (also includes starter weapons)
                .Where(x => x.ItemSeries.RowId is <= 3 or >= 28)
                .SelectMany(LoadItem)
                .SelectMany(x => x.ClassJobs.Select(y => x.Item with { ClassJob = y }))
                .Where(x => x.ClassJob != EClassJob.Scholar || x.ItemUiCategory != 10) // exclude ACN weapon as scholar
                .Where(x =>
                {
                    bool isGatheringItem = x.HasAnyStat(EBaseParam.Gathering, EBaseParam.Perception, EBaseParam.GP);
                    if (x.ClassJob.IsGatherer())
                        return isGatheringItem;

                    bool isCraftingItem = x.HasAnyStat(EBaseParam.Craftsmanship, EBaseParam.Control, EBaseParam.CP);
                    if (x.ClassJob.IsCrafter())
                        return isCraftingItem;

                    return !isGatheringItem && !isCraftingItem;
                })
                .GroupBy(item => new
                {
                    item.ClassJob,
                    item.EquipSlotCategory,
                    item.ItemUiCategory,
                })
                .Select(x => new ItemList
                {
                    ClassJob = x.Key.ClassJob,
                    EquipSlotCategory = x.Key.EquipSlotCategory,
                    ItemUiCategory = x.Key.ItemUiCategory,
                    Items = x.Cast<BaseItem>().ToList(),
                })
                .ToList()
                .AsReadOnly();

        UpdateAndSortLists();
    }

    public IReadOnlyList<(EClassJob ClassJob, string Name)> ClassJobNames { get; }
    public IReadOnlyList<(uint ItemUiCategory, string Name)> ItemUiCategoryNames { get; }
    public Dictionary<EClassJob, EBaseParam> PrimaryStats { get; }

    public Dictionary<EBaseParam, string> StatNames { get; } = new()
    {
        { EBaseParam.Crit, "Crit" },
        { EBaseParam.DirectHit, "DH" },
        { EBaseParam.Determination, "Det" },
        { EBaseParam.SkillSpeed, "SkS" },
        { EBaseParam.SpellSpeed, "SpS" },
        { EBaseParam.Tenacity, "Tenacity" },

        { EBaseParam.CP, "CP" },
        { EBaseParam.Craftsmanship, "CMS" },
        { EBaseParam.Control, "Control" },

        { EBaseParam.GP, "GP" },
        { EBaseParam.Gathering, "Gathering" },
        { EBaseParam.Perception, "Perception" },
    };

    public InventoryType[] DefaultInventoryTypes { get; } =
    [
        InventoryType.Inventory1,
        InventoryType.Inventory2,
        InventoryType.Inventory3,
        InventoryType.Inventory4,
        InventoryType.ArmoryMainHand,
        InventoryType.ArmoryOffHand,
        InventoryType.ArmoryHead,
        InventoryType.ArmoryBody,
        InventoryType.ArmoryHands,
        InventoryType.ArmoryLegs,
        InventoryType.ArmoryFeets,
        InventoryType.ArmoryEar,
        InventoryType.ArmoryNeck,
        InventoryType.ArmoryWrist,
        InventoryType.ArmoryRings,
        InventoryType.EquippedItems
    ];

    public void UpdateAndSortLists()
    {
        foreach (ItemList itemList in _allItemLists)
        {
            itemList.UpdateStats(PrimaryStats, _configuration);
            itemList.Sort();
        }
    }

    public IEnumerable<ItemList> GetItemLists(EClassJob classJob)
        => _allItemLists.Where(x => x.ClassJob == classJob);

    public ItemList? GetItemList(EClassJob classJob, EEquipSlotCategory equipSlotCategory)
        => _allItemLists.SingleOrDefault(x => x.ClassJob == classJob && x.EquipSlotCategory == equipSlotCategory);

    public IList<(EEquipSlotCategory EquipSlotCategory, string UiCategoryName)> GetItemListsForJob(EClassJob classJob)
    {
        return ItemUiCategoryNames
            .Select(x => new
            {
                x.Name,
                List = _allItemLists.SingleOrDefault(
                    y => y.ClassJob == classJob && x.ItemUiCategory == y.ItemUiCategory)
            })
            .Where(x => x.List != null)
            .Select(x => (x.List!.EquipSlotCategory, x.Name))
            .ToList();
    }

    private IEnumerable<(EquipmentItem Item, List<EClassJob> ClassJobs)> LoadItem(Item item)
    {
        var classJobCategories = _classJobCategories[item.ClassJobCategory.RowId];
        yield return (
            new EquipmentItem(item, false, _gearStatsCalculator.CalculateGearStats(item, false, []), _itemsToItemSources.GetValueOrDefault(item.RowId)),
            classJobCategories);
        if (item.CanBeHq)
        {
            yield return (
                new EquipmentItem(item, true, _gearStatsCalculator.CalculateGearStats(item, true, []), _itemsToItemSources.GetValueOrDefault(item.RowId)),
                classJobCategories);
        }
    }
}
