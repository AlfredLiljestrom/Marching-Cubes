using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    Mesh mesh;
    public int chunkID;
    [SerializeField] int verticesCount = 0;
    [SerializeField] int triangleCount = 0;
    MeshCollider meshCollider; 


    public void createMesh(ChunkInfo ci)
    {
        
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        if (meshCollider == null)
            meshCollider = this.AddComponent<MeshCollider>();

        if (ci == null) return;
        chunkID = ci.chunkID;

        ci = filterEdges(ci); 

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
        Dictionary<Vector3, int> addedPoints = new Dictionary<Vector3, int> ();
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
            tri.Add(edgeOffset);
            addedPoints.Add(vertex, edgeOffset);
            edgeOffset++;
        }

        ChunkInfo res = new ChunkInfo(ci.chunkID);
        res.vertices = vert;
        res.triangles = tri;

        return res; 
    }

}
