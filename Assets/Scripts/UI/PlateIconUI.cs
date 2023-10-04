using UnityEngine;

public class PlateIconUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.IngredientEventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach (Transform child in transform)
        {
            if (child == iconTemplate) { continue; }
            Destroy(child.gameObject);
        }
        foreach (KitchenObjectFactory kitchenObjectFactory in plateKitchenObject.GetKitchenObjectFactories())
        {
            Transform iconTransform = Instantiate(iconTemplate, transform);
            iconTransform.GetComponent<PlateIconSingleUI>().SetKitchenObjectFactory(kitchenObjectFactory);
            iconTransform.gameObject.SetActive(true);
        }
    }
}
