using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPorRaton : MonoBehaviour
{
    private float velocidad = 9f;
    public static List<ControlPorRaton> objetoSeleccionado = new List<ControlPorRaton>();
    private Vector3 prosicionJugador;
    private bool seleccionado = false;
    private Rigidbody2D rb;
    private GameObject border;

    void Start()
    {
        objetoSeleccionado.Add(this);
        prosicionJugador = this.transform.position;

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

        // Encuentra el objeto "rojo" hijo del jugador con el tag "border"
        border = transform.Find("rojo")?.gameObject;
        if (border != null && border.CompareTag("border"))
        {
            border.SetActive(false); // Asegúrate de que el borde esté desactivado al inicio
        }
        else
        {
            Debug.LogWarning("El objeto 'rojo' no tiene el tag 'border' o no se encontró.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && seleccionado)
        {
            prosicionJugador = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            prosicionJugador.z = this.transform.position.z;
        }

        this.transform.position = Vector3.MoveTowards(this.transform.position, prosicionJugador, velocidad * Time.deltaTime);
    }

    private void OnMouseDown()
    {
        seleccionado = true;
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
                jugador.gameObject.GetComponent<SpriteRenderer>().color = Color.white;

                // Desactiva el borde de los otros objetos
                if (jugador.border != null)
                {
                    jugador.border.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Colisión con otro jugador (Trigger)!");

            // Detener el movimiento del personaje actual
            rb.linearVelocity = Vector2.zero;
            prosicionJugador = transform.position; // Evitar que siga moviéndose hacia el objetivo

            // También detener al otro personaje (si tiene Rigidbody2D)
            Rigidbody2D rbOther = other.GetComponent<Rigidbody2D>();
            if (rbOther != null)
            {
                rbOther.linearVelocity = Vector2.zero;
            }
        }
    }
}