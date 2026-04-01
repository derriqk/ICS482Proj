using UnityEngine;

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
    public const int branch_angle = 5; 
    public const int branch_probability = 6; // idea: branching increases scores of sort
    public const int stalk_height = 7; // the recursion limit for main stalk
    public const int main_stalk_count = 8; 

    [Header("Stalk Parameter Ranges")]
    public float minStalkLength = 0.5f;
    public float maxStalkLength = 2f;
    public float minStalkWidth = 0.1f;
    public float maxStalkWidth = 0.5f;
    public float minBranchAngle = 20f;
    public float maxBranchAngle = 75f;
    public float minStalkHeight = 1f; // no branches, one flower at the end
    public float maxStalkHeight = 10f; // 10 stalks high
    public float minMainStalkCount = 1f; // only one main stalk
    public float maxMainStalkCount = 5f; // up to 5 main

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
    public float stalkStrengthScore; // stalk length and width (longer and thicker stalks are stronger)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        parameters[branch_angle] = Random.Range(0f, 1f);
        parameters[branch_probability] = Random.Range(0f, 1f);
        parameters[stalk_height] = Random.Range(0f, 1f);
    }
}
