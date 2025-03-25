using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LogutScript : MonoBehaviour
{
    public Button logoutButton;
    public WelcomeTextScript welcomeTextScript;
    void Start()
    {

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(Logout);
        }else{
            Debug.Log("No se ha encontrado el botón de logout");
        }
    }

    // Update is called once per frame
    void Logout()
    {
        Debug.Log($"Datos actuales de UserStore antes de cerrar sesión: " +
                  $"ID: {UserStore.Instance.id}, " +
                  $"Nombre: {UserStore.Instance.username}, " +
                  $"Email: {UserStore.Instance.email}, " +
                  $"Token: {UserStore.Instance.token}");
        Debug.Log("Cerrando sesión...");
        UserStore.Instance.ClearUserData();

        Debug.Log($"Datos de UserStore después de cerrar sesión: " +
                  $"ID: {UserStore.Instance.id}, " +
                  $"Nombre: {UserStore.Instance.username}, " +
                  $"Email: {UserStore.Instance.email}, " +
                  $"Token: {UserStore.Instance.token}");
        Debug.Log("Sesión cerrada");

        if (welcomeTextScript != null)
        {
            welcomeTextScript.UpdateWelcomeText();
        }

    }
}
