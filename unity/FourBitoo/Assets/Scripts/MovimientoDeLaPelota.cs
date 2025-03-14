using UnityEngine;

public class MovimientoDeLaPelota : MonoBehaviour
{
    private Rigidbody2D rb;
    private float desaceleracion = 0.98f; // Factor de desaceleración (más cerca de 1 = desaceleración lenta)
    private float velocidadMinima = 0.3f; // Velocidad mínima antes de detenerse
    private bool enColision = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }

        collider.isTrigger = true; // Asegurar que sea un Trigger
    }

    void Update()
    {
        // Si la pelota no está en colisión, desacelera
        if (!enColision && rb.linearVelocity.magnitude > velocidadMinima)
        {
            rb.linearVelocity *= desaceleracion;
        }
        else if (rb.linearVelocity.magnitude <= velocidadMinima)
        {
            rb.linearVelocity = Vector2.zero; // Detener la pelota completamente
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enColision = true; // Marca que está en contacto con el jugador
            Vector2 direccionDeRebote = (transform.position - other.transform.position).normalized;
            rb.AddForce(direccionDeRebote * 5f, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enColision = false; // Cuando el Player sale, la pelota empieza a desacelerar
        }
    }
}
