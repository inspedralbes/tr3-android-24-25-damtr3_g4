using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class GrassScreenManager : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
    public Sprite[] playerSprites; // Array de sprites de jugadores disponibles
    public GameObject canvasPrefab; // Prefab del jugador
    public Image escudoImage; // Referencia a la imagen del escudo
    
    [Header("Configuración de Equipos")]
    public int maxPlayersPerTeam = 5;
    public int defaultPlayers = 4; // Número de jugadores por defecto
    public Color team1Color = Color.red;
    public Color team2Color = Color.blue;
    public GameObject flechaPrefab; // Prefab de la flecha para movimiento
    
    [Header("Posiciones de Jugadores")]
    public Transform[] team1Positions; // Posiciones para equipo 1
    public Transform[] team2Positions; // Posiciones para equipo 2
    
    private List<GameObject> players = new List<GameObject>();
    private bool usarUserStore = true; // Cambiar a true para usar los datos de UserStore

    void Start()
    {
        // Detectar si hay posiciones definidas y crearlas si no
        if (team1Positions == null || team1Positions.Length == 0)
        {
            GenerarPosicionesPredeterminadas();
        }
        
        if (usarUserStore)
        {
            // Cargar desde UserStore
            LoadPlayersFromUserStore();
        }
        else
        {
            // Crear jugadores por defecto
            SetupPlayers(defaultPlayers, true); // Equipo 1
            SetupPlayers(defaultPlayers, false); // Equipo 2
        }
    }
    
    // Crea posiciones predeterminadas si no se asignaron en el inspector
    private void GenerarPosicionesPredeterminadas()
    {
        Debug.Log("Generando posiciones predeterminadas para los equipos...");
        
        // Definir las posiciones específicas para TEAM1
        Vector3[] pos1 = new Vector3[]
        {
            new Vector3(-90, 25, 0),
            new Vector3(-15, 65, 0),
            new Vector3(-15, 10, 0),
            new Vector3(73, -6, 0),
            new Vector3(73, 65, 0)
        };

        // Definir las posiciones específicas para TEAM2
        Vector3[] pos2 = new Vector3[]
        {
            new Vector3(341, 25, 0),
            new Vector3(260, 78, 0),
            new Vector3(260, -18, 0),
            new Vector3(180, -6, 0),
            new Vector3(180, 63, 0)
        };
        
        // Crear los Transform para TEAM1
        team1Positions = new Transform[pos1.Length];
        for (int i = 0; i < pos1.Length; i++)
        {
            GameObject pos = new GameObject($"Team1_Pos{i+1}");
            pos.transform.position = pos1[i];
            pos.transform.SetParent(transform);
            team1Positions[i] = pos.transform;
        }
        
        // Crear los Transform para TEAM2
        team2Positions = new Transform[pos2.Length];
        for (int i = 0; i < pos2.Length; i++)
        {
            GameObject pos = new GameObject($"Team2_Pos{i+1}");
            pos.transform.position = pos2[i];
            pos.transform.SetParent(transform);
            team2Positions[i] = pos.transform;
        }
        
        Debug.Log("✅ Posiciones predeterminadas generadas correctamente");
    }
    
    void LoadPlayersFromUserStore()
    {
        Debug.Log("🔄 Cargando jugadores desde UserStore...");
        
        // Intentar cargar el nombre del escudo seleccionado desde UserStore
        string badgeName = UserStore.Instance.GetTeamBadge(true);
        
        if (!string.IsNullOrEmpty(badgeName))
        {
            LoadBadge(badgeName);
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró ningún escudo guardado en UserStore");
        }
        
        // Cargar la lista de jugadores desde UserStore
        List<UserPlayerData> savedPlayers = UserStore.Instance.GetPlayerList(true);
        
        if (savedPlayers != null && savedPlayers.Count > 0)
        {
            Debug.Log($"✅ Se encontraron {savedPlayers.Count} jugadores en UserStore");
            SetupPlayersFromUserStore(savedPlayers, true);
            
            // También crear jugadores para el equipo 2 (equipo contrario)
            // Puedes modificar esto si quieres que el equipo 2 también se cargue de UserStore
            SetupPlayers(savedPlayers.Count, false);
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontraron jugadores guardados en UserStore, creando jugadores predeterminados");
            SetupPlayers(defaultPlayers, true); // Equipo 1
            SetupPlayers(defaultPlayers, false); // Equipo 2
        }
    }
    
    void LoadBadge(string badgeName)
    {
        if (escudoImage == null)
        {
            Debug.LogWarning("⚠️ La referencia a escudoImage no está asignada. El escudo no se mostrará.");
            return;
        }
        
        Sprite badgeSprite = Resources.Load<Sprite>($"Emblems/{badgeName}");
        if (badgeSprite != null)
        {
            escudoImage.sprite = badgeSprite;
            Debug.Log($"✅ Escudo '{badgeName}' cargado correctamente");
        }
        else
        {
            Debug.LogError($"❌ No se pudo cargar el sprite del escudo: {badgeName}");
        }
    }
    
    void SetupPlayersFromUserStore(List<UserPlayerData> savedPlayers, bool isPlayer1)
    {
        Debug.Log($"♻️ Configurando {savedPlayers.Count} jugadores desde UserStore...");
        
        // Elimina jugadores existentes del equipo correspondiente
        List<GameObject> jugadoresAEliminar = new List<GameObject>();
        foreach (GameObject player in players)
        {
            ControlPorRaton control = player.GetComponent<ControlPorRaton>();
            if (control != null && (isPlayer1 ? control.TeamID == 1 : control.TeamID == 2))
            {
                jugadoresAEliminar.Add(player);
            }
        }
        
        foreach (GameObject player in jugadoresAEliminar)
        {
            players.Remove(player);
            Destroy(player);
        }
        
        // Obtener las posiciones del equipo correspondiente
        Transform[] selectedPositions = isPlayer1 ? team1Positions : team2Positions;
        
        // Asegurar que no intentemos crear más jugadores que posiciones disponibles
        int numPlayers = Mathf.Min(savedPlayers.Count, selectedPositions.Length);
        
        for (int i = 0; i < numPlayers; i++)
        {
            // Obtener datos del jugador
            UserPlayerData playerData = savedPlayers[i];
            
            // Obtener la posición predefinida
            Vector3 newPosition = selectedPositions[i].position;
            
            // Instanciar el prefab del jugador en la posición predefinida
            GameObject playerInstance = Instantiate(canvasPrefab, newPosition, Quaternion.identity);
            playerInstance.transform.SetParent(transform, false);
            playerInstance.name = $"Player_{playerData.id}_{(isPlayer1 ? "Team1" : "Team2")}";
            playerInstance.tag = "Player";
            
            // Configurar el CaptionImage dentro del prefab
            Transform captionImageTransform = playerInstance.transform.Find("Canvas/CaptionImage");
            if (captionImageTransform != null)
            {
                Image captionImage = captionImageTransform.GetComponent<Image>();
                if (captionImage != null)
                {
                    // Intentar cargar el sprite del jugador desde su nombre guardado
                    string spritePath = playerData.spriteImage;
                    // Usar el método mejorado para encontrar el sprite
                    Sprite playerSprite = FindSpriteByName(spritePath);

                    if (playerSprite != null)
                    {
                        captionImage.sprite = playerSprite;
                        Debug.Log($"✅ Sprite '{spritePath}' asignado al jugador {i + 1}");
                    }
                    else
                    {
                        Debug.LogError($"❌ No se encontró el sprite '{spritePath}' para el jugador {i + 1}");
                        
                        // Intentar extraer solo el nombre del archivo (eliminar posibles rutas)
                        string fileNameOnly = System.IO.Path.GetFileName(spritePath);
                        playerSprite = FindSpriteByName(fileNameOnly);
                        
                        if (playerSprite != null)
                        {
                            captionImage.sprite = playerSprite;
                            Debug.Log($"✅ Sprite encontrado usando solo el nombre de archivo: '{fileNameOnly}'");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"❌ El CaptionImage del jugador {i + 1} no tiene un componente Image.");
                }
            }
            else
            {
                Debug.LogError($"❌ No se encontró el CaptionImage en el prefab del jugador {i + 1}.");
            }
            
            // Añadir componentes necesarios para el movimiento
            AddPlayerComponents(playerInstance, i, isPlayer1 ? 1 : 2);
            
            // Agregar el jugador a la lista
            players.Add(playerInstance);
        }
        
        Debug.Log($"✅ Se han configurado {numPlayers} jugadores para el {(isPlayer1 ? "Equipo 1" : "Equipo 2")}");
        
        // Seleccionar el primer jugador del equipo 1 automáticamente
        if (players.Count > 0 && isPlayer1)
        {
            ControlPorRaton primerJugador = players[0].GetComponent<ControlPorRaton>();
            if (primerJugador != null)
            {
                primerJugador.Seleccionar(); 
                Debug.Log("✅ Primer jugador seleccionado automáticamente");
            }
        }
    }
    
    // Método para añadir todos los componentes necesarios a un jugador
    private void AddPlayerComponents(GameObject playerInstance, int playerIndex, int teamId)
    {
        // 1. Añadir ControlPorRaton si no lo tiene
        ControlPorRaton controlScript = playerInstance.GetComponent<ControlPorRaton>();
        if (controlScript == null)
        {
            controlScript = playerInstance.AddComponent<ControlPorRaton>();
            Debug.Log($"✅ ControlPorRaton añadido al jugador {playerIndex + 1}");
        }
        
        // Establecer el TeamID
        controlScript.TeamID = teamId;
        
        // 2. Añadir Rigidbody2D si no lo tiene
        Rigidbody2D rb = playerInstance.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = playerInstance.AddComponent<Rigidbody2D>();
            // Configurar el Rigidbody2D
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            Debug.Log($"✅ Rigidbody2D añadido al jugador {playerIndex + 1}");
        }
        
        // 3. Añadir CircleCollider2D si no lo tiene
        CircleCollider2D collider = playerInstance.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = playerInstance.AddComponent<CircleCollider2D>();
            // Configurar el collider
            collider.radius = 20f;
            // Crear material físico para rebotes
            PhysicsMaterial2D material = new PhysicsMaterial2D("BouncyMaterial");
            material.bounciness = 0.8f;
            material.friction = 0.1f;
            collider.sharedMaterial = material;
            Debug.Log($"✅ CircleCollider2D añadido al jugador {playerIndex + 1}");
        }
        
        // 4. Añadir LineRenderer para la trayectoria
        LineRenderer lineRenderer = playerInstance.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = playerInstance.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 2f;
            lineRenderer.endWidth = 2f;
            lineRenderer.positionCount = 0;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.red;
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
            Debug.Log($"✅ LineRenderer añadido al jugador {playerIndex + 1}");
        }
        
        // 5. Configurar el LineRenderer en el script ControlPorRaton
        controlScript.lineRenderer = lineRenderer;
        
        // 6. Configurar el prefab de la flecha
        if (flechaPrefab != null && controlScript.flechaPrefab == null)
        {
            controlScript.flechaPrefab = flechaPrefab;
            Debug.Log($"✅ Flecha asignada al jugador {playerIndex + 1}");
        }
        
        // 7. Añadir SpriteRenderer si no lo tiene (para cambio de color en selección)
        SpriteRenderer spriteRenderer = playerInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = playerInstance.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 5;
            // Hacer este SpriteRenderer invisible para que no interfiera con la imagen
            spriteRenderer.color = new Color(1, 1, 1, 0);
            Debug.Log($"✅ SpriteRenderer añadido al jugador {playerIndex + 1}");
        }
    }

    // Método para encontrar un sprite por nombre, buscando en diferentes carpetas de recursos
    private Sprite FindSpriteByName(string spriteName)
    {
        // Intenta cargar desde la carpeta Characters directamente
        Sprite foundSprite = Resources.Load<Sprite>($"Characters/{spriteName}");
        
        // Si no lo encuentra, prueba sin la carpeta Characters (por si el nombre ya incluye la ruta)
        if (foundSprite == null)
        {
            foundSprite = Resources.Load<Sprite>(spriteName);
        }
        
        // Prueba a cargar desde otras posibles carpetas
        if (foundSprite == null)
        {
            string[] possiblePaths = new string[] {
                $"Players/{spriteName}",
                $"Sprites/{spriteName}",
                $"Sprites/Characters/{spriteName}",
                $"Sprites/Players/{spriteName}"
            };
            
            foreach (string path in possiblePaths)
            {
                foundSprite = Resources.Load<Sprite>(path);
                if (foundSprite != null)
                {
                    Debug.Log($"✅ Sprite encontrado en ruta alternativa: {path}");
                    break;
                }
            }
        }
        
        return foundSprite;
    }

    // Configurar jugadores por defecto
    void SetupPlayers(int numJugadores, bool isPlayer1)
    {
        Debug.Log($"♻️ Configurando {numJugadores} jugadores para el {(isPlayer1 ? "Equipo 1" : "Equipo 2")}...");

        // Elimina jugadores existentes del equipo correspondiente
        List<GameObject> jugadoresAEliminar = new List<GameObject>();
        foreach (GameObject player in players)
        {
            ControlPorRaton control = player.GetComponent<ControlPorRaton>();
            if (control != null && (isPlayer1 ? control.TeamID == 1 : control.TeamID == 2))
            {
                jugadoresAEliminar.Add(player);
            }
        }
        
        foreach (GameObject player in jugadoresAEliminar)
        {
            players.Remove(player);
            Destroy(player);
        }

        // Seleccionar las posiciones según el equipo
        Transform[] selectedPositions = isPlayer1 ? team1Positions : team2Positions;
        
        // Limitar el número de jugadores según las posiciones disponibles
        int numPlayersToCreate = Mathf.Min(numJugadores, selectedPositions.Length);

        for (int i = 0; i < numPlayersToCreate; i++)
        {
            // Verificar si hay suficientes posiciones predefinidas
            if (i >= selectedPositions.Length)
            {
                Debug.LogError($"❌ No hay suficientes posiciones predefinidas para el jugador {i + 1}.");
                break;
            }

            // Obtener la posición predefinida
            Vector3 newPosition = selectedPositions[i].position;

            // Instanciar el prefab del jugador en la posición predefinida
            GameObject playerInstance = Instantiate(canvasPrefab, newPosition, Quaternion.identity);
            playerInstance.transform.SetParent(transform, false);
            playerInstance.name = $"Player_{i}_{(isPlayer1 ? "Team1" : "Team2")}";
            playerInstance.tag = "Player";

            // Configurar el CaptionImage dentro del prefab
            Transform captionImageTransform = playerInstance.transform.Find("Canvas/CaptionImage");
            if (captionImageTransform != null)
            {
                Image captionImage = captionImageTransform.GetComponent<Image>();
                if (captionImage != null)
                {
                    // Asignar un sprite dinámico al CaptionImage
                    Sprite playerSprite = GetPlayerSprite(i);
                    if (playerSprite != null)
                    {
                        captionImage.sprite = playerSprite;
                        Debug.Log($"🎨 Sprite asignado al jugador {i + 1}: {playerSprite.name}");
                    }
                    else
                    {
                        Debug.LogError($"❌ No se encontró un sprite para el jugador {i + 1}.");
                    }
                }
                else
                {
                    Debug.LogError($"❌ El CaptionImage del jugador {i + 1} no tiene un componente Image.");
                }
            }
            else
            {
                Debug.LogError($"❌ No se encontró el CaptionImage en el prefab del jugador {i + 1}.");
            }
            
            // Añadir componentes necesarios para el movimiento
            AddPlayerComponents(playerInstance, i, isPlayer1 ? 1 : 2);

            // Agregar el jugador a la lista
            players.Add(playerInstance);
        }
        
        // Seleccionar el primer jugador del equipo 1 automáticamente
        if (players.Count > 0 && isPlayer1)
        {
            // Buscar un jugador del equipo 1
            ControlPorRaton jugadorSeleccionable = null;
            foreach (GameObject player in players)
            {
                ControlPorRaton control = player.GetComponent<ControlPorRaton>();
                if (control != null && control.TeamID == 1)
                {
                    jugadorSeleccionable = control;
                    break;
                }
            }
            
            if (jugadorSeleccionable != null)
            {
                jugadorSeleccionable.Seleccionar();
                Debug.Log("✅ Primer jugador del equipo 1 seleccionado automáticamente");
            }
        }
    }

    Sprite GetPlayerSprite(int playerIndex)
    {
        if (playerSprites == null || playerSprites.Length == 0)
        {
            Debug.LogError("❌ Los sprites no se cargaron correctamente. Asegúrate de que la carpeta Resources/Player contenga los sprites.");
            return null;
        }

        // Usar módulo para permitir más jugadores que sprites
        int spriteIndex = playerIndex % playerSprites.Length;
        return playerSprites[spriteIndex];
    }
}

[System.Serializable]
public class MatchConfiguration
{
    public int matchDuration;
    public int selectedPlayer;
    public int goalsToWin;
}