using UnityEngine;

[CreateAssetMenu()]
public class CuttingRecpieFactory : ScriptableObject
{

    public KitchenObjectFactory input;
    public KitchenObjectFactory output;
    public int cuttingProgressMax;

}
