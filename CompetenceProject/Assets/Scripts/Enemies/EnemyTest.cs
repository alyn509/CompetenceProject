using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class EnemyTest : MonoBehaviour
{ 
    public float moveSpeed = 6.0F;
    public float rotateSpeed = 100.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 rotateDirection = Vector3.zero;
    public Node startPoint;
    //private NavMeshAgent agent;
    public List<Transform> patrolPoints = new List<Transform>();
    int currentPoint = -1;
    public Node[,] map;
    MazeGenerator MazeGen;
    int width;
    int height;
    bool mapCreated = false;
    public GameObject drop;
    bool pathfinding = false;
    bool move = false;
    private Thread m_Thread = null;
    int count = 0;
    public int life = 3;
    public GameObject frogMaterial;

    //Delegates are a variable that points to a function
    public delegate void PathfindingJobComplete(List<Node> path);


    public int MaxJobs = 2;

    void Start()
    {
        currentPoint = 0;

        MazeGen = GameObject.Find("MazeGenerator").GetComponent<MazeGenerator>();
        map = MazeGen.nodeMap;
        mapCreated = true;
        width = map.GetLength(0);
        height = map.GetLength(1);
        startPoint = map[5, 5];
        transform.position = CoordToWorldPoint(new Coord(startPoint.x, startPoint.y)) + new Vector3(0,-5,0);
    }

    int pathPoint = 0;
    Vector3 currentGoal;
    float speed = 2f;
    void Update()
    {
        if (move)
        {
            if (Vector3.Distance(transform.position, CoordToWorldPoint(new Coord(Path[pathPoint].x, Path[pathPoint].y))) < 2f && pathPoint < Path.Count-1)
            {
                pathPoint++;
                currentGoal = CoordToWorldPoint(new Coord(Path[pathPoint].x, Path[pathPoint].y));
            }
            if (pathPoint >= Path.Count - 1)
            {
                move = false;
            }
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, currentGoal, step);

        }

        //this shouldn't be here, but I'm testing the functionality.
        if (Input.GetKeyDown("b") && !pathfinding)
        {
            currentPoint++;
            m_Thread = new Thread(Run);
            m_Thread.Start();
            //FindPath(map[0, 0], map[width/2-1, height/2-1]);
            pathfinding = true;
        }

    }

    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collision!");
        //If we collided with a player, add item to player.
        if (collision.transform.tag == "bullet" || collision.transform.tag == "Player")
        {
            life--;
            Hit();
            if (life <= 0)
            {
                Death();
            }          
        }

        if (collision.transform.tag == "Player")
        {
            collision.transform.GetComponent<BasicMovement>().addPoints(5);
            collision.transform.GetComponent<BasicMovement>().Hit();

        }
    }

    public void Hit()
    {
        life--;
        if (life > 0)
        {
            StartCoroutine(Flash());

            //hit back?
        }
        else
        {
            //play death animation/pausing, etc.
        }

    }

    IEnumerator Flash()
    {
        Color normalColor = frogMaterial.GetComponent<Renderer>().material.color;
        frogMaterial.GetComponent<Renderer>().material.color = Color.red;
        yield return new WaitForSeconds(.25f);
        frogMaterial.GetComponent<Renderer>().material.color = normalColor;
    }

    void Death()
    {
        //play death animation
        Destroy(gameObject);
    }

    private void Run()
    {
        if (count == 0)
            FindPath(startPoint, map[width / 2 - 1, height / 2 - 1]);
        else
            FindPath(startPoint, map[width - 5, height -5]);
        count++;
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

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public int CheckNeighbourRadius(int radius, int xPos, int yPos)
    {
        Node tile = map[xPos, yPos];
        bool breakLoops = false;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int checkX = tile.x + x;
                    int checkY = tile.x + y;
                    if (IsInMapRange(checkX, checkY)) // if it's on the map...
                    {
                        if (map[checkX, checkY].Type == 1)
                        {// ... and it's a wall, or taken, break the loop and go to another tile!
                            breakLoops = true;
                            break;
                        }
                    }
                    else
                    {   // if not on map, breakx2 and go to new tile!
                        breakLoops = true;
                        break;
                    }
                }
                if (breakLoops)
                    break;
            }
        }
        if (!breakLoops)
        {
            return 1000;
        }
        else
            return 1;
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
            foreach (Node neighbour in neighbours)
            {
                if (Closed.Contains(neighbour))
                {
                    continue;
                }

                float newCostToAdj = currentNode.G + CheckNeighbourRadius(5, currentNode.x, currentNode.y) + CostToEnter(currentNode.x, currentNode.y);
                if (newCostToAdj < neighbour.G || !Open.Contains(neighbour))
                {
                    neighbour.G = newCostToAdj;
                    neighbour.H = CheckNeighbourRadius(5, currentNode.x, currentNode.y) + CostToEnter(currentNode.x, currentNode.y);
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

        pathfinding = false;
        move = true;
        currentGoal = CoordToWorldPoint(new Coord(Path[pathPoint].x, Path[pathPoint].y));
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
        //return CheckNeighbourRadius();
        if (map[x, y].Type == 1)
            return 1000;
        else
            return 1;
    }
}
