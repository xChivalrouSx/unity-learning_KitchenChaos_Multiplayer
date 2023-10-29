using System;
using System.Collections.Generic;
using Unity.Netcode;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady()
    {
        SetReadyToLocalPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyToLocalPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        setPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientIsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientIsReady = false;
                break;
            }
        }

        if (allClientIsReady)
        {
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    [ClientRpc]
    private void setPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }

}
