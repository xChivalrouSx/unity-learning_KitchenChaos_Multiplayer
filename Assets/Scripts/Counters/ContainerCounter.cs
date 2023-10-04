using System;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayyerGrabbedObject;


    [SerializeField] private KitchenObjectFactory kitchenObjectFactory;

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectFactory, player);
            OnPlayyerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }

}
