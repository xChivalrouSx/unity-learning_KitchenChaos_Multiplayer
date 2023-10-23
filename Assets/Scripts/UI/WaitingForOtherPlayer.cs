using UnityEngine;

public class WaitingForOtherPlayer : MonoBehaviour
{
    private void Start()
    {
        KitchenChaosGameManager.Instance.OnLocalPlayerReadyChange += KitchenChaosGameManager_OnLocalPlayerReadyChange;
        KitchenChaosGameManager.Instance.OnStateChange += KitchenChaosGameManager_OnStateChange;

        Hide();
    }

    private void KitchenChaosGameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (KitchenChaosGameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
    }

    private void KitchenChaosGameManager_OnLocalPlayerReadyChange(object sender, System.EventArgs e)
    {
        if (KitchenChaosGameManager.Instance.IsLocalPLayerReady())
        {
            Show();
        }
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
