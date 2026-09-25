using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using LLib.GameData;
using LLib.Gear;
using Lumina.Excel.Sheets;

namespace Gearsetter.Model;

internal sealed class GearsetData
{
    public unsafe GearsetData(IDataManager dataManager, GearStatsCalculator gearStatsCalculator,
        RaptureGearsetModule.GearsetEntry* gearset, string name)
    {
        Id = gearset->Id;
        ClassJob = (EClassJob)gearset->ClassJob;
        Name = name;
        MainHand = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.MainHand);
        OffHand = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.OffHand);
        Head = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.Head);
        Body = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.Body);
        Hands = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.Hands);
        Legs = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.Legs);
        Feet = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.Feet);
        Ears = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.Ears);
        Neck = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.Neck);
        Wrists = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.Wrists);
        RingLeft = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.RingLeft);
        RingRight = GetItem(dataManager, gearStatsCalculator, gearset, RaptureGearsetModule.GearsetItemIndex.RingRight);
    }

    private static unsafe EquipmentItem? GetItem(IDataManager dataManager, GearStatsCalculator gearStatsCalculator,
        RaptureGearsetModule.GearsetEntry* gearset, RaptureGearsetModule.GearsetItemIndex index)
    {
        var gearsetItem = gearset->GetItem(index);
        if (gearsetItem.ItemId == 0)
            return null;

        var item = dataManager.GetExcelSheet<Item>().GetRow(gearsetItem.ItemId % 1_000_000);
        bool hq = gearsetItem.ItemId > 1_000_000;
        return new EquipmentItem(item, hq, gearStatsCalculator.CalculateGearStats(item, hq, []), null);
    }

    public byte Id { get; }
    public EClassJob ClassJob { get; }
    public string Name { get; }
    public EquipmentItem? MainHand { get; }
    public EquipmentItem? OffHand { get; }
    public EquipmentItem? Head { get; }
    public EquipmentItem? Body { get; }
    public EquipmentItem? Hands { get; }
    public EquipmentItem? Legs { get; }
    public EquipmentItem? Feet { get; }
    public EquipmentItem? Ears { get; }
    public EquipmentItem? Neck { get; }
    public EquipmentItem? Wrists { get; }
    public EquipmentItem? RingLeft { get; }
    public EquipmentItem? RingRight { get; }
}
