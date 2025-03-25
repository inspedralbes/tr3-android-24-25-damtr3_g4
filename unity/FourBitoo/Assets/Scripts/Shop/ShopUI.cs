using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public Button[] buyButtons; // Botones de compra en la tienda
    private PurchaseHandler purchaseHandler;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        purchaseHandler = FindObjectOfType<PurchaseHandler>();

        if (purchaseHandler == null)
        {
            Debug.LogError("PurchaseHandler no encontrado en la escena.");
            return;
        }

        // Asignar la acción de compra a cada botón
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int itemId = i + 1; // Asumimos que los IDs de los ítems son 1, 2, 3, etc.
            buyButtons[i].onClick.AddListener(() => BuyItem(itemId));
        }
    }

    // Update is called once per frame
    void BuyItem(int itemId)
    {
        int userId = UserStore.Instance.id;

        if (userId == 0)
        {
            Debug.LogError("Error: No se encontró el ID del usuario en UserStore. No se puede realizar la compra.");
            return;
        }

        Debug.Log($"Intentando comprar el item {itemId} para el usuario {userId}");
        purchaseHandler.BuyItem(itemId);
    }
}