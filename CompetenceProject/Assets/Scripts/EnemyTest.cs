using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyTest : MonoBehaviour
{

    public float moveSpeed = 6.0F;
    public float rotateSpeed = 100.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 rotateDirection = Vector3.zero;
    //private NavMeshAgent agent;
    public List<Transform> patrolPoints = new List<Transform>();
    int currentPoint = -1;
    public Node[,] map;
    MazeGenerator MazeGen;
    int width;
    int height;
    bool mapCreated = false;
    public GameObject drop;

    void Start()
    {
        currentPoint = 0;

        MazeGen = GameObject.Find("MazeGenerator").GetComponent<MazeGenerator>();

    }

    void Update()
    {
        if (!mapCreated && MazeGen.generationDone)
        {
            map = MazeGen.nodeMap;
            mapCreated = true;
            width = map.GetLength(0);
            height = map.GetLength(1);
        }

        if (Input.GetKeyDown("space") && mapCreated)
        {
            currentPoint++;
            FindPath(map[0, 0], map[width/2-1, height/2-1]);
            GameObject obj = Instantiate(drop) as GameObject;
            obj.transform.position = CoordToWorldPoint(new Coord(width / 2 - 1, height / 2 - 1));

            this.transform.position = CoordToWorldPoint(new Coord(width, height));
            Debug.Log("Path length: " + Path.Count);
            gameObject.transform.position = CoordToWorldPoint(new Coord(Path[Path.Count-1].x, Path[Path.Count - 1].y));
        }

    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    public List<Node> path;

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int i = -1; i <= 1; i++)
        {
            if (i == 0)
                continue;

            int checkX = node.x + i;
            int checkY = node.y + i;
            if (checkX >= 0 && checkX < map.GetLength(0) && checkY >= 0 && checkY < map.GetLength(1))
            {
                neighbours.Add(map[node.x, checkY]);
                neighbours.Add(map[checkX, node.y]);
            }
        }
        return neighbours;
    }

    void FindPath(Node startNode, Node targetNode)
    {
        int openDoesntContainNeighbour = 0;
        List<Node> Open = new List<Node>(); ///not yet evaluated
        HashSet<Node> Closed = new HashSet<Node>();// already evaluated
        Open.Add(startNode); // add startNode to Open

        while (Open.Count > 0)
        {
            //Node currentNode = Open[0];
            Node currentNode = Open[0]; //pick the first node,
            for (int i = 1; i < Open.Count; i++) //compare to other neighbours, if any,
            {
                if (Open[i].F < currentNode.F || Open[i].F == currentNode.F && Open[i].H < currentNode.H)
                {
                    currentNode = Open[i]; // if other neighbours are a better choice, pick them instead.
                }
            }
            Open.Remove(currentNode); //remove the node.
            Closed.Add(currentNode); //add to evaluated tiles.

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            List<Node> neighbours = GetNeighbours(currentNode);
            //Debug.Log("number of neighbours: " + neighbours.Count);
            foreach (Node neighbour in neighbours)
            {
                if (Closed.Contains(neighbour))
                {
                    continue;
                }

                float newCostToAdj = currentNode.G + CostToEnter(currentNode.x, currentNode.y);
                if (newCostToAdj < neighbour.G || !Open.Contains(neighbour))
                {
                    neighbour.G = newCostToAdj;
                    neighbour.H = CostToEnter(currentNode.x, currentNode.y);
                    neighbour.ParentNode = currentNode;

                    if (!Open.Contains(neighbour))
                    {
                        openDoesntContainNeighbour++;
                        Open.Add(neighbour);
                    }
                }
            }
        }
    }

    public List<Node> Path = new List<Node>();
    void RetracePath(Node startNode, Node endNode)
    {
        Debug.Log("Retracing path! ");
        int retracinglength = 0;
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            retracinglength++;
            path.Add(currentNode);
            currentNode = currentNode.ParentNode;
        }
        path.Reverse();
        Path = path;
    }

    //float GetDistance(Node locationA, Node locationB)
    float GetDistance(Node locationA, Node locationB)
    {
        int distX = Mathf.Abs(locationA.x - locationB.x);
        int distY = Mathf.Abs(locationA.y - locationB.y);
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }


    public float CostToEnter(int x, int y)
    {
        if (map[x, y].Type == 1)
            return 1000;
        else
            return 1;
    }
}
