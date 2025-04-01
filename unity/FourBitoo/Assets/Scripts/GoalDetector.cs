using UnityEngine;
using TMPro; // Asegúrate de tener la referencia a TextMeshPro
using System.Collections; // Para usar corutinas

public class GoalDetector : MonoBehaviour
{
    public int teamID; // 1 = Gol en la portería del equipo 1, 2 = Gol en la portería del equipo 2
    [SerializeField] private ScoreManager scoreManager; // Asigna en el Inspector
    [SerializeField] private TextMeshProUGUI goalText; // Asigna el objeto de texto en el Inspector
    [SerializeField] private float goalTextDuration = 2f; // Duración del texto en pantalla

    private void Start()
    {
        scoreManager = Object.FindFirstObjectByType<ScoreManager>();

        if (scoreManager == null)
        {
            Debug.LogError("❌ ScoreManager no está asignado en GoalDetector.");
        }

        if (goalText != null)
        {
            goalText.gameObject.SetActive(false); // Asegúrate de que el texto esté desactivado al inicio
        }
        else
        {
            Debug.LogError("❌ GoalText no está asignado en GoalDetector.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball")) // Asegúrate de que la pelota tiene el tag "Ball"
        {
            Debug.Log("⚽ ¡Gol detectado en la portería del equipo " + teamID + "!");
            scoreManager.AddGoal(teamID);

            // Mostrar el texto de gol
            if (goalText != null)
            {
                StartCoroutine(ShowGoalText());
            }
        }
    }

    private IEnumerator ShowGoalText()
    {
        goalText.gameObject.SetActive(true); // Activar el texto
        goalText.text = "¡Gool!"; // Cambiar el texto
        goalText.fontSize = 100; // Ajustar el tamaño del texto
        yield return new WaitForSeconds(goalTextDuration); // Esperar
        goalText.gameObject.SetActive(false); // Desactivar el texto
    }
}