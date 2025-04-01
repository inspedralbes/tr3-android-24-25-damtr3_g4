using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class MatchConfigManager : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
    public Transform[] spawnPoints; // Puntos de spawn para los jugadores
    public GameObject canvasPrefab; // Prefab del canvas que contiene el dropdown y la imagen del jugador
    public Button confirmButton; // Botón de confirmar

    private List<GameObject> players = new List<GameObject>();
    private string selectedBadgeName; // Store the selected badge name

    void Start()
    {
        CheckSpawnPoints();
        StartCoroutine(LoadMatchConfig());
        LoadAvailableBadges(); // Load badges dynamically

        // Eliminar la llamada inicial a SaveSelectedPlayers
        if (confirmButton != null)
        {
            // Configurar el evento onClick para guardar los jugadores al presionar el botón
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

    void CheckSpawnPoints()
    {
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

            MatchConfig config = JsonUtility.FromJson<MatchConfig>(jsonResponse);
            Debug.Log($"🎮 Número de jugadores recibidos: {config.selectedPlayer}");

            SetupPlayers(config.selectedPlayer);
        }
        else
        {
            Debug.LogError("❌ Error al obtener la configuración del partido: " + request.error);
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
        
        GameObject badgeDropdownObject = GameObject.Find("BadgeDropdown");
        if (badgeDropdownObject == null) yield break;
        
        Transform badgeCaptionImageTransform = badgeDropdownObject.transform.Find("CaptionImage");
        if (badgeCaptionImageTransform == null) yield break;
        
        Image badgeCaptionImage = badgeCaptionImageTransform.GetComponent<Image>();
        
        selectedBadgeName = badgeCaptionImage.sprite.name;
        Sprite badgeSprite = Resources.Load<Sprite>($"Emblems/{selectedBadgeName}");
        
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

        WWWForm form = new WWWForm();
        List<PlayerData> playerDataList = new List<PlayerData>();

        for (int i = 0; i < players.Count; i++)
        {
            GameObject player = players[i];
            
            Transform playerDropdownTransform = player.transform.Find("DropdownPersonajes");
            if (playerDropdownTransform == null) continue;
            
            Transform playerCaptionImageTransform = playerDropdownTransform.Find("CaptionImage");
            if (playerCaptionImageTransform == null) continue;
            
            Image playerCaptionImage = playerCaptionImageTransform.GetComponent<Image>();
            string spriteName = playerCaptionImage.sprite.name;
            Debug.Log($"🎨 Sprite seleccionado para el jugador {i + 1}: {spriteName}");

            teamForm.AddField($"players[{i}][name]", $"Player_{i + 1}");
            teamForm.AddField($"players[{i}][img]", spriteName);
        }

        UnityWebRequest teamRequest = UnityWebRequest.Post($"{URL}/teams", teamForm);

        yield return teamRequest.SendWebRequest();

        if (teamRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Equipo {teamName} y jugadores guardados correctamente.");

            // Cargar la pantalla del partido con el número de jugadores seleccionados
            Debug.Log($"🎮 Cargando pantalla del partido con {players.Count} jugadores.");
            PlayerPrefs.SetInt("TeamPlayerCount", players.Count); // Guardar el número de jugadores en PlayerPrefs
            UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene"); // Cambiar a la escena del partido
        }

        string jsonData = JsonUtility.ToJson(new PlayerList { players = playerDataList });
        form.AddField("players", jsonData);
        
        UnityWebRequest request = UnityWebRequest.Post($"{URL}/players", form);
        yield return request.SendWebRequest();
    }
}

[System.Serializable]
public class MatchConfig
{
    public int matchDuration;
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
