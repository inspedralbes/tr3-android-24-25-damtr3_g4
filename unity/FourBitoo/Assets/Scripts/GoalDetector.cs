using UnityEngine;
using UnityEngine.UI; // Necesario para trabajar con imágenes
using System.Collections; // Para usar corutinas

public class GoalDetector : MonoBehaviour
{
    public int teamID; // 1 = Gol en la portería del equipo 1, 2 = Gol en la portería del equipo 2
    [SerializeField] private ScoreManager scoreManager; // Asigna en el Inspector
    [SerializeField] private Image golImage; // Asigna la imagen en el Inspector
    [SerializeField] private float goalImageDuration = 2f; // Duración de la imagen en pantalla

    private void Start()
    {
        scoreManager = Object.FindFirstObjectByType<ScoreManager>();

        if (scoreManager == null)
        {
            Debug.LogError("❌ ScoreManager no está asignado en GoalDetector.");
        }

        if (golImage != null)
        {
            golImage.gameObject.SetActive(false); // Asegúrate de que la imagen esté desactivada al inicio
        }
        else
        {
            Debug.LogError("❌ GolImage no está asignada en GoalDetector.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball")) // Asegúrate de que la pelota tiene el tag "Ball"
        {
            Debug.Log("⚽ ¡Gol detectado en la portería del equipo " + teamID + "!");
            scoreManager.AddGoal(teamID);

            // Mostrar la imagen de gol
            if (golImage != null)
            {
                StartCoroutine(ShowGoalImage());
            }
        }
    }

    private IEnumerator ShowGoalImage()
    {
        golImage.gameObject.SetActive(true); // Activar la imagen
        yield return new WaitForSeconds(goalImageDuration); // Esperar
        golImage.gameObject.SetActive(false); // Desactivar la imagen
    }
}