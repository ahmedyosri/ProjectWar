using UnityEngine;
using System.Collections;

public class TacticalGrid : StateMachine
{

    Color32[] m_Bitmap;
    GameObject[] m_ArmiesUnits;
    public Color[] m_ArmiesColors;

    Vector3 tmpPos;
    GameObject tmpObj;

	// Use this for initialization
	void Start () {
        m_Bitmap = new Color32[InfluenceMaps.inflMapTiles];
	}
	
	// Update is called once per frame
	void Update () {
        switch (m_State)
        {
            case 0:
                m_ArmiesUnits = InfluenceMaps.m_ArmiesUnits;
                UpdateBitMap();
                m_Timer = Time.time + 3;
                m_State++;
                break;


            case 1:
                if (Time.time > m_Timer)
                {
                    m_State++;
                }
                break;

            case 2:
                UpdateBitMap();
                GameObject.Find("GridVisualizer").SendMessage("SetColor", m_Bitmap);
                m_Timer = Time.time + 3;
                m_State = 1;
                break;
        }
	}

    void UpdateBitMap()
    {
        m_Bitmap = new Color32[InfluenceMaps.inflMapTiles];
        for (int i = 0; i < m_ArmiesUnits.Length; i++)
        {
            for (int j = 0; j < m_ArmiesUnits[i].transform.childCount; j++)
            {
                tmpObj = m_ArmiesUnits[i].transform.GetChild(j).gameObject;
                if (!tmpObj.activeSelf)
                    continue;

                FillWithInfluence(tmpObj, m_ArmiesUnits[i].GetComponent<ArmyUnit>().armyId);
            }
        }
    }

    void FillWithInfluence(GameObject NPC, int id)
    {
        int radius, npcIdx, tmpIdx, I, J;
        Vector2 arrPos;

        radius = 10;
        npcIdx = InfluenceMaps.WorldToIdx(NPC.transform.position);
        arrPos = InfluenceMaps.WorldToArrayPos(NPC.transform.position);
        I = (int)arrPos.y;
        J = (int)arrPos.x;
        
        m_Bitmap[npcIdx] += m_ArmiesColors[id];
        for (int i = I - radius; i <= I + radius; i++)
        {
            for (int j = J - radius; j <= J + radius; j++)
            {
                if ( (i==I && j==J) || i < 0 || j < 0 || i >= InfluenceMaps.inflMapLength || j >= InfluenceMaps.inflMapLength)
                    continue;

                tmpIdx = InfluenceMaps.ArrayPosToIdx(Vector2.up*i + Vector2.right*j);

                m_Bitmap[tmpIdx] += m_ArmiesColors[id] / (Mathf.Abs(i - I)+Mathf.Abs(j - J));
            }
        }
    }

    public Color32[] GetBitmap()
    {
        if (m_State > 0)
            return m_Bitmap;
        else
            return null;
    }


}
