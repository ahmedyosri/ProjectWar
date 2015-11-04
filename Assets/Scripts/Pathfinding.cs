using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

    static Color32[] m_bitmap; //should be a squared bitmap
    static bool[,] m_pathfindinGrid;
    static bool[] m_pathfindingGridFlatted;
    static AStar pathfinder = new AStar();
    static bool isInited;

    int m_MipMapLevel = 1;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public
    void SetPathFindingBitmap(Color32[] pathfindingBitmap)
    {
        isInited = true;
        m_bitmap = pathfindingBitmap;
        UpdateDataFromBitmap();
    }

    void UpdateDataFromBitmap()
    {
        int gridLength = 60;// (int)Mathf.Sqrt(m_bitmap.Length);
        m_pathfindinGrid = new bool[gridLength, gridLength];

        for (int y = 0, I=0; y < gridLength; y++)
        {
            for (int x = 0; x < gridLength; x++, I++)
            {
                m_pathfindinGrid[x, y] = (m_bitmap[I] == Color.white) ? true : false;
                pathfinder.AddNode(new Node(I));
            }
        }

        //GameObject edge, cube = GameObject.Find("Cube"), cell = GameObject.Find("Cell");
        //edge = new GameObject();
        //edge.AddComponent<LineRenderer>();
        //edge.GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
        //edge.GetComponent<LineRenderer>().SetPosition(1, Vector3.one * 200);


        for (int y = 0, I = 0; y < gridLength - m_MipMapLevel; y+=m_MipMapLevel, I+=(m_MipMapLevel+gridLength*(m_MipMapLevel-1)))   //I+=skipped here too, la2eny bafawet wa7da mn ta7t f cumulative kanet bte3mel moshkela
        {
            for (int x = 0; x < gridLength - m_MipMapLevel; x+=m_MipMapLevel, I+=m_MipMapLevel)
            {
                if (!m_pathfindinGrid[x, y])
                    continue;

                //edge = Instantiate(cube);
                //edge.transform.position = ArrayPosToWorld(new Vector2(I%gridLength, (I)/gridLength))+Vector3.up*2;
                //edge.name = (I%gridLength).ToString();
                //edge.transform.parent = cell.transform;

                if (m_pathfindinGrid[x, y + m_MipMapLevel])
                {
                    pathfinder.AddEdge(new Edge(I, I + m_MipMapLevel * gridLength));

                    //edge = new GameObject();
                    //edge.AddComponent<LineRenderer>();
                    //edge.GetComponent<LineRenderer>().SetPosition(0, IdxToWorld(I));
                    //edge.GetComponent<LineRenderer>().SetPosition(1, IdxToWorld(I + skipped * gridLength));
                }
                if (m_pathfindinGrid[x + m_MipMapLevel, y])
                {
                    pathfinder.AddEdge(new Edge(I, I + m_MipMapLevel));

                    //edge = new GameObject();
                    //edge.AddComponent<LineRenderer>();
                    //edge.GetComponent<LineRenderer>().SetPosition(0, IdxToWorld(I));
                    //edge.GetComponent<LineRenderer>().SetPosition(1, IdxToWorld(I + skipped));
                }
                if (m_pathfindinGrid[x + m_MipMapLevel, y + m_MipMapLevel])
                {
                    pathfinder.AddEdge(new Edge(I, I + m_MipMapLevel * gridLength + m_MipMapLevel));

                    //edge = new GameObject();
                    //edge.AddComponent<LineRenderer>();
                    //edge.GetComponent<LineRenderer>().SetPosition(0, IdxToWorld(I));
                    //edge.GetComponent<LineRenderer>().SetPosition(1, IdxToWorld(I + skipped * gridLength + skipped));
                }
                if (y > m_MipMapLevel && x < gridLength - m_MipMapLevel && m_pathfindinGrid[x + m_MipMapLevel, y - m_MipMapLevel])
                {
                    pathfinder.AddEdge(new Edge(I, I + m_MipMapLevel * gridLength - m_MipMapLevel));

                    //edge = new GameObject();
                    //edge.AddComponent<LineRenderer>();
                    //edge.GetComponent<LineRenderer>().SetPosition(0, IdxToWorld(I));
                    //edge.GetComponent<LineRenderer>().SetPosition(1, IdxToWorld(I + skipped * gridLength - skipped));
                }
            }
        }
    }

    public void GetPath(Vector3 from, Vector3 to)
    {
        if (!isInited)
        {
            print("Pathfinding is not intitialized !!");
            return;
        }


        pathfinder.GetPath(WorldToIdx(from), WorldToIdx(to));

    }

    public List<Vector3> GetResultPath()
    {
        List<int> foundPath = new List<int>();
        List<Vector3> worldPath = new List<Vector3>();

        foundPath = pathfinder.GetResultPath();

        if (foundPath == null)
            return null;

        foreach (int i in foundPath)
        {
            worldPath.Add(IdxToWorld(i));
        }

        return worldPath;
    }

    public List<Vector3> GetExploredPath() {
        List<int> foundPath = new List<int>();
        List<Vector3> worldPath = new List<Vector3>();

        foundPath = pathfinder.GetExploredNodes();

        if (foundPath == null)
            return null;

        foreach (int i in foundPath)
        {
            worldPath.Add(IdxToWorld(i));
        }

        return worldPath;
    }

    #region converter wrappers

    public Vector2 WorldToArrayPos(Vector3 worldPos)
    {
        return IMapsMgr.WorldToArrayPos(worldPos);
    }

    public Vector3 ArrayPosToWorld(Vector2 arrayPos)
    {
        return IMapsMgr.ArrayPosToWorld(arrayPos);
    }


    public int WorldToIdx(Vector3 worldPos)
    {
        int idx;
        idx = IMapsMgr.WorldToIdx(worldPos);
        Vector2 arrPos = WorldToArrayPos(worldPos);
        arrPos.x -= arrPos.x%m_MipMapLevel;
        arrPos.y -= arrPos.y%m_MipMapLevel;
        idx = ArrayPosToIdx(arrPos);
        return idx;
    }

    public Vector3 IdxToWorld(int idx)
    {
        return IMapsMgr.IdxToWorld(idx);
    }


    public int ArrayPosToIdx(Vector2 arrayPos)
    {
        return IMapsMgr.ArrayPosToIdx(arrayPos);
    }

    public Vector2 IdxToArrayPos(int idx)
    {
        return IMapsMgr.IdxToArrayPos(idx);
    }

    #endregion

    public bool[,] BitmapGrid() { return m_pathfindinGrid; }

};

