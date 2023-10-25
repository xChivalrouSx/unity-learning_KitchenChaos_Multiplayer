using UnityEngine;

public class PauseMultiplayerUI : MonoBehaviour
{
    private void Start()
    {
        KitchenChaosGameManager.Instance.OnMultiplayerGamePaused += KitchenChaosGameManager_OnMultiplayerGamePaused;
        KitchenChaosGameManager.Instance.OnMultiplayerGameUnpaused += KitchenChaosGameManager_OnMultiplayerGameUnpaused;

        Hide();
    }

    private void KitchenChaosGameManager_OnMultiplayerGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }

    private void KitchenChaosGameManager_OnMultiplayerGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
