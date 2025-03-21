using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System.Collections;

public class LoginScriptUI : MonoBehaviour
{
    public UIDocument uIDocument;
    private TextField emailField;
    private TextField passwordField;
    private Button loginButton;
    private Button registerButton;
    private Label messageLabel;

    private const string BASE_URL = "http://localhost:3000";

    void Start()
    {
        var root = uIDocument.rootVisualElement;
        emailField = root.Q<TextField>("email");
        passwordField = root.Q<TextField>("password");
        loginButton = root.Q<Button>("loginBtn");
        registerButton = root.Q<Button>("registerBtn");
        messageLabel = root.Q<Label>("messageLabel");

        if (loginButton != null)
            loginButton.clicked += () => StartCoroutine(Login());

        if (registerButton != null)
            registerButton.clicked += () => SceneManager.LoadScene("RegisterScene"); // 🔄 Cambia de escena
    }

    IEnumerator Login()
    {
        string jsonData = JsonUtility.ToJson(new UserData(emailField.value, passwordField.value));

        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

            if (request.result == UnityWebRequest.Result.Success)
            {
                
                // Puedes cargar otra escena aquí si quieres

                if(response.success){
                    UserStore.Instance.SetUserData(response.user.id, response.user.name, response.user.email, response.user.token);
                
                    ShowMessage("Login exitoso", true);
                    Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);
                }
                else
                {
                    ShowMessage(response.message, false);
                }
            }
            else
            {
                ShowMessage("Error al hacer login", false);
            }
        }
    }

    void ShowMessage(string message, bool isSuccess)
    {
        messageLabel.text = message;
    }

    [System.Serializable]
    public class UserData
    {
        public string email;
        public string password;

        public UserData(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }
}

[System.Serializable]
public class ServerResponse
{
    public bool success;
    public User user;
    public string message;
}

[System.Serializable]
public class User
{
    public int id;
    public string name;
    public string email;
    public string token;
}
