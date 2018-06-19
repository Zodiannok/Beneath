using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PartyPanel : MonoBehaviour {

    public GameObject UnitPanelPrefab;

    public Party AssignedParty
    {
        get
        {
            return _AssignedParty;
        }
        set
        {
            _AssignedParty = value;
            if (_AssignedParty == null)
            {
                ClearPanel(DefensePanel);
                ClearPanel(AttackPanel);
                ClearPanel(SupportPanel);
            }
            else
            {
                SetPanel(DefensePanel, PartyPosition.Defense);
                SetPanel(AttackPanel, PartyPosition.Attack);
                SetPanel(SupportPanel, PartyPosition.Support);
            }
        }
    }

    private void SetPanel(Transform panel, PartyPosition position)
    {
        ClearPanel(panel);
        Unit unit = _AssignedParty.GetAssignedUnit(position);
        if (unit != null)
        {
            GameObject unitPanel = Instantiate(UnitPanelPrefab);
            unitPanel.transform.SetParent(panel, false);
            UnitPanel panelControl = unitPanel.GetComponent<UnitPanel>();
            if (panelControl != null)
            {
                panelControl.DisplayedUnit = unit;
                panelControl.UnitPosition = position;
            }
        }
    }

    private void ClearPanel(Transform panel)
    {
        List<GameObject> children = new List<GameObject>();
        int childCount = panel.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            GameObject childObj = panel.GetChild(i).gameObject;
            children.Add(childObj);
        }
        children.ForEach(child => Destroy(child));
    }

    private Party _AssignedParty;
    private Transform DefensePanel;
    private Transform AttackPanel;
    private Transform SupportPanel;

	// Use this for initialization
	void Start ()
    {
        DefensePanel = transform.Find("PanelDef");
        AttackPanel = transform.Find("PanelAtk");
        SupportPanel = transform.Find("PanelSup");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
