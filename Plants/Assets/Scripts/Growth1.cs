using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Growth1 : MonoBehaviour
{
    public GameObject plantParent;
    public GameObject plantStart;
    public GameObject plantPrefab;
    public List<GameObject> plantStages;
    public float growthTime = 1f; // intevral between growth
    private float timer;
    public int growthStage; // current growth stage

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plantStages.Add(plantStart); // add initial plant to stages
        growthStage = 1; // start at stage 1

        // start coroutine with interval
        StartCoroutine(Grow());
    }

    private IEnumerator Grow() // grow a 2d plant
    {

        float chance = Random.Range(0f, 1f); // random chance to grow

        while (chance < .8f)
        {
            yield return new WaitForSeconds(growthTime); 

            Vector3 prevPos = plantStages[growthStage - 1].transform.position; 

            GameObject newPlant = Instantiate(plantPrefab, transform.position, Quaternion.identity); // create new plant
            
            SpriteRenderer sr = plantStages[growthStage - 1].GetComponent<SpriteRenderer>(); 
            float location = sr.bounds.size.y; // get height of current stage

            Vector3 spawnPos = plantStages[growthStage - 1].transform.position + 
            plantStages[growthStage - 1].transform.up * location;
        
            // get rotation of previous stage and apply to new plant
            newPlant.transform.rotation = plantStages[growthStage - 1].transform.rotation;

            newPlant.transform.position = spawnPos; // set position of new plant

            newPlant.transform.Rotate(0, 0, Random.Range(-20f, 20f)); // add random rotation for natural look

            plantStages.Add(newPlant); // add new plant to stages

            newPlant.transform.SetParent(plantParent.transform); 
            
            growthStage++; // increment stage
            
            // really rare chance to start a new starting plant
            if (chance < .02f)
            {
                GameObject newStart = Instantiate(plantStart, spawnPos, plantStages[growthStage - 1].transform.rotation); // create new starting plant
                newStart.transform.SetParent(plantParent.transform); // set parent
                plantStages.Add(newStart); // add to stages
            }
            
            chance = Random.Range(0f, 1f); // update chance 
        }
        Debug.Log("stop"); // log stop message
        yield break; // end coroutine
    }
}