public class Edge
{
    public int a, b;
    public float weight;

    public Edge()
    {
        a = b = -1;
        weight = -1f;
    }

    public Edge(int A, int B, int W = 1)
    {
        a = A;
        b = B;
        weight = W;
    }
};

public class Node
{
    public int id;
    public List<Edge> edges;
    private List<KeyValuePair<Node, float>> neighbors;

    public int prev;
    public float costSoFar, estimated, totalEstimated;

    public Node()
    {
        id = -1;
        neighbors = new List<KeyValuePair<Node, float>>();
        edges = new List<Edge>();

        prev = -1;
        estimated = totalEstimated = costSoFar = float.MaxValue;
    }

    public Node(int nodeId)
    {
        id = nodeId;
        neighbors = new List<KeyValuePair<Node, float>>();
        edges = new List<Edge>();

        prev = -1;
        estimated = totalEstimated = costSoFar = float.MaxValue;
    }

    public Node(int nodeId, List<Edge> nodeEdges)
    {
        id = nodeId;
        neighbors = new List<KeyValuePair<Node, float>>();
        edges = nodeEdges;

        prev = -1;
        estimated = totalEstimated = costSoFar = float.MaxValue;
    }

    public void UpdateTotalEstimated()
    {
        totalEstimated = costSoFar + estimated;
    }

    public void AddNeighbor(Node nbr, float nbrDistance = 1)
    {
        neighbors.Add(new KeyValuePair<Node, float>(nbr, nbrDistance));
    }

    public List<KeyValuePair<Node, float>> Neighbors()
    {
        return neighbors;
    }

};

public class Graph
{
    public List<Node> nodes;
    public List<Edge> edges;

    public Graph()
    {
        nodes = new List<Node>();
        edges = new List<Edge>();
    }

