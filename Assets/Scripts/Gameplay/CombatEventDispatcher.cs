using UnityEngine;
using System.Collections;

// Handles all combat events that could ever happen in a battle.
// Skill effects may only interact with the combat through this class.
public class CombatEventDispatcher {

    private CombatResolver Resolver { get; set; }

    public CombatEventDispatcher(CombatResolver resolver)
    {
        Resolver = resolver;
    }

    // Gets the party of a unit.
    public CombatPartyType GetUnitParty(Unit unit)
    {
        return Resolver.GetUnitParty(unit);
    }

    // Notifys the combat resolver that a skill is going to be used by a unit.
    // Checks if any skill should then react to this.
    public void NotifyPreCast(Unit source, Skill skill)
    {

    }

    // Source unit deals an amount of damage to target unit.
    // This reduces the health of target, while having the potential of triggering reaction effects.
    public void DealDamage(Unit source, Unit target, int damage)
    {
        if (damage > 0)
        {
            target.Status.Life -= damage;
            if (target.Status.Life <= 0)
            {
                target.Status.Life = 0;
                NotifyUnitDeath(target);
            }
        }
    }

    // Source unit recovers an amount of damage from target.
    public void RecoverLife(Unit source, Unit target, int recover, bool affectDead)
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

    // Notifys the combat resolver that a unit has died.
    public void NotifyUnitDeath(Unit unitDead)
    {

    }
}
