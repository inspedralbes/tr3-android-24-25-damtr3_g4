using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System;

public class GoalManager : MonoBehaviour
{
    [SerializeField] private int goalsToWin = 3; // Default value, can be overridden
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private string finalSceneName = "FinalGame"; // Name of the scene to load when match ends
    [SerializeField] private float delayBeforeFinalScene = 3f; // Delay in seconds before loading final scene
    [SerializeField] private string apiUrl = "http://localhost:3000"; // API URL, change as needed

    private bool matchEnded = false;
    private int team1Goals = 0;
    private int team2Goals = 0;

    private void Start()
    {
        // Find ScoreManager if not assigned
        if (scoreManager == null)
        {
            scoreManager = UnityEngine.Object.FindFirstObjectByType<ScoreManager>();
            if (scoreManager == null)
            {
                Debug.LogError("❌ No se encontró el ScoreManager en la escena.");
            }
        }

        // Load goals to win from API
        LoadGoalsToWin();

        Debug.Log($"🥅 Juego configurado inicialmente para terminar al llegar a {goalsToWin} goles.");
    }

    // Call this method whenever a goal is scored
    public void UpdateGoals(int team1CurrentGoals, int team2CurrentGoals)
    {
        // Update local goal tracking
        team1Goals = team1CurrentGoals;
        team2Goals = team2CurrentGoals;

        Debug.Log($"🏆 Marcador actual: Equipo 1 - {team1Goals} | Equipo 2 - {team2Goals}");
        Debug.Log($"🎯 Goles necesarios para ganar: {goalsToWin}");

        // Check if any team has exactly reached the winning goal count from API
        if (!matchEnded && (team1Goals == goalsToWin || team2Goals == goalsToWin))
        {
            matchEnded = true;
            int winningTeam = (team1Goals == goalsToWin) ? 1 : 2;
            Debug.Log($"🏆 El equipo {winningTeam} ha alcanzado exactamente {goalsToWin} goles y ha ganado el partido!");
            StartCoroutine(EndMatch(winningTeam));
        }
    }

    private IEnumerator EndMatch(int winningTeam)
    {
        Debug.Log($"🎮 ¡El equipo {winningTeam} ha ganado el partido! Finalizando el juego...");
        
        // Save game data before transitioning to final scene
        SaveMatchResults(winningTeam);
        
        // Wait for the specified delay
        yield return new WaitForSeconds(delayBeforeFinalScene);
        
        // Load the final scene
        LoadFinalScene();
    }

    private void LoadFinalScene()
    {
        Debug.Log("🔄 Cargando escena final...");
        SceneManager.LoadScene(finalSceneName);
    }

    private void SaveMatchResults(int winningTeam)
    {
        // Save match results to be displayed in the final scene
        PlayerPrefs.SetInt("Team1FinalScore", team1Goals);
        PlayerPrefs.SetInt("Team2FinalScore", team2Goals);
        PlayerPrefs.SetInt("WinningTeam", winningTeam);
        PlayerPrefs.Save();
        
        Debug.Log("💾 Resultados del partido guardados correctamente.");
    }

    // Method to be called from ScoreManager when a goal is scored
    public void OnGoalScored(int team1CurrentGoals, int team2CurrentGoals)
    {
        UpdateGoals(team1CurrentGoals, team2CurrentGoals);
    }

    // Method to manually set goals required to win - can be called from settings or other scripts
    public void SetGoalsToWin(int goals)
    {
        if (goals > 0)
        {
            goalsToWin = goals;
            Debug.Log($"🎯 Objetivos actualizados: Ahora se necesitan {goalsToWin} goles para ganar.");
        }
        else
        {
            Debug.LogError("❌ El número de goles para ganar debe ser mayor que cero.");
        }
    }

    // Load goals to win from API
    private void LoadGoalsToWin()
    {
        StartCoroutine(GetMatchConfig());
    }

    // Coroutine to fetch match configuration from API
    private IEnumerator GetMatchConfig()
    {
        Debug.Log("📡 Obteniendo configuración del partido desde la API...");
        
        string matchConfigUrl = $"{apiUrl}/getMatchConfig";
        using (UnityWebRequest request = UnityWebRequest.Get(matchConfigUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"❌ Error al obtener la configuración: {request.error}");
                Debug.LogWarning("⚠️ Usando el valor predeterminado para goalsToWin: " + goalsToWin);
            }
            else
            {
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"📥 Respuesta de API: {jsonResponse}");
                    
                    // Parse JSON response usando la clase MatchConfig existente
                    MatchConfig config = JsonUtility.FromJson<MatchConfig>(jsonResponse);
                    
                    if (config != null && config.goalsToWin > 0)
                    {
                        goalsToWin = config.goalsToWin;
                        Debug.Log($"✅ Configuración cargada: Se necesitan {goalsToWin} goles para ganar.");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ No se pudo extraer goalsToWin de la respuesta de la API. Usando valor predeterminado: " + goalsToWin);
                        
                        // Intentar analizar el JSON manualmente si la deserialización falló
                        if (!string.IsNullOrEmpty(jsonResponse) && jsonResponse.Contains("goalsToWin"))
                        {
                            try
                            {
                                // Extraer goalsToWin directamente del JSON
                                int startIndex = jsonResponse.IndexOf("\"goalsToWin\":") + "\"goalsToWin\":".Length;
                                int endIndex = jsonResponse.IndexOf(',', startIndex);
                                
                                if (endIndex == -1) // Último elemento en el JSON
                                    endIndex = jsonResponse.IndexOf('}', startIndex);
                                
                                string goalsToWinStr = jsonResponse.Substring(startIndex, endIndex - startIndex).Trim();
                                
                                if (int.TryParse(goalsToWinStr, out int parsedGoals) && parsedGoals > 0)
                                {
                                    goalsToWin = parsedGoals;
                                    Debug.Log($"✅ goalsToWin extraído manualmente: {goalsToWin}");
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"❌ Error al analizar manualmente goalsToWin: {e.Message}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ Error al procesar la respuesta: {e.Message}");
                    Debug.LogWarning("⚠️ Usando el valor predeterminado para goalsToWin: " + goalsToWin);
                }
            }
        }
    }
}
