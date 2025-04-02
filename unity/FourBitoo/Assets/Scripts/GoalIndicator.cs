using UnityEngine;
using System.Collections;

public class GoalIndicator : MonoBehaviour
{
    public int goalTeamID; // ID del equipo cuya portería es esta (1 o 2)
    
    [SerializeField] private GameObject arrowPrefab; // Prefab de flecha para mostrar sobre la portería
    private GameObject arrowInstance; // Instancia de la flecha
    
    // Colores para los equipos (mismo esquema de colores que ControlPorRaton)
    [SerializeField] private Color teamColor1 = new Color(1f, 0.2f, 0.2f, 1f); // Rojo para equipo 1
    [SerializeField] private Color teamColor2 = new Color(0.2f, 0.2f, 1f, 1f); // Azul para equipo 2
    
    // Tamaño de la flecha
    [SerializeField] private Vector3 arrowScale = new Vector3(2.0f, 2.0f, 1f); // Escala más grande por defecto
    
    // Variables para la animación de pulso
    private Coroutine pulseCoroutine;
    [SerializeField] private float pulseAmount = 0.2f;
    [SerializeField] private float pulseSpeed = 2f;
    private Vector3 originalArrowScale;
    
    // Equipo que debe marcar en esta portería (el contrario al dueño de la portería)
    private int teamToScore;
    
    // Ajustes de posición
    [SerializeField] private float heightOffset = 2.5f; // Distancia sobre la portería
    [SerializeField] private bool useOriginalPrefabScale = true; // Mantener escala original del prefab
    [SerializeField] private bool useOriginalPrefabPosition = true; // Mantener posición original del prefab
    
    void Start()
    {
        // Determinar qué equipo debe marcar en esta portería (el contrario)
        teamToScore = (goalTeamID == 1) ? 2 : 1;
        
        // Crear la flecha sobre la portería (inicialmente oculta)
        CreateArrowOverGoal();
        
        // Ocultar la flecha al inicio
        if (arrowInstance != null)
        {
            arrowInstance.SetActive(false);
        }
    }
    
    void Update()
    {
        // Comprobar si hay un jugador seleccionado del equipo que debe marcar aquí
        if (ControlPorRaton.jugadorSeleccionado != null)
        {
            int selectedTeamID = ControlPorRaton.jugadorSeleccionado.GetTeamID();
            
            // Si el jugador seleccionado es del equipo que debe marcar en esta portería
            if (selectedTeamID == teamToScore)
            {
                // Mostrar la flecha
                if (arrowInstance != null && !arrowInstance.activeSelf)
                {
                    arrowInstance.SetActive(true);
                    
                    // Iniciar la animación de pulso
                    if (pulseCoroutine != null)
                    {
                        StopCoroutine(pulseCoroutine);
                    }
                    pulseCoroutine = StartCoroutine(PulseAnimation());
                }
            }
            else
            {
                // Ocultar la flecha si el jugador seleccionado es del otro equipo
                if (arrowInstance != null && arrowInstance.activeSelf)
                {
                    arrowInstance.SetActive(false);
                    
                    // Detener la animación de pulso
                    if (pulseCoroutine != null)
                    {
                        StopCoroutine(pulseCoroutine);
                        pulseCoroutine = null;
                    }
                }
            }
        }
        else
        {
            // No hay jugador seleccionado, ocultar la flecha
            if (arrowInstance != null && arrowInstance.activeSelf)
            {
                arrowInstance.SetActive(false);
                
                // Detener la animación de pulso
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                    pulseCoroutine = null;
                }
            }
        }
    }
    
    // Crear la instancia de la flecha sobre la portería
    private void CreateArrowOverGoal()
    {
        // Buscar el collider de la portería para posicionar la flecha
        BoxCollider2D goalCollider = GetComponent<BoxCollider2D>();
        if (goalCollider == null)
        {
            Debug.LogError("La portería necesita un BoxCollider2D para posicionar correctamente la flecha");
            return;
        }
        
        // Usar el prefab asignado manualmente en el inspector
        if (arrowPrefab == null)
        {
            Debug.LogError("No se ha asignado un prefab de flecha en el inspector para la portería " + goalTeamID);
            return;
        }
        
        // Crear la flecha - mantener la posición original del prefab si está configurado así
        if (useOriginalPrefabPosition)
        {
            // Instantiate manteniendo la posición original del prefab
            arrowInstance = Instantiate(arrowPrefab);
            
            // Solo ajustar la posición X para alinear con la portería
            arrowInstance.transform.position = new Vector3(
                transform.position.x,
                arrowInstance.transform.position.y,
                arrowInstance.transform.position.z);
                
            Debug.Log("Usando posición original del prefab: Y=" + arrowInstance.transform.position.y);
        }
        else
        {
            // Método original que posiciona en relación a la portería
            arrowInstance = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            
            // Posicionar la flecha sobre la portería (más arriba)
            float arrowYOffset = goalCollider.size.y + heightOffset;
            arrowInstance.transform.position = new Vector3(
                transform.position.x, 
                transform.position.y + arrowYOffset, 
                transform.position.z - 0.1f);
                
            Debug.Log("Posicionando flecha a Y=" + arrowInstance.transform.position.y);
        }
        
        // Gestionar la escala de la flecha
        if (!useOriginalPrefabScale)
        {
            // Usar la escala configurada en el inspector de este script
            arrowInstance.transform.localScale = arrowScale;
        }
        
        // Guardar la escala para la animación de pulso
        originalArrowScale = arrowInstance.transform.localScale;
        
        // Añadir el texto debajo de la flecha
        CreateTextLabel();
        
        // Colorear según el equipo que debe marcar en esta portería
        UpdateArrowColor();
    }
    
    // Crear el texto indicador
    private void CreateTextLabel()
    {
        GameObject textObj = new GameObject("GoalText");
        textObj.transform.SetParent(arrowInstance.transform);
        textObj.transform.localPosition = new Vector3(0f, -1.5f, 0f); // Más separado de la flecha
        
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.fontSize = 40; // Texto más grande
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        
        // Configurar el texto según qué equipo debe marcar aquí
        if (teamToScore == 1)
        {
            textMesh.text = "EQUIPO 1";
            textMesh.color = teamColor1;
        }
        else
        {
            textMesh.text = "EQUIPO 2";
            textMesh.color = teamColor2;
        }
    }
    
    // Actualizar el color de la flecha según el equipo que debe marcar
    private void UpdateArrowColor()
    {
        if (arrowInstance != null)
        {
            // Buscar todos los SpriteRenderer en el prefab y sus hijos
            SpriteRenderer[] renderers = arrowInstance.GetComponentsInChildren<SpriteRenderer>(true);
            
            // Si no se encuentran renderers, buscar en el objeto principal
            if (renderers.Length == 0)
            {
                SpriteRenderer mainRenderer = arrowInstance.GetComponent<SpriteRenderer>();
                if (mainRenderer != null)
                {
                    renderers = new SpriteRenderer[] { mainRenderer };
                }
            }
            
            // Aplicar el color a todos los renderers encontrados
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer != null)
                {
                    // Colorear según el equipo que debe marcar
                    if (teamToScore == 1)
                    {
                        renderer.color = teamColor1; // Rojo para equipo 1
                        Debug.Log("Aplicando color rojo al equipo 1: " + teamColor1);
                    }
                    else
                    {
                        renderer.color = teamColor2; // Azul para equipo 2
                        Debug.Log("Aplicando color azul al equipo 2: " + teamColor2);
                    }
                }
            }
        }
    }
    
    // Animación de pulso para la flecha
    private IEnumerator PulseAnimation()
    {
        while (arrowInstance != null && arrowInstance.activeSelf)
        {
            // Ciclo de escala arriba y abajo
            for (float t = 0; t <= 1; t += Time.deltaTime * pulseSpeed)
            {
                if (arrowInstance == null || !arrowInstance.activeSelf) break;
                
                float scale = 1 + Mathf.Sin(t * Mathf.PI) * pulseAmount;
                arrowInstance.transform.localScale = originalArrowScale * scale;
                yield return null;
            }
        }
        
        // Restaurar escala original al finalizar
        if (arrowInstance != null)
        {
            arrowInstance.transform.localScale = originalArrowScale;
        }
    }
    
    // Animar la flecha para hacerla más visible
    private IEnumerator AnimateArrow()
    {
        // Guardar la posición original
        Vector3 originalPosition = arrowInstance.transform.position;
        
        while (arrowInstance != null && arrowInstance.activeSelf) // Solo animar mientras esté activa
        {
            // Mover la flecha arriba y abajo con un patrón sinusoidal
            for (float t = 0; t < Mathf.PI * 2; t += 0.05f)
            {
                if (arrowInstance == null || !arrowInstance.activeSelf) break;
                
                float yOffset = Mathf.Sin(t) * 0.3f; // Mayor amplitud para que se note más
                arrowInstance.transform.position = new Vector3(
                    originalPosition.x,
                    originalPosition.y + yOffset,
                    originalPosition.z
                );
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
}
