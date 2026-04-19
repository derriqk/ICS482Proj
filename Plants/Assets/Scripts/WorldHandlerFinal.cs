using UnityEngine;

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
    public GameObject plantPrefab; // prefab for instantiating new plants
    public GameObject plantParent; // parent object for plants

    [Header("Location List")]
    public GameObject locationParent; // parent object for plant locations
    public GameObject[] locationList; // list of locations for plants to grow

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        locationSetup();
        setWorldState();
        GeneratePlants();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GeneratePlants()
    {
        plantList = new GameObject[locationList.Length];
        for (int i = 0; i < locationList.Length; i++)
        {
            GameObject newPlant = Instantiate(plantPrefab, locationList[i].transform.position, Quaternion.identity);
            plantList[i] = newPlant;
            newPlant.transform.parent = plantParent.transform;
        }
    }

    public void setWorldState()
    {
        initMinMaxStates();
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
}
