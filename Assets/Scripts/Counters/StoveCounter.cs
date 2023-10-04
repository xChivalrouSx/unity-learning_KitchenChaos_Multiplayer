using System;
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

    private State currentState;
    private float fryingTimer;
    private float burnedTimer;
    private FryingRecpieFactory fryingRecpieFactory;
    private BurningRecipeFactory burningRecipeFactory;

    private void Start()
    {
        currentState = State.Idle;
    }

    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (currentState)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecpieFactory.fryingTimerMax
                    });

                    if (fryingTimer > fryingRecpieFactory.fryingTimerMax)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(fryingRecpieFactory.output, this);

                        currentState = State.Fried;
                        burnedTimer = 0f;
                        burningRecipeFactory = GetBurningRecipeFactoryWithInput(GetKitchenObject().GetKitchenObjectFactory());

                        OnStateChange?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = currentState
                        });
                    }
                    break;
                case State.Fried:
                    burnedTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
                    {
                        progressNormalized = burnedTimer / burningRecipeFactory.burningTimerMax
                    });

                    if (burnedTimer > burningRecipeFactory.burningTimerMax)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(burningRecipeFactory.output, this);
                        currentState = State.Burned;

                        OnStateChange?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = currentState
                        });

                        OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
                        {
                            progressNormalized = 0f
                        });
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
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    fryingRecpieFactory = GetFryingRecipeFactoryWithInput(GetKitchenObject().GetKitchenObjectFactory());
                    currentState = State.Frying;
                    fryingTimer = 0f;

                    OnStateChange?.Invoke(this, new OnStateChangedEventArgs
                    {
                        state = currentState
                    });

                    OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecpieFactory.fryingTimerMax
                    });
                }
            }
        }
        else
        {
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                currentState = State.Idle;

                OnStateChange?.Invoke(this, new OnStateChangedEventArgs
                {
                    state = currentState
                });

                OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
                {
                    progressNormalized = 0f
                });
            }
            else
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject != null && plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectFactory()))
                    {
                        GetKitchenObject().DestroySelf();

                        currentState = State.Idle;

                        OnStateChange?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = currentState
                        });

                        OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }
                }
            }
        }
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
