using UnityEngine;
using UnityEngine.UI; // Necesario para trabajar con imágenes
using System.Collections; // Para usar corutinas

public class GoalDetector : MonoBehaviour
{
    public int teamID; // 1 = Gol en la portería del equipo 1, 2 = Gol en la portería del equipo 2
    [SerializeField] private ScoreManager scoreManager; // Asigna en el Inspector
        [SerializeField] private Image golImage; // Asigna la imagen en el Inspector
    [SerializeField] private float goalImageDuration = 2f; // Duración de la imagen en pantalla

    [Header("Configuración alternativa de detección")]
    [Tooltip("Si el método normal de colisiones no funciona, esto comprobará la distancia")]
    public bool usarDeteccionPorDistancia = true;
    public float distanciaDeteccion = 1.5f;
    private GameObject ball;
    private bool goalDetected = false;
    private float cooldownTimer = 0f;

    private void Start()
    {
        // FindObjectOfType es más compatible con versiones anteriores de Unity
        if (scoreManager == null)
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }

        if (scoreManager == null)
        {
            Debug.LogError(" ScoreManager no está asignado en GoalDetector. El sistema de goles no funcionará!");
        }
        else
        {
            Debug.LogError(" ScoreManager no está asignado en GoalDetector. El sistema de goles no funcionará!");
        }
        else
        {
            Debug.Log($" GoalDetector inicializado correctamente. Detectando goles para el equipo {teamID}");
        }
        
        // Encontrar el balón
        ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball == null)
        {
            Debug.LogError(" No se encontró el balón con el tag 'Ball'");
        }
        
        // Verificar que el objeto tiene un Collider2D configurado como trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError(" GoalDetector no tiene un Collider2D! Agrega un Box Collider 2D al objeto.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError(" El Collider2D de GoalDetector no está configurado como trigger! Activa 'Is Trigger' en el inspector.");
        }
    }
    
    private void Update()
    {
        // Si estamos usando la detección por distancia y tenemos el balón
        if (usarDeteccionPorDistancia && ball != null && !goalDetected && cooldownTimer <= 0)
        {
            // Obtener el centro de la portería
            Vector3 goalCenter = transform.position;
            
            // Calcular la distancia entre el balón y la portería
            float distance = Vector3.Distance(ball.transform.position, goalCenter);
            
            // Si el balón está dentro de la distancia de detección
            if (distance < distanciaDeteccion)
            {
                Debug.LogWarning($"GOL DETECTADO POR DISTANCIA! El balón está a {distance} de la portería del equipo {teamID}");
                goalDetected = true;
                cooldownTimer = 3f; // 3 segundos de cooldown
                
                // Registrar el gol
                if (scoreManager != null)
                {
                    scoreManager.AddGoal(teamID);
                }
                else
                {
                    Debug.LogError(" No se pudo registrar el gol porque ScoreManager es null");
                    
                    // Intento de recuperación de último recurso
                    scoreManager = FindObjectOfType<ScoreManager>();
                    if (scoreManager != null)
                    {
                        Debug.Log(" ScoreManager encontrado! Registrando gol...");
                        scoreManager.AddGoal(teamID);
                    }
                }
            }
        }
        
        // Actualizar el temporizador de enfriamiento
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                goalDetected = false;
            }
        }

        if (golImage != null)
        {
            golImage.gameObject.SetActive(false); // Asegúrate de que la imagen esté desactivada al inicio
        }
        else
        {
            Debug.LogError("❌ GolImage no está asignada en GoalDetector.");
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
        Debug.Log($"Colisión detectada con: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        
        if (collision.CompareTag("Ball") && !goalDetected) // Asegúrate de que la pelota tiene el tag "Ball"
        {
            Debug.Log(" ¡Gol detectado en la portería del equipo " + teamID + "!");
            goalDetected = true;
            cooldownTimer = 3f; // 3 segundos de cooldown
            
            if (scoreManager != null)
            {
                scoreManager.AddGoal(teamID);
            }
            else
            {
                Debug.LogError(" No se pudo registrar el gol porque ScoreManager es null");
                
                // Intento de recuperación de último recurso
                scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager != null)
                {
                    Debug.Log(" ScoreManager encontrado! Registrando gol...");
                    scoreManager.AddGoal(teamID);
                }
            }
        }
    }
    
    // Para fines de depuración, dibujar el área del trigger en el editor
    private void OnDrawGizmos()
    {
        // Dibujar el collider
        Gizmos.color = Color.red;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col is BoxCollider2D)
        {
            BoxCollider2D boxCol = col as BoxCollider2D;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCol.offset, boxCol.size);
        }
        
        // Dibujar la esfera de detección si está activada
        if (usarDeteccionPorDistancia)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Naranja semi-transparente
            Gizmos.DrawSphere(transform.position, distanciaDeteccion);
        }
    }

    private IEnumerator ShowGoalImage()
    {
        golImage.gameObject.SetActive(true); // Activar la imagen
        yield return new WaitForSeconds(goalImageDuration); // Esperar
        golImage.gameObject.SetActive(false); // Desactivar la imagen
    }
}