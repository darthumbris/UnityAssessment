using UnityEngine;
using System.Collections.Generic;

class Quad {
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
    public Vector3 d;

    public Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }
}

//TODO make the textures tile properly depending on the w and h
public class Grid : MonoBehaviour
{
    [Range(10, 250)]
    public int w = 50;
    [Range(10, 250)]
    public int h = 50;
    public Material terrainMaterial;
    public Material edgeMaterial;
    public Material floorMaterial;

    Cell[,] grid;

    void Start() {
        grid = new Cell[this.h, this.w];
        gameObject.transform.position = new Vector3(-this.w / 2, 0, - this.h / 2);
        MeshFilter meshFilterGrid = gameObject.GetComponent<MeshFilter>() ? gameObject.GetComponent<MeshFilter>() : gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRendererGrid =  gameObject.GetComponent<MeshRenderer>() ? gameObject.GetComponent<MeshRenderer>() :   gameObject.AddComponent<MeshRenderer>();

        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.position = new Vector3(-this.w / 2, 0, - this.h / 2);
        edgeObj.transform.SetParent(transform);
        MeshFilter meshFilterEdge = edgeObj.AddComponent<MeshFilter>();
        MeshRenderer meshRendererEdge = edgeObj.AddComponent<MeshRenderer>();
        
        GameObject floorObj = new GameObject("Floor");
        floorObj.transform.position = new Vector3(-this.w / 2, 0, - this.h / 2);
        floorObj.transform.SetParent(transform);
        MeshFilter meshFilterFloor = floorObj.AddComponent<MeshFilter>();
        MeshRenderer meshRendererFloor = floorObj.AddComponent<MeshRenderer>();

        GenerateMaze();
    }

    void OnValidate() {
        grid = new Cell[this.h, this.w];
        gameObject.transform.position = new Vector3(-this.w / 2, 0, - this.h / 2);
        GameObject edgeObj = GameObject.Find("Edge");
        if (edgeObj)
            edgeObj.transform.position = new Vector3(-this.w / 2, 0, - this.h / 2);
        GameObject floorObj = GameObject.Find("Floor");
        if (floorObj)
            floorObj.transform.position = new Vector3(-this.w / 2, 0, - this.h / 2);
        GenerateMaze();
    }

    void Update() {
        GameObject edgeObj = GameObject.Find("Edge");
        GameObject floorObj = GameObject.Find("Floor");
        if(!Application.isPlaying || !edgeObj || !floorObj) return;
        DrawTerrainMesh(ref grid);
        DrawEdgeMesh(ref grid);
    }

    void GenerateMaze() {
        for (int i = 0; i < this.h; i++) {
            for (int j = 0; j < this.w; j++) {
                Cell cell = new Cell();
                cell.type = MazeTerrain.EMPTY;
                grid[i, j] = cell;
            }
        }
        AddMazeWalls(true, new Vector2Int(1, 1), new Vector2Int(this.w - 2, this.h - 2));
        AddBorderWalls();
        AddEntrance();
    }

    //For generating the walls at the outside of the maze
    void AddBorderWalls() {
        for (int i = 0; i < this.h; i++) {
            if (i == 0 || i == this.h - 1) {
                for (int j = 0; j < this.w; j++) {
                    Cell cell = new Cell();
                    cell.type = MazeTerrain.BORDER;
                    grid[i, j] = cell;
                }
            }
            else {
                Cell cell = new Cell();
                cell.type = MazeTerrain.BORDER;
                grid[i, 0] = cell;
                grid[i, this.w - 1] = cell;
            }
        }
    }

