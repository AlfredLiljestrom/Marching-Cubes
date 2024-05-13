using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderGround : MonoBehaviour
{
    //int prevDim;

    //public float scale = 0.2f;
    //public float height = 1.0f;
    //public float scaleAnim = 0.5f;
    //public float speed = 100f;
    
    //public GameObject chunkPrefab;
    //List<GameObject> chunkObjects;
    //ValueGenerator vg;
    //MarchingCubes mc; 
    //Point[] points;

    //public int dimX = 10;
    //public int dimY = 10;
    //public int dimZ = 10;

    //public float perlinMul = .1f;
    //public float perlinHeight = 1f;

    //public int numChunksX = 1;
    //public int numChunksY = 1;
    //public int numChunksZ = 1;

    //bool init = false; 

    //public float size = 1f;
    //public ComputeShader shader;
    //public ComputeShader shader2;  

    //// Start is called before the first frame update
    //void Start()
    //{
    //    prevDim = dimX * dimY * dimZ;
    //    chunkObjects = new List<GameObject>();
    //    render();
    //}

    

    //void render()
    //{
    //    //Vector3[] points = new Vector3[dimX * dimY * dimZ];
    //    //float[] values = new float[points.Length];

    //    //for (int y = 0, i = 0; y < dimY; y++)
    //    //{
    //    //    for (int z = 0; z < dimZ; z++)
    //    //    {
    //    //        for (int x = 0; x < dimX; x++)
    //    //        {

    //    //            values[i] = (y - (Mathf.PerlinNoise(x * scale, z * scale) * height)) - scaleAnim;
    //    //            points[i] = transform.TransformPoint(new Vector3(x, y, z) * size + transform.position);
    //    //            i++;

    //    //        }
    //    //    }
    //    //}


    //    if (vg == null)
    //    {
    //        vg = new ValueGenerator(shader2, shape: 1);

    //    }

    //    vg.dimX = dimX;
    //    vg.dimY = dimY;
    //    vg.dimZ = dimZ;
    //    vg.size = size;
    //    vg.perlinMultiplier = perlinMul;
    //    vg.perlinHeight = perlinHeight;

    //    points = vg.compute();



    //    if (mc == null)
    //    {
    //        mc = new MarchingCubes(dimX, dimY, dimZ, shader, numChunksX, numChunksY, numChunksZ, points);
    //    }
    //    else
    //    {
    //        mc.setValues(dimX, dimY, dimZ, numChunksX, numChunksY, numChunksZ, points);
    //    }
    //    mc.MarchingCubesAlgorithmS();


    //    generateChunks(mc.chunkInfos);
    //}

    //void generateChunks(ChunkInfo[] chunkInfos)
    //{
        
    //    if (chunkObjects.Count != chunkInfos.Length)
    //    {

    //        instantiateChunks(chunkInfos);
    //    }

    //    for (int i = 0; i < chunkObjects.Count; i++)
    //    {
    //        StartCoroutine(createMeshes(chunkInfos[i], i));
    //    }
    //}

    //IEnumerator createMeshes(ChunkInfo chunkInfo, int i)
    //{
    //    Chunk chunkScript = chunkObjects[i].GetComponent<Chunk>();
    //    chunkScript.createMesh(chunkInfo);
    //    yield return null;
    //}

    //void instantiateChunks(ChunkInfo[] chunkInfos)
    //{
        
    //    if (chunkObjects.Count != 0)
    //    {
    //        foreach (GameObject chunkObject in chunkObjects)
    //        {
    //            Destroy(chunkObject);
    //        }
    //        chunkObjects.Clear();
    //    }
    //    foreach (ChunkInfo chunkInfo in chunkInfos)
    //    {
    //        GameObject chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
    //        chunk.transform.SetParent(transform);
    //        chunkObjects.Add(chunk);
    //    }
    //}
}
