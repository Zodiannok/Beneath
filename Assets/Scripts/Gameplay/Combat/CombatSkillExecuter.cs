using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Execution of each skill in the combat is divided into a series of events. They execute in the
// following order: Declare, Target, Apply.
//
// However, if a reaction skill is triggered, the reaction skill's events will be inserted between
// the action's declaration and execution. The reaction skill's event chain can then further be
// interrupted by other reaction skills, causing a "stack" of reaction.
//
// The stack will not continue infinitely - we can either set a maximum number of reactions allowed,
// or their cost will eventually make them stop.
//
// Additionally, each skill will have an array of effects. These effects will be handled by
// CombatResolver as events, and these events may also trigger other skills. If this happens, the
// triggered skill will always start its execution after the triggering effects take place.
public enum CombatEventType
{
    // Execution flow events
    Declare,
    Target,
    Apply,

    // Skill effect events
    OnDamage,
    OnRecover,
    OnDeath,
}

public class CombatSkillExecuter {

    public bool IsStarted { get; private set; }
    public bool IsComplete { get; private set; }
    public CombatEventType CurrentStage { get; private set; }

    public CombatResolver Resolver { get; private set; }
    public CombatUnit Owner { get; private set; }
    public Skill ExecutedSkill { get; private set; }

    public bool IsInterrupted { get; private set; }

    public bool IsExecutionStopped
    {
        get
        {
            return IsComplete || IsInterrupted;
        }
    }

    public List<CombatUnit> Targets { get; private set; }

    public CombatSkillExecuter(CombatResolver resolver, CombatUnit combatUnit)
    {
        IsStarted = false;
        IsComplete = false;
        CurrentStage = CombatEventType.Declare;

        Resolver = resolver;
        Owner = combatUnit;
        ExecutedSkill = Owner.Skill;
        IsInterrupted = false;

        Targets = new List<CombatUnit>();
    }

    // Advance to declare stage.
    public void Declare()
    {
        if (IsStarted)
        {
            Debug.LogError("Skill declare called when skill is already executing!");
            return;
        }

        IsStarted = true;
    }

    // Advance to target stage.
    public void Target()
    {
        if (!IsStarted || CurrentStage != CombatEventType.Declare)
        {
            Debug.LogError("Skill target called when skill is not in declare stage!");
            return;
        }

        CurrentStage = CombatEventType.Target;
    }

    // Advance to apply stage.
    public void Apply()
    {
        if (!IsStarted || CurrentStage != CombatEventType.Target)
        {
            Debug.LogError("Skill apply called when skill is not in target stage!");
            return;
        }

        CurrentStage = CombatEventType.Apply;
    }

    // Advance to complete stage
    public void Finish()
    {
        if (!IsStarted || IsComplete || CurrentStage != CombatEventType.Apply)
        {
            Debug.LogError("Skill finish called when skill is not in apply stage!");
            return;
        }

        IsComplete = true;
    }

    // Interrupt this skill.
    public void Interrupt()
    {
        IsInterrupted = true;
    }

    // Generate skill targets list
    public void GenerateSkillTargets()
    {
        Targets.Clear();
        if (ExecutedSkill == null || ExecutedSkill.SkillDefinition.Targeting == null)
        {
            return;
        }

        ExecutedSkill.SkillDefinition.Targeting.GetTargets(Resolver, Owner, Targets);
    }
}
