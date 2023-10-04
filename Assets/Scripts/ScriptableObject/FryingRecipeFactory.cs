using UnityEngine;

[CreateAssetMenu()]
public class FryingRecpieFactory : ScriptableObject
{

    public KitchenObjectFactory input;
    public KitchenObjectFactory output;
    public float fryingTimerMax;

}
