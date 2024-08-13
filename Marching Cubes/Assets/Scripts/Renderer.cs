using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renderer : MonoBehaviour
{
    Vector3Int prevNumChunks;
    Vector3Int prevNumDimensions;
    Vector3 prevSize;

    public Vector3Int dimensions = Vector3Int.one * 5;
    public Vector3 size = Vector3.one * 10f;
    public ComputeShader shader;
    public ComputeShader shader2;
    public Vector3Int numChunks = Vector3Int.one;
    public GameObject chunkPrefab;
    public int dimensions2 = 5;
    public float size2 = 10; 
    int prevDim2;
    float prevSize2; 

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
            || Vector3.Distance(prevSize, size) > 0.1f
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

        if (Mathf.Abs(size2 - prevSize2) < 0.0001)
        {
            size = Vector3.one * size2;
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
        prevSize2 = size2;
    }


    void render()
    {
        Vector3 currentPosition = transform.position;
        

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
