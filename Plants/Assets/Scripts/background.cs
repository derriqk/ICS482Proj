using UnityEngine;

public class background : MonoBehaviour
{
    public GameObject glass;
    public GameObject dirt;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            glass.SetActive(!glass.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            dirt.SetActive(!dirt.activeSelf);
        }
    }
}
