using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Para trabajar con UI

public class ControlPorRaton : MonoBehaviour
{
    private float velocidad = 70f;
    public static ControlPorRaton jugadorSeleccionado = null;

    private bool seleccionado = false;
    private bool enMovimiento = false;
    private Rigidbody2D rb;
    private GameObject border;
    public bool activo = true;
    [SerializeField] private int teamID; // ID del equipo al que pertenece este jugador (1 o 2)

    // Corrutina de movimiento para poder cancelarla
    private Coroutine movimientoCoroutine = null;

    // Variables para el efecto de selección
    private Vector3 originalScale;
    private Color originalColor;
    private Coroutine pulseCoroutine;
    [SerializeField] private float pulseAmount = 0.2f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private Color selectionColorTeam1 = new Color(0.2f, 0.2f, 1f, 0.6f); // Color AZUL para equipo 1
    [SerializeField] private Color selectionColorTeam2 = new Color(1f, 0.2f, 0.2f, 0.6f); // Color ROJO para equipo 2
    [SerializeField] private GameObject selectionMarkerPrefab; // Prefab del marcador de selección
    private GameObject selectionMarker; // Instancia del marcador

    public LineRenderer lineRenderer; // Para dibujar la trayectoria
    private List<Vector3> trajectoryPoints = new List<Vector3>();

    [SerializeField]
    public GameObject flechaPrefab; // Prefab de la flecha
    private GameObject flecha; // Instancia de la flecha

    // Cursor personalizado (opcional)
    public Texture2D cursorPointerTexture;
    private CursorMode cursorMode = CursorMode.Auto;
    private Vector2 hotSpot = Vector2.zero;

    // Referencias para trabajar con Canvas (si usa UI)
    private Image playerImage;
    private bool usingCanvasImage = false;
    
    // Destino marcado (para el sistema de turnos)
    private Vector3? destinoMarcado = null;
    private bool destinoConfirmado = false;

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

