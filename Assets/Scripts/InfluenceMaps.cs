using UnityEngine;
using System.Collections;

public class InfluenceMaps : StateMachine {

    public const int inflMapTiles = 3600,
                            wrldMapLength = 600,
                            inflMapLength = 60,
                            wrldTileSize = 10;

    public static GameObject[] m_ArmiesUnits;

    GameObject pathfindingGrid, tacticalGrid, visualizingGrid;
    Color32[] tmpBitmap;
    
	// Use this for initialization
	void Start () {
        tacticalGrid = transform.GetChild(0).gameObject;
        pathfindingGrid = transform.GetChild(1).gameObject;
        visualizingGrid = transform.GetChild(2).gameObject;

        m_ArmiesUnits = GameObject.FindGameObjectsWithTag("ArmyUnit");
	}
	
	// Update is called once per frame
    void Update()
    {
        switch (m_State)
        {
            case 0:
            {
                tmpBitmap = pathfindingGrid.GetComponent<PathfindingMap>().GetBitmap();
                if (tmpBitmap != null)
                {
                    visualizingGrid.GetComponent<GridVisualizer>().SetColor(tmpBitmap);
                    m_Timer = Time.time + 200;
                    m_State=2;
                }
            }
            break;

            case 1:
            {
                tmpBitmap = tacticalGrid.GetComponent<TacticalGrid>().GetBitmap();
                if (tmpBitmap != null)
                {
                    visualizingGrid.GetComponent<GridVisualizer>().SetColor(tmpBitmap);
                    m_Timer = Time.time+5;
                    m_State++;
                }
            }
            break;

            case 2:
                if (Time.time > m_Timer)
                    m_State = 0;
                break;
        }
    }



    public static Vector2 WorldToArrayPos(Vector3 worldPos)
    {
        return new Vector2((int)((worldPos.x / InfluenceMaps.wrldMapLength) * InfluenceMaps.inflMapLength), (int)((worldPos.z / InfluenceMaps.wrldMapLength) * InfluenceMaps.inflMapLength));
    }

    public static Vector3 ArrayPosToWorld(Vector2 arrayPos)
    {
        Vector3 worldPos = Vector2.zero;

        worldPos.x = (arrayPos.x * wrldTileSize);
        worldPos.z = (arrayPos.y * wrldTileSize);

        return worldPos;
    }


    public static int WorldToIdx(Vector3 worldPos)
    {
        int idx = ArrayPosToIdx(WorldToArrayPos(worldPos));
        return idx;
    }

    public static Vector3 IdxToWorld(int idx)
    {
        Vector3 worldPos = ArrayPosToWorld(IdxToArrayPos(idx));
        return worldPos;
    }


    public static int ArrayPosToIdx(Vector2 arrayPos)
    {
        return (int)(arrayPos.y * InfluenceMaps.inflMapLength) + (int)arrayPos.x;
    }

    public static Vector2 IdxToArrayPos(int idx)
    {
        Vector2 arrPos = new Vector2(idx % inflMapLength, idx / (float)inflMapLength);
        return arrPos;
    }



}


public class StateMachine : MonoBehaviour
{
    protected float m_Timer = 0;
    protected int m_State = 0;
}