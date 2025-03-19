using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI team1ScoreText;
    public TextMeshProUGUI team2ScoreText;

    public GameObject ball;

    public List<GameObject> players;

    private int team1Score = 0;
    private int team2Score = 0;
    private Vector3 initialBallPosition;
    private List<Vector3> initialPlayerPositions = new List<Vector3>();

    public void AddGoal(int teamID){

        if(teamID == 1){

            team2Score++;
        }
        else if(teamID == 2){

            team1Score++;
        }
        Debug.Log("📢 Marcador actualizado: Equipo 1 - " + team1Score + " | Equipo 2 - " + team2Score);
        UpdateScoreUI();

        StartCoroutine(ResetBall(teamID));
    }

    private System.Collections.IEnumerator ResetBall( int lastScoringTeamID){

        yield return new WaitForSeconds(2f);

        ball.transform.position = initialBallPosition;

        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        if(ballRigidbody != null){
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }

        for (int i = 0; i < players.Count; i++)
        {
            if(players[i] != null){
                players[i].transform.position = initialPlayerPositions[i];
            
                Rigidbody playerRigidbody = players[i].GetComponent<Rigidbody>();
                if (playerRigidbody != null)
                {
                    playerRigidbody.linearVelocity = Vector3.zero;
                    playerRigidbody.angularVelocity = Vector3.zero;
                }

                ControlPorRaton playerControl = players[i].GetComponent<ControlPorRaton>();
                if(playerControl != null){
                    playerControl.activo = false;
                    playerControl.GetComponent<SpriteRenderer>().enabled = true;
                }
            }
        }

        Debug.Log("⚽ El equipo contrario saca después del gol.");
    }

    // Update is called once per frame
    private void UpdateScoreUI(){
        Debug.Log("🖥️ UI actualizada: " + team1Score + " - " + team2Score);
        team1ScoreText.text = team1Score.ToString();
        team2ScoreText.text = team2Score.ToString();
    }

void Start()
{
    if (ball == null)
    {
        Debug.LogError("❌ La variable 'ball' no está asignada en el Inspector. Por favor, asigna la pelota.");
        return;
    }

    initialBallPosition = ball.transform.position;

    // Guarda las posiciones iniciales de los jugadores
    initialPlayerPositions.Clear(); // Asegúrate de limpiar la lista antes de llenarla
    foreach (GameObject player in players)
    {
        if (player != null)
        {
            initialPlayerPositions.Add(player.transform.position);
        }
        else
        {
            Debug.LogError("❌ Uno de los jugadores no está asignado en la lista.");
        }
    }

    // Código existente para inicializar los textos...
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
}