        // Verificar si estamos usando una imagen UI o un SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            usingCanvasImage = false;
            originalColor = spriteRenderer.color;
        }
        else
        {
            // Buscar la imagen UI dentro del Canvas
            Transform canvasTransform = transform.Find("Canvas");
            if (canvasTransform != null)
            {
                Transform captionImageTransform = canvasTransform.Find("CaptionImage");
                if (captionImageTransform != null)
                {
                    playerImage = captionImageTransform.GetComponent<Image>();
                    if (playerImage != null)
                    {
                        usingCanvasImage = true;
                        originalColor = playerImage.color;
                    }
                }
            }
        }

        // Guardar la escala original para usarla luego en las animaciones
        originalScale = transform.localScale;

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
        
        // Registrar en el sistema de turnos al inicio
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayersByTeamID();
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
            
            // Crear un círculo con borde más delgado
            if (distSqr < 840 && distSqr > 800)
            {
                Color teamColor = (teamID == 1) ? selectionColorTeam1 : selectionColorTeam2;
                colors[i] = teamColor;
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
        // Solo realizar acciones si este es el jugador seleccionado actualmente
        if (jugadorSeleccionado != this)
        {
            // Asegurarse de que los elementos visuales estén desactivados para jugadores no seleccionados
            if (flecha != null)
            {
                flecha.SetActive(false);
            }
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
            if (selectionMarker != null)
            {
                selectionMarker.SetActive(false);
            }
            return;
        }

        // Si el jugador está en movimiento, ocultar la flecha
        if (enMovimiento)
        {
            if (flecha != null)
            {
                flecha.SetActive(false);
            }
            return;
        }

        // Solo mostrar la flecha si este jugador está seleccionado y es su turno
        if (flecha != null && CanMove()) 
        {
            flecha.SetActive(true);

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            Vector3 direccion = (mousePos - transform.position).normalized;
            float radio = GetComponent<CircleCollider2D>().radius * transform.localScale.x;
            flecha.transform.position = transform.position + direccion * radio;
            flecha.transform.up = direccion;
            
            // Si el destino está marcado, mostrar la línea de trayectoria
            if (destinoMarcado.HasValue)
            {
                trajectoryPoints.Clear();
                trajectoryPoints.Add(transform.position);
                trajectoryPoints.Add(destinoMarcado.Value);
                
                lineRenderer.positionCount = trajectoryPoints.Count;
                lineRenderer.SetPositions(trajectoryPoints.ToArray());
                lineRenderer.enabled = true;
            }
        }

        // Solo permitir marcar destino si es el turno del jugador
        if (Input.GetMouseButtonDown(1) && CanMove())
        {
            MarcarDestino();
        }
    }
    
    // Método para marcar el destino con el clic derecho
    private void MarcarDestino()
    {
        // Solo permitir marcar destino si es el jugador actual en turno
        if (!CanMove())
        {
            Debug.LogWarning($"No puedes marcar destino para {gameObject.name} (Tag: {gameObject.tag}) porque no es su turno.");
            return;
        }
        
        Vector3 rightClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        rightClickPos.z = 0;
        
        // Marcar el destino (pero no moverse aún)
        destinoMarcado = rightClickPos;
        destinoConfirmado = false;
        
        if (flecha != null)
        {
            flecha.SetActive(false); // Ocultar la flecha al marcar destino
        }

        // Mostrar trayectoria
        trajectoryPoints.Clear();
        trajectoryPoints.Add(transform.position);
        trajectoryPoints.Add(rightClickPos);

        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());
        lineRenderer.enabled = true;
        
        // Confirmar el destino inmediatamente y pasar al siguiente jugador
        ConfirmarDestino();
        
        // Log de depuración
        Debug.Log($"Destino marcado para {gameObject.name} (Tag: {gameObject.tag}). Pasando al siguiente jugador.");
    }

    private void OnMouseDown()
    {
        // Solo permitimos marcar el destino si es el jugador actual según el orden por tag
        if (GameManager.Instance != null && GameManager.Instance.CanPlayerMove(gameObject))
        {
            // Este es el jugador que debe moverse ahora, no hacemos nada más
            // porque ya estará seleccionado automáticamente
            Debug.Log($"Este es el turno del jugador {gameObject.name} (Tag: {gameObject.tag})");
        }
        else
        {
            Debug.Log($"No es el turno del jugador {gameObject.name} (Tag: {gameObject.tag})");
        }
    }

    public void Seleccionar()
    {
        jugadorSeleccionado = this;
        seleccionado = true;
        
        // Cambiar el color para indicar la selección según el equipo
        Color teamColor = (teamID == 1) ? selectionColorTeam1 : selectionColorTeam2;
        
        if (usingCanvasImage && playerImage != null)
        {
            playerImage.color = teamColor;
        }
        else
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = teamColor;
            }
        }

        if (border != null)
        {
            border.SetActive(true);
        }

        // Crear la flecha solo cuando se selecciona este jugador
        if (flechaPrefab != null)
        {
            // Si ya existe una flecha, destruirla
            if (flecha != null)
            {
                Destroy(flecha);
                flecha = null;
            }
            
            // Crear una nueva flecha
            flecha = Instantiate(flechaPrefab, transform.position, Quaternion.identity, transform);
            flecha.transform.localScale = new Vector3(0.4f, 0.5f, 1f); // Reducido para mejor ajuste

            // Colorear la flecha según el equipo
            SpriteRenderer flechaRenderer = flecha.GetComponent<SpriteRenderer>();
            if (flechaRenderer != null)
            {
                flechaRenderer.color = teamColor;
            }
        }

        // Activar la flecha solo si el jugador puede moverse
        if (flecha != null)
        {
            flecha.SetActive(CanMove()); 
        }

        // Iniciar animación de pulsación
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        pulseCoroutine = StartCoroutine(PulseAnimation());

        // Crear y activar el marcador de selección solo para este jugador
        if (selectionMarkerPrefab != null)
        {
            // Si ya existe un marcador, destruirlo
            if (selectionMarker != null)
            {
                Destroy(selectionMarker);
                selectionMarker = null;
            }
            
            // Crear un nuevo marcador
            selectionMarker = Instantiate(selectionMarkerPrefab, transform.position, Quaternion.identity);
            selectionMarker.transform.SetParent(transform);
            selectionMarker.transform.localPosition = new Vector3(0, 0, -0.1f); // Colocar justo encima del jugador
            selectionMarker.transform.localScale = Vector3.one * 1.2f; // Reducido para hacer el borde más pequeño
            
            // Activar y hacer visible el marcador
            selectionMarker.SetActive(true);
            
            // Si tiene SpriteRenderer, asegurar que tiene el color correcto del equipo
            SpriteRenderer markerRenderer = selectionMarker.GetComponent<SpriteRenderer>();
            if (markerRenderer != null)
            {
                markerRenderer.color = teamColor;
            }
        }

        Debug.Log("Jugador seleccionado: " + gameObject.name);
    }

    private void Deseleccionar()
    {
        // Restaurar el color original
        if (usingCanvasImage && playerImage != null)
        {
            playerImage.color = originalColor;
        }
        else
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        if (border != null)
        {
            border.SetActive(false);
        }

        // Destruir la flecha para evitar objetos huérfanos
        if (flecha != null)
        {
            Destroy(flecha);
            flecha = null;
        }

        // Destruir el marcador de selección para evitar objetos huérfanos
        if (selectionMarker != null)
        {
            Destroy(selectionMarker);
            selectionMarker = null;
        }

        // Desactivar LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        // Detener la animación de pulsación si está en curso
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        seleccionado = false;
        
        // Si este jugador era el seleccionado, resetearlo
        if (jugadorSeleccionado == this)
        {
            jugadorSeleccionado = null;
        }
    }
    
    IEnumerator PulseAnimation()
    {
        float t = 0;
        
        while (true)
        {
            t += Time.deltaTime * pulseSpeed;
            float scale = 1 + (Mathf.Sin(t) * 0.5f + 0.5f) * pulseAmount;
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }

    // Método para confirmar el destino y notificar al GameManager
    public void ConfirmarDestino()
    {
        if (destinoMarcado.HasValue && !destinoConfirmado)
        {
            destinoConfirmado = true;
            
            // Notificar al GameManager que este jugador ha establecido su destino
            if (GameManager.Instance != null)
            {
                // Pasar el gameObject y dejar que GameManager obtenga el teamID
                GameManager.Instance.PlayerSetDestination(gameObject);
            }
            
            // Ocultar la flecha y desactivar la línea de trayectoria
            if (flecha != null)
            {
                flecha.SetActive(false);
            }
            
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
            
            Debug.Log("Destino confirmado para: " + gameObject.name);
        }
    }
    
    // Método para iniciar el movimiento (será llamado por el GameManager)
    public void InitiateMovement()
    {
        if (destinoMarcado.HasValue)
        {
            // Iniciar el movimiento
            if (movimientoCoroutine != null)
            {
                StopCoroutine(movimientoCoroutine);
            }
            
            // Si hay una flecha o marcador, ocultarlos antes del movimiento
            if (flecha != null)
            {
                flecha.SetActive(false);
            }
            
            if (selectionMarker != null)
            {
                selectionMarker.SetActive(false);
            }
            
            movimientoCoroutine = StartCoroutine(MoverJugador(destinoMarcado.Value));
            
            // Limpiar el destino marcado
            destinoMarcado = null;
        }
    }
    
    // Método para verificar si el jugador puede moverse (según el sistema de turnos)
    private bool CanMove()
    {
        return GameManager.Instance != null && GameManager.Instance.CanPlayerMove(gameObject);
    }

    // Método para verificar si el jugador está en movimiento (usado por GameManager)
    public bool IsMoving()
    {
        return enMovimiento;
    }

    // Para verificar si ya ha establecido un destino
    public bool HasDestinationSet()
    {
        return destinoMarcado.HasValue && destinoConfirmado;
    }
    
    // Obtener el destino marcado (para dibujar la trayectoria)
    public Vector3? GetMarkedDestination()
    {
        return destinoMarcado;
    }

    IEnumerator MoverJugador(Vector3 destino)
    {
        enMovimiento = true;
        Vector3 inicio = transform.position;
        float tiempo = 0;

        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * (velocidad / Vector3.Distance(inicio, destino));
            transform.position = Vector3.Lerp(inicio, destino, tiempo);
            yield return null;
        }

        transform.position = destino;
        lineRenderer.enabled = false;

        enMovimiento = false;
        movimientoCoroutine = null;
        destinoConfirmado = false;
        
        // Notificar al GameManager que este jugador ha terminado de moverse
        if (GameManager.Instance != null)
        {
            Debug.Log($"JUGADOR {gameObject.name} (Tag: {gameObject.tag}) HA TERMINADO DE MOVERSE. Notificando al GameManager...");
            GameManager.Instance.PlayerFinishedMoving(gameObject);
        }
        else
        {
            Debug.LogError("No se pudo notificar al GameManager que el jugador terminó - GameManager.Instance es null");
        }
    }

    void FixedUpdate()
    {
        // Solo aplicar fricción si no hay un movimiento controlado en curso
        if (!enMovimiento && rb.linearVelocity.magnitude > minVelocityToStop)
        {
            // Aplicar fricción gradualmente
            rb.linearVelocity *= friction;
            
            // Si la velocidad cae por debajo del umbral, detener completamente
            if (rb.linearVelocity.magnitude < minVelocityToStop)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    IEnumerator ReducirVelocidad(Rigidbody2D targetRb)
    {
        float time = 0;
        Vector2 initialVelocity = targetRb.linearVelocity;
        
        while (time < 2.0f) // Reducir durante 2 segundos
        {
            time += Time.deltaTime;
            targetRb.linearVelocity = Vector2.Lerp(initialVelocity, Vector2.zero, time / 2.0f);
            yield return null;
        }
        
        // Asegurarse de que la velocidad sea cero al final
        targetRb.linearVelocity = Vector2.zero;
    }
    
    // Manejar colisión entre jugadores
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si colisiona con otro jugador, no cambiar el turno
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.tag.StartsWith("Player "))
        {
            Debug.Log($"¡Colisión entre jugadores! No se cambia el turno.");
            // No hacer nada para mantener el turno actual
        }

        // Si estamos en movimiento programado, cancelarlo para permitir un rebote físico
        if (enMovimiento && movimientoCoroutine != null)
        {
            StopCoroutine(movimientoCoroutine);
            movimientoCoroutine = null;
            enMovimiento = false;
            
            if (destinoMarcado.HasValue)
            {
                lineRenderer.enabled = false;
            }
        }

        Debug.Log($"Colisión detectada entre {gameObject.name} (Tag: {gameObject.tag}, RB: {rb != null}, Collider: {GetComponent<Collider2D>() != null}) y {collision.gameObject.name} (Tag: {collision.gameObject.tag}, RB: {collision.gameObject.GetComponent<Rigidbody2D>() != null}, Collider: {collision.gameObject.GetComponent<Collider2D>() != null})");
    }

    // Getters para acceder a los valores desde otras clases
    public int GetTeamID()
    {
        return teamID;
    }
    
    // Métodos para el cursor personalizado (opcional)
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

    // Método para limpiar todos los elementos visuales (marcadores y flechas)
    public void CleanupVisualElements()
    {
        // Destruir la flecha si existe
        if (flecha != null)
        {
            Destroy(flecha);
            flecha = null;
        }
        
        // Destruir el marcador de selección si existe
        if (selectionMarker != null)
        {
            Destroy(selectionMarker);
            selectionMarker = null;
        }
        
        // Desactivar el LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        
        // Restaurar el color original
        if (usingCanvasImage && playerImage != null)
        {
            playerImage.color = originalColor;
        }
        else
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
        
        // Desactivar el borde
        if (border != null)
        {
            border.SetActive(false);
        }
        
        // Detener la animación de pulsación
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }

    // Reiniciar el estado del jugador al comenzar un nuevo ciclo
    public void ResetPlayerState()
    {
        // Reiniciar variables de estado
        enMovimiento = false;
        destinoConfirmado = false;
        seleccionado = false;
        
        // Detener cualquier corrutina en curso
        if (movimientoCoroutine != null)
        {
            StopCoroutine(movimientoCoroutine);
            movimientoCoroutine = null;
        }
        
        // Desactivar el LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        
        // Desactivar cualquier marcador de selección
        if (selectionMarker != null)
        {
            Destroy(selectionMarker);
            selectionMarker = null;
        }
        
        // Desactivar la flecha de dirección
        if (flecha != null)
        {
            Destroy(flecha);
            flecha = null;
        }
        
        // Detener el movimiento del Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        Debug.Log($"Estado del jugador {gameObject.name} reiniciado completamente");
    }
}