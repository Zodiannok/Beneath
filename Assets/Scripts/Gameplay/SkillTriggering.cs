using UnityEngine;
using System.Collections;
using System;

// Defines an interface for checking skill triggers.
//
// A skill can have ReactionPhase as its performed phase. When this is the case,
// the skill's CanReact will be called on every combat event to see if this skill
// should be triggered by that event.
public interface ISkillTriggering
{
    // Check if this skill should react to combat event happening on target.
    bool CanReact(CombatEventDispatcher dispatcher, Unit skillOwner, CombatEvent combatEvent);
}

public class InterruptCastingTrigger : ISkillTriggering
{
    public bool CanReact(CombatEventDispatcher dispatcher, Unit skillOwner, CombatEvent combatEvent)
    {
        // Triggered by Declare event.
        if (combatEvent.Event != CombatEventType.Declare)
        {
            return false;
        }

        if (combatEvent.Party == dispatcher.GetUnitParty(skillOwner))
        {
            return false;
        }
        bool isCastSkill = combatEvent.Skill.SkillDefinition.HasTag(SkillTag.Cast);
        // TODO: can only interrupt spells up to a certain level?
        return isCastSkill;
    }
}
