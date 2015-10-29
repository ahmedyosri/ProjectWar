using UnityEngine;
using System.Collections;

public class TacticalGrid : StateMachine
{

    GameObject[] m_ArmiesUnits;
    public Color[] m_ArmiesColors;

    Color32[] m_Bitmap;
    Vector3 tmpPos;
    GameObject tmpObj;
    int I, J;

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
                if (!tmpObj.active)
                    continue;
                tmpPos = m_ArmiesUnits[i].transform.GetChild(j).position;
                J = (int)((tmpPos.x/InfluenceMaps.wrldMapLength)*InfluenceMaps.inflMapLength);
                I = (int)((tmpPos.z/InfluenceMaps.wrldMapLength)*InfluenceMaps.inflMapLength);
                FillWithInfluence(I, J, ref tmpObj, m_ArmiesUnits[i].GetComponent<ArmyUnit>().armyId);
            }
        }
    }

    void FillWithInfluence(int I, int J, ref GameObject NPC, int id)
    {
        //print()
        int radius = 2;
        m_Bitmap[Idx(I, J)] += m_ArmiesColors[id];
        for (int i = I - radius; i <= I + radius; i++)
        {
            for (int j = J - radius; j <= J + radius; j++)
            {
                if ( (i==I && j==J) || i < 0 || j < 0 || i >= InfluenceMaps.inflMapLength || j >= InfluenceMaps.inflMapLength)
                    continue;
                m_Bitmap[Idx(i, j)] += m_ArmiesColors[id] / Mathf.Max(Mathf.Abs(i - I), Mathf.Abs(j - J));
                if (m_Bitmap[Idx(i, j)].r == m_Bitmap[Idx(i, j)].g && m_Bitmap[Idx(i, j)].r > 0)
                {
                    //m_Bitmap[Idx(i, j)].r = 0;
                    //m_Bitmap[Idx(i, j)].g = 0;
                    //m_Bitmap[Idx(i, j)].b = 255;
                }
            }
        }
    }

    int Idx(int I, int J)
    {
        return (I * InfluenceMaps.inflMapLength) + J;
    }

    public Color32[] GetBitmap()
    {
        if (m_State > 0)
            return m_Bitmap;
        else
            return null;
    }


}
