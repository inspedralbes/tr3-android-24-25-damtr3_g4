using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System.Collections;

public class RegisterScriptUI : MonoBehaviour
{
    public UIDocument uIDocument;
    private TextField usernameField;
    private TextField emailField;
    private TextField passwordField;
    private TextField confirmPasswordField;
    private Button registerButton;
    // private Button backButton;
    private Label messageLabel;

    private const string BASE_URL = "http://localhost:3000";

    void Start()
    {
        var root = uIDocument.rootVisualElement;
        usernameField = root.Q<TextField>("username");
        emailField = root.Q<TextField>("email");
        passwordField = root.Q<TextField>("password");
        confirmPasswordField = root.Q<TextField>("confirmPassword");
        registerButton = root.Q<Button>("registerBtn");
        // backButton = root.Q<Button>("backBtn");
        messageLabel = root.Q<Label>("messageLabel");

        if (registerButton != null)
            registerButton.clicked += () => StartCoroutine(Register());

        // if (backButton != null)
        //     backButton.clicked += () => SceneManager.LoadScene("LoginScene"); // 🔄 Vuelve a la escena de login
    }

    IEnumerator Register()
    {
        if (passwordField.value != confirmPasswordField.value)
        {
            ShowMessage("Las contraseñas no coinciden", false);
            yield break;
        }

        string jsonData = JsonUtility.ToJson(new RegisterData(usernameField.value, emailField.value, passwordField.value));

        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ShowMessage("Registro exitoso", true);
                Debug.Log("Registro exitoso: " + request.downloadHandler.text);
                SceneManager.LoadScene("LoginScene"); // 🔄 Redirige a Login tras registrarse
            }
            else
            {
                ShowMessage("Error en el registro", false);
            }
        }
    }

    void ShowMessage(string message, bool isSuccess)
    {
        messageLabel.text = message;
    }

    [System.Serializable]
    public class RegisterData
    {
        public string username;
        public string email;
        public string password;

        public RegisterData(string username, string email, string password)
        {
            this.username = username;
            this.email = email;
            this.password = password;
        }
    }
}