    //For generating the start of the maze at one of the borderWalls
    void AddEntrance() {
        int wall = Random.Range(1, 5);
        Cell cell = new Cell();
        cell.type = MazeTerrain.ENTRANCE;
        int i = 0;
        int j = 0;
        int x = 1;
        int y = 1;
        int rotation = 0;
        float cX = 0;
        float cY = 0;
        switch (wall)
        {
            case 1: // South Wall
                j = Random.Range(1, this.w - 1);
                x = j;
                i = this.h - 1;
                cY = this.h + 0.5f;
                cX = j;
                y = this.h - 2;
                rotation = 180;
                break;
            case 2: // North Wall
                j = Random.Range(1, this.w - 1);
                x = j;
                cY = -1.5f;
                cX = j;
                break;
            case 3: // East Wall
                i = Random.Range(1, this.h - 1);
                y = i;
                j = this.w - 1;
                x = this.w - 2;
                rotation = -90;
                cY = i;
                cX = this.w + 0.5f;
                break;
            case 4: // West Wall
                i = Random.Range(1, this.h - 1);
                y = i;
                rotation = 90;
                cY = i;
                cX = -1.5f;
                break;
            default:
                break;
        }
        grid[i, j] = cell;
        Cell empty = new Cell();
        empty.type = MazeTerrain.EMPTY;
        grid[y, x] = empty; //This is to prevent a wall being placed directly in front of entrance

        //This is to place the camera at the entrance
        GameObject camera = GameObject.Find("Camera");
        camera.transform.position = new Vector3(-this.w / 2 + cX, -0.5f, -this.h / 2 + cY);
        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0, rotation, 0);
        camera.transform.rotation = rot;
    }

    void AddMazeWalls(bool hor, Vector2Int min, Vector2Int max) {
        if (hor) {
            if (max.x - min.x < 2)
                return;
            int i = (Random.Range(min.y, max.y) / 2) * 2;
            int door = (Random.Range(min.x, max.x) / 2) * 2 + 1;
            for (int j = min.x; j <= max.x; j++) {
                Cell cell = new Cell();
                if (j == door) {
                    cell.type = MazeTerrain.EMPTY;
                }
                else {
                    cell.type = MazeTerrain.WALL;
                }
                grid[i , j] = cell;
            }

            AddMazeWalls(!hor, min, new Vector2Int(max.x, i - 1));
            AddMazeWalls(!hor, new Vector2Int(min.x, i + 1), max);
        }
        else {
            if (max.y - min.y < 2)
                return;

            int j = (Random.Range(min.x, max.x) / 2) * 2;
            int door = (Random.Range(min.y, max.y) / 2) * 2 + 1;
            for (int i = min.y; i <= max.y; i++) {
                Cell cell = new Cell();
                if (i == door) {
                    cell.type = MazeTerrain.EMPTY;
                }
                else {
                    cell.type = MazeTerrain.WALL;
                }
                grid[i,j] = cell;
            }

            AddMazeWalls(!hor, min, new Vector2Int(j - 1, max.y));
            AddMazeWalls(!hor, new Vector2Int(j + 1, min.y), max);
        }
    }

    void DrawTerrainMesh(ref Cell[,] grid) {
        Mesh wallMesh = new Mesh();
        wallMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //This for mazes bigger than ~ 90 * 90
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        List<Vector2> wallUvs = new List<Vector2>();

        Mesh floorMesh = new Mesh();
        floorMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //This for mazes bigger than ~ 90 * 90
        List<Vector3> floorVertices = new List<Vector3>();
        List<int> floorTriangles = new List<int>();
        List<Vector2> floorUvs = new List<Vector2>();
        for (int i = 0; i < this.h; i++) {
            for (int j = 0; j < this.w; j++) {
                Cell cell = grid[i,j];
                if (cell.type == MazeTerrain.WALL || cell.type == MazeTerrain.BORDER) {
                    Vector3 a = new Vector3(j - 0.5f, 0, i + 0.5f);
                    Vector3 b = new Vector3(j + 0.5f, 0, i + 0.5f);
                    Vector3 c = new Vector3(j - 0.5f, 0, i - 0.5f);
                    Vector3 d = new Vector3(j + 0.5f, 0, i - 0.5f);
                    Quad quad = new Quad(a, b, c, d);
                    AddQuad(quad, j, i, ref wallVertices, ref wallTriangles, ref wallUvs);
                }
                else {
                    Vector3 a = new Vector3(j - 0.5f, -1, i + 0.5f);
                    Vector3 b = new Vector3(j + 0.5f, -1, i + 0.5f);
                    Vector3 c = new Vector3(j - 0.5f, -1, i - 0.5f);
                    Vector3 d = new Vector3(j + 0.5f, -1, i - 0.5f);
                    Quad quad = new Quad(a, b, c, d);
                    AddQuad(quad, j, i, ref floorVertices, ref floorTriangles, ref floorUvs);
                }
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.uv = wallUvs.ToArray();
        wallMesh.RecalculateNormals();

        floorMesh.vertices = floorVertices.ToArray();
        floorMesh.triangles = floorTriangles.ToArray();
        floorMesh.uv = floorUvs.ToArray();
        floorMesh.RecalculateNormals();

        MeshFilter meshFilterGrid = gameObject.GetComponent<MeshFilter>();
        if (meshFilterGrid)
            meshFilterGrid.mesh = wallMesh;
        MeshRenderer meshRendererGrid = gameObject.GetComponent<MeshRenderer>();
        meshRendererGrid.material = terrainMaterial;


        GameObject floorObj = GameObject.Find("Floor");
        MeshFilter meshFilterFloor = floorObj.GetComponent<MeshFilter>();
        if (meshFilterFloor)
            meshFilterFloor.mesh = floorMesh;
        MeshRenderer meshRendererFloor = floorObj.GetComponent<MeshRenderer>();
        meshRendererFloor.material = floorMaterial;
    }

    void AddVerticesVert(int j, int i, float offset, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs) {
        Vector3 a = new Vector3(j + offset, 0, i + offset);
        Vector3 b = new Vector3(j - offset, 0, i + offset);
        Vector3 c = new Vector3(j + offset, -1, i + offset);
        Vector3 d = new Vector3(j - offset, -1, i + offset);
        Quad quad = new Quad(a, b, c, d);
        AddQuad(quad, j, i, ref vertices, ref triangles, ref uvs);
    }

    void AddVerticesHor(int j, int i, float offset, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs) {
        Vector3 a = new Vector3(j + offset, 0, i - offset);
        Vector3 b = new Vector3(j + offset, 0, i + offset);
        Vector3 c = new Vector3(j + offset, -1, i - offset);
        Vector3 d = new Vector3(j + offset, -1, i + offset);
        Quad quad = new Quad(a, b, c, d);
        AddQuad(quad, j, i, ref vertices, ref triangles, ref uvs);
    }

    void AddQuad(Quad q, int j, int i, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs) {
        Vector2 uvA = new Vector2(j / (float)this.w, i / (float)this.h);
        Vector2 uvB = new Vector2((j + 1) / (float)this.w, i / (float)this.h);
        Vector2 uvC = new Vector2(j / (float)this.w, (i + 1) / (float)this.h);
        Vector2 uvD = new Vector2((j + 1) / (float)this.w, (i + 1) / (float)this.h);
        Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
        Vector3[] v = new Vector3[] { q.a, q.b, q.c, q.b, q.d, q.c };
        for (int k = 0; k < 6; k++) {
            vertices.Add(v[k]);
            triangles.Add(triangles.Count);
            uvs.Add(uv[k]);
        }
    }

    void DrawEdgeMesh(ref Cell[,] grid) {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //This for mazes bigger than ~ 90 * 90
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for (int i = 0; i < this.h; i++) {
            for (int j = 0; j < this.w; j++) {
                Cell cell = grid[i,j];
                if (cell.type == MazeTerrain.WALL || cell.type == MazeTerrain.BORDER) {
                    if (j > 0) { //LEFT
                        Cell edge = grid[i, j - 1];
                        if (edge.type != MazeTerrain.WALL && edge.type != MazeTerrain.BORDER)
                            AddVerticesHor(j, i, -0.5f, ref vertices, ref triangles, ref uvs);
                    }
                    if (j < this.w - 1) { //RIGHT
                        Cell edge = grid[i, j + 1];
                        if (edge.type != MazeTerrain.WALL && edge.type != MazeTerrain.BORDER)
                            AddVerticesHor(j, i, +0.5f, ref vertices, ref triangles, ref uvs);
                    }
                    if (i > 0) {//DOWN
                        Cell edge = grid[i - 1, j];
                        if (edge.type != MazeTerrain.WALL && edge.type != MazeTerrain.BORDER)
                            AddVerticesVert(j, i, -0.5f, ref vertices, ref triangles, ref uvs);
                    }
                    if (i < this.h - 1) { //UP
                        Cell edge = grid[i + 1, j];
                        if (edge.type != MazeTerrain.WALL && edge.type != MazeTerrain.BORDER)
                            AddVerticesVert(j, i, +0.5f, ref vertices, ref triangles, ref uvs);
                    }
                    if (i == 0 || j == 0 || j == this.w - 1 || i == this.h - 1) { //BORDER
                        Cell edge = grid[i, j];
                        if (edge.type != MazeTerrain.ENTRANCE) {
                            AddVerticesVert(j, i, +0.5f, ref vertices, ref triangles, ref uvs);
                            AddVerticesVert(j, i, -0.5f, ref vertices, ref triangles, ref uvs);
                            AddVerticesHor(j, i, -0.5f, ref vertices, ref triangles, ref uvs);
                            AddVerticesHor(j, i, +0.5f, ref vertices, ref triangles, ref uvs);
                        }
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        GameObject edgeObj = GameObject.Find("Edge");

        MeshFilter meshFilter = edgeObj.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = edgeObj.GetComponent<MeshRenderer>();
        meshRenderer.material = edgeMaterial;
    }
}
