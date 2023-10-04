using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RecipeFactory : ScriptableObject
{
    public List<KitchenObjectFactory> kitchenObjectFactories;
    public string recipeName;
}
