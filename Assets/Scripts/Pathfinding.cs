using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

    static Color32[] m_bitmap; //should be a squared bitmap
    static bool[,] m_pathfindinGrid;
    static bool[] m_pathfindingGridFlatted;
    static AStar pathfinder = new AStar();
    static bool isInited;

    int m_MipMapLevel = 2;

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

    #region converter wrappers

    public Vector2 WorldToArrayPos(Vector3 worldPos)
    {
        return InfluenceMaps.WorldToArrayPos(worldPos);
    }

    public Vector3 ArrayPosToWorld(Vector2 arrayPos)
    {
        return InfluenceMaps.ArrayPosToWorld(arrayPos);
    }


    public int WorldToIdx(Vector3 worldPos)
    {
        int idx;
        idx = InfluenceMaps.WorldToIdx(worldPos);
        Vector2 arrPos = WorldToArrayPos(worldPos);
        arrPos.x -= arrPos.x%m_MipMapLevel;
        arrPos.y -= arrPos.y%m_MipMapLevel;
        idx = ArrayPosToIdx(arrPos);
        return idx;
    }

    public Vector3 IdxToWorld(int idx)
    {
        return InfluenceMaps.IdxToWorld(idx);
    }


    public int ArrayPosToIdx(Vector2 arrayPos)
    {
        return InfluenceMaps.ArrayPosToIdx(arrayPos);
    }

    public Vector2 IdxToArrayPos(int idx)
    {
        return InfluenceMaps.IdxToArrayPos(idx);
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
    public List<int> neighbors;
    public List<Edge> edges;


    public int prev;
    public float minDist, estimated, totalEstimated;

    public Node()
    {
        id = -1;
        neighbors = new List<int>();
        edges = new List<Edge>();

        prev = -1;
        estimated = totalEstimated = minDist = float.MaxValue;
    }

    public Node(int nodeId)
    {
        id = nodeId;
        neighbors = new List<int>();
        edges = new List<Edge>();

        prev = -1;
        estimated = totalEstimated = minDist = float.MaxValue;
    }

    public Node(int nodeId, List<Edge> nodeEdges)
    {
        id = nodeId;
        neighbors = new List<int>();
        edges = nodeEdges;

        prev = -1;
        estimated = totalEstimated = minDist = float.MaxValue;
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
    }
};

class AStar : MonoBehaviour
{
    Graph graph;
    int c, iFrom, iTo;
    List<int> resultPath;
    System.Threading.Thread m_Thread;
    System.Threading.ThreadStart m_ThreadStarter;

    public AStar()
    {
        graph = new Graph();
    }

    public void AddNode(Node newNode){
        graph.AddNode(newNode);
    }

    public void AddEdge(Edge newEdge)
    {
        graph.AddEdge(newEdge);
    }

    void CalculatePath()
    {
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
        List<Node> visited, unvisited;

        print("started");

        Node tmpNode, currNode;
        int tmpId;
        float tmpDist;
        Vector3 tmpPos, targetPos;

        int maxVisited = -1;

        visited = new List<Node>();
        unvisited = new List<Node>();
        currNode = new Node();
        targetPos = InfluenceMaps.IdxToWorld(to);


        for (int i = 0; i < graph.nodes.Count; i++)
        {
            tmpNode = graph.nodes[i];
            tmpNode.prev = -1;
            tmpNode.minDist = float.MaxValue;
            tmpPos = InfluenceMaps.IdxToWorld(tmpNode.id);
            tmpNode.estimated = Mathf.Abs(tmpPos.x - targetPos.x) + Mathf.Abs(tmpPos.z-targetPos.z);
        }
        c = 0;

        tmpNode = graph.nodes[from];
        tmpNode.minDist = 0;
        tmpNode.totalEstimated = tmpNode.estimated;

        unvisited.Add(graph.nodes[from]);
        

        while (unvisited.Count > 0 && c++<20000)
        {
            //print(">> " + c + " " + unvisited.Count);
            maxVisited = Mathf.Max(visited.Count, maxVisited);
            tmpDist = int.MaxValue;


            for (int i = 0; i < unvisited.Count; i++)
            {
                if (unvisited[i].totalEstimated < tmpDist)
                {
                    currNode = unvisited[i];
                    tmpDist = unvisited[i].totalEstimated;
                    tmpId = i;
                }
            }


            if (currNode.id == to)
                break;

            unvisited.Remove(currNode);
            visited.Add(currNode);


            for (int i = 0; i < currNode.edges.Count; i++)
            {
                tmpId = (currNode.id == currNode.edges[i].a ? currNode.edges[i].b : currNode.edges[i].a);
                tmpDist = currNode.minDist + currNode.edges[i].weight;
                tmpNode = graph.nodes[tmpId];

                if(unvisited.Contains(tmpNode)){
                    if(tmpDist < tmpNode.minDist){
                        tmpNode.minDist = tmpDist;
                        tmpNode.totalEstimated = tmpNode.minDist+tmpNode.estimated;
                        tmpNode.prev = currNode.id;
                    }
                }
                else if (visited.Contains(tmpNode)){
                    if (tmpDist < tmpNode.minDist){
                        visited.Remove(tmpNode);
                        unvisited.Add(tmpNode);
                        tmpNode.totalEstimated = tmpNode.minDist+tmpNode.estimated;
                        tmpNode.minDist = tmpDist;
                        tmpNode.prev = currNode.id;
                    }
                }
                else{
                    tmpNode.minDist = tmpDist;
                    unvisited.Add(tmpNode);
                    tmpNode.totalEstimated = tmpNode.minDist+tmpNode.estimated;
                    tmpNode.prev = currNode.id;
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

};