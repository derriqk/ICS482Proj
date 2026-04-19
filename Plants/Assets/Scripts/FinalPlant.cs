using UnityEngine;
using System.Collections.Generic;
// FINAL SCRIPT, ignore flowerscript, growth1, stalkgrower
public class FinalPlant : MonoBehaviour
{
    // this script handles the stalk parameter space growth for my project
    // its intended to grow and branch based on parameters
    // at the end of each stalk it will instantiate the flower 

    public float[] flowerParameters;
    public float[] parameters;
    public float[] leafParameters;
    public float[] branchParameters;

    [Header("Stalk Parameter Constants")]
    public const int stalk_color_hue = 0;
    public const int stalk_color_saturation = 1;
    public const int stalk_color_value = 2;
    public const int stalk_length = 3; // the segment length
    public const int stalk_width = 4; // the segment width
    public const int main_stalk_count = 5; // always 1 to 3, or whatever i choose
    public const int stalk_height = 6; // stalk limit for each main stalk
    public const int segment_angle_variation = 7; // how much each segment can vary in angle, for more natural growth
    public const int start_stalk_angle = 8; // the angle of the first segment, for more variation in growth direction
    public const int height_variation = 9; // main stalk variation
    public const int stalk_balance = 10; // the chance of a main stalk NOT switching direction (constant switching = balance)

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
    public float minHeightVariation = 0f; // same height for all main stalks
    public float maxHeightVariation = .5f; // up to 50% variation in height between main stalks

    [Header("Stalk Genetic Parameter List")]
    private int paramDimension = 11;

    [Header ("Leaf Parameters Constants")]
    public const int leaf_color_hue = 0;
    public const int leaf_color_saturation = 1;
    public const int leaf_color_value = 2;
    public const int leaf_length = 3;
    public const int leaf_width = 4;
    public const int leaf_distribution_bias = 5;

    [Header ("Leaf Parameter Ranges")]
    public float minLeafLength = 0.5f;
    public float maxLeafLength = 2f;
    public float minLeafWidth = 0.1f;
    public float maxLeafWidth = 0.5f;

    [Header ("Leaf Genetic Parameter List")]
    private int leafParamDimension = 6;

    [Header("Leaf Objects")]
    public GameObject leafPrefab;
    public GameObject leafHolder; // parent object to hold all the leaves for organization
    public int leafCount = 0;

    [Header("Stalk Objects")]
    public GameObject spawnPoint;
    // each stalk prefab has a circle at the end to connect whatever
    public GameObject stalkPrefab;
    public float percentageShrink;
    public List<List<GameObject>> stalkSegments = new List<List<GameObject>>(); // list of stalks, each stalk is a list of segments
    public GameObject segmentHolder; // parent object to hold all the segments for organization, one for each list
    int stalkCount;
    Color stalkColor;
    public int[] stalkHeights;
    float mainwidth;

    // these will guarantee flower at the end, no need for leaves at branches
    [Header("Branch Parameters Constants")]
    // color is the same as stalk color
    public const int branch_length = 0;
    public const int branch_angle = 1;
    public const int branch_thickness = 2; // percentage of the main stalk thickness
    public const int branch_segment_count = 3;
    public const int branch_distribution_bias = 4; // top or bottom bias, or even 0.5
    public const int branch_symmetry = 5; // contributes to balance

    [Header("Branch Parameter Ranges")]
    public float minBranchLength;
    public float maxBranchLength;
    public float minBranchThickness;
    public float maxBranchThickness;
    
    [Header("Branch Objects")]
    public GameObject branchPrefab;
    private int branchParamDimension = 6;
    public int branchCount = 0;

    [Header("Flower Parameter Constants")]
    public const int flower_color_Hue = 0; // HSV hue
    public const int flower_color_Saturation = 1; // HSV saturation
    public const int flower_color_Value = 2; // HSV value
    public const int petal_length = 3; // length of the petal
    public const int petal_width = 4; // width of the petal
    public const int petal_count = 5; // number of petals
    public const int total_layers = 6; // total layers of petals, including the inner layer
    public const int energy = 7; // affects shrinkage of flower as more are made, also contributes to score

