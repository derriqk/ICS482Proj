using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldHandlerFinal : MonoBehaviour
{
     // this script handles EVERYTHING
    // it will handle the list of plant object
    // it will have access to each plant’s structure and data
    // it will keep a list of fitness score
    // this script defines the world

    [Header("Rates")]
    public float mutationRate;
    public float crossoverRate; // for breeding
    public float deviationAmount;
    public float randomGenome; // rlly low please, or -1 if unused
    public float retentionRate; // size of breeder pool
    private float currRetention;

    // example if wind is off, wind is not used in score calc
    [Header("Toggle State Scores")]
    public bool useTempScore;
    public bool useSunScore;
    public bool useWindScore;
    public bool useRainScore;
    public bool usePollinatorScore;

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
    public GameObject bestSpawn; // holder for best plant spawn location

    [Header("Generation Control")]
    public float[] fitnessScores; // list of fitness scores for each plant
    public int[] breederPool; // index list of the top retention
    public GameObject bestPlant;
    public FinalPlant bestPlantScript;
    public int[] fitnessSortedIndices; // sorted index list based on fitness scores
    public float[][] breederPoolSeeds; // seeds of breeder pool for easy access during breeding
    private float gentimer = 0f;
    public float genspeed = .25f;
    public bool auto = true;

    public bool randAuto = false;
    private float randTimer = 0f;
    public float randSpeed = 5f;

    private float delay = 0f;
    private int count;
    public PredeterminedFlowers predeterminedFlowers;
    public SeasonBehavior seasonBehavior;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(startLater());
    }

    private IEnumerator startLater() 
    {
        while (!seasonBehavior.startDone) 
        {
            yield return null; // wait until season behavior is done initializing
        }

        initMinMaxStates();

        copyTemp();
        
        // below for specific flower states
        //predeterminedFlowers.Lilacs();
        //predeterminedFlowers.Sunflower();

        count = locationParent.transform.childCount;
        fitnessScores = new float[count];
        plantList = new GameObject[count];
        plantScripts = new FinalPlant[count];
        setupBreederPool();
        locationSetup();

        //Debug.Log(breederPoolSeeds.Length);

        GeneratePlants(); // first gen is always random
        StartCoroutine(getBestPlants(1f)); 
    }

    public void copyTemp() 
    {
        temperature = seasonBehavior.startTemp;
        sunlight_Level = seasonBehavior.startSun;
        windSpeed = seasonBehavior.startWind;
        rain_Level = seasonBehavior.startRain;
        pollinator_Level = seasonBehavior.startPollinator;

        normalizeWorldStates();
        seasonBehavior.currentLerpCoroutine = StartCoroutine(seasonBehavior.lerpSeason());
    }

    public void setupBreederPool()
    {
        breederPool = new int[Mathf.CeilToInt(count * retentionRate)];
        breederPoolSeeds = new float[breederPool.Length][];

        for (int i = 0; i < breederPool.Length; i++)
        {
            breederPoolSeeds[i] = new float[31];
        }

        currRetention = retentionRate;
    }

    // Update is called once per frame
    void Update()
    {
        delay += Time.deltaTime;
        if (delay < 1f) return;

        // resets generation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            refreshPlants();
        }

        // next gen
        if (Input.GetKeyDown(KeyCode.N))
        {
            //setWorldState();
            if (currRetention != retentionRate)
            {
                // it means user has changed retention rate, so we need to update breeder pool size
                setupBreederPool();
            }
            NextGeneration();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            auto = !auto;
        }

        if (auto) gentimer += Time.deltaTime;
        if (gentimer >= genspeed)
        {
            if (currRetention != retentionRate)
            {
                // it means user has changed retention rate, so we need to update breeder pool size
                setupBreederPool();
            }
            NextGeneration();
            gentimer = 0f;
        }

        if (randAuto) randTimer += Time.deltaTime;
        if (randTimer >= randSpeed)
        {
            setWorldState();
            randTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            setWorldState();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            updateStates();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            for (int i = 0; i < plantList.Length; i++)
            {
                plantScripts[i].refreshPlant();
            }
        }
    }

    public void copyBestPlantToSpawner()
    {
        //Debug.Log(bestSpawn.transform.childCount);
        for (int i = bestSpawn.transform.childCount - 1; i >= 0; i--)
        {
            bestSpawn.transform.GetChild(i).gameObject.SetActive(false);
            Destroy(bestSpawn.transform.GetChild(i).gameObject);
        }

        GameObject best = Instantiate(
            plantList[breederPool[0]],
            bestSpawn.transform.position,
            Quaternion.identity
        );

        best.transform.SetParent(bestSpawn.transform);
    }

    public void sortFitnessScores()
    {
        fitnessSortedIndices = new int[fitnessScores.Length];
        float[] scoreCopy = new float[fitnessScores.Length];

        for (int i = 0; i < fitnessScores.Length; i++)
        {
            fitnessSortedIndices[i] = i;
            scoreCopy[i] = fitnessScores[i];
        }

        System.Array.Sort(scoreCopy, fitnessSortedIndices);
        System.Array.Reverse(fitnessSortedIndices);
    }

    public void createBreederPool()
    {
        sortFitnessScores();

        for (int i = 0; i < breederPool.Length; i++)
        {
            breederPool[i] = fitnessSortedIndices[i];

            float[] source = plantScripts[breederPool[i]].seed;
            breederPoolSeeds[i] = new float[source.Length];

            for (int j = 0; j < source.Length; j++)
            {
                breederPoolSeeds[i][j] = source[j];
            }
        }

        // updateBestPlant();

        copyBestPlantToSpawner();

        //printSeeds();
    }

    public void printSeeds()
    {
        Debug.Log(breederPoolSeeds.Length);
        for (int i = 0; i < breederPoolSeeds.Length; i++)
        {
            string seedStr = "Plant " + breederPool[i] + " Seed: ";
            for (int j = 0; j < breederPoolSeeds[i].Length; j++)
            {
                seedStr += breederPoolSeeds[i][j].ToString("F2") + " ";
            }
            Debug.Log(seedStr);
        }
    }

    public void updateStates()
    {
        temperature = tempScore * (maxTemp - minTemp) + minTemp;
        sunlight_Level = sunScore * (maxSun - minSun) + minSun;
        windSpeed = windScore * (maxWind - minWind) + minWind;
        rain_Level = rainScore * (maxRain - minRain) + minRain;
        pollinator_Level = pollinatorScore * (maxPollinator - minPollinator) + minPollinator;

        for (int i = 0; i < plantList.Length; i++)
        {
            plantScripts[i].mutationRate = mutationRate;
            plantScripts[i].crossoverRate = crossoverRate;
            plantScripts[i].deviationAmount = deviationAmount;
        }
    }

    private IEnumerator getBestPlants(float delay)
    {
        // first time is always 2f
        yield return new WaitForSeconds(delay);

        createBreederPool();
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
        float tempFit = Match(p.tempResistanceScore, tempScore);

        float sunFit = Match(
            (p.sunlightAbsorptionScore + p.lightCompetitionScore) * 0.5f,
            sunScore
        );

        float heightTrait = p.HeightTrait;

        // ideal height depends on wind:
        float idealHeight = 1f - windScore;

        float heightFit = 1f - Mathf.Abs(heightTrait - idealHeight);

        float windFit = Match(p.stabilityScore, windScore) * 0.5f
                    + heightFit * 0.5f;

        float rainFit = Match(
            (p.waterSheddingScore +
            p.waterStressScore +
            p.energyStressScore) / 3f,
            rainScore
        );

        float pollinatorReward =
            Mathf.Pow(pollinatorScore, 2f) * p.pollinatorAttractScore * 1.2f;

        float flowerMaintenanceCost =
            p.flowerCount * (1f - pollinatorScore) * 0.05f;

        float matchScore =
            (useTempScore ? tempFit : 0.01f) +
            (useSunScore ? sunFit : 0.01f) +
            (useWindScore ? windFit : 0.01f) +
            (useRainScore ? rainFit : 0.01f);

        float pollinatorScoreFinal =
            usePollinatorScore ? pollinatorReward : 0.01f;

        float overallScore =
            matchScore +
            pollinatorScoreFinal -
            flowerMaintenanceCost;

        fitnessScores[i] = overallScore;
    }

    float Match(float trait, float env)
    {
        return 1f - Mathf.Abs(trait - env);
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
            plantScripts[i].mutationRate = mutationRate;
            plantScripts[i].crossoverRate = crossoverRate;
            plantScripts[i].deviationAmount = deviationAmount;
            plantScripts[i].randomGenome = randomGenome;
            newPlant.transform.parent = plantParent.transform;
            Vector3 pos = newPlant.transform.position;
            pos.z = startZ + i;
            newPlant.transform.position = pos;
        }

        for (int i = 0; i < plantList.Length; i++)
        {
            plantScripts[i].randomGeneration();
        }

        //instantiateBestPlant();
        
    }

    public void instantiateBestPlant()
    {
        bestPlant = Instantiate(plantPrefab, bestSpawn.transform.position, Quaternion.identity);
        bestPlantScript = bestPlant.GetComponent<FinalPlant>();
        bestPlantScript.worldHandler = this;
        bestPlant.transform.parent = bestSpawn.transform;
        bestPlantScript.seed = new float[31];
        bestPlantScript.parameters = new float[11];
        bestPlantScript.leafParameters = new float[6];
        bestPlantScript.branchParameters = new float[6];
        bestPlantScript.flowerParameters = new float[8];
    }

    public void updateBestPlant()
    {
        //Debug.Log(bestPlantScript.seed.Length);
        for (int i = 0; i < bestPlantScript.seed.Length; i++)
        {
            bestPlantScript.seed[i] = breederPoolSeeds[0][i];
            //Debug.Log(bestPlantScript.seed[i]);
        }

        bestPlantScript.initParamsFromSeed(false);
        bestPlantScript.createFlowerTemplate();
        bestPlantScript.clearPlant();
        bestPlantScript.createStalk();
        //bestPlantScript.calculateScore();

        // work on for next wed
        // parameter flags and seasons (use sine waves)
    }

    public void NextGeneration()
    {
        for (int i = 0; i < plantList.Length; i++)
        {
            
            int randomP1 = Random.Range(0, breederPool.Length);
            int randomP2 = -1;
            do
            {
                randomP2 = Random.Range(0, breederPool.Length);
            } while (randomP2 == randomP1);

            plantScripts[i].seededGeneration(
                
                breederPoolSeeds[randomP1],
                breederPoolSeeds[randomP2]
            );
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

    public void normalizeWorldStates()
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

}
