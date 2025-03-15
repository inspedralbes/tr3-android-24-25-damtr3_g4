using UnityEngine;

public class ColisionInvisible : MonoBehaviour
{
    void Start()
    {
        // Agrega un BoxCollider2D si no existe
        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        collider.size = new Vector2(1f, 1f);
        collider.offset = new Vector2(0f, 0f); 


        collider.isTrigger = true;
    }

    // Método para manejar colisiones
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Colisión con: " + other.gameObject.name);
    }
}