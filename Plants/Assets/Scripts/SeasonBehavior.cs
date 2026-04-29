using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SeasonBehavior : MonoBehaviour
{
    public int[] seasons = new int[4]; // used for looping
    public string[] seasonNames = new string[4] {"Summer", "Fall", "Winter", "Spring"};
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

    public GameObject grounParent;
    public List<GameObject> extraGroundObjects = new List<GameObject>(); // second ground layer that changes color based on season, purely aesthetic
    private List<SpriteRenderer> extraGroundRenderers = new List<SpriteRenderer>();
    private Color startGroundColor; // for lerping
    private Color nextGroundColor; // for lerping

    public Coroutine currentLerpCoroutine;

    public GameObject[] sunlight_beams = new GameObject[2];
    public int maxAlpha = 20; // min is 0
    private int currentAlpha = 0; // alpha for sunlight, based on sunlight level and max alpha

    public GameObject[] wind_particles = new GameObject[3];
    public int maxWindAlpha = 50; // min is 0
    private int currentWindAlpha = 0; // alpha for wind particles, based on wind
    public GameObject windParent; // to jitter randomly for more dynamic wind effect
    public Transform windParentogPos; // original position to jitter around

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        populateGround();
        initMinMaxStates(); // create min/max states
        currSeason = Random.Range(0, seasons.Length); // pick random season to start
        seasonCreate(currSeason); // create the environment for the starting season
        updateStartStates(); // set the start states to the generated states for the season
        currSeason = (currSeason + 1) % seasons.Length; 
        seasonCreate(currSeason); // create the environment for the starting season again to generate the next states for lerping
        windParentogPos = windParent.transform; // set original position for wind jitter
        startDone = true;
    }

    public void populateGround() 
    {
        foreach (Transform child in grounParent.transform) 
        {
            foreach (Transform grandChild in child.transform) 
            {
                GameObject obj = grandChild.gameObject;
                extraGroundObjects.Add(obj);
                extraGroundRenderers.Add(obj.GetComponent<SpriteRenderer>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        seasonTimer += Time.deltaTime;
        updateTimer += Time.deltaTime;

        if (seasonTimer >= seasonLength) 
        {
            seasonTimer = 0f; // reset timer for next season
            updateStartStates(); // set the start states to the end states of the last season
            currSeason = (currSeason + 1) % seasons.Length; // loop seasons
            seasonCreate(currSeason); // create the environment for the new season
            StopCoroutine(currentLerpCoroutine); // stop the current lerp if it's still going
            WHScript.copyTemp(); // copy the new start states to the world handler so it can lerp from there
        }
    }

    public void updateCurrAlpha()
    {
        currentAlpha = Mathf.RoundToInt((WHScript.sunlight_Level - minSun) / (maxSun - minSun) * maxAlpha);
        currentAlpha = Mathf.Clamp(currentAlpha, 0, maxAlpha);

        foreach (GameObject beam in sunlight_beams) 
        {
            Color c = beam.GetComponent<SpriteRenderer>().color;
            c.a = currentAlpha / 255f; // convert to 0-1 range for alpha
            beam.GetComponent<SpriteRenderer>().color = c;
        }

        currentWindAlpha = Mathf.RoundToInt((WHScript.windSpeed - minWind) / (maxWind - minWind) * maxWindAlpha);
        currentWindAlpha = Mathf.Clamp(currentWindAlpha, 0, maxWindAlpha);

        foreach (GameObject wind in wind_particles) 
        {
            Color c = wind.GetComponent<SpriteRenderer>().color;
            c.a = currentWindAlpha / 255f; // convert to 0-1 range for alpha
            wind.GetComponent<SpriteRenderer>().color = c;
        }
    }

    public IEnumerator lerpSeason() 
    {
        updateTimer = 0f;

        while (updateTimer < transitionLength) 
        {
            yield return null; // wait for next frame
            // yield return new WaitForSeconds(updateInterval); // wait for the update interval

            float t = updateTimer / transitionLength;
            t = (1f - Mathf.Cos(t * Mathf.PI)) * 0.5f;

            WHScript.temperature = Mathf.Lerp(startTemp, nextTemp, t);
            WHScript.sunlight_Level = Mathf.Lerp(startSun, nextSun, t);
            WHScript.windSpeed = Mathf.Lerp(startWind, nextWind, t);
            WHScript.rain_Level = Mathf.Lerp(startRain, nextRain, t);
            WHScript.pollinator_Level = Mathf.Lerp(startPollinator, nextPollinator, t);

            WHScript.normalizeWorldStates();
            updateCurrAlpha();
            if (Random.value < .9f) 
            {
            windParent.transform.position = windParentogPos.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0f); // jitter wind parent for dynamic effect
            } else {
            windParent.transform.position = windParentogPos.position; // reset to original position most of the time to prevent excessive drifting
            }

            for (int i = 0; i < extraGroundRenderers.Count; i++) 
            {
                extraGroundRenderers[i].color = Color.Lerp(startGroundColor, nextGroundColor, t);
            }
        }
    }

    public void updateStartStates() 
    {
        startTemp = nextTemp;
        startSun = nextSun;
        startWind = nextWind;
        startRain = nextRain;
        startPollinator = nextPollinator;

        startGroundColor = nextGroundColor;

        //Debug.Log("current season: " + seasonNames[currSeason]);
    }

    public void seasonCreate(int index) 
    {
        //Debug.Log("Season Change: " + index);
        switch (index) 
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

        nextGroundColor = new Color(0.110f, 0.373f, 0.082f); // bright green for summer
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

        nextGroundColor = new Color(0.8f, 0.8f, 0.8f); // gray for winter
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

        nextGroundColor = new Color(0.420f, 0.753f, 0.329f); // lighter green for spring
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

        nextGroundColor = new Color(0.659f, 0.302f, 0.075f); // brown for fall
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
