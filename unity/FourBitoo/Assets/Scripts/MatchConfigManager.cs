using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class MatchConfigManager : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
    public Transform[] spawnPoints; // Puntos de spawn para los jugadores
    public GameObject canvasPrefab; // Prefab del canvas que contiene el dropdown y la imagen del jugador
    public Button confirmButton; // Botón de confirmar

    // Referencia estática para el patrón Singleton
    public static MatchConfigManager Instance { get; private set; }
    // Propiedad pública para la configuración del partido
    public MatchConfig CurrentMatchConfig { get; private set; }

    private List<GameObject> players = new List<GameObject>();
    private string selectedBadgeName; // Store the selected badge name
    
    // Para determinar si estamos en la pantalla de juego o de selección
    private bool isGameScene = false;
    // Referencia al GrassScreenManager
    private GrassScreenManager grassScreenManager;

    void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // No destruir al cargar nuevas escenas para mantener la configuración
        DontDestroyOnLoad(gameObject);
        
        // Suscribirse al evento de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Este método se llama cuando se carga una nueva escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Determinar si estamos en la escena de juego
        isGameScene = scene.name.Contains("Match") || scene.name.Contains("Grass");
        Debug.Log($"🎬 Escena cargada: {scene.name}, isGameScene: {isGameScene}");
        
        if (isGameScene)
        {
            // Buscar el GrassScreenManager en la nueva escena
            grassScreenManager = FindAnyObjectByType<GrassScreenManager>();
            
            // Si encontramos el GrassScreenManager, inicializar los jugadores
            if (grassScreenManager != null)
            {
                Debug.Log("🔄 Encontrado GrassScreenManager, inicializando jugadores...");
                grassScreenManager.LoadPlayerDataFromMatchConfig();
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró GrassScreenManager en la escena de juego.");
            }
        }
    }

    void OnDestroy()
    {
        // Desuscribirse del evento cuando se destruye este objeto
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Solo verificar spawn points e inicializar componentes si NO estamos en la escena de juego
        if (!isGameScene)
        {
            CheckSpawnPoints();
        }
        
        // Siempre cargamos la configuración del partido
        StartCoroutine(LoadMatchConfig());
        
        // Solo cargar badges y configurar botón de confirmación si NO estamos en la escena de juego
        if (!isGameScene)
        {
            LoadAvailableBadges(); // Load badges dynamically

            // Configurar el evento onClick para guardar los jugadores al presionar el botón
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(() =>
                {
                    Debug.Log("🖱️ Botón de confirmar presionado. Guardando jugadores...");
                    StartCoroutine(SaveSelectedPlayers());
                });
            }
            else
            {
                Debug.LogError("❌ El botón confirmButton no está asignado en el inspector.");
            }
        }
        else
        {
            // Desactivar los componentes visuales que no se usan en la escena de juego
            if (canvasPrefab != null)
            {
                canvasPrefab.SetActive(false);
            }
            
            // Desactivar cualquier otro componente visual de la selección de personajes
            var canvases = GetComponentsInChildren<Canvas>(true);
            foreach (var canvas in canvases)
            {
                if (canvas.gameObject != gameObject)
                {
                    canvas.gameObject.SetActive(false);
                }
            }
        }
    }

    void CheckSpawnPoints()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay spawn points asignados en MatchConfigManager.");
            return;
        }
        
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
            
            // Asegurar que matchDurationSeconds esté establecido correctamente
            if (CurrentMatchConfig.matchDuration > 0)
            {
                CurrentMatchConfig.matchDurationSeconds = CurrentMatchConfig.matchDuration * 60;
                Debug.Log($"⏱️ Duración del partido: {CurrentMatchConfig.matchDuration} minutos ({CurrentMatchConfig.matchDurationSeconds} segundos)");
            }
            else
            {
                Debug.LogWarning("⚠️ La duración del partido es 0 o no válida. Usando valor predeterminado.");
                CurrentMatchConfig.matchDuration = 5;
                CurrentMatchConfig.matchDurationSeconds = 300; // 5 minutos por defecto
            }
            
            Debug.Log($"🎮 Número de jugadores recibidos: {CurrentMatchConfig.selectedPlayer}");

            // Solo configurar jugadores si no estamos en la escena de juego
            if (!isGameScene)
            {
                SetupPlayers(CurrentMatchConfig.selectedPlayer);
            }
            else if (grassScreenManager != null)
            {
                // Si estamos en la escena de juego y hay un GrassScreenManager, usarlo
                grassScreenManager.LoadPlayerDataFromMatchConfig();
                
                // IMPORTANTE: Eliminar cualquier jugador que hayamos creado previamente
                foreach (GameObject player in players)
                {
                    if (player != null)
                    {
                        Destroy(player);
                    }
                }
                players.Clear();
            }
        }
        else
        {
            Debug.LogError("❌ Error al obtener la configuración del partido: " + request.error);
            // Configuración por defecto en caso de error
            CurrentMatchConfig = new MatchConfig
            {
                matchDuration = 5, // 5 minutos por defecto
                matchDurationSeconds = 300, // 300 segundos por defecto
                goalsToWin = 3,
                selectedPlayer = 4
            };
            
            // Solo configurar jugadores si no estamos en la escena de juego
            if (!isGameScene)
            {
                SetupPlayers(CurrentMatchConfig.selectedPlayer);
            }
        }
    }

    void SetupPlayers(int selectedPlayer)
    {
        Debug.Log($"♻️ Eliminando jugadores anteriores... Número de jugadores seleccionados: {selectedPlayer}");

        // Elimina jugadores existentes
        foreach (GameObject player in players)
        {
            Destroy(player);
        }
        players.Clear();

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
                Debug.LogError("❌ No se encontró DropdownPersonajes en el prefab.");
            }

            // Configurar el Canvas en World Space
            Canvas canvas = canvasInstance.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = i * 10; // Asegurar visibilidad

                // Asegurar que el CanvasScaler esté configurado
                CanvasScaler canvasScaler = canvasInstance.GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    canvasScaler = canvasInstance.AddComponent<CanvasScaler>();
                }
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;

                // Asegurar que el GraphicRaycaster esté configurado
                GraphicRaycaster graphicRaycaster = canvasInstance.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster == null)
                {
                    canvasInstance.AddComponent<GraphicRaycaster>();
                }
            }

            Debug.Log($"🎯 Jugador {i + 1} instanciado en: {newPosition} con escala: {canvasInstance.transform.localScale}");

            players.Add(canvasInstance);
        }
    }

    void LoadAvailableBadges()
    {
        // Seleccionar un badge dinámicamente entre 0 y 5
        int badgeIndex = Random.Range(0, 6); // Generar un índice aleatorio entre 0 y 5
        selectedBadgeName = $"DS_DSi_-_Inazuma_Eleven_-_Team_Emblems-removebg-preview_{badgeIndex}";
        Debug.Log($"🎨 Badge seleccionado dinámicamente: {selectedBadgeName}");

        // Verificar si el badge existe
        Sprite badgeSprite = Resources.Load<Sprite>($"Emblems/{selectedBadgeName}");
        if (badgeSprite == null)
        {
            Debug.LogError($"❌ No se encontró el badge con el nombre: {selectedBadgeName}");
        }
        else
        {
            Debug.Log($"✅ Badge encontrado: {selectedBadgeName}");
        }
    }

    IEnumerator SaveSelectedPlayers()
    {
        int userId = 1;
        string teamName = "TeamName";
        
        // Obtener el escudo seleccionado
        GameObject badgeDropdownObject = GameObject.Find("BadgeDropdown");
        if (badgeDropdownObject == null) yield break;
        
        Transform badgeCaptionImageTransform = badgeDropdownObject.transform.Find("CaptionImage");
        if (badgeCaptionImageTransform == null) yield break;
        
        Image badgeCaptionImage = badgeCaptionImageTransform.GetComponent<Image>();
        
        selectedBadgeName = badgeCaptionImage.sprite.name;
        Sprite badgeSprite = Resources.Load<Sprite>($"Emblems/{selectedBadgeName}");
        
        // Guardar el escudo seleccionado en UserStore
        UserStore.Instance.SetTeamBadge(true, selectedBadgeName);
        Debug.Log($"💾 Escudo '{selectedBadgeName}' guardado en UserStore");
        
        // Limpiar jugadores anteriores en UserStore
        UserStore.Instance.ClearPlayers(true);
        
        Texture2D badgeTexture = badgeSprite.texture;
        Texture2D readableBadgeTexture = new Texture2D(badgeTexture.width, badgeTexture.height, TextureFormat.RGBA32, false);
        readableBadgeTexture.SetPixels(badgeTexture.GetPixels());
        readableBadgeTexture.Apply();
        byte[] badgeBytes = readableBadgeTexture.EncodeToPNG();

        WWWForm teamForm = new WWWForm();
        teamForm.AddField("id_user", userId.ToString());
        teamForm.AddField("name", teamName);
        teamForm.AddBinaryData("badge", badgeBytes, $"{selectedBadgeName}.png", "image/png");
        UnityWebRequest teamRequest = UnityWebRequest.Post($"{URL}/teams", teamForm);
        
        yield return teamRequest.SendWebRequest();
        if (teamRequest.result != UnityWebRequest.Result.Success) yield break;

        WWWForm playerForm = new WWWForm(); // Usar un formulario diferente para jugadores
        List<PlayerData> playerDataList = new List<PlayerData>();

        // Depuración para ver cuántos jugadores hay
        Debug.Log($"🔍 Número de jugadores para procesar: {players.Count}");

        for (int i = 0; i < players.Count; i++)
        {
            GameObject player = players[i];
            
            // Depuración para verificar el objeto player
            Debug.Log($"🔍 Procesando jugador {i + 1}: {(player != null ? "objeto válido" : "NULL")}");
            
            Transform playerDropdownTransform = player.transform.Find("DropdownPersonajes");
            if (playerDropdownTransform == null) 
            {
                Debug.LogError($"❌ No se encontró 'DropdownPersonajes' para el jugador {i + 1}");
                continue;
            }
            
            Transform playerCaptionImageTransform = playerDropdownTransform.Find("CaptionImage");
            if (playerCaptionImageTransform == null) 
            {
                Debug.LogError($"❌ No se encontró 'CaptionImage' para el jugador {i + 1}");
                continue;
            }
            
            Image playerCaptionImage = playerCaptionImageTransform.GetComponent<Image>();
            if (playerCaptionImage == null || playerCaptionImage.sprite == null)
            {
                Debug.LogError($"❌ Image o Sprite nulo para el jugador {i + 1}");
                continue;
            }
            
            string spriteName = playerCaptionImage.sprite.name;
            Debug.Log($"🎨 Sprite seleccionado para el jugador {i + 1}: {spriteName}");
            
            // Guardar el personaje seleccionado en UserStore - Asegurarse de que esto se ejecute
            try {
                UserStore.Instance.AddPlayerToUser(true, i, $"Player_{i + 1}", spriteName);
                Debug.Log($"💾 Jugador {i + 1} con sprite '{spriteName}' guardado en UserStore");
            } 
            catch (System.Exception e) {
                Debug.LogError($"❌ Error al guardar jugador en UserStore: {e.Message}");
            }

            playerForm.AddField($"players[{i}][name]", $"Player_{i + 1}");
            playerForm.AddField($"players[{i}][img]", spriteName);
            
            // Añadir a la lista para JSON
            playerDataList.Add(new PlayerData($"Player_{i + 1}", spriteName));
        }

        // Asegurarse de que se hayan agregado jugadores antes de continuar
        Debug.Log($"✅ Total de jugadores procesados: {playerDataList.Count}");

        UnityWebRequest playerRequest = UnityWebRequest.Post($"{URL}/teams", playerForm); // Usar playerForm en lugar de teamForm

        yield return playerRequest.SendWebRequest();

        if (playerRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Equipo {teamName} y jugadores guardados correctamente.");

            // Cargar la pantalla del partido con el número de jugadores seleccionados
            Debug.Log($"🎮 Cargando pantalla del partido con {players.Count} jugadores.");
            PlayerPrefs.SetInt("TeamPlayerCount", players.Count); // Guardar el número de jugadores en PlayerPrefs
            UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene"); // Cambiar a la escena del partido
        }

        string jsonData = JsonUtility.ToJson(new PlayerList { players = playerDataList });
        WWWForm form = new WWWForm();
        form.AddField("players", jsonData);
        
        UnityWebRequest request = UnityWebRequest.Post($"{URL}/players", form);
        yield return request.SendWebRequest();
    }
}

[System.Serializable]
public class MatchConfig
{
    public int matchDuration;
    public int matchDurationSeconds; // Necesario para GameManager
    public int goalsToWin;
    public int selectedPlayer;
}

[System.Serializable]
public class PlayerData {
    public string name;
    public string img;
    public PlayerData(string name, string img) {
        this.name = name;
        this.img = img;
    }
}

[System.Serializable]
public class PlayerList {
    public List<PlayerData> players;
}