    [Header("Flower Parameter Ranges")]
    public int minPetalCount = 8; // minimum number of petals to ensure the flower is visible
    public int maxPetalCount = 20; // maximum number of petals to prevent performance issues
    public int maxLayers = 5; // maximum number of layers 
    public int minLayers = 2; // minimum number of layers 
    public float minPetalLength = 1.5f;
    public float maxPetalLength = 5f;
    public float minPetalWidth = 1f;
    public float maxPetalWidth = 3f;

    [Header("Flower Genetic Parameter List")]
    private int flowerParamDimension = 8;

    [Header("Flower Objects")]
    public GameObject centerPrefab;
    public List<GameObject> centers = new List<GameObject>(); // will add at each end of branch and stalk
    public GameObject petalPrefab;
    public GameObject flowerHolder; 
    public GameObject flowerTemplate;
    public int flowerCount = 0; // contributes to score
    Color flowerColor;

    // assume commented is the best for it
    [Header("Scores")] // this is just using all the traits and combining them, no individual needed
    public float windResistanceScore; // thick stalk, shorter petals, more layers
    public float sunlightAbsorptionScore; // greener leaves and stalks, bigger leaf area
    public float tempResistanceScore; // petal width, layers for insulation
    public float stabilityScore; // stalk height to stalk area ratio, stalk balance, branch thickness, leaf distribution for stability, branch symmetry for balance, branch distribution bias for balance
    public float pollinatorAttractScore; // petal length, color brightness, flower count
    public float waterSheddingScore; // petal width, layers
    public float energyStressScore; // flower energy param, 0 means less stress and shrinkage as more flowers are made, 1 means more stress and shrinkage
    public float waterStressScore; // flower count contributes to water stress, more flowers means more water needed, but also contributes to pollinator attract score, so there's a balance there
    public float lightCompetitionScore; // height and leaf area and darker greens


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initRandomParam();
        createFlowerTemplate();
        createStalk();
        calculateScore();
    }

    public void calculateScore()
    {
        // just weighted sums that add to 1 based on priority
        windResistanceScore =
        (parameters[stalk_width] * 0.2f) +
        (flowerParameters[petal_length] * 0.4f) + // shorter petals are more resistant to wind damage
        (flowerParameters[total_layers] * 0.3f) + // more layers better
        (branchParameters[branch_thickness] * 0.1f);

        // values good greens and bigger leaves
        sunlightAbsorptionScore =
        (parameters[stalk_color_saturation] * 0.15f) +
        (parameters[stalk_color_value] * 0.1f) +
        (leafParameters[leaf_color_saturation] * 0.2f) +
        (leafParameters[leaf_color_value] * 0.15f) +
        (leafParameters[leaf_length] * leafParameters[leaf_width] * 0.4f); // leaf area

        // smaller petals and more layers
        float biasScore = 1f - Mathf.Abs(leafParameters[leaf_distribution_bias] - 0.5f) / 0.5f;
        biasScore = Mathf.Clamp01(biasScore);
        tempResistanceScore = 
        (flowerParameters[petal_width] * 0.3f) +
        (flowerParameters[total_layers] * 0.4f) +
        (biasScore * 0.3f); // even leaves help

        float branchBiasScore =
        1f - Mathf.Abs(branchParameters[branch_distribution_bias] - 0.5f) / 0.5f;
        branchBiasScore = Mathf.Clamp01(branchBiasScore);

        float heightScore = Mathf.Clamp01(stalkHeights[0] / 10f);
        float widthScore = 1f - Mathf.Clamp01((mainwidth - 0.1f) / 0.4f);
        float structureScore = heightScore * widthScore;

        stabilityScore =
        (structureScore * 0.35f) + // area
        (parameters[stalk_balance] * 0.15f) +
        (branchParameters[branch_thickness] * 0.1f) +
        (branchParameters[branch_symmetry] * 0.2f) +
        (branchBiasScore * 0.1f) + // branch
        (biasScore * 0.1f); // leaf

        // brighter bigger flowers
        pollinatorAttractScore = 
        (flowerParameters[petal_length] * 0.25f) + 
        (flowerParameters[flower_color_Saturation] * 0.25f) + 
        (flowerParameters[flower_color_Value] * 0.4f); 

        // better at remove water
        waterSheddingScore =
        (flowerParameters[petal_width] * 0.3f) +
        (flowerParameters[total_layers] * 0.25f) +
        ((1f - flowerParameters[petal_length]) * 0.25f) +
        ((1f - flowerParameters[petal_count]) * 0.2f);

        // balanced leaf to flower ratio
        energyStressScore =
        (flowerParameters[energy] * 0.5f) +
        (Mathf.Clamp01(flowerCount / (leafCount + 1f)) * 0.5f);

        // longer stalks/branches and more flowers means more water needed
        waterStressScore =
        (parameters[stalk_length] * 0.35f) +
        (branchParameters[branch_length] * 0.3f) +
        energyStressScore * 0.3f;

        // reach capability for light
        lightCompetitionScore =
        (parameters[stalk_height] * 0.4f) + // far reach
        (leafParameters[leaf_length] * .4f) + // far reach
        (leafParameters[leaf_width] * 0.2f);
    }

    public void createFlowerTemplate()
    {
        flowerTemplate = new GameObject("FlowerTemplate");
        flowerTemplate.transform.parent = flowerHolder.transform;
        flowerTemplate.SetActive(false); // hide

        GameObject center = Instantiate(centerPrefab, Vector3.zero, Quaternion.identity);
        center.transform.parent = flowerTemplate.transform;

        flowerColor = Color.HSVToRGB(parameters[flower_color_Hue], parameters[flower_color_Saturation], parameters[flower_color_Value]);

        float petalWidth = parameters[petal_width] * (maxPetalWidth - minPetalWidth) + minPetalWidth;
        float petalLength = parameters[petal_length] * (maxPetalLength - minPetalLength) + minPetalLength;

        float petalCount = Mathf.Ceil(parameters[petal_count] * (maxPetalCount - minPetalCount) + minPetalCount); // scale petal count to be between min and max

        float angleBetweenPetals = 360f / petalCount;

        int layers = Mathf.Max(minLayers, (int) Mathf.Ceil(maxLayers * parameters[total_layers]));

        Vector3 pos = center.transform.position;
        pos.z = -1f;
        center.transform.position = pos;
        center.GetComponent<SpriteRenderer>().color = flowerColor * .2f;
            
        // get length of center, then make petal length minimum of 1.5 times the center length to ensure petals are long enough to be visible
        float centerLength_X = center.GetComponent<SpriteRenderer>().bounds.size.x;

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
                petal.transform.localPosition = new Vector3(0f, 0f, 1f - (0.1f * j)); // ensure petals are in front of the center, and each     
            }
        }
    }

    public void createFlowers()
    {
        int index = 0;
        foreach(GameObject center in centers)
        {
            // scale dies down as more are made
            // determined by flower energy
            // energy can reduce scale by up to 50%, and each flower reduces scale by 5%
            float scale = 1f - (parameters[energy] * 0.5f) - (flowerCount * 0.05f); 
            center.GetComponent<SpriteRenderer>().color = flowerColor;
            float sizeScale;
            if (index < 40)
            {
                sizeScale = 1f - (index * 0.02f); 
            } else
            {
                sizeScale = 0.2f;
            }
            if (scale <= .2f)
            {
                // keep as bud
                center.transform.localScale = new Vector3(2f, 2f, 1f) * sizeScale;
                index++;
                continue;
            }
            
            GameObject flower = Instantiate(flowerTemplate, center.transform.position, Quaternion.identity);
            flower.transform.parent = flowerHolder.transform;
            flower.SetActive(true);

            
            flower.transform.localScale = new Vector3(scale, scale, 1f);

            // use for score
            flowerCount++;
            index++;
        }
    }

    public void createBranches()
    {
        Color branchColor = stalkColor; // same for now

        // copies stalk
        minBranchLength = minStalkLength *5; // 3 times branch length
        maxBranchLength = maxStalkLength *9;
        minBranchThickness = minStalkWidth;
        maxBranchThickness = maxStalkWidth;

        float length = Mathf.Lerp(minBranchLength, maxBranchLength, branchParameters[branch_length]);
        float thickness = mainwidth;
        float currLenght = length;

        float angle = 20f; // base

        if (Random.value > 0.5f) angle *= -1f; // randomize left or right
        if (Random.value > 0.5f) angle += 10*branchParameters[branch_angle]; else angle -= 10*branchParameters[branch_angle];

        // now for each stalk, loop through distribution
        for (int i = 0; i < stalkCount; i++)
        {
            List<GameObject> segments = stalkSegments[i];
            int index = 0;
            foreach (GameObject segment in segments)
            {
                float t = (float)index / (segments.Count - 1);

                float bias = Mathf.Lerp(-1f, 1f, branchParameters[branch_distribution_bias]);

                float chance = (bias >= 0)
                    ? Mathf.Pow(t, 1f + bias * 4f)
                    : Mathf.Pow(1f - t, 1f + (-bias) * 4f);

                if (Random.value > chance)
                {
                    index++;
                    continue;
                }
                index++;

                float newThickness = thickness * Mathf.Pow(percentageShrink, index *.5f);

                float rand = Random.Range(0f, 1f);
                if (rand > .5) angle *= -1f; // randomize left or right for each branch

                // only apply after chance
                Vector3 spawnPos = segment.transform.position 
                + segment.transform.up * segment.transform.localScale.y * 0.5f;

                Quaternion spawnRot = segment.transform.rotation *
                Quaternion.Euler(0f, 0f, angle);

                currLenght = Mathf.Lerp(minBranchLength, maxBranchLength, branchParameters[branch_length]) * Mathf.Pow(percentageShrink, index*2f);

                GameObject branch = Instantiate(branchPrefab, spawnPos, spawnRot);
                branch.transform.localScale = new Vector3(newThickness, currLenght, 1f);
                Color c = branchColor * (1f - (0.1f * i));
                c.a = 1f;

                Vector3 right = segment.transform.right * thickness * 0.5f;

                if (Random.value > branchParameters[branch_symmetry])
                {
                    // spawn another branch on the opposite side for more symmetry
                    Quaternion oppositeRot = segment.transform.rotation *
                    Quaternion.Euler(0f, 0f, -angle);
                    GameObject oppositeBranch = Instantiate(branchPrefab, spawnPos, oppositeRot);
                    oppositeBranch.transform.localScale = new Vector3(newThickness, currLenght * .5f , 1f);
                    oppositeBranch.GetComponent<SpriteRenderer>().color = c;
                    oppositeBranch.transform.parent = segmentHolder.transform;
                    
                    if (angle > 0)
                    {
                        oppositeBranch.transform.position += right;
                    }
                    else
                    {
                        oppositeBranch.transform.position -= right;
                    }

                    branchCount++;

                    // take branches child and add it to center since branches have inherent flowercenter at end
                    GameObject center = oppositeBranch.transform.GetChild(0).gameObject;
                    center.transform.parent = flowerHolder.transform;
                    centers.Add(center);
                    // reset center scale
                    center.transform.localScale = Vector3.one * 0.5f;
                }

                branch.GetComponent<SpriteRenderer>().color = c;
                branch.transform.parent = segmentHolder.transform;

                if (angle > 0)
                {
                    branch.transform.position -= right;
                }
                else
                {
                    branch.transform.position += right;
                }

                GameObject center2 = branch.transform.GetChild(0).gameObject;
                center2.transform.parent = flowerHolder.transform;
                centers.Add(center2);
                // reset center scale
                center2.transform.localScale = Vector3.one * 0.5f;

                branchCount++;
            }
        }
    }
    public void createLeavesAtStalk()
    {
        // keep greenish
        float h = Mathf.Lerp(0.28f, 0.38f, leafParameters[leaf_color_hue]);
        float s = Mathf.Lerp(0.6f, 1.0f, leafParameters[leaf_color_saturation]);
        float v = Mathf.Lerp(0.4f, 0.9f, leafParameters[leaf_color_value]);
        Color leafColor = Color.HSVToRGB(h, s, v);

        float leafLength = Mathf.Lerp(minLeafLength, maxLeafLength, leafParameters[leaf_length]);
        float leafWidth = Mathf.Lerp(minLeafWidth, maxLeafWidth, leafParameters[leaf_width]);

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

                float t = (float)index / (segments.Count - 1);

                float bias = Mathf.Lerp(-1f, 1f, leafParameters[leaf_distribution_bias]);

                float chance = (bias >= 0)
                    ? Mathf.Pow(t, 1f + bias * 4f)
                    : Mathf.Pow(1f - t, 1f + (-bias) * 4f);

                if (Random.value > chance)
                {
                    index++;
                    continue;
                }

                float rand = Random.Range(0f, 1f);
                if (rand > .5) leafAngle *= -1f; 

                Quaternion spawnRot = segment.transform.rotation *
                Quaternion.Euler(0f, 0f, leafAngle);

                GameObject leaf = Instantiate(leafPrefab, spawnPos, spawnRot);

                leaf.transform.localScale = new Vector3(leafWidth, leafLength, 1f);

                Vector3 right = segment.transform.right * leafWidth * 0.5f;
                if (leafAngle > 0)
                {
                    leaf.transform.position -= right;
                }
                else
                {
                    leaf.transform.position += right;
                }

                // make leaf shrink as it goes up the stalk, for more natural look
                leaf.transform.localScale *= Mathf.Lerp(1f, 0.25f, (float)index / segments.Count);

                float brightness = 1f - (0.01f * i * index);

                Color c = leafColor * brightness;
                c.a = 1f;
                leaf.GetComponent<SpriteRenderer>().color = c;

                leaf.transform.parent = leafHolder.transform;

                index++;
                leafCount++;
            }
        }
    }

    public void createStalk()
    {
        float heightVar = Mathf.Lerp(minHeightVariation, maxHeightVariation, parameters[height_variation]);

        stalkCount = Mathf.RoundToInt(minMainStalkCount 
        + parameters[main_stalk_count] * (maxMainStalkCount - minMainStalkCount));

        stalkHeights = new int[stalkCount];
        stalkHeights[0] = stalkCount; // since first is base

        // keep greenish
        float h = Mathf.Lerp(0.28f, 0.38f, parameters[stalk_color_hue]);
        float s = Mathf.Lerp(0.6f, 1.0f, parameters[stalk_color_saturation]);
        float v = Mathf.Lerp(0.4f, 0.9f, parameters[stalk_color_value]);
        stalkColor = Color.HSVToRGB(h, s, v);
        
        mainwidth = Mathf.Lerp(minStalkWidth, maxStalkWidth, parameters[stalk_width]);
        float length = Mathf.Lerp(minStalkLength, maxStalkLength, parameters[stalk_length]);

        int stalkHeight = Mathf.RoundToInt(minStalkHeight 
        + parameters[stalk_height] * (maxStalkHeight - minStalkHeight));

        int stalkHeightVar = stalkHeight; // one stalk is always the base height

        float startAngle = Mathf.Lerp(minStartStalkAngle, maxStartStalkAngle, parameters[start_stalk_angle]);

        // predetermined for each stalk
        // adds to sway score
        float angleVariation = Mathf.Lerp(minSegmentAngleVariation, maxSegmentAngleVariation, parameters[segment_angle_variation]);
        

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

            float dir = -1f; // left by default
            if (Random.value > parameters[stalk_balance]) dir *= -1f; 
            
            // create the main stalk segments
            for (int j = 0; j < stalkHeightVar; j++)
            {
                GameObject segment = Instantiate(stalkPrefab, currentPosition, currentRotation);
                segment.transform.localScale = new Vector3(
                    mainwidth * Mathf.Pow(percentageShrink, j),
                    length, // length stay
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

                currentRotation *= Quaternion.Euler(0f, 0f, dir * angleVariation);

                // check for final segment to add flower
                if (j == stalkHeightVar - 1)
                {
                    GameObject center = Instantiate(centerPrefab, currentPosition, Quaternion.identity);
                    centers.Add(center);
                    center.transform.parent = segmentHolder.transform; 
                }
            }
            stalkHeights[i] = stalkHeightVar; // store for future use
            
            stalkHeightVar = stalkHeight; // reset
            stalkHeightVar += Random.Range(-Mathf.RoundToInt(heightVar * stalkHeight), Mathf.RoundToInt(heightVar * stalkHeight) + 1);
            stalkSegments.Add(segments);
        }
        
        createLeavesAtStalk();
        createBranches();
        createFlowers();
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
            for (int i = 0; i < leafHolder.transform.childCount; i++)
            {
                Destroy(leafHolder.transform.GetChild(i).gameObject);
            }
            stalkSegments.Clear();
            for (int i = 0; i < segmentHolder.transform.childCount; i++)
            {
                Destroy(segmentHolder.transform.GetChild(i).gameObject);
            }

            centers.Clear();
            for (int i = 0; i < flowerHolder.transform.childCount; i++)
            {
                Destroy(flowerHolder.transform.GetChild(i).gameObject);
            }

            // clear flower template petals inside first child
            for (int i = 0; i < flowerTemplate.transform.GetChild(0).childCount; i++)
            {
                Destroy(flowerTemplate.transform.GetChild(0).GetChild(i).gameObject);
            }
            Destroy(flowerTemplate.transform.GetChild(0).gameObject);

            flowerCount = 0;
            leafCount = 0;

            Start();
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
        parameters[main_stalk_count] = Random.Range(0f, 1f);
        parameters[stalk_height] = Random.Range(0f, 1f);
        parameters[segment_angle_variation] = Random.Range(0f, 1f);
        parameters[start_stalk_angle] = Random.Range(0f, 1f);
        parameters[height_variation] = Random.Range(0f, 1f);
        parameters[stalk_balance] = Random.Range(0f, 1f);

        leafParameters = new float[leafParamDimension];
        leafParameters[leaf_color_hue] = Random.Range(0f, 1f);
        leafParameters[leaf_color_saturation] = Random.Range(0f, 1f);
        leafParameters[leaf_color_value] = Random.Range(0f, 1f);    
        leafParameters[leaf_length] = Random.Range(0f, 1f);
        leafParameters[leaf_width] = Random.Range(0f, 1f);

        // 0 is near bottom, 1 is near top
        leafParameters[leaf_distribution_bias] = Random.Range(0f, 1f);

        // brnach
        branchParameters = new float[branchParamDimension];
        branchParameters[branch_length] = Random.Range(0f, 1f);
        branchParameters[branch_angle] = Random.Range(0f, 1f);
        branchParameters[branch_thickness] = Random.Range(.2f, 1f);
        branchParameters[branch_segment_count] = Random.Range(0f, 1f);
        branchParameters[branch_distribution_bias] = Random.Range(0f, 1f);
        branchParameters[branch_symmetry] = Random.Range(0f, 1f);

        flowerParameters = new float[flowerParamDimension];
        flowerParameters[flower_color_Hue] = Random.Range(0f, 1f);
        flowerParameters[flower_color_Saturation] = Random.Range(0f, 1f);
        flowerParameters[flower_color_Value] = Random.Range(0f, 1f);

        // length will be for sunlight absorb fitness
        flowerParameters[petal_length] = Random.Range(0f, 1f);
        flowerParameters[petal_width] = Random.Range(0f, 1f);
        flowerParameters[petal_count] = Random.Range(0f, 1f);
        flowerParameters[total_layers] = Random.Range(0f, 1f);
        flowerParameters[energy] = Random.Range(0f, 1f);
    }
}
