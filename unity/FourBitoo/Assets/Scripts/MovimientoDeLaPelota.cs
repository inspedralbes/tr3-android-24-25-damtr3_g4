using UnityEngine;

public class MovimientoDeLaPelota : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float desaceleracion = 0.99f; // Mantener este valor cerca de lo que tienes
    [SerializeField] private float velocidadMinima = 0.5f; // Igual a lo que tienes configurado
    [SerializeField] private float factorDeImpulso = 1500f; // AUMENTADO DRÁSTICAMENTE
    [SerializeField] private float velocidadMaxima = 200f; // AUMENTADO DRÁSTICAMENTE
    [SerializeField] private float masaPelota = 0.5f; // Mantener igual a lo que tienes configurado

    // Nuevas variables para efectos especiales
    [SerializeField] private float multiplicadorImpactoBase = 10f; // Nuevo parámetro para control extremo
    [SerializeField] private float efectoExplosivo = 1.8f; // Efecto explosivo al golpear
    private Animator animator; // Para efectos visuales
    private float tiempoUltimaColision = 0f;
    private float tiempoEntreColisiones = 0.01f; // Tiempo aún más corto

    // Nueva variable para controlar la reducción del giro
    [SerializeField] private float factorReduccionGiro = 0.98f; // Ajusta este valor para controlar la velocidad de parada del giro

    void Start()
    {
        // Configurar el Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Configuración física extrema
        rb.gravityScale = 0;
        rb.mass = masaPelota;
        rb.linearDamping = 0.005f; // Prácticamente sin resistencia
        rb.angularDamping = 0.005f; // Menor resistencia a la rotación
        rb.constraints = RigidbodyConstraints2D.None; // Permitir rotación para efectos más dinámicos
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Configurar el collider físico
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = false;

        // Material de física ultra-rebotante
        PhysicsMaterial2D materialPelota = new PhysicsMaterial2D("MaterialPelota");
        materialPelota.bounciness = 0.7f; // Máximo rebote
        materialPelota.friction = 0.01f; // Mínima fricción
        collider.sharedMaterial = materialPelota;
    }

    void FixedUpdate()
    {
        float velocidadActual = rb.linearVelocity.magnitude;

        if (velocidadActual > velocidadMinima)
        {
            rb.linearVelocity *= desaceleracion;
            // Reducir gradualmente la velocidad angular (giro)
            rb.angularVelocity *= factorReduccionGiro;
            if (animator != null)
            {
                animator.enabled = true; // La animación sigue mientras hay movimiento
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f; // Detener rotación cuando se detiene la pelota
            if (animator != null)
            {
                animator.enabled = false; // Detener animación cuando se detiene la pelota
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time - tiempoUltimaColision < tiempoEntreColisiones)
        {
            return;
        }

        tiempoUltimaColision = Time.time;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Resetear velocidad antes del nuevo impulso
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Obtener información de la colisión
            ContactPoint2D punto = collision.GetContact(0);

            // Obtener el Rigidbody2D del jugador
            Rigidbody2D rbJugador = collision.gameObject.GetComponent<Rigidbody2D>();

            if (rbJugador != null)
            {
                // Impulso EXTREMADAMENTE POTENTE
                float velocidadJugador = rbJugador.linearVelocity.magnitude;

                // Multiplicador de fuerza básico fijo muy alto
                float multiplicadorFuerza = multiplicadorImpactoBase + (velocidadJugador * 2f);

                // Dirección del impulso: desde el jugador hacia la pelota
                Vector2 direccionImpulso = (rb.position - rbJugador.position).normalized;

                // Aplicar fuerza EXTREMA
                float fuerzaBase = factorDeImpulso * multiplicadorFuerza;

                // IMPULSO PRINCIPAL - Extremadamente potente
                rb.AddForce(direccionImpulso * fuerzaBase, ForceMode2D.Impulse);

                // Aplicar torque para efecto visual de rotación violenta
                rb.AddTorque(Random.Range(-500f, 500f), ForceMode2D.Impulse);

                // NUEVO: Impulso secundario retardado para efecto de explosión
                StartCoroutine(EfectoExplosivo(direccionImpulso, fuerzaBase));

                Debug.Log($"IMPACTO POTENTE - Fuerza: {fuerzaBase}, Multiplicador: {multiplicadorFuerza}");
            }
        }
        else if (collision.gameObject.CompareTag("ColisionInvisible"))
        {
            // Manejar colisiones con los bordes con rebotes más violentos
            ContactPoint2D punto = collision.GetContact(0);
            Vector2 normal = punto.normal;
            Vector2 velocidadEntrada = rb.linearVelocity;

            // Rebote mejorado con aceleración en cada rebote
            Vector2 velocidadRebote = Vector2.Reflect(velocidadEntrada, normal) * 0.8f;

            rb.linearVelocity = velocidadRebote;
            rb.AddTorque(Random.Range(-100f, 100f), ForceMode2D.Impulse);

            Debug.Log("Pelota colisionó con el borde: " + collision.gameObject.name + "a velocidad: " + rb.linearVelocity);
        }
    }

    // Sistema de impulsos múltiples para efecto explosivo
    System.Collections.IEnumerator EfectoExplosivo(Vector2 direccion, float fuerza)
    {
        yield return new WaitForFixedUpdate();

        // Primer impulso adicional
        rb.AddForce(direccion * fuerza * 0.4f, ForceMode2D.Impulse);

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Segundo impulso para efecto de "aceleración después del golpe"
        rb.AddForce(direccion * fuerza * 0.3f, ForceMode2D.Impulse);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Evitar que la pelota se quede pegada con una fuerza extrema
            Vector2 direccionSeparacion = (rb.position - collision.rigidbody.position).normalized;

            // Fuerza de separación EXTREMA
            rb.AddForce(direccionSeparacion * 300f, ForceMode2D.Force);

            // Añadir efecto de "rebote" adicional
            rb.AddForce(Vector2.up * 50f, ForceMode2D.Force);
        }
    }
}