using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System.Collections;

public class LoginRegisterUI : MonoBehaviour
{
    public UIDocument uIDocument;

    private TextField usernameField;
    private TextField emailField;
    private TextField passwordField;
    private Button loginButton;
    private Button registerButton;
    private Label messageLabel;

    private const string BASE_URL = "http://localhost:3000";

    void Start()
    {
        if (uIDocument == null)
        {
            Debug.LogError("❌ uIDocument no está asignado en el Inspector.");
            return;
        }

        var root = uIDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("❌ rootVisualElement es null. Revisa que el UIDocument está en la jerarquía.");
            return;
        }

        usernameField = root.Q<TextField>("username");
        emailField = root.Q<TextField>("email");
        passwordField = root.Q<TextField>("password");
        loginButton = root.Q<Button>("loginBtn");
        registerButton = root.Q<Button>("registerBtn");
        messageLabel = root.Q<Label>("messageLabel");

        if (usernameField == null || emailField == null || passwordField == null || loginButton == null || registerButton == null || messageLabel == null)
        {
            Debug.LogError("❌ Uno o más elementos del UI son null. Revisa que los IDs en UI Builder coincidan exactamente.");
            return;
        }

        loginButton.clicked += () => StartCoroutine(Login());
        registerButton.clicked += () => StartCoroutine(Register());
    }


    IEnumerator Login()
    {
        string jsonData = JsonUtility.ToJson(new UserData(usernameField.value, emailField.value, passwordField.value));

        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ShowMessage("Login exitoso", true);
                Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);
            }
            else
            {
                ShowMessage("Error al hacer login", false);
                Debug.LogError("Error al hacer login: " + request.error);
            }
        }
    }

    IEnumerator Register()
    {
        string jsonData = JsonUtility.ToJson(new UserData(usernameField.value, emailField.value, passwordField.value));

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
                Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);
            }
            else
            {
                ShowMessage("Error al hacer registro", false);
                Debug.LogError("Error al hacer registro: " + request.error);
            }
        }
    }

    void ShowMessage(string message, bool isSuccess)
    {
        messageLabel.text = message;
        messageLabel.RemoveFromClassList("success");
        messageLabel.RemoveFromClassList("error");

        if (isSuccess)
            messageLabel.AddToClassList("success");
        else
            messageLabel.AddToClassList("error");
    }

    [System.Serializable]
    public class UserData
    {
        public string username;
        public string email;
        public string password;

        public UserData(string username, string email, string password)
        {
            this.username = username;
            this.email = email;
            this.password = password;
        }
    }
}
