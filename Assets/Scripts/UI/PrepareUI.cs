using UnityEngine;
using UnityEngine.UI;

public class PrepareUI : MonoBehaviour
{
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject waitingUI;

    private void Awake()
    {
        readyButton.onClick.AddListener(() =>
        {
            waitingUI.SetActive(true);
            Hide();
            KitchenChaosGameManager.Instance.SetReadyToLocalPlayer();
        });
    }

    private void Start()
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
