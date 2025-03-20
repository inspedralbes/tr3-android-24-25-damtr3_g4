using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfigToGame : MonoBehaviour
{
    public Button gameButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameButton != null)
        {
            gameButton.onClick.AddListener(() => LoadScene());
        } else {
            Debug.LogError("gameButton is not set in the inspector");
        }
    }

    void LoadScene()
    {
        SceneManager.LoadScene("cesped");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
