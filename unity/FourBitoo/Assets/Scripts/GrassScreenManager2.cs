using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class GrassScreenManager2 : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
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

            SetupPlayers(config.selectedPlayer); // Configura los jugadores
        }
        else
        {
            Debug.LogError("❌ Error al obtener la configuración del partido: " + request.error);
        }
    }

    void SetupPlayers(int selectedPlayer)
    {
        Debug.Log($"♻️ Configurando jugadores... Número de jugadores seleccionados: {selectedPlayer}");

        // Elimina jugadores existentes
        foreach (GameObject player in players)
        {
            Destroy(player);
        }
        players.Clear();

        // Definir las posiciones específicas para los jugadores
        Vector3[] player1Positions = new Vector3[]
        {
            new Vector3(144, -60, 0),
            new Vector3(71, -10, 0),
            new Vector3(71, -87, 0),
            new Vector3(-5, -90, 0),
            new Vector3(-5, -20, 0)
        };

        for (int i = 0; i < selectedPlayer; i++)
        {
            // Verificar si hay suficientes posiciones predefinidas
            if (i >= player1Positions.Length)
            {
                Debug.LogError($"❌ No hay suficientes posiciones predefinidas para el jugador {i + 1}.");
                break;
            }

            // Obtener la posición predefinida
            Vector3 newPosition = player1Positions[i];

            // Instanciar el prefab del jugador en la posición predefinida
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

        return playerSprites[playerIndex];
    }
}