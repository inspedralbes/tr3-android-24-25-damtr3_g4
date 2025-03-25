using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InicioToSeleccionPersonaje : MonoBehaviour
{
    public string seleccionPersonajeSceneName = "SeleccionPersonaje"; // Nombre de la escena de selección de personaje
    public Button iniciarButton; // Botón de iniciar

    public WelcomeTextScript welcomeTextScript;

    void Start()
    {
        if (iniciarButton != null)
        {
            iniciarButton.onClick.AddListener(OnIniciarButtonClick);
        }
        else
        {
            Debug.LogError("El botón de iniciar no está asignado en el Inspector.");
        }
    }

    private void OnIniciarButtonClick()
    {
        if (UserStore.Instance.id != 0) // Verifica si el usuario está logueado
        {
            Debug.Log("Usuario logueado. Redirigiendo a la pantalla de selección de personaje...");
            SceneManager.LoadScene(seleccionPersonajeSceneName); // Cargar la escena de selección de personaje
        }
        else
        {
            Debug.Log("Usuario no logueado. No se puede continuar.");
            if (welcomeTextScript != null)
            {
                welcomeTextScript.welcomeText.text = "Necessita iniciar sessió";
            }
        }
    }
}