using System.Collections.Generic;
using Dalamud.Configuration;
using LLib.Gear;

namespace Gearsetter;

internal sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool ShowRecommendationsWhenEnteringGcArea { get; set; } = true;
    public List<EBaseParam> StatPriorityTanks { get; set; } = new();
    public List<EBaseParam> StatPriorityHealer { get; set; } = new();
    public List<EBaseParam> StatPriorityMelee { get; set; } = new();
    public List<EBaseParam> StatPriorityPhysicalRanged { get; set; } = new();
    public List<EBaseParam> StatPriorityCaster { get; set; } = new();
    public List<EBaseParam> StatPriorityCrafter { get; set; } = new();
    public List<EBaseParam> StatPriorityGatherer { get; set; } = new();

    public static Configuration Create()
    {
        // this isn't ideal in all cases, but it's a starting point in case we ever want to make this class-specific
        return new Configuration
        {
            StatPriorityTanks = [EBaseParam.Crit, EBaseParam.DirectHit, EBaseParam.Determination, EBaseParam.Tenacity],
            StatPriorityHealer =
                [EBaseParam.Crit, EBaseParam.DirectHit, EBaseParam.Determination, EBaseParam.SpellSpeed],
            StatPriorityMelee =
                [EBaseParam.Crit, EBaseParam.Determination, EBaseParam.DirectHit, EBaseParam.SkillSpeed],
            StatPriorityPhysicalRanged =
                [EBaseParam.Crit, EBaseParam.Determination, EBaseParam.DirectHit, EBaseParam.SkillSpeed],
            StatPriorityCaster =
                [EBaseParam.Crit, EBaseParam.Determination, EBaseParam.DirectHit, EBaseParam.SpellSpeed],
            StatPriorityCrafter = [EBaseParam.CP, EBaseParam.Craftsmanship, EBaseParam.Control],
            StatPriorityGatherer = [EBaseParam.GP, EBaseParam.Gathering, EBaseParam.Perception]
        };
    }
}
