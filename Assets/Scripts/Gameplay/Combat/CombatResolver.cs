using System;
using System.Collections.Generic;

public enum CombatPartyType
{
    None,
    Offense,
    Defense,
}

public class CombatEvent
{
    public CombatEvent(CombatSkillExecuter triggeringSkillExecutor, CombatEventType eventType, CombatUnit eventTarget)
    {
        CombatResolver resolver = triggeringSkillExecutor.Resolver;

        Executer = triggeringSkillExecutor;

        Skill = triggeringSkillExecutor.ExecutedSkill;
        User = triggeringSkillExecutor.Owner;

        Party = User.CombatParty;
        Phase = Skill.SkillDefinition.PerformedPhase;
        Event = eventType;
        Target = eventTarget;
    }

    public CombatSkillExecuter Executer { get; private set; }

    public CombatPartyType Party { get; private set; }
    public CombatPhase Phase { get; private set; }
    public CombatEventType Event { get; private set; }

    public Skill Skill { get; private set; }
    public CombatUnit User { get; private set; }
    public CombatUnit Target { get; private set; }
}

public class CombatEventLog
{
    public CombatEventLog(Skill skill)
    {
        Skill = skill;
        _LogValues = new Dictionary<string, string>();
    }

    public Skill Skill { get; private set; }
    public IDictionary<string, string> LogValues { get { return _LogValues; } }

    private Dictionary<string, string> _LogValues;
}

// Class that takes two parties and resolve a combat between them.
public class CombatResolver {

    // When multiple skills are being used in the same phase, they are resolved in this priority order.
    public static readonly PartyPosition[] SkillUsageOrder = { PartyPosition.Support, PartyPosition.Attack, PartyPosition.Defense };

    // The two parties have an offense side and a defense side.
    // If two actions take place at the same time, the offense party's action is always resolved first.
    public Party OffenseParty { get; private set; }
    public Party DefenseParty { get; private set; }

    public IList<CombatEventLog> CombatEventLog { get { return _CombatEventLog; } }

    // Store the CombatUnits created to reflect combat status of participating units.
    private CombatUnit[] _OffenseCombatUnits;
    private CombatUnit[] _DefenseCombatUnits;

    // Event log. Stores all actions in chronological order.
    private List<CombatEventLog> _CombatEventLog;

    public CombatResolver(Party offense, Party defense)
    {
        OffenseParty = offense;
        DefenseParty = defense;

        _CombatEventLog = new List<CombatEventLog>();
        _OffenseCombatUnits = new CombatUnit[Party.MaxAssignments];
        _DefenseCombatUnits = new CombatUnit[Party.MaxAssignments];
    }

    public void GenerateAllCombatEvents()
    {
        _CombatEventLog.Clear();
        InitializeCombatStatus();

        // Figure out the action order.
        // Iterate through each phase to see if any action is triggered.
        for (CombatPhase phase = CombatPhase.PreparationPhase; phase <= CombatPhase.RecoveryPhase; ++phase)
        {
            foreach (PartyPosition position in SkillUsageOrder)
            {
                Skill offenseSkill = OffenseParty.GetAssignedSkill(position);
                if (offenseSkill != null && offenseSkill.SkillDefinition.PerformedPhase == phase)
                {
                    ResolveSkill(GetCombatUnit(CombatPartyType.Offense, position));
                }

                Skill defenseSkill = DefenseParty.GetAssignedSkill(position);
                if (defenseSkill != null && defenseSkill.SkillDefinition.PerformedPhase == phase)
                {
                    ResolveSkill(GetCombatUnit(CombatPartyType.Defense, position));
                }
            }
        }
    }

    // Initializes party status to make sure all values are set to the start of the combat.
    private void InitializeCombatStatus()
    {
        foreach (PartyPosition position in SkillUsageOrder)
        {
            CombatUnit offenseCombatUnit = new CombatUnit(this, CombatPartyType.Offense, position);
            _OffenseCombatUnits[(int)position] = offenseCombatUnit;
            CombatUnit defenseCombatUnit = new CombatUnit(this, CombatPartyType.Defense, position);
            _DefenseCombatUnits[(int)position] = defenseCombatUnit;
        }
    }

    private void ResolveSkill(CombatUnit skillOwner)
    {
        CombatSkillExecuter executer = new CombatSkillExecuter(this, skillOwner);
        ResolveDeclareEvent(executer);

        // If the combat event is still valid, execute the event.
        if (!executer.IsInterrupted)
        {
            ResolveExecuteEvent(executer);
        }
    }

