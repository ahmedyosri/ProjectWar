using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public class UnitBehaviors : MonoBehaviour {
//
//    // Use this for initialization
//    void Start () {
//    }
//
//    // Update is called once per frame
//    void Update () {
//    }
//
//}

public class IsNavigating : BTTask
{
    public override BT_Status Run()
    {
        //print(m_Entity.name+": " + m_Entity.GetComponent<ArmyUnit>().m_NavigationStatus);
        return m_Entity.GetComponent<ArmyUnit>().m_NavigationStatus != NavigationStatus.ReachedDestination ? BT_Status.SUCCEEDED : BT_Status.FAILED;
    }
}

public class RedefinePathIfInterrupted : BTTask
{
    public override BT_Status Run()
    {
        throw new System.NotImplementedException();
    }
}

public class StopNavigation : BTTask
{
    public override BT_Status Run()
    {
        throw new System.NotImplementedException();
    }
}


public class ProtectedEscaping : BTTask
{
    public override BT_Status Run()
    {
        throw new System.NotImplementedException();
    }
}

public class NormalEscaping : BTTask
{
    public override BT_Status Run()
    {
        throw new System.NotImplementedException();
    }
}


//=============================================================================================

//Needs to be modified to handle more than two armies
public class SelectEnemy : BTTask
{
    float selectionRange, tmpDistance, minDistance;
    ArmyUnit myUnit;
    public SelectEnemy(int enemySelectionRange)
    {
        selectionRange = enemySelectionRange;
    }
    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }
    public override BT_Status Run()
    {
        if (myUnit.targetUnit != null)
        {
            if (myUnit.targetUnit.GetComponent<ArmyUnit>().m_UnitStatus != UnitStatus.Defeated)
            {
                return BT_Status.SUCCEEDED;
            }
            minDistance = selectionRange;

            print("sssssxxxx");

            foreach (GameObject g in BattleBB.Instance.BattleUnits()[m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().armyId])
            {
                tmpDistance = Vector3.Distance(g.transform.position, m_Entity.transform.position);
                if (tmpDistance <= minDistance)
                {
                    minDistance = tmpDistance;
                    m_Entity.GetComponent<ArmyUnit>().targetUnit = g;
                }
            }

            return BT_Status.SUCCEEDED;
        }
        else
        {
            List<GameObject>[] bunits = BattleBB.Instance.BattleUnits();
            float closestTargetDist = float.MaxValue, tmpDist;
            for (int i = 0; i < bunits.Length; i++)
            {
                if (i == myUnit.armyId)
                    continue;
                foreach (GameObject unit in bunits[i])
                {
                    tmpDist = Vector3.Distance(m_Entity.transform.position, unit.transform.position);
                    if (tmpDist < closestTargetDist && tmpDist <= myUnit.unitAttackingRange)
                    {
                        closestTargetDist = tmpDist;
                        myUnit.tmpTargetUnit = unit;
                        return BT_Status.SUCCEEDED;
                    }
                }
            }
            myUnit.tmpTargetUnit = null;
            return BT_Status.SUCCEEDED;
        }
    }
}


public class RegisterCombatGroup : BTTask
{
    public override BT_Status Run()
    {
        BattleBB.Instance.RegisterCombatGroup(m_Entity, m_Entity.GetComponent<ArmyUnit>().targetUnit);
        return BT_Status.SUCCEEDED;
    }
}

public class IsEnemyRetreating : BTTask
{
    public override BT_Status Run()
    {
        return m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().m_UnitStatus == UnitStatus.Retreating ? BT_Status.SUCCEEDED : BT_Status.FAILED;
    }
}


public class HaveEnoughPower : BTTask
{
    ArmyUnit myUnit, enemyUnit;

    public override BT_Status Run()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
        if(myUnit.targetUnit == null)
            return BT_Status.FAILED;

        enemyUnit = myUnit.targetUnit.GetComponent<ArmyUnit>();
        if(enemyUnit.m_UnitStatus == UnitStatus.Defeated)
            return BT_Status.FAILED;

        return myUnit.unitAttackPower * myUnit.unitPower >= 0.5f * enemyUnit.unitAttackPower * enemyUnit.unitPower ? BT_Status.SUCCEEDED : BT_Status.FAILED;

    }
}

//Embeded
//public class LowEnemyKillRate : BTTask
//{
//    public override BT_Status Run()
//    {
//        throw new System.NotImplementedException();
//    }
//}

