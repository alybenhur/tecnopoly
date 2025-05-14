using UnityEngine;
using System.Collections;

public class TileAction : MonoBehaviour
{
    public enum TileType
    {
        Property,    // Artefactos tecnológicos que se pueden comprar
        Chance,      // Casillas de eventos aleatorios (interrogación roja)
        CommunityChest, // Casillas de preguntas
        Tax,         // Casillas de impuestos
        GoToJail,    // Casilla del ladrón
        Jail,        // Casilla de la cárcel/no relevante para este juego
        FreeParking, // Casilla del tesoro
        Go           // Casilla de Salida
    }

    [Header("Configuración Básica")]
    public TileType tileType; // Tipo de casilla
    public int tileIndex;     // Índice único de la casilla en el tablero
    public string tileName;   // Nombre descriptivo de la casilla

    [Header("Configuración de Propiedad")]
    [Tooltip("Costo para comprar este artefacto")]
    public int propertyCost;
    [Tooltip("Tarifa que paga otro jugador al caer aquí (entre 20-50)")]
    public int rentCost;

    [Header("Configuración de Impuesto")]
    public int taxAmount;

    // Referencia al GameManager
    private GameManager gameManager;

    void Start()
    {
        // Buscar el GameManager en la escena
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("No se encontró el GameManager en la escena");
        }

        // Si no se ha establecido un nombre, usar uno genérico basado en el tipo
        if (string.IsNullOrEmpty(tileName))
        {
            tileName = GetDefaultTileName();
        }
    }

    // Generar un nombre predeterminado basado en el tipo de casilla
    private string GetDefaultTileName()
    {
        switch (tileType)
        {
            case TileType.Property:
                return "Artefacto " + tileIndex;
            case TileType.Chance:
                return "Evento Aleatorio";
            case TileType.CommunityChest:
                return "Pregunta Tecnológica";
            case TileType.Tax:
                return "Impuesto Tecnológico";
            case TileType.GoToJail:
                return "Ladrón de Tecnología";
            case TileType.Jail:
                return "Centro de Reparación";
            case TileType.FreeParking:
                return "Tesoro Tecnológico";
            case TileType.Go:
                return "Salida";
            default:
                return "Casilla " + tileIndex;
        }
    }

    // Este método es llamado cuando un jugador cae en esta casilla
    public void OnPlayerLanded(int playerIndex)
    {
        if (gameManager != null)
        {
            gameManager.OnPlayerLandedOnTile(tileIndex);
        }
        else
        {
            Debug.LogError("GameManager no encontrado, no se puede procesar la acción de la casilla");
        }
    }

    // Método opcional para visualizar la casilla en el editor
    private void OnDrawGizmos()
    {
        // Dibujar un cubo para representar la casilla
        Gizmos.color = GetGizmoColor();
        Gizmos.DrawCube(transform.position + Vector3.up * 0.1f, new Vector3(0.9f, 0.1f, 0.9f));

        // Mostrar el índice como texto
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, tileIndex.ToString());
#endif
    }

    // Determinar el color del gizmo según el tipo de casilla
    private Color GetGizmoColor()
    {
        switch (tileType)
        {
            case TileType.Property:
                return new Color(0, 0.7f, 0, 0.6f); // Verde
            case TileType.Chance:
                return new Color(1, 0, 0, 0.6f); // Rojo
            case TileType.CommunityChest:
                return new Color(0, 0, 1, 0.6f); // Azul
            case TileType.Tax:
                return new Color(0.7f, 0, 0.7f, 0.6f); // Púrpura
            case TileType.GoToJail:
                return new Color(0, 0, 0, 0.6f); // Negro
            case TileType.Jail:
                return new Color(0.5f, 0.5f, 0.5f, 0.6f); // Gris
            case TileType.FreeParking:
                return new Color(1, 0.8f, 0, 0.6f); // Dorado
            case TileType.Go:
                return new Color(1, 0.5f, 0, 0.6f); // Naranja
            default:
                return new Color(0.5f, 0.5f, 0.5f, 0.6f); // Gris
        }
    }
}