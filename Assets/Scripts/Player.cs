using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyPlayerSpawned;

    public static Player LocalInstance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> spawnPositionList;
    [SerializeField] private PlayerVisual playerVisual;

    private bool isWailking;
    private Vector3 lastInteractDirectory;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenChaosGameManager.Instance.IsGamePlaying()) { return; }
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenChaosGameManager.Instance.IsGamePlaying()) { return; }
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        // --- Added for Server Auth
        // HandeMovementServerAuth();

        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWailking;
    }

    private void HandleInteractions()
    {
        Vector3 movementDirectory = GameInput.Instance.GetMovementVectorNormalized();

        if (movementDirectory != Vector3.zero)
        {
            lastInteractDirectory = movementDirectory;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDirectory, out RaycastHit raycastHit, interactDistance, counterLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandeMovementServerAuth()
    {
        Vector3 movementDirectory = GameInput.Instance.GetMovementVectorNormalized();
        HandleMovementServerRpc(movementDirectory);
    }

    [ServerRpc]
    private void HandleMovementServerRpc(Vector3 movementDirectory)
    {
        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = 0.7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirectory, moveDistance);
        if (!canMove)
        {
            Vector3 movementDirectoryX = new Vector3(movementDirectory.x, 0, 0).normalized;
            canMove = movementDirectory.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirectoryX, moveDistance);

            if (canMove)
            {
                movementDirectory = movementDirectoryX;
            }
            else
            {
                Vector3 movementDirectoryZ = new Vector3(0, 0, movementDirectory.z).normalized;
                canMove = movementDirectory.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirectoryZ, moveDistance);
                if (canMove)
                {
                    movementDirectory = movementDirectoryZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += movementDirectory * moveDistance;
        }

        isWailking = movementDirectory != Vector3.zero;
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movementDirectory, Time.deltaTime * rotateSpeed);
    }

    private void HandleMovement()
    {
        Vector3 movementDirectory = GameInput.Instance.GetMovementVectorNormalized();

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = 0.7f;
        float playerHeight = 2f;
        // bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirectory, moveDistance, collisionsLayerMask);
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, movementDirectory, Quaternion.identity, moveDistance, collisionsLayerMask);
        if (!canMove)
        {
            Vector3 movementDirectoryX = new Vector3(movementDirectory.x, 0, 0).normalized;
            // canMove = movementDirectory.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirectoryX, moveDistance, collisionsLayerMask);
            canMove = movementDirectory.x != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, movementDirectoryX, Quaternion.identity, moveDistance, collisionsLayerMask);

            if (canMove)
            {
                movementDirectory = movementDirectoryX;
            }
            else
            {
                Vector3 movementDirectoryZ = new Vector3(0, 0, movementDirectory.z).normalized;
                // canMove = movementDirectory.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirectoryZ, moveDistance, collisionsLayerMask);
                canMove = movementDirectory.z != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, movementDirectoryZ, Quaternion.identity, moveDistance, collisionsLayerMask);
                if (canMove)
                {
                    movementDirectory = movementDirectoryZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += movementDirectory * moveDistance;
        }

        isWailking = movementDirectory != Vector3.zero;
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movementDirectory, Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs { selectedCounter = selectedCounter });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        this.kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

}
