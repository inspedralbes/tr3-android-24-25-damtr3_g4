using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class MatchConfigManager : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
    public Transform[] spawnPoints; // Puntos de spawn para los jugadores
    public GameObject canvasPrefab; // Prefab del canvas que contiene el dropdown y la imagen del jugador

    private List<GameObject> players = new List<GameObject>();

    void Start()
    {
        CheckSpawnPoints();
        StartCoroutine(LoadMatchConfig());
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
}

[System.Serializable]
public class MatchConfig
{
    public int matchDuration;
    public int goalsToWin;
    public int selectedPlayer;
}