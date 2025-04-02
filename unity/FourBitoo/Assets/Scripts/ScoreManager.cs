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
    
    [SerializeField] private GoalManager goalManager; 

    private int team1Score = 0;
    private int team2Score = 0;
    private Vector3 initialBallPosition;
    private List<Vector3> initialPlayerPositions = new List<Vector3>();
    
    // Referencia al GameManager para sincronizar el sistema de turnos
    private GameManager gameManager;

    private void Awake()
    {
        // Asegurar que solo existe una instancia
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("ScoreManager inicializado - Listo para detectar goles");
    }

    public void AddGoal(int teamID)
    {
        Debug.Log($"u26bd SCOREMANAGER: Gol registrado en la porteru00eda del equipo {teamID}!");

        if(teamID == 1)
        {
            team2Score++;
        }
        else if(teamID == 2)
        {
            team1Score++;
        }
        Debug.Log("ud83dudce2 Marcador actualizado: Equipo 1 - " + team1Score + " | Equipo 2 - " + team2Score);
        UpdateScoreUI();
        
        // Notify the GoalManager about the updated scores
        if (goalManager != null) {
            goalManager.OnGoalScored(team1Score, team2Score);
        }
        
        // Notificar al GameManager que se ha marcado un gol
        // El teamID en GoalDetector es inverso al concepto de "equipo que marca"
        // teamID = 1 significa porteru00eda del equipo 1, por lo que el equipo 2 marco
        int scoringTeamID = (teamID == 1) ? 2 : 1;
        
        if (gameManager != null)
        {
            Debug.Log($"u26bd Notificando a GameManager que el equipo {scoringTeamID} ha marcado gol");
            // Le indicamos al GameManager quu00e9 equipo marcu00f3
            gameManager.GoalScored(scoringTeamID);
            
            // Ya no es necesario continuar con la corrutina, el GameManager se encargaru00e1 de todo
            return;
        }
        else
        {
            Debug.LogWarning("u274c GameManager no encontrado, utilizando la lu00f3gica antigua de reinicio");
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log($"u26bd GameManager encontrado en el u00faltimo intento. Notificando gol del equipo {scoringTeamID}");
                gameManager.GoalScored(scoringTeamID);
                return;
            }
            StartCoroutine(ResetBall(teamID));
        }
    }

    private System.Collections.IEnumerator ResetBall( int lastScoringTeamID)
    {
        Debug.Log("Iniciando reset de posiciones (mu00e9todo antiguo)");
        yield return new WaitForSeconds(2f);

        ball.transform.position = initialBallPosition;

        Rigidbody2D ballRigidbody = ball.GetComponent<Rigidbody2D>();
        if(ballRigidbody != null){
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }

        // Deseleccionar todos los jugadores primero
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                ControlPorRaton playerControl = player.GetComponent<ControlPorRaton>();
                if (playerControl != null)
                {
                    playerControl.Deseleccionar();
                }
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            if(players[i] != null){
                players[i].transform.position = initialPlayerPositions[i];
            
                Rigidbody2D playerRigidbody = players[i].GetComponent<Rigidbody2D>();
                if (playerRigidbody != null)
                {
                    playerRigidbody.linearVelocity = Vector2.zero;
                    playerRigidbody.angularVelocity = 0f;
                }

                ControlPorRaton playerControl = players[i].GetComponent<ControlPorRaton>();
                if(playerControl != null){
                    playerControl.activo = false;
                    playerControl.GetComponent<SpriteRenderer>().enabled = true;
                    
                    // Seleccionar jugador del equipo opuesto al que marcu00f3 gol
                    int equipoJugador = playerControl.GetTeamID();
                    if (equipoJugador != 0 && equipoJugador != lastScoringTeamID)
                    {
                        playerControl.Seleccionar();
                        break; // Solo seleccionar un jugador
                    }
                }
            }
        }

        Debug.Log("u26bd El equipo contrario saca despu00e9s del gol.");
    }

    // Update is called once per frame
    private void UpdateScoreUI(){
        Debug.Log("ud83dudda5ufe0f UI actualizada: " + team1Score + " - " + team2Score);
        if (team1ScoreText != null)
        {
            team1ScoreText.text = team1Score.ToString();
        }
        else
        {
            Debug.LogError("team1ScoreText es null!");
        }
        
        if (team2ScoreText != null)
        {
            team2ScoreText.text = team2Score.ToString();
        }
        else
        {
            Debug.LogError("team2ScoreText es null!");
        }
        
        // En lugar de intentar modificar las propiedades directamente, solo notificamos al GameManager
        // cuando se marca un gol (ya lo estamos haciendo en AddGoal)
    }

    void Start()
    {
        Debug.Log("u2705 ScoreManager Start");
        if (ball == null)
        {
            Debug.Log("Buscando el balu00f3n...");
            ball = GameObject.FindGameObjectWithTag("Ball");
            if (ball == null)
            {
                Debug.LogError("u274c No se encontru00f3 la pelota con el tag 'Ball'");
            }
            else
            {
                Debug.LogError("❌ Uno de los jugadores no está asignado en la lista.");
            }
        }

        // Find GoalManager if not assigned
        if (goalManager == null) {
            goalManager = UnityEngine.Object.FindFirstObjectByType<GoalManager>();
            if (goalManager == null) {
                Debug.LogWarning("⚠️ No se encontró el GoalManager en la escena. El juego no terminará automáticamente.");
            }
        }

        // Código existente para inicializar los textos...
        if (team1ScoreText == null)
        {
            GameObject team1TextObject = GameObject.FindWithTag("Team1ScoreText");
            if (team1TextObject != null)
            {
                Debug.LogError("u274c El balu00f3n no tiene un Collider2D! Agrega un Circle Collider 2D al balu00f3n.");
            }
            else
            {
                Debug.Log("u2705 Balu00f3n tiene Collider2D configurado correctamente");
            }
            
            // Verificar si el balu00f3n tiene el tag correcto
            if (!ball.CompareTag("Ball"))
            {
                Debug.LogError($"u274c El balu00f3n tiene el tag incorrecto: '{ball.tag}'. Deberu00eda ser 'Ball'");
            }
        }
        
        // Buscar el GameManager para sincronizar sistemas
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            Debug.Log("u2713 GameManager encontrado, sistema de turnos sincronizado");
            // No intentamos modificar las propiedades directamente
        }
        else
        {
            Debug.LogWarning("u274c GameManager no encontrado, el sistema de turnos no estaru00e1 sincronizado");
        }

        if (players == null || players.Count == 0)
        {
            Debug.Log("Buscando jugadores...");
            players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
            if (players.Count == 0)
            {
                Debug.LogError("u274c No se encontraron jugadores con el tag 'Player'");
            }
            else
            {
                Debug.Log($"Se encontraron {players.Count} jugadores con el tag 'Player'");
            }
        }

        initialPlayerPositions.Clear();
        foreach (GameObject player in players)
        {
            if (player != null)
            {
                initialPlayerPositions.Add(player.transform.position);
                Debug.Log($"ud83dude64 Guardando posici00f3n inicial de jugador: {player.name} en {player.transform.position}");
            }
        }
    }

    // Add getters for the current scores that can be accessed by other scripts
    public int GetTeam1Score() {
        return team1Score;
    }

    public int GetTeam2Score() {
        return team2Score;
    }

    // Add getters for the current scores that can be accessed by other scripts
    public int GetTeam1Score() {
        return team1Score;
    }

    public int GetTeam2Score() {
        return team2Score;
    }
}
