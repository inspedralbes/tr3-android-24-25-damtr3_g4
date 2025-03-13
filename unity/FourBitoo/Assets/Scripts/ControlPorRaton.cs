using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPorRaton : MonoBehaviour
{
    private float velocidad = 5f;
    public static List<ControlPorRaton> objetoSeleccionado = new List<ControlPorRaton>();
    private Vector3 prosicionJugador;
    private bool selecionado = false;
    void Start()
    {
        objetoSeleccionado.Add(this);
        prosicionJugador = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

       if(Input.GetMouseButtonDown(0) && selecionado) {
        prosicionJugador = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        prosicionJugador.z = this.transform.position.z;
       }

       this.transform.position = Vector3.MoveTowards(this.transform.position, prosicionJugador, velocidad * Time.deltaTime);
    }
    private void OnMouseDown()
    {
        selecionado = true;
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        foreach(ControlPorRaton jugador in objetoSeleccionado)
        {
            if(jugador != this)
            {
                jugador.selecionado = false;
                jugador.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }
}
