using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI team1ScoreText;
    public TextMeshProUGUI team2ScoreText;

    private int team1Score = 0;
    private int team2Score = 0;

    public void AddGoal(int teamID){

        if(teamID == 1){

            team2Score++;
        }
        else if(teamID == 2){

            team1Score++;
        }
        Debug.Log("📢 Marcador actualizado: Equipo 1 - " + team1Score + " | Equipo 2 - " + team2Score);
        UpdateScoreUI();
    }

    private void UpdateScoreUI(){
        Debug.Log("🖥️ UI actualizada: " + team1Score + " - " + team2Score);
        team1ScoreText.text = team1Score.ToString();
        team2ScoreText.text = team2Score.ToString();
    }
    void Start()
{
    if (team1ScoreText == null)
    {
        team1ScoreText = GameObject.Find("Team1ScoreText").GetComponent<TextMeshProUGUI>();
        if (team1ScoreText == null)
        {
            Debug.LogError("❌ No se encontró el objeto de texto para el Equipo 1.");
        }
    }

    if (team2ScoreText == null)
    {
        team2ScoreText = GameObject.Find("Team2ScoreText").GetComponent<TextMeshProUGUI>();
        if (team2ScoreText == null)
        {
            Debug.LogError("❌ No se encontró el objeto de texto para el Equipo 2.");
        }
    }
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