public class Attack : BTTask
{
    float m_timer;
    float attackTimeout;
    ArmyUnit myUnit, enemyUnit;

    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
        if (myUnit.targetUnit != null)
            enemyUnit = m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>();
        else
            enemyUnit = null;
        m_timer = 0;// myUnit.unitAttackTimeout;
    }

    public override BT_Status Run()
    {
        if (Time.time > m_timer)
        {
            if (enemyUnit == null)
            {
                if (myUnit.targetUnit != null)
                    enemyUnit = m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>();
                else if (myUnit.tmpTargetUnit != null)
                    enemyUnit = myUnit.tmpTargetUnit.GetComponent<ArmyUnit>();
            }

            if (enemyUnit != null)
            {
                m_Entity.GetComponent<ArmyUnit>().m_UnitStatus = UnitStatus.Attacking;
                enemyUnit.ReceiveAttack(myUnit.unitAttackPower * myUnit.unitPower);
                m_timer = Time.time + myUnit.unitAttackTimeout;
            }
        }
        return BT_Status.SUCCEEDED;
    }
}

public class IsEnemyInAttackingRange : BTTask
{
    ArmyUnit myUnit;
    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }
    public override BT_Status Run()
    {
        if (myUnit.targetUnit != null)
        {
            if (Vector3.Distance(m_Entity.transform.position, myUnit.targetUnit.transform.position) <= myUnit.unitAttackingRange)
                return BT_Status.SUCCEEDED;
            return BT_Status.FAILED;
        }
        else
        {
            return myUnit.tmpTargetUnit != null ? BT_Status.SUCCEEDED : BT_Status.FAILED;
        }
    }
}

//=============================================================================================

public class Defend : BTTask
{
    public override BT_Status Run()
    {
        m_Entity.GetComponent<ArmyUnit>().m_UnitStatus = UnitStatus.Defending;
        return BT_Status.SUCCEEDED;
    }
}

public class IsEnemyDefeated : BTTask
{
    public override BT_Status Run()
    {
        if (m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().m_UnitStatus == UnitStatus.Defeated ||
                m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().m_UnitStatus == UnitStatus.Retreating)
            print(m_Entity.name + " : sa77777");
        return (m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().m_UnitStatus == UnitStatus.Defeated ||
                m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().m_UnitStatus == UnitStatus.Retreating) ?
                BT_Status.SUCCEEDED : BT_Status.FAILED;
    }
}

public class UnRegisterCombatGroup : BTTask
{
    public override BT_Status Run()
    {
        BattleBB.Instance.UnRegisterCombatGroup(m_Entity, m_Entity.GetComponent<ArmyUnit>().targetUnit);
        return BT_Status.SUCCEEDED;
    }
}

//=============================================================================================

public class SelectRetreatingPoint : BTTask
{
    Pathfinding pathfinder;
    bool[,] bitgrid;

    public SelectRetreatingPoint()
    {
        pathfinder = new Pathfinding();
        bitgrid = null; //won't initilize here to give the pathfinder some time to construct the bitmap
    }
    public override BT_Status Run()
    {

        if (bitgrid == null)
            bitgrid = pathfinder.BitmapGrid();
        if (bitgrid == null)
            return BT_Status.RUNNING;

        int srBoxLn = 2;
        float tmpDistance, maxDistance = -1;
        Vector3 rtrtPoint;
        Vector2 myArrPos, enemyArrPos, rtrtarrPoint;
        myArrPos = pathfinder.WorldToArrayPos(m_Entity.transform.position);
        enemyArrPos = pathfinder.WorldToArrayPos(m_Entity.GetComponent<ArmyUnit>().targetUnit.transform.position);

        //rtrtarrPoint = myArrPos + (myArrPos - enemyArrPos).normalized * 2;   //2 3shan yeb2a aktar mn el 1, 3shan el float lw gama3to 3al int hayeb2a 0
        int I=0, J=0;
        rtrtarrPoint = new Vector2();
        //print("===============");
        for (int i = -srBoxLn; i <= srBoxLn; i++)
        {
            for (int j = -srBoxLn; j <= srBoxLn; j++)
            {
                I = (int)(myArrPos.y + i); J = (int)(myArrPos.x + j);

                //print(j+" , " + i + " | " + J + " , " + I);
                if (!bitgrid[I, J])
                {
                    //print(" is disabled");
                    continue;
                }

                tmpDistance = Mathf.Abs(J - enemyArrPos.x) + Mathf.Abs(I - enemyArrPos.y);
                if (tmpDistance > maxDistance)
                {
                    rtrtarrPoint = myArrPos + new Vector2(j, i);
                    //print(" was selected");
                    maxDistance = tmpDistance;
                }
                else
                {
                    //print(" is ab3ad");
                }
            }
        }

        rtrtPoint = IMapsMgr.IdxToWorld(pathfinder.ArrayPosToIdx(rtrtarrPoint));
        m_Entity.GetComponent<ArmyUnit>().NavigateTo(rtrtPoint, true);
        return BT_Status.SUCCEEDED;
    }
}

