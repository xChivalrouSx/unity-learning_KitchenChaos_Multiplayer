using System;
using Unity.Netcode;
using UnityEngine;
using static IHasProgress;

public class CuttingCounter : BaseCounter, IHasProgress
{

    public event EventHandler<OnProgressChangeEventArgs> OnProgressChanged;
    public event EventHandler OnCut;

    [SerializeField] private CuttingRecpieFactory[] cuttingRecipeFactories;

    private int cuttingProgress;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                KitchenObject kitchenObject = player.GetKitchenObject();
                kitchenObject.SetKitchenObjectParent(this);

                InterractLogicPlaceObjectOnCounterServerRpc();
            }
        }
        else
        {
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            else
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject != null && plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectFactory()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InterractLogicPlaceObjectOnCounterServerRpc()
    {
        InterractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InterractLogicPlaceObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
        {
            progressNormalized = 0f
        });

    }

    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject())
        {
            CutObjectServerRpc();
            TestCuttingProgressDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        KitchenObjectFactory outputKitchenObjectFactory = GetOutputForInput(GetKitchenObject().GetKitchenObjectFactory());
        if (outputKitchenObjectFactory != null)
        {
            cuttingProgress++;
            OnCut?.Invoke(this, EventArgs.Empty);

            CuttingRecpieFactory cuttingRecipeFactory = GetCuttingRecipeFactoryWithInput(GetKitchenObject().GetKitchenObjectFactory());
            OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
            {
                progressNormalized = (float)cuttingProgress / cuttingRecipeFactory.cuttingProgressMax
            });


        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        CuttingRecpieFactory cuttingRecipeFactory = GetCuttingRecipeFactoryWithInput(GetKitchenObject().GetKitchenObjectFactory());
        if (cuttingProgress >= cuttingRecipeFactory.cuttingProgressMax)
        {
            KitchenObjectFactory outputKitchenObjectFactory = GetOutputForInput(GetKitchenObject().GetKitchenObjectFactory());
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
            KitchenObject.SpawnKitchenObject(outputKitchenObjectFactory, this);
        }
    }

    private KitchenObjectFactory GetOutputForInput(KitchenObjectFactory inputKitchenObjectFactory)
    {
        CuttingRecpieFactory cuttingRecipeFactory = GetCuttingRecipeFactoryWithInput(inputKitchenObjectFactory);
        if (cuttingRecipeFactory != null)
        {
            return cuttingRecipeFactory.output;
        }
        return null;
    }

    private CuttingRecpieFactory GetCuttingRecipeFactoryWithInput(KitchenObjectFactory inputKitchenObjectFactory)
    {
        foreach (CuttingRecpieFactory cuttingRecipeFactory in cuttingRecipeFactories)
        {
            if (cuttingRecipeFactory.input == inputKitchenObjectFactory)
            {
                return cuttingRecipeFactory;
            }
        }
        return null;
    }

}
