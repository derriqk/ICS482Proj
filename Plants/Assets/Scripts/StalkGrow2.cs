using UnityEngine;
using System.Collections.Generic;

public class StalkGrow2 : MonoBehaviour
{
    // this script handles the stalk parameter space growth for my project
    // its intended to grow and branch based on parameters
    // at the end of each stalk it will instantiate the flower 

    [Header("Stalk Parameter Constants")]
    public const int stalk_color_hue = 0;
    public const int stalk_color_saturation = 1;
    public const int stalk_color_value = 2;
    public const int stalk_length = 3; // the segment length
    public const int stalk_width = 4; // the segment width
    public const int main_stalk_count = 5; // alays 1 to 3, or whatever
    public const int stalk_height = 6; //stalk limit for each main stalk
    public const int segment_angle_variation = 7; // how much each segment can vary in angle, for more natural growth
    public const int start_stalk_angle = 8; // the angle of the first segment, for more variation in growth direction

    [Header("Stalk Parameter Ranges")]
    public float minStalkLength = 0.5f;
    public float maxStalkLength = 2f;
    public float minStalkWidth = 0.1f;
    public float maxStalkWidth = 0.5f;
    public float minMainStalkCount = 1f;
    public float maxMainStalkCount = 3f;
    public float minStalkHeight = 1f; // no branches, one flower at the end
    public float maxStalkHeight = 10f; // 10 stalks high
    public float minSegmentAngleVariation = 0f;
    public float maxSegmentAngleVariation = 10f;
    public float minStartStalkAngle = -30f;
    public float maxStartStalkAngle = 30f;

    [Header("Stalk Genetic Parameter List")]
    private int paramDimension = 9;
    public float[] parameters;

    [Header("Generation")]
    public string seed;
    public string useSeed = "";    
    public float mutationRate; // chance for each gene to mutate when creating a new generation
    public float deviation; // how much a gene can change when it mutates, as a percentage of the total range of that gene

    [Header ("Leaf Parameters Constants")]
    public const int leaf_color_hue = 0;
    public const int leaf_color_saturation = 1;
    public const int leaf_color_value = 2;
    public const int leaf_length = 3;
    public const int leaf_width = 4;
    public const int leaf_density = 5;
    public const int leaf_angle = 6;

    [Header ("Leaf Parameter Ranges")]
    public float minLeafLength = 0.5f;
    public float maxLeafLength = 2f;
    public float minLeafWidth = 0.1f;
    public float maxLeafWidth = 0.5f;
    public float minLeafDensity = 0.1f; 
    public float maxLeafDensity = 0.9f;
    public float minLeafAngle = -30f;
    public float maxLeafAngle = 30f;

    [Header ("Leaf Genetic Parameter List")]
    private int leafParamDimension = 7;
    public float[] leafParameters;

    [Header("Leaf Objects")]
    public GameObject leafPrefab;
    int leafCount = 0; // later used for fitness
    public GameObject leafHolder; // parent object to hold all the leaves for organization

    [Header("Stalk Objects")]
    public GameObject spawnPoint;
    // each stalk prefab has a circle at the end to connect whatever
    public GameObject stalkPrefab;
    public float percentageShrink;
    public List<List<GameObject>> stalkSegments = new List<List<GameObject>>(); // list of stalks, each stalk is a list of segments
    public GameObject segmentHolder; // parent object to hold all the segments for organization, one for each list
    int stalkCount;
    Color stalkColor;

