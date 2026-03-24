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
    public const int branch_probability = 6;

    [Header("Stalk Genetic Parameter List")]
    private int paramDimension = 7;
    public float[] parameters;

    [Header("Generation")]
    public string seed;
    public string useSeed = "";    
    public float mutationRate; // chance for each gene to mutate when creating a new generation
    public float deviation; // how much a gene can change when it mutates, as a percentage of the total range of that gene


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
