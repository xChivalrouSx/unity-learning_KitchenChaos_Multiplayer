using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    private const int MAX_PLAYER_COUNT = 4;

    public static KitchenGameMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailToJoinGame;
    public event EventHandler OnPLayerDataNetworkListChanged;

    [SerializeField] private KitchenObjectListFactory kitchenObjectListFactories;

    private NetworkList<PlayerData> playerDataNetworkList;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPLayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost(GameObject prepareUI = null)
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.StartHost();
        if (prepareUI != null)
        {
            prepareUI.SetActive(true);
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData { clientId = clientId });
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (!SceneManager.GetActiveScene().name.Equals(Loader.Scene.CharacterSelectScene.ToString()))
        {
            response.Approved = false;
            response.Reason = "Game already started.";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_COUNT)
        {
            response.Approved = false;
            response.Reason = "Game is full.";
            return;
        }

        response.Approved = true;

        //bool connectionApproved = KitchenChaosGameManager.Instance.IsWaitingToStart();
        //response.Approved = connectionApproved;
        //response.CreatePlayerObject = connectionApproved;
    }

    public void StartClient(GameObject prepareUI = null)
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
        if (prepareUI != null)
        {
            prepareUI.SetActive(true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailToJoinGame?.Invoke(this, EventArgs.Empty);
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

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playedIndex)
    {
        return playerDataNetworkList[playedIndex];
    }

}
