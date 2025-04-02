using UnityEngine;

public class CollisionTester : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.LogWarning($"[CollisionTester] Colisión detectada con: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.LogWarning($"[CollisionTester] Colisión normal (no trigger) con: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
    }

    // Para poner en la pelota
    void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning($"[CollisionTester-3D] Colisión 3D con: {collision.gameObject.name}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning($"[CollisionTester-3D] Trigger 3D con: {other.gameObject.name}");
    }

    // Usar esto para depurar
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.LogWarning("[CollisionTester] Test Log - Si ves esto, los logs funcionan correctamente");
            
            // Intentar obtener los objetos por tag
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            Debug.LogWarning($"[CollisionTester] Jugadores encontrados: {players.Length}");
            
            GameObject ball = GameObject.FindGameObjectWithTag("Ball");
            if (ball != null)
            {
                Debug.LogWarning($"[CollisionTester] Balón encontrado: {ball.name}");
                
                // Verificar componentes del balón
                Collider2D ballCollider = ball.GetComponent<Collider2D>();
                if (ballCollider != null)
                {
                    Debug.LogWarning($"[CollisionTester] Balón tiene Collider2D: {ballCollider.GetType().Name}, isTrigger: {ballCollider.isTrigger}");
                }
                else
                {
                    Debug.LogWarning("[CollisionTester] ¡El balón NO tiene Collider2D!");
                    
                    // Verificar si tiene Collider 3D
                    Collider ballCollider3D = ball.GetComponent<Collider>();
                    if (ballCollider3D != null)
                    {
                        Debug.LogWarning($"[CollisionTester] Balón tiene Collider 3D: {ballCollider3D.GetType().Name}, isTrigger: {ballCollider3D.isTrigger}");
                    }
                }
                
                // Verificar Rigidbody
                Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
                if (ballRb != null)
                {
                    Debug.LogWarning("[CollisionTester] Balón tiene Rigidbody2D");
                }
                else
                {
                    Debug.LogWarning("[CollisionTester] ¡El balón NO tiene Rigidbody2D!");
                    
                    // Verificar si tiene Rigidbody 3D
                    Rigidbody ballRb3D = ball.GetComponent<Rigidbody>();
                    if (ballRb3D != null)
                    {
                        Debug.LogWarning("[CollisionTester] Balón tiene Rigidbody 3D");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CollisionTester] No se encontró ningún objeto con tag 'Ball'");
            }
            
            // Verificar porterías
            var goalDetectors = FindObjectsOfType<GoalDetector>();
            Debug.LogWarning($"[CollisionTester] GoalDetectors encontrados: {goalDetectors.Length}");
            
            foreach (var detector in goalDetectors)
            {
                Debug.LogWarning($"[CollisionTester] GoalDetector en: {detector.gameObject.name}, TeamID: {detector.teamID}");
                
                // Verificar collider
                Collider2D goalCollider = detector.GetComponent<Collider2D>();
                if (goalCollider != null)
                {
                    Debug.LogWarning($"[CollisionTester] Portería tiene Collider2D: {goalCollider.GetType().Name}, isTrigger: {goalCollider.isTrigger}");
                }
                else
                {
                    Debug.LogWarning("[CollisionTester] ¡La portería NO tiene Collider2D!");
                    
                    // Verificar si tiene Collider 3D
                    Collider goalCollider3D = detector.GetComponent<Collider>();
                    if (goalCollider3D != null)
                    {
                        Debug.LogWarning($"[CollisionTester] Portería tiene Collider 3D: {goalCollider3D.GetType().Name}, isTrigger: {goalCollider3D.isTrigger}");
                    }
                }
            }
        }
    }
}
