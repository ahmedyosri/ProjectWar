using UnityEngine;
using System.Collections;

public class NPCUnit : StateMachine {

    GameObject leader;
    BTRoot m_BehaviorTree;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        switch (m_State)
        {
            case 0:
                //IDLE waiting for leader
                break;

            case 1:
                InitTree();
                m_BehaviorTree.RunTree();
                m_State++;
                break;

            case 2:
                //if (leader == null)
                //{
                //    m_State++;
                //    break;
                //}
                m_BehaviorTree.Run();
                break;

            case 3:
                //Match Ended
                break;
        }
	}

    void SetLeader(GameObject leaderObj)
    {
        leader = leaderObj;
        m_State = 1;
    }

    void InitTree()
    {

        m_BehaviorTree = new BTRoot(
                new BTSelector(
                        new BTSequence(
                            new SelectEnemy(30),
                            new RegisterCombatGroup(),
                            new BTUntilFail(
                                new BTSequence(
                                    new BTNot(new IsEnemyRetreating()),
                                    new HaveEnoughPower(),
                                    new BTDebugger("attacking", true),
                                    new Attack()
                                )
                            )
                        ),
                        new BTParallel(
                            new BTUntilFail(
                                new BTSequence(
                                    new IsEnemyRetreating(),
                                    new Chase(),
                                    new BTDebugger("Chasing", true),
                                    new BTWait(0.25f)
                                )
                            ),
                            new BTNot(new BTUntilSucceed(
                                new BTSelector(
                                    new IsUnderAttack(),
                                    new IsEnemyAway()
                                )
                            )),
                            new BTNot(new BTWait(3))
                        ),
                        //new BTNot(new BTUntilSucceed(
                        //    new BTSequence(
                        //        new IsEnemyRetreating(),
                        //        new BTDebugger("chasing", true),
                        //        new Chase(),
                        //        new BTSelector(
                        //            new IsUnderAttack(),
                        //            new IsEnemyAway()
                        //        ),
                        //        new UnRegisterCombatGroup()
                        //    )
                        //)),
                        new BTSelector( //Fuzzy instead
                            new BTSequence(
                                new BTDebugger("defending", true),
                                new Defend(),
                                new IsEnemyDefeated(),
                                new BTDebugger("Calling for unregistering", true),
                                new UnRegisterCombatGroup(),
                                new IDLE()
                            ),
                            new BTSequence(
                                new BTDebugger("retreating", true),
                                new SelectRetreatingPoint(),
                                new Navigate(NavigationStatus.Escaping),
                                new BTUntilSucceed(
                                    new BTSequence(
                                        new NoUnitIsChasingMe(),
                                        new Fade()
                                    )
                                )
                            )
                        )
                    )
            );
        m_BehaviorTree.SetupTree(gameObject);
        m_BehaviorTree.RunTree();
    }
}
