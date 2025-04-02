using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchTimerUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject halfTimePanel;
    [SerializeField] private GameObject endMatchPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    
    [Header("Posicionamiento")]
    [SerializeField] private bool useScoreManagerLayout = true; // Usar el layout junto al ScoreManager
    [SerializeField] private ScoreManager scoreManager; // Referencia al ScoreManager existente
    [SerializeField] private Vector2 offsetFromScore = new Vector2(150f, 0f); // Offset desde la puntuación
    
    [Header("Configuración")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private int warningThreshold = 60; // Mostrar en amarillo cuando quede 1 minuto
    [SerializeField] private int dangerThreshold = 30;  // Mostrar en rojo cuando queden 30 segundos
    
    // Referencia al panel del tiempo para animaciones
    [SerializeField] private RectTransform timerPanel;
    private Vector3 originalScale;
    private bool isPulsing = false;
    private Coroutine pulseCoroutine;
    
    private void Start()
    {
        // Buscar componentes si no están asignados
        if (timerText == null)
            timerText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (timerPanel != null)
            originalScale = timerPanel.localScale;
        
        // Ocultar paneles al inicio
        if (halfTimePanel != null)
            halfTimePanel.SetActive(false);
        
        if (endMatchPanel != null)
            endMatchPanel.SetActive(false);
        
        // Buscar ScoreManager si no está asignado y queremos usar su layout
        if (useScoreManagerLayout && scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
        
        // Posicionar el temporizador si estamos usando el layout del ScoreManager
        if (useScoreManagerLayout && scoreManager != null)
            PositionTimerNextToScoreManager();
        
        // Suscribirse a eventos del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerUpdated += UpdateTimerDisplay;
            GameManager.Instance.OnMatchStart += OnMatchStart;
            GameManager.Instance.OnHalfTime += OnHalfTime;
            GameManager.Instance.OnMatchEnd += OnMatchEnd;
            GameManager.Instance.OnGoalScored += OnGoalScored;
            
            // Actualizar la UI con los valores iniciales
            UpdateTimerDisplay(GameManager.Instance != null ? 
                GameManager.Instance.GetFormattedTime() : "00:00");
                
            if (scoreText != null)
                scoreText.text = GameManager.Instance != null ? 
                    GameManager.Instance.GetScoreText() : "0 - 0";
        }
        else
        {
            Debug.LogError("MatchTimerUI: No se ha encontrado el GameManager. Asegúrate de que existe en la escena.");
        }
    }
    
    // Posicionar el temporizador junto al ScoreManager
    private void PositionTimerNextToScoreManager()
    {
        if (scoreManager == null || timerPanel == null)
            return;
            
        // Verificar si el ScoreManager tiene textos para el puntaje
        if (scoreManager.team1ScoreText != null)
        {
            // Obtener la posición del texto de puntuación del equipo 2 (normalmente está a la derecha)
            RectTransform scoreTextRect = scoreManager.team2ScoreText != null ?
                scoreManager.team2ScoreText.rectTransform :
                scoreManager.team1ScoreText.rectTransform;
                
            if (scoreTextRect != null)
            {
                // Posicionar nuestro panel del temporizador junto al texto de puntuación
                timerPanel.position = scoreTextRect.position;
                
                // Aplicar offset (desplazamiento) para que no esté justo encima
                Vector3 newPos = timerPanel.position;
                newPos.x += offsetFromScore.x; // Mover hacia la derecha del marcador
                newPos.y += offsetFromScore.y; // Ajustar verticalmeente si es necesario
                timerPanel.position = newPos;
                
                Debug.Log("Temporizador posicionado junto al ScoreManager");
            }
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse de eventos para evitar referencias nulas
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerUpdated -= UpdateTimerDisplay;
            GameManager.Instance.OnMatchStart -= OnMatchStart;
            GameManager.Instance.OnHalfTime -= OnHalfTime;
            GameManager.Instance.OnMatchEnd -= OnMatchEnd;
            GameManager.Instance.OnGoalScored -= OnGoalScored;
        }
        
        // Detener corutinas
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
    }
    
    // Método sobrecargado para manejar tanto enteros como strings
    private void UpdateTimerDisplay(int remainingSeconds)
    {
        if (timerText != null)
        {
            // Convertir a formato MM:SS
            string timeString = GameManager.Instance.GetFormattedTime();
            UpdateTimerDisplay(timeString);
            
            // Cambiar color según el tiempo restante
            if (remainingSeconds <= dangerThreshold)
            {
                timerText.color = dangerColor;
                StartPulsing(); // Animar cuando quede poco tiempo
            }
            else if (remainingSeconds <= warningThreshold)
            {
                timerText.color = warningColor;
                StopPulsing();
            }
            else
            {
                timerText.color = normalColor;
                StopPulsing();
            }
        }
    }
    
    private void UpdateTimerDisplay(string timeString)
    {
        if (timerText != null)
            timerText.text = timeString;
    }
    
    private void OnMatchStart()
    {
        // Ocultar paneles y resetear display
        if (halfTimePanel != null)
            halfTimePanel.SetActive(false);
        
        if (endMatchPanel != null)
            endMatchPanel.SetActive(false);
        
        if (timerText != null)
        {
            timerText.color = normalColor;
            UpdateTimerDisplay(GameManager.Instance.GetFormattedTime());
        }
        
        if (scoreText != null)
            scoreText.text = "0 - 0";
            
        StopPulsing();
    }
    
    private void OnHalfTime()
    {
        // Mostrar panel de medio tiempo
        if (halfTimePanel != null)
            halfTimePanel.SetActive(true);
            
        // Se podría añadir una corrutina para ocultar automáticamente después de un tiempo
    }
    
    private void OnMatchEnd()
    {
        // Mostrar panel de fin de partido
        if (endMatchPanel != null)
        {
            endMatchPanel.SetActive(true);
            
            // Mostrar quién ganó
            if (winnerText != null)
            {
                int team1Score = GameManager.Instance.Team1Score;
                int team2Score = GameManager.Instance.Team2Score;
                
                if (team1Score > team2Score)
                    winnerText.text = "¡EQUIPO 1 GANA!";
                else if (team2Score > team1Score)
                    winnerText.text = "¡EQUIPO 2 GANA!";
                else
                    winnerText.text = "¡EMPATE!";
            }
        }
        
        StopPulsing();
    }
    
    private void OnGoalScored(int teamID)
    {
        // Actualizar el marcador
        if (scoreText != null)
            scoreText.text = GameManager.Instance.GetScoreText();
        
        // Aquí se podría añadir una animación de celebración para el equipo que marcó
    }
    
    // Botones de UI
    public void StartMatch()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartMatch();
    }
    
    public void PauseMatch()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PauseMatch();
    }
    
    public void ResumeMatch()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeMatch();
    }
    
    public void CloseHalfTimePanel()
    {
        if (halfTimePanel != null)
            halfTimePanel.SetActive(false);
        
        // Reanudar el partido si está pausado
        if (GameManager.Instance != null && !GameManager.Instance.IsMatchRunning())
            GameManager.Instance.ResumeMatch();
    }
    
    // Efectos visuales
    private void StartPulsing()
    {
        if (!isPulsing && timerPanel != null)
        {
            isPulsing = true;
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);
                
            pulseCoroutine = StartCoroutine(PulseAnimation());
        }
    }
    
    private void StopPulsing()
    {
        if (isPulsing)
        {
            isPulsing = false;
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);
                
            // Resetear escala
            if (timerPanel != null)
                timerPanel.localScale = originalScale;
        }
    }
    
    private System.Collections.IEnumerator PulseAnimation()
    {
        float pulseMagnitude = 0.2f; // 20% más grande en el pulso
        float pulseSpeed = 2.0f;     // Velocidad de la pulsación
        
        while (isPulsing)
        {
            // Ciclo completo de pulsación
            for (float t = 0; t <= 1; t += Time.deltaTime * pulseSpeed)
            {
                if (!isPulsing) break;
                
                float scale = 1.0f + (Mathf.Sin(t * Mathf.PI) * pulseMagnitude);
                timerPanel.localScale = originalScale * scale;
                
                yield return null;
            }
        }
        
        // Asegurar que vuelve al tamaño original
        timerPanel.localScale = originalScale;
    }
}
