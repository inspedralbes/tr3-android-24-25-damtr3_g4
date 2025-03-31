using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public Transform shopContainer;
    public GameObject shopItemPrefab;

    private string shopApiUrl = "http://localhost:4000/shopItems";

    void Start()
    {
        StartCoroutine(LoadShopItems());
    }

    IEnumerator LoadShopItems()
    {
        UnityWebRequest request = UnityWebRequest.Get(shopApiUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            List<ShopItemData> items = JsonUtility.FromJson<ShopItemList>("{\"items\":" + request.downloadHandler.text + "}").items;

            foreach (var itemData in items)
            {
                GameObject newItem = Instantiate(shopItemPrefab, shopContainer);
                ShopItem shopItem = newItem.GetComponent<ShopItem>();
                shopItem.SetItemData(itemData);
            }
        }
        else
        {
            Debug.LogError("Error al cargar la tienda: " + request.error);
        }
    }
}

[System.Serializable]
public class ShopItemList
{
    public List<ShopItemData> items;
}