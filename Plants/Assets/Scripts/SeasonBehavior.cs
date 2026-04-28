using UnityEngine;
using System.Collections;

public class SeasonBehavior : MonoBehaviour
{
    public int[] seasons = new int[4]; // used for looping
    public int currSeason; // 0 = summer, 1 = fall, 2 = winter, 3 = spring
    private float seasonTimer = 0f; // counts season changing
    public float seasonLength = 10f; // how long each season lasts in seconds
    public float transitionLength = 5f; // how long the transition between seasons lasts in seconds

    // due to fitness calc, update interval should not be every frame, but every few seconds, to give plants time to grow and react to the environment
    public float updateInterval = 1f; // how often to update the environment during transition (in seconds)
    private float updateTimer = 0f; 
    public WorldHandlerFinal WHScript; // holder for script

    // start states
    public float startTemp;
    public float startSun;
    public float startWind;
    public float startRain;
    public float startPollinator;

    // used to generate next (season), can lerp
    public float nextTemp;
    public float nextSun;
    public float nextWind;
    public float nextRain;
    public float nextPollinator;
    
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
    public bool startDone = false;

    public Coroutine currentLerpCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initMinMaxStates(); // create min/max states
        currSeason = Random.Range(0, seasons.Length); // pick random season to start
        seasonCreate(currSeason); // create the environment for the starting season
        updateStartStates(); // set the start states to the generated states for the season
        currSeason = (currSeason + 1) % seasons.Length; 
        seasonCreate(currSeason); // create the environment for the starting season again to generate the next states for lerping
        startDone = true;
    }

    // Update is called once per frame
    void Update()
    {
        seasonTimer += Time.deltaTime;
        updateTimer += Time.deltaTime;

        if (seasonTimer >= seasonLength) 
        {
            seasonTimer = 0f; // reset timer for next season
            currSeason = (currSeason + 1) % seasons.Length; // loop seasons
            updateStartStates(); // set the start states to the end states of the last season
            seasonCreate(currSeason); // create the environment for the new season
            StopCoroutine(currentLerpCoroutine); // stop the current lerp if it's still going
            WHScript.copyTemp(); // copy the new start states to the world handler so it can lerp from there
        }
    }

    public IEnumerator lerpSeason() 
    {
        updateTimer = 0f; // reset timer for next transition
        while (updateTimer < transitionLength) 
        {
            //Debug.Log("Lerping season");
            yield return new WaitForSeconds(updateInterval);

            WHScript.temperature = Mathf.Lerp(startTemp, nextTemp, updateTimer / transitionLength);
            WHScript.sunlight_Level = Mathf.Lerp(startSun, nextSun, updateTimer / transitionLength);
            WHScript.windSpeed = Mathf.Lerp(startWind, nextWind, updateTimer / transitionLength);
            WHScript.rain_Level = Mathf.Lerp(startRain, nextRain, updateTimer / transitionLength);
            WHScript.pollinator_Level = Mathf.Lerp(startPollinator, nextPollinator, updateTimer / transitionLength);

            WHScript.normalizeWorldStates();
        }

        yield break;
    }

    public void updateStartStates() 
    {
        startTemp = nextTemp;
        startSun = nextSun;
        startWind = nextWind;
        startRain = nextRain;
        startPollinator = nextPollinator;
    }

    public void seasonCreate(int index) 
    {
        //Debug.Log("Season Change: " + index);
        switch (index) 
        {
            case 0:
                Debug.Log("Summer");
                generateSummer();
                break;
            case 1:
                Debug.Log("Fall");
                generateFall();
                break;
            case 2:
                Debug.Log("Winter");
                generateWinter();
                break;
            case 3:
                Debug.Log("Spring");
                generateSpring();
                break;
        }
    }

    public void generateSummer() 
    {
        nextTemp = Random.Range(
            Mathf.Lerp(minTemp, maxTemp, 0.6f),
            maxTemp
        );
        
        nextSun = Random.Range(
            Mathf.Lerp(minSun, maxSun, 0.5f),
            maxSun
        );

        nextWind = Random.Range(
            Mathf.Lerp(minWind, maxWind, 0.2f),
            Mathf.Lerp(minWind, maxWind, 0.6f)
        );
      
        nextRain = Random.Range(
            Mathf.Lerp(minRain, maxRain, 0.3f),
            Mathf.Lerp(minRain, maxRain, 0.8f)
        );
      
        nextPollinator = Random.Range(
            Mathf.Lerp(minPollinator, maxPollinator, 0.5f),
            maxPollinator
        );
    }

    public void generateWinter() 
    {
        nextTemp = Random.Range(
            minTemp,
            Mathf.Lerp(minTemp, maxTemp, 0.4f)
        );
        
        nextSun = Random.Range(
            minSun,
            Mathf.Lerp(minSun, maxSun, 0.5f)
        );

        nextWind = Random.Range(
            Mathf.Lerp(minWind, maxWind, 0.4f),
            Mathf.Lerp(minWind, maxWind, 0.8f)
        );
      
        nextRain = Random.Range(
            minRain,
            Mathf.Lerp(minRain, maxRain, 0.5f)
        );
      
        nextPollinator = Random.Range(
            minPollinator,
            Mathf.Lerp(minPollinator, maxPollinator, 0.5f)
        );
    }

    public void generateSpring() 
    {
        nextTemp = Random.Range(
            Mathf.Lerp(minTemp, maxTemp, 0.4f),
            Mathf.Lerp(minTemp, maxTemp, 0.6f)
        );
        
        nextSun = Random.Range(
            Mathf.Lerp(minSun, maxSun, 0.4f),
            Mathf.Lerp(minSun, maxSun, 0.6f)
        );

        nextWind = Random.Range(
            Mathf.Lerp(minWind, maxWind, 0.3f),
            Mathf.Lerp(minWind, maxWind, 0.7f)
        );
      
        nextRain = Random.Range(
            Mathf.Lerp(minRain, maxRain, 0.4f),
            Mathf.Lerp(minRain, maxRain, 0.7f)
        );
      
        nextPollinator = Random.Range(
            Mathf.Lerp(minPollinator, maxPollinator, 0.4f),
            Mathf.Lerp(minPollinator, maxPollinator, 0.6f)
        );
    }

    public void generateFall() 
    {
        nextTemp = Random.Range(
            Mathf.Lerp(minTemp, maxTemp, 0.3f),
            Mathf.Lerp(minTemp, maxTemp, 0.5f)
        );
        
        nextSun = Random.Range(
            Mathf.Lerp(minSun, maxSun, 0.3f),
            Mathf.Lerp(minSun, maxSun, 0.5f)
        );

        nextWind = Random.Range(
            Mathf.Lerp(minWind, maxWind, 0.4f),
            Mathf.Lerp(minWind, maxWind, 0.8f)
        );
      
        nextRain = Random.Range(
            minRain,
            Mathf.Lerp(minRain, maxRain, 0.5f)
        );
      
        nextPollinator = Random.Range(
            minPollinator,
            Mathf.Lerp(minPollinator, maxPollinator, 0.5f)
        );
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
