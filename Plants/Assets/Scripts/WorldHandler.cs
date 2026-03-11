using UnityEngine;
using System.Collections;

public class WorldHandler : MonoBehaviour
{
    // this script handles EVERYTHING
    // it will handle the list of plant object
    // it will have access to each plant’s structure and data
    // it will keep a list of fitness score
    // this script defines the world

    // this is the actual stats of the global states, aka numbers with meaning
    // for example: temperature is on a range of -100 to 100 degrees fahrenheit
    // but on a score scale (0 - 1), a 70 would be .85 normalized
    [Header("Global States")]
    public float temperature = 0;
    public float sunlight_Level = 0;
    public float windSpeed = 0;
    public float rain_Level = 0;
    public float oxygen_Level = 0;
    public float pollinator_Level = 0;

    // actual 0-1 scaled 
    [Header("State Level Normalized: Score")]
    public float tempScore;
    public float sunScore;
    public float windScore;
    public float rainScore;
    public float oxygenScore;
    public float pollinatorScore;

    [Header("State Ranges")]
    public float minTemp;
    public float maxTemp;
    public float minSun;
    public float maxSun;
    public float minWind;
    public float maxWind;
    public float minRain;
    public float maxRain;
    public float minOxygen;
    public float maxOxygen;
    public float minPollinator;
    public float maxPollinator;

    [Header("Plant List")]
    public GameObject flowerPrefab; // holder for flower 
    private int plantCount; // determined by locations
    public GameObject locationHolder; // predetermined spawn locations
    public GameObject[] plantLocationList; // holder for locations

    // holds all plant roots
    // will iterate through each child and add it to plantlist
    public GameObject plantHolderParent; 
    public GameObject[] plantList;
    public FlowerScript[] scriptList; // holds the flower script for each plant, to access their parameters and scores

    [Header("Generation")]
    public int generation;
    public float[] scoreList; // parallel to flowers, uses flower scores and world states to determine fitness score for each plant
    public GameObject best2; // best score
    public GameObject best1; // best score
    public string best1Seed;
    public string best2Seed;

    // index 2 - 6 are the 5 randomly chosen flowers
    // index 0 and 1 are the best of the 5 to create next generation
    public GameObject[] boxes;
    public GameObject boxesParent;
    public GameObject[] random5flowers;
    public int[] indicesOf5;
    public int bestIndex1;
    public int bestIndex2;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plantCount = locationHolder.transform.childCount;
        scoreList = new float[plantCount];
        boxes = new GameObject[5];
        random5flowers = new GameObject[5];
        indicesOf5 = new int[5];

        int i = 0;
        foreach (Transform child in boxesParent.transform)
        {
            boxes[i] = child.gameObject;
            i++;
        }

        populateLocation();

        populatePlantAndScriptList();

        initMinMaxStates();

        // random states
        createWorldStates();

        //generateSummer(); // generate summer for this gen
        
        generation = 1;
    }

    private void populatePlantAndScriptList(bool randomize = true)
    {
        plantList = new GameObject[plantCount];
        scriptList = new FlowerScript[plantCount];

        for (int i = 0; i < plantCount; i++)
        {
            plantList[i] = Instantiate(flowerPrefab, plantLocationList[i].transform.position, Quaternion.identity, plantHolderParent.transform);
            scriptList[i] = plantList[i].GetComponent<FlowerScript>();
            if (randomize) {
                scriptList[i].randomStart();
            } else
            {
                scriptList[i].controlStart(best1Seed, best2Seed);
            }
        }
    }

    private void populateLocation()
    {
        plantLocationList = new GameObject[plantCount];

        for (int i = 0; i < plantCount; i++)
        {
            plantLocationList[i] = locationHolder.transform.GetChild(i).gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // for testing, press space to restart from generation 0 with new random parameters
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetEvolution();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            NextGeneration(); 
        }
    }

    private void NextGeneration()
    {
        generation++;

        if (boxesParent.activeSelf == false)
        {
            boxesParent.SetActive(true);
        }

        best1.transform.position = new Vector3(110, -10, 0);
        best2.transform.position = new Vector3(110, -10, 0);

        calculateFitnessScores();

        chooseRandom5();

        pickBest2();

        getSeed(bestIndex1, bestIndex2);

        // destroy old plants
        StartCoroutine(DestroyOldPlants());
    }   

    private IEnumerator DestroyOldPlants()
    {
        yield return new WaitForSeconds(1f); // wait for 1 second before destroying old plants to allow time for best 2 to be shown

        foreach (GameObject plant in plantList)
        {
            Destroy(plant);
        }

        boxesParent.SetActive(false);
        best1.transform.position = new Vector3(110, -10, 0);
        best2.transform.position = new Vector3(110, -10, 0);
        populatePlantAndScriptList(false); // create new plants with seeds from best 2

        yield return null; // wait for next frame to ensure new plants are created before any further actions
    }

    private void pickBest2()
    {
        // reset search for best 2 among the 5 random flowers
        bestIndex1 = 0;
        bestIndex2 = -1; 
        // first pass to find bestIndex1
        for (int i = 0; i < random5flowers.Length; i++)
        {
            int indexInPlantList = indicesOf5[i];
            if (scoreList[indexInPlantList] > scoreList[indicesOf5[bestIndex1]])
            {
                bestIndex1 = i;
            }
        }

        // second pass to find bestIndex2, ensuring it's different from bestIndex1
        for (int i = 0; i < random5flowers.Length; i++)
        {
            if (i == bestIndex1) continue; // skip the best already chosen

            int indexInPlantList = indicesOf5[i];

            if (bestIndex2 == -1 || scoreList[indexInPlantList] > scoreList[indicesOf5[bestIndex2]])
            {
                bestIndex2 = i;
            }
        }

        // save seed of the best two
        getSeed(bestIndex1, bestIndex2);

        // set the best 2 boxes to the positions of the best 2 flowers
        best1.transform.position = random5flowers[bestIndex1].transform.position;
        Vector3 pos = best1.transform.position;
        pos.z = -1f; 
        best1.transform.position = pos;
        best2.transform.position = random5flowers[bestIndex2].transform.position;
        pos = best2.transform.position;
        pos.z = -1f; 
        best2.transform.position = pos;
    }

    private void getSeed(int p1, int p2)
    {
        best1Seed = scriptList[indicesOf5[p1]].seed;
        best2Seed = scriptList[indicesOf5[p2]].seed;
    }

    private void chooseRandom5()
    {
        int plantsNeeded = 5;
        int plantChosen = 0;

        for (int i = 0; i < plantCount && plantChosen < plantsNeeded; i++)
        {
            int plantsRemaining = plantCount - i;
            float chance = (plantsNeeded - plantChosen) / (float)plantsRemaining;

            if (Random.Range(0f, 1f) < chance)
            {
                random5flowers[plantChosen] = plantList[i];
                indicesOf5[plantChosen] = i;
                plantChosen++;
            }
        }

        for (int i = 0; i < random5flowers.Length; i++)
        {
            boxes[i].transform.position = random5flowers[i].transform.position;
        }
    }

    private void ResetEvolution()
    {
        generation = 0;
        
        foreach (FlowerScript script in scriptList)
        {
            script.RestartParams(); // call inner script reset function
        }

        createWorldStates(); // new world states for new evolution

        boxesParent.SetActive(false);
        best1.transform.position = new Vector3(110, -10, 0);
        best2.transform.position = new Vector3(110, -10, 0);
    }

    public void initMinMaxStates()
    {
        // based on faren, 20f is a hard freeze that would damage most plants, 110f causes rapid desiccation and death for most plants
        minTemp = 20f; 
        maxTemp = 110f;

        // based on hours of direct sun per day
        // less than 2 hours causes poor growth whereas more than 14 can cause leaf scorch
        minSun = 2f;
        maxSun = 14f;

        // based on MPH
        // 30mph causes physical tearing
        // 70mph is hurricane force/uprooting
        minWind = 30f;
        maxWind = 70f;

        // based on inches per week. 0 is drought and 5+ inches causes root rot/drowning
        minRain = 0.1f;
        maxRain = 5.0f;

        // based on percentage of atmospheric composition
        // below 5% causes poor growth and above 35% can cause oxidative damage to plant tissues
        minOxygen = 5.0f;
        maxOxygen = 35.0f;

        // amount of pollinators in the area, based on number of pollinator visits per day
        minPollinator = 1.0f;
        maxPollinator = 50.0f;
    }

    public void createWorldStates()
    {
        // for testing, generate random world states each generation
        temperature = Random.Range(minTemp, maxTemp);
        sunlight_Level = Random.Range(minSun, maxSun);
        windSpeed = Random.Range(minWind, maxWind);
        rain_Level = Random.Range(minRain, maxRain);
        oxygen_Level = Random.Range(minOxygen, maxOxygen);
        pollinator_Level = Random.Range(minPollinator, maxPollinator);

        normalizeWorldStates();
    }

    private void normalizeWorldStates()
    {
        // normalize to 0-1 score
        tempScore = (temperature - minTemp) / (maxTemp - minTemp);
        sunScore = (sunlight_Level - minSun) / (maxSun - minSun);
        windScore = (windSpeed - minWind) / (maxWind - minWind);
        rainScore = (rain_Level - minRain) / (maxRain - minRain);
        oxygenScore = (oxygen_Level - minOxygen) / (maxOxygen - minOxygen);
        pollinatorScore = (pollinator_Level - minPollinator) / (maxPollinator - minPollinator);
    }

    public void calculateFitnessScores()
    {
        // weights that determine importance of each factor in overall fitness, should add up to 1
        float pollinationWeight = 0.4f; // since pollination is the main purpose of flowers, it should have the highest weight
        float tempWeight        = 0.3f;
        float windWeight        = 0.2f;
        float waterWeight       = 0.1f;

        for (int i = 0; i < plantCount; i++)
        {
            FlowerScript f = scriptList[i];

            float pollinationFitness = f.pollinatorAttractScore * pollinatorScore;

            float tempMismatch = Mathf.Abs(f.tempResistanceScore - tempScore);
            float tempFitness = 1f - tempMismatch; // higher mismatch means lower fitness, so we subtract from 1
            tempFitness = Mathf.Clamp01(tempFitness); // ensure 0-1

            // similar to temp, resistance should match levels
            float windMismatch = Mathf.Abs(f.windResistanceScore - windScore);
            float windFitness = 1f - windMismatch;
            windFitness = Mathf.Clamp01(windFitness);

            // same for above
            float waterMismatch = Mathf.Abs(f.waterSheddingScore - rainScore);
            float waterFitness = 1f - waterMismatch;
            waterFitness = Mathf.Clamp01(waterFitness);

            // sum up then make it a 0-1 scale
            float totalWeight = pollinationWeight + tempWeight + windWeight + waterWeight;

            float overallFitness = 
                (pollinationFitness * pollinationWeight +
                tempFitness * tempWeight +
                windFitness * windWeight +
                waterFitness * waterWeight) / totalWeight;

            // save in list
            scoreList[i] = overallFitness;

        }

    }

    private void generateSummer()
    {
        temperature = Random.Range(minTemp + (maxTemp - minTemp) * 0.6f, maxTemp * 0.8f);
        sunlight_Level = Random.Range(minSun + (maxSun - minSun) * 0.6f, maxSun);
        windSpeed = Random.Range(minWind, minWind + (maxWind - minWind) * 0.3f);
        rain_Level = Random.Range(minRain, minRain + (maxRain - minRain) * 0.3f);
        oxygen_Level = Random.Range(minOxygen + (maxOxygen - minOxygen) * 0.7f, maxOxygen);
        pollinator_Level = Random.Range(minPollinator + (maxPollinator - minPollinator) * 0.5f, maxPollinator);

        normalizeWorldStates(); // convert to 0-1
    }
}
