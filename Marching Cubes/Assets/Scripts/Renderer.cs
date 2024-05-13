using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renderer : MonoBehaviour
{
    Vector3Int prevNumChunks;
    Vector3Int prevNumDimensions;
    float prevSize;

    public Vector3Int dimensions = Vector3Int.one * 5;
    public float size = 1f;
    public ComputeShader shader;
    public ComputeShader shader2;
    public Vector3Int numChunks = Vector3Int.one;
    public GameObject chunkPrefab;
    public int dimensions2 = 5;
    int prevDim2; 

    float prevPM;
    float prevPH;
    public float perlinMul = .1f;
    public float perlinHeight = 1f;

    public int shape; 
    ValueGenerator vg;
    MarchingCubes mc;


    List<GameObject> chunkObjects;
    Point[] points;

    // Start is called before the first frame update

    void Start()
    {
        chunkObjects = new List<GameObject>();
        setValues();
        render();
    }


    private void Update()
    {
        if (prevNumChunks != numChunks
            || prevNumDimensions != dimensions
            || Mathf.Abs(prevSize - size) > 0.1f
            || Mathf.Abs(prevPM - perlinMul) > 0.0001f
            || Mathf.Abs(prevPH - perlinHeight) > 0.01f)
        {
            render();
            setValues();
        }

        if (dimensions2 != prevDim2)
        {
            dimensions = Vector3Int.one * dimensions2;
            render();
            setValues(); 
        }
    }

    void setValues()
    {
        prevNumChunks = numChunks;
        prevNumDimensions = dimensions;
        prevSize = size;
        prevPM = perlinMul;
        prevPH = perlinHeight;
        prevDim2 = dimensions2;
    }


    void render()
    {
        Vector3 currentPosition = transform.position;
        //points = new Vector3[dimensions * dimensions * dimensions];
        //values = new float[points.Length];

        //Vector3 midpoint = (new Vector3(0, 0, 0) + new Vector3(dimensions - 1, dimensions - 1, dimensions - 1)) / 2;

        //for (int y = 0, i = 0; y < dimensions; y++)
        //{
        //    for (int z = 0; z < dimensions; z++)
        //    {
        //        for (int x = 0; x < dimensions; x++)
        //        {
        //            values[i] = ((Vector3.Distance(new Vector3(x, y, z), midpoint)) - (float) dimensions / 2 + 2) + Perlin3D(x, y, z);
        //            points[i] = transform.TransformPoint(new Vector3(x, y, z) * size + transform.position);
        //            i++;

        //        }
        //    }
        //}

        if (vg == null)
        {
            vg = new ValueGenerator(shader2, shape);

        }
        vg.dim = dimensions;
        vg.size = size;
        vg.perlinMultiplier = perlinMul;
        vg.perlinHeight = perlinHeight;

        points = vg.compute();



        if (mc == null)
        {
            mc = new MarchingCubes(dimensions, shader, numChunks, points, size);
        }
        else
        {
            mc.setValues(dimensions, numChunks, points);
        }
        mc.MarchingCubesAlgorithmS();


        generateChunks(mc.chunkInfos);


        
        transform.position = currentPosition;
    }

    void generateChunks(ChunkInfo[] chunkInfos)
    {

        if (chunkObjects.Count != chunkInfos.Length)
        {

            instantiateChunks(chunkInfos);
        }



        for (int i = 0; i < chunkObjects.Count; i++)
        {
            StartCoroutine(createMeshes(chunkInfos[i], i));
        }


    }

    IEnumerator createMeshes(ChunkInfo chunkInfo, int i)
    {
        Chunk chunkScript = chunkObjects[i].GetComponent<Chunk>();
        chunkScript.createMesh(chunkInfo);
        yield return null;
    }

    void instantiateChunks(ChunkInfo[] chunkInfos)
    {
        if (chunkObjects.Count != 0)
        {
            foreach (GameObject chunkObject in chunkObjects)
            {
                Destroy(chunkObject);
            }
            chunkObjects.Clear();
        }
        foreach (ChunkInfo chunkInfo in chunkInfos)
        {
            GameObject chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
            chunk.transform.SetParent(transform);
            chunkObjects.Add(chunk);
        }
    }

    private void OnDestroy()
    {
        if (mc == null) return;
        mc.Release();
    }
}
