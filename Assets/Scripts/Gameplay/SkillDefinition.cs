using UnityEngine;
using System.Collections;

public enum CombatPhase
{
    // The very first phase. Occurs before anything else when the two parties are preparing for the fight.
    PreparationPhase,

    // Initial contact phase where fast attackers may perform their first strikes.
    PreemptivePhase,

    // Generic ranged attack phase to give ranged attackers a slight edge over melee attackers.
    RangedPhase,

    // Generic regular (melee) attack phase.
    AttackPhase,

    // Phase for actions that take time to perform, usually spells that need channeling.
    ChannelPhase,

    // Last phase. Occurs after all combat actions have concluded and the parties are recovering from the fight.
    RecoveryPhase,

    // Special phase for skills that are used not actively but instead reactively to certain actions.
    ReactionPhase,
}

// "Tags" of all skills. A skill can have an arbitrary number of tags.
public enum SkillTag : int
{
    Melee,
    Ranged,
    Attack,
    Cast,

    NumSkillTags,
}

public class SkillDefinition {

    public string InternalName { get; set; }
    public string DisplayedName { get; set; }

    // By default, how many times can this skill be used?
    public int BaseUsage { get; set; }

    // Which position in the party can use this skill?
    public PartyPositionFlag AllowedPositions { get; set; }

    // In which phase of combat is this skill used?
    public CombatPhase PerformedPhase { get; set; }

    // Skill tags.
    // Use SetTag() and HasTag() to access this.
    private BitArray Tags { get; set; }

    // Effect of this skill.
    // TODO: Support list of effects?
    public ISkillEffect Effect { get; set; }

    // Targeting criteria of this skill.
    // TODO: If we are going to support list of effects, we may need multiple criterias for each skill.
    public ISkillTargeting Targeting { get; set; }

    // Triggering criteria of this skill. Checked when PerformedPhase == CombatPhase.ReactionPhase.
    public ISkillTriggering Triggering { get; set; }

    #region Display Methods

    // Format of the log text. This is a string key to the actual string in the global string table.
    public string LogTextFormat { get; internal set; }

    #endregion

    public SkillDefinition()
    {
        InternalName = "";
        DisplayedName = "Default Skill";
        BaseUsage = 1;

        Tags = new BitArray((int)SkillTag.NumSkillTags, false);

        LogTextFormat = "";
    }

    public bool HasTag(SkillTag tag)
    {
        return Tags.Get((int)tag);
    }

    public void SetTag(SkillTag tag, bool value)
    {
        Tags.Set((int)tag, value);
    }
}
