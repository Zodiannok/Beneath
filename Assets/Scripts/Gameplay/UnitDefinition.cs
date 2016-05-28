using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitDefinition
{
    // Unique name used to identify unit type.
    public string DefinitionName { get; set; }

    // Displayed name for players.
    public string DisplayedName { get; set; }

    #region Stats

    public float BaseLife { get; set; }
    public float LifePerLevel { get; set; }

    #endregion

    #region Skills

    // This skill list is for units that are spawned via UnitLibrary - e.g. enemies that will just spawn and stay at that level.
    // Player units would start with no skills in this list, and learn skills through classes.
    public List<string> InnateSkills { get; private set; }

    #endregion

    public UnitDefinition()
        : this("", "", false)
    {
    }

    public UnitDefinition(string defName, string displayedName, bool hasInnateSkills)
    {
        DefinitionName = defName;
        DisplayedName = displayedName;

        BaseLife = 1.0f;
        LifePerLevel = 0.0f;

        if (hasInnateSkills)
        {
            InnateSkills = new List<string>();
        }
        else
        {
            InnateSkills = null;
        }
    }
}
