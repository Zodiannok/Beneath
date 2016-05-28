using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StringTableComponent : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        Dictionary<string, string> table = new Dictionary<string, string>();
        table["Generic_Attack_EffectLogText"] = "{user}'s {skillname} deals {damage} damage to {target}!";
        table["Generic_Heal_EffectLogText"] = "{user}'s {skillname} heals {target} for {healed} life!";
        table["Generic_Armor_EffectLogText"] = "{target} is protected with {shielded} armor!";
        table["Generic_Absorb_EffectLogText"] = "{target} is protected to absorb {shielded} damage!";

        foreach (var pair in table)
        {
            StringTable.GlobalStringTable.Add(pair.Key, pair.Value);
        }
	}
	
}
