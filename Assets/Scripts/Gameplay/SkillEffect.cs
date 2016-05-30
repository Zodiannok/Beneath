using System;
using UnityEngine;

public interface ISkillEffect
{
    void Apply(CombatEventDispatcher resolver, Unit user, Unit target, CombatEventLog log);
}

public enum DamageType
{
    Physical = 0,
    Magic = 1,
}

public class AttackEffect : ISkillEffect
{
    public DamageType DamageType { get; set; }
    public int BaseDamage { get; set; }
    public float CharacterScaling { get; set; }
    public float ItemScaling { get; set; }

    public void Apply(CombatEventDispatcher resolver, Unit user, Unit target, CombatEventLog log)
    {
        int damage = BaseDamage + Mathf.FloorToInt(user.Status.CharacterLevel * CharacterScaling) + Mathf.FloorToInt(user.Status.ItemLevel * ItemScaling);
        if (DamageType == DamageType.Physical && target.Status.Armor > 0)
        {
            int mitigated = Math.Min(damage, target.Status.Armor);
            damage -= mitigated;

            if (log != null)
            {
                log.LogValues.Add("mitigated", mitigated.ToString());
            }
        }
        if (damage > 0 && target.Status.Absorb > 0)
        {
            int absorbed = Math.Min(damage, target.Status.Absorb);
            damage -= absorbed;
            target.Status.Absorb -= absorbed;

            if (log != null)
            {
                log.LogValues.Add("absorbed", absorbed.ToString());
            }
        }

        resolver.DealDamage(user, target, damage);

        if (log != null)
        {
            log.LogValues.Add("damage", damage.ToString());
        }
    }
}

public class RecoverEffect : ISkillEffect
{
    public int BaseRecover { get; set; }
    public float CharacterScaling { get; set; }
    public float ItemScaling { get; set; }

    public void Apply(CombatEventDispatcher resolver, Unit user, Unit target, CombatEventLog log)
    {
        int recover = BaseRecover + Mathf.FloorToInt(user.Status.CharacterLevel * CharacterScaling) + Mathf.FloorToInt(user.Status.ItemLevel * ItemScaling);
        resolver.RecoverLife(user, target, recover, false);

        if (log != null)
        {
            log.LogValues.Add("healed", recover.ToString());
        }
    }
}

public class ShieldEffect : ISkillEffect
{
    public enum ShieldType
    {
        GrantArmor,
        GrantAbsorb,
    }

    public ShieldType Type { get; set; }
    public int BaseShield { get; set; }
    public float CharacterScaling { get; set; }
    public float ItemScaling { get; set; }

    public void Apply(CombatEventDispatcher resolver, Unit user, Unit target, CombatEventLog log)
    {
        int shield = BaseShield + Mathf.FloorToInt(user.Status.CharacterLevel * CharacterScaling) + Mathf.FloorToInt(user.Status.ItemLevel * ItemScaling);
        if (Type == ShieldType.GrantArmor)
        {
            target.Status.Armor += shield;
        }
        else
        {
            target.Status.Absorb += shield;
        }

        if (log != null)
        {
            log.LogValues.Add("shielded", shield.ToString());
        }
    }
}
