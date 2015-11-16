using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatGroup
{
    List<GameObject> units;
    public HashSet<int> armiesIds;
    public CombatGroup(GameObject unitA, GameObject unitB)
    {
        units = new List<GameObject>();
        armiesIds = new HashSet<int>();
        units.Add(unitA);
        units.Add(unitB);
        armiesIds.Add(unitA.GetComponent<ArmyUnit>().armyId);
        armiesIds.Add(unitB.GetComponent<ArmyUnit>().armyId);
    }
    public void AddUnit(GameObject newUnit)
    {
        units.Add(newUnit);
        armiesIds.Add(newUnit.GetComponent<ArmyUnit>().armyId);
    }
    public string RemoveUnit(GameObject removedUnit)
    {
        units.Remove(removedUnit);
        for (int i = 0; i < units.Count; i++)
            if (units[i].GetComponent<ArmyUnit>().armyId == removedUnit.GetComponent<ArmyUnit>().armyId)
            {

                return "foundddddddddd";
            }
        armiesIds.Remove(removedUnit.GetComponent<ArmyUnit>().armyId);
        return "NOOOOOOOOOOOOOOOOOOOO";
        //print("removeddddddddddddddddddddd");
    }
    public List<GameObject> Units()
    {
        return units;
    }

}

public class BattleBB : MonoBehaviour {

    int numOfArmies = 2;
    List<GameObject>[] armiesUnits;
    
    int minCombatUnits = 2;
    List<CombatGroup> combatGroups;

    private static BattleBB instance = new BattleBB();
    private static bool isInited = false;

    private BattleBB()
    {
    }

    public static BattleBB Instance
    {
        get{
            if (!isInited)
            {
                instance.InitBB();
                isInited = true;
            }
            return instance;
        }

    }

    //// Use this for initialization
    //void Start () {
    //    InitBB();
    //}

    void InitBB()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("ArmyUnit");
        armiesUnits = new List<GameObject>[numOfArmies];
        for (int i = 0; i < armiesUnits.Length; i++)
            armiesUnits[i] = new List<GameObject>();
        foreach (GameObject g in units)
        {
            armiesUnits[g.GetComponent<ArmyUnit>().armyId].Add(g);
        }

        combatGroups = new List<CombatGroup>();
    }

    public List<GameObject>[] BattleUnits()
    {
        return armiesUnits;
    }

    public void RegisterCombatGroup(GameObject unitA, GameObject unitB)
    {
        CombatGroup grp = null;
        for (int i = 0; i < combatGroups.Count && grp == null; i++)
        {
            foreach (GameObject g in combatGroups[i].Units())
            {
                if (g == unitA || g == unitB)
                {
                    grp = combatGroups[i];
                    break;
                }
            }
        }
        if (grp == null)
            combatGroups.Add(new CombatGroup(unitA, unitB));
        else
        {
            if (grp.Units().Contains(unitA) && grp.Units().Contains(unitB))
                return;
            else if (grp.Units().Contains(unitA))
                grp.Units().Add(unitB);
            else
                grp.Units().Add(unitA);
        }
    }

    public void UnRegisterCombatGroup(GameObject unitA, GameObject unitB)
    {
        for (int i = 0; i < combatGroups.Count; i++)
        {
            if (combatGroups[i].Units().Contains(unitA) && combatGroups[i].Units().Contains(unitB))
            {
                if (combatGroups[i].Units().Count == minCombatUnits)    //[Test Case] what if there were 3 units in the same group ??
                {
                    combatGroups.RemoveAt(i);
                    return;
                }
                else
                {
                    print(combatGroups[i].RemoveUnit(unitA));
                    print(combatGroups[i].RemoveUnit(unitB));
                    return;
                }
            }
        }
    }

    public CombatGroup GetCombatGroupFor(GameObject unit)
    {
        CombatGroup grp = null;
        for (int i = 0; i < combatGroups.Count && grp == null; i++)
        {
            if (combatGroups[i].Units().Contains(unit))
            {
                return combatGroups[i];
            }
        }
        return null;
    }
	
	// Update is called once per frame
	void Update () {
	    
	}
}
