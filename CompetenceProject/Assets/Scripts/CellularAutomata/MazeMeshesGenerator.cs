using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeMeshesGenerator : MonoBehaviour
{

    public SquareGrid squareGrid;
    public MeshFilter hedges;
    public Texture hedgesTexture;
    public int testInt;
    public MeshFilter hedgeTops;
    [Tooltip("Set the ground floor. This will be moved down so it matches hedgesHeight.")]
    public GameObject ground;
    [Tooltip("Drag the ground floor to this (it locates the mesh on its own).")]
    public MeshFilter groundFilter;
    [Tooltip("Set the height of the hedges.")]
    public float hedgesHeight = 5f;
    [Header("Show Texture on walls:")]
    [Tooltip("Since the wall-texture isn't working optimally, you can choose to not apply it.")]
    public bool showHedgeTexture = false;

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    public GameObject testPlane;

    public void GenerateMesh(int[,] map, float squareSize)
    {
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        hedgeTops.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, map.GetLength(1) / 2 * squareSize, vertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;

        MeshCollider hedgetopCollider = gameObject.AddComponent<MeshCollider>();
        hedgetopCollider = GameObject.Find("HedgeTops").AddComponent<MeshCollider>();

        /*GameObject hedgetops = GameObject.Find("HedgeTops");

        hedgetops.AddComponent<NavMeshObstacle>();
        hedgetops.GetComponent<NavMeshObstacle>().carving = true;
        hedgetops.GetComponent<NavMeshObstacle>().carveOnlyStationary = true;*/



        CreateHedgesMesh(map, squareSize);

        /*this.gameObject.AddComponent<NavMeshObstacle>();
        this.gameObject.GetComponent<NavMeshObstacle>().carving = true;
        this.gameObject.GetComponent<NavMeshObstacle>().carveOnlyStationary = true;*/

        GameObject hedge = GameObject.Find("Hedges");
        hedge.AddComponent<NavMeshObstacle>();

        hedge.GetComponent<NavMeshObstacle>().shape = NavMeshObstacleShape.Capsule;
        hedge.GetComponent<NavMeshObstacle>().carving = true;
        hedge.GetComponent<NavMeshObstacle>().carveOnlyStationary = true;
    }

    void CreateHedgesMesh(int[,] map, float squareSize)
    {

        /*MeshCollider[] currentColliders3D = hedges.gameObject.GetComponents<MeshCollider>();

        for (int i = 0; i < currentColliders3D.Length; i++)
        {
            Destroy(currentColliders3D[i]);

        }*/

        MeshCollider currentCollider = GetComponent<MeshCollider>();
        Destroy(currentCollider);

        //hedges.gameObject.GetComponents<MeshCollider>()

        CalculateMeshOutlines();

        List<Vector3> hedgesVertices = new List<Vector3>();
        List<int> hedgesTriangles = new List<int>();
        Mesh hedgesMesh = new Mesh();
        hedgesMesh.name = "hedgesMesh";

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = hedgesVertices.Count;
                hedgesVertices.Add(vertices[outline[i]]); // left
                hedgesVertices.Add(vertices[outline[i + 1]]); // right
                hedgesVertices.Add(vertices[outline[i]] - Vector3.up * hedgesHeight); // bottom left
                hedgesVertices.Add(vertices[outline[i + 1]] - Vector3.up * hedgesHeight); // bottom right

                hedgesTriangles.Add(startIndex + 0);
                hedgesTriangles.Add(startIndex + 2);
                hedgesTriangles.Add(startIndex + 3);

                hedgesTriangles.Add(startIndex + 3);
                hedgesTriangles.Add(startIndex + 1);
                hedgesTriangles.Add(startIndex + 0);
            }
        }

        //hedgesVertices.Reverse();
        hedgesMesh.vertices = hedgesVertices.ToArray();
        hedgesMesh.triangles = hedgesTriangles.ToArray();
        

        hedges.mesh = hedgesMesh;
        
        int tileAmount = 10;
        Vector2[] uvs = new Vector2[hedgesVertices.Count];

        if (showHedgeTexture)
        {
            ////is the issue when only the z-coordinate is changed between vertices on a wall? 
            //it might be z-fighting, but I think it's because of the order I applied the mesh in.
            for (int i = 0; i < hedgesMesh.vertices.Length; i++) // the render order is still messed up.
            {
                float x = hedgesMesh.vertices[i].x;

                if (i + 1 < hedgesMesh.vertices.Length && x == hedgesMesh.vertices[i + 1].x && i > 1 && hedgesMesh.vertices[i - 1].x == x)
                {
                    //Fix bug texture
                    x = hedgesMesh.vertices[i].z;

                }
                uvs[i] = new Vector2(x/tileAmount, hedgesMesh.vertices[i].y/tileAmount);
        
            }

            hedgesMesh.RecalculateBounds();

            hedgesMesh.RecalculateNormals();

            hedgesMesh.uv = uvs;

            //hedges.GetComponent<Renderer>().GetComponent<Material>().mainTexture = hedgesTexture;
        }

        MeshCollider hedgesCollider = gameObject.AddComponent<MeshCollider>();
        hedgesCollider = GameObject.Find("Hedges").AddComponent<MeshCollider>();

        //MeshCollider hedgesCollider = gameObject.GetComponent<MeshCollider>();
        hedgesCollider.sharedMesh = hedgesMesh;
        GameObject hedge = GameObject.Find("Hedges");

        /*hedge.GetComponent<NavMeshObstacle>().carving = true;
        hedge.GetComponent<NavMeshObstacle>().carveOnlyStationary = true;


        gameObject.GetComponent<NavMeshObstacle>().carving = true;
        gameObject.GetComponent<NavMeshObstacle>().carveOnlyStationary = true;

        GameObject testing = Instantiate(testPlane) as GameObject;

        testing.GetComponent<NavMeshObstacle>().carving = true;
        testing.GetComponent<NavMeshObstacle>().carveOnlyStationary = true;*/


        /*var dropZone = new List<Coord>();

        foreach (var c in survivingRooms[0].tiles)
        {
            if (!survivingRooms[0].edgeTiles.Contains(c))
                dropZone .Add(c);
        }

        var rnd = UnityEngine.Random.Range(0, dropZone.Count - 1);

        var pos = new Vector3(-width / 2 + dropZone[rnd].tileX
            , -2.2f
            , -height / 2 + dropZone[rnd].tileY);

        player.transform.position = pos;
*/
        //suggestion for placing player.
        /*
                Mesh mesh = new Mesh();
                groundFilter.mesh = mesh;
                int xSize = 20;
                int ySize = 20;
                Vector3[] groundVertices = new Vector3[(xSize + 1) * (ySize + 1)];

                for (int i = 0, y = 0; y <= ySize; y++)
                {
                    for (int x = 0; x <= xSize; x++, i++)
                    {
                        groundVertices[i] = new Vector3(x, y);
                    }
                }

                mesh.vertices = groundVertices;

                int[] triangles = new int[xSize * ySize * 6];
                for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
                    for (int x = 0; x < xSize; x++, ti += 6, vi++) {
                        triangles[ti] = vi;
                        triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                        triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                        triangles[ti + 5] = vi + xSize + 2;
                    }
                }
                mesh.triangles = triangles;
                */
        //ground.transform.position = new Vector3(ground.transform.position.x, ground.transform.position.y - hedgesHeight, ground.transform.position.z);
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }

    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }

        SimplifyMeshOutlines();
    }

    void SimplifyMeshOutlines()
    {
        for (int outlineIndex = 0; outlineIndex < outlines.Count; outlineIndex++)
        {
            List<int> simplifiedOutline = new List<int>();
            Vector3 dirOld = Vector3.zero;
            for (int i = 0; i < outlines[outlineIndex].Count; i++)
            {
                Vector3 p1 = vertices[outlines[outlineIndex][i]];
                Vector3 p2 = vertices[outlines[outlineIndex][(i + 1) % outlines[outlineIndex].Count]];
                Vector3 dir = p1 - p2;
                if (dir != dirOld)
                {
                    dirOld = dir;
                    simplifiedOutline.Add(outlines[outlineIndex][i]);
                }
            }
            outlines[outlineIndex] = simplifiedOutline;
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

}