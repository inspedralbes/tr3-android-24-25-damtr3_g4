using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance { get; private set; }
    
    // Eventos
    public event Action<int> OnTimerUpdated;
    public event Action OnMatchStart;
    public event Action OnHalfTime;
    public event Action OnMatchEnd;
    public event Action<int> OnGoalScored; // Evento para cuando se marca un gol (parámetro: teamID que marcó)
    
    // Estado del partido
    public int Team1Score { get; private set; } = 0;
    public int Team2Score { get; private set; } = 0;
    
    // Variables de tiempo
    private int remainingSeconds;
    private bool isMatchRunning = false;
    private bool isFirstHalf = true;
    private bool isConfigLoaded = false;
    
    // Configuración del partido
    private int matchDurationSeconds = 300; // 5 minutos por defecto
    private int goalsToWin = 3; // Goles para ganar por defecto
    
    [Header("Depuración")]
    public bool dontDestroyOnLoad = true;
    public bool debugMode = true;
    
    void Start()
    {
        // Inicializar singleton
        if (Instance != null && Instance != this)
        {
            if (debugMode)
                Debug.LogWarning($"GameManager: Ya existe una instancia. Destruyendo duplicado en {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
        
        if (debugMode)
            Debug.Log($"GameManager: Iniciando en {gameObject.name}. DontDestroyOnLoad: {dontDestroyOnLoad}");
        
        // Inicializar valores predeterminados
        remainingSeconds = matchDurationSeconds;
        
        // Esperar a que se cargue la configuración del partido
        StartCoroutine(WaitForMatchConfig());
        
        // Para pruebas, iniciar automáticamente el partido después de 1 segundo
        Invoke("StartMatch", 1f);
    }
    
    private IEnumerator WaitForMatchConfig()
    {
        // Esperar hasta que la configuración del partido esté disponible
        int maxAttempts = 10; // Máximo número de intentos para evitar esperar infinitamente
        int attempts = 0;
        
        while (attempts < maxAttempts)
        {
            // Verificar si MatchConfigManager existe y tiene la configuración cargada
            if (MatchConfigManager.Instance != null && MatchConfigManager.Instance.CurrentMatchConfig != null)
            {
                // Configuración encontrada
                LoadMatchConfig();
                yield break; // Salir del coroutine
            }
            
            attempts++;
            if (debugMode)
                Debug.Log($"GameManager: Esperando MatchConfigManager... Intento {attempts}/{maxAttempts}");
                
            yield return new WaitForSeconds(1f); // Esperar 1 segundo antes de intentar de nuevo
        }
        
        // Si llegamos aquí, no se pudo encontrar la configuración
        if (debugMode)
            Debug.LogWarning("GameManager: No se pudo encontrar el MatchConfigManager después de varios intentos. Usando valores por defecto.");
            
        // Usar valores predeterminados
        remainingSeconds = matchDurationSeconds;
        isConfigLoaded = true;
        OnTimerUpdated?.Invoke(remainingSeconds);
    }
    
    private void LoadMatchConfig()
    {
        if (MatchConfigManager.Instance != null && MatchConfigManager.Instance.CurrentMatchConfig != null)
        {
            // Usar matchDurationSeconds en lugar de matchDuration
            matchDurationSeconds = MatchConfigManager.Instance.CurrentMatchConfig.matchDurationSeconds;
            goalsToWin = MatchConfigManager.Instance.CurrentMatchConfig.goalsToWin;
            
            if (debugMode)
                Debug.Log($"GameManager: Configuración cargada - Duración: {matchDurationSeconds} segundos, Goles para ganar: {goalsToWin}");
            
            // Inicializar el temporizador con el tiempo de partida
            remainingSeconds = matchDurationSeconds;
            isConfigLoaded = true;
        }
        else
        {
            if (debugMode)
                Debug.LogWarning("GameManager: No se pudo cargar la configuración del partido, usando valores por defecto");
            // Usar valores predeterminados
            remainingSeconds = matchDurationSeconds;
            isConfigLoaded = true;
        }
        
        // Informar a la UI del tiempo inicial
        OnTimerUpdated?.Invoke(remainingSeconds);
    }
    
    public void StartMatch()
    {
        if (!isConfigLoaded)
        {
            if (debugMode)
                Debug.LogWarning("GameManager: No se puede iniciar el partido: configuración no cargada");
            return;
        }
        
        isMatchRunning = true;
        isFirstHalf = true;
        remainingSeconds = matchDurationSeconds;
        Team1Score = 0;
        Team2Score = 0;
        
        StartCoroutine(CountdownTimer());
        OnMatchStart?.Invoke();
        
        if (debugMode)
            Debug.Log("GameManager: ¡Partido iniciado!");
    }
    
    public void PauseMatch()
    {
        isMatchRunning = false;
        if (debugMode)
            Debug.Log("GameManager: Partido pausado");
    }
    
    public void ResumeMatch()
    {
        isMatchRunning = true;
        if (debugMode)
            Debug.Log("GameManager: Partido reanudado");
    }
    
    public void ScoreGoal(int teamID)
    {
        // Incrementar la puntuación del equipo correspondiente
        if (teamID == 1)
        {
            Team1Score++;
            if (debugMode)
                Debug.Log($"GameManager: ¡Gol del Equipo 1! Marcador: {Team1Score}-{Team2Score}");
        }
        else if (teamID == 2)
        {
            Team2Score++;
            if (debugMode)
                Debug.Log($"GameManager: ¡Gol del Equipo 2! Marcador: {Team1Score}-{Team2Score}");
        }
        
        // Notificar a otros componentes del gol
        OnGoalScored?.Invoke(teamID);
        
        // Comprobar si el partido ha terminado
        if (Team1Score >= goalsToWin || Team2Score >= goalsToWin)
        {
            EndMatch();
            return;
        }
    }
    
    private void EndMatch()
    {
        isMatchRunning = false;
        OnMatchEnd?.Invoke();
        if (debugMode)
            Debug.Log("GameManager: Partido finalizado");
        
        // Determinar el ganador
        string winnerMessage = Team1Score > Team2Score ? "¡Equipo 1 GANA!" : 
                              Team2Score > Team1Score ? "¡Equipo 2 GANA!" : 
                              "¡EMPATE!";
        if (debugMode)
            Debug.Log($"GameManager: {winnerMessage} Resultado final: {Team1Score}-{Team2Score}");
    }
    
    private IEnumerator CountdownTimer()
    {
        while (isMatchRunning && remainingSeconds > 0)
        {
            yield return new WaitForSeconds(0.1f);
            if (isMatchRunning) // Comprobar de nuevo por si se ha pausado o detenido
            {
                remainingSeconds--;
                OnTimerUpdated?.Invoke(remainingSeconds);
                
                // Comprobar si el tiempo se ha agotado
                if (remainingSeconds <= 0)
                {
                    EndMatch();
                    break;
                }
                
                // Comprobar si es tiempo de descanso (mitad del partido)
                if (isFirstHalf && remainingSeconds == matchDurationSeconds / 2)
                {
                    isFirstHalf = false;
                    OnHalfTime?.Invoke();
                    if (debugMode)
                        Debug.Log("GameManager: ¡Descanso! Mitad del partido");
                }
            }
        }
    }
    
    // Formatear el tiempo para mostrarlo (MM:SS)
    public string GetFormattedTime()
    {
        int minutes = remainingSeconds / 60;
        int seconds = remainingSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }
    
    // Obtener la puntuación actual como texto
    public string GetScoreText()
    {
        return $"{Team1Score} - {Team2Score}";
    }
    
    // Verificar si el partido está en curso
    public bool IsMatchRunning()
    {
        return isMatchRunning;
    }
}
