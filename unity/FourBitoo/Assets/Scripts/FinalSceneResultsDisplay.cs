using UnityEngine;
using TMPro;

public class FinalSceneResultsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI team1ScoreText;
    [SerializeField] private TextMeshProUGUI team2ScoreText;
    [SerializeField] private TextMeshProUGUI winnerText;

    private void Start()
    {
        // Recuperar los resultados guardados en PlayerPrefs
        int team1FinalScore = PlayerPrefs.GetInt("Team1FinalScore", 0);
        int team2FinalScore = PlayerPrefs.GetInt("Team2FinalScore", 0);
        int winningTeam = PlayerPrefs.GetInt("WinningTeam", 0);

        // Mostrar las puntuaciones
        if (team1ScoreText != null)
        {
            team1ScoreText.text = team1FinalScore.ToString();
        }
        
        if (team2ScoreText != null)
        {
            team2ScoreText.text = team2FinalScore.ToString();
        }

        // Mostrar mensaje del ganador
        if (winnerText != null)
        {
            if (winningTeam == 1)
            {
                winnerText.text = "¡El Equipo 1 ha ganado!";
            }
            else if (winningTeam == 2)
            {
                winnerText.text = "¡El Equipo 2 ha ganado!";
            }
            else
            {
                winnerText.text = "Resultado desconocido";
            }
        }

        Debug.Log($"Resultados cargados: Equipo 1 - {team1FinalScore}, Equipo 2 - {team2FinalScore}, Ganador - Equipo {winningTeam}");
    }
}
