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
    private Button registerButton;
    // private Button backButton;
    private Label messageLabel;

    private const string BASE_URL = "http://localhost:4000";

    void Start()
    {
        var root = uIDocument.rootVisualElement;
        usernameField = root.Q<TextField>("username");
        emailField = root.Q<TextField>("email");
        passwordField = root.Q<TextField>("password");
        registerButton = root.Q<Button>("registerBtn");
        messageLabel = root.Q<Label>("messageLabel");

        Debug.Log($"usernameField: {usernameField}, emailField: {emailField}, passwordField: {passwordField}, registerButton: {registerButton}, messageLabel: {messageLabel}");

        if (registerButton != null)
            registerButton.clicked += () => StartCoroutine(Register());

        // if (backButton != null)
        //     backButton.clicked += () => SceneManager.LoadScene("LoginScene"); // 🔄 Vuelve a la escena de login
    }

    IEnumerator Register()
    {
        Debug.Log($"usernameField: {usernameField.value}, emailField: {emailField.value}, passwordField: {passwordField.value}");

        // Crear los datos del usuario principal
        string jsonDataMainUser = JsonUtility.ToJson(new RegisterData(usernameField.value, emailField.value, passwordField.value));

        // Registrar el mainUser
        using (UnityWebRequest request = new UnityWebRequest(BASE_URL + "/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonDataMainUser);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ShowMessage("Registro exitoso del usuario principal", true);
                Debug.Log("Main User registrado exitosamente: " + request.downloadHandler.text);

                // Parsear la respuesta del servidor para obtener los datos del usuario principal
                ServerResponse mainUserResponse = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);


                // Ahora registrar el guestUser
                string guestEmail = emailField.value.Replace("@", "_guest@");
                string guestPassword = passwordField.value + "_guest";

                string jsonDataGuestUser = JsonUtility.ToJson(new RegisterData(usernameField.value + "_guest", guestEmail, guestPassword));

                using (UnityWebRequest guestRequest = new UnityWebRequest(BASE_URL + "/register", "POST"))
                {
                    byte[] bodyRawGuest = System.Text.Encoding.UTF8.GetBytes(jsonDataGuestUser);
                    guestRequest.uploadHandler = new UploadHandlerRaw(bodyRawGuest);
                    guestRequest.downloadHandler = new DownloadHandlerBuffer();
                    guestRequest.SetRequestHeader("Content-Type", "application/json");

                    yield return guestRequest.SendWebRequest();

                    if (guestRequest.result == UnityWebRequest.Result.Success)
                    {
                        ShowMessage("Usuario invitado registrado exitosamente", true);

                        Debug.Log("Guest User registrado exitosamente: " + guestRequest.downloadHandler.text);

                        // Parsear la respuesta del servidor para obtener los datos del usuario invitado
                        ServerResponse guestUserResponse = JsonUtility.FromJson<ServerResponse>(guestRequest.downloadHandler.text);

                        // Guardar los datos en UserStore
                        UserStore.Instance.SetMainUser(mainUserResponse.user.id, mainUserResponse.user.username, mainUserResponse.user.email, mainUserResponse.user.token);
                        UserStore.Instance.SetGuestUser(guestUserResponse.user.id, guestUserResponse.user.username, guestUserResponse.user.email, guestUserResponse.user.token);

                        // Redirigir a la escena de login
                        SceneManager.LoadScene("LoginScene");
                    }
                    else
                    {
                        Debug.LogError("Error en el registro del usuario invitado: " + guestRequest.error);
                        Debug.LogError("Respuesta del servidor: " + guestRequest.downloadHandler.text);
                        ShowMessage("Error en el registro del usuario invitado", false);
                    }
                }
            }
            else
            {
                ShowMessage("Error en el registro del usuario principal", false);
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
