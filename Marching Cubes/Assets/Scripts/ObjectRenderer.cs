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

    public GameObject cavePrefab;
    public bool createCave = false;


    public List<GameObject> gameObjects;

    private void Start()
    {
        gameObjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (createBall)
        {
            gameObjects.Add(Instantiate(ballPrefab, Vector3.zero, Quaternion.identity));
            createBall = false;
        }

        if (createGround)
        {
            gameObjects.Add(Instantiate(groundPrefab, Vector3.zero, Quaternion.identity));
            createGround = false;
        }

        if (createCave)
        {
            gameObjects.Add(Instantiate(cavePrefab, Vector3.zero, Quaternion.identity));
            createCave = false;
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
