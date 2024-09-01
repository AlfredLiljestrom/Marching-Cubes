using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

public class GenerateChunks : MonoBehaviour
{

    public GameObject player;
    public int shape = 0;
    public float power = 1f;
    public float width = 0.1f; 

    public float rayDistance = 100f;

    public GameObject[,,] chunks;
    public GameObject chunkPrefab; 
    public ComputeShader shader2;
    public int dim;
    public float size;
    public float perlinMultiplier;
    public float perlinHeight;
    PrevValues pv;

    public bool dov = false; 


    public int numChunkX = 2, numChunkY = 2, numChunkZ = 2;


    private void Start()
    {
        updateChunks();

        pv = new PrevValues();
        assign();

    }

    void assign()
    {
        pv.dim = dim;
        pv.size = size;
        pv.numChunksX = numChunkX;
        pv.numChunksY = numChunkY;
        pv.numChunksZ = numChunkZ;
        pv.perlinMultiplier = perlinMultiplier;
        pv.perlinHeight = perlinHeight;
        pv.dov = dov;
    }

    bool checkUpdate()
    {
        if (pv.dim != dim ||
            pv.size != size ||
            pv.numChunksX != numChunkX ||
            pv.numChunksY != numChunkY ||
            pv.numChunksZ != numChunkZ ||
            pv.perlinMultiplier != perlinMultiplier ||
            pv.perlinHeight != perlinHeight ||
            pv.dov != dov)
        {
            return true; 
        }
        return false; 
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            int dir;
            if (Input.GetKey(KeyCode.Q))
                dir = 1;
            else
                dir = -1;
                
            Vector3 hitPos;
            if (sendRay(out hitPos))
            {
                List<GameObject> affectedChunkss = affectedChunks(hitPos);

                foreach (GameObject ac in affectedChunkss)
                {
                    ac.GetComponent<Chunk2>().Terraform(hitPos, power, width, dir);
                }
            }
        }
        


