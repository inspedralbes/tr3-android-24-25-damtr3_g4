using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LandingToLogin : MonoBehaviour
{
    public Button loginBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (loginBtn != null)
        {
            // Agrega un listener para que el botón cargue la escena de login
            loginBtn.onClick.AddListener(() => LoadLoginScene());
        }
        else
        {
            Debug.LogError("❌ El botón de login no está asignado en el Inspector.");
        }
        
    }

    void LoadLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
