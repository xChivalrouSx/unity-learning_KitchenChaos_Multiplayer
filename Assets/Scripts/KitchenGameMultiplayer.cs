using Unity.Netcode;
using UnityEngine;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public static KitchenGameMultiplayer Instance { get; private set; }

    [SerializeField] private KitchenObjectListFactory kitchenObjectListFactories;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnKitchenObject(KitchenObjectFactory kitchenObjectFactory, IKitchenObjectParent kitchenObjectParent)
    {
        SpawnKitchenObjectServerRpc(GetKitchenObjectFactoryIndex(kitchenObjectFactory), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectFactoryIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        KitchenObjectFactory kitchenObjectFactory = GetKitchenObjectFactoryFromIndex(kitchenObjectFactoryIndex);

        Transform kitchenObjectTransform = Instantiate(kitchenObjectFactory.prefab);
        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    private int GetKitchenObjectFactoryIndex(KitchenObjectFactory kitchenObjectFactory)
    {
        return kitchenObjectListFactories.kitchenObjectFactoryList.IndexOf(kitchenObjectFactory);
    }

    private KitchenObjectFactory GetKitchenObjectFactoryFromIndex(int index)
    {
        return kitchenObjectListFactories.kitchenObjectFactoryList[index];
    }

}