public class Navigate : BTTask
{
    NavigationStatus navigationTarget;
    ArmyUnit myUnit;

    public Navigate(NavigationStatus navigationStatusTarget)
    {
        navigationTarget = navigationStatusTarget;
    }
    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }
    public override BT_Status Run()
    {
        //print(m_Entity.name + ": mn hena :" + navigationTarget);
        switch (myUnit.m_NavigationStatus)
        {
            case NavigationStatus.Normally:
                myUnit.unitSpeed = myUnit.unitBaseSpeed;
                myUnit.m_NavigationStatus = navigationTarget;
                break;
            case NavigationStatus.Escaping:
                myUnit.unitSpeed = myUnit.unitBaseSpeed * myUnit.unitHighSpeedFactor;
                myUnit.m_NavigationStatus = navigationTarget;
                myUnit.m_UnitStatus = UnitStatus.Retreating;
                break;

            case NavigationStatus.Defending:
                myUnit.m_NavigationStatus = navigationTarget;
                myUnit.unitSpeed = myUnit.unitBaseSpeed * myUnit.unitLowSpeedFactor;
                break;
            default: 
                break;
        }
        return BT_Status.SUCCEEDED;
    }
}

public class NoUnitIsChasingMe : BTTask
{
    ArmyUnit myUnit, enemyUnit;

    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }

    public override BT_Status Run()
    {

        List<GameObject>[] bunits = BattleBB.Instance.BattleUnits();
        
        for (int i = 0; i < bunits.Length; i++)
        {
            if (i == myUnit.armyId)
                continue;
            foreach (GameObject g in bunits[i])
            {
                enemyUnit = g.GetComponent<ArmyUnit>();
                if (enemyUnit.m_UnitStatus == UnitStatus.Chasing && enemyUnit.targetUnit == m_Entity)
                    return BT_Status.FAILED;
            }
        }

        return BT_Status.SUCCEEDED;

    }
}

public class Fade : BTTask
{
    public override BT_Status Run()
    {
        //lazem tUnregisterha mn kol 7aga fel system
        //m_Entity.transform.position = Vector3.one * 10000;
        return BT_Status.SUCCEEDED;
    }
}

//=============================================================================================

public class Chase : BTTask
{
    ArmyUnit myUnit;

    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }

    public override BT_Status Run()
    {
        myUnit.m_UnitStatus = UnitStatus.Chasing;
        myUnit.unitSpeed = myUnit.unitBaseSpeed * myUnit.unitLowSpeedFactor * myUnit.unitLowSpeedFactor;
        myUnit.NavigateTo(myUnit.targetUnit.transform.position, true);
        return BT_Status.SUCCEEDED;
    }
}

public class UpdateUnderAttackState : BTTask
{
    float prevHealth, checkingTimeOut, checkTimer;
    ArmyUnit myUnit;
    BT_Status lastStatus;

    public override void Init()
    {
        checkingTimeOut = 1f;
        checkTimer = -1;
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }

    public override BT_Status Run()
    {
        if (checkTimer == -1)
        {
            prevHealth = myUnit.unitPower;
            checkTimer = Time.time + checkingTimeOut;
            myUnit.m_isUnderAttack = false;
        }
        else if (Time.time > checkTimer)
        {
            if (prevHealth == myUnit.unitPower)
            {
                myUnit.m_isUnderAttack = false;
            }
            else
            {
                prevHealth = myUnit.unitPower;
                checkTimer = Time.time + checkingTimeOut;
                myUnit.m_isUnderAttack = true;
            }
        }
        return BT_Status.SUCCEEDED;
    }
}

public class IsUnderAttack : BTTask
{
    ArmyUnit myUnit;

    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }

    public override BT_Status Run()
    {

        return myUnit.m_isUnderAttack ? BT_Status.SUCCEEDED : BT_Status.FAILED;
    }
}

