using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;


public class MarchingCubes2
{
    Vector3Int dim;

    ComputeShader shader;


    public ChunkInfo[] chunkInfos;

    ComputeBuffer pointBuffer;
    ComputeBuffer outputCubes;
    ComputeBuffer countBuffer;

    Point[] points;
    Vector3 size;




    public MarchingCubes2(Vector3Int dim,
       ComputeShader shader, Point[] points, Vector3 size)
    {
        this.dim = dim;
        this.shader = shader;
        this.points = points;
        this.size = size;
    }

    public void setValues(Vector3Int dim, Point[] points)
    {
        this.dim = dim;
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

        int threadsX = Mathf.CeilToInt(dim.x / (float)8);
        int threadsY = Mathf.CeilToInt(dim.y / (float)8);
        int threadsZ = Mathf.CeilToInt(dim.z / (float)8);
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
        chunkInfos = new ChunkInfo[1];



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



