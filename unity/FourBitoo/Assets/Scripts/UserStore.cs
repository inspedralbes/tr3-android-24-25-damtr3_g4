using System;
using UnityEngine;

[Serializable]
public class UserStore
{
    private static UserStore instance;

    public int id;
    public string username;
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

    public void SetUserData(int id, string username, string email, string token)
    {
        this.id = id;
        this.username = username;
        this.email = email;
        this.token = token;

        SaveUserData();

         Debug.Log($"Datos guardados en UserStore: ID={id}, Nombre={username}, Email={email}, Token={token}");
    }

    public void ClearUserData()
    {
        id = 0;
        username = "";
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