public class IsEnemyAway : BTTask
{
    float safeDistance = 300f;
    public override BT_Status Run()
    {
        return (Vector3.Distance(m_Entity.GetComponent<ArmyUnit>().targetUnit.transform.position, m_Entity.transform.parent.position) >= safeDistance) ?
            BT_Status.SUCCEEDED : BT_Status.FAILED;
    }
}

public class IDLE : BTTask
{
    ArmyUnit myUnit;

    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }
    public override BT_Status Run()
    {
        if (myUnit.m_UnitStatus == UnitStatus.IDLE)
            return BT_Status.SUCCEEDED;

        myUnit.m_UnitStatus = UnitStatus.IDLE;
        myUnit.m_NavigationStatus = NavigationStatus.ReachedDestination;
        myUnit.unitSpeed = myUnit.unitBaseSpeed;
        myUnit.NavigateTo(m_Entity.transform.position, true);
        return BT_Status.SUCCEEDED;
    }
}

//=============================================================================================

public class CheckEnemyStatus : BTTask
{
    UnitStatus targetUnitStatus;
    NavigationStatus targetNavStatus;
    public CheckEnemyStatus(UnitStatus unitStatus){
        targetUnitStatus = unitStatus;
    }
    public CheckEnemyStatus(NavigationStatus navStatus)
    {
        targetNavStatus = navStatus;
    }

    public override BT_Status Run()
    {

        if (targetNavStatus == null)
        {
            return m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().m_UnitStatus == targetUnitStatus ? BT_Status.SUCCEEDED : BT_Status.FAILED;
        }
        else
        {
            return m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().m_NavigationStatus == targetNavStatus ? BT_Status.SUCCEEDED : BT_Status.FAILED;
        }
    }
}

public class HaveEnoughGrpPwr : BTTask
{
    public override BT_Status Run()
    {
        CombatGroup grp = BattleBB.Instance.GetCombatGroupFor(m_Entity);

        int myId, enemyId;
        float[] grpArmyPowers = new float[grp.armiesIds.Count];
        float[] grpArmyAttackPowers = new float[grp.armiesIds.Count];

        float rankStepValue = 1f / grp.armiesIds.Count;
        int rankInGrpArmies = 0;//will be increased to 1 automatically when compared with myself

        myId = m_Entity.GetComponent<ArmyUnit>().armyId;
        enemyId = m_Entity.GetComponent<ArmyUnit>().targetUnit.GetComponent<ArmyUnit>().armyId;

        foreach (GameObject g in grp.Units())
        {
            grpArmyPowers[g.GetComponent<ArmyUnit>().armyId] += g.GetComponent<ArmyUnit>().unitPower;
            grpArmyAttackPowers[g.GetComponent<ArmyUnit>().armyId] += g.GetComponent<ArmyUnit>().unitAttackPower;
        }

        for (int i = 0; i < grp.armiesIds.Count; i++)
        {
            rankInGrpArmies += grpArmyPowers[myId] * grpArmyAttackPowers[myId] >= grpArmyPowers[i] * grpArmyAttackPowers[i] ? 1 : 0;
        }

        return rankInGrpArmies * rankStepValue > 0.5f ? BT_Status.SUCCEEDED : BT_Status.FAILED;

    }
}

public class ExecuteCommand : BTTask
{
    ArmyUnit myUnit;
    Command currCmd;

    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
        currCmd = null;
    }

    public override BT_Status Run()
    {
        if (currCmd == myUnit.currCmd)
            return BT_Status.SUCCEEDED;

        currCmd = myUnit.currCmd;

        if (currCmd.cmd == CommandType.Attack)
        {
            myUnit.targetUnit = ((GameObject)currCmd.cmdParam);
            myUnit.NavigateTo(((GameObject)currCmd.cmdParam).transform.position);
        }
        else if (currCmd.cmd == CommandType.DefendPosition || currCmd.cmd == CommandType.TacticalPosition)
            myUnit.NavigateTo((Vector3)currCmd.cmdParam);

        return BT_Status.SUCCEEDED;
    }
}

public class IsCommandedTo : BTTask{
    CommandType comparedCmd;
    ArmyUnit myUnit;
    public IsCommandedTo(CommandType cmd)
    {
        comparedCmd = cmd;
    }

    public override void Init()
    {
        myUnit = m_Entity.GetComponent<ArmyUnit>();
    }

    public override BT_Status Run()
    {
        return (myUnit.currCmd != null && myUnit.currCmd.cmd == comparedCmd) ?
                BT_Status.SUCCEEDED : BT_Status.FAILED;
    }
}