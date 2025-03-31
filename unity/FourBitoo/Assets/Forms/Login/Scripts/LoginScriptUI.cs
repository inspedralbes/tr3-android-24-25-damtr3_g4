using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Networking; // Importa UnityWebRequest
using System.Collections;

public class LoginScriptUI : MonoBehaviour
{
    public UIDocument uIDocument;
    private TextField emailField;
    private TextField passwordField;
    private Button loginButton;
    private Button registerButton;
    private Label messageLabel;

    private const string BASE_URL = "http://localhost:4000";

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
        // Crear el objeto UserData
        UserData userData = new UserData(emailField.value, passwordField.value);

        // Depurar el contenido de UserData
        Debug.Log($"UserData - Email: {userData.email}, Password: {userData.password}");

        // Convertir UserData a JSON
        string jsonData = JsonUtility.ToJson(userData);

        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log("Raw Response: " + request.downloadHandler.text);

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    ShowMessage("Empty response from server", false);
                    yield break;
                }

                if (!request.GetResponseHeader("Content-Type").Contains("application/json"))
                {
                    Debug.LogError("Invalid response type: " + request.GetResponseHeader("Content-Type"));
                    ShowMessage("Error: Respuesta no válida del servidor", false);
                    yield break;
                }

                try
                {
                    ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);

                    if (response.user != null)
                    {
                        // Guardar los datos del usuario principal en el UserStore
                        ShowMessage("Login exitoso", true);

                        FindFirstObjectByType<LoginButtonManager>()?.UpdateLoginButtonVisibility();
                        SceneManager.LoadScene("Inicio");
                    }
                    else
                    {
                        ShowMessage("Error: Usuario no encontrado", false);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("JSON Parsing Error: " + ex.Message);
                    ShowMessage("Error al procesar la respuesta del servidor", false);
                }
            }
            else
            {
                Debug.LogError("Request Error: " + request.error);
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
    public string username;
    public string email;
    public string token;
}