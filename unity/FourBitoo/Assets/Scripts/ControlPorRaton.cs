using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPorRaton : MonoBehaviour
{
    private float velocidad = 5f;
    private Vector3 prosicionJugador;

    void Start()
    {
        prosicionJugador = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

       if(Input.GetMouseButtonDown(0)) {
        prosicionJugador = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        prosicionJugador.z = this.transform.position.z;
       }

       this.transform.position = Vector3.MoveTowards(this.transform.position, prosicionJugador, velocidad * Time.deltaTime);
    }
}
