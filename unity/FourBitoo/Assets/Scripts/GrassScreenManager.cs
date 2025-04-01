using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class GrassScreenManager : MonoBehaviour
{
    public string URL = "http://localhost:4000"; // URL del backend
    public GameObject canvasPrefab; // Prefab del canvas que contiene el CaptionImage
    public Sprite[] playerSprites; // Array de sprites asignados desde el Inspector
    public Image escudoImage; // Imagen para mostrar el escudo del equipo
    
    private List<GameObject> players = new List<GameObject>();
    private bool usarUserStore = true; // Cambiar a true para usar los datos de UserStore

    void Start()
    {
        if (usarUserStore)
        {
            // Cargar desde UserStore
            LoadPlayersFromUserStore();
        }
        else
        {
            // Cargar desde backend (método original)
            StartCoroutine(LoadMatchConfig());
        }
    }
    
    void LoadPlayersFromUserStore()
    {
        Debug.Log("🔄 Cargando jugadores desde UserStore...");
        
        // Cargar el escudo desde UserStore
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
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontraron jugadores guardados en UserStore, cargando desde backend");
            StartCoroutine(LoadMatchConfig());
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
        
        // Elimina jugadores existentes
        foreach (GameObject player in players)
        {
            Destroy(player);
        }
        players.Clear();

        // Definir las posiciones específicas para PLAYER1 y PLAYER2
        Vector3[] player1Positions = new Vector3[]
        {
            new Vector3(-90, 25, 0),
            new Vector3(-15, 65, 0),
            new Vector3(-15, 10, 0),
            new Vector3(73, -6, 0),
            new Vector3(73, 65, 0)
        };

        Vector3[] player2Positions = new Vector3[]
        {
            new Vector3(341, 25, 0),
            new Vector3(260, 78, 0),
            new Vector3(260, -18, 0),
            new Vector3(180, -6, 0),
            new Vector3(180, 63, 0)
        };

        // Seleccionar las posiciones según el equipo
        Vector3[] selectedPositions = isPlayer1 ? player1Positions : player2Positions;
        
        for (int i = 0; i < savedPlayers.Count; i++)
        {
            // Verificar si hay suficientes posiciones predefinidas
            if (i >= selectedPositions.Length)
            {
                Debug.LogError($"❌ No hay suficientes posiciones predefinidas para el jugador {i + 1}.");
                break;
            }
            
            // Obtener la posición predefinida
            Vector3 newPosition = selectedPositions[i];
            
            // Obtener datos del jugador guardado
            UserPlayerData playerData = savedPlayers[i];
            
            // Instanciar el prefab del jugador en la posición predefinida
            GameObject playerInstance = Instantiate(canvasPrefab, newPosition, Quaternion.identity);
            playerInstance.transform.SetParent(transform, false);
            playerInstance.name = $"Player_{playerData.id}";
            
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
                        else if (i < playerSprites.Length)
                        {
                            // Usar sprite predeterminado si está disponible
                            captionImage.sprite = playerSprites[i];
                            Debug.Log($"⚠️ Usando sprite predeterminado para el jugador {i + 1}");
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
            
            // Agregar el jugador a la lista
            players.Add(playerInstance);
        }
        
        Debug.Log($"✅ Se han configurado {players.Count} jugadores desde UserStore");
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

    IEnumerator LoadMatchConfig()
    {
        Debug.Log("🔄 Consultando configuración del partido desde el backend...");

        UnityWebRequest request = UnityWebRequest.Get($"{URL}/getMatchConfig");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log($"📥 Respuesta del servidor: {jsonResponse}");

            MatchConfiguration config = JsonUtility.FromJson<MatchConfiguration>(jsonResponse);
            Debug.Log($"🎮 Número de jugadores recibidos: {config.selectedPlayer}");

            SetupPlayers(config.selectedPlayer, true); // Configura el equipo PLAYER1
        }
        else
        {
            Debug.LogError("❌ Error al obtener la configuración del partido: " + request.error);
        }
    }

    void SetupPlayers(int selectedPlayer, bool isPlayer1)
    {
        Debug.Log($"♻️ Configurando jugadores... Número de jugadores seleccionados: {selectedPlayer}");

        // Elimina jugadores existentes
        foreach (GameObject player in players)
        {
            Destroy(player);
        }
        players.Clear();

        // Definir las posiciones específicas para PLAYER1 y PLAYER2
        Vector3[] player1Positions = new Vector3[]
        {
            new Vector3(-90, 25, 0),
            new Vector3(-15, 65, 0),
            new Vector3(-15, 10, 0),
            new Vector3(73, -6, 0),
            new Vector3(73, 65, 0)
        };

        Vector3[] player2Positions = new Vector3[]
        {
            new Vector3(341, 25, 0),
            new Vector3(260, 78, 0),
            new Vector3(260, -18, 0),
            new Vector3(180, -6, 0),
            new Vector3(180, 63, 0)
        };

        // Seleccionar las posiciones según el equipo
        Vector3[] selectedPositions = isPlayer1 ? player1Positions : player2Positions;

        for (int i = 0; i < selectedPlayer; i++)
        {
            // Verificar si hay suficientes posiciones predefinidas
            if (i >= selectedPositions.Length)
            {
                Debug.LogError($"❌ No hay suficientes posiciones predefinidas para el jugador {i + 1}.");
                break;
            }

            // Obtener la posición predefinida
            Vector3 newPosition = selectedPositions[i];

            // Instanciar el prefab del jugador en la posición predefinida
            GameObject playerInstance = Instantiate(canvasPrefab, newPosition, Quaternion.identity);
            playerInstance.transform.SetParent(transform, false);

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

            // Agregar el jugador a la lista
            players.Add(playerInstance);
        }
    }

    Sprite GetPlayerSprite(int playerIndex)
    {
        if (playerSprites == null || playerSprites.Length == 0)
        {
            Debug.LogError("❌ Los sprites no se cargaron correctamente. Asegúrate de que la carpeta Resources/Player contenga los sprites.");
            return null;
        }

        if (playerIndex < 0 || playerIndex >= playerSprites.Length)
        {
            Debug.LogError($"❌ Índice de jugador fuera de rango: {playerIndex}. Asegúrate de que los sprites estén correctamente configurados.");
            return null;
        }

        return playerSprites[playerIndex];
    }
}

[System.Serializable]
public class MatchConfiguration
{
    public int matchDuration;
    public int goalsToWin;
    public int selectedPlayer;
}