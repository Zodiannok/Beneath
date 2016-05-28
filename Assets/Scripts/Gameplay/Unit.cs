using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitProfile
{
    public string Name { get; set; }
}

public class UnitStatus
{
    public int CharacterLevel { get; set; }
    public int ItemLevel { get; set; }
    public int Life { get; set; }
    public int MaxLife { get; set; }

    // Both armor and absorb are in-combat only damage mitigation tools.

    // Armor reduces physical damages taken (not consumed).
    public int Armor { get; set; }

    // Absorb reduces all damages taken (consumed).
    public int Absorb { get; set; }
}

public class Unit
{
    public UnitDefinition Definition { get; private set; }

    public UnitProfile Profile { get; private set; }
    public UnitStatus Status { get; private set; }

    public List<Skill> Skills { get; private set; }

    public Party JoinedParty { get; internal set; }

    public Unit()
    {
        Definition = null;

        Profile = new UnitProfile();

        Status = new UnitStatus();
        Status.CharacterLevel = 1;
        Status.ItemLevel = 0;
        Status.Life = 1;
        Status.MaxLife = 1;

        Skills = new List<Skill>();
        JoinedParty = null;
    }

    public Unit(UnitDefinition definition, SkillLibrary skillLib)
    {
        Definition = definition;

        Profile = new UnitProfile();
        Profile.Name = definition.DisplayedName;

        Status = new UnitStatus();
        SetLevelFullStats(1, 0);

        Skills = new List<Skill>();
        if (definition.InnateSkills != null)
        {
            foreach (string skillName in definition.InnateSkills)
            {
                skillLib.LearnSkill(this, skillName);
            }
        }

        JoinedParty = null;
    }

    private void SetLevelFullStats(int characterLevel, int itemLevel)
    {
        Status.CharacterLevel = characterLevel;
        Status.ItemLevel = itemLevel;

        Status.MaxLife = Mathf.FloorToInt(Definition.BaseLife + characterLevel * Definition.LifePerLevel);
        Status.Life = Status.MaxLife;
    }

    public bool HasSkill(Skill skill)
    {
        return Skills.Contains(skill);
    }

    public bool HasSkill(string skillInternalName)
    {
        return GetSkill(skillInternalName) != null;
    }

    public Skill GetSkill(string skillInternalName)
    {
        return Skills.Find((Skill skill) => skill.InternalName == skillInternalName);
    }

    public bool IsDead
    {
        get
        {
            return Status.Life == 0;
        }
    }
}
