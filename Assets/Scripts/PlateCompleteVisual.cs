using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjectFactory_GameObject
    {
        public KitchenObjectFactory KitchenObjectFactory;
        public GameObject gameObject;
    }

    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectFactory_GameObject> kitchenObjectFactoryGameObjects;

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;

        foreach (KitchenObjectFactory_GameObject KitchenObjectFactoryGameObject in kitchenObjectFactoryGameObjects)
        {
            KitchenObjectFactoryGameObject.gameObject.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.IngredientEventArgs e)
    {
        foreach (KitchenObjectFactory_GameObject KitchenObjectFactoryGameObject in kitchenObjectFactoryGameObjects)
        {
            if (KitchenObjectFactoryGameObject.KitchenObjectFactory == e.kitchenObjectFatory)
            {
                KitchenObjectFactoryGameObject.gameObject.SetActive(true);
            }
        }
    }
}
