using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectFactory kitchenObjectFactory;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
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
                else
                {
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        if (plateKitchenObject != null && plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectFactory()))
                        {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
        }
    }

}
