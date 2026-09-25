using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace Gearsetter.Model;

internal sealed record RecommendedItemChange(
    uint ItemId,
    InventoryType? SourceInventory,
    int? SourceInventorySlot,
    RaptureGearsetModule.GearsetItemIndex TargetSlot,
    SeString Text);
