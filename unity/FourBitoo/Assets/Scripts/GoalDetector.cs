using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public int teamID; // 1 = Gol en la portería del equipo 1, 2 = Gol en la portería del equipo 2
    [SerializeField] private ScoreManager scoreManager; // Asigna en el Inspector

    private void Start()
    {
        scoreManager = Object.FindFirstObjectByType<ScoreManager>();

        if (scoreManager == null)
    {
        Debug.LogError("❌ ScoreManager no está asignado en GoalDetector.");
    }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball")) // Asegúrate de que la pelota tiene el tag "Ball"
        {
            Debug.Log("⚽ ¡Gol detectado en la portería del equipo " + teamID + "!");
            scoreManager.AddGoal(teamID);
        }
    }
}
