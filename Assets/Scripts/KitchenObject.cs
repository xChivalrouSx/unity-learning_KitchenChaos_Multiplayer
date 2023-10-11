using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private KitchenObjectFactory kitchenObjectFactory;

    private IKitchenObjectParent kitchenObjectParent;

    public KitchenObjectFactory GetKitchenObjectFactory()
    {
        return kitchenObjectFactory;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }
        this.kitchenObjectParent = kitchenObjectParent;
        kitchenObjectParent.SetKitchenObject(this);

        //transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        //transform.localPosition = Vector3.zero;
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {
        kitchenObjectParent.ClearKitchenObject();
        Destroy(gameObject);
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        plateKitchenObject = null;
        return false;
    }

    public static void SpawnKitchenObject(KitchenObjectFactory kitchenObjectFactory, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectFactory, kitchenObjectParent);
    }

}
