using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ShopItem : MonoBehaviour
{
    public Image itemImage;
    public Text itemName;
    public Transform shopItemContainer;
    public Text itemPrice;
    private ShopItemData itemData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetItemData(ShopItemData data)
    {
        itemData = data;
        itemName.text = data.name;
        itemPrice.text = data.price.ToString() + "Fourbits ";
        StartCoroutine(LoadImage(data.imageUrl));
    }

    IEnumerator LoadImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        }
        else
        {
            Debug.LogError("Error al cargar la imagen: " + request.error);
        }
    }

    public void OnBuyButtonClick()
    {
        FindObjectOfType<PurchaseHandler>().BuyItem(itemData.id);
        Debug.Log("Comprando: " + itemData.name);
    }


}
