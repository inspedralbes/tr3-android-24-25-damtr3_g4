using UnityEngine;

public class ColisionInvisible : MonoBehaviour
{
    public PhysicsMaterial2D gomaMaterial; // Asigna el material desde el inspector

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

        collider.isTrigger = false; // Cambia a false para permitir colisiones físicas

        // Asigna el material físico al collider
        if (gomaMaterial != null)
        {
            collider.sharedMaterial = gomaMaterial;
        }
        else
        {
            Debug.LogWarning("No se asignó un Physics Material 2D al objeto.");
        }
    }

    // Método para manejar colisiones
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Colisión con: " + collision.gameObject.name);
    }
}