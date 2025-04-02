using UnityEngine;

public class GoalVisualizer : MonoBehaviour
{
    [SerializeField] private int goalTeamID; // ID del equipo cuya portería es esta (1 o 2)
    [SerializeField] private Color team1Color = new Color(1f, 0.2f, 0.2f, 0.5f); // Rojo semi-transparente
    [SerializeField] private Color team2Color = new Color(0.2f, 0.2f, 1f, 0.5f); // Azul semi-transparente
    
    [SerializeField] private GameObject visualIndicator; // Objeto visual que mostrará el color
    
    private SpriteRenderer indicatorRenderer;
    private TextMesh textIndicator;
    
    void Start()
    {
        // Siempre crear un nuevo indicador visual al iniciar
        // Destruir el anterior si existe
        if (visualIndicator != null)
        {
            Destroy(visualIndicator);
        }
        
        // Crear un indicador visual siempre
        visualIndicator = new GameObject("GoalIndicator");
        visualIndicator.transform.SetParent(transform);
        visualIndicator.transform.localPosition = Vector3.zero;
        
        // Añadir un SpriteRenderer
        indicatorRenderer = visualIndicator.AddComponent<SpriteRenderer>();
        
        // Crear un sprite cuadrado simple
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100);
        indicatorRenderer.sprite = sprite;
        indicatorRenderer.sortingOrder = 5; // Aumentado para asegurar visibilidad
        
        // Escalar el indicador para cubrir la portería y hacerlo más grande
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            visualIndicator.transform.localScale = new Vector3(
                collider.size.x * 1.5f, // Aumentado de 1.2f a 1.5f
                collider.size.y * 1.5f, // Aumentado de 1.2f a 1.5f
                1f
            );
        }
        else
        {
            // Si no hay collider, usar un tamaño predeterminado
            visualIndicator.transform.localScale = new Vector3(3f, 3f, 1f);
        }
        
        // Añadir texto indicador
        GameObject textObj = new GameObject("GoalText");
        textObj.transform.SetParent(visualIndicator.transform);
        textObj.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        
        textIndicator = textObj.AddComponent<TextMesh>();
        textIndicator.alignment = TextAlignment.Center;
        textIndicator.anchor = TextAnchor.MiddleCenter;
        textIndicator.fontSize = 30; // Aumentado de 14 a 30
        textIndicator.characterSize = 0.1f;
        textIndicator.color = Color.black; // Cambiar a negro para mayor contraste
        
        // Configurar el color según el equipo
        UpdateVisual();
    }
    
    void UpdateVisual()
    {
        if (indicatorRenderer != null)
        {
            // El color indica el equipo que debe marcar en esta portería
            // (el color es del equipo CONTRARIO al teamID de la portería)
            if (goalTeamID == 1)
            {
                indicatorRenderer.color = team2Color; // El equipo 2 marca en la portería 1
                if (textIndicator != null)
                {
                    textIndicator.text = "EQUIPO 2\nMARCA AQUÍ"; // Texto en mayúsculas
                    textIndicator.color = Color.white;
                    textIndicator.fontSize = 30;
                }
            }
            else if (goalTeamID == 2)
            {
                indicatorRenderer.color = team1Color; // El equipo 1 marca en la portería 2
                if (textIndicator != null)
                {
                    textIndicator.text = "EQUIPO 1\nMARCA AQUÍ"; // Texto en mayúsculas
                    textIndicator.color = Color.white;
                    textIndicator.fontSize = 30;
                }
            }
        }
    }
    
    // Método para actualizar dinámicamente el color si es necesario
    public void RefreshVisual()
    {
        UpdateVisual();
    }
}
