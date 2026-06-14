using ImGuiNET;
using Dalamud.Plugin;
using LLib.ImGui;

namespace Gearsetter.Windows;

internal sealed class ConfigWindow : LWindow
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly Configuration _configuration;

    public ConfigWindow(IDalamudPluginInterface pluginInterface, Configuration configuration)
        : base("Gearsetter - Config###GearsetterConfig", ImGuiWindowFlags.AlwaysAutoResize)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;
    }

    public override void DrawContent()
    {
        bool showRecommendationsWhenEnteringGcArea = _configuration.ShowRecommendationsWhenEnteringGcArea;
        if (ImGui.Checkbox("Show recommendations when entering Grand Company area",
                ref showRecommendationsWhenEnteringGcArea))
        {
            _configuration.ShowRecommendationsWhenEnteringGcArea = showRecommendationsWhenEnteringGcArea;
            _pluginInterface.SavePluginConfig(_configuration);
        }
    }
}