    [Header("Score Traits for STALK")]
    public float windResistanceScore; // stalk thickness and flexibility
    public float sunlightAbsorptionScore; // stalk length (more surface area = better for sunlight absorption)
    public float tempResistanceScore; // stalk color (darker green = better for more temperature resistance)
    public float stalkStrengthScore; // stalk length and width (longer and thicker stalks are stronger)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initRandomParam();
        createStalk();
    }

    public void createLeaves()
    {
        // keep greenish
        float h = Mathf.Lerp(0.28f, 0.38f, leafParameters[leaf_color_hue]);
        float s = Mathf.Lerp(0.6f, 1.0f, leafParameters[leaf_color_saturation]);
        float v = Mathf.Lerp(0.4f, 0.9f, leafParameters[leaf_color_value]);
        Color leafColor = Color.HSVToRGB(h, s, v);

        float leafLength = Mathf.Lerp(minLeafLength, maxLeafLength, leafParameters[leaf_length]);
        float leafWidth = Mathf.Lerp(minLeafWidth, maxLeafWidth, leafParameters[leaf_width]);

        //float leafAngle = Mathf.Lerp(minLeafAngle, maxLeafAngle, leafParameters[leaf_angle]);
        float leafAngle = 45f;

        // now iterate through stalk segment
        for (int i = 0; i < stalkCount; i++)
        {
            List<GameObject> segments = stalkSegments[i];
            int index = 0;
            foreach (GameObject segment in segments)
            {
                Vector3 spawnPos = segment.transform.position 
                + segment.transform.up * segment.transform.localScale.y * 0.5f;

                Quaternion spawnRot = Quaternion.Euler(0f, 0f, Random.Range(-leafAngle, leafAngle));

                GameObject leaf = Instantiate(leafPrefab, spawnPos, spawnRot);

                float brightness = 1f - (0.1f * i);

                Color c = stalkColor * brightness;
                c.a = 1f;

                leaf.transform.localScale = new Vector3(leafWidth, leafLength, 1f);

                // make leaf shrink as it goes up the stalk, for more natural look
                leaf.transform.localScale *= Mathf.Lerp(1f, 0.5f, (float)index / segments.Count);

                leaf.GetComponent<SpriteRenderer>().color = c;
                leaf.transform.parent = leafHolder.transform;

                leafCount++;
                index++;
            }
        }
    }

    public void createStalk()
    {
        stalkCount = Mathf.RoundToInt(minMainStalkCount 
        + parameters[main_stalk_count] * (maxMainStalkCount - minMainStalkCount));

        // keep greenish
        float h = Mathf.Lerp(0.28f, 0.38f, parameters[stalk_color_hue]);
        float s = Mathf.Lerp(0.6f, 1.0f, parameters[stalk_color_saturation]);
        float v = Mathf.Lerp(0.4f, 0.9f, parameters[stalk_color_value]);
        stalkColor = Color.HSVToRGB(h, s, v);

        int stalkHeight = Mathf.RoundToInt(minStalkHeight 
        + parameters[stalk_height] * (maxStalkHeight - minStalkHeight));

        float startAngle = Mathf.Lerp(minStartStalkAngle, maxStartStalkAngle, parameters[start_stalk_angle]);

        for (int i = 0; i < stalkCount; i++)
        {
            List<GameObject> segments = new List<GameObject>();
            Vector3 currentPosition = spawnPoint.transform.position;

            float totalSpread = 15f;

            float baseAngle;

            if (stalkCount == 1)
            {
                // if one main stalk, just go straight up with some angle variation
                baseAngle = 0f;
            }
            else
            {
                float t = (float)i / (stalkCount - 1); 
                baseAngle = Mathf.Lerp(-totalSpread * 0.5f, totalSpread * 0.5f, t);
            }

            Quaternion currentRotation = Quaternion.Euler(0f, 0f, baseAngle + startAngle);

            // create holder for each main stalk
            GameObject stalkHolder = new GameObject("StalkHolder" + i);
            stalkHolder.transform.parent = segmentHolder.transform;

            // allow stalkHeight to not always be the same for each main stalk, add some variation
            

            // create the main stalk segments
            for (int j = 0; j < Mathf.RoundToInt(stalkHeight); j++)
            {
                GameObject segment = Instantiate(stalkPrefab, currentPosition, currentRotation);
                segment.transform.localScale = new Vector3(
                    Mathf.Lerp(minStalkWidth, maxStalkWidth, parameters[stalk_width]),
                    Mathf.Lerp(minStalkLength, maxStalkLength, parameters[stalk_length]),
                    1f
                );

                float brightness = 1f - (0.1f * i);

                Color c = stalkColor * brightness;
                c.a = 1f;

                segment.GetComponent<SpriteRenderer>().color = c;

                segments.Add(segment);
                segment.transform.parent = stalkHolder.transform;

                // add z value based on j to ensure proper layering
                Vector3 pos = segment.transform.position;
                pos.z += i * 0.01f;
                segment.transform.position = pos;

                // update position and rotation for the next segment
                currentPosition += currentRotation * Vector3.up * segment.transform.localScale.y * 0.95f;
                float angleVariation = Mathf.Lerp(minSegmentAngleVariation, maxSegmentAngleVariation, parameters[segment_angle_variation]);
                currentRotation *= Quaternion.Euler(0f, 0f, Random.Range(-angleVariation, angleVariation));
            }

            stalkSegments.Add(segments);
        }

        createLeaves();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // for testing, regenerate the stalks with new random parameters
            foreach (List<GameObject> stalk in stalkSegments)
            {
                foreach (GameObject segment in stalk)
                {
                    Destroy(segment);
                }
            }
            stalkSegments.Clear();
            for (int i = 0; i < segmentHolder.transform.childCount; i++)
            {
                Destroy(segmentHolder.transform.GetChild(i).gameObject);
            }

            Start();
        }
    }

    public void createScore()
    {
        // notes:
        // wind resistance: thicker stalks are more resistant 
        // greenness is for sunlight absorption
        // stalk length for wind
        // todo
    }

    public void initRandomParam()
    {
        // initialize the parameters with random values within their respective ranges
        parameters = new float[paramDimension];

        parameters[stalk_color_hue] = Random.Range(0f, 1f);
        parameters[stalk_color_saturation] = Random.Range(0f, 1f);
        parameters[stalk_color_value] = Random.Range(0f, 1f);

        parameters[stalk_length] = Random.Range(0f, 1f);
        parameters[stalk_width] = Random.Range(0f, 1f);
        parameters[main_stalk_count] = Random.Range(0f, 1f);
        parameters[stalk_height] = Random.Range(0f, 1f);
        parameters[segment_angle_variation] = Random.Range(0f, 1f);
        parameters[start_stalk_angle] = Random.Range(0f, 1f);

        leafParameters = new float[leafParamDimension];
        leafParameters[leaf_color_hue] = Random.Range(0f, 1f);
        leafParameters[leaf_color_saturation] = Random.Range(0f, 1f);
        leafParameters[leaf_color_value] = Random.Range(0f, 1f);    
        leafParameters[leaf_length] = Random.Range(0f, 1f);
        leafParameters[leaf_width] = Random.Range(0f, 1f);
        leafParameters[leaf_density] = Random.Range(0f, 1f);
        leafParameters[leaf_angle] = Random.Range(0f, 1f);
    }
}
