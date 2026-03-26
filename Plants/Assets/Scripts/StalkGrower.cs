using UnityEngine;

public class StalkGrower : MonoBehaviour
{
    // this script handles the stalk parameter space growth for my project
    // its intended to grow and branch based on parameters
    // at the end of each stalk it will instantiate the flower 

    [Header("Stalk Parameter Constants")]
    public const int stalk_color_hue = 0;
    public const int stalk_color_saturation = 1;
    public const int stalk_color_value = 2;
    public const int stalk_length = 3;
    public const int stalk_width = 4;
    public const int branch_angle = 5; 
    public const int branch_probability = 6; // idea: branching increases scores of sort
    public const int stalk_height = 7; // the recursion limit for main stalk

    [Header("Stalk Parameter Ranges")]
    public float minStalkLength = 0.5f;
    public float maxStalkLength = 2f;
    public float minStalkWidth = 0.1f;
    public float maxStalkWidth = 0.5f;
    public float minBranchAngle = 20f;
    public float maxBranchAngle = 75f;
    public float minStalkHeight = 1f; // no branches, one flower at the end
    public float maxStalkHeight = 10f; // 10 stalks high

    [Header("Stalk Genetic Parameter List")]
    private int paramDimension = 8;
    public float[] parameters;

    [Header("Generation")]
    public string seed;
    public string useSeed = "";    
    public float mutationRate; // chance for each gene to mutate when creating a new generation
    public float deviation; // how much a gene can change when it mutates, as a percentage of the total range of that gene

    [Header("Stalk Objects")]
    public GameObject spawnPoint;
    public GameObject stalkPrefab;
    public float percentageShrink;

    [Header("Score Traits for STALK")]
    public float windResistanceScore; // stalk thickness and flexibility
    public float sunlightAbsorptionScore; // stalk length (more surface area = better for sunlight absorption)
    public float tempResistanceScore; // stalk color (darker green = better for more temperature resistance)


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        randomStart();
    }

    public void randomStart()
    {
       initRandomParam();

       createStalk();
    }

    public void createStalk()
    {
        // keep greenish
        float h = Mathf.Lerp(0.28f, 0.38f, parameters[stalk_color_hue]);
        float s = Mathf.Lerp(0.6f, 1.0f, parameters[stalk_color_saturation]);
        float v = Mathf.Lerp(0.4f, 0.9f, parameters[stalk_color_value]);
        Color stalkColor = Color.HSVToRGB(h, s, v);

        float stalkLength = parameters[stalk_length] * (maxStalkLength - minStalkLength) + minStalkLength;
        float stalkWidth = parameters[stalk_width] * (maxStalkWidth - minStalkWidth) + minStalkWidth;
        float branchAngle = parameters[branch_angle] * (maxBranchAngle - minBranchAngle) + minBranchAngle;
        float stalkHeight = parameters[stalk_height] * (maxStalkHeight - minStalkHeight) + minStalkHeight;

        for (int i = 0; i < stalkHeight; i++)
        {
            GameObject newStalk = Instantiate(stalkPrefab, spawnPoint.transform.position + Vector3.up * i * stalkLength, Quaternion.identity);

            // new stalk is lesser width of prev capped at i = 4
            if (i < 4)
            {
                newStalk.transform.localScale = 
                new Vector3(stalkWidth * Mathf.Pow(percentageShrink, i), stalkLength, stalkWidth * Mathf.Pow(percentageShrink, i));
            } else
            {
                newStalk.transform.localScale = 
                new Vector3(stalkWidth * Mathf.Pow(percentageShrink, 4), stalkLength, stalkWidth * Mathf.Pow(percentageShrink, 4));
            }
            
            newStalk.GetComponent<SpriteRenderer>().color = stalkColor;

            newStalk.transform.parent = spawnPoint.transform;

            float randAngle = Random.Range(-branchAngle, branchAngle);

            // branching logic
            if (Random.value < parameters[branch_probability] & i > 0 & i < stalkHeight - 1)
            {
                GameObject branch = Instantiate(stalkPrefab, newStalk.transform.position + Vector3.up * stalkLength, Quaternion.Euler(0, 0, randAngle));
                branch.transform.localPosition = newStalk.transform.position + Vector3.left * stalkWidth;
                if (i < 4)
                {
                    branch.transform.localScale =
                    new Vector3(stalkWidth * Mathf.Pow(percentageShrink, i), stalkLength, stalkWidth * Mathf.Pow(percentageShrink, i));
                } else
                {
                    branch.transform.localScale = 
                    new Vector3(stalkWidth * Mathf.Pow(percentageShrink, 4), stalkLength, stalkWidth * Mathf.Pow(percentageShrink, 4));
                }
                
                branch.GetComponent<SpriteRenderer>().color = stalkColor;
            }
        }



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
        parameters[branch_angle] = Random.Range(0f, 1f);
        parameters[branch_probability] = Random.Range(0f, 1f);
        parameters[stalk_height] = Random.Range(0f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetEvolution();
        }
    }

    public void ResetEvolution()
    {
        foreach (Transform child in spawnPoint.transform)
        {
            Destroy(child.gameObject);
        }

        randomStart();
    }
}
