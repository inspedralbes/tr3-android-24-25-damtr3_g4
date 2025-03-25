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
         Debug.Log($"UserStore.Instance.id: {UserStore.Instance.id}, UserStore.Instance.name: {UserStore.Instance.username}");
        if(UserStore.Instance.id != 0)
        {
            welcomeText.text = $"Benvingut {UserStore.Instance.username}";
        }else{
            welcomeText.text = "Benvingut";
        }
    }
}
