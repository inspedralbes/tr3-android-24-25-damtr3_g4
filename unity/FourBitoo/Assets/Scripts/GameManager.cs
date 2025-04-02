using System;
using System.Collections;
using System.Collections.Generic;
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
    public event Action<int> OnTeamTurnChanged; // Evento para cuando cambia el turno (parámetro: teamID)
    public event Action<int, int> OnScoreChanged; // Evento para cuando cambia la puntuación
    
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
    
    // Sistema de turnos por equipo
    public int CurrentTeamTurn { get; private set; } = 1; // Equipo que tiene el turno actual (1 o 2)
    private List<int> team1PendingPlayers = new List<int>(); // IDs de jugadores del equipo 1 que aún no han marcado su destino
    private List<int> team2PendingPlayers = new List<int>(); // IDs de jugadores del equipo 2 que aún no han marcado su destino
    private List<GameObject> team1MovingPlayers = new List<GameObject>(); // Jugadores del equipo 1 que están en movimiento
    private List<GameObject> team2MovingPlayers = new List<GameObject>(); // Jugadores del equipo 2 que están en movimiento
    private List<GameObject> team1Players = new List<GameObject>(); // Lista ordenada de jugadores del equipo 1 (Player1-Player4)
    private List<GameObject> team2Players = new List<GameObject>(); // Lista ordenada de jugadores del equipo 2 (Player5-Player8)
    private int currentPlayerIndexTeam1 = 0; // Índice del jugador actual en el equipo 1
    private int currentPlayerIndexTeam2 = 0; // Índice del jugador actual en el equipo 2
    private bool isTeam1Ready = false; // Indica si el equipo 1 ha marcado todos sus destinos
    private bool isTeam2Ready = false; // Indica si el equipo 2 ha marcado todos sus destinos
    private bool allPlayersMoving = false; // Indica si todos los jugadores están en movimiento
    
    // Variables para almacenar los jugadores ordenados por tag
    private List<GameObject> orderedPlayers = new List<GameObject>();
    private int currentPlayerIndex = -1;

    // Variable para rastrear si se acaba de marcar un gol
    private bool goalJustScored = false;

    // Diccionario para guardar las posiciones iniciales de cada jugador
    private Dictionary<int, Vector3> initialPlayerPositions;

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
        
        // Inicializar el sistema de turnos
        CurrentTeamTurn = 1; // Empieza el equipo 1
        RegisterPlayersByTeamID();
        isTeam1Ready = false;
        isTeam2Ready = false;
        allPlayersMoving = false;
        
        // Seleccionar automáticamente al primer jugador del equipo 1
        SelectNextPlayer();
        
        if (debugMode)
            Debug.Log("GameManager: ¡Partido iniciado! Turno del Equipo 1");
            
        // Notificar a otros componentes
        OnMatchStart?.Invoke();
        
        // Iniciar el temporizador
        StartCoroutine(CountdownTimer());
        OnTeamTurnChanged?.Invoke(CurrentTeamTurn);
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
    
    // Método para manejar cuando se marca un gol
    public void GoalScored(int scoringTeamID)
    {
        goalJustScored = true;
        
        // Incrementar la puntuación
        if (scoringTeamID == 1)
        {
            Team1Score++;
            // Dar el turno al equipo que recibió el gol
            CurrentTeamTurn = 2;
        }
        else if (scoringTeamID == 2)
        {
            Team2Score++;
            // Dar el turno al equipo que recibió el gol
            CurrentTeamTurn = 1;
        }
        
        // Notificar del cambio de turno
        OnTeamTurnChanged?.Invoke(CurrentTeamTurn);
        OnGoalScored?.Invoke(scoringTeamID); // Notificar que se ha marcado gol
        
        // Notificar del cambio de puntuación
        OnScoreChanged?.Invoke(Team1Score, Team2Score);
        
        Debug.Log($"GOL DEL EQUIPO {scoringTeamID}! Puntuación: {Team1Score} - {Team2Score}. El siguiente turno es para el Equipo {CurrentTeamTurn}");
        
        // Comprobar si el partido ha terminado
        if (Team1Score >= goalsToWin || Team2Score >= goalsToWin)
        {
            EndMatch();
            return;
        }
        
        // Devolver a todos los jugadores a sus posiciones iniciales
        ResetAllPlayersPositions();
        
        // Limpiar cualquier selección o marcadores
        CleanupAllVisualElements();
        
        // Detener cualquier movimiento en progreso
        team1MovingPlayers.Clear();
        team2MovingPlayers.Clear();
        allPlayersMoving = false;
        
        // Reiniciar el sistema de turnos
        isTeam1Ready = false;
        isTeam2Ready = false;
        
        // Reiniciar el índice al primer jugador
        currentPlayerIndex = -1;
        
        // Buscar y reiniciar la posición del balón, si existe
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            // Si hay un balón, reiniciarlo a su posición central
            ball.transform.position = Vector3.zero;
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.linearVelocity = Vector2.zero;
                ballRb.angularVelocity = 0f;
            }
            Debug.Log("Balón reiniciado a posición central");
        }
        
        // Registrar jugadores para el nuevo ciclo
        RegisterPlayersByTeamID();
        
        // Iniciar un nuevo turno con el equipo que no marcó gol
        SelectNextPlayer();
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
    
    private void ResetAllPlayersPositions()
    {
        // Guardar las posiciones iniciales si no se ha hecho antes
        if (initialPlayerPositions == null)
        {
            initialPlayerPositions = new Dictionary<int, Vector3>();
            
            // Registrar la posición inicial de todos los jugadores del equipo 1
            foreach (GameObject player in team1Players)
            {
                initialPlayerPositions[player.GetInstanceID()] = player.transform.position;
            }
            
            // Registrar la posición inicial de todos los jugadores del equipo 2
            foreach (GameObject player in team2Players)
            {
                initialPlayerPositions[player.GetInstanceID()] = player.transform.position;
            }
            
            Debug.Log($"Posiciones iniciales guardadas para {initialPlayerPositions.Count} jugadores");
        }
        
        // Resetear la posición de todos los jugadores del equipo 1
        foreach (GameObject player in team1Players)
        {
            if (initialPlayerPositions.TryGetValue(player.GetInstanceID(), out Vector3 initialPos))
            {
                player.transform.position = initialPos;
                
                // Reiniciar también el controlador
                ControlPorRaton controller = player.GetComponent<ControlPorRaton>();
                if (controller != null)
                {
                    controller.ResetPlayerState();
                }
                
                Debug.Log($"Posición de {player.name} reiniciada a {initialPos}");
            }
        }
        
        // Resetear la posición de todos los jugadores del equipo 2
        foreach (GameObject player in team2Players)
        {
            if (initialPlayerPositions.TryGetValue(player.GetInstanceID(), out Vector3 initialPos))
            {
                player.transform.position = initialPos;
                
                // Reiniciar también el controlador
                ControlPorRaton controller = player.GetComponent<ControlPorRaton>();
                if (controller != null)
                {
                    controller.ResetPlayerState();
                }
                
                Debug.Log($"Posición de {player.name} reiniciada a {initialPos}");
            }
        }
        
        Debug.Log("Todos los jugadores han vuelto a sus posiciones iniciales");
    }
    
    private IEnumerator CountdownTimer()
    {
        while (isMatchRunning && remainingSeconds > 0)
        {
            yield return new WaitForSeconds(1f);
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
    
    // Método alternativo para registrar jugadores sin depender de tags
    public void RegisterPlayersByTeamID()
    {
        // Limpiar las listas existentes
        team1Players.Clear();
        team2Players.Clear();
        orderedPlayers.Clear();
        ResetPendingPlayers();
        
        Debug.Log("=== REGISTRANDO JUGADORES POR TEAM ID ====");
        
        // Encuentra todos los GameObjects con el componente ControlPorRaton
        ControlPorRaton[] allControllers = GameObject.FindObjectsOfType<ControlPorRaton>();
        Debug.Log($"Encontrados {allControllers.Length} jugadores con componente ControlPorRaton");
        
        // Listas temporales para clasificar jugadores por equipo
        List<GameObject> team1PlayersTmp = new List<GameObject>();
        List<GameObject> team2PlayersTmp = new List<GameObject>();
        
        // Clasificar jugadores por team ID
        foreach (ControlPorRaton controller in allControllers)
        {
            GameObject player = controller.gameObject;
            int teamID = controller.GetTeamID();
            
            Debug.Log($"Encontrado jugador: {player.name}, Tag: {player.tag}, TeamID: {teamID}");
            
            if (teamID == 1)
            {
                team1PlayersTmp.Add(player);
                team1Players.Add(player);
            }
            else if (teamID == 2)
            {
                team2PlayersTmp.Add(player);
                team2Players.Add(player);
            }
        }
        
        // Ordenar jugadores por nombre (que debería contener el número)
        team1PlayersTmp.Sort((a, b) => string.Compare(a.name, b.name));
        team2PlayersTmp.Sort((a, b) => string.Compare(a.name, b.name));
        
        // Agregar primero todos los jugadores del equipo 1 en orden
        foreach (GameObject player in team1PlayersTmp)
        {
            orderedPlayers.Add(player);
            Debug.Log($"Ordenado Equipo 1: {player.name}, Tag: {player.tag}");
        }
        
        // Luego agregar todos los jugadores del equipo 2 en orden
        foreach (GameObject player in team2PlayersTmp)
        {
            orderedPlayers.Add(player);
            Debug.Log($"Ordenado Equipo 2: {player.name}, Tag: {player.tag}");
        }
        
        Debug.Log("=== ORDEN FINAL DE JUGADORES (POR TEAM ID) ===");
        for (int i = 0; i < orderedPlayers.Count; i++)
        {
            Debug.Log($"{i+1}. {orderedPlayers[i].name} (Tag: {orderedPlayers[i].tag}, TeamID: {orderedPlayers[i].GetComponent<ControlPorRaton>().GetTeamID()})");
        }
        
        // Reiniciar el índice del jugador actual
        currentPlayerIndex = -1; // Lo configuramos a -1 porque SelectNextPlayer() incrementará a 0
    }
    
    // Seleccionar el siguiente jugador según el orden estricto
    public void SelectNextPlayer()
    {
        // Primero mostrar el estado actual para depuración
        if (debugMode)
        {
            Debug.Log($"ANTES de SelectNextPlayer: currentPlayerIndex = {currentPlayerIndex}");
            Debug.Log($"Jugadores ordenados: {orderedPlayers.Count}");
            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                Debug.Log($"JugadorOrdenado[{i}]: {orderedPlayers[i].name} (Tag: {orderedPlayers[i].tag})");
            }
        }
        
        // Deseleccionar el jugador actual si existe
        if (currentPlayerIndex < orderedPlayers.Count && currentPlayerIndex >= 0)
        {
            GameObject currentPlayer = orderedPlayers[currentPlayerIndex];
            ControlPorRaton controller = currentPlayer.GetComponent<ControlPorRaton>();
            if (controller != null)
            {
                // Deseleccionar primero antes de pasar al siguiente
                Debug.Log($"DESELECCIONANDO explícitamente al jugador: {currentPlayer.name} (Tag: {currentPlayer.tag})");
                controller.Deseleccionar();
            }
            else
            {
                Debug.LogError($"Jugador {currentPlayer.name} no tiene componente ControlPorRaton!");
            }
        }
        
        // Avanzar al siguiente jugador
        currentPlayerIndex++;
        
        // Verificar si hemos terminado todos los jugadores
        if (currentPlayerIndex >= orderedPlayers.Count)
        {
            // Todos los jugadores han marcado su destino, iniciar movimiento
            Debug.Log("¡Todos los jugadores han elegido destino! Iniciando movimiento.");
            
            StartAllPlayersMovement();
            return;
        }
        
        // Obtener el jugador actual según el índice
        GameObject nextPlayer = orderedPlayers[currentPlayerIndex];
        ControlPorRaton nextController = nextPlayer.GetComponent<ControlPorRaton>();
        
        if (nextController != null)
        {
            // Determinar a qué equipo pertenece para actualizar el turno
            int teamID = nextController.GetTeamID();
            
            // Si cambiamos de equipo, actualizar el turno
            if (CurrentTeamTurn != teamID)
            {
                CurrentTeamTurn = teamID;
                OnTeamTurnChanged?.Invoke(CurrentTeamTurn);
                
                Debug.Log($"Cambio de equipo: ahora es turno del Equipo {teamID}");
            }
            
            // Realizar la selección con un pequeño retraso para asegurar que se deseleccione el anterior
            StartCoroutine(SelectPlayerWithDelay(nextController, 0.1f));
            
            Debug.Log($"ORDEN DE SELECCIONAR JUGADOR: {nextPlayer.name} (Tag: {nextPlayer.tag}, índice: {currentPlayerIndex})");
        }
        else
        {
            Debug.LogError($"Error: El siguiente jugador {nextPlayer.name} no tiene componente ControlPorRaton");
        }
    }
    
    // Corrutina para seleccionar un jugador con un pequeño retraso
    private IEnumerator SelectPlayerWithDelay(ControlPorRaton controller, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Asegurarse de que se limpia cualquier elemento visual residual
        CleanupAllVisualElements();
        
        // Ahora seleccionar al jugador
        Debug.Log($"SELECCIONANDO con delay al jugador: {controller.gameObject.name} (Tag: {controller.gameObject.tag})");
        controller.Seleccionar();
    }
    
    // Verificar si un jugador puede moverse (si es su turno y no ha marcado destino)
    public bool CanPlayerMove(GameObject player)
    {
        // Verificar si este es el jugador actual según el orden estricto por tag
        if (currentPlayerIndex < 0 || currentPlayerIndex >= orderedPlayers.Count)
            return false;
            
        // El único jugador que puede moverse es el jugador actual en la lista ordenada
        return (orderedPlayers[currentPlayerIndex] == player);
    }
    
    // Verificar si es el turno de un equipo específico
    public bool IsTeamTurn(int teamID)
    {
        return CurrentTeamTurn == teamID;
    }
    
    // Marcar que un jugador ha establecido su movimiento
    public void PlayerSetDestination(GameObject player, bool moveImmediately = false)
    {
        ControlPorRaton controller = player.GetComponent<ControlPorRaton>();
        if (controller == null) return;
        
        int teamID = controller.GetTeamID();
        int playerID = player.GetInstanceID();
        
        // Añadir a la lista de jugadores que se moverán
        if (teamID == 1)
        {
            team1PendingPlayers.Remove(playerID);
            if (!team1MovingPlayers.Contains(player))
            {
                team1MovingPlayers.Add(player);
            }
        }
        else if (teamID == 2)
        {
            team2PendingPlayers.Remove(playerID);
            if (!team2MovingPlayers.Contains(player))
            {
                team2MovingPlayers.Add(player);
            }
        }
        
        if (debugMode)
            Debug.Log($"Jugador {player.name} (Tag: {player.tag}) ha marcado su destino.");
        
        // Seleccionar al siguiente jugador en el orden estricto
        SelectNextPlayer();
        
        // Si se solicita mover inmediatamente (como en el caso de un clic en el balón)
        if (moveImmediately)
        {
            StartAllPlayersMovement();
        }
    }
    
    // Iniciar el movimiento de todos los jugadores
    public void StartAllPlayersMovement()
    {
        if (allPlayersMoving) return; // Evitar iniciar el movimiento más de una vez
        
        allPlayersMoving = true;
        
        Debug.Log("GameManager: Iniciando movimiento de TODOS los jugadores");
        
        // Mover primero los jugadores del equipo 1
        if (team1MovingPlayers.Count > 0)
        {
            foreach (GameObject player in team1MovingPlayers)
            {
                ControlPorRaton controller = player.GetComponent<ControlPorRaton>();
                if (controller != null)
                {
                    controller.InitiateMovement();
                    Debug.Log($"GameManager: Iniciando movimiento del jugador {player.name} del Equipo 1");
                }
            }
        }
        
        // Luego mover los jugadores del equipo 2
        if (team2MovingPlayers.Count > 0)
        {
            foreach (GameObject player in team2MovingPlayers)
            {
                ControlPorRaton controller = player.GetComponent<ControlPorRaton>();
                if (controller != null)
                {
                    controller.InitiateMovement();
                    Debug.Log($"GameManager: Iniciando movimiento del jugador {player.name} del Equipo 2");
                }
            }
        }
        
        Debug.Log($"Se ha iniciado el movimiento de {team1MovingPlayers.Count + team2MovingPlayers.Count} jugadores");
        
        // Iniciar una corrutina para verificar cuando todos los jugadores han terminado de moverse
        StartCoroutine(CheckMovementComplete());
    }
    
    // Notificación de que un jugador ha terminado de moverse
    public void PlayerFinishedMoving(GameObject player)
    {
        // En caso de que el jugador ya haya sido removido, evitar errores
        if (player == null) return;
        
        // Quitar el jugador de la lista correspondiente
        if (team1MovingPlayers.Contains(player))
        {
            team1MovingPlayers.Remove(player);
            Debug.Log($"Jugador {player.name} del Equipo 1 ha terminado de moverse. Quedan {team1MovingPlayers.Count} jugadores moviéndose en Team1.");
        }
        else if (team2MovingPlayers.Contains(player))
        {
            team2MovingPlayers.Remove(player);
            Debug.Log($"Jugador {player.name} del Equipo 2 ha terminado de moverse. Quedan {team2MovingPlayers.Count} jugadores moviéndose en Team2.");
        }
        else
        {
            // Este jugador no estaba en ninguna lista de movimiento... algo raro pasó
            Debug.LogWarning($"Jugador {player.name} ha terminado de moverse pero no estaba en ninguna lista de movimiento.");
        }
        
        // Si todas las listas están vacías, todos los jugadores han terminado
        if ((team1MovingPlayers.Count == 0 && team2MovingPlayers.Count == 0) && allPlayersMoving)
        {
            Debug.Log("¡TODOS LOS JUGADORES HAN TERMINADO DE MOVERSE! Iniciando el próximo ciclo...");
            
            // Importante: Para asegurar que se reinicie el ciclo, lo iniciamos aquí directamente
            StartCoroutine(ForceStartNewCycle());
            
            // Marcar que ya no están en movimiento colectivo
            allPlayersMoving = false;
        }
    }
    
    // Corrutina para verificar cuando todos los jugadores han terminado de moverse
    private IEnumerator CheckMovementComplete()
    {
        Debug.Log($"Verificando si jugadores han terminado. Team1: {team1MovingPlayers.Count}, Team2: {team2MovingPlayers.Count}");
        
        // Mostrar qué jugadores están aún en movimiento
        foreach (GameObject player in team1MovingPlayers)
        {
            Debug.Log($"Jugador aún moviéndose (Team1): {player.name} (Tag: {player.tag})");
        }
        
        foreach (GameObject player in team2MovingPlayers)
        {
            Debug.Log($"Jugador aún moviéndose (Team2): {player.name} (Tag: {player.tag})");
        }
        
        // Esperar un breve momento para dejar que los jugadores inicien su movimiento
        yield return new WaitForSeconds(0.5f);
        
        // Variable para el timeout - si pasan más de 5 segundos, forzamos el fin del movimiento
        float timeout = 5.0f;
        float timer = 0.0f;
        
        // Esperar hasta que todos los jugadores hayan terminado de moverse o se agote el timeout
        while ((team1MovingPlayers.Count > 0 || team2MovingPlayers.Count > 0) && timer < timeout)
        {
            // Incrementar el timer
            timer += Time.deltaTime;
            
            // Cada segundo mostrar los jugadores que aún se están moviendo
            if (Time.frameCount % 60 == 0) // Aproximadamente cada segundo
            {
                Debug.Log($"Esperando que {team1MovingPlayers.Count + team2MovingPlayers.Count} jugadores terminen... Timeout en {timeout - timer:F1} segundos");
            }
            yield return null;
        }
        
        // Si se agotó el tiempo, forzar la finalización
        if (timer >= timeout)
        {
            Debug.LogWarning("TIMEOUT: Forzando la finalización del movimiento porque algunos jugadores no notificaron correctamente");
            
            // Limpiar las listas de jugadores en movimiento
            if (team1MovingPlayers.Count > 0)
            {
                Debug.LogWarning($"Limpiando {team1MovingPlayers.Count} jugadores atascados del Equipo 1");
                team1MovingPlayers.Clear();
            }
            
            if (team2MovingPlayers.Count > 0) 
            {
                Debug.LogWarning($"Limpiando {team2MovingPlayers.Count} jugadores atascados del Equipo 2");
                team2MovingPlayers.Clear();
            }
        }
        
        Debug.Log("GameManager: Todos los jugadores han completado su movimiento. Iniciando nuevo ciclo.");
        
        // Reiniciar el sistema de turnos
        allPlayersMoving = false;
        isTeam1Ready = false;
        isTeam2Ready = false;
        
        // Limpiar cualquier selección o marcadores que puedan haber quedado
        CleanupAllVisualElements();
        
        // Reiniciar el índice al primer jugador
        currentPlayerIndex = -1;  // -1 porque SelectNextPlayer() incrementará a 0
        
        // Si no ha habido gol, dar el turno al equipo 1 para la siguiente ronda
        // (en caso de gol, GoalScored() ya habrá cambiado el turno)
        if (!goalJustScored)
        {
            CurrentTeamTurn = 1;
            OnTeamTurnChanged?.Invoke(CurrentTeamTurn);
            Debug.Log("GameManager: Iniciando nueva ronda de turnos. Turno del Equipo 1");
        }
        else
        {
            Debug.Log($"GameManager: Después del gol, el turno es del Equipo {CurrentTeamTurn}");
            goalJustScored = false; // Reiniciar la bandera
        }
        
        // Registrar nuevamente los jugadores para el siguiente turno
        RegisterPlayersByTeamID();
        
        // IMPORTANTE: Iniciar el siguiente ciclo seleccionando al siguiente jugador
        SelectNextPlayer();
        Debug.Log("NUEVO CICLO INICIADO! Siguiente jugador seleccionado.");
    }
    
    // Corrutina para forzar el inicio de un nuevo ciclo
    private IEnumerator ForceStartNewCycle()
    {
        // Esperar un momento para asegurar que todas las notificaciones se procesen
        yield return new WaitForSeconds(1.0f);
        
        Debug.Log("FORZANDO INICIO DE NUEVO CICLO...");
        
        // Limpiar todos los elementos visuales residuales
        CleanupAllVisualElements();
        
        // Reiniciar variables de estado
        isTeam1Ready = false;
        isTeam2Ready = false;
        allPlayersMoving = false;
        
        // Reiniciar el índice al primer jugador
        currentPlayerIndex = -1; // -1 porque SelectNextPlayer() incrementará a 0
        
        // Por defecto, el turno comienza con el equipo 1 a menos que haya habido un gol
        if (!goalJustScored)
        {
            CurrentTeamTurn = 1;
            OnTeamTurnChanged?.Invoke(CurrentTeamTurn);
        }
        
        // Registrar jugadores para el nuevo ciclo
        RegisterPlayersByTeamID();
        
        // Iniciar el siguiente ciclo seleccionando al siguiente jugador
        SelectNextPlayer();
        Debug.Log("NUEVO CICLO INICIADO FORZOSAMENTE! Siguiente jugador seleccionado.");
    }
    
    // Método para limpiar elementos visuales de todos los jugadores
    private void CleanupAllVisualElements()
    {
        // Limpia todos los marcadores de selección y flechas
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
        {
            ControlPorRaton controller = player.GetComponent<ControlPorRaton>();
            if (controller != null)
            {
                controller.CleanupVisualElements();
            }
        }
    }
    
    // Cambiar al turno del otro equipo (usado principalmente cuando hay un gol)
    private void SwitchTeamTurn()
    {
        // Limpiar cualquier marcador o flecha residual
        CleanupAllVisualElements();
        
        // Restablecer las listas y banderas para el nuevo turno
        ResetPendingPlayers();
        
        // Cambiar al equipo contrario
        CurrentTeamTurn = (CurrentTeamTurn == 1) ? 2 : 1;
        
        // Reiniciar los índices de jugador
        currentPlayerIndex = 0;
        
        // Notificar el cambio de turno
        OnTeamTurnChanged?.Invoke(CurrentTeamTurn);
        
        // Seleccionar automáticamente al primer jugador del equipo con el turno
        SelectNextPlayer();
        
        if (debugMode)
            Debug.Log($"GameManager: Cambiando al turno del Equipo {CurrentTeamTurn}");
    }
    
    // Resetear la lista de jugadores pendientes
    public void ResetPendingPlayers()
    {
        team1PendingPlayers.Clear();
        team2PendingPlayers.Clear();
        team1MovingPlayers.Clear();
        team2MovingPlayers.Clear();
        isTeam1Ready = false;
        isTeam2Ready = false;
        allPlayersMoving = false;
        
        // Registrar nuevamente todos los jugadores para mantener las listas actualizadas
        foreach (GameObject player in team1Players)
        {
            ControlPorRaton controller = player.GetComponent<ControlPorRaton>();
            if (controller != null)
            {
                int playerID = player.GetInstanceID();
                team1PendingPlayers.Add(playerID);
                
                // Reiniciar el estado de cada jugador
                controller.ResetPlayerState();
            }
        }
        
        foreach (GameObject player in team2Players)
        {
            ControlPorRaton controller = player.GetComponent<ControlPorRaton>();
            if (controller != null)
            {
                int playerID = player.GetInstanceID();
                team2PendingPlayers.Add(playerID);
                
                // Reiniciar el estado de cada jugador
                controller.ResetPlayerState();
            }
        }
        
        // Reiniciar los índices para el siguiente ciclo
        currentPlayerIndex = 0;
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
