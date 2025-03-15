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
        else if (other.CompareTag("ColisionInvisible"))
        {
            enColision = true; // Marca que está en contacto con la colisión invisible
            Vector2 direccionDeRebote = (transform.position - other.transform.position).normalized;
            rb.linearVelocity = direccionDeRebote * rb.linearVelocity.magnitude; // Rebote en la dirección opuesta
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("ColisionInvisible"))
        {
            // Mantener la pelota rebotando mientras esté en contacto con "ColisionInvisible"
            Vector2 direccionDeRebote = (transform.position - other.transform.position).normalized;
            rb.linearVelocity = direccionDeRebote * rb.linearVelocity.magnitude; // Rebote en la dirección opuesta
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("ColisionInvisible"))
        {
            enColision = false; // Cuando el Player o la colisión invisible sale, la pelota empieza a desacelerar
        }
    }
}