using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



public class ValueGenerator
{
    ComputeBuffer valueBuffer;
    ComputeBuffer countBuffer;
    ComputeShader shader;

    public Vector3Int dim;
    //public Vector3 start; 
    //public Vector3 end;
    public Vector3 size;

    int shape;
    
    public float perlinMultiplier = .1f;
    public float perlinHeight = 3f; 
    Point[] points;

    

    public ValueGenerator(ComputeShader shader, int shape)
    {
        this.shader = shader;
        this.shape = shape;
    }

    Vector3 getMidpoint()
    {
        return size / 2f; 
    }


    public Point[] compute()
    {

        int count = countVector(dim);
        int sizePoint = Marshal.SizeOf(typeof(Point));
        valueBuffer = new ComputeBuffer(count, sizePoint);


        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        countBuffer.SetData(new int[] { 0 });
        shader.SetInts("dim", dim.x, dim.y, dim.z);
        
        shader.SetInt("shape", shape);
        shader.SetFloat("perlinMultiplier", perlinMultiplier);
        shader.SetFloat("perlinHeight", perlinHeight); 
        shader.SetFloats("size", size.x, size.y, size.z);  
        
        //shader.SetFloats("startPos", start.x, start.y, start.z);
        //shader.SetFloats("endPos", end.x, end.y, end.z);

        Vector3 mid = getMidpoint();
        shader.SetFloats("midPoint", mid.x, mid.y, mid.z); 

        int kernel = shader.FindKernel("ComputePoints");
        shader.SetBuffer(kernel, "outputPoints", valueBuffer);
        shader.SetBuffer(kernel, "CountBuffer", countBuffer);

        int threadsX = Mathf.CeilToInt(dim.x / (float)8);
        int threadsY = Mathf.CeilToInt(dim.y / (float)8);
        int threadsZ = Mathf.CeilToInt(dim.z / (float)8);
        shader.Dispatch(kernel, threadsX, threadsY, threadsZ);

        int[] pointCount = { 0 };
        countBuffer.GetData(pointCount);
        int numPoints = pointCount[0];

        points = new Point[numPoints];
        
        valueBuffer.GetData(points);

       
        Release();
        return points;

    }

    void Release()
    {
        try
        {
            valueBuffer.Release();
            countBuffer.Release();
            countBuffer.Dispose();
            valueBuffer.Dispose();
            valueBuffer = null;
            countBuffer = null;
        }
        catch
        { Debug.Log("Failed value"); }
        
    }

    int countVector(Vector3Int vector)
    {
        return vector.x * vector.y * vector.z;
    }

}

public struct Point
{
    public Vector3 pos;
    public float value;
}