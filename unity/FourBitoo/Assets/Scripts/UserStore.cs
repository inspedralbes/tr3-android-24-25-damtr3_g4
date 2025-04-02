using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] // Solo una vez por clase que deba ser serializada
public class UserPlayerData // Cambiado de PlayerData a UserPlayerData
{
    public int id;
    public string name;
    public string spriteImage; // Nombre del sprite del personaje
    public Dictionary<int, Vector2[]> trajectories = new Dictionary<int, Vector2[]>(); // Trayectorias por turno

    public UserPlayerData(int id, string name)
    {
        this.id = id;
        this.name = name;
    }

    public UserPlayerData(int id, string name, string spriteImage)
    {
        this.id = id;
        this.name = name;
        this.spriteImage = spriteImage;
    }
}

[Serializable] // Solo una vez por clase que deba ser serializada
public class UserData
{
    public int id;
    public string username;
    public string email;
    public string token;
    public string selectedBadge; // Escudo seleccionado para el equipo

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

    // Asegura que el usuario principal esté inicializado
    private void EnsureMainUserExists()
    {
        if (mainUser == null)
        {
            mainUser = new UserData(1, "DefaultUser", "", "");
            Debug.Log("Usuario principal inicializado con valores predeterminados");
        }
    }

    // Asegura que el usuario invitado esté inicializado
    private void EnsureGuestUserExists()
    {
        if (guestUser == null)
        {
            guestUser = new UserData(2, "DefaultGuest", "", "");
            Debug.Log("Usuario invitado inicializado con valores predeterminados");
        }
    }

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
        if (isMainUser)
        {
            EnsureMainUserExists();
            mainUser.players.Add(new UserPlayerData(playerId, playerName));
        }
        else
        {
            EnsureGuestUserExists();
            guestUser.players.Add(new UserPlayerData(playerId, playerName));
        }
        
        SaveUserData();
        Debug.Log($"Jugador '{playerName}' agregado a {(isMainUser ? "Usuario Principal" : "Usuario Invitado")}");
    }

    // Nuevo método para agregar jugador con su sprite
    public void AddPlayerToUser(bool isMainUser, int playerId, string playerName, string spriteImage)
    {
        if (isMainUser)
        {
            EnsureMainUserExists();
            mainUser.players.Add(new UserPlayerData(playerId, playerName, spriteImage));
        }
        else
        {
            EnsureGuestUserExists();
            guestUser.players.Add(new UserPlayerData(playerId, playerName, spriteImage));
        }
        
        SaveUserData();
        Debug.Log($"Jugador '{playerName}' con sprite '{spriteImage}' agregado a {(isMainUser ? "Usuario Principal" : "Usuario Invitado")}");
    }

    // Método para establecer el escudo del equipo
    public void SetTeamBadge(bool isMainUser, string badgeName)
    {
        if (isMainUser)
        {
            EnsureMainUserExists();
            mainUser.selectedBadge = badgeName;
        }
        else
        {
            EnsureGuestUserExists();
            guestUser.selectedBadge = badgeName;
        }
        
        SaveUserData();
        Debug.Log($"Escudo '{badgeName}' establecido para {(isMainUser ? "Usuario Principal" : "Usuario Invitado")}");
    }

    // Método para obtener el escudo del equipo
    public string GetTeamBadge(bool isMainUser)
    {
        if (isMainUser)
        {
            if (mainUser == null) return null;
            return mainUser.selectedBadge;
        }
        else
        {
            if (guestUser == null) return null;
            return guestUser.selectedBadge;
        }
    }

    // Método para obtener el escudo del equipo (Alias para GetTeamBadge para compatibilidad)
    public string GetSelectedBadge()
    {
        return GetTeamBadge(true);
    }

    // Método para obtener la lista de jugadores
    public List<UserPlayerData> GetPlayerList(bool isMainUser)
    {
        if (isMainUser)
        {
            if (mainUser == null) return new List<UserPlayerData>();
            return mainUser.players;
        }
        else
        {
            if (guestUser == null) return new List<UserPlayerData>();
            return guestUser.players;
        }
    }

    // Método para limpiar la lista de jugadores
    public void ClearPlayers(bool isMainUser)
    {
        if (isMainUser)
        {
            if (mainUser == null)
            {
                EnsureMainUserExists();
            }
            mainUser.players.Clear();
        }
        else
        {
            if (guestUser == null)
            {
                EnsureGuestUserExists();
            }
            guestUser.players.Clear();
        }
        
        SaveUserData();
        Debug.Log($"Lista de jugadores limpiada para {(isMainUser ? "Usuario Principal" : "Usuario Invitado")}");
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