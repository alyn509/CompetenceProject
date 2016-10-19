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
    [Tooltip("How much of a room's tiles is allowed to be filled with objects/enemies, etc.")]
    public float objectToRoomPercent = 0.6f;

    public Vector3 playerStart; 
    public int playerRadius = 5;
    public Vector3 mazeExit;
    public int exitRadius = 3;

    [Tooltip("A list of drop-off points. Only one for all of them, for now.")]
    [HideInInspector]
    public List<Vector3> dropAreas = new List<Vector3>();

    //the first room, last room, and the list of all rooms.
    public Room mainRoom;
    public Room lastRoom;
    //the first room in this list is the first room.
    //try to avoid it when placing enemies initially :)
    //A Room has a list of all the tiles in the room.
    public List<Room> roomsInMaze;

    HashSet<Coord> TakenSpaces = new HashSet<Coord>();

    public string seed;
    public bool useRandomSeed;
    public Camera mainCamera;

    [Tooltip("49 with a 200x200 area seems nice.")]
    [Range(0, 100)]
    public int randomFillPercent;

    [HideInInspector]
    public int[,] map;

    List<Coord> listOfCoord = new List<Coord>();

    public GameObject drop;

    void Start()
    {
        GenerateMap();
        //right now this finds (and fills) all possible spaces.
        //add possibility of chosing a set number?
        FindStartAndEndPos();
        dropAreas.Add(playerStart);
        dropAreas.Add(mazeExit);

        GenerateDropAreas(3, 100);
        
       /* for (int i = 0; i < dropAreas.Count; i += 1)
        {
            if (dropAreas.Count-1 >= i + 1)
            {
                Debug.DrawLine(dropAreas[i], dropAreas[i+1], Color.red, 100);
            }
        }*/

       /*foreach (Coord tile in TakenSpaces)
        {
            GameObject newObj = Instantiate(drop) as GameObject;
            newObj.transform.position = CoordToWorldPoint(tile);
        }*/

        foreach (Vector3 vec in dropAreas)
        {
            GameObject newObj = Instantiate(drop) as GameObject;
            newObj.transform.position = vec;
        }
        Debug.Log("Number of rooms: " + roomsInMaze.Count + " and taken tiles: " + TakenSpaces.Count + " and takenspaces found times: " + takenSpaces);
    }

    //take into consideration that we are dropping enemies, the player,
    // exits, and items... perhaps have a list of Coord tiles that are taken,
    // and cross reference?
    //then we choose areas in terms of priority (player, exit, item, enemy)
    //note that the number of enemy drop points will wary.
    // should the drop points also be the patrol points?
    //create transfrom objects procedurally as necessary?

    //Find how many drop areas a room can have
    //remember not to cram the room full.
    //how many rooms does the standard map have? check.
    //also, room has to have enough tiles to even be considered. sizeRadius +10?
    public int CalculateNumberOfDropAreas(Room room, int sizeRadius){
        if (room.tiles.Count < sizeRadius * sizeRadius + 2)
        {   //not enough room.
            return 0;
        }else {
            //The number of drop-off areas.
            int tileCount = 0;
            foreach (Coord tile in room.tiles)
            {
                if (!TakenSpaces.Contains(tile))
                {
                    tileCount++; //how many untaken tiles are left?
                }
            }//take sizeRadius into account:
            return (int)((tileCount * objectToRoomPercent)/(sizeRadius*sizeRadius));
        } 
    }

    //'numberOfDrops' is set to 0/null by default, which indicates that all possible spaces should be filled.
    public void GenerateDropAreas(int sizeRadius, int numberOfDrops = 0)
    {
        Coord dropArea;
        int drops = 0;

        //find item/enemy dropoff area
        if (numberOfDrops > 0)
        {
            int tries = 0; //just to keep it from going forever.
            for (int i = 0; i < numberOfDrops; i++)
            {
                Room room = roomsInMaze[ UnityEngine.Random.Range(0, roomsInMaze.Count-1) ];

                int areas = CalculateNumberOfDropAreas(room, sizeRadius);
                if (areas == 0)
                { //if there are none, skip.
                    room = roomsInMaze[UnityEngine.Random.Range(0, roomsInMaze.Count - 1)];
                    tries++;
                }

                
                dropArea = MakeDropArea(room, sizeRadius);
                while (dropArea.tileX == 0 && tries < 10)
                {
                    room = roomsInMaze[UnityEngine.Random.Range(0, roomsInMaze.Count - 1)];
                    dropArea = MakeDropArea(room, sizeRadius);
                    tries++;
                }

                //assuming we have a good tile, save it.
                if (dropArea.tileX != 0)
                {
                    drops++;
                    dropAreas.Add(CoordToWorldPoint(dropArea));
                }
                else // otherwise, stop.
                {
                    Debug.Log("Couldn't create drop area " + (i + 1) + " of " + areas + ". Continueing to next room.");
                    break; //might as well stop completely.
                }

            }

        }
        else
        {
            foreach (Room room in roomsInMaze)
            {
                int areas = CalculateNumberOfDropAreas(room, sizeRadius);
                if (areas == 0) //if there are none, skip.
                    continue;

                for (int i = 0; i < areas; i++)
                {
                    dropArea = MakeDropArea(room, sizeRadius);
                    if (dropArea.tileX != 0)
                    {
                        drops++;
                        dropAreas.Add(CoordToWorldPoint(dropArea));
                    }
                    else
                    {
                        Debug.Log("Couldn't create drop area " + (i + 1) + " of " + areas + ". Continueing to next room.");
                        break;
                    }



                    //if we have the desired number of drops, return.
                    if (numberOfDrops != 0 && drops == numberOfDrops)
                    {
                        return;
                    }
                }
            }
        }
    }

    public void FindStartAndEndPos()
    {
        //find player drop off area
        Coord bestCoord = FindBestArea(mainRoom, playerRadius);
        if (bestCoord.tileX == 0)
        {   //if I didn't find an area with enough space, make one and get the coordinates!
            try
            {
                bestCoord = MakeDropArea(mainRoom, playerRadius);
                playerStart = CoordToWorldPoint(bestCoord); 
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't get valid playerStart coordinates.");
            }
        }
        else
        {   //if a best area was found, find the coordinates
            playerStart = CoordToWorldPoint(bestCoord); 
        }

        //find exit drop off area
        bestCoord = FindBestArea(lastRoom, exitRadius);
        if (bestCoord.tileX == 0)
        {   //if I didn't find an area with enough space, make one and get the coordinates!
            try
            {
                bestCoord = MakeDropArea(lastRoom, exitRadius);
                mazeExit = CoordToWorldPoint(bestCoord);
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't get valid mazeExit coordinates.");
            }
        }
        else
        {   //if a best area was found, find the coordinates
            mazeExit = CoordToWorldPoint(bestCoord);
        }
    }

    //this function finds an area in a room where the 'item' can fit
    //or makes room for it.
    //maybe add a +1 to radius to leave room for passage.
    public Coord MakeDropArea(Room room, int sizeRadius)
    {
        //find a good tile in radius range and draw a circle
        Coord bestTile = FindBestArea(room, sizeRadius);
        if (bestTile.tileX == 0)
        {
            foreach (Coord tile in room.tiles) //in emergencies, find the first, the best.
            {
                if (tile.tileX < width - sizeRadius - 2 && tile.tileX > 2 &&
                    tile.tileY < height - sizeRadius - 2 && tile.tileY > 2)
                {
                    bestTile = tile;
                    break;
                }
                else
                {
                    Debug.Log("Couldn't find a drop-off place within map limits. Last checked Tile: " + bestTile.tileX+ ", " + bestTile.tileY);
                }
            }
        }
        if (bestTile.tileX != 0)
        DrawCircle(bestTile, sizeRadius, true);
        return bestTile;
    }

    int takenSpaces = 0;

    public Coord FindBestArea(Room room, int radius)
    {
        // foreach tile in room, check the radius. 
        // if none have an entirely empty radius, 
        // MakeDropArea clears one.
        bool breakLoops = false;
		int tries = 0;
        HashSet<Coord> checkedSpaces = new HashSet<Coord>();
        //foreach (Coord tile in room.tiles)
        int roomTiles = room.tiles.Count;
        for (int i = 0; i < room.tiles.Count; i++ )
        {
            Coord tile = room.tiles[UnityEngine.Random.Range(0, roomTiles - 1)];
			while (checkedSpaces.Contains(tile) && tries == 20)
            {
                tile = room.tiles[UnityEngine.Random.Range(0, roomTiles - 1)];
				tries++;
            }
			if (tries >= 20) {
                break;
			}
            checkedSpaces.Add(tile);

            breakLoops = false;
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int checkX = tile.tileX + x;
                        int checkY = tile.tileY + y;
                        if (IsInMapRange(checkX, checkY)) // if it's on the map...
                        {
                            if (map[checkX, checkY] == 1 || TakenSpaces.Contains(new Coord(checkX, checkY)))
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
                return tile;
            }
        }
        Coord tileTest = new Coord(0, 0);
        return tileTest;
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
        List<Room> finalRooms = new List<Room>();

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
                finalRooms.Add(new Room(roomRegion, map));
            }
        }
        finalRooms.Sort();
        finalRooms[0].isMainRoom = true;
        mainRoom = finalRooms[0];
        finalRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(finalRooms);

        roomsInMaze = finalRooms;
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

    void DrawCircle(Coord gridTile, int radius, bool savePoints = false)
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
                    if (savePoints)
                    {
                        if (TakenSpaces.Contains(new Coord(widenX, widenY)))
                        {
                            Debug.Log("FOUND A DROPAREA IN TAKENSPACES!");
                        }
                        TakenSpaces.Add(new Coord(widenX, widenY));   
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