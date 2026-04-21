using UnityEngine;

public class PredeterminedFlowers : MonoBehaviour
{
    public WorldHandlerFinal worldHandler;
    public void Lilacs()
    {
        worldHandler.tempScore        = 0.45f;
        worldHandler.sunScore         = 0.75f;
        worldHandler.windScore        = 0.30f;
        worldHandler.rainScore        = 0.60f;
        worldHandler.pollinatorScore  = 0.85f;

        worldHandler.updateStates();
    }

    public void Sunflower()
{
    worldHandler.tempScore       = 0.70f;
    worldHandler.sunScore        = 0.95f;
    worldHandler.windScore       = 0.45f;
    worldHandler.rainScore       = 0.50f;
    worldHandler.pollinatorScore = 0.85f;

    worldHandler.updateStates();
}
}
