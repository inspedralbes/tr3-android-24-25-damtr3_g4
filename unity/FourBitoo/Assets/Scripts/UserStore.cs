using System;
using UnityEngine;

[Serializable]
public class UserStore
{
    private static UserStore instance;

    public int id;
    public string name;
    public string email;
    public string token;

    // Singleton para acceder al store desde cualquier parte
    public static UserStore Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UserStore();
                instance.LoadUserData();
            }
            return instance;
        }
    }

    private UserStore() { }

    public void SetUserData(int id, string name, string email, string token)
    {
        this.id = id;
        this.name = name;
        this.email = email;
        this.token = token;

        SaveUserData();
    }

    public void ClearUserData()
    {
        id = 0;
        name = "";
        email = "";
        token = "";

        PlayerPrefs.DeleteKey("userData");
        PlayerPrefs.Save();
    }

    private void SaveUserData()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("userData", json);
        PlayerPrefs.Save();
    }

    private void LoadUserData()
    {
        if (PlayerPrefs.HasKey("userData"))
        {
            string jsonData = PlayerPrefs.GetString("userData");
            if (!string.IsNullOrEmpty(jsonData))
            {
                JsonUtility.FromJsonOverwrite(jsonData, this);
            }
        }
    }

}