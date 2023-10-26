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

    public void StartHost(GameObject prepareUI)
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();
        prepareUI.SetActive(true);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        bool connectionApproved = KitchenChaosGameManager.Instance.IsWaitingToStart();
        response.Approved = connectionApproved;
        response.CreatePlayerObject = connectionApproved;
    }

    public void StartClient(GameObject prepareUI)
    {
        NetworkManager.Singleton.StartClient();
        prepareUI.SetActive(true);
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

    public int GetKitchenObjectFactoryIndex(KitchenObjectFactory kitchenObjectFactory)
    {
        return kitchenObjectListFactories.kitchenObjectFactoryList.IndexOf(kitchenObjectFactory);
    }

    public KitchenObjectFactory GetKitchenObjectFactoryFromIndex(int index)
    {
        return kitchenObjectListFactories.kitchenObjectFactoryList[index];
    }


    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        ClearKitchenObjectOnParentClientRpc(kitchenObjectNetworkObjectReference);
        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        kitchenObject.ClearKitchenObjectOnParent();
    }

}
