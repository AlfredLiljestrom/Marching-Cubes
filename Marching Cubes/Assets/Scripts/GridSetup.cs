using System;
using System.Collections.Generic;
using UnityEngine;

public class GridSetup : MonoBehaviour
{

    [SerializeField] private int dimensionBase = 10;
    [SerializeField] private int dimensionHeight = 10;
    [SerializeField] private float spacing = 1f;
    [SerializeField][Range(-10f, 10f)] private float surface = 0f;
    [SerializeField] private bool interpolate = false; 

    private Point[] points;
    public float distVal = 0.0001f;
    float prevRad; 
    float[] values;
    private float[] prevValues;
    List<Vector3> vertices;
    List<int> triangles;

    public float perlin = 0.01f;
    public float radius = 5f; 
    public float height = 1f;
    int edgeOffset = 0;

    public ComputeShader shader;
    ComputeBuffer bufferFloatArray;
    ComputeBuffer bufferIntArray;
    ComputeBuffer outputCubes;
    ComputeBuffer countBuffer; 

    struct Triangle
    {
        public Vector3 a; 
        public Vector3 b;
        public Vector3 c;
        public float value; 

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

    Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;  
        prevRad = radius;
    }

    private void Update()
    {
        if (Mathf.Abs(radius - prevRad) > 0.01f)
        {
            setupGrid();
            MarchingCubes2();
            prevRad = radius;
        }
    }

    int[] setupCubeIndexes()
    {
        int[] ci = new int[(dimensionHeight - 1) * (dimensionBase - 1) * (dimensionBase -1)];
        for (int y = 0, i = 0, cubeIndex = 0; y < (dimensionHeight - 1); y++)
        {
            for (int z = 0; z < (dimensionBase - 1); z++)
            {
                for (int x = 0; x < (dimensionBase - 1); x++)
                {
                    ci[i] = cubeIndex; 
                    cubeIndex++;
                    i++;
                }
                cubeIndex++;
            }
            cubeIndex += dimensionBase;
        }
        return ci; 
    }

    Triangle[] setupShader(Vector3[] points, float[] values)
    {
        if (points.Length != values.Length)
        {
            Debug.Log("Different Lengths");
            return null;
        }

        int count = points.Length;
        bufferFloatArray = new ComputeBuffer(count, sizeof(float) * 3);
        bufferIntArray = new ComputeBuffer(values.Length, sizeof(float));

        int triCount = ((dimensionBase) * (dimensionBase) * (dimensionHeight) * 5);
        outputCubes = new ComputeBuffer(triCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);

        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);


        bufferFloatArray.SetData(points);
        bufferIntArray.SetData(values);

        int zero = 0;
        countBuffer.SetData(new int[] { zero });

        int kernel = shader.FindKernel("CSMain");

        shader.SetBuffer(kernel, "dataFloats", bufferFloatArray);
        shader.SetBuffer(kernel, "dataValues", bufferIntArray);
        shader.SetBuffer(kernel, "outputCubes", outputCubes);
        shader.SetBuffer(kernel, "CountBuffer", countBuffer);

        shader.SetInt("dimBase", dimensionBase);
        shader.SetInt("dimHeight", dimensionHeight);

        int dim = Mathf.CeilToInt(dimensionBase / (float) 8); 
        shader.Dispatch(kernel, dim, dim, dim);


        int numtriangles = ReadTriangleCount();
        Debug.Log(numtriangles);


        Triangle[] triangles = new Triangle[numtriangles];
        outputCubes.GetData(triangles);