        if(checkUpdate())
        {
            destroyAll();
            assign();
            updateChunks();
        }
            
    }

    bool sendRay(out Vector3 hitPos)
    {
        // Step 1: Define the ray's origin and direction
        Vector3 rayOrigin = Camera.main.transform.position;
        Vector3 rayDirection = Camera.main.transform.forward;

        // Step 2: Perform the raycast
        Ray ray = new Ray(rayOrigin, rayDirection);
        RaycastHit hit;

        // Step 3: Check if the ray hits something within the specified distance
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if (hit.collider.tag == "Mesh")
            {
                Debug.DrawLine(rayOrigin, hit.point, Color.red);
                hitPos = hit.point;
                return true;
            }         
        }
        hitPos = Vector3.zero;  
        return false; 
    }

    List<GameObject> affectedChunks(Vector3 hitPos)
    {
        List<GameObject> cc = new List<GameObject>();


        // Hittar närmaste chunken 
        for (int i = 0; i < numChunkX; i++)
        {
            for (int j = 0; j < numChunkY; j++)
            {
                for (int k = 0; k < numChunkZ; k++)
                {
                    if (chunks[i, j, k] == null)
                        continue;

                    Vector3 pos = chunks[i, j, k].GetComponent<Chunk2>().midOfChunk;
                    Vector3 posStart = chunks[i, j, k].GetComponent<Chunk2>().start;

                    float distInChunk = (Vector3.Distance(pos, posStart) + 1) / (size / 100); 

                    float distance = Vector3.Distance(pos, hitPos)/ (size / 100);

                    if (distance > (Mathf.Sqrt(width) + distInChunk))
                        continue;

                    chunks[i, j, k].GetComponent<Chunk2>().distToRay = distance;
                    cc.Add(chunks[i, j, k]);
                }
            }
        }

        foreach (var chunk in cc)
        {
            chunk.GetComponent<Chunk2>().mark();
        }

        return cc;
    }

    

    List<GameObject> closestChunks(Vector3 hitPos)
    {
        List<GameObject> cc = new List<GameObject>();

        LinkedList ll = new LinkedList(100); 

        // Hittar närmaste chunken 
        for (int i = 0; i < numChunkX; i++)
        {
            for (int j = 0; j < numChunkY; j++)
            {
                for (int k = 0; k < numChunkZ; k++)
                {
                    if (chunks[i, j, k] == null)
                        continue; 

                    Vector3 pos = chunks[i, j, k].GetComponent<Chunk2>().midOfChunk;

                    float distance = Vector3.Distance(pos, hitPos);

                    chunks[i, j, k].GetComponent<Chunk2>().distToRay = distance;

                    ll.AddElement(distance, new Vector3Int(i, j, k));
                }
            }
        }

        List<ListElement> list = ll.extractList();

        foreach(var le in list)
        {
            cc.Add(chunks[le.index.x, le.index.y, le.index.z]); 
        }


        foreach (var chunk in cc) 
        {
            chunk.GetComponent<Chunk2>().mark(); 
        }

        return cc;
    }

    public class LinkedList
    {
        private int maxLength;
        public ListElement root = null;

        public LinkedList(int maxLength)
        {
            this.maxLength = maxLength;
        }

        public void AddElement(float distance, Vector3Int index)
        {
            ListElement current = root;
            ListElement prev = null;
            int i = 1;
            while (i <= maxLength)
            {
                if (current == null)
                {
                    current = new ListElement(distance, index);
                    
                    if (prev != null)
                    {
                        current.head = prev;
                    }
                    else
                    {
                        root = current;
                    }
                    break; 
                }

                if(current.distance > distance)
                {
                    ListElement le = new ListElement(distance, index); 

                    if (prev != null)
                    {
                        le.head = prev;
                        prev.tail = le;
                    }
                    else
                    {
                        root = le;
                    }
                        
                    le.tail = current;
                    
                    current.head = le; 
                    break; 
                    
                }

                prev = current;
                current = current.tail;
                i++;
            }
            
        }

        public List<ListElement> extractList()
        {
            List<ListElement> elements = new List<ListElement>();

            ListElement current = root; 
            for (int i = 0; i < maxLength; i++)
            {
                if (current == null)
                    break;
                elements.Add(current);
                current = current.tail;
            }

            return elements;
        }
    }

    public class ListElement
    {
        public ListElement head; 
        public ListElement tail;
        public float distance;
        public Vector3Int index;

        public ListElement(float distance, Vector3Int index)
        {
            this.distance = distance;
            this.index = index;
        }
    }

    private void OnDrawGizmos()
    {
         
    }

    void destroyAll()
    {
        foreach(var obj in chunks)
        {
            Destroy(obj);    
        }
    }


    void updateChunks()
    {
        chunks = new GameObject[numChunkX, numChunkY, numChunkZ]; 
        for (int i = 0; i < numChunkX; i++)
        {
            for (int j = 0; j < numChunkY; j++)
            {
                for (int k = 0; k < numChunkZ; k++)
                {
                    (Vector3, Vector3) ends = calculateStartEnd(i, j, k);
                    chunks[i, j, k] = Instantiate(chunkPrefab);
                    chunks[i, j, k].transform.SetParent(transform, false);
                    StartCoroutine(RenderChunk(chunks[i, j, k], ends.Item1, ends.Item2)); 
                }
            }
        }
    }

    (Vector3, Vector3) calculateStartEnd(int i, int j, int k)
    {
        float iScaled = (float) i / numChunkX;
        float jScaled = (float) j / numChunkY;
        float kScaled = (float) k / numChunkZ;

        float iScaled2 = (float) (i + 1) / numChunkX;
        float jScaled2 = (float) (j + 1) / numChunkY;
        float kScaled2 = (float) (k + 1) / numChunkZ;

        Vector3 start = new Vector3(iScaled, jScaled, kScaled) * size;
        Vector3 end = new Vector3(iScaled2, jScaled2, kScaled2) * size;

        return (transform.TransformPoint(start), transform.TransformPoint(end));
    }

    Vector3 calculateMidPoint()
    {
        return transform.TransformPoint(Vector3.one * size / 2f);
    }


    IEnumerator RenderChunk(GameObject c, Vector3 start, Vector3 end)
    {
        
        c.GetComponent<Chunk2>().setValues(start, end, Vector3Int.one * dim,
                        perlinMultiplier, perlinHeight, shape, player, size, dov, calculateMidPoint());

        c.GetComponent<Chunk2>().calculatePoints();
        c.GetComponent<Chunk2>().MarchingCubesAlgorithmS(); 
        yield return null;
    }

    struct PrevValues
    {
        public int dim;
        public int numChunksX;
        public int numChunksY;
        public int numChunksZ;
        public float size;
        public float perlinMultiplier;
        public float perlinHeight;
        public bool dov; 
    }
}
