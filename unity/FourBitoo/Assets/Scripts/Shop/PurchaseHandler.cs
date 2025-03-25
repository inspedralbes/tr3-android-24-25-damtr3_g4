using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
public class PurchaseHandler : MonoBehaviour
{

    private string purchaseUrl = "http://localhost:4000/shop/buy";
    private int userId => UserStore.Instance.id;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void BuyItem(int itemId)
    {
        if (userId == 0)
        {
            Debug.LogError("Error: No se encontró el ID del usuario en UserStore.");
            return;
        }

        StartCoroutine(SendPurchaseRequest(itemId));
    }

    // Update is called once per frame
    IEnumerator SendPurchaseRequest(int itemId)
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("itemId", itemId);
        form.AddField("quantity", 1);

        UnityWebRequest request = UnityWebRequest.Post(purchaseUrl, form);
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
