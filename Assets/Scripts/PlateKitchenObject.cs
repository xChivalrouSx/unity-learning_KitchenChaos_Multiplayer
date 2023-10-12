using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{

    public event EventHandler<IngredientEventArgs> OnIngredientAdded;
    public class IngredientEventArgs : EventArgs
    {
        public KitchenObjectFactory kitchenObjectFatory;
    }

    [SerializeField] private List<KitchenObjectFactory> validKitchenObjectFactories;

    List<KitchenObjectFactory> kitchenObjectFactories;

    protected override void Awake()
    {
        base.Awake();
        kitchenObjectFactories = new List<KitchenObjectFactory>();
    }

    public bool TryAddIngredient(KitchenObjectFactory kitchenObjectFactory)
    {
        if (!validKitchenObjectFactories.Contains(kitchenObjectFactory) || kitchenObjectFactories.Contains(kitchenObjectFactory))
        {
            return false;
        }
        kitchenObjectFactories.Add(kitchenObjectFactory);

        OnIngredientAdded?.Invoke(this, new IngredientEventArgs()
        {
            kitchenObjectFatory = kitchenObjectFactory
        });

        return true;
    }

    public List<KitchenObjectFactory> GetKitchenObjectFactories()
    {
        return kitchenObjectFactories;
    }

}
