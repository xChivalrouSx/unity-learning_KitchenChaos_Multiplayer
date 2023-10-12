using System;
using Unity.Netcode;
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
            InteractLogicServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnPlayyerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }

}
