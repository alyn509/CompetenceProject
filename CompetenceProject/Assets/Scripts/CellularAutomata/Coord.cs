using UnityEngine;
using System.Collections;

//a Coordinates class, for holding tile coordinates.
public struct Coord 
{

    public int tileX;
    public int tileY;

    public Coord(int x, int y)
    {
        tileX = x;
        tileY = y;
    }

}
