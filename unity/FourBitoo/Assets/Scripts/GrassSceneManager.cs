using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrassSceneManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image escudoImage; // Referencia a la imagen donde mostrar el escudo
    public Transform[] jugadoresSpawnPoints; // Puntos de spawn para los jugadores en la escena
    public GameObject jugadorPrefab; // Prefab del jugador para instanciar

    [Header("Configuración")]
    public bool usarUsuarioPrincipal = true; // true = usar usuario principal, false = usar invitado

    // Lista para mantener referencias a los jugadores instanciados
    private List<GameObject> jugadoresInstanciados = new List<GameObject>();

    void Start()
    {
        // Cargar los datos guardados
        CargarDatosGuardados();
    }

    private void CargarDatosGuardados()
    {
        // Obtener el escudo seleccionado del UserStore
        string badgeName = UserStore.Instance.GetTeamBadge(usarUsuarioPrincipal);
        if (!string.IsNullOrEmpty(badgeName))
        {
            MostrarEscudo(badgeName);
        }
        else
        {
            Debug.LogWarning("❌ No se encontró ningún escudo guardado en UserStore");
        }

        // Obtener la lista de jugadores del UserStore
        List<UserPlayerData> playerList = UserStore.Instance.GetPlayerList(usarUsuarioPrincipal);
        if (playerList != null && playerList.Count > 0)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                // Solo instanciar si tenemos suficientes spawn points
                if (i < jugadoresSpawnPoints.Length)
                {
                    InstanciarJugador(playerList[i], jugadoresSpawnPoints[i].position);
                }
                else
                {
                    Debug.LogWarning($"⚠️ No hay suficientes puntos de spawn para el jugador {i + 1}");
                    break;
                }
            }
            Debug.Log($"✅ Se han cargado {playerList.Count} jugadores desde UserStore");
        }
        else
        {
            Debug.LogWarning("❌ No se encontraron jugadores guardados en UserStore");
        }
    }

    private void MostrarEscudo(string badgeName)
    {
        if (escudoImage == null)
        {
            Debug.LogError("❌ La referencia a escudoImage no está asignada");
            return;
        }

        // Cargar el sprite del escudo desde Resources
        Sprite badgeSprite = Resources.Load<Sprite>($"Emblems/{badgeName}");
        if (badgeSprite != null)
        {
            // Asignar el sprite a la imagen del escudo
            escudoImage.sprite = badgeSprite;
            Debug.Log($"✅ Escudo '{badgeName}' cargado correctamente");
        }
        else
        {
            Debug.LogError($"❌ No se pudo cargar el sprite del escudo: {badgeName}");
        }
    }

    private void InstanciarJugador(UserPlayerData playerData, Vector3 spawnPosition)
    {
        if (jugadorPrefab == null)
        {
            Debug.LogError("❌ La referencia a jugadorPrefab no está asignada");
            return;
        }

        // Instanciar el prefab del jugador en la posición de spawn
        GameObject jugador = Instantiate(jugadorPrefab, spawnPosition, Quaternion.identity);
        jugador.name = $"Jugador_{playerData.id}";

        // Buscar el componente SpriteRenderer o Image para asignar el sprite
        SpriteRenderer spriteRenderer = jugador.GetComponent<SpriteRenderer>();
        Image spriteImage = jugador.GetComponent<Image>();

        // Cargar el sprite del personaje
        string spritePath = playerData.spriteImage; // Usar directamente el nombre del sprite guardado
        Sprite playerSprite = Resources.Load<Sprite>(spritePath);

        if (playerSprite != null)
        {
            // Asignar el sprite dependiendo del componente que tenga el jugador
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = playerSprite;
            }
            else if (spriteImage != null)
            {
                spriteImage.sprite = playerSprite;
            }
            else
            {
                // Buscar en hijos si no está en el objeto principal
                spriteRenderer = jugador.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = playerSprite;
                }
                else
                {
                    spriteImage = jugador.GetComponentInChildren<Image>();
                    if (spriteImage != null)
                    {
                        spriteImage.sprite = playerSprite;
                    }
                    else
                    {
                        Debug.LogError($"❌ El prefab del jugador no tiene SpriteRenderer ni Image");
                    }
                }
            }
            Debug.Log($"✅ Jugador '{playerData.name}' con sprite '{spritePath}' instanciado correctamente");
        }
        else
        {
            Debug.LogError($"❌ No se pudo cargar el sprite: {spritePath}");
        }

        // Guardar la referencia al jugador instanciado
        jugadoresInstanciados.Add(jugador);
    }

    // Método opcional para limpiar los jugadores instanciados
    public void LimpiarJugadores()
    {
        foreach (GameObject jugador in jugadoresInstanciados)
        {
            Destroy(jugador);
        }
        jugadoresInstanciados.Clear();
    }
}
