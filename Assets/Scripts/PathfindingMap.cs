using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfindingMap : StateMachine {

    public Terrain m_Terrain;

    Color32[] m_Bitmap, tmpBitmap ;
    Vector3 tmpPos;
    int m_BitmapLength;
    List<Vector3> tmpPath;

	// Use this for initialization
	void Start () {
        m_Bitmap = new Color32[InfMapsMgr.inflMapTiles];
        tmpPos = Vector3.up;
        m_BitmapLength = InfMapsMgr.wrldMapLength / InfMapsMgr.wrldTileSize;
	}
	
	// Update is called once per frame
	void Update () {
        switch (m_State)
        {
            case 0:
                for (int y = 0; y < m_BitmapLength; y++)
                {
                    for (int x =  0; x < m_BitmapLength; x ++)
                    {
                        tmpPos.x = x*InfMapsMgr.wrldTileSize + InfMapsMgr.wrldTileSize*0.5f;
                        tmpPos.z = y*InfMapsMgr.wrldTileSize + InfMapsMgr.wrldTileSize*0.5f;
                        m_Bitmap[(y * m_BitmapLength + x)] = (m_Terrain.SampleHeight(tmpPos) > 0) ? Color.black : Color.white;
                    }
                }
                m_State++;
                break;

            case 1:
                    {
                        tmpBitmap = new Color32[m_Bitmap.Length];
                        for (int i = 0; i < m_Bitmap.Length; i++)
                            tmpBitmap[i] = m_Bitmap[i];
                        for (int i = 0; i < m_Bitmap.Length; i++)
                        {
                            if (m_Bitmap[i] == Color.black)
                            {
                                for(int I=-1 ; I<=1 ; I++)
                                    for(int J=-1 ; J<=1 ; J++)
                                        try { tmpBitmap[i + I * m_BitmapLength + J] = Color.black; }
                                        catch { }
                            }
                        }
                        GetComponent<Pathfinding>().SetPathFindingBitmap(tmpBitmap);
                        m_State++;
                    }
                    break;

            case 2:
                break;

            //case 3:
            //    tmpPath = GetComponent<Pathfinding>().GetResultPath();
            //    if (tmpPath != null)
            //        m_State++;
            //    break;

            case 4:
                foreach (Vector3 v in tmpPath)
                {
                    //Vector2 bitmapPos;
                    //bitmapPos.x = (int)((v.x / InfluenceMaps.wrldMapLength) * InfluenceMaps.inflMapLength);
                    //bitmapPos.y = (int)((v.z / InfluenceMaps.wrldMapLength) * InfluenceMaps.inflMapLength);
                    m_Bitmap[InfMapsMgr.WorldToIdx(v)] = Color.red;
                }
                //GameObject.Find("GridVisualizer").SendMessage("SetColor", m_Bitmap);
                m_Timer = Time.time + 3000;
                m_State++;
                break;

            case 5:
                if (Time.time > m_Timer)
                {
                    m_State = 0;
                }
                break;
        }
	}

    public Color32[] GetBitmap()
    {
        if (m_State > 1)
            return m_Bitmap;
        else
            return null;
    }

    //public List<Vector3> GetPath(Vector3 from, Vector3 to)
    //{
    //    GetComponent<Pathfinding>().GetPath(from, to);
    //    m_State = 3;
    //    return tmpPath;
    //}

    public void Visualize(List<Vector3> visualizedPath)
    {
        print("called");
        tmpPath = visualizedPath;
        m_State = 4;
    }
}