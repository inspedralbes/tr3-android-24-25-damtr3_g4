using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WelcomeTextScript : MonoBehaviour
{
    public TMP_Text welcomeText;

    void Start()
    {
        // Explicitly initialize UserStore
        UserStore.Initialize();

        if (UserStore.Instance.mainUser == null)
        {
            Debug.LogWarning("mainUser is null. Initializing default mainUser.");
            UserStore.Instance.SetMainUser(0, "DefaultUser", "default@example.com", "defaultToken");
        }

        if (welcomeText == null)
        {
            Debug.LogWarning("WelcomeText is not assigned in the inspector.");
        }
        UpdateWelcomeText();
    }

    public void UpdateWelcomeText()
    {
        if (UserStore.Instance == null || UserStore.Instance.mainUser == null)
        {
            Debug.LogWarning("UserStore.Instance or mainUser is null. Cannot update welcome text.");
            if (welcomeText != null)
            {
                welcomeText.text = "Benvingut";
            }
            return;
        }

        Debug.Log($"UserStore.Instance.id: {UserStore.Instance.mainUser.id}, UserStore.Instance.name: {UserStore.Instance.mainUser.username}");
        if (!string.IsNullOrEmpty(UserStore.Instance.mainUser.username))
        {
            if (welcomeText != null)
            {
                welcomeText.text = $"Benvingut {UserStore.Instance.mainUser.username}";
            }
        }
        else
        {
            if (welcomeText != null)
            {
                welcomeText.text = "Benvingut";
            }
        }
    }
}
