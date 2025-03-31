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

    private UserStore() { mainUser = new UserData(0, "", "", ""); guestUser = new UserData(0, "", "", ""); } // Inicializar usuarios

    public void SetMainUser(int id, string username, string email, string token)
    {
        mainUser = new UserData(id, username, email, token);
        Debug.Log($"Usuario principal actualizado: {username} (ID={id})");

        SaveUserData();
    }

    public void SetGuestUser(int id, string username, string email, string token)
    {
        guestUser = new UserData(id, username, email, token); // Generar usuario invitado
        Debug.Log($"SetGuestUser llamado con: ID={id}, Nombre={username}, Email={email}, Token={token}");
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
        Debug.Log($"Guardando datos de usuario: {json}"); // Línea añadida
        PlayerPrefs.SetString("userData", json);
        PlayerPrefs.Save();
    }

    private void LoadUserData()
    {
        if (PlayerPrefs.HasKey("userData"))
        {
            string jsonData = PlayerPrefs.GetString("userData");
            Debug.Log($"Cargando datos de usuario: {jsonData}"); // Línea añadida
            if (!string.IsNullOrEmpty(jsonData))
            {
                JsonUtility.FromJsonOverwrite(jsonData, this);
            }
        }
    }

    public void DebugGuestUser()
    {
        if (guestUser != null)
        {
            Debug.Log($"Guest User Details:\nID: {guestUser.id}\nUsername: {guestUser.username}\nEmail: {guestUser.email}\nToken: {guestUser.token}");
            foreach (var player in guestUser.players)
            {
                Debug.Log($"Player ID: {player.id}, Name: {player.name}");
                foreach (var trajectory in player.trajectories)
                {
                    Debug.Log($"Turn: {trajectory.Key}, Trajectory: {trajectory.Value[0]} -> {trajectory.Value[1]}");
                }
            }
        }
        else
        {
            Debug.Log("Guest user is null.");
        }
    }
}