using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum BT_Status{
    FAILED,
    SUCCEEDED,
    RUNNING
};

public abstract class BTTask : MonoBehaviour
{

    protected   BT_Status    m_Status;
    protected   GameObject   m_Entity;
    protected   int          m_itr;
    protected   bool         m_pInterruptionFlag, m_pIsTreeInterruptable;
    protected   List<BTTask> m_Children;

    protected   void         FillChildren(params BTTask[] childs){
        for(int i=0 ; i<childs.Length ; i++){
            m_Children.Add(childs[i]);
        }
    }


    public BTTask()
    {
        m_Status = BT_Status.RUNNING;
        m_Children = new List<BTTask>();
    }

    public virtual void ResetNode() { }

    public abstract BT_Status Run();
    public virtual void Init() { }
    public virtual void ResetEveryNode(){
        for(int i=0 ; i<m_Children.Count ; i++)
            m_Children[0].ResetEveryNode();
        ResetNode();
    }

    public void SetData(ref GameObject targetEntity, ref bool interruptionFlag, ref bool isTreeInterruptable){
        m_Entity = targetEntity;
        m_pInterruptionFlag = interruptionFlag;
        m_pIsTreeInterruptable = isTreeInterruptable;
        
        for(int i=0 ; i<m_Children.Count ; i++){
            m_Children[i].SetData(ref targetEntity, ref interruptionFlag, ref isTreeInterruptable);
            ResetNode();
        }
        Init();
    }

}

public class BTRoot : BTTask{
    private bool    m_isTreeRunning, m_interruptionFlag, m_isTreeInterruptable;


    public          BTRoot(BTTask initTask){
        m_Children.Add(initTask);
    }

    public  void    SetupTree(GameObject targetEntity){
        m_Children[0].SetData(ref targetEntity, ref m_interruptionFlag, ref m_isTreeInterruptable);
        m_isTreeRunning = true;
    }

    public  void    StopTree(){
        m_isTreeRunning = false;
    }

    public  void    RunTree(){
        m_isTreeRunning = true;
    }

    public override BT_Status Run()
    {
        if(m_interruptionFlag){
            ResetEveryNode();
            m_interruptionFlag = false;
        }

        if(m_isTreeRunning){
            m_Status = m_Children[0].Run();
            if(m_Status != BT_Status.RUNNING)
                ResetEveryNode();
        }
        else{
            m_Status = BT_Status.FAILED;
        }

        return m_Status;
    }

}

public class BTSequence : BTTask
{
    public  BTSequence(params BTTask[] childs){
        FillChildren(childs);
    }

    public override BT_Status Run()
    {
        m_Status = m_Children[m_itr].Run();
        if(m_Status == BT_Status.FAILED){
            ResetNode();
            return BT_Status.FAILED;
        }
        else if(m_Status == BT_Status.SUCCEEDED){
            m_itr = (m_itr+1)%m_Children.Count;
            return m_itr == 0 ? BT_Status.SUCCEEDED : BT_Status.RUNNING;
        }
        else
            return BT_Status.RUNNING;
    }

    public override void ResetNode()
    {
        m_itr = 0;
        m_Status = BT_Status.RUNNING;
    }
}

public class BTSelector : BTTask
{
    public  BTSelector(params BTTask[] childs){
        FillChildren(childs);
        m_itr = 0;
    }

    public override BT_Status Run()
    {
        m_Status = m_Children[m_itr].Run();
        if(m_Status == BT_Status.FAILED){
            m_itr = (m_itr+1)%m_Children.Count;
            return m_itr == 0 ? BT_Status.FAILED : BT_Status.RUNNING;
        }
        else if (m_Status == BT_Status.SUCCEEDED)
        {
            ResetNode();
            return BT_Status.SUCCEEDED;
        }
        else{
            return BT_Status.RUNNING;
        }
    }

    public override void ResetNode()
    {
        m_itr = 0;
        m_Status = BT_Status.RUNNING;
    }
}

public class BTParallel : BTTask
{
    int cSucceded, cFailed;
    List<BT_Status>   m_ChildsStatus;


    public BTParallel(params BTTask[] childs){
        m_ChildsStatus = new List<BT_Status>();
        FillChildren(childs);
        for(int i=0 ; i<childs.Length ; i++)
            m_ChildsStatus.Add(BT_Status.RUNNING);
    }

    public override void ResetNode()
    {
        cSucceded = 0;
        cFailed = 0;
        for(int i=0 ; i<m_Children.Count ; i++)
            m_ChildsStatus[i] = BT_Status.RUNNING;
    }

    public override BT_Status Run()
    {
        cSucceded = 0;
        cFailed   = 0;
        for(int i=0 ; i<m_Children.Count ; i++){

            if(m_ChildsStatus[i] != BT_Status.SUCCEEDED)
                m_ChildsStatus[i] = m_Children[i].Run();

            cSucceded   += (m_ChildsStatus[i] == BT_Status.SUCCEEDED ? 1 : 0);
            cFailed     += (m_ChildsStatus[i] == BT_Status.FAILED ? 1 : 0);
        }

        if(cFailed > 0){
            ResetEveryNode();
            return BT_Status.FAILED;
        }
        else if(cSucceded == m_Children.Count)
        {
            ResetEveryNode();
            return BT_Status.SUCCEEDED;
        }
        else
        {
            return BT_Status.RUNNING;
        }
    }
}

class BTUntilFail : BTTask{

    public  BTUntilFail(BTTask childTask){
        m_Children.Clear();
        m_Children.Add(childTask);
    }

    public override BT_Status Run()
    {
        m_Status = m_Children[0].Run();
        while(m_Status != BT_Status.FAILED){
            return BT_Status.RUNNING;
        }
        return BT_Status.FAILED;
    }
}

class BTUntilSucceed : BTTask{

    public  BTUntilSucceed(BTTask childTask){
        m_Children.Clear();
        m_Children.Add(childTask);
    }

    public override BT_Status Run()
    {
        m_Status = m_Children[0].Run();
        if(m_Status != BT_Status.SUCCEEDED){
            return BT_Status.RUNNING;
        }
        return BT_Status.SUCCEEDED;
    }
}

class BTNot : BTTask{

    public BTNot(BTTask childTask){
        m_Children.Clear();
        m_Children.Add(childTask);
    }

    public override BT_Status Run()
    {
        m_Status = m_Children[0].Run();
        
        if(m_Status == BT_Status.SUCCEEDED)
            return BT_Status.FAILED;
        else if(m_Status == BT_Status.FAILED)
            return BT_Status.SUCCEEDED;
        else
            return BT_Status.RUNNING;
    }

}

class BTWait : BTTask{
    float m_Timeout, m_TimeToWait;
    public BTWait(float timeToWait){
        m_TimeToWait = timeToWait;
        ResetNode();
    }
    public override BT_Status Run()
    {
        if(m_Timeout == 0){
            m_Timeout = Time.time + m_TimeToWait;
        }
        
        if(Time.time < m_Timeout)
            return BT_Status.RUNNING;
        else{
            ResetNode();
            return BT_Status.SUCCEEDED;
        }
    }

    public override void ResetNode()
    {
        m_Timeout = 0;
    }
}

class BTDebugger : BTTask{
    string  m_msg;
    BT_Status m_returnValue;

    public BTDebugger(string msg, bool returnValue){
        m_msg = msg;
        m_returnValue = returnValue ? BT_Status.SUCCEEDED : BT_Status.FAILED;
    }

    public override BT_Status Run()
    {
        //print(m_Entity.name + ": " + m_msg);
        return m_returnValue;
    }
}

public abstract class BTFuzzyAction : BTTask
{
    protected float m_CurrScore;
    bool isScoreInverted;
    BT_Status runningResult;
    public float GetScore() { 
        m_CurrScore = Mathf.Clamp(m_CurrScore, 0f, 1f);
        return isScoreInverted ? 1 - m_CurrScore : m_CurrScore;
    }
    public abstract void UpdateScore();

    public BTFuzzyAction(BTTask child, bool scoreInverted)
    {
        FillChildren(child);
        isScoreInverted = scoreInverted;
    }
    public override sealed BT_Status Run()
    {
        runningResult = m_Children[0].Run();
        return runningResult;
    }

}

class BTFuzzySet : BTTask
{
    int m_State, maxAction, currIdx;
    float tmpScore, maxScore;
    List<Vector2> scores;


    public BTFuzzySet(params BTFuzzyAction[] childs)
    {
        FillChildren(childs);
        m_State = 0;
        scores = new List<Vector2>();
        for (int i = 0; i < childs.Length; i++)
            scores.Add(new Vector2(i, 0));
        currIdx = 0;
        maxScore = int.MinValue;
    }

    public override BT_Status Run()
    {
        switch (m_State) { 
            case 0:
            {
                maxScore = int.MinValue;
                for (int i = 0; i < m_Children.Count; i++)
                {
                    ((BTFuzzyAction)m_Children[i]).UpdateScore();
                    scores[i] = new Vector2(i, ((BTFuzzyAction)m_Children[i]).GetScore());
            
                    if (scores[i].y > maxScore)
                    {
                        maxScore = scores[i].y;
                        maxAction = i;
                    }
                }
                scores.OrderBy(v => v.y);
                currIdx = 0;
                m_State++;
            }
            return BT_Status.RUNNING;

            case 1:
            {
                BT_Status result = m_Children[(int)scores[currIdx].x].Run();
                if (result == BT_Status.FAILED)
                {
                    currIdx = (currIdx + 1) % m_Children.Count;
                    if (currIdx == 0)
                    {
                        ResetNode();
                        return BT_Status.FAILED;
                    }
                    return BT_Status.RUNNING;
                }
                if (result == BT_Status.SUCCEEDED)
                {
                    ResetNode();
                    print(m_Entity.name + " : Succeded from action #" + currIdx);
                }
                return result;
            }

            default:

            return BT_Status.RUNNING;
        }

    }
    public override void ResetNode()
    {
        currIdx = 0;
        maxScore = int.MinValue;
    }
}

class BTCheckUntil : BTTask
{
    float timer, checkingTimeout;
    BT_Status lastResult;

    public BTCheckUntil(BTTask child, float CheckingTimeout)
    {
        FillChildren(child);
        checkingTimeout = CheckingTimeout;
    }

    public override void Init()
    {
        timer = -1;
    }

    public override BT_Status Run()
    {
        if (timer == -1)
        {
            timer = Time.time + checkingTimeout;
            return BT_Status.RUNNING;
        }

        if (Time.time < timer)
        {
            lastResult = m_Children[0].Run();
            if (lastResult == BT_Status.SUCCEEDED)
            {
                timer = -1;
                return BT_Status.SUCCEEDED;
            }
            else
            {
                return BT_Status.RUNNING;
            }
        }
        lastResult = m_Children[0].Run();
        if (lastResult != BT_Status.RUNNING)
            timer = -1;
        return lastResult;
    }
}