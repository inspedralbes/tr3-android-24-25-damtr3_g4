using UnityEngine;
using UnityEngine.UI;

public class LogoutButtonManager : MonoBehaviour
{
    public Button logoutButton; // Botón de logout

    void Start()
    {
        UpdateLogoutButtonVisibility();
    }

    public void UpdateLogoutButtonVisibility()
    {
        if (UserStore.Instance.mainUser.id != 0) // Usuario logueado
        {
            logoutButton.gameObject.SetActive(true); // Mostrar botón de logout
        }
        else // Usuario no logueado
        {
            logoutButton.gameObject.SetActive(false); // Ocultar botón de logout
        }
    }
}