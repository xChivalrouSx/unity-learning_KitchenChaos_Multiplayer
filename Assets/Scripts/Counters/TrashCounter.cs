public class TrashCounter : BaseCounter
{

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
        }
    }

}
