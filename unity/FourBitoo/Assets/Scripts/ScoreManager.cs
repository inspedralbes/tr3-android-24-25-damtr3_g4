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
        GameObject team1TextObject = GameObject.FindWithTag("Team1ScoreText");
        if (team1TextObject != null)
        {
            team1ScoreText = team1TextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("❌ No se encontró el objeto con la etiqueta 'Team1ScoreText'.");
        }
    }

    if (team2ScoreText == null)
    {
        GameObject team2TextObject = GameObject.FindWithTag("Team2ScoreText");
        if (team2TextObject != null)
        {
            team2ScoreText = team2TextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("❌ No se encontró el objeto con la etiqueta 'Team2ScoreText'.");
        }
    }
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
