using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    public void SetRecipeFactory(RecipeFactory recipeFactory)
    {
        recipeNameText.text = recipeFactory.recipeName;

        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) { continue; }
            Destroy(child.gameObject);
        }

        foreach (KitchenObjectFactory kitchenObjectFactory in recipeFactory.kitchenObjectFactories)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<Image>().sprite = kitchenObjectFactory.sprite;
        }
    }

}
