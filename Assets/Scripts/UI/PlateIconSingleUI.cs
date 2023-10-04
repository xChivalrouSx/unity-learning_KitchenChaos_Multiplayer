using UnityEngine;
using UnityEngine.UI;

public class PlateIconSingleUI : MonoBehaviour
{
    [SerializeField] private Image image;

    public void SetKitchenObjectFactory(KitchenObjectFactory kitchenObjectFactory)
    {
        image.sprite = kitchenObjectFactory.sprite;
    }
}
