using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk2 : MonoBehaviour
{
    public bool lookAt = false; 
    GameObject player; 
    Mesh mesh;
    public int chunkID;
    [SerializeField] int verticesCount = 0;
    [SerializeField] int triangleCount = 0;

    public float diste;

    public float distToRay;

    bool dov; 

    public List<float> vals = new List<float>();



    public Color color2 = Color.black;
    public float radius = 1f;

    Point[] points; 

    [SerializeField] Vector3[] t; 
    public int prevDim; 
    MeshCollider meshCollider;

    Vector3 mid;
    int originalDim; 

    public Vector3 start;
    public Vector3 end;
    public Vector3 midOfChunk; 
    Vector3Int dim;
    public float size; 

    public ComputeShader shader2;
    public ComputeShader shader; 
    ValueGenerator vg;

    ComputeBuffer pointBuffer;
    ComputeBuffer outputCubes;
    ComputeBuffer countBuffer;

    public float perlinMultiplier;
    public float perlinHeight;

    float prevpm;
    float prevph; 

    int shape; 

    public void setValues(Vector3 start, Vector3 end, Vector3Int dim, 
        float perlinMultiplier, float perlinHeight, int shape, GameObject player, float size, bool dov, Vector3 mid)
    {
        this.start = start;
        this.end = end;
        this.dim = dim;
        this.perlinMultiplier = perlinMultiplier;
        this.perlinHeight = perlinHeight;
        this.shape = shape;
        this.player = player;
        this.dov = dov;
        originalDim = dim.x;
        this.mid = mid; 
        this.size = size;
        prevDim = dim.x;

        prevpm = perlinMultiplier; 
        prevph = perlinHeight;
    }

    private void Update()
    {
        if (prevDim != dim.x || prevpm != perlinMultiplier || prevph != perlinHeight) 
        {
            dim = Vector3Int.one * prevDim;
            prevDim = dim.x; 
            prevpm = perlinMultiplier;
            prevph = perlinHeight;
            calculatePoints();
            MarchingCubesAlgorithmS(); 
        }

        diste = Vector3.Distance(midOfChunk, player.transform.position);
        
        if (shape == 0 && dov)
        {
            changeDimSphere();
        }
        else if (shape == 1 && dov)
        {
            changeDimPlane();
        }
    }

    void changeDimPlane()
    {
        if (100 > diste && diste >= 0)
        {
            prevDim = originalDim * 4;
        }
        else if (300 > diste && diste >= 100)
        {
            prevDim = originalDim;
        }
        else if (diste >= 300)
        {
            prevDim = originalDim / 4;
        }
    }

    void changeDimSphere()
    {
        if (50 > diste && diste >= 0)
        {
            prevDim = originalDim * 4;
        }
        else if (80 > diste && diste >= 50)
        {
            prevDim = originalDim;
        }
        else if (diste >= 80)
        {
            prevDim = originalDim / 2;
        }
    }

    public void Terraform(Vector3 pos, float power, float width, int dir)
    {
        for (int i = 0; i < points.Length; i++) 
        {
            float distance = Vector3.Distance(pos, points[i].pos) / (size / 100);
            float effect = (-((distance * distance) / width) + 1) * power;

            if (effect < 0)
                effect = 0; 

            points[i].value += effect * dir;
        }

        MarchingCubesAlgorithmS(); 
    }



    public void calculatePoints()
    {
        if (vg == null)
        {
            vg = new ValueGenerator(shader2, shape);

        }
        vg.start = start;
        vg.end = end;
        vg.dim = dim;
        vg.mid = mid;
        vg.size = size;
        vg.perlinMultiplier = perlinMultiplier;
        vg.perlinHeight = perlinHeight; 
        
        points = vg.compute();
    }

    

    Triangle[] MarchingCubes(Point[] points)
    {
        
        int triCountS = (dim.x - 1) * (dim.y - 1) * (dim.z - 1) * 5;
        int sizeT = Marshal.SizeOf(typeof(Triangle));
        outputCubes = new ComputeBuffer(triCountS, sizeT, ComputeBufferType.Append);
        outputCubes.SetCounterValue(0);


        int size = Marshal.SizeOf(typeof(Point));
        pointBuffer = new ComputeBuffer(points.Length, size);

        pointBuffer.SetData(points);

        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        countBuffer.SetData(new int[] { 0 });

        int kernel = shader.FindKernel("ComputeMesh2");
        shader.SetBuffer(kernel, "outputCubes", outputCubes);
        shader.SetBuffer(kernel, "CountBuffer", countBuffer);
        shader.SetBuffer(kernel, "dataPoints", pointBuffer);

        shader.SetInt("dimX", dim.x);
        shader.SetInt("dimY", dim.y);
        shader.SetInt("dimZ", dim.z);

        int threadsX = Mathf.CeilToInt(dim.x / (float)8);
        int threadsY = Mathf.CeilToInt(dim.y / (float)8);
        int threadsZ = Mathf.CeilToInt(dim.z / (float)8);
        shader.Dispatch(kernel, threadsX, threadsY, threadsZ);

        int[] triCount = { 0 };


        countBuffer.GetData(triCount);

        int numtriangles = triCount[0];

        Triangle[] triangles = new Triangle[numtriangles];
        outputCubes.GetData(triangles);

        Release();
        return triangles;
    }

    public void MarchingCubesAlgorithmS()
    {   
        
        Triangle[] triangles2 = MarchingCubes(points);

        if (triangles2.Length == 0)
            Destroy(gameObject);



        ChunkInfo ci = new ChunkInfo(1); 
        foreach (var triangle in triangles2)
        {
            for (int i = 0; i < 3; i++)
            {
                ci.vertices.Add(triangle[i]);
                ci.increment();
            }
        }

        createMesh(ci);
    }

    public void mark()
    {
        if (!lookAt)
            StartCoroutine(StartTimer(0.1f));
    }

    IEnumerator StartTimer(float duration)
    {
        // Set the value to true when the timer starts
        lookAt = true;

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Set the value to false when the timer ends
        lookAt = false;
    }


    private void OnDrawGizmos()
    {
        
        if (lookAt)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(midOfChunk, 5f);
        }

        //foreach(var p  in points)
        //{
        //    Gizmos.color = color2; 
        //    Gizmos.DrawSphere(p.pos, radius);
        //}

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(mid, 10f);
       
    }


    public void Release()
    {
        if (outputCubes != null)
        {
            outputCubes.Release();
            outputCubes.Dispose(); 
            outputCubes = null;
        }

        if (pointBuffer != null)
        {
            pointBuffer.Release();
            pointBuffer.Dispose();
            pointBuffer = null;
        }

        if (countBuffer != null)
        {
            countBuffer.Release();
            countBuffer.Dispose();
            countBuffer = null;
        }

    }

    public void createMesh(ChunkInfo ci)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        if (meshCollider == null)
            meshCollider = this.AddComponent<MeshCollider>();

        ci = filterEdges(ci);

        for (int i = 0; i < ci.vertices.Count; i++)
        {
            ci.vertices[i] = transform.InverseTransformPoint(ci.vertices[i]);
        }

        mesh.Clear();
        mesh.vertices = ci.vertices.ToArray();
        mesh.triangles = ci.triangles.ToArray();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;

        verticesCount = mesh.vertices.Length;
        triangleCount = mesh.triangles.Length;
    }

    ChunkInfo filterEdges(ChunkInfo ci)
    {
        midOfChunk = Vector3.zero; 
        Dictionary<Vector3, int> addedPoints = new Dictionary<Vector3, int>();
        List<Vector3> vert = new List<Vector3>();
        List<int> tri = new List<int>();

        int edgeOffset = 0;
        foreach (Vector3 vertex in ci.vertices)
        {
            if (addedPoints.ContainsKey(vertex))
            {
                int index = addedPoints.GetValueOrDefault(vertex);
                tri.Add(index);
                continue;
            }

            vert.Add(vertex);

            midOfChunk += vertex; 

            tri.Add(edgeOffset);
            addedPoints.Add(vertex, edgeOffset);
            edgeOffset++;
        }

        midOfChunk /= vert.Count;
        ChunkInfo res = new ChunkInfo(ci.chunkID);
        res.vertices = vert;
        res.triangles = tri;

        return res;
    }

}
