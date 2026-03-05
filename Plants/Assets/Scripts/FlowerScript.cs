using UnityEngine;

public class FlowerScript : MonoBehaviour
{
    // these are the indices to the genoes in the param list
    [Header("Flower Parameter Constants")]
    public const int flower_color_Hue = 0; // HSV hue
    public const int flower_color_Saturation = 1; // HSV saturation
    public const int flower_color_Value = 2; // HSV value
    public const int petal_length = 3; // length of the petal
    public const int petal_width = 4; // width of the petal
    public const int petal_count = 5; // number of petals
    public const int total_layers = 6; // total layers of petals, including the inner layer

    [Header("Flower Parameter Ranges")]
    public int minPetalCount = 8; // minimum number of petals to ensure the flower is visible
    public int maxPetalCount = 20; // maximum number of petals to prevent performance issues
    public int maxLayers = 5; // maximum number of layers 
    public int minLayers = 2; // minimum number of layers 
    public float minPetalLength = 1.5f;
    public float maxPetalLength = 5f;
    public float minPetalWidth = 1f;
    public float maxPetalWidth = 3f;
    
    // this is the list of parameters that will be used to determine the characteristics of the flowers
    // param list has to stay scale of 0 - 1 for mutating and mating
    [Header("Flower Genetic Parameter List")]
    private int paramDimension = 7;
    public float[] parameters;

    [Header("Generation")]
    public int generation = 0;
    public string seed;
    public string useSeed = "";    
    public float mutationRate = 0.4f; // chance for each gene to mutate when creating a new generation
    public float deviation = 0.15f; // how much a gene can change when it mutates, as a percentage of the total range of that gene
    
    [Header("Parents")]
    public GameObject parent1;
    public GameObject parent2;
    public bool asset = false;

    [Header("Flower Objects")]
    public GameObject center;
    public GameObject petalPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (asset) return;
        
        parameters = new float[paramDimension];

        if (generation == 0)
        {
            // first generation is initialized with random parameters
            initRandomParam();
        } 

        if (parent1 != null && parent2 != null)
        {
            // genomes are initialized from the parents for the next generations
            initParamFromParents(parent1, parent2);
        }

        CreateFlower();
        makeSeed();

        useSeed = seed; 
    }

    // generates 2D flower structure based on the parameters
    public void CreateFlower()
    {
        // convert HSV to RGB for the flower color
        Color flowerColor = Color.HSVToRGB(parameters[flower_color_Hue], parameters[flower_color_Saturation], parameters[flower_color_Value]);
        center.GetComponent<SpriteRenderer>().color = flowerColor * .2f;
        Vector3 pos = center.transform.position;
        pos.z = -1f;
        center.transform.position = pos;

        // get length of center, then make petal length minimum of 1.5 times the center length to ensure petals are long enough to be visible
        float centerLength_X = center.GetComponent<SpriteRenderer>().bounds.size.x;
        float petalLength = parameters[petal_length] * (maxPetalLength - minPetalLength) + minPetalLength;


        float petalWidth = parameters[petal_width] * (maxPetalWidth - minPetalWidth) + minPetalWidth;

        float petalCount = Mathf.Ceil(parameters[petal_count] * (maxPetalCount - minPetalCount) + minPetalCount); // scale petal count to be between min and max

        float angleBetweenPetals = 360f / petalCount;

        int layers = Mathf.Max(minLayers, (int) Mathf.Ceil(maxLayers * parameters[total_layers]));

        for (int j = 0; j < layers; j++)
        {
            for (int i = 0; i < petalCount; i++)
            {
                GameObject petal = Instantiate(petalPrefab, center.transform.position, Quaternion.identity);
                petal.transform.SetParent(center.transform);

                petal.transform.localScale = new Vector3(petalWidth - (.2f * petalWidth * j), petalLength - (.4f * petalLength * j), 1f); // scale down each layer
                petal.transform.Rotate(0f, 0f, i * angleBetweenPetals + (j * angleBetweenPetals / 2)); // rotate each layer of petals to be in between the previous layer
                petal.GetComponent<SpriteRenderer>().color = flowerColor;
                
                // make each layer a bit darker than the previous layer to create depth
                petal.GetComponent<SpriteRenderer>().color *= 1f - (0.1f * j); // darken each layer by 10%

                // make z in front of center
                petal.transform.localPosition = new Vector3(0f, 0f, 1f - (0.1f * j)); // ensure petals are in front of the center, and each layer is in front of the previous layer
            }
        }
    }

    public void makeSeed()
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            seed += parameters[i].ToString("F2");
            if (i < parameters.Length - 1)
            {
                seed += "_";
            }
        }
    }

    public void initParamFromSeed(string seed)
    {
        string[] paramStrings = seed.Split('_');
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = float.Parse(paramStrings[i]);
        }
    }

    public void initRandomParam()
    {
        parameters[flower_color_Hue] = Random.Range(0f, 1f);
        parameters[flower_color_Saturation] = Random.Range(0f, 1f);
        parameters[flower_color_Value] = Random.Range(0f, 1f);

        // length will be for sunlight absorb fitness
        parameters[petal_length] = Random.Range(0f, 1f);
        parameters[petal_width] = Random.Range(0f, 1f);
        parameters[petal_count] = Random.Range(0f, 1f);
        parameters[total_layers] = Random.Range(0f, 1f);
    }

    public void initParamFromParents(GameObject parent1, GameObject parent2)
    {
        FlowerScript script1 = parent1.GetComponentInChildren<FlowerScript>();
        FlowerScript script2 = parent2.GetComponentInChildren<FlowerScript>();

        randomGene(script1, script2);
        mutate();
    }

    private void randomGene(FlowerScript s1, FlowerScript s2)
    {
        float[] parent1 = s1.parameters;
        float[] parent2 = s2.parameters;

        for (int i = 0; i < paramDimension; i++)
        {
            float rand = Random.Range(0f, 1f);
            if (rand < 0.5f)
            {
                parameters[i] = parent1[i];
            } else
            {
                parameters[i] = parent2[i];
            }
        }
    }

    private void mutate()
    {
        for (int i = 0; i < paramDimension; i++)
        {
            float rand = Random.Range(0f, 1f);
            if (rand < mutationRate)
            {
                // mutate this gene
                float change = Random.Range(-deviation, deviation);
                parameters[i] += change;
                // ensure parameter stays between 0 and 1
                parameters[i] = Mathf.Clamp(parameters[i], 0f, 1f);
            }
        }
    }
    

    void Update()
    {
        // for testing, press space to regenerate the flower with new random parameters
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RedoFirstGeneration();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            NextGeneration();
        }
    }

    public void NextGeneration()
    {
        generation++;
        // to do
    }

    public void RedoFirstGeneration()
    {
        if (asset) return;
        generation = 0;
        // destroy all petals
        foreach (Transform child in center.transform)
        {
            if (child.gameObject != center)
            {
                Destroy(child.gameObject);
            }
        }
        Start(); // recreate the flower with new random parameters
    }
}
