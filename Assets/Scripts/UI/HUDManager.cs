using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public void ShowState(string state)
    {
        Debug.Log($"HUD State: {state}");
    }

    public void UpdateScore(ScoringData score)
    {
        Debug.Log("HUD Score updated");
    }
}
