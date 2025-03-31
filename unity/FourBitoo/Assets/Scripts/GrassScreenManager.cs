using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class GrassScreenManager : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
    public Transform[] spawnPoints; // Puntos de spawn para los jugadores
    public GameObject canvasPrefab; // Prefab del canvas que contiene el CaptionImage
    public Sprite[] playerSprites; // Array de sprites asignados desde el Inspector

    private List<GameObject> players = new List<GameObject>();

    void Start()
    {
        StartCoroutine(LoadMatchConfig());
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

            MatchConfiguration config = JsonUtility.FromJson<MatchConfiguration>(jsonResponse);
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

            // Instanciar el prefab del jugador en la posición calculada
            GameObject playerInstance = Instantiate(canvasPrefab, newPosition, Quaternion.identity);
            playerInstance.transform.SetParent(transform, false);

            // Configurar el CaptionImage dentro del prefab
            Transform captionImageTransform = playerInstance.transform.Find("Canvas/CaptionImage");
            if (captionImageTransform != null)
            {
                Image captionImage = captionImageTransform.GetComponent<Image>();
                if (captionImage != null)
                {
                    // Asignar un sprite dinámico al CaptionImage
                    Sprite playerSprite = GetPlayerSprite(i);
                    if (playerSprite != null)
                    {
                        captionImage.sprite = playerSprite;
                        Debug.Log($"🎨 Sprite asignado al jugador {i + 1}: {playerSprite.name}");
                    }
                    else
                    {
                        Debug.LogError($"❌ No se encontró un sprite para el jugador {i + 1}.");
                    }
                }
                else
                {
                    Debug.LogError($"❌ El CaptionImage del jugador {i + 1} no tiene un componente Image.");
                }
            }
            else
            {
                Debug.LogError($"❌ No se encontró el CaptionImage en el prefab del jugador {i + 1}.");
            }

            // Agregar el jugador a la lista
            players.Add(playerInstance);
        }
    }

    Sprite GetPlayerSprite(int playerIndex)
    {
        if (playerSprites == null || playerSprites.Length == 0)
        {
            Debug.LogError("❌ Los sprites no se cargaron correctamente. Asegúrate de que la carpeta Resources/Player contenga los sprites.");
            return null;
        }

        if (playerIndex < 0 || playerIndex >= playerSprites.Length)
        {
            Debug.LogError($"❌ Índice de jugador fuera de rango: {playerIndex}. Asegúrate de que los sprites estén correctamente configurados.");
            return null;
        }

        // Ajustar el nombre del sprite si es necesario
        string expectedName = $"Wii - Inazuma Eleven Strikers 2012 Xtreme - Save File Icons_{playerIndex}";
        Sprite sprite = playerSprites[playerIndex];

        if (sprite.name != expectedName)
        {
            Debug.LogWarning($"⚠️ El nombre del sprite no coincide con el esperado. Sprite actual: {sprite.name}, esperado: {expectedName}");
        }

        return sprite;
    }
}

[System.Serializable]
public class MatchConfiguration
{
    public int matchDuration;
    public int goalsToWin;
    public int selectedPlayer;
}