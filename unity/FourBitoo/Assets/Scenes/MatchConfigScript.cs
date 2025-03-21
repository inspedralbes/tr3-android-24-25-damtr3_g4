using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MatchConfigManager : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
    public Transform[] spawnPoints; // Puntos de spawn para los jugadores
    public GameObject canvasPrefab; // Prefab del canvas que contiene el dropdown y la imagen del jugador

    private List<GameObject> players = new List<GameObject>();

    void Start()
    {
        StartCoroutine(LoadMatchConfig());
    }

    IEnumerator LoadMatchConfig()
    {
        UnityWebRequest request = UnityWebRequest.Get($"{URL}/getMatchConfig");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            MatchConfig config = JsonUtility.FromJson<MatchConfig>(request.downloadHandler.text);
            SetupPlayers(config.selectedPlayer);
        }
        else
        {
            Debug.LogError("Error al obtener la configuración del partido: " + request.error);
        }
    }

    void SetupPlayers(int selectedPlayer)
    {
        // Elimina jugadores existentes si los hay
        foreach (GameObject player in players)
        {
            Destroy(player);
        }
        players.Clear();

        // Instancia el número de jugadores basado en selectedPlayer
        for (int i = 0; i < selectedPlayer; i++)
        {
            if (i < spawnPoints.Length)
            {
                // Instancia el canvasPrefab en la posición del spawnPoint
                GameObject canvasInstance = Instantiate(canvasPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
                canvasInstance.transform.SetParent(spawnPoints[i], false);

                players.Add(canvasInstance);
            }
            else
            {
                Debug.LogWarning("No hay suficientes puntos de spawn para el número de jugadores seleccionados.");
            }
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