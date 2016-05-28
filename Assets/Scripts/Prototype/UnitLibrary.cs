using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UnitLibrary : MonoBehaviour {

    private Dictionary<string, UnitDefinition> Units;
    private SkillLibrary SkillLib;

	// Use this for initialization
	void Start () {
        Units = new Dictionary<string, UnitDefinition>();
        SkillLib = GetComponent<SkillLibrary>();

        LoadPredefinedUnits();
	}

    // Update is called once per frame
    void Update () {
	
	}

    bool AddUnitDefinition(UnitDefinition definition)
    {
        if (definition == null)
        {
            return false;
        }

        if (Units.ContainsKey(definition.DefinitionName))
        {
            return false;
        }

        Units.Add(definition.DefinitionName, definition);
        return true;
    }

    UnitDefinition GetUnitDefinition(string definitionName)
    {
        UnitDefinition def = null;
        Units.TryGetValue(definitionName, out def);
        return def;
    }

    public Unit CreateUnit(string definitionName)
    {
        UnitDefinition def = GetUnitDefinition(definitionName);
        if (def == null)
        {
            return null;
        }

        return new Unit(def, SkillLib);
    }

    private void LoadPredefinedUnits()
    {
        {
            UnitDefinition def = new UnitDefinition("TestAttacker", "Village Swordsman", true);
            def.BaseLife = 20.0f;
            def.LifePerLevel = 2.0f;

            def.InnateSkills.Add("Slash_1");

            AddUnitDefinition(def);
        }

        {
            UnitDefinition def = new UnitDefinition("TestDefender", "Village Guard", true);
            def.BaseLife = 20.0f;
            def.LifePerLevel = 2.0f;

            def.InnateSkills.Add("ShieldUp_1");

            AddUnitDefinition(def);
        }

        {
            UnitDefinition def = new UnitDefinition("TestSupporter", "Village Medic", true);
            def.BaseLife = 20.0f;
            def.LifePerLevel = 2.0f;

            def.InnateSkills.Add("FirstAid_1");
            def.InnateSkills.Add("ManaBolt_1");

            AddUnitDefinition(def);
        }

        {
            UnitDefinition def = new UnitDefinition("Goblin_1", "Goblin", true);
            def.BaseLife = 5.0f;
            def.LifePerLevel = 5.0f;

            def.InnateSkills.Add("Slash_1");
            def.InnateSkills.Add("ShieldUp_1");
            def.InnateSkills.Add("FirstAid_1");

            AddUnitDefinition(def);
        }
    }
}
