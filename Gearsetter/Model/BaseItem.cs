using System.Collections.Generic;
using System.Linq;
using Gearsetter.GameData;
using LLib.GameData;
using LLib.Gear;
using Lumina.Excel.Sheets;

namespace Gearsetter.Model;

internal abstract record BaseItem(Item Item, bool Hq, EquipmentStats Stats, IReadOnlyList<uint>? SourceIds)
{
    public Item Item { get; } = Item;
    public uint ItemId { get; } = Item.RowId;
    public bool Hq { get; } = Hq;
    public bool CanBeHq { get; } = Item.CanBeHq;
    public string Name { get; } = Item.Name.ToString();
    public byte Level { get; } = Item.LevelEquip;
    public uint ItemLevel { get; } = Item.LevelItem.RowId;
    public byte Rarity { get; } = Item.Rarity;
    public EEquipSlotCategory EquipSlotCategory { get; } = (EEquipSlotCategory)Item.EquipSlotCategory.RowId;
    public uint ItemUiCategory { get; } = Item.ItemUICategory.RowId;
    public abstract EClassJob ClassJob { get; init; }
    public EquipmentStats Stats { get; } = Stats;
    public IReadOnlyList<uint> SourceIds { get; } = SourceIds ?? [];

    public int PrimaryStat { get; init; } = -1;

    public int Damage
    {
        get
        {
            if (ClassJob.DealsMagicDamage())
                return Item.DamageMag + Stats.Get(EBaseParam.DamageMag);
            else if (ClassJob.DealsPhysicalDamage())
                return Item.DamagePhys + Stats.Get(EBaseParam.DamagePhys);
            else
                return 0;
        }
    }

    public bool HasAnyStat(params EBaseParam[] substats)
        => substats.Any(x => Stats.Has(x));

    public bool IsCombatRelicWithoutSubstats()
    {
        return Rarity == 4
               && EquipSlotCategory is EEquipSlotCategory.OneHandedMainHand
                   or EEquipSlotCategory.TwoHandedMainHand
                   or EEquipSlotCategory.Shield
               && !ClassJob.IsCrafter()
               && !ClassJob.IsGatherer()
               && !HasAnyStat(EBaseParam.Crit,
                   EBaseParam.DirectHit, EBaseParam.Determination, EBaseParam.SkillSpeed,
                   EBaseParam.SpellSpeed, EBaseParam.Tenacity);
    }
}
