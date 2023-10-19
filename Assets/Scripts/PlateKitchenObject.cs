using System;
using System.Collections.Generic;
using Unity.Netcode;
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

        TryAddIngredientServerRpc(
            KitchenGameMultiplayer.Instance.GetKitchenObjectFactoryIndex(kitchenObjectFactory)
                );

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryAddIngredientServerRpc(int index)
    {
        TryAddIngredientClientRpc(index);
    }

    [ClientRpc]
    private void TryAddIngredientClientRpc(int index)
    {
        KitchenObjectFactory kitchenObjectFactory = KitchenGameMultiplayer.Instance.GetKitchenObjectFactoryFromIndex(index);
        kitchenObjectFactories.Add(kitchenObjectFactory);

        OnIngredientAdded?.Invoke(this, new IngredientEventArgs()
        {
            kitchenObjectFatory = kitchenObjectFactory
        });
    }

    public List<KitchenObjectFactory> GetKitchenObjectFactories()
    {
        return kitchenObjectFactories;
    }

}