    public void AddNode(Node newNode)
    {
        nodes.Add(newNode);
    }
    public void AddEdge(Edge newEdge)
    {
        edges.Add(newEdge);
        nodes[newEdge.a].edges.Add(newEdge);
        nodes[newEdge.b].edges.Add(newEdge);
        nodes[newEdge.a].AddNeighbor(nodes[newEdge.b]);
        nodes[newEdge.b].AddNeighbor(nodes[newEdge.a]);
    }
};

class AStar : MonoBehaviour
{
    const float kTileBasedHeuristic = 1.001f;

    Graph graph;
    int c, iFrom, iTo;
    List<int> resultPath, explored;
    List<bool> isOpened;
    System.Threading.Thread m_Thread;
    System.Threading.ThreadStart m_ThreadStarter;

    public AStar()
    {
        graph = new Graph();
        isOpened = new List<bool>();
        resultPath = new List<int>();
    }

    public void AddNode(Node newNode){
        graph.AddNode(newNode);
        isOpened.Add(false);
    }

    public void AddEdge(Edge newEdge)
    {
        graph.AddEdge(newEdge);
    }

    void CalculatePath()
    {
        // The flow of the algorithm require it to be in 3 states (null: not searched yet, Empty: path not found, List: path found)
        resultPath = null;

        CalculatePath(iFrom, iTo);
        List<int> path = new List<int>();
        int tracker = iTo;
        while (tracker != iFrom)
        {
            path.Insert(0, tracker);
            tracker = graph.nodes[tracker].prev;
        }
        resultPath = path;
        print("path: " + resultPath.Count);
    }
    void CalculatePath(int from, int to)
    {
        List<Node> opened = new List<Node>();
        explored = new List<int>();
        Vector3 frmPos, toPos;
        Node tmpNode = graph.nodes[from];

        for (int i = 0; i < graph.nodes.Count; i++)
            isOpened[i] = false;


        toPos = IMapsMgr.IdxToWorld(to);

        foreach (Node n in graph.nodes)
        {
            frmPos = IMapsMgr.IdxToWorld(n.id);

            n.prev = -1;
            n.costSoFar = float.MaxValue;
            n.estimated = ( Mathf.Abs(frmPos.x - toPos.x) + Mathf.Abs(frmPos.z - toPos.z) ) * (float)kTileBasedHeuristic;
            n.UpdateTotalEstimated();
        }


        tmpNode.costSoFar = 0;
        tmpNode.UpdateTotalEstimated();

        opened.Add(tmpNode);
        isOpened[from] = true;

        float tmpDist = 0, nbrDist = 0;
        Node pCurrNode = null, pNbrNode = null;

        while (opened.Count > 0)
        {
            opened.Sort((n1, n2) => (n2.totalEstimated.CompareTo(n1.totalEstimated)));
            pCurrNode = opened[opened.Count - 1];
            opened.RemoveAt(opened.Count - 1);

            if (pCurrNode.id == to)
                break;

            //Add to explored nodes list
            explored.Add(pCurrNode.id);

            foreach(KeyValuePair<Node, float> n in pCurrNode.Neighbors()){
                pNbrNode = n.Key;
                nbrDist = n.Value;
                tmpDist = pCurrNode.costSoFar + nbrDist + pNbrNode.estimated;


                if (tmpDist < pNbrNode.totalEstimated)
                {
                    pNbrNode.prev = pCurrNode.id;
                    pNbrNode.costSoFar = pCurrNode.costSoFar + nbrDist;
                    pNbrNode.UpdateTotalEstimated();

                    if (!isOpened[pNbrNode.id])
                    {
                        isOpened[pNbrNode.id] = true;
                        opened.Add(pNbrNode);
                        
                    }

                }
            }

        }

            print("finished");

    }

    public void GetPath(int from, int to)
    {
        iFrom = from;
        iTo = to;
        m_ThreadStarter = new System.Threading.ThreadStart(CalculatePath);
        m_Thread = new System.Threading.Thread(m_ThreadStarter);
        m_Thread.Start();

        ////CalculatePath(from, to);
        //List<int> path = new List<int>();
        //int tracker = to;
        //while (tracker != from)
        //{
        //    path.Insert(0, tracker);
        //    tracker = graph.nodes[tracker].prev;
        //}
        //return path;
    }

    public List<int> GetResultPath()
    {
        return resultPath;
    }

    public List<int> GetExploredNodes()
    {
        return explored;
    }
};