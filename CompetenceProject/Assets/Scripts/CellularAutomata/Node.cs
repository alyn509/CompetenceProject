using UnityEngine;
using System.Collections;

//The basic node class, which the ControlNode class inherits from.
//It contains information on the position, as well as a reference to
//its vertex index.
public class Node
{
    public Vector3 position;
    public int vertexIndex = -1;

    public Node(Vector3 _pos)
    {
        position = _pos;
    }
}
