using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour {

    /*public Node startPosition;
    public Node endPosition;
    EnemyTest.PathfindingJobComplete callback;

    public volatile bool jobDone = false;


    public Pathfinder(Node start, Node target, EnemyTest.PathfindingJobComplete callback)
    {
        startPosition = start;
        endPosition = target;
        //completeCallback = callback;
    }

    /*public void RequestPathfind(Node start, Node target, PathfindingJobComplete completeCallback)
    {
        Pathfinder newJob = new Pathfinder(start, target, completeCallback);
        todoJobs.Add(newJob);
    }*/


    /*public List<Node> path;

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

        pathfinding = false;
        move = true;
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
    }*/
}
