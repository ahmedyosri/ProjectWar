using UnityEngine;
using System.Collections;

public class PlayerUnit : StateMachine {

    BTRoot m_BehaviorTree;
    BTTask m_EngagmentBranch;

	// Use this for initialization
	void Start () {
        InitTree();
	}
	
	// Update is called once per frame
	void Update () {
        m_BehaviorTree.Run();
	}

    void InitTree()
    {
        #region engagment
        m_EngagmentBranch =
                new BTSelector(
                        new BTSequence(
                            new SelectEnemy(30),
                            new BTNot(new IsEnemyRetreating()),
                            new HaveEnoughPower(),
                            new RegisterCombatGroup(),
                            new BTDebugger("attacking", true),
                            new BTUntilFail(
                                new BTSequence(
                                    new BTNot(new IsEnemyRetreating()),
                                    new HaveEnoughPower(),
                                    new Attack()
                                )
                            )
                        ),
                        new BTParallel(
                            new BTUntilFail(
                                new BTSequence(
                                    new IsEnemyRetreating(),
                                    //new BTDebugger("Enemy is retreating, going to chase him", true),
                                    new Chase(),
                                    new BTDebugger("Chasing", true),
                                    new BTWait(1f)
                                )
                            ),
                            new BTNot(new BTUntilSucceed(
                                new BTSelector(
                                    new BTNot(new BTWait(3f)),
                                    new IsUnderAttack(),
                                    //new BTDebugger("under attack = false", false),
                                    new IsEnemyAway()
                                    //new BTDebugger("Is Enemy Away = false", false)
                                )
                            )),
                            new BTNot(new BTWait(10f))
                        ),

                        new BTSequence(
                            new BTUntilSucceed(
                                new BTSelector(
                                    new BTSequence(new IsEnemyDefeated()),
                                    new BTSequence(
                                        new HaveEnoughGrpPwr(),
                                        //new BTDebugger("defending", true),
                                        new Defend(),
                                        new BTWait(1f),
                                        new BTNot(new IsUnderAttack())
                                    ),
                                    new BTSequence(
                                        //new BTDebugger("retreating", true),
                                        //new BTDebugger("selecting new point", true),
                                        new SelectRetreatingPoint(),
                                        //new BTDebugger("retreating point selected", true),
                                        new Navigate(NavigationStatus.Escaping),
                                        new BTWait(1f),
                                        new NoUnitIsChasingMe()
                                        //new BTDebugger("chance happened !!! fading", true)
                                    )
                                )
                            ),
                            //new BTDebugger("Calling for unregistering", true),
                            new UnRegisterCombatGroup(),
                            new BTUntilFail(new IDLE())
                        )
                    );
        #endregion

        m_BehaviorTree = new BTRoot(
                    new BTParallel(
                        new BTUntilFail(
                            new BTSelector(
                                new BTSequence(
                                    //new BTDebugger("1", true),
                                    new IsNavigating(),
                                    new BTDebugger("2", true),
                                    new BTSelector(
                                        new BTSequence(
                                            new IsUnderAttack(),
                                            new Navigate(NavigationStatus.Defending)
                                        ),
                                        //new BTDebugger("test", false),
                                        new Navigate(NavigationStatus.Normally)
                                    )
                                ),
                                new BTSelector(
                                    //new BTDebugger("3", false),
                                    new BTSequence(
                                        new IsCommandedTo(CommandType.TacticalPosition),
                                        new BTSelector(
                                            new BTSequence(
                                                new IsEnemyInAttackingRange(),
                                                new Attack()
                                            ),
                                            new BTDebugger("failed", false),
                                            new IDLE()
                                        )

                                    ),
                                    //new BTDebugger("4", false),
                                    new BTSequence(
                                        new IsCommandedTo(CommandType.DefendPosition),
                                        new Defend()
                                    ),
                                    //new BTDebugger("5", false),
                                    new BTSequence(
                                        new IsCommandedTo(CommandType.Attack),
                                        new IsEnemyInAttackingRange(),
                                        m_EngagmentBranch
                                    )
                                )
                            )
                        ),
                        new BTUntilFail(new BTSequence(
                            new ExecuteCommand(),
                            new UpdateUnderAttackState(),
                            new SelectEnemy(30)
                        ))
                    )
            );

        m_BehaviorTree.SetupTree(gameObject);
        m_BehaviorTree.RunTree();
    }
}
