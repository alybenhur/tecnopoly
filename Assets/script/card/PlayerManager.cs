using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Player
{
    public string playerName;
    public int credits;
    public List<int> ownedProperties = new List<int>(); // Índices de las casillas que posee
    public int skipTurnCount = 0;
    public bool mustSkipNextTurn = false;// Número de turnos que debe saltarse
    public TokenMovement tokenMovement; // Referencia al componente de movimiento de la ficha
}

public class PlayerManager : MonoBehaviour
{
    [Header("Jugadores")]
    public List<Player> players = new List<Player>();

    [Header("Referencias")]
    public Transform[] startingPositions; // Posiciones iniciales para las fichas
    public GameObject playerTokenPrefab; // Prefab de la ficha del jugador

    // Diccionario para mapear casillas a sus propietarios
    private Dictionary<int, int> propertyOwners = new Dictionary<int, int>();

    void Start()
    {
        // Inicializar jugadores si es necesario
        InitializePlayers();
    }

    // Inicializar las fichas de los jugadores
    private void InitializePlayers()
    {
        // Si no hay jugadores configurados, crear algunos por defecto
        if (players.Count == 0)
        {
            // Crear 4 jugadores por defecto
            for (int i = 0; i < 4; i++)
            {
                Player player = new Player
                {
                    playerName = $"Jugador {i + 1}",
                    credits = 1000 // Créditos iniciales
                };

                players.Add(player);
            }
        }

        // Crear las fichas para cada jugador
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].tokenMovement == null && playerTokenPrefab != null)
            {
                // Determinar posición inicial
                Vector3 position = (i < startingPositions.Length) ?
                                    startingPositions[i].position :
                                    new Vector3(0, 0.5f, 0);

                // Instanciar la ficha
                GameObject token = Instantiate(playerTokenPrefab, position, Quaternion.identity);
                token.name = $"Token_Player{i + 1}";

                // Asignar el componente de movimiento
                players[i].tokenMovement = token.GetComponent<TokenMovement>();

                // Si el token ya tiene un componente TokenMovement, configurarlo
                if (players[i].tokenMovement != null)
                {
                    players[i].tokenMovement.SetPosition(0); // Colocar en la casilla inicial
                }
                else
                {
                    Debug.LogError($"El prefab de ficha no tiene el componente TokenMovement");
                }
            }
        }
    }

    // Obtener el número total de jugadores
    public int GetPlayerCount()
    {
        return players.Count;
    }

    // Añadir créditos a un jugador (valores negativos para restar)
    public void AddCredits(int playerIndex, int amount)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            players[playerIndex].credits += amount;
           // Debug.Log($"{players[playerIndex].playerName} {'recibió' if amount > 0 else 'pagó'} {Mathf.Abs(amount)} créditos");
        }
    }

    // Establecer los créditos de un jugador
    public void SetPlayerCredits(int playerIndex, int credits)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            players[playerIndex].credits = credits;
        }
    }

    // Obtener los créditos de un jugador
    public int GetPlayerCredits(int playerIndex)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            return players[playerIndex].credits;
        }
        return 0;
    }

    // Asignar una propiedad a un jugador
    public void AssignPropertyToPlayer(int playerIndex, int tileIndex)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            // Añadir la propiedad a la lista del jugador
            if (!players[playerIndex].ownedProperties.Contains(tileIndex))
            {
                players[playerIndex].ownedProperties.Add(tileIndex);
            }

            // Actualizar el diccionario de propietarios
            propertyOwners[tileIndex] = playerIndex;

            Debug.Log($"{players[playerIndex].playerName} adquirió la propiedad en la casilla {tileIndex}");
        }
    }

    // Obtener el índice del propietario de una casilla
    public int GetPropertyOwner(int tileIndex)
    {
        if (propertyOwners.ContainsKey(tileIndex))
        {
            return propertyOwners[tileIndex];
        }
        return -1; // -1 indica que no tiene propietario
    }

    // Obtener lista de propiedades de un jugador
    public List<int> GetPlayerProperties(int playerIndex)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            return players[playerIndex].ownedProperties;
        }
        return new List<int>();
    }

   /* public void SetSkipTurn(int playerIndex, int turnsToSkip)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            players[playerIndex].skipTurnCount = turnsToSkip;
            Debug.Log($"Se ha configurado que el Jugador {playerIndex + 1} se saltará {turnsToSkip} turnos");
        }
    }*/
    public void SetSkipNextTurn(int playerIndex, bool skip)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            players[playerIndex].mustSkipNextTurn = skip;
            Debug.Log($"Configurado que el Jugador {playerIndex + 1} {(skip ? "se saltará" : "no se saltará")} su próximo turno");
        }
    }

    public bool MustSkipNextTurn(int playerIndex)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            return players[playerIndex].mustSkipNextTurn;
        }
        return false;
    }
    // Configurar que un jugador se salte turnos
    /*public void SetSkipTurn(int playerIndex, bool skip, int turnCount = 1)
    {


        if (IsValidPlayerIndex(playerIndex))
        {
            players[playerIndex].skipTurnCount = skip ? turnCount : 0;

            if (skip)
            {
                Debug.Log($"{players[playerIndex].playerName} se saltará {turnCount} turno(s)");
            }
        }
    }
*/
    // Verificar si un jugador debe saltarse el turno
    public bool ShouldSkipTurn(int playerIndex)
    {
        if (IsValidPlayerIndex(playerIndex))
        {
            bool shouldSkip = players[playerIndex].skipTurnCount > 0;

            if (shouldSkip)
            {
                Debug.Log($"El Jugador {playerIndex + 1} debe saltarse el turno. Contador: {players[playerIndex].skipTurnCount}");
            }

            return shouldSkip;
        }
        return false;

      /*  if (IsValidPlayerIndex(playerIndex))
        {
            return players[playerIndex].skipTurnCount > 0;
        }
        return false;*/
    }

    // Decrementar el contador de turnos a saltar
    public void DecrementSkipTurnCounter(int playerIndex)
    {
        if (IsValidPlayerIndex(playerIndex) && players[playerIndex].skipTurnCount > 0)
        {
            players[playerIndex].skipTurnCount--;
            Debug.Log($"{players[playerIndex].playerName} le quedan {players[playerIndex].skipTurnCount} turno(s) por saltar");
        }
    }

    // Mover al jugador un número determinado de casillas
    public void MovePlayer(int playerIndex, int spaces)
    {
        if (IsValidPlayerIndex(playerIndex) && players[playerIndex].tokenMovement != null)
        {
            Debug.Log($"avanza:{spaces}");
            players[playerIndex].tokenMovement.ForceMove(spaces);
           // players[playerIndex].tokenMovement.MoveToken(spaces);
            
        }
    }

    // Mover al jugador a una casilla específica
    public void MovePlayerToTile(int playerIndex, int tileIndex)
    {
        if (IsValidPlayerIndex(playerIndex) && players[playerIndex].tokenMovement != null)
        {
            // Calcular el número de casillas a avanzar
            int currentPosition = players[playerIndex].tokenMovement.GetCurrentTileIndex();
            int spacesToMove;

            // Si la casilla destino está adelante
            if (tileIndex > currentPosition)
            {
                spacesToMove = tileIndex - currentPosition;
            }
            // Si la casilla destino está atrás (dar la vuelta al tablero)
            else
            {
                int totalTiles = players[playerIndex].tokenMovement.boardTiles.Length;
                spacesToMove = (totalTiles - currentPosition) + tileIndex;
            }

            // Mover al jugador
            MovePlayer(playerIndex, spacesToMove);
        }
    }

    // Quitar una propiedad a un jugador (útil para la casilla del ladrón)
    public int RemoveRandomPropertyFromPlayer(int playerIndex)
    {
        if (IsValidPlayerIndex(playerIndex) && players[playerIndex].ownedProperties.Count > 0)
        {
            // Seleccionar una propiedad aleatoria
            int randomIndex = Random.Range(0, players[playerIndex].ownedProperties.Count);
            int propertyTileIndex = players[playerIndex].ownedProperties[randomIndex];

            // Remover la propiedad
            players[playerIndex].ownedProperties.RemoveAt(randomIndex);
            propertyOwners.Remove(propertyTileIndex);

            Debug.Log($"{players[playerIndex].playerName} perdió la propiedad en la casilla {propertyTileIndex}");

            return propertyTileIndex;
        }

        return -1;
    }

    // Transferir una propiedad entre jugadores
    public void TransferProperty(int fromPlayerIndex, int toPlayerIndex, int tileIndex)
    {
        if (IsValidPlayerIndex(fromPlayerIndex) && IsValidPlayerIndex(toPlayerIndex))
        {
            // Verificar que el jugador de origen posee la propiedad
            if (players[fromPlayerIndex].ownedProperties.Contains(tileIndex))
            {
                // Remover la propiedad del jugador original
                players[fromPlayerIndex].ownedProperties.Remove(tileIndex);

                // Asignar al nuevo jugador
                AssignPropertyToPlayer(toPlayerIndex, tileIndex);

                Debug.Log($"Propiedad en casilla {tileIndex} transferida de {players[fromPlayerIndex].playerName} a {players[toPlayerIndex].playerName}");
            }
        }
    }

    // Verificar si el índice del jugador es válido
    private bool IsValidPlayerIndex(int index)
    {
        return index >= 0 && index < players.Count;
    }
}
