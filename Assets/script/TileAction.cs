using UnityEngine;
using System.Collections;

public class TileAction : MonoBehaviour
{
    public enum TileType
    {
        Property,    // Artefactos tecnol�gicos que se pueden comprar
        Chance,      // Casillas de eventos aleatorios (interrogaci�n roja)
        CommunityChest, // Casillas de preguntas
        Tax,         // Casillas de impuestos
        GoToJail,    // Casilla del ladr�n
        Jail,        // Casilla de la c�rcel/no relevante para este juego
        FreeParking, // Casilla del tesoro
        Go           // Casilla de Salida
    }

    [Header("Configuraci�n B�sica")]
    public TileType tileType; // Tipo de casilla
    public int tileIndex;     // �ndice �nico de la casilla en el tablero
    public string tileName;   // Nombre descriptivo de la casilla

    [Header("Configuraci�n de Propiedad")]
    [Tooltip("Costo para comprar este artefacto")]
    public int propertyCost;
    [Tooltip("Tarifa que paga otro jugador al caer aqu� (entre 20-50)")]
    public int rentCost;

    [Header("Configuraci�n de Impuesto")]
    public int taxAmount;

    // Referencia al GameManager
    private GameManager gameManager;

    void Start()
    {
        // Buscar el GameManager en la escena
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("No se encontr� el GameManager en la escena");
        }

        // Si no se ha establecido un nombre, usar uno gen�rico basado en el tipo
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
                return "Pregunta Tecnol�gica";
            case TileType.Tax:
                return "Impuesto Tecnol�gico";
            case TileType.GoToJail:
                return "Ladr�n de Tecnolog�a";
            case TileType.Jail:
                return "Centro de Reparaci�n";
            case TileType.FreeParking:
                return "Tesoro Tecnol�gico";
            case TileType.Go:
                return "Salida";
            default:
                return "Casilla " + tileIndex;
        }
    }

    // Este m�todo es llamado cuando un jugador cae en esta casilla
    public void OnPlayerLanded(int playerIndex)
    {
        if (gameManager != null)
        {
            gameManager.OnPlayerLandedOnTile(tileIndex);
        }
        else
        {
            Debug.LogError("GameManager no encontrado, no se puede procesar la acci�n de la casilla");
        }
    }

    // M�todo opcional para visualizar la casilla en el editor
    private void OnDrawGizmos()
    {
        // Dibujar un cubo para representar la casilla
        Gizmos.color = GetGizmoColor();
        Gizmos.DrawCube(transform.position + Vector3.up * 0.1f, new Vector3(0.9f, 0.1f, 0.9f));

        // Mostrar el �ndice como texto
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, tileIndex.ToString());
#endif
    }

    // Determinar el color del gizmo seg�n el tipo de casilla
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
                return new Color(0.7f, 0, 0.7f, 0.6f); // P�rpura
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