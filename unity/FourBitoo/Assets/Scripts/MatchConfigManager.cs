using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class MatchConfigManager : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
    // Comentado para centrarnos en el temporizador
    // public Transform[] spawnPoints; // Puntos de spawn para los jugadores
    // public GameObject canvasPrefab; // Prefab del canvas que contiene el dropdown y la imagen del jugador
    // public Button confirmButton; // Botón de confirmar

    // Propiedad pública para acceder a la configuración del partido
    public static MatchConfigManager Instance { get; private set; }
    public MatchConfig CurrentMatchConfig { get; private set; }

    // Comentado para centrarnos en el temporizador
    // private List<GameObject> players = new List<GameObject>();

    void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Comentado para centrarnos en el temporizador
        // CheckSpawnPoints();
        StartCoroutine(LoadMatchConfig());

        // Comentado para centrarnos en el temporizador
        /*
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => StartCoroutine(SaveSelectedPlayers()));
        }
        */
    }

    // Comentado para centrarnos en el temporizador
    /*
    void CheckSpawnPoints()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("❌ No se han asignado spawn points.");
            return;
        }

        Debug.Log($"🏁 Número de spawn points: {spawnPoints.Length}");
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError($"❌ Spawn point {i + 1} no está asignado.");
            }
            else
            {
                Debug.Log($"✅ Spawn point {i + 1} está en la posición {spawnPoints[i].position}.");
            }
        }
    }
    */

    IEnumerator LoadMatchConfig()
    {
        Debug.Log("🔄 Consultando configuración del partido desde el backend...");

        UnityWebRequest request = UnityWebRequest.Get($"{URL}/getMatchConfig");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log($"📥 Respuesta del servidor: {jsonResponse}");

            CurrentMatchConfig = JsonUtility.FromJson<MatchConfig>(jsonResponse);
            
            // Convertir la duración del partido de minutos a segundos
            if (CurrentMatchConfig != null && CurrentMatchConfig.matchDuration > 0)
            {
                CurrentMatchConfig.matchDurationSeconds = CurrentMatchConfig.matchDuration * 60;
                Debug.Log($"⏱️ Duración del partido: {CurrentMatchConfig.matchDuration} minutos ({CurrentMatchConfig.matchDurationSeconds} segundos)");
            }
            else
            {
                Debug.LogWarning("⚠️ La duración del partido es 0 o inválida. Usando valor por defecto.");
                CurrentMatchConfig.matchDurationSeconds = 300; // 5 minutos por defecto
            }
            
            Debug.Log($"🎮 Número de jugadores recibidos: {CurrentMatchConfig.selectedPlayer}");
            Debug.Log($"🥅 Goles para ganar: {CurrentMatchConfig.goalsToWin}");

            // Comentado para evitar el error de los spawn points
            // SetupPlayers(CurrentMatchConfig.selectedPlayer);
        }
        else
        {
            Debug.LogError("❌ Error al obtener la configuración del partido: " + request.error);
            // Configuración por defecto en caso de error
            CurrentMatchConfig = new MatchConfig
            {
                id = 0,
                matchDuration = 5, // 5 minutos por defecto
                matchDurationSeconds = 300, // 5 minutos en segundos
                goalsToWin = 3,
                selectedPlayer = 4
            };
        }
    }

    // Comentado para centrarnos en el temporizador
    /*
    void SetupPlayers(int selectedPlayer)
    {
        Debug.Log($"♻️ Eliminando jugadores anteriores... Número de jugadores seleccionados: {selectedPlayer}");

        // Elimina jugadores existentes
        foreach (GameObject player in players)
        {
            Destroy(player);
        }
        players.Clear();

        // Verificar que hay puntos de spawn disponibles
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("❌ Error: No hay puntos de spawn configurados. Añade spawn points al MatchConfigManager en el Inspector.");
            return;
        }

        for (int i = 0; i < selectedPlayer; i++)
        {
            // Obtener la posición de spawn desde el array
            Vector3 newPosition = spawnPoints[i % spawnPoints.Length].position;

            // Instanciar el prefab en la posición calculada
            GameObject canvasInstance = Instantiate(canvasPrefab, Vector3.zero, Quaternion.identity);
            canvasInstance.transform.SetParent(transform, false);

            // Asegurar que el objeto esté activo
            canvasInstance.SetActive(true);

            // Ajustar la escala para evitar problemas de visibilidad
            canvasInstance.transform.localScale = Vector3.one;

            // Encontrar el DropdownPersonajes dentro del prefab
            Transform dropdownTransform = canvasInstance.transform.Find("DropdownPersonajes");
            if (dropdownTransform != null)
            {
                // Mover el DropdownPersonajes a la posición del spawn point
                dropdownTransform.position = newPosition;
                dropdownTransform.localScale = Vector3.one;

                Debug.Log($"🎯 DropdownPersonajes {i + 1} instanciado en: {dropdownTransform.position} con escala: {dropdownTransform.localScale}");
            }
            else
            {
                Debug.LogError($"❌ No se encontró el DropdownPersonajes dentro del prefab para el jugador {i + 1}.");
            }

            // Guardar referencia al objeto jugador
            players.Add(canvasInstance);
        }
    }

    // Método para guardar jugadores seleccionados en el backend
    IEnumerator SaveSelectedPlayers()
    {
        // Crear el formulario con los datos del equipo
        WWWForm teamForm = new WWWForm();
        string teamName = "Equipo_" + Random.Range(1000, 9999); // Nombre aleatorio para el equipo
        teamForm.AddField("name", teamName);

        Debug.Log($"🏆 Guardando equipo: {teamName}");

        // Recorrer los jugadores para añadirlos al formulario
        for (int i = 0; i < players.Count; i++)
        {
            GameObject playerObj = players[i];
            Transform captionTransform = playerObj.transform.Find("DropdownPersonajes/CaptionImage");
            
            if (captionTransform == null)
            {
                Debug.LogError($"❌ No se encontró CaptionImage para el jugador {i + 1}.");
                continue; // Continuar con el siguiente jugador
            }

            Image captionImage = captionTransform.GetComponent<Image>();
            if (captionImage == null)
            {
                Debug.LogError($"❌ El CaptionImage no tiene un componente Image en el jugador {i + 1}.");
                continue; // Continuar con el siguiente jugador
            }

            // Obtener el nombre del sprite asociado al CaptionImage
            string spriteName = captionImage.sprite.name; // Nombre del sprite seleccionado
            Debug.Log($"🎨 Sprite seleccionado para el jugador {i + 1}: {spriteName}");

            // Agregar los datos del jugador al formulario
            teamForm.AddField($"players[{i}][name]", $"Player_{i + 1}"); // Nombre del jugador
            teamForm.AddField($"players[{i}][img]", spriteName); // Nombre del sprite como imagen
        }

        UnityWebRequest teamRequest = UnityWebRequest.Post($"{URL}/teams", teamForm);

        yield return teamRequest.SendWebRequest();

        if (teamRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Equipo {teamName} y jugadores guardados correctamente.");
        }
        else
        {
            Debug.LogError($"❌ Error al guardar el equipo y jugadores: {teamRequest.error}");
        }
    }
    */
}

[System.Serializable]
public class MatchConfig
{
    public int id;
    public int matchDuration;
    public int matchDurationSeconds;
    public int goalsToWin;
    public int selectedPlayer;
}

[System.Serializable]
public class PlayerData
{
    public string name;
    public string img;

    public PlayerData(string name, string img)
    {
        this.name = name;
        this.img = img;
    }
}