using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListFactory recipeListFactory;

    private List<RecipeFactory> waitingRecipes;
    private float spawnRecipeTimer = 5f;
    private float spawnRecipeTimerMax = 5f;
    private int waitingRecipesMax = 4;

    private int successfulRecipesAmount;

    private void Awake()
    {
        Instance = this;

        waitingRecipes = new List<RecipeFactory>();
        spawnRecipeTimer = spawnRecipeTimerMax;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingRecipes.Count < waitingRecipesMax)
            {
                SpawnNewWaitingRecipeClientRpc(UnityEngine.Random.Range(0, recipeListFactory.recipeFactoryList.Count));
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int recipeFactoryIndex)
    {
        waitingRecipes.Add(recipeListFactory.recipeFactoryList[recipeFactoryIndex]);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipes.Count; i++)
        {
            RecipeFactory waitingRecipeFactory = waitingRecipes[i];
            if (waitingRecipeFactory.kitchenObjectFactories.Count == plateKitchenObject.GetKitchenObjectFactories().Count)
            {
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectFactory recipeKitchenObjectFactory in waitingRecipeFactory.kitchenObjectFactories)
                {
                    bool ingredientFound = false;
                    foreach (KitchenObjectFactory plateKitchenObjectFactory in plateKitchenObject.GetKitchenObjectFactories())
                    {
                        if (plateKitchenObjectFactory == recipeKitchenObjectFactory)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        plateContentsMatchesRecipe = false;
                        break;
                    }
                }
                if (plateContentsMatchesRecipe)
                {
                    DeliverCorrenctRecipeServerRpc(i);
                    return;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrenctRecipeServerRpc(int index)
    {
        DeliverCorrenctRecipeClientRpc(index);
    }

    [ClientRpc]
    private void DeliverCorrenctRecipeClientRpc(int index)
    {
        successfulRecipesAmount++;
        waitingRecipes.RemoveAt(index);
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeFactory> GetWaitingRecipes()
    {
        return waitingRecipes;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipesAmount;
    }

}
