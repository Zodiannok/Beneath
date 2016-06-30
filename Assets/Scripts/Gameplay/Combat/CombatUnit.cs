using UnityEngine;
using System.Collections;

public class CombatUnitStatus
{
    // Both armor and absorb are in-combat only damage mitigation tools.

    // Armor reduces physical damages taken (not consumed).
    public int Armor { get; set; }

    // Absorb reduces all damages taken (consumed).
    public int Absorb { get; set; }

    public void Reset()
    {
        Armor = 0;
        Absorb = 0;
    }
}

public class CombatUnit
{
    public CombatPartyType CombatParty { get; private set; }
    public PartyPosition Position { get; private set; }

    public Unit Unit { get; private set; }
    public Skill Skill { get; private set; }

    public CombatUnitStatus CombatStatus { get; private set; }

    public CombatSkillExecuter CurrentSkillExecuter { get; private set; }
    public int CurrentSkillUsage { get; internal set; }
    public int SkillUsageMax { get; internal set; }

    private CombatResolver Resolver { get; set; }

    public CombatPartyType OpponentParty
    {
        get
        {
            CombatPartyType opponentPartyType;
            switch (CombatParty)
            {
                case CombatPartyType.None:
                default:
                    opponentPartyType = CombatPartyType.None;
                    break;
                case CombatPartyType.Offense:
                    opponentPartyType = CombatPartyType.Defense;
                    break;
                case CombatPartyType.Defense:
                    opponentPartyType = CombatPartyType.Offense;
                    break;
            }
            return opponentPartyType;
        }
    }

    public CombatUnit(CombatResolver resolver, CombatPartyType partyType, PartyPosition position)
    {
        Resolver = resolver;

        CombatParty = partyType;
        Position = position;

        Party partyRef = Resolver.GetPartyReference(partyType);
        Unit = partyRef.GetAssignedUnit(position);
        Skill = partyRef.GetAssignedSkill(position);

        CombatStatus = new CombatUnitStatus();
        CombatStatus.Reset();

        CurrentSkillExecuter = null;
        CurrentSkillUsage = Skill.SkillCurrentUsage;
        SkillUsageMax = Skill.SkillMaxUsage;
    }
}
