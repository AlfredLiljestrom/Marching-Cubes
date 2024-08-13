using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;


public class MarchingCubes
{
    Vector3Int dim;

    ComputeShader shader;


    public ChunkInfo[] chunkInfos;
    Vector3Int numChunks;

    ComputeBuffer pointBuffer; 
    ComputeBuffer outputCubes;
    ComputeBuffer countBuffer;

    Point[] points;
    Vector3 size;




    public MarchingCubes(Vector3Int dim, 
       ComputeShader shader, Vector3Int numChunks, Point[] points, Vector3 size)
    {
        this.dim = dim;
        this.shader = shader;
        this.numChunks = numChunks;
        this.points = points; 
        this.size = size;
    }

    public void setValues(Vector3Int dim, Vector3Int numChunks, Point[] points)
    {
        this.dim = dim;
        this.numChunks = numChunks;
        this.points = points;
    }

    Triangle[] setupShader(Point[] points)
    {
        Release();
        int triCountS = (dim.x - 1) * (dim.y - 1) * (dim.z - 1) * 5;
        int sizeT = Marshal.SizeOf(typeof(Triangle));
        outputCubes = new ComputeBuffer(triCountS, sizeT, ComputeBufferType.Append);
        outputCubes.SetCounterValue(0);


        int size = Marshal.SizeOf(typeof(Point));
        pointBuffer = new ComputeBuffer(points.Length, size);

        pointBuffer.SetData(points);

        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        countBuffer.SetData(new int[] { 0 });

        int kernel = shader.FindKernel("ComputeMesh");
        shader.SetBuffer(kernel, "outputCubes", outputCubes);
        shader.SetBuffer(kernel, "CountBuffer", countBuffer);
        shader.SetBuffer(kernel, "dataPoints", pointBuffer); 

        shader.SetInt("dimX", dim.x);
        shader.SetInt("dimY", dim.y);
        shader.SetInt("dimZ", dim.z);

        shader.SetInt("chunkSizeX", numChunks.x);
        shader.SetInt("chunkSizeY", numChunks.y);
        shader.SetInt("chunkSizeZ", numChunks.z);

        int threadsX = Mathf.CeilToInt(dim.x / (float) 8);
        int threadsY = Mathf.CeilToInt(dim.y / (float) 8);
        int threadsZ = Mathf.CeilToInt(dim.z / (float) 8);
        shader.Dispatch(kernel, threadsX, threadsY, threadsZ);

        int[] triCount = { 0 };

        
        countBuffer.GetData(triCount);
        

        int numtriangles = triCount[0];

        Triangle[] triangles = new Triangle[numtriangles];
        outputCubes.GetData(triangles);

        
        return triangles;
    }

    public void MarchingCubesAlgorithmS()
    {

        Vector3 mid; 
        mid.x = (dim.x - 1) / size.x * 2;
        mid.y = (dim.y - 1) / size.y * 2;
        mid.z = (dim.z - 1) / size.z * 2;

        // Run the shader 
        Triangle[] triangles2 = setupShader(points);
        chunkInfos = new ChunkInfo[numChunks.x * numChunks.z * numChunks.y];

        

        foreach (var triangle in triangles2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (chunkInfos[triangle.chunkID] == null)
                    chunkInfos[triangle.chunkID] = new ChunkInfo(triangle.chunkID);

                Vector3 pos = (triangle[i] - mid);
                pos.x /= dim.x - 1;
                pos.y /= dim.y - 1;
                pos.z /= dim.z - 1;

                chunkInfos[triangle.chunkID].vertices.Add(pos);
                chunkInfos[triangle.chunkID].increment();
            }
        }

        stripChunkInfos(); 
    }

    void stripChunkInfos()
    {
        List<ChunkInfo> chunkInfoList = new List<ChunkInfo>();  
        foreach (var chunkInfo in chunkInfos)
        {
            if (chunkInfo == null) continue;

            if (chunkInfo.index > 0)
            {
                chunkInfoList.Add(chunkInfo);
            }
        }
        chunkInfos = chunkInfoList.ToArray();
    }

    public void Release()
    {
        if (outputCubes != null)
        {
            outputCubes.Release();
            outputCubes = null;
        }

        if (pointBuffer != null)
        {
            pointBuffer.Release();
            pointBuffer = null;
        }

        if (countBuffer != null)
        {
            countBuffer.Release();
            countBuffer = null;
        }

    }
}


struct Triangle
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
    public int chunkID; 

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

public class ChunkInfo
{
    public List<Vector3> vertices;
    public List<int>triangles;
    public int index;
    public int chunkID;

    public ChunkInfo(int chunkID)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        index = 0;
        this.chunkID = chunkID;
    }

    public void increment()
    {
        triangles.Add(index++);
    }


}
