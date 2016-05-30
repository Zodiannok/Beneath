using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestCombatSetup : MonoBehaviour {

    public GameObject GameLibrary;
    public GameObject PlayerPartyPanelObj;
    public GameObject EnemyPartyPanelObj;
    public GameObject CombatLogPanelObj;

    public GameObject CombatLogTextPrefab;

    internal Party PlayerParty { get; set; }
    internal Party EnemyParty { get; set; }

    internal PartyPanel PlayerPartyPanel { get; set; }
    internal PartyPanel EnemyPartyPanel { get; set; }

    internal CombatResolver Resolver { get; set; }

    internal UnitLibrary UnitLibrary { get; set; }
    internal SkillLibrary SkillLibrary { get; set; }

	// Use this for initialization
	void Start ()
    {
        UnitLibrary = GameLibrary.GetComponent<UnitLibrary>();
        SkillLibrary = GameLibrary.GetComponent<SkillLibrary>();
        PlayerPartyPanel = PlayerPartyPanelObj.GetComponent<PartyPanel>();
        EnemyPartyPanel = EnemyPartyPanelObj.GetComponent<PartyPanel>();

        CreateDefaultPlayerParty();
        CreateDefaultEnemyParty();

        Resolver = new CombatResolver(PlayerParty, EnemyParty);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void CreateDefaultPlayerParty()
    {
        PlayerParty = new Party();

        Unit PlayerDefense = UnitLibrary.CreateUnit("TestDefender");

        PlayerParty.AddPartyMember(PlayerDefense);
        PlayerParty.SetAssignment(PlayerDefense, PartyPosition.Defense);

        Unit PlayerAttack = UnitLibrary.CreateUnit("TestAttacker");

        PlayerParty.AddPartyMember(PlayerAttack);
        PlayerParty.SetAssignment(PlayerAttack, PartyPosition.Attack);

        Unit PlayerSupport = UnitLibrary.CreateUnit("TestSupporter");

        PlayerParty.AddPartyMember(PlayerSupport);
        PlayerParty.SetAssignment(PlayerSupport, PartyPosition.Support, PlayerSupport.GetSkill("ManaBolt_1"));

        PlayerPartyPanel.AssignedParty = PlayerParty;
    }

    void CreateDefaultEnemyParty()
    {
        EnemyParty = new Party();

        Unit EnemyDefense = UnitLibrary.CreateUnit("Goblin_1");
        EnemyDefense.Profile.Name = "Goblin Defense";

        EnemyParty.AddPartyMember(EnemyDefense);
        EnemyParty.SetAssignment(EnemyDefense, PartyPosition.Defense, EnemyDefense.GetSkill("ShieldUp_1"));

        Unit EnemyAttack = UnitLibrary.CreateUnit("Goblin_1");
        EnemyAttack.Profile.Name = "Goblin Attack";

        EnemyParty.AddPartyMember(EnemyAttack);
        EnemyParty.SetAssignment(EnemyAttack, PartyPosition.Attack);

        Unit EnemySupport = UnitLibrary.CreateUnit("Goblin_1");
        EnemySupport.Profile.Name = "Goblin Support";

        EnemyParty.AddPartyMember(EnemySupport);
        EnemyParty.SetAssignment(EnemySupport, PartyPosition.Support);

        EnemyPartyPanel.AssignedParty = EnemyParty;
    }

    void FullHealPlayerParty()
    {
        // Restore all HP and skill usage.
        foreach (Unit partyMember in PlayerParty.Members)
        {
            partyMember.Status.Life = partyMember.Status.MaxLife;
            foreach (Skill skill in partyMember.Skills)
            {
                skill.SkillCurrentUsage = skill.SkillMaxUsage;
            }
        }
    }

    // Called to resolve all combat events.
    public void OnResolveAll()
    {
        Resolver.GenerateAllCombatEvents();

        ScrollRect scrollRect = CombatLogPanelObj.GetComponent<ScrollRect>();
        RectTransform content = scrollRect.content;
        List<GameObject> logs = new List<GameObject>();
        int childCount = content.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            GameObject childObj = content.GetChild(i).gameObject;
            logs.Add(childObj);
        }
        logs.ForEach(child => Destroy(child));

        foreach (CombatEventLog log in Resolver.CombatEventLog)
        {
            string logFormatText = StringTable.GlobalStringTable.Get(log.Skill.SkillDefinition.LogTextFormat);
            string logText = StringTable.FormatSubstitute(logFormatText, log.LogValues);

            GameObject textObj = Instantiate(CombatLogTextPrefab);
            textObj.transform.SetParent(content, false);
            Text textComp = textObj.GetComponent<Text>();
            textComp.text = logText;
        }
    }

    // Called to resolve the next combat event.
    public void OnResolveNext()
    {

    }

    // Called to create a new enemy party and reset player party.
    public void OnNewEnemies()
    {
        FullHealPlayerParty();

        // TODO: create different parties.
        CreateDefaultEnemyParty();

        Resolver = new CombatResolver(PlayerParty, EnemyParty);
    }
}
