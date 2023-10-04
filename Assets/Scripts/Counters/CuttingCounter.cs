using System;
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
                player.GetKitchenObject().SetKitchenObjectParent(this);
                cuttingProgress = 0;

                CuttingRecpieFactory cuttingRecipeFactory = GetCuttingRecipeFactoryWithInput(GetKitchenObject().GetKitchenObjectFactory());
                if (cuttingRecipeFactory != null)
                {
                    OnProgressChanged?.Invoke(this, new OnProgressChangeEventArgs
                    {
                        progressNormalized = (float)cuttingProgress / cuttingRecipeFactory.cuttingProgressMax
                    });
                }
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

    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject())
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

                if (cuttingProgress >= cuttingRecipeFactory.cuttingProgressMax)
                {
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(outputKitchenObjectFactory, this);
                }
            }
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
