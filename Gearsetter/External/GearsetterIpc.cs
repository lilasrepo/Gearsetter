using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace Gearsetter.External;

internal sealed class GearsetterIpc : IDisposable
{
    private const string IpcGetRecommendationsForGearset = "Gearsetter.GetRecommendationsForGearset";

    private readonly GearsetterPlugin _plugin;
    private readonly IPluginLog _pluginLog;

    private readonly ICallGateProvider<byte,
            List<(uint ItemId, InventoryType? SourceInventory, int? SourceInventorySlot,
                RaptureGearsetModule.GearsetItemIndex TargetSlot)>>
        _getRecommendationsForGearset;

    public GearsetterIpc(GearsetterPlugin plugin, IDalamudPluginInterface pluginInterface, IPluginLog pluginLog)
    {
        _plugin = plugin;
        _pluginLog = pluginLog;
        _getRecommendationsForGearset =
            pluginInterface
                .GetIpcProvider<byte,
                    List<(uint, InventoryType?, int?, RaptureGearsetModule.GearsetItemIndex TargetSlot)>>(
                    IpcGetRecommendationsForGearset);
        _getRecommendationsForGearset.RegisterFunc(GetRecommendationsForGearset);
    }

    private unsafe List<(uint ItemId, InventoryType? SourceInventory, int? SourceInventorySlot,
            RaptureGearsetModule.GearsetItemIndex TargetSlot)>
        GetRecommendationsForGearset(byte gearsetId)
    {
        if (gearsetId > 100)
            throw new ArgumentOutOfRangeException(nameof(gearsetId));

        var gearsetModule = RaptureGearsetModule.Instance();
        if (gearsetModule == null)
            throw new InvalidOperationException($"{nameof(RaptureGearsetModule)} is null");

        var gearset = gearsetModule->GetGearset(gearsetId);
        if (gearset == null)
            throw new InvalidOperationException($"Gearset {gearsetId} is null");

        if (!gearset->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists))
            throw new InvalidOperationException($"Gearset {gearsetId} does not exist");

        _pluginLog.Verbose($"Checking for gearset upgrades for gearset {gearset->Id}.");
        return _plugin.GetRecommendedUpgrades(gearset)
            .Select(x => (x.ItemId, x.SourceInventory, x.SourceInventorySlot, x.TargetSlot))
            .ToList();
    }

    public void Dispose()
    {
        _getRecommendationsForGearset.UnregisterFunc();
    }
}
