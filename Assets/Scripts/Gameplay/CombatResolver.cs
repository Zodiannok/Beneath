using System;
using System.Collections.Generic;

// Each action in the combat is divided into two events: declaration and execution.
// When an action is declared, its cost is applied and the action is now prone to reaction skills.
// Without any other reaction skills, the action is then immediately executed and its result resolved.
//
// However, if a reaction skill is triggered, the reaction skill's declaration and execution will be
// inserted between the action's declaration and execution. the reaction skill's execution can
// then further be interrupted by other reaction skills, causing a "stack" of reaction.
//
// The stack will not continue infinitely - we can either set a maximum number of reactions allowed,
// or their cost will eventually make them stop.
public enum CombatEventType
{
    Declare,
    Execute,
}

public enum CombatPartyType
{
    Offense,
    Defense,
}

public class CombatEvent
{
    public CombatEvent(CombatPartyType partyType, PartyPosition position, CombatPhase phase, CombatEventType eventType)
    {
        Party = partyType;
        MemberPosition = position;
        Phase = phase;
        Event = eventType;

        Targets = null;
        IsValid = true;
    }

    public CombatPartyType Party { get; set; }
    public PartyPosition MemberPosition { get; set; }
    public CombatPhase Phase { get; set; }
    public CombatEventType Event { get; set; }

    public Skill Skill { get; set; }
    public Unit User { get; set; }

    public List<Unit> Targets { get; set; }

    public bool IsValid { get; set; }
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
                if (OffenseParty.GetAssignedSkill(position).PerformedPhase == phase)
                {
                    CombatEvent combatEvent = new CombatEvent(CombatPartyType.Offense, position, phase, CombatEventType.Declare);
                    ResolveDeclareEvent(combatEvent, eventsStack);

                    // If the combat event is still valid, execute the event.
                    if (combatEvent.IsValid)
                    {
                        combatEvent.Event = CombatEventType.Execute;
                        ResolveExecuteEvent(combatEvent, eventsStack);
                    }
                }

                if (DefenseParty.GetAssignedSkill(position).PerformedPhase == phase)
                {
                    CombatEvent combatEvent = new CombatEvent(CombatPartyType.Defense, position, phase, CombatEventType.Declare);
                    ResolveDeclareEvent(combatEvent, eventsStack);

                    // If the combat event is still valid, execute the event.
                    if (combatEvent.IsValid)
                    {
                        combatEvent.Event = CombatEventType.Execute;
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
            member.Status.Armor = member.Status.Absorb = 0;
        }
        foreach (Unit member in DefenseParty.Members)
        {
            member.Status.Armor = member.Status.Absorb = 0;
        }
    }

    private void ResolveDeclareEvent(CombatEvent declareEvent, Stack<CombatEvent> eventsStack)
    {
        // Push the event into stack.
        eventsStack.Push(declareEvent);

        Party userParty;
        Unit userUnit;
        Skill userSkill;

        GetEventUser(declareEvent, out userParty, out userUnit, out userSkill);
        declareEvent.User = userUnit;
        declareEvent.Skill = userSkill;

        // Check if the skill can be used (user is alive, skill is not used up, etc.)
        if (userUnit == null || userUnit.IsDead)
        {
            declareEvent.IsValid = false;
        }
        if (userSkill == null || userSkill.SkillCurrentUsage == 0)
        {
            declareEvent.IsValid = false;
        }

        if (declareEvent.IsValid)
        {
            declareEvent.Targets = new List<Unit>();
            GetSkillTargets(declareEvent, userSkill, declareEvent.Targets);

            // If we don't have valid targets, then this skill will not be triggered.
            if (declareEvent.Targets.Count == 0)
            {
                declareEvent.IsValid = false;
            }
        }

        if (declareEvent.IsValid)
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
        if (userSkill == null || userSkill.Targeting == null)
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

        userSkill.Targeting.GetTargets(this, combatEvent.User, allyParty, targetParty, targets);
    }

    private void ResolveExecuteEvent(CombatEvent executeEvent, Stack<CombatEvent> eventsStack)
    {
        // Push the event into stack.
        eventsStack.Push(executeEvent);

        // Execute effects on all targets.
        if (executeEvent.Skill != null && executeEvent.Skill.Effect != null)
        {
            foreach (Unit target in executeEvent.Targets)
            {
                CombatEventLog log = new CombatEventLog(executeEvent.Skill);
                log.LogValues.Add("user", executeEvent.User.Profile.Name);
                log.LogValues.Add("target", target.Profile.Name);
                log.LogValues.Add("skillname", executeEvent.Skill.DisplayedName);

                executeEvent.Skill.Effect.Apply(this, executeEvent.User, target, log);
                _CombatEventLog.Add(log);
            }
        }

        // Pop the event from stack.
        // TODO: output the declaration result into a linear log.
        eventsStack.Pop();
    }

    private void GetEventUser(CombatEvent combatEvent, out Party userParty, out Unit userUnit, out Skill userSkill)
    {
        if (combatEvent.Party == CombatPartyType.Offense)
        {
            userParty = OffenseParty;
        }
        else
        {
            userParty = DefenseParty;
        }

        userUnit = userParty.GetAssignedUnit(combatEvent.MemberPosition);
        userSkill = userParty.GetAssignedSkill(combatEvent.MemberPosition);
    }

    // Source unit deals an amount of damage to target unit.
    // This reduces the health of target, while having the potential of triggering reaction effects.
    internal void DealDamage(Unit source, Unit target, int damage)
    {
        if (damage > 0)
        {
            target.Status.Life -= damage;
            if (target.Status.Life <= 0)
            {
                // TODO: trigger death event.
                target.Status.Life = 0;
            }
        }
    }

    internal void RecoverLife(Unit user, Unit target, int recover, bool affectDead)
    {
        if (recover > 0)
        {
            // If the spell does not affect dead, skip units that are dead.
            if (!affectDead && target.Status.Life == 0)
            {
                return;
            }
            target.Status.Life += recover;
            if (target.Status.Life > target.Status.MaxLife)
            {
                target.Status.Life = target.Status.MaxLife;
            }
        }
    }
}
