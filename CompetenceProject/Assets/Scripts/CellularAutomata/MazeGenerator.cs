using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MazeGenerator : MonoBehaviour
{

    public int width;
    public int height;
    [Tooltip("How wide the passage should be.")]
    public int passageRadius = 5;

    //the first room, last room, and the list of all rooms.
    public Room mainRoom;
    public Room lastRoom;
    //the first room in this list is the first room.
    //try to avoid it when placing enemies initially :)
    //A Room has a list of all the tiles in the room.
    public List<Room> roomsInMaze;

    public string seed;
    public bool useRandomSeed;
    public Camera mainCamera;

    [Tooltip("49 with a 200x200 area seems nice.")]
    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        int recursion = 4;

        for (int i = 0; i < recursion+1; i++)
        {
            if (i < recursion)
            {
                SmoothMap(4, true, false);
            }
            else
            {
                SmoothMap(4, false , true);
            }
        }

        ProcessMap();

        int borderSize = 1;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MazeMeshesGenerator meshGen = GetComponent<MazeMeshesGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);

        //Attempt to fix texture issue.
        mainCamera.nearClipPlane += 1f;
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    if (IsInMapRange(tile.tileX, tile.tileY))
                    {
                        map[tile.tileX, tile.tileY] = 0;
                    }
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        mainRoom = survivingRooms[0];
        survivingRooms[0].isAccessibleFromMainRoom = true;

        //Debug.DrawLine (CoordToWorldPoint (survivingRooms[0].tiles[0]), CoordToWorldPoint (survivingRooms[survivingRooms.Count-2].tiles[0]), Color.green, 100);
        ConnectClosestRooms(survivingRooms);

        roomsInMaze = survivingRooms;
        Debug.DrawLine(CoordToWorldPoint(mainRoom.tiles[mainRoom.tiles.Count / 2]), CoordToWorldPoint(lastRoom.tiles[lastRoom.tiles.Count/2]), Color.green, 100);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        //through the first running of this function, the rooms are connected to the closest neighbour,
        //if not already connect.

        //in the second run, the rooms are forced to have a cohesive connection to the first room.

        //A list of rooms that aren't connected to the main room.
        List<Room> unConnectedRooms = new List<Room>();
        //list of rooms that ARE connected to the main room.
        List<Room> connectedRooms = new List<Room>();

        List<Room> roomOrderList = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            roomOrderList.Add(allRooms[0]); //the first room is the main room.
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    connectedRooms.Add(room);
                }
                else
                {
                    unConnectedRooms.Add(room);
                }
            }
        }
        else
        {
            unConnectedRooms = allRooms;
            connectedRooms = allRooms;
        }

        //The best distance variable tells us which room is closest,
        //and easiest to connect to.
        int closestDistance = 0;
        Coord closestUnconnectedTile = new Coord();
        Coord closestConnectedTile = new Coord();
        Room closestUnconnectedRoom = new Room();
        Room closestConnectedRoom = new Room();
        bool possibleConnectionFound = false;

        //for each room A (in second run, without a connection to the first room)...
        foreach (Room roomA in unConnectedRooms)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                //only continues to next room, if current room already has connections:
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            //check through all rooms B (that does have a connection to the first room, in the second run)...
            foreach (Room roomB in connectedRooms)
            {
                //If the two rooms are the same room, or are already connect, skip.
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                //for all tiles in room A...
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    //and all tiles in room B...
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        //find the distance between the two...
                        Coord unconnectedTile = roomA.edgeTiles[tileIndexA];
                        Coord connectedTile = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(unconnectedTile.tileX - connectedTile.tileX, 2) + Mathf.Pow(unconnectedTile.tileY - connectedTile.tileY, 2));

                        //... and save the best one.
                        if (distanceBetweenRooms < closestDistance || !possibleConnectionFound)
                        {
                            closestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            closestUnconnectedTile = unconnectedTile;
                            closestConnectedTile = connectedTile;
                            closestUnconnectedRoom = roomA;
                            closestConnectedRoom = roomB;
                        }
                    }
                }
            }
            //if a connection (or several) was found, the best distanced tiles are chosen to create a passage:
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {   //maybe place this in next part as well?
                CreatePassage(closestUnconnectedRoom, closestConnectedRoom, closestUnconnectedTile, closestConnectedTile);
            }
        }

        //in the second run, this is called to ensure connection to a room that connects to the first room.
        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
        else
        {
            roomOrderList.Add(roomOrderList[0].connectedRooms[0]);
            bool endFound = false;
            while (!endFound)
            {
                int i = 0;
                if (roomOrderList[roomOrderList.Count - 1].connectedRooms.Count < i)
                {
                    Room room = roomOrderList[roomOrderList.Count - 1].connectedRooms[i];
                    if (!roomOrderList.Contains(room))
                    {
                        roomOrderList.Add(room);
                    }
                }
                else
                {
                    lastRoom = roomOrderList[roomOrderList.Count - 1];
                    endFound = true;
                }
            }
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        //Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);
        List<Coord> line = GetLine(tileA, tileB);

        //Each point in the line is given at radius, around which the tiles are change to non-walls.
        //The radius is important for character movement and -visuals.
        foreach (Coord gridTile in line)
        {
            DrawCircle(gridTile, passageRadius);
        }
    }

    void DrawCircle(Coord gridTile, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int widenX = gridTile.tileX + x;
                    int widenY = gridTile.tileY + y;
                    if (IsInMapRange(widenX, widenY))
                    {
                        map[widenX, widenY] = 0;
                    }
                }
            }
        }
    }

    /*returns a list of coordinates for each point in the line
     which we use to eventually find the cells that need to change to non-walls.*/
    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        /*The equation for a line goes y = dx/dy + c*/
        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        //which way we increment is decided by the sign of dx:
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        //
        int furthest = Mathf.Abs(dx);
        int least = Mathf.Abs(dy);


        //if this is the case, it is inverted, so we need to flip which variable is used for incremention.
        //This makes our logic work for both cases of either x or y being longer.
        if (furthest < least)
        {
            inverted = true;
            furthest = Mathf.Abs(dy);
            least = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        //
        int gradientAccumulation = furthest / 2;
        for (int i = 0; i < furthest; i++)
        {
            //add the new coordinate first
            line.Add(new Coord(x, y));

            //According to which axis we travel the furthest across...
            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += least;
            if (gradientAccumulation >= furthest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= furthest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }


    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap(int cellDeath, bool changeEmpty, bool widenPassages)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                
                if (neighbourWallTiles > cellDeath && changeEmpty)
                    map[x, y] = 1;
                else if (neighbourWallTiles < cellDeath)
                    map[x, y] = 0;

                //if widenPassages is ordered, call the function.
                if (widenPassages && map[x, y] == 0 && x > 1 && y > 1 && x < width-1 && y < height -1)
                {
                    WidenPassages(new Coord(x, y), passageRadius+20);
                }

            }
        }
    }

    //only checks in a range of 1.
   /* void WidenPassages(Coord gridTile)
    {
        bool drawCircle = false;

        int gridX = gridTile.tileX;
        int gridY = gridTile.tileY;

        //in this function, we need to check two neighbours at one time;
        //if any of the immediate neighbours are both walls, and the current tile is empty,
        //the walls should be removed, to widen passage.
        //if (gridX > width - 2 || gridY > height - 2)
        
        for (int x1 = gridX - 1; x1 <= gridX + 1; x1++)
        {
            for (int y1 = gridY - 1; y1 <= gridY + 1; y1++)
            {
                for (int x2 = gridX - 1; x2 <= gridX + 1; x2++)
                {
                    for (int y2 = gridY - 1; y2 <= gridY + 1; y2++)
                    {
                        if (x1 != gridX && x2 != gridX && y1 != gridY && y2 != gridY)
                        {
                            if (x1 == x2 || y1 == y2 ||
                                x1 == x2 - 2 || x1 == x2 + 2 ||
                                y1 == y2 - 2 || y1 == y2 + 2)
                                //Debug.Log("x1,y1: " + x1 + ", " + y1 + "     x2,y2: " + x2 + ", " + y2);
                                if (map[x1, y1] == 1 && map[x2, y2] == 1)
                                {
                                    if (x1 != x2 && y1 != y2)
                                    {
                                        
                                        drawCircle = true;
                                        break;
                                    }
                                }
                        }
                    }
                }
            }
        }
        //improvements: this logic only account for tiles with immediate wall-neighbours.
        if (drawCircle)
            DrawCircle(gridTile, passageRadius);
    }*/

    //this method checks for walls in a set range.
    //a variable wallCount counts them, and
    //only erases the walls, if there is more than two.
    void WidenPassages(Coord gridTile, int radius)
    {
        int wallCount = 0;
        bool breakLoops = false;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int checkX = gridTile.tileX + x;
                    int checkY = gridTile.tileY + y;
                    if (IsInMapRange(checkX, checkY))
                    {
                        if (map[checkX, checkY] == 1)
                            wallCount++;
                    }
                    else
                    {
                        wallCount = 0;
                        breakLoops = true;
                        break;
                    }
                }
            }
            break;
        }
        if (wallCount > 1)
            DrawCircle(gridTile, radius);
    }


    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }
}