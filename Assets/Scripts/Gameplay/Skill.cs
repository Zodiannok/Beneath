using System;
using System.Collections;

public class Skill {

    public Unit Owner { get; internal set; }

    public int SkillMaxUsage { get; internal set; }
    public int SkillCurrentUsage { get; internal set; }

    public SkillDefinition SkillDefinition { get; internal set; }

    public Skill(SkillDefinition definition)
    {
        SkillDefinition = definition;

        Owner = null;
        SkillMaxUsage = definition.BaseUsage;
        SkillCurrentUsage = definition.BaseUsage;
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
        return (SkillDefinition.AllowedPositions & positionToFlag) != 0;
    }
}