    private void ResolveDeclareEvent(CombatSkillExecuter executer)
    {
        Skill userSkill = executer.ExecutedSkill;
        CombatUnit user = executer.Owner;

        // Check if the skill can be used (user is alive, skill is not used up, etc.)
        if (user.Unit == null || user.Unit.IsDead)
        {
            executer.Interrupt();
        }
        if (userSkill == null || userSkill.SkillCurrentUsage == 0)
        {
            executer.Interrupt();
        }

        if (!executer.IsInterrupted)
        {
            executer.GenerateSkillTargets();

            // If we don't have valid targets, then this skill will not be triggered.
            if (executer.Targets.Count == 0)
            {
                executer.Interrupt();
            }
        }

        if (!executer.IsInterrupted)
        {
            // The skill is now triggered. Decrement skill usage times right now.
            // If another reaction skill cancels this skill, the skill will still be consumed.
            --userSkill.SkillCurrentUsage;

            // Check if any reactions are triggered.
            // Reactions may modify declareEvent, causing the attempt to use this skill to fail.
            // If this happens, the IsValid field on the event shall also be set to false.

            // React to declare event first.
            CombatEvent declareEvent = new CombatEvent(executer, CombatEventType.Declare, user);
            HandleCombatEvent(declareEvent);

            // Then react to targeting event for each target, if the skill execution is still valid.
            if (!executer.IsInterrupted)
            {
                foreach (CombatUnit targetUnit in executer.Targets)
                {
                    CombatEvent targetingEvent = new CombatEvent(executer, CombatEventType.Target, targetUnit);
                    HandleCombatEvent(targetingEvent);
                }
            }
        }
    }

    private void ResolveExecuteEvent(CombatSkillExecuter executer)
    {
        Skill userSkill = executer.ExecutedSkill;

        // Execute effects on all targets.
        if (userSkill != null && userSkill.SkillDefinition.Effect != null)
        {
            CombatEventDispatcher dispatcher = new CombatEventDispatcher(this);
            CombatUnit userUnit = executer.Owner;

            foreach (CombatUnit target in executer.Targets)
            {
                CombatEventLog log = new CombatEventLog(userSkill);
                log.LogValues.Add("user", userUnit.Unit.Profile.Name);
                log.LogValues.Add("target", target.Unit.Profile.Name);
                log.LogValues.Add("skillname", userSkill.SkillDefinition.DisplayedName);

                userSkill.SkillDefinition.Effect.Apply(dispatcher, userUnit, target, log);
                _CombatEventLog.Add(log);
            }

            // Trigger execute event after skill usage.
            CombatEvent executeEvent = new CombatEvent(executer, CombatEventType.Apply, userUnit);
            HandleCombatEvent(executeEvent);
        }
    }

    // Check all units to handle spells triggered by a combat event.
    internal void HandleCombatEvent(CombatEvent combatEvent)
    {
        CombatEventDispatcher dispatcher = new CombatEventDispatcher(this);

        foreach (PartyPosition position in SkillUsageOrder)
        {
            Skill offenseSkill = OffenseParty.GetAssignedSkill(position);
            if (offenseSkill != null && offenseSkill.SkillDefinition.PerformedPhase == CombatPhase.ReactionPhase)
            {
                ISkillTriggering triggering = offenseSkill.SkillDefinition.Triggering;
                if (triggering != null && triggering.CanReact(dispatcher, offenseSkill.Owner, combatEvent))
                {
                    // Trigger skill.
                    ResolveSkill(GetCombatUnit(CombatPartyType.Offense, position));
                }
            }

            Skill defenseSkill = DefenseParty.GetAssignedSkill(position);
            if (defenseSkill != null && defenseSkill.SkillDefinition.PerformedPhase == CombatPhase.ReactionPhase)
            {
                ISkillTriggering triggering = defenseSkill.SkillDefinition.Triggering;
                if (triggering != null && triggering.CanReact(dispatcher, defenseSkill.Owner, combatEvent))
                {
                    // Trigger skill.
                    ResolveSkill(GetCombatUnit(CombatPartyType.Defense, position));
                }
            }
        }
    }

    internal CombatPartyType GetUnitParty(Unit unit)
    {
        if (OffenseParty.IsPartyMember(unit))
        {
            return CombatPartyType.Offense;
        }
        else if (DefenseParty.IsPartyMember(unit))
        {
            return CombatPartyType.Defense;
        }
        return CombatPartyType.None;
    }

    internal Party GetPartyReference(CombatPartyType partyType)
    {
        if (partyType == CombatPartyType.Offense)
        {
            return OffenseParty;
        }
        else if (partyType == CombatPartyType.Defense)
        {
            return DefenseParty;
        }
        return null;
    }

    internal CombatUnit GetCombatUnit(CombatPartyType partyType, PartyPosition position)
    {
        if (partyType == CombatPartyType.Offense)
        {
            return _OffenseCombatUnits[(int)position];
        }
        else if (partyType == CombatPartyType.Defense)
        {
            return _DefenseCombatUnits[(int)position];
        }
        return null;
    }
}
