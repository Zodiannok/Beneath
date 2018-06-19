using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnitPanel : MonoBehaviour {

    public Unit DisplayedUnit { get; set; }
    public PartyPosition UnitPosition { get; set; }

    private Text UnitNameText;
    private Text UnitSkillText;
    private Text UnitSkillUsageText;
    private Text UnitHPText;
    private Scrollbar UnitHPBar;

	// Use this for initialization
	void Start ()
    {
        Transform temp;

        temp = transform.Find("TextName");
        if (temp)
        {
            UnitNameText = temp.GetComponent<Text>();
        }

        temp = transform.Find("TextSkill");
        if (temp)
        {
            UnitSkillText = temp.GetComponent<Text>();
        }

        temp = transform.Find("TextSkillUsage");
        if (temp)
        {
            UnitSkillUsageText = temp.GetComponent<Text>();
        }

        temp = transform.Find("TextHP");
        if (temp)
        {
            UnitHPText = temp.GetComponent<Text>();
        }

        temp = transform.Find("HPBar");
        if (temp)
        {
            UnitHPBar = temp.GetComponent<Scrollbar>();
        }
    }

    // Update is called once per frame
    void Update ()
    {
	    if (DisplayedUnit == null)
        {
            return;
        }

        UnitNameText.text = DisplayedUnit.Profile.Name;
        int life = DisplayedUnit.Status.Life;
        int maxLife = DisplayedUnit.Status.MaxLife;
        float lifePercentage = (float)life / (float)maxLife;
        UnitHPText.text = string.Format("{0} / {1}", life, maxLife);
        UnitHPBar.size = lifePercentage;

        Party unitParty = DisplayedUnit.JoinedParty;
        if (unitParty != null)
        {
            Skill assignedSkill = unitParty.GetAssignedSkill(UnitPosition);
            UnitSkillText.text = assignedSkill.SkillDefinition.DisplayedName;
            UnitSkillUsageText.text = string.Format("{0} / {1}", assignedSkill.SkillCurrentUsage, assignedSkill.SkillMaxUsage);
        }
	}
}
