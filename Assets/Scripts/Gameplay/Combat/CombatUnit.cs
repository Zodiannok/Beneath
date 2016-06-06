using UnityEngine;
using System.Collections;

public class CombatUnit
{
    public Unit Unit { get; private set; }
    public Skill Skill { get; private set; }
    public CombatPartyType CombatParty { get; private set; }
    public PartyPosition Position { get; private set; }

    public CombatSkillExecuter CurrentSkillExecuter { get; private set; }
    public int CurrentSkillUsage { get; internal set; }
    public int SkillUsageMax { get; internal set; }

    public CombatUnit(CombatResolver resolver, Unit unit)
    {
        Unit = unit;
    }
}
