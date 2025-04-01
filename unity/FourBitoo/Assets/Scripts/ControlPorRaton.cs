using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPorRaton : MonoBehaviour
{
    private float velocidad = 400f;
    public static ControlPorRaton jugadorSeleccionado = null;

    private bool seleccionado = false;
    private bool enMovimiento = false;
    private Rigidbody2D rb;
    private GameObject border;
    public bool activo = true;

    // Corrutina de movimiento para poder cancelarla
    private Coroutine movimientoCoroutine = null;

    // Variables para el efecto de selección
    private Vector3 originalScale;
    private Color originalColor;
    private Coroutine pulseCoroutine;
    [SerializeField] private float pulseAmount = 0.2f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private Color selectionColor = new Color(1f, 0.8f, 0.1f, 1f); // Color amarillo dorado para selección

    // Marcador de selección (icono sobre el jugador)
    [SerializeField] private GameObject selectionMarkerPrefab; // Prefab del marcador de selección
    private GameObject selectionMarker; // Instancia del marcador

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

        // Guardar la escala y color original para usarlos luego en las animaciones
        originalScale = transform.localScale;
        originalColor = GetComponent<SpriteRenderer>().color;

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PhysicsMaterial2D materialRebote = new PhysicsMaterial2D();
        materialRebote.bounciness = 0.9f; // Ajustar el rebote según sea necesario
        materialRebote.friction = 0.1f; // Ajustar la fricción según sea necesario

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.sharedMaterial = materialRebote; // Asignar el material al collider
        }
        else
        {
            Debug.LogError("No se encontró CircleCollider2D en " + gameObject.name);
        }


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

        // Crear el marcador de selección si no existe el prefab
        if (selectionMarkerPrefab == null)
        {
            CreateDefaultSelectionMarker();
        }
    }

    // Crear un marcador de selección por defecto (un círculo con el color de selección)
    private void CreateDefaultSelectionMarker()
    {
        selectionMarkerPrefab = new GameObject("SelectionMarkerPrefab");
        SpriteRenderer markerRenderer = selectionMarkerPrefab.AddComponent<SpriteRenderer>();
        
        // Crear un sprite circular simple
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++) 
        {
            int x = i % 64 - 32;
            int y = i / 64 - 32;
            float distSqr = x * x + y * y;
            
            // Crear un círculo con borde
            if (distSqr < 900 && distSqr > 700)
            {
                colors[i] = selectionColor;
            }
            else
            {
                colors[i] = new Color(0, 0, 0, 0); // Transparente
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100);
        markerRenderer.sprite = sprite;
        markerRenderer.sortingOrder = 10; // Asegurar que está sobre el jugador
        
        // No destruir el prefab al cargar nuevas escenas
        DontDestroyOnLoad(selectionMarkerPrefab);
        selectionMarkerPrefab.SetActive(false);
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

            // Guardar la referencia a la corrutina para poder cancelarla
            if (jugadorSeleccionado.movimientoCoroutine != null)
            {
                jugadorSeleccionado.StopCoroutine(jugadorSeleccionado.movimientoCoroutine);
            }
            jugadorSeleccionado.movimientoCoroutine = jugadorSeleccionado.StartCoroutine(jugadorSeleccionado.MoverJugador(rightClickPos));
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
        
        // Cambiar el color para indicar la selección
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = selectionColor;

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

        // Iniciar animación de pulsación
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        pulseCoroutine = StartCoroutine(PulseAnimation());

        // Activar el marcador de selección
        if (selectionMarker == null && selectionMarkerPrefab != null)
        {
            selectionMarker = Instantiate(selectionMarkerPrefab, transform.position, Quaternion.identity);
            selectionMarker.transform.SetParent(transform);
            selectionMarker.transform.localPosition = new Vector3(0, 0, -0.1f); // Colocar justo encima del jugador
            selectionMarker.transform.localScale = Vector3.one * 1.5f; // Escalar adecuadamente
            
            // Activar y hacer visible el marcador
            selectionMarker.SetActive(true);
            
            // Si tiene SpriteRenderer, asegurar que tiene el color correcto
            SpriteRenderer markerRenderer = selectionMarker.GetComponent<SpriteRenderer>();
            if (markerRenderer != null)
            {
                markerRenderer.color = selectionColor;
            }
        }
        else if (selectionMarker != null)
        {
            selectionMarker.SetActive(true);
        }

        Debug.Log("Jugador seleccionado: " + gameObject.name);
    }

    private void Deseleccionar()
    {
        seleccionado = false;
        lineRenderer.enabled = false;
        trajectoryPoints.Clear();
        
        // Detener la animación de pulsación
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // Restaurar la escala original
        transform.localScale = originalScale;
        
        // Restaurar el color original
        GetComponent<SpriteRenderer>().color = originalColor;
        
        if (border != null)
        {
            border.SetActive(false);
        }

        if (flecha != null)
        {
            Destroy(flecha); // Eliminar la flecha al deseleccionar
            flecha = null;
        }
        
        // Desactivar el marcador de selección
        if (selectionMarker != null)
        {
            Destroy(selectionMarker);
            selectionMarker = null;
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
        movimientoCoroutine = null;

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
        Debug.Log($"Colisión detectada entre {gameObject.name} (Tag: {gameObject.tag}, RB: {rb != null}, Collider: {GetComponent<Collider2D>() != null}) y {collision.gameObject.name} (Tag: {collision.gameObject.tag}, RB: {collision.gameObject.GetComponent<Rigidbody2D>() != null}, Collider: {collision.gameObject.GetComponent<Collider2D>() != null})");

        // Si estamos en movimiento programado, cancelarlo para permitir un rebote físico
        if (enMovimiento && movimientoCoroutine != null)
        {
            StopCoroutine(movimientoCoroutine);
            movimientoCoroutine = null;
            enMovimiento = false;
            lineRenderer.enabled = false;

            // Si estaba seleccionado, mantener la selección después de la colisión
            if (seleccionado && flecha != null)
            {
                flecha.SetActive(true);
            }
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rbOtro = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbOtro != null)
            {
                Debug.Log($"{gameObject.name} - Velocidad ANTES del impulso: {rb.linearVelocity}");
                Debug.Log($"{collision.gameObject.name} - Velocidad ANTES del impulso: {rbOtro.linearVelocity}");

                // Para un rebote más realista, usar el punto de contacto
                ContactPoint2D contacto = collision.GetContact(0);
                Vector2 normal = contacto.normal; // Normal en el punto de contacto

                // Obtener las velocidades actuales
                Vector2 velocidadA = rb.linearVelocity;
                Vector2 velocidadB = rbOtro.linearVelocity;

                // Calcular las masas (o usar las reales si están configuradas)
                float masaA = rb.mass;
                float masaB = rbOtro.mass;

                // Calcular la velocidad relativa en dirección de la normal
                float velocidadRelativa = Vector2.Dot(velocidadB - velocidadA, normal);

                // Calcular el impulso (con un coeficiente de restitución para el rebote)
                float coefRestitution = 1.2f; // Mayor que 1 para un rebote más enérgico
                float impulso = (2.0f * velocidadRelativa) / (masaA + masaB) * coefRestitution;

                // Aplicar el impulso a ambos cuerpos en dirección de la normal
                rb.linearVelocity = velocidadA + impulso * masaB * normal;
                rbOtro.linearVelocity = velocidadB - impulso * masaA * normal;

                // Multiplicar por un factor para hacer el rebote más pronunciado
                float factorRebote = 2.5f;
                rb.linearVelocity *= factorRebote;
                rbOtro.linearVelocity *= factorRebote;

                Debug.Log($"{gameObject.name} - Velocidad DESPUÉS del impulso: {rb.linearVelocity}");
                Debug.Log($"{collision.gameObject.name} - Velocidad DESPUÉS del impulso: {rbOtro.linearVelocity}");

                // Asegurarse de que ambos jugadores están en modo "no controlado"
                ControlPorRaton controlOtro = collision.gameObject.GetComponent<ControlPorRaton>();
                if (controlOtro != null && controlOtro.enMovimiento && controlOtro.movimientoCoroutine != null)
                {
                    controlOtro.StopCoroutine(controlOtro.movimientoCoroutine);
                    controlOtro.movimientoCoroutine = null;
                    controlOtro.enMovimiento = false;
                    controlOtro.lineRenderer.enabled = false;

                    // Si estaba seleccionado, mantener la selección después de la colisión
                    if (controlOtro.seleccionado && controlOtro.flecha != null)
                    {
                        controlOtro.flecha.SetActive(true);
                    }
                }

                // Reducir gradualmente la velocidad de ambos jugadores después del rebote
                StartCoroutine(ReducirVelocidad(rb));
                StartCoroutine(ReducirVelocidad(rbOtro));

                Debug.Log($"Rebote aplicado entre {gameObject.name} y {collision.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"El objeto {collision.gameObject.name} tiene la etiqueta 'Player' pero no tiene Rigidbody2D.");
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
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.05f);
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Asegurarse de que la velocidad sea cero al final
        rb.linearVelocity = Vector2.zero;
    }
    
    // Corrutina para la animación de pulsación
    private IEnumerator PulseAnimation()
    {
        float t = 0;
        while (seleccionado)
        {
            t += Time.deltaTime * pulseSpeed;
            float pulse = 1 + Mathf.Sin(t) * pulseAmount;
            transform.localScale = originalScale * pulse;
            yield return null;
        }
    }
}