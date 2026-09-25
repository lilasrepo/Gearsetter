using System.Collections.Generic;
using LLib.GameData;
using LLib.Gear;
using Lumina.Excel.Sheets;

namespace Gearsetter.Model;

internal sealed record InventoryItem(Item Item, bool Hq, EquipmentStats Stats, EClassJob ClassJob, IReadOnlyList<uint>? SourceIds)
    : BaseItem(Item, Hq, Stats, SourceIds)
{
}
