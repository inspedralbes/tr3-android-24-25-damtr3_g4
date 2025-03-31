using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WelcomeTextScript : MonoBehaviour
{
    public TMP_Text welcomeText;
    void Start()
    {
        UpdateWelcomeText();
    }

    // Update is called once per frame
    public void UpdateWelcomeText()
    {
        Debug.Log($"UserStore.Instance.id: {UserStore.Instance.mainUser.id}, UserStore.Instance.name: {UserStore.Instance.mainUser.username}");
        if (UserStore.Instance.mainUser != null && !string.IsNullOrEmpty(UserStore.Instance.mainUser.username))
        {
            welcomeText.text = $"Benvingut {UserStore.Instance.mainUser.username}";
        }
        else
        {
            welcomeText.text = "Benvingut";
        }
    }
}
