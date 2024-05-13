using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectRenderer : MonoBehaviour
{
    public GameObject ballPrefab;
    public bool createBall = false;

    public GameObject player;
    public bool spawnPlayer = false;

    public GameObject groundPrefab;
    public bool createGround = false;

    public List<GameObject> gameObjects;

    private void Start()
    {
        gameObjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (createBall )
        {
            GameObject go = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
            Renderer r = go.GetComponent<Renderer>();
            r.renderObject(0); 
            gameObjects.Add(go);

            createBall = false;
        }

        if (createGround)
        {
            GameObject go = Instantiate(groundPrefab, Vector3.zero, Quaternion.identity);
            Renderer r = go.GetComponent<Renderer>();
            r.renderObject(1);
            gameObjects.Add(go);

            createGround = false;
        }

        if (spawnPlayer)
        {
            Instantiate(player, Vector3.zero, Quaternion.identity); 
        }
    }

    public void updateObjectList()
    {
        foreach (GameObject obj in gameObjects)
        {
            if (obj == null)
            {
                gameObjects.Remove(obj);
            }
        }
    }

    public GameObject getClosestObject(Vector3 pos)
    {
        float minDist = float.MaxValue;
        int minIndex = -1;

        for (int i = 0; i < gameObjects.Count; i++)  
        {
            float dist = Vector3.Distance(gameObjects[i].transform.position, pos);
            if (dist < minDist)
            {
                minDist = dist;
                minIndex = i;
            }
        }
        return gameObjects[minIndex];
    }


}
