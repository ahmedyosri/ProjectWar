using UnityEngine;
using System.Collections;

public class AILeader : StateMachine {

    BTRoot m_BehaviorTree;
    public GameObject[] m_Units;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        switch (m_State)
        {
            case 0:
                InitTree();
                SetupLeadership();
                
                m_State++;
                break;

            case 1:
                m_BehaviorTree.Run();
                break;
        }
	}

    void SetupLeadership()
    {
        for (int i = 0; i < m_Units.Length; i++)
        {
            m_Units[i].SendMessage("SetLeader", gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }

    void InitTree()
    {
        Time.timeScale = 4;
        m_BehaviorTree = 
            new BTRoot(
                new BTSequence(
                    new BTCommand(0),
                    new BTWait(300.0f),
                    new BTCommand(1),
                    new BTWait(60.0f),
                    new BTDebugger("waiting", true)
                )
            );


        m_BehaviorTree.SetupTree(gameObject);
        m_BehaviorTree.RunTree();
    }
}

class BTCommand : BTTask
{
    int m_Cmd;

    public BTCommand(int x)
    {
        m_Cmd = x;
    }

    public override BT_Status Run()
    {
        switch (m_Cmd)
        {
            case 0:
                m_Entity.GetComponent<AILeader>().m_Units[6].GetComponent<ArmyUnit>().CommandTo(new Command(CommandType.TacticalPosition, new Vector3(480, 1, 380)));
                break;

            case 1:
                m_Entity.GetComponent<AILeader>().m_Units[6].GetComponent<ArmyUnit>().CommandTo(new Command(CommandType.DefendPosition, new Vector3(220, 1, 430)));
                break;
        }
        return BT_Status.SUCCEEDED;
    }
}

