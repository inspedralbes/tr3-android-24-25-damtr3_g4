using System;

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
    }

    public void ClearUserData()
    {
        id = 0;
        name = null;
        email = null;
        token = null;
    }
}