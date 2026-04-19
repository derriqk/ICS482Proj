using UnityEngine;
using System.Collections;

public class WorldHandlerFinal : MonoBehaviour
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
    public float pollinator_Level = 0;

    // start states
    private float startTemp;
    private float startSun;
    private float startWind;
    private float startRain;
    private float startPollinator;

    // used to generate next (season), can lerp
    private float nextTemp;
    private float nextSun;
    private float nextWind;
    private float nextRain;
    private float nextPollinator;

    // actual 0-1 scaled 
    [Header("State Level Normalized: Score")]
    public float tempScore;
    public float sunScore;
    public float windScore;
    public float rainScore;
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
    public float minPollinator;
    public float maxPollinator;

    [Header("Plant List")]
    public GameObject[] plantList; // list of plants
    public FinalPlant[] plantScripts; // list of plant scripts for easy access to parameters and fitness scores
    public GameObject plantPrefab; // prefab for instantiating new plants
    public GameObject plantParent; // parent object for plants

    [Header("Location List")]
    public GameObject locationParent; // parent object for plant locations
    public GameObject[] locationList; // list of locations for plants to grow

    [Header("Generation Control")]
    public float[] fitnessScores; // list of fitness scores for each plant
    public int best1Index; // index of best plant
    public float[] best1seed;
    public int best2Index; // index of second best plant
    public float[] best2seed;
    private float gentimer = 0f;
    public float genspeed;
    private float seasonTimer = 0f;
    private int season; // 0 = summer, 1 = fall, 2 = winter, 3 = spring
    public float seasonDuration = 20f;
    public bool randomState = true;
    public bool auto = true;

    private float delay = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initMinMaxStates();
        season = 0; // start in summer
        generateSummer();
        //setWorldState();

        int count = locationParent.transform.childCount;
        fitnessScores = new float[count];
        plantList = new GameObject[count];
        plantScripts = new FinalPlant[count];
        locationSetup();

        GeneratePlants(); // first gen is always random
        StartCoroutine(getBestPlants(1f)); 
    }

    // Update is called once per frame
    void Update()
    {
        delay += Time.deltaTime;
        if (delay < 4f) return;

        // resets generation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            setWorldState();
            refreshPlants();
        }

        // next gen
        if (Input.GetKeyDown(KeyCode.N))
        {
            //setWorldState();
            NextGeneration();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            auto = !auto;
        }

        if (auto) gentimer += Time.deltaTime;
        if (gentimer >= genspeed)
        {
            NextGeneration();
            gentimer = 0f;
        }

        if (auto) seasonTimer += Time.deltaTime;
        if (seasonTimer >= seasonDuration)
        {
            season = (season + 1) % 4; // cycle through seasons
            switch (season)
            {
                case 0:
                    generateSummer();
                    break;
                case 1:
                    generateFall();
                    break;
                case 2:
                    generateWinter();
                    break;
                case 3:
                    generateSpring();
                    break;
            }
            if (randomState) setWorldState(); // random

            seasonTimer = 0f;
        }

    }

    private IEnumerator getBestPlants(float delay)
    {
        // first time is always 2f
        yield return new WaitForSeconds(delay);
        // find best and second best plant indices
        float best1Score = float.MinValue;
        float best2Score = float.MinValue;

        for (int i = 0; i < fitnessScores.Length; i++)
        {
            if (fitnessScores[i] > best1Score)
            {
                best2Score = best1Score;
                best2Index = best1Index;
                best1Score = fitnessScores[i];
                best1Index = i;
            }
            else if (fitnessScores[i] > best2Score)
            {
                best2Score = fitnessScores[i];
                best2Index = i;
            }
        }
        
        // Debug.Log(best1Index);
        // Debug.Log(best2Index);
        best1seed = new float[plantScripts[best1Index].seed.Length];
        best2seed = new float[plantScripts[best2Index].seed.Length];
        copyArray(plantScripts[best1Index].seed, best1seed);
        copyArray(plantScripts[best2Index].seed, best2seed);

    }

    public void copyArray(float[] source, float[] destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = source[i];
        }
    }

    // using script and now world states to get overall score
    public void EvalScore(FinalPlant p, int i)
    {
        //Debug.Log(p.flowerCount);

        float overallScore =
        windScore * (p.windResistanceScore + p.stabilityScore) +
        sunScore * (p.sunlightAbsorptionScore + p.lightCompetitionScore) +
        tempScore * p.tempResistanceScore +
        rainScore * (p.waterSheddingScore + p.waterStressScore + p.energyStressScore) +
        pollinatorScore * p.pollinatorAttractScore * 1.2f;

        fitnessScores[i] = overallScore;
    }

    public void refreshPlants()
    {
        for (int i = 0; i < plantList.Length; i++)
        {
            plantScripts[i].clearPlant();
            plantScripts[i].randomGeneration();
        }

        StartCoroutine(getBestPlants(0f)); 
    }

    public void GeneratePlants()
    {
        int startZ = locationList.Length * -1;
        
        for (int i = 0; i < locationList.Length; i++)
        {
            GameObject newPlant = Instantiate(plantPrefab, locationList[i].transform.position, Quaternion.identity);
            plantList[i] = newPlant;
            plantScripts[i] = newPlant.GetComponent<FinalPlant>();
            plantScripts[i].worldHandler = this; // set reference to world handler in plant script
            plantScripts[i].index = i; // set index in plant script for score updating
            newPlant.transform.parent = plantParent.transform;
            Vector3 pos = newPlant.transform.position;
            pos.z = startZ + i;
            newPlant.transform.position = pos;
        }
    }

    public void NextGeneration()
    {
        for (int i = 0; i < plantList.Length; i++)
        {
            plantScripts[i].seededGeneration(best1seed, best2seed);
        }

        StartCoroutine(getBestPlants(0f));
    }

    public void setWorldState()
    {
        
        createWorldStates();
        normalizeWorldStates();
    }

    public void locationSetup()
    {
        locationList = new GameObject[locationParent.transform.childCount];
        int index = 0;
        foreach (Transform child in locationParent.transform)
        {
            locationList[index] = child.gameObject;
            index++;
        }
    }

    public void createWorldStates()
    {
        // for testing, generate random world states each generation
        temperature = Random.Range(minTemp, maxTemp);
        sunlight_Level = Random.Range(minSun, maxSun);
        windSpeed = Random.Range(minWind, maxWind);
        rain_Level = Random.Range(minRain, maxRain);
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
        pollinatorScore = (pollinator_Level - minPollinator) / (maxPollinator - minPollinator);
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

        // amount of pollinators in the area, based on number of pollinator visits per day
        minPollinator = 1.0f;
        maxPollinator = 50.0f;
    }

    private void generateSummer()
    {
        temperature = Random.Range(
            Mathf.Lerp(minTemp, maxTemp, 0.6f),
            maxTemp
        );
        
        sunlight_Level = Random.Range(
            Mathf.Lerp(minSun, maxSun, 0.5f),
            maxSun
        );

        windSpeed = Random.Range(
            Mathf.Lerp(minWind, maxWind, 0.2f),
            Mathf.Lerp(minWind, maxWind, 0.6f)
        );
      
        rain_Level = Random.Range(
            Mathf.Lerp(minRain, maxRain, 0.3f),
            Mathf.Lerp(minRain, maxRain, 0.8f)
        );
      
        pollinator_Level = Random.Range(
            Mathf.Lerp(minPollinator, maxPollinator, 0.5f),
            maxPollinator
        );

        normalizeWorldStates();
      
    }

    private void generateWinter()
    {
        temperature = Random.Range(
            minTemp,
            Mathf.Lerp(minTemp, maxTemp, 0.3f)
        );

        sunlight_Level = Random.Range(
            minSun,
            Mathf.Lerp(minSun, maxSun, 0.4f)
        );

        windSpeed = Random.Range(
            Mathf.Lerp(minWind, maxWind, 0.4f),
            maxWind
        );

        rain_Level = Random.Range(
            minRain,
            Mathf.Lerp(minRain, maxRain, 0.5f)
        );

        pollinator_Level = Random.Range(
            minPollinator,
            Mathf.Lerp(minPollinator, maxPollinator, 0.4f)
        );

        normalizeWorldStates();
}

    private void generateSpring()
    {
        temperature = Random.Range(
            Mathf.Lerp(minTemp, maxTemp, 0.4f),
            Mathf.Lerp(minTemp, maxTemp, 0.7f)
        );

        sunlight_Level = Random.Range(
            Mathf.Lerp(minSun, maxSun, 0.5f),
            Mathf.Lerp(minSun, maxSun, 0.8f)
        );

        windSpeed = Random.Range(
            Mathf.Lerp(minWind, maxWind, 0.2f),
            Mathf.Lerp(minWind, maxWind, 0.5f)
        );

        rain_Level = Random.Range(
            Mathf.Lerp(minRain, maxRain, 0.4f),
            Mathf.Lerp(minRain, maxRain, 0.8f)
        );

        pollinator_Level = Random.Range(
            Mathf.Lerp(minPollinator, maxPollinator, 0.6f),
            maxPollinator
        );

        normalizeWorldStates(); 
    }

    private void generateFall()
    {
        temperature = Random.Range(
            Mathf.Lerp(minTemp, maxTemp, 0.3f),
            Mathf.Lerp(minTemp, maxTemp, 0.6f)
        );

        sunlight_Level = Random.Range(
            Mathf.Lerp(minSun, maxSun, 0.4f),
            Mathf.Lerp(minSun, maxSun, 0.7f)
        );

        windSpeed = Random.Range(
            Mathf.Lerp(minWind, maxWind, 0.3f),
            Mathf.Lerp(minWind, maxWind, 0.7f)
        );

        rain_Level = Random.Range(
            Mathf.Lerp(minRain, maxRain, 0.2f),
            Mathf.Lerp(minRain, maxRain, 0.6f)
        );

        pollinator_Level = Random.Range(
            Mathf.Lerp(minPollinator, maxPollinator, 0.3f),
            Mathf.Lerp(minPollinator, maxPollinator, 0.7f)
        );

        normalizeWorldStates();
    }
}
