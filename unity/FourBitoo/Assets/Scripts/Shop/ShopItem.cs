using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro; // Importa TextMeshPro

public class ShopItem : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemName; // Nombre del ítem
    public TextMeshProUGUI itemPrice; // Precio del ítem
    public TextMeshProUGUI itemDescription; // Descripción del ítem
    private ShopItemData itemData;

    public void SetItemData(ShopItemData data)
    {
        itemData = data;
        itemName.text = data.name;
        itemPrice.text = data.price.ToString() + " Fourbits";
        itemDescription.text = data.description; // Asigna la descripción
        StartCoroutine(LoadImage(data.img));
    }

    IEnumerator LoadImage(string imageName)
    {
        if (string.IsNullOrEmpty(imageName))
        {
            Debug.LogError("Error: El nombre de la imagen es nulo o vacío.");
            yield break;
        }

        string baseUrl = "http://localhost:4000/uploads/items/";
        string fullUrl = baseUrl + imageName;

        Debug.Log("Cargando imagen desde: " + fullUrl);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogError("Error al cargar la imagen desde: " + fullUrl + " - " + request.error);
        }
    }

    public void OnBuyButtonClick()
    {
        StartCoroutine(BuyItemRequest());
    }

    IEnumerator BuyItemRequest()
    {
        string buyUrl = "http://localhost:4000/shop/buy";

        int userId = 7;

        if (userId == 0)
        {
            Debug.LogError("Error: No se encontró el ID del usuario en UserStore.");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("itemId", itemData.id);
        form.AddField("quantity", 1);

        UnityWebRequest request = UnityWebRequest.Post(buyUrl, form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Compra exitosa: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error al comprar: " + request.error);
        }
    }


}