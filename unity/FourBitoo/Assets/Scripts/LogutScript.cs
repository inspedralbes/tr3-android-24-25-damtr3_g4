using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LogutScript : MonoBehaviour
{
    public Button logoutButton;
    public WelcomeTextScript welcomeTextScript;
    public GameObject loginButton;
    void Start()
    {

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(Logout);
        }
        else
        {
            Debug.Log("No se ha encontrado el botón de logout");
        }
    }

    // Update is called once per frame
    public void Logout()
    {
        // Ensure UserStore is initialized
        UserStore.Initialize();

        Debug.Log($"Datos actuales de UserStore antes de cerrar sesión: " +
                  $"ID: {UserStore.Instance.mainUser?.id}, " +
                  $"Nombre: {UserStore.Instance.mainUser?.username}, " +
                  $"Email: {UserStore.Instance.mainUser?.email}, " +
                  $"Token: {UserStore.Instance.mainUser?.token}");

        Debug.Log("Cerrando sesión...");
        UserStore.Instance.ClearUserData();

        Debug.Log($"Datos de UserStore después de cerrar sesión: " +
                  $"ID: {UserStore.Instance.mainUser?.id}, " +
                  $"Nombre: {UserStore.Instance.mainUser?.username}, " +
                  $"Email: {UserStore.Instance.mainUser?.email}, " +
                  $"Token: {UserStore.Instance.mainUser?.token}");
        Debug.Log("Sesión cerrada");

        if (loginButton != null)
        {
            loginButton.SetActive(true);
        }
        else
        {
            Debug.LogWarning("loginButton no está asignado.");
        }

        if (logoutButton != null)
        {
            logoutButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("logoutButton no está asignado.");
        }

        if (welcomeTextScript != null)
        {
            // Update welcome text even if mainUser is null
            welcomeTextScript.UpdateWelcomeText();
        }
        else
        {
            Debug.LogWarning("welcomeTextScript is null. Cannot update welcome text.");
        }
    }
}
