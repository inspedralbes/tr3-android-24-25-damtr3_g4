using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPorRaton : MonoBehaviour
{
    private float velocidad = 120f;
    public static ControlPorRaton jugadorSeleccionado = null;

    private bool seleccionado = false;
    private bool enMovimiento = false;
    private Rigidbody2D rb;
    private GameObject border;
    public bool activo = true;

    public LineRenderer lineRenderer; // Para dibujar la trayectoria
    private List<Vector3> trajectoryPoints = new List<Vector3>();

    [SerializeField]
    private float friction = 0.98f;
    [SerializeField]
    private float minVelocityToStop = 0.1f; // Velocidad mínima para detener el jugador

    [SerializeField]
    public GameObject flechaPrefab; // Prefab de la flecha
    private GameObject flecha; // Instancia de la flecha

    // Cursor personalizado (opcional)
    public Texture2D cursorPointerTexture;
    private CursorMode cursorMode = CursorMode.Auto;
    private Vector2 hotSpot = Vector2.zero;

    void Start()
    {
        if (this.gameObject.GetComponent<CircleCollider2D>() == null)
        {
            this.gameObject.AddComponent<CircleCollider2D>();
        }

        rb = this.gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = this.gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PhysicsMaterial2D materialRebote = new PhysicsMaterial2D();
        materialRebote.bounciness = 0.9f; // Ajustar el rebote según sea necesario
        materialRebote.friction = 0.1f; // Ajustar la fricción según sea necesario

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        collider.sharedMaterial = materialRebote; // Asignar el material al collider

        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f; // Ajustar la fricción angular según sea necesario
        // Asegurar que haya un LineRenderer
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }
        lineRenderer.enabled = false;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (jugadorSeleccionado == null || !jugadorSeleccionado.seleccionado || enMovimiento)
        {
            if (flecha != null)
            {
                flecha.SetActive(false);
            }
            return;
        }

        if (flecha != null && jugadorSeleccionado == this) // Solo mostrar la flecha si este es el jugador seleccionado
        {
            flecha.SetActive(true);

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            Vector3 direccion = (mousePos - transform.position).normalized;
            float radio = GetComponent<CircleCollider2D>().radius * transform.localScale.x;
            flecha.transform.position = transform.position + direccion * radio;
            flecha.transform.up = direccion;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 rightClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rightClickPos.z = 0;

            if (flecha != null)
            {
                flecha.SetActive(false); // Ocultar la flecha al iniciar el movimiento
            }

            trajectoryPoints.Clear();
            trajectoryPoints.Add(jugadorSeleccionado.transform.position);
            trajectoryPoints.Add(rightClickPos);

            lineRenderer.positionCount = trajectoryPoints.Count;
            lineRenderer.SetPositions(trajectoryPoints.ToArray());
            lineRenderer.enabled = true;

            jugadorSeleccionado.StartCoroutine(jugadorSeleccionado.MoverJugador(rightClickPos));
        }
    }

    private void OnMouseDown()
    {
        if (jugadorSeleccionado != this)
        {
            if (jugadorSeleccionado != null)
            {
                jugadorSeleccionado.Deseleccionar();
            }
            Seleccionar();
        }
    }

    private void Seleccionar()
    {
        jugadorSeleccionado = this;
        seleccionado = true;
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;

        if (border != null)
        {
            border.SetActive(true);
        }

        if (flechaPrefab != null && flecha == null)
        {
            flecha = Instantiate(flechaPrefab, transform.position, Quaternion.identity, transform);
            flecha.transform.localScale = new Vector3(0.5f, 0.6f, 1f); // Ajustar el tamaño de la flecha
        }

        if (flecha != null)
        {
            flecha.SetActive(true); // Activar la flecha al seleccionar el jugador
        }

        Debug.Log("Jugador seleccionado: " + gameObject.name);
    }

    private void Deseleccionar()
    {
        seleccionado = false;
        lineRenderer.enabled = false;
        trajectoryPoints.Clear();

        if (border != null)
        {
            border.SetActive(false);
        }

        if (flecha != null)
        {
            Destroy(flecha); // Eliminar la flecha al deseleccionar
            flecha = null;
        }
    }

    private void OnMouseEnter()
    {
        if (cursorPointerTexture != null)
        {
            Cursor.SetCursor(cursorPointerTexture, hotSpot, cursorMode);
        }
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    IEnumerator MoverJugador(Vector3 destino)
    {
        enMovimiento = true;
        Vector3 inicio = transform.position;
        float tiempo = 0;

        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * (velocidad / Vector3.Distance(inicio, destino));
            rb.MovePosition(Vector3.Lerp(inicio, destino, tiempo)); // Usa MovePosition en lugar de modificar transform.position
            yield return null;
        }

        rb.MovePosition(destino);
        lineRenderer.enabled = false;

        enMovimiento = false;

        // Deseleccionar al jugador al finalizar el movimiento
        Deseleccionar();
    }

    void FixedUpdate()
    {
        // Aplicar fricción continuamente
        if (rb.linearVelocity.magnitude > minVelocityToStop)
        {
            rb.linearVelocity *= friction;
        }
        else if (!enMovimiento) // Solo detener completamente si no está en movimiento controlado
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Verifica que es otro jugador
        {
            Rigidbody2D rbOtro = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbOtro != null)
            {
                // Calcular la dirección del rebote
                Vector2 direccionRebote = (rb.position - rbOtro.position).normalized;

                // Reducir la fuerza del rebote
                float fuerzaRebote = 2f; // Ajusta este valor para un rebote más suave

                // Aplicar la fuerza en direcciones opuestas, proporcional a la velocidad actual
                rb.linearVelocity = direccionRebote * fuerzaRebote;
                rbOtro.linearVelocity = -direccionRebote * fuerzaRebote;

                // Reducir gradualmente la velocidad de ambos jugadores
                StartCoroutine(ReducirVelocidad(rb));
                StartCoroutine(ReducirVelocidad(rbOtro));

                Debug.Log($"Rebote entre {gameObject.name} y {collision.gameObject.name}");
            }
        }
    }

    IEnumerator ReducirVelocidad(Rigidbody2D rb)
    {
        float duracion = 1.5f; // Tiempo hasta que se detiene completamente
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            // Reduce la velocidad gradualmente
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.1f);
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Asegurarse de que la velocidad sea cero al final
        rb.linearVelocity = Vector2.zero;
    }
}