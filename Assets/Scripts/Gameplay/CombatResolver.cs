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
    public CombatEvent(CombatSkillExecuter triggeringSkillExecutor, CombatEventType eventType)
    {
        CombatResolver resolver = triggeringSkillExecutor.Resolver;

        Executer = triggeringSkillExecutor;

        Skill = triggeringSkillExecutor.ExecutedSkill;
        User = Skill.Owner;

        Party = resolver.GetUnitParty(User);
        Phase = Skill.SkillDefinition.PerformedPhase;
        Event = eventType;
    }

    public CombatSkillExecuter Executer { get; private set; }

    public CombatPartyType Party { get; private set; }
    public CombatPhase Phase { get; private set; }
    public CombatEventType Event { get; private set; }

    public Skill Skill { get; private set; }
    public Unit User { get; private set; }
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
        Stack<CombatEvent> eventsStack = new Stack<CombatEvent>();

        // Figure out the action order.
        // Iterate through each phase to see if any action is triggered.
        for (CombatPhase phase = CombatPhase.PreparationPhase; phase <= CombatPhase.RecoveryPhase; ++phase)
        {
            foreach (PartyPosition position in SkillUsageOrder)
            {
                Skill offenseSkill = OffenseParty.GetAssignedSkill(position);
                if (offenseSkill != null && offenseSkill.SkillDefinition.PerformedPhase == phase)
                {
                    CombatSkillExecuter executer = new CombatSkillExecuter(this, offenseSkill);

                    CombatEvent combatEvent = new CombatEvent(executer, CombatEventType.Declare);
                    ResolveDeclareEvent(combatEvent, eventsStack);

                    // If the combat event is still valid, execute the event.
                    if (!executer.IsInterrupted)
                    {
                        combatEvent = new CombatEvent(executer, CombatEventType.Apply);
                        ResolveExecuteEvent(combatEvent, eventsStack);
                    }
                }

                Skill defenseSkill = DefenseParty.GetAssignedSkill(position);
                if (defenseSkill != null && defenseSkill.SkillDefinition.PerformedPhase == phase)
                {
                    CombatSkillExecuter executer = new CombatSkillExecuter(this, defenseSkill);

                    CombatEvent combatEvent = new CombatEvent(executer, CombatEventType.Declare);
                    ResolveDeclareEvent(combatEvent, eventsStack);

                    // If the combat event is still valid, execute the event.
                    if (!executer.IsInterrupted)
                    {
                        combatEvent = new CombatEvent(executer, CombatEventType.Apply);
                        ResolveExecuteEvent(combatEvent, eventsStack);
                    }
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

    private void ResolveDeclareEvent(CombatEvent declareEvent, Stack<CombatEvent> eventsStack)
    {
        // Push the event into stack.
        eventsStack.Push(declareEvent);

        Unit userUnit = declareEvent.User;
        Skill userSkill = declareEvent.Skill;

        // Check if the skill can be used (user is alive, skill is not used up, etc.)
        if (userUnit == null || userUnit.IsDead)
        {
            declareEvent.Executer.Interrupt();
        }
        if (userSkill == null || userSkill.SkillCurrentUsage == 0)
        {
            declareEvent.Executer.Interrupt();
        }

        if (!declareEvent.Executer.IsInterrupted)
        {
            GetSkillTargets(declareEvent, userSkill, declareEvent.Executer.Targets);

            // If we don't have valid targets, then this skill will not be triggered.
            if (declareEvent.Executer.Targets.Count == 0)
            {
                declareEvent.Executer.Interrupt();
            }
        }

        if (!declareEvent.Executer.IsInterrupted)
        {
            // The skill is now triggered. Decrement skill usage times right now.
            // If another reaction skill cancels this skill, the skill will still be consumed.
            --userSkill.SkillCurrentUsage;

            // TODO: check if any reactions are triggered.
            // Reactions may modify declareEvent, causing the attempt to use this skill to fail.
            // If this happens, the IsValid field on the event shall also be set to false.
        }

        // Pop the event from stack.
        // TODO: output the declaration result into a linear log.
        eventsStack.Pop();
    }

    private void GetSkillTargets(CombatEvent combatEvent, Skill userSkill, List<Unit> targets)
    {
        if (userSkill == null || userSkill.SkillDefinition.Targeting == null)
        {
            return;
        }

        Party allyParty;
        Party targetParty;
        if (combatEvent.Party == CombatPartyType.Offense)
        {
            allyParty = OffenseParty;
            targetParty = DefenseParty;
        }
        else
        {
            allyParty = DefenseParty;
            targetParty = OffenseParty;
        }

        userSkill.SkillDefinition.Targeting.GetTargets(this, combatEvent.User, allyParty, targetParty, targets);
    }

    private void ResolveExecuteEvent(CombatEvent executeEvent, Stack<CombatEvent> eventsStack)
    {
        // Push the event into stack.
        eventsStack.Push(executeEvent);

        // Execute effects on all targets.
        if (executeEvent.Skill != null && executeEvent.Skill.SkillDefinition.Effect != null)
        {
            CombatEventDispatcher dispatcher = new CombatEventDispatcher(this);

            foreach (Unit target in executeEvent.Executer.Targets)
            {
                CombatEventLog log = new CombatEventLog(executeEvent.Skill);
                log.LogValues.Add("user", executeEvent.User.Profile.Name);
                log.LogValues.Add("target", target.Profile.Name);
                log.LogValues.Add("skillname", executeEvent.Skill.SkillDefinition.DisplayedName);

                executeEvent.Skill.SkillDefinition.Effect.Apply(dispatcher, executeEvent.User, target, log);
                _CombatEventLog.Add(log);
            }
        }

        // Pop the event from stack.
        // TODO: output the declaration result into a linear log.
        eventsStack.Pop();
    }

    // Check all units to handle spells triggered by a combat event.
    internal void HandleCombatEvent(CombatEvent combatEvent)
    {

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
