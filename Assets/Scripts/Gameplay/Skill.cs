using System;
using System.Collections;

[Flags]
public enum PartyPositionFlag
{
    Defense = 1 << PartyPosition.Defense,
    Attack = 1 << PartyPosition.Attack,
    Support = 1 << PartyPosition.Support,
}

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

public class Skill {

    public string InternalName { get; internal set; }
    public string DisplayedName { get; internal set; }

    public int SkillMaxUsage { get; set; }
    public int SkillCurrentUsage { get; set; }

    // Which position in the party can use this skill?
    public PartyPositionFlag AllowedPositions { get; internal set; }

    // In which phase of combat is this skill used?
    public CombatPhase PerformedPhase { get; internal set; }

    // Skill tags.
    // Use SetTag() and HasTag() to access this.
    private BitArray Tags { get; set; }

    // Effect of this skill.
    // TODO: Support list of effects?
    public ISkillEffect Effect { get; internal set; }

    // Targeting criteria of this skill.
    // TODO: If we are going to support list of effects, we may need multiple criterias for each skill.
    public ISkillTargeting Targeting { get; internal set; }

    #region Display Methods

    // Format of the log text. This is a string key to the actual string in the global string table.
    public string LogTextFormat { get; internal set; }

    #endregion

    public Skill()
    {
        InternalName = "";
        DisplayedName = "Default Skill";

        SkillMaxUsage = 1;
        SkillCurrentUsage = 1;

        AllowedPositions = PartyPositionFlag.Defense | PartyPositionFlag.Attack | PartyPositionFlag.Support;
        PerformedPhase = CombatPhase.AttackPhase;
        Tags = new BitArray((int)SkillTag.NumSkillTags, false);

        // TODO: How do we setup different effect / targeting?
        Effect = new AttackEffect();
        Targeting = new StandardSingleTargeting();

        LogTextFormat = "";
    }

    // Called internally to create a copy of a skill.
    internal Skill Copy()
    {
        return MemberwiseClone() as Skill;
    }

    public bool CanBeUsedInPosition(PartyPosition position)
    {
        if (position == PartyPosition.NotAssigned)
        {
            return false;
        }
        PartyPositionFlag positionToFlag = (PartyPositionFlag)(1 << (int)position);
        return (AllowedPositions & positionToFlag) != 0;
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
