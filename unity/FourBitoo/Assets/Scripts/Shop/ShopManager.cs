using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ShopManager : MonoBehaviour
{
    public Transform shopContainer; // Contenedor donde se mostrarán los objetos
    public GameObject shopItemPrefab; // Prefab del objeto en la tienda

    private string shopApiUrl = "http://localhost:4000";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadShopItems());
    }

    IEnumerator LoadShopItems()
    {
        UnityWebRequest request = UnityWebRequest.Get(shopApiUrl + "/shopItems");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Sucess)
        {
            List<ShopItemData> items = JsonConvert.DeserializeObject<List<ShopItemData>>(request.downloadHandler.text);
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

    // Update is called once per frame

}
