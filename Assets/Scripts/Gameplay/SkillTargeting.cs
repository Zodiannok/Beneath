using System;
using System.Collections.Generic;

public interface ISkillTargeting
{
    void GetTargets(CombatResolver resolver, Unit user, Party allyParty, Party opponentParty, List<Unit> targets);
}

public class StandardSingleTargeting : ISkillTargeting
{
    // When a skill uses "standard targeting", it targets members of a party in this order.
    public static readonly PartyPosition[] SkillTargetOrder = { PartyPosition.Defense, PartyPosition.Attack, PartyPosition.Support };

    public void GetTargets(CombatResolver resolver, Unit user, Party allyParty, Party opponentParty, List<Unit> targets)
    {
        foreach (PartyPosition position in SkillTargetOrder)
        {
            Unit targetUnit = opponentParty.GetAssignedUnit(position);
            if (targetUnit != null && !targetUnit.IsDead)
            {
                targets.Add(targetUnit);
                break;
            }
        }
    }
}

public class LowestHealthTargeting : ISkillTargeting
{
    // Does this target ally party or enemy party?
    public bool TargetAllyParty { get; set; }

    // Is "lowest health" calculated by percentage or by absolute value?
    public bool ByPercentage { get; set; }

    public void GetTargets(CombatResolver resolver, Unit user, Party allyParty, Party opponentParty, List<Unit> targets)
    {
        Party targetParty = TargetAllyParty ? allyParty : opponentParty;

        Unit targetUnit = null;
        if (ByPercentage)
        {
            float percentage = 0.0f;
            // Go through each position in standard targeting order, such that if multiple units have the same percentage,
            // it prioritize the unit with standard targeting order.
            foreach (PartyPosition position in StandardSingleTargeting.SkillTargetOrder)
            {
                Unit member = targetParty.GetAssignedUnit(position);
                if (member != null && !member.IsDead)
                {
                    float percentageLife = (float)member.Status.Life / (float)member.Status.MaxLife;
                    if (targetUnit == null || percentageLife < percentage)
                    {
                        targetUnit = member;
                        percentage = percentageLife;
                    }
                }
            }
        }
        else
        {
            int life = 0;
            // Same prioritization as the percentage case.
            foreach (PartyPosition position in StandardSingleTargeting.SkillTargetOrder)
            {
                Unit member = targetParty.GetAssignedUnit(position);
                if (member != null && !member.IsDead)
                {
                    if (targetUnit == null || member.Status.Life < life)
                    {
                        targetUnit = member;
                        life = member.Status.Life;
                    }
                }
            }
        }

        if (targetUnit != null)
        {
            targets.Add(targetUnit);
        }
    }
}

// Target a specific party position
public class PositionTargeting : ISkillTargeting
{
    public bool TargetAlly { get; set; }
    public PartyPosition Position { get; set; }

    public void GetTargets(CombatResolver resolver, Unit user, Party allyParty, Party opponentParty, List<Unit> targets)
    {
        Party targetedParty = TargetAlly ? allyParty : opponentParty;
        Unit targetedUnit = targetedParty.GetAssignedUnit(Position);

        if (targetedUnit != null)
        {
            targets.Add(targetedUnit);
        }
    }
}