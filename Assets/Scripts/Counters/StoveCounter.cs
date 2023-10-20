using System;
using Unity.Netcode;
using UnityEngine;
using static IHasProgress;

public class StoveCounter : BaseCounter, IHasProgress
{

    public event EventHandler<OnProgressChangeEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChange;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned
    }

    [SerializeField] private FryingRecpieFactory[] FryingRecpieFactories;
    [SerializeField] private BurningRecipeFactory[] burningRecipeFactories;

    private NetworkVariable<State> currentState = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> burnedTimer = new NetworkVariable<float>(0f);
    private FryingRecpieFactory fryingRecpieFactory;
    private BurningRecipeFactory burningRecipeFactory;

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChange;
        burnedTimer.OnValueChanged += BurnedTimer_OnValueChange;
        currentState.OnValueChanged += CurrentState_OnValueChange;
    }

    private void FryingTimer_OnValueChange(float pref, float current)
    {
        float fryingTimerMax = fryingRecpieFactory != null ? fryingRecpieFactory.fryingTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

    private void BurnedTimer_OnValueChange(float pref, float current)
    {
        float burnedTimerMax = burningRecipeFactory != null ? burningRecipeFactory.burningTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
        {
            progressNormalized = burnedTimer.Value / burnedTimerMax
        });
    }

    private void CurrentState_OnValueChange(State pref, State current)
    {
        OnStateChange?.Invoke(this, new OnStateChangedEventArgs
        {
            state = currentState.Value
        });

        if (currentState.Value == State.Burned || currentState.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
            {
                progressNormalized = 0f
            });
        }

    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (HasKitchenObject())
        {
            switch (currentState.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer.Value += Time.deltaTime;

                    if (fryingTimer.Value > fryingRecpieFactory.fryingTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(fryingRecpieFactory.output, this);

                        currentState.Value = State.Fried;
                        burnedTimer.Value = 0f;
                        SetBurningRecipeFactoryClientRpc(
                            KitchenGameMultiplayer.Instance.GetKitchenObjectFactoryIndex(GetKitchenObject().GetKitchenObjectFactory())
                                );
                    }
                    break;
                case State.Fried:
                    burnedTimer.Value += Time.deltaTime;

                    if (burnedTimer.Value > burningRecipeFactory.burningTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(burningRecipeFactory.output, this);
                        currentState.Value = State.Burned;

                    }
                    break;
                case State.Burned:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectFactory()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc(
                            KitchenGameMultiplayer.Instance.GetKitchenObjectFactoryIndex(kitchenObject.GetKitchenObjectFactory())
                        );
                }
            }
        }
        else
        {
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                SetStateIdleServerRpc();
            }
            else
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject != null && plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectFactory()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        SetStateIdleServerRpc();
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        currentState.Value = State.Idle;
    }


    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int index)
    {
        fryingTimer.Value = 0f;
        currentState.Value = State.Frying;
        SetFryingRecipeFactoryClientRpc(index);
    }

    [ClientRpc]
    private void SetFryingRecipeFactoryClientRpc(int index)
    {
        KitchenObjectFactory kitchenObjectFactory = KitchenGameMultiplayer.Instance.GetKitchenObjectFactoryFromIndex(index);
        fryingRecpieFactory = GetFryingRecipeFactoryWithInput(kitchenObjectFactory);
    }

    [ClientRpc]
    private void SetBurningRecipeFactoryClientRpc(int index)
    {
        KitchenObjectFactory kitchenObjectFactory = KitchenGameMultiplayer.Instance.GetKitchenObjectFactoryFromIndex(index);
        burningRecipeFactory = GetBurningRecipeFactoryWithInput(kitchenObjectFactory);
    }

    private bool HasRecipeWithInput(KitchenObjectFactory inputKitchenObjectFactory)
    {
        FryingRecpieFactory fryingRecipeSO = GetFryingRecipeFactoryWithInput(inputKitchenObjectFactory);
        return fryingRecipeSO != null;
    }

    private FryingRecpieFactory GetFryingRecipeFactoryWithInput(KitchenObjectFactory inputKitchenObjectFactory)
    {
        foreach (FryingRecpieFactory fryingRecpieFactory in FryingRecpieFactories)
        {
            if (fryingRecpieFactory.input == inputKitchenObjectFactory)
            {
                return fryingRecpieFactory;
            }
        }
        return null;
    }

    private BurningRecipeFactory GetBurningRecipeFactoryWithInput(KitchenObjectFactory inputKitchenObjectFactory)
    {
        foreach (BurningRecipeFactory burningRecpieFactory in burningRecipeFactories)
        {
            if (burningRecpieFactory.input == inputKitchenObjectFactory)
            {
                return burningRecpieFactory;
            }
        }
        return null;
    }

    //private void Start()
    //{
    //    StartCoroutine(HandleFryTimer());
    //}

    //private IEnumerator HandleFryTimer()
    //{
    //    yield return new WaitForSeconds(1f);
    //}
}
