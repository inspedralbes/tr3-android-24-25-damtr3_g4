using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPorRaton : MonoBehaviour
{
    private float velocidad = 70f;
    public static ControlPorRaton jugadorSeleccionado = null;

    private bool seleccionado = false;
    private bool enMovimiento = false;
    private Rigidbody2D rb;
    private GameObject border;
    public bool activo = true;
    // Añadir propiedad TeamID que faltaba
    [SerializeField] private int teamID = 1; // Equipo por defecto 1
    
    // Propiedad para acceder al TeamID
    public int TeamID 
    {
        get { return teamID; }
        set { teamID = value; }
    }

    public LineRenderer lineRenderer; // Para dibujar la trayectoria
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    
    [SerializeField]
    public GameObject flechaPrefab; // Prefab de la flecha
    private GameObject flecha; // Instancia de la flecha

    // Cursor personalizado
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
        
        // Asegurar que haya un LineRenderer
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }
        
        lineRenderer.startWidth = 2f;
        lineRenderer.endWidth = 2f;
        lineRenderer.enabled = false;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (seleccionado && flecha != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            Vector3 direccion = (mousePos - transform.position).normalized;
            float radio = GetComponent<CircleCollider2D>().radius * transform.localScale.x;
            flecha.transform.position = transform.position + direccion * radio;
            flecha.transform.up = direccion;
        }

        if (Input.GetMouseButtonDown(1) && jugadorSeleccionado != null && !jugadorSeleccionado.enMovimiento)
        {
            Vector3 rightClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rightClickPos.z = 0;

            // Limpiar puntos anteriores
            jugadorSeleccionado.trajectoryPoints.Clear();
            
            // Añadir posición inicial y final
            jugadorSeleccionado.trajectoryPoints.Add(jugadorSeleccionado.transform.position);
            jugadorSeleccionado.trajectoryPoints.Add(rightClickPos);
            
            // Actualizar LineRenderer
            jugadorSeleccionado.lineRenderer.positionCount = jugadorSeleccionado.trajectoryPoints.Count;
            jugadorSeleccionado.lineRenderer.SetPositions(jugadorSeleccionado.trajectoryPoints.ToArray());
            jugadorSeleccionado.lineRenderer.enabled = true;

            jugadorSeleccionado.StartCoroutine(jugadorSeleccionado.MoverJugador(rightClickPos));
        }
    }

    // Hacerlo público para que otros scripts puedan llamarlo
    public void OnMouseDown()
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

    // Hacerlo público para que otros scripts puedan llamarlo
    public void Seleccionar()
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

    // Hacerlo público para que otros scripts puedan llamarlo
    public void Deseleccionar()
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
        
        // Desactivar la flecha durante el movimiento
        if (flecha != null)
        {
            flecha.SetActive(false);
        }

        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * (velocidad / Vector3.Distance(inicio, destino));
            transform.position = Vector3.Lerp(inicio, destino, tiempo);
            yield return null;
        }

        transform.position = destino;
        lineRenderer.enabled = false;

        enMovimiento = false;

        // Deseleccionar al jugador al finalizar el movimiento
        Deseleccionar();
    }
    
    // Añadir método GetTeamID para compatibilidad con scripts existentes
    public int GetTeamID()
    {
        return teamID;
    }
}