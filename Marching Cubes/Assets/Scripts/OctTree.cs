using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctTree
{
    Vector3 pos;
    float size; 

    public OctTree(Vector3 pos, float size) 
    { 
        this.size = size;
        this.pos = pos;
    }

    public ChunkInf[] generateOctTree()
    {
        ChunkInf[] ci = new ChunkInf[8];
        ci[0] = new ChunkInf(pos - Vector3.one * size/2, pos, 0);
        return ci; 
    }
}

public struct ChunkInf
{
    Vector3 start;
    Vector3 end;
    int id; 

    public ChunkInf(Vector3 start, Vector3 end, int id)
    {
        this.start = start;
        this.end = end;
        this.id = id;
    }
}
