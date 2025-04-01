using UnityEngine;
using UnityEngine.UI;

public class LoginButtonManager : MonoBehaviour
{
    public Button loginButton;
    void Start()
    {
        UpdateLoginButtonVisibility();
    }

    // Update is called once per frame
    public void UpdateLoginButtonVisibility()
{
    if (UserStore.Instance.mainUser != null && UserStore.Instance.mainUser.id != 0)
    {
        loginButton.gameObject.SetActive(false);
    }
    else
    {
        loginButton.gameObject.SetActive(true);
    }
}
}
