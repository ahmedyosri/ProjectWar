using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NavigationStatus
{
    ReachedDestination,
    Normally,
    Defending,
    Escaping
};

public enum UnitStatus
{
    IDLE,
    Attacking,
    Defending,
    Chasing,
    Retreating,
    Defeated
};

public enum CommandType
{
    TacticalPosition,
    DefendPosition,
    Attack
};

public class Command
{
    public CommandType cmd;
    public object cmdParam;

    public Command(CommandType ct, object param)
    {
        cmd = ct;
        cmdParam = param;
    }
}

public class ArmyUnit : StateMachine {

    public int armyId, untiWidth, unitLength, unitPower, unitAttackTimeout, lowDefensivePower, highDefensivePower, unitAttackingRange;
    public float unitAttackPower;
    public float unitSpeed, unitBaseSpeed = 10, unitHighSpeedFactor = 1.1f, unitLowSpeedFactor = 0.8f;
    public TextMesh m_PowerText;

    const float MAX_DEFENSE_PWR = 100f,
                MAX_ATTACK_PWR = 100f,
                DESTINATION_RADIUS = 1f;

    
    Transform       tmpTransform;
    Pathfinding     pathfinder;
    List<Vector3>   currPath;
    int             currPathPoint;

    Vector3 targetPosition;
    public
    GameObject targetUnit ;//{ get; set; }
    public
    GameObject tmpTargetUnit { get; set; }
    Vector3 targetCmdPosition;

    public
    UnitStatus m_UnitStatus { get; set; }
    public
    NavigationStatus m_NavigationStatus { get; set; }
    public
    bool m_isUnderAttack { get; set; }
    public Command currCmd { get; set; }


	// Use this for initialization
	void Start () {
        m_PowerText.text = unitPower.ToString();
        pathfinder = new Pathfinding();
        currCmd = null;
        currPath = new List<Vector3>();
        currPathPoint = -1;
        //tmpTransform = new GameObject().transform;
	}
	
	// Update is called once per frame
    void FixedUpdate(){
        m_PowerText.text = unitPower.ToString();

        switch (m_State)
        {
            case 0:
                //print(name + ": " + m_NavigationStatus);
                //IDLE
                break;

            case 1:
                currPath = pathfinder.GetResultPath();
                if (currPath != null)
                {
                    UpdatePathTarget();

                    GameObject.Find("PathfindingGrid").SendMessage("Visualize", currPath);
                    for (int i = 0; i < currPath.Count; i++) //Make sure the path points are on the correct y
                    {
                        Vector3 tmppos = currPath[i];
                        tmppos.y = transform.position.y;
                    }
                    m_State++;
                }
                else
                    print("still");
                break;

            case 2: //NAVIGATING
                if (!UpdateUnitPosition())
                {
                    Stop();
                    m_State = 0;
                }
                break;


            case 3:
                if (!ReachedTargetPosition())
                {

                    MoveTowards(targetPosition);
                    //print(name + ": Movingg");
                }
                else
                {
                    Stop();
                    m_State = 0;
                    //print(name + ": Not moving anymore");
                }
                break;
        }

	}

    void GoTo(Vector3 position)
    {
        m_State = 1;
        Vector3 tmpFrom, tmpTo;
        tmpFrom = transform.position; tmpFrom.y = 0;
        tmpTo = position;   tmpTo.y = 0;
        pathfinder.GetPath(tmpFrom, tmpTo);
        currPathPoint = -1;
    }

    void GoAheadTo(Vector3 position)
    {
        targetPosition = position;
        m_State = 3;
    }

    public void NavigateTo(Vector3 position, bool goAhead = false)
    {
        position.y = transform.position.y;
        print(name + ": commanded from " + transform.position + " to " + position);
        if (goAhead)
            GoAheadTo(position);
        else
        {
            GoTo(position);
            m_NavigationStatus = NavigationStatus.Normally;
        }
    }

    void MoveTowards(Vector3 position)
    {
        transform.LookAt(position);
        GetComponent<Rigidbody>().velocity = transform.forward * unitSpeed;
    }

    void Stop()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_NavigationStatus = NavigationStatus.ReachedDestination;
    }

    bool UpdateUnitPosition()
    {
        if(ReachedTargetPosition()){
            if (!UpdatePathTarget())
                return false;
        }

        MoveTowards(targetPosition);
        return true;
    }

    bool UpdatePathTarget()
    {
        currPathPoint++;
        if (currPathPoint == currPath.Count)
            return false;
        targetPosition = currPath[currPathPoint];
        return true;

    }

    bool ReachedTargetPosition()
    {
        //print(name+": distance is " + Vector3.Distance(transform.position, targetPosition));
        return Vector3.Distance(transform.position, targetPosition) < DESTINATION_RADIUS;
    }

    public void ReceiveAttack(float attackPower)
    {
        switch(m_UnitStatus){
            case UnitStatus.Attacking:
            case UnitStatus.Chasing:
            case UnitStatus.Retreating:
                unitPower -= (int)(attackPower * (MAX_DEFENSE_PWR - lowDefensivePower) / MAX_DEFENSE_PWR);
                break;

            default:
                unitPower -= (int)(attackPower * (MAX_DEFENSE_PWR - highDefensivePower) / MAX_DEFENSE_PWR);
                break;
        }
    }

    //==================================================

    public void CommandTo(Command cmd)
    {
        if (cmd == currCmd)
            return;
        currCmd = cmd;
    }
}