        return triangles; 
    }

    int ReadTriangleCount()
    {
        int[] triCount = { 0 };
        ComputeBuffer.CopyCount(outputCubes, countBuffer, 0);
        countBuffer.GetData(triCount);
        return triCount[0];
    }

    void MarchingCubes2()
    {
        vertices.Clear();
        triangles.Clear();

        Debug.Log("Here1");
        // Run the shader 
        Triangle[] triangles2 = setupShader(extractPositions(points), values);
        Debug.Log("Here2");
        int index = 0;
        foreach (var triangle in triangles2)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 position = triangle[i];
                vertices.Add(position);
                triangles.Add(index++);
            }
        }
        Debug.Log("Here3");
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    Vector3[] extractPositions(Point[] points)
    {
        Vector3[] extractedPositions = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++) 
        {
            extractedPositions[i] = points[i].pos;
        }
        return extractedPositions;
    }
         
    

    void MarchingCubes()
    {
        vertices.Clear();
        triangles.Clear();
        edgeOffset = 0;

        for (int y = 0, cubeIndex = 0; y < (dimensionHeight - 1); y++)
        {
            for (int z = 0; z < (dimensionBase - 1); z++)
            {
                for (int x = 0; x < (dimensionBase - 1); x++)
                {
                    var pointsInCube = getCubePoints(cubeIndex);

                    if (pointsInCube == null)
                    {
                        return; 
                    }

                    var edgesInAlgorithm = setupEdges(pointsInCube);
                    updateMeshData(edgesInAlgorithm);
                    cubeIndex++; 
                }
                cubeIndex++; 
            }
            cubeIndex += dimensionBase;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void updateMeshData(Vector3[] edges)
    {
        if (edges.Length == 0) { return; }

        int[] tri = new int[edges.Length];
        List<Vector3> appendedEdges = new List<Vector3>();
        for (int i = 0; i < edges.Length; i++)
        {
            int index = findSameEdge(vertices, edges[i], distVal);

            if (index != -1)
            {
                tri[i] = index;
                
            }
            else
            {
                tri[i] = edgeOffset;
                edgeOffset++;
                appendedEdges.Add(edges[i]);
            } 
        }
        
        // Define vertices and triangles
        if (appendedEdges.Count > 0)
        {
            vertices.AddRange(appendedEdges.ToArray());
        }
     
        triangles.AddRange(tri);
    }

    int findSameEdge(List<Vector3> edges, Vector3 searchEdge, float distMax)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            float dist = Vector3.Distance(edges[i], searchEdge);

            if (dist < distMax)
            {
                return i;
            }
        }
        return -1;
    }

    void setupGrid()
    {
        transform.position = Vector3.zero;
        points = new Point[dimensionBase * dimensionBase * dimensionHeight];
        values = new float[points.Length];
        prevValues = new float[values.Length];

        Vector3 midpoint = (new Vector3(0, 0, 0) + new Vector3(dimensionBase - 1 ,dimensionHeight - 1,dimensionBase - 1)) / 2;

        for (int y = 0, i = 0; y < dimensionHeight; y++)
        {
            for (int z = 0; z < dimensionBase; z++)
            {
                for (int x = 0; x < dimensionBase; x++)
                {
                    values[i] = (Vector3.Distance(new Vector3(x, y, z), midpoint) * height) - radius;
                    points[i] = new Point(transform.TransformPoint(new Vector3(x, y, z) * spacing + transform.position), values[i] , Color.black); 
                    i++;
                        
                }
            }
        }
        copyValues();
    }

    void copyValues()
    {
        for (int i = 0; i < values.Length; i++)
        {
            prevValues[i] = values[i];
        }
    }


    Point[] getCubePoints(int cubeIndex)
    {
        Point[] cubePoints = new Point[8];
        cubePoints[0] = points[cubeIndex];
        cubePoints[1] = points[cubeIndex + 1];
        cubePoints[2] = points[cubeIndex + 1 + dimensionBase];
        cubePoints[3] = points[cubeIndex + dimensionBase];
        cubePoints[4] = points[cubeIndex + dimensionBase * dimensionBase];
        cubePoints[5] = points[cubeIndex + 1 + dimensionBase * dimensionBase];
        cubePoints[6] = points[cubeIndex + 1 + dimensionBase + dimensionBase * dimensionBase];
        cubePoints[7] = points[cubeIndex + dimensionBase + dimensionBase * dimensionBase];

        //foreach (point point in cubePoints)
        //{
        //    point.color = Color.blue; 
        //}

        return cubePoints;
    }

    Vector3[] setupEdges(Point[] pointsInCube)
    {

        int cubeIndex2 = 0;
        for (int i = 0; i < 8; i++)
        {
            if (pointsInCube[i].value < surface)
                cubeIndex2 |= (1 << i); 
        }


        int[] edgesFromTri = TriData.triTable[cubeIndex2]; 

        for (int i = 0; i < edgesFromTri.Length; i++)
        {
            if (edgesFromTri[i] == -1)
            {
                Array.Resize(ref edgesFromTri, i);
                break; 
            }
        }

        //for (int i = 0; i < edgesFromTri.Length; i++)
        //{
        //    edgesFromTri[i] += 12 * edgeOffset; 
        //}

        Vector3[] edges = new Vector3[12];
        edges[3] = interpolatedPoint(pointsInCube[3], pointsInCube[0]);
        edges[2] = interpolatedPoint(pointsInCube[2], pointsInCube[3]);
        edges[1] = interpolatedPoint(pointsInCube[1], pointsInCube[2]);
        edges[0] = interpolatedPoint(pointsInCube[0], pointsInCube[1]);


        edges[7] = interpolatedPoint(pointsInCube[7], pointsInCube[4]);
        edges[6] = interpolatedPoint(pointsInCube[6], pointsInCube[7]);
        edges[5] = interpolatedPoint(pointsInCube[5], pointsInCube[6]);
        edges[4] = interpolatedPoint(pointsInCube[4], pointsInCube[5]);

        edges[11] = interpolatedPoint(pointsInCube[3], pointsInCube[7]);
        edges[10] = interpolatedPoint(pointsInCube[2], pointsInCube[6]);
        edges[9] = interpolatedPoint(pointsInCube[1], pointsInCube[5]);
        edges[8] = interpolatedPoint(pointsInCube[0], pointsInCube[4]);


        List<Vector3> edgesInAlgorithm = new List<Vector3>();
        foreach (var index in edgesFromTri)
        {
            edgesInAlgorithm.Add(edges[index]); 
        }
        return edgesInAlgorithm.ToArray();
    }

    Vector3 interpolatedPoint(Point a, Point b)
    {
        a.value = Mathf.Clamp(a.value, -10f, 10f);
        b.value = Mathf.Clamp(b.value, -10f, 10f);
        if (a.value / b.value > 0f || !interpolate) 
        {
            return (a.pos + b.pos) / 2f;
        }
        float t = (surface - a.value) / (b.value - a.value);
        return a.pos + t * (b.pos - a.pos);
    }

    void drawCube(Point[] pointsInCube)
    {
        if (pointsInCube == null)
            return;
        Gizmos.color = Color.green;

        Gizmos.DrawLine(pointsInCube[0].pos, pointsInCube[1].pos);
        Gizmos.DrawLine(pointsInCube[1].pos, pointsInCube[2].pos); 
        Gizmos.DrawLine(pointsInCube[2].pos, pointsInCube[3].pos);
        Gizmos.DrawLine(pointsInCube[3].pos, pointsInCube[0].pos);

        Gizmos.DrawLine(pointsInCube[4].pos, pointsInCube[5].pos);
        Gizmos.DrawLine(pointsInCube[5].pos, pointsInCube[6].pos);
        Gizmos.DrawLine(pointsInCube[6].pos, pointsInCube[7].pos);
        Gizmos.DrawLine(pointsInCube[7].pos, pointsInCube[4].pos);


        Gizmos.DrawLine(pointsInCube[0].pos, pointsInCube[4].pos);
        Gizmos.DrawLine(pointsInCube[1].pos, pointsInCube[5].pos);
        Gizmos.DrawLine(pointsInCube[2].pos, pointsInCube[6].pos);
        Gizmos.DrawLine(pointsInCube[3].pos, pointsInCube[7].pos);
    }

    void Release()
    {
        bufferFloatArray.Release();
        bufferIntArray.Release();
        outputCubes.Release();
        countBuffer.Release();

        bufferFloatArray = null;
        bufferIntArray = null;
        outputCubes = null;
        countBuffer = null;
    }

    private void OnDestroy()
    {
        if (bufferFloatArray == null) { return; }
        Release();
    }


    //private void OnDrawGizmos()
    //{
    //    if (points == null) return;

    //    foreach (var point in points)
    //    {
    //        Gizmos.color = point.color;
    //        Gizmos.DrawSphere(point.pos, size);
    //    }

    //    //if (drawCubes) { drawCube(getCubePoints()); }


    //}

    internal struct Point
    {
        public Vector3 pos;
        public float value;
        public Color color; 

        public Point (Vector3 pos, float value, Color color)
        {
            this.pos = pos;
            this.value = value;
            this.color = color;
        }
    }
}


