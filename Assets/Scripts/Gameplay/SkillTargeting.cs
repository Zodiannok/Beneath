using System;
using System.Collections.Generic;

public interface ISkillTargeting
{
    void GetTargets(CombatResolver resolver, CombatUnit user, List<CombatUnit> targets);
}

public class StandardSingleTargeting : ISkillTargeting
{
    // When a skill uses "standard targeting", it targets members of a party in this order.
    public static readonly PartyPosition[] SkillTargetOrder = { PartyPosition.Defense, PartyPosition.Attack, PartyPosition.Support };

    public void GetTargets(CombatResolver resolver, CombatUnit user, List<CombatUnit> targets)
    {
        CombatPartyType opponentParty = user.OpponentParty;
        foreach (PartyPosition position in SkillTargetOrder)
        {
            CombatUnit targetUnit = resolver.GetCombatUnit(opponentParty, position);
            if (targetUnit != null && !targetUnit.Unit.IsDead)
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

    public void GetTargets(CombatResolver resolver, CombatUnit user, List<CombatUnit> targets)
    {
        CombatPartyType targetParty = TargetAllyParty ? user.CombatParty : user.OpponentParty;

        CombatUnit targetUnit = null;
        if (ByPercentage)
        {
            float percentage = 0.0f;
            // Go through each position in standard targeting order, such that if multiple units have the same percentage,
            // it prioritize the unit with standard targeting order.
            foreach (PartyPosition position in StandardSingleTargeting.SkillTargetOrder)
            {
                CombatUnit member = resolver.GetCombatUnit(targetParty, position);
                if (member != null && !member.Unit.IsDead)
                {
                    float percentageLife = (float)member.Unit.Status.Life / (float)member.Unit.Status.MaxLife;
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
                CombatUnit member = resolver.GetCombatUnit(targetParty, position);
                if (member != null && !member.Unit.IsDead)
                {
                    if (targetUnit == null || member.Unit.Status.Life < life)
                    {
                        targetUnit = member;
                        life = member.Unit.Status.Life;
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

    public void GetTargets(CombatResolver resolver, CombatUnit user, List<CombatUnit> targets)
    {
        CombatPartyType targetedParty = TargetAlly ? user.CombatParty : user.OpponentParty;
        CombatUnit targetedUnit = resolver.GetCombatUnit(targetedParty, Position);

        if (targetedUnit != null)
        {
            targets.Add(targetedUnit);
        }
    }
}
