﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SkillLibrary : MonoBehaviour {

    private Dictionary<string, SkillDefinition> Skills;

	// Use this for initialization
	void Start () {
        Skills = new Dictionary<string, SkillDefinition>();

        LoadPredefinedSkills();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    bool AddSkillTemplate(SkillDefinition template)
    {
        if (template == null)
        {
            return false;
        }

        if (Skills.ContainsKey(template.InternalName))
        {
            return false;
        }

        Skills.Add(template.InternalName, template);
        return true;
    }

    SkillDefinition GetSkillTemplate(string templateName)
    {
        SkillDefinition result = null;
        Skills.TryGetValue(templateName, out result);
        return result;
    }

    // Used to create a skill from a template.
    Skill CreateSkill(string templateName)
    {
        SkillDefinition template = GetSkillTemplate(templateName);
        if (template == null)
        {
            return null;
        }

        return new Skill(template);
    }

    // Used to add a skill to a unit.
    public void LearnSkill(Unit unit, string templateName)
    {
        if (unit.HasSkill(templateName))
        {
            return;
        }

        Skill skill = CreateSkill(templateName);
        if (skill != null)
        {
            skill.Owner = unit;
            unit.Skills.Add(skill);
        }
    }

    private void LoadPredefinedSkills()
    {
        {
            SkillDefinition skill = new SkillDefinition();
            skill.InternalName = "Slash_1";
            skill.DisplayedName = "Slash I";
            skill.BaseUsage = 8;
            skill.AllowedPositions = PartyPositionFlag.Attack;
            skill.PerformedPhase = CombatPhase.AttackPhase;
            skill.SetTag(SkillTag.Melee, true);
            skill.SetTag(SkillTag.Attack, true);

            AttackEffect effect = new AttackEffect();
            effect.DamageType = DamageType.Physical;
            effect.BaseDamage = 10;
            effect.CharacterScaling = 1.0f;
            effect.ItemScaling = 1.0f;
            skill.Effect = effect;

            StandardSingleTargeting targeting = new StandardSingleTargeting();
            skill.Targeting = targeting;

            skill.LogTextFormat = "Generic_Attack_EffectLogText";

            AddSkillTemplate(skill);
        }

        {
            SkillDefinition skill = new SkillDefinition();
            skill.InternalName = "FirstAid_1";
            skill.DisplayedName = "First Aid";
            skill.BaseUsage = 8;
            skill.AllowedPositions = PartyPositionFlag.Defense | PartyPositionFlag.Support;
            skill.PerformedPhase = CombatPhase.RecoveryPhase;

            RecoverEffect effect = new RecoverEffect();
            effect.BaseRecover = 4;
            effect.CharacterScaling = 1.0f;
            effect.ItemScaling = 0.0f;
            skill.Effect = effect;

            LowestHealthTargeting targeting = new LowestHealthTargeting();
            targeting.ByPercentage = true;
            targeting.TargetAllyParty = true;
            skill.Targeting = targeting;

            skill.LogTextFormat = "Generic_Heal_EffectLogText";

            AddSkillTemplate(skill);
        }

        {
            SkillDefinition skill = new SkillDefinition();
            skill.InternalName = "ShieldUp_1";
            skill.DisplayedName = "Shield Up";
            skill.BaseUsage = 8;
            skill.AllowedPositions = PartyPositionFlag.Defense;
            skill.PerformedPhase = CombatPhase.PreparationPhase;

            ShieldEffect effect = new ShieldEffect();
            effect.Type = ShieldEffect.ShieldType.GrantArmor;
            effect.BaseShield = 6;
            effect.CharacterScaling = 0.5f;
            effect.ItemScaling = 1.0f;
            skill.Effect = effect;

            PositionTargeting targeting = new PositionTargeting();
            targeting.TargetAlly = true;
            targeting.Position = PartyPosition.Defense;
            skill.Targeting = targeting;

            skill.LogTextFormat = "Generic_Armor_EffectLogText";

            AddSkillTemplate(skill);
        }

        {
            SkillDefinition skill = new SkillDefinition();
            skill.InternalName = "ManaBolt_1";
            skill.DisplayedName = "Mana Bolt I";
            skill.BaseUsage = 8;
            skill.AllowedPositions = PartyPositionFlag.Support | PartyPositionFlag.Attack;
            skill.PerformedPhase = CombatPhase.ChannelPhase;
            skill.SetTag(SkillTag.Cast, true);
            skill.SetTag(SkillTag.Ranged, true);

            AttackEffect effect = new AttackEffect();
            effect.DamageType = DamageType.Magic;
            effect.BaseDamage = 6;
            effect.CharacterScaling = 1.0f;
            effect.ItemScaling = 0.5f;
            skill.Effect = effect;

            StandardSingleTargeting targeting = new StandardSingleTargeting();
            skill.Targeting = targeting;

            skill.LogTextFormat = "Generic_Attack_EffectLogText";

            AddSkillTemplate(skill);
        }

        {
            SkillDefinition skill = new SkillDefinition();
            skill.InternalName = "Counterspell_1";
            skill.DisplayedName = "Counterspell I";
            skill.BaseUsage = 6;
            skill.AllowedPositions = PartyPositionFlag.Support;
            skill.PerformedPhase = CombatPhase.ReactionPhase;
            skill.SetTag(SkillTag.Cast, true);
            skill.SetTag(SkillTag.Ranged, true);

            AttackEffect effect = new AttackEffect();
            effect.DamageType = DamageType.Magic;
            effect.BaseDamage = 6;
            effect.CharacterScaling = 1.0f;
            effect.ItemScaling = 0.5f;
            skill.Effect = effect;

            StandardSingleTargeting targeting = new StandardSingleTargeting();
            skill.Targeting = targeting;

            InterruptCastingTrigger triggering = new InterruptCastingTrigger();
            skill.Triggering = triggering;

            skill.LogTextFormat = "Generic_Attack_EffectLogText";

            AddSkillTemplate(skill);
        }
    }

}
