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
    public CombatEvent(CombatSkillExecuter triggeringSkillExecutor, CombatEventType eventType, Unit eventTarget)
    {
        CombatResolver resolver = triggeringSkillExecutor.Resolver;

        Executer = triggeringSkillExecutor;

        Skill = triggeringSkillExecutor.ExecutedSkill;
        User = Skill.Owner;

        Party = resolver.GetUnitParty(User);
        Phase = Skill.SkillDefinition.PerformedPhase;
        Event = eventType;
        Target = eventTarget;
    }

    public CombatSkillExecuter Executer { get; private set; }

    public CombatPartyType Party { get; private set; }
    public CombatPhase Phase { get; private set; }
    public CombatEventType Event { get; private set; }

    public Skill Skill { get; private set; }
    public Unit User { get; private set; }
    public Unit Target { get; private set; }
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

    // Event log. Stores all actions in chronological order.
    private List<CombatEventLog> _CombatEventLog;

    public CombatResolver(Party offense, Party defense)
    {
        OffenseParty = offense;
        DefenseParty = defense;

        _CombatEventLog = new List<CombatEventLog>();
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
                    ResolveSkill(offenseSkill);
                }

                Skill defenseSkill = DefenseParty.GetAssignedSkill(position);
                if (defenseSkill != null && defenseSkill.SkillDefinition.PerformedPhase == phase)
                {
                    ResolveSkill(defenseSkill);
                }
            }
        }
    }

    // Initializes party status to make sure all values are set to the start of the combat.
    private void InitializeCombatStatus()
    {
        // Resets armor and absorb to 0.
        // TODO: also do this at clean up?
        foreach (Unit member in OffenseParty.Members)
        {
            member.ResetCombatStatus();
        }
        foreach (Unit member in DefenseParty.Members)
        {
            member.ResetCombatStatus();
        }
    }

    private void ResolveSkill(Skill skill)
    {
        CombatSkillExecuter executer = new CombatSkillExecuter(this, skill);
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
        Unit userUnit = userSkill.Owner;

        // Check if the skill can be used (user is alive, skill is not used up, etc.)
        if (userUnit == null || userUnit.IsDead)
        {
            executer.Interrupt();
        }
        if (userSkill == null || userSkill.SkillCurrentUsage == 0)
        {
            executer.Interrupt();
        }

        if (!executer.IsInterrupted)
        {
            GetSkillTargets(executer);

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
            CombatEvent declareEvent = new CombatEvent(executer, CombatEventType.Declare, userUnit);
            HandleCombatEvent(declareEvent);

            // Then react to targeting event for each target, if the skill execution is still valid.
            if (!executer.IsInterrupted)
            {
                foreach (Unit targetUnit in executer.Targets)
                {
                    CombatEvent targetingEvent = new CombatEvent(executer, CombatEventType.Target, targetUnit);
                    HandleCombatEvent(targetingEvent);
                }
            }
        }
    }

    private void GetSkillTargets(CombatSkillExecuter executer)
    {
        Skill userSkill = executer.ExecutedSkill;
        Unit userUnit = userSkill.Owner;

        if (userSkill == null || userSkill.SkillDefinition.Targeting == null)
        {
            return;
        }

        Party allyParty;
        Party targetParty;
        if (GetUnitParty(userUnit) == CombatPartyType.Offense)
        {
            allyParty = OffenseParty;
            targetParty = DefenseParty;
        }
        else
        {
            allyParty = DefenseParty;
            targetParty = OffenseParty;
        }

        userSkill.SkillDefinition.Targeting.GetTargets(this, userUnit, allyParty, targetParty, executer.Targets);
    }

    private void ResolveExecuteEvent(CombatSkillExecuter executer)
    {
        Skill userSkill = executer.ExecutedSkill;

        // Execute effects on all targets.
        if (userSkill != null && userSkill.SkillDefinition.Effect != null)
        {
            CombatEventDispatcher dispatcher = new CombatEventDispatcher(this);
            Unit userUnit = userSkill.Owner;

            foreach (Unit target in executer.Targets)
            {
                CombatEventLog log = new CombatEventLog(userSkill);
                log.LogValues.Add("user", userUnit.Profile.Name);
                log.LogValues.Add("target", target.Profile.Name);
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
                    ResolveSkill(offenseSkill);
                }
            }

            Skill defenseSkill = DefenseParty.GetAssignedSkill(position);
            if (defenseSkill != null && defenseSkill.SkillDefinition.PerformedPhase == CombatPhase.ReactionPhase)
            {
                ISkillTriggering triggering = defenseSkill.SkillDefinition.Triggering;
                if (triggering != null && triggering.CanReact(dispatcher, defenseSkill.Owner, combatEvent))
                {
                    // Trigger skill.
                    ResolveSkill(defenseSkill);
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
}
