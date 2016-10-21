using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//The basic node class, which the ControlNode class inherits from.
//It contains information on the position, as well as a reference to
//its vertex index.
public class Node
{
    public Vector3 position;
    public int x;
    public int y;
    public int vertexIndex = -1;
    public int Type;
    public Node Left;
    public Node Right;
    public Node Top;
    public Node Bottom;
    public Node ParentNode;

    public Coord Location { get; private set; }
    public float G;//{ get; private set; }
    //public int 
    public float H; //{ get; private set; }
    public float F { get { return this.G + this.H; } }

    public NodeState State { get; set; }
    public List<Node> neighbours;

    public Node(Vector3 _pos, int x = 0, int y = 0)
    {
        position = _pos;
        this.x = x;
        this.y = y;
    }
}
public enum NodeState { Untested, Open, Closed }
