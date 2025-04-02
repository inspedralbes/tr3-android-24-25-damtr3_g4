using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] // Solo una vez por clase que deba ser serializada
public class UserPlayerData // Cambiado de PlayerData a UserPlayerData
{
    public int id;
    public string name;
    public Dictionary<int, Vector2[]> trajectories = new Dictionary<int, Vector2[]>(); // Trayectorias por turno

    public UserPlayerData(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

[Serializable] // Solo una vez por clase que deba ser serializada
public class UserData
{
    public int id;
    public string username;
    public string email;
    public string token;

    public List<UserPlayerData> players = new List<UserPlayerData>(); // Cambiado PlayerData a UserPlayerData

    public UserData(int id, string username, string email, string token)
    {
        this.id = id;
        this.username = username;
        this.email = email;
        this.token = token;
    }
}

public class UserStore
{
    private static UserStore instance;

    public UserData mainUser;   // Usuario principal (registrado)
    public UserData guestUser;  // Usuario invitado (derivado del principal)

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

    public void SetMainUser(int id, string username, string email, string token)
    {
        mainUser = new UserData(id, username, email, token);
        Debug.Log($"Usuario principal: {username} (ID={id})");

        SaveUserData();
    }

    public void SetGuestUser(int id, string username, string email, string token)
    {
        guestUser = new UserData(id, username, email, token); // Generar usuario invitado
        Debug.Log($"Usuario invitado: {guestUser.username} (ID={guestUser.id})");
        SaveUserData();
    }

    public void AddPlayerToUser(bool isMainUser, int playerId, string playerName)
    {
        UserData user = isMainUser ? mainUser : guestUser;
        if (user != null)
        {
            user.players.Add(new UserPlayerData(playerId, playerName)); // Cambiado PlayerData a UserPlayerData
            SaveUserData();
            Debug.Log($"Jugador '{playerName}' agregado a {(isMainUser ? "Usuario Principal" : "Usuario Invitado")}");
        }
    }

    public void AddOrUpdateTrajectory(bool isMainUser, int playerIndex, int turn, Vector2 startPoint, Vector2 endPoint)
    {
        UserData user = isMainUser ? mainUser : guestUser;

        if (user != null && playerIndex < user.players.Count)
        {
            UserPlayerData player = user.players[playerIndex]; // Cambiado PlayerData a UserPlayerData
            player.trajectories[turn] = new Vector2[] { startPoint, endPoint };
            SaveUserData();
            Debug.Log($"Trayectoria actualizada para {player.name}, Turno {turn}: {startPoint} -> {endPoint}");
        }
    }

    public Vector2[] GetTrajectory(bool isMainUser, int playerIndex, int turn)
    {
        UserData user = isMainUser ? mainUser : guestUser;

        if (user != null && playerIndex < user.players.Count)
        {
            UserPlayerData player = user.players[playerIndex]; // Cambiado PlayerData a UserPlayerData
            if (player.trajectories.ContainsKey(turn))
                return player.trajectories[turn];
        }
        return null;
    }

    public void ClearUserData()
    {
        mainUser = null;
        guestUser = null;
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