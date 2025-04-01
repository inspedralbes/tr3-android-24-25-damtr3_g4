using UnityEngine;
using UnityEngine.UI;

public class LogoutButtonManager : MonoBehaviour
{
    public Button logoutButton;

    void Start()
    {
        UpdateLogoutButtonVisibility();
    }

    public void UpdateLogoutButtonVisibility()
    {
        if (UserStore.Instance.mainUser != null && UserStore.Instance.mainUser.id != 0)
        {
            logoutButton.gameObject.SetActive(true);
        }
        else
        {
            logoutButton.gameObject.SetActive(false);
        }
    }
}