using System.Collections.Generic;
using LLib.GameData;
using LLib.Gear;
using Lumina.Excel.Sheets;

namespace Gearsetter.Model;

internal sealed record EquipmentItem(Item Item, bool Hq, EquipmentStats Stats, IReadOnlyList<uint>? SourceIds)
    : BaseItem(Item, Hq, Stats, SourceIds)
{
    public override EClassJob ClassJob { get; init; } = EClassJob.Adventurer;
}
