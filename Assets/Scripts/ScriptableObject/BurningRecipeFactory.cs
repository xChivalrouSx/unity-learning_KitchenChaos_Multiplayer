using UnityEngine;

[CreateAssetMenu()]
public class BurningRecipeFactory : ScriptableObject
{

    public KitchenObjectFactory input;
    public KitchenObjectFactory output;
    public float burningTimerMax;

}
