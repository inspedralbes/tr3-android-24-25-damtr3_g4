using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPorRaton : MonoBehaviour
{
    private float velocidad = 70f;
    public static List<ControlPorRaton> objetoSeleccionado = new List<ControlPorRaton>();
    private Vector3 prosicionJugador;
    private bool seleccionado = false;

    public bool activo = false;
    private Rigidbody2D rb;
    private GameObject border;
    //private GameObject flechaTrayectoria;    // Flecha de trayectoria (Comentado ya que usaremos LineRenderer)
    //private Vector3 direccionTrayectoria; // Dirección de la trayectoria (Comentado ya que usaremos LineRenderer)

    public LineRenderer lineRenderer; // Para dibujar la trayectoria
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    private bool isDrawing = false;

    void Start()
    {
        objetoSeleccionado.Add(this);
        prosicionJugador = this.transform.position;

        activo = false;

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

        // Encuentra el objeto "FlechaTrayectoria" hijo del jugador (Comentado)
        // flechaTrayectoria = transform.Find("FlechaTrayectoria")?.gameObject;
        // if (flechaTrayectoria != null)
        // {
        //     flechaTrayectoria.SetActive(false); // Asegúrate de que la flecha esté desactivada al inicio
        // }
        // else
        // {
        //     Debug.LogError($"No se encontró el objeto 'FlechaTrayectoria' como hijo del jugador: {gameObject.name}");
        // }

        // Asegurarse de que haya un LineRenderer
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }
        lineRenderer.enabled = false;
        lineRenderer.useWorldSpace = true; // Asegúrate de que los puntos estén en el espacio mundial
        lineRenderer.positionCount = 0; // Initialize positionCount here
    }

    void Update()
    {
        if (!activo) return;

        // Si el jugador está seleccionado y el clic del ratón está presionado (inicio del dibujo)
        if (Input.GetMouseButtonDown(0) && seleccionado)
        {
            isDrawing = true;
            trajectoryPoints.Clear();
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = true;
            // trajectoryPoints.Add(transform.position); // Moved to the first mouse move
            // lineRenderer.SetPosition(0, transform.position);
        }

        // Mientras el botón del ratón está presionado y estamos dibujando
        if (isDrawing && Input.GetMouseButton(0) && seleccionado)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Asegurarse de que esté en el plano 2D

            // Add the starting position on the first move
            if (trajectoryPoints.Count == 0)
            {
                trajectoryPoints.Add(transform.position);
                lineRenderer.positionCount = 1;
                lineRenderer.SetPosition(0, transform.position);
            }
            // Añadir el punto a la trayectoria si está lo suficientemente lejos del último punto
            else if (Vector3.Distance(trajectoryPoints[trajectoryPoints.Count - 1], mousePos) > 0.1f) // Ajusta la distancia según sea necesario
            {
                trajectoryPoints.Add(mousePos);
                lineRenderer.positionCount = trajectoryPoints.Count;
                lineRenderer.SetPositions(trajectoryPoints.ToArray()); // Line 80
            }
        }

        // Si se suelta el botón del ratón después de dibujar
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            lineRenderer.enabled = false;

            // La trayectoria dibujada ahora está en la lista 'trajectoryPoints'
            // El Vector3 que representa la trayectoria podría ser el vector desde la posición inicial
            // del jugador hasta el último punto de la trayectoria.
            if (trajectoryPoints.Count > 1)
            {
                Vector3 vectorTrayectoriaFinal = trajectoryPoints[trajectoryPoints.Count - 1] - transform.position;
                Debug.Log("Vector de trayectoria dibujado: " + vectorTrayectoriaFinal);
                // Aquí puedes usar 'vectorTrayectoriaFinal' para lo que necesites en tu juego.
            }
            else
            {
                Debug.Log("No se dibujó una trayectoria significativa.");
            }
        }
        else if (!Input.GetMouseButton(0) && !isDrawing && seleccionado)
        {
            // ... (rest of the code)
        }
        else if (!seleccionado)
        {
            // ... (rest of the code)
        }
    }

    private void OnMouseDown()
    {
        seleccionado = true;
        activo = true;
        Debug.Log("Objeto seleccionado: " + gameObject.name);

        this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;

        // Activa el borde del objeto seleccionado
        if (border != null)
        {
            border.SetActive(true);
        }

        foreach (ControlPorRaton jugador in objetoSeleccionado)
        {
            if (jugador != this)
            {
                jugador.seleccionado = false;
                jugador.activo = false;
                jugador.gameObject.GetComponent<SpriteRenderer>().color = Color.white;

                // Desactiva el borde de los otros objetos
                if (jugador.border != null)
                {
                    jugador.border.SetActive(false);
                }

                // Desactiva la flecha de los jugadores no seleccionados (Comentado)
                // if (jugador.flechaTrayectoria != null)
                // {
                //     jugador.flechaTrayectoria.SetActive(false);
                // }
                if (jugador.lineRenderer != null)
                {
                    jugador.lineRenderer.enabled = false;
                }
                jugador.isDrawing = false;
                jugador.trajectoryPoints.Clear();
            }
        }
    }
}