using UnityEngine;
using System.Collections;

public class DiceSystem : MonoBehaviour
{
    [Header("Referencias")]
    public TokenMovement[] playerTokens; // Referencias a todas las fichas de los jugadores
    public SimpleDiceDisplay diceDisplay; // Referencia al nuevo sistema de visualización de dados
    public GameManager gameManager;
    public PlayerManager playerManager;

    [Header("Configuración de Dados")]
    public int numberOfDice = 2; // Cantidad de dados (tradicionalmente 2)

    [Header("Sistema de Turnos")]
    public int currentPlayerIndex = 0; // Índice del jugador actual
    public bool isRolling = false; // Indica si los dados están en animación

    // Variables privadas
    private int[] diceValues;
    private int totalDiceValue;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();
        // Inicializar los dados
        diceValues = new int[numberOfDice];

        // Verificar que tenemos la referencia al sistema de visualización
        if (diceDisplay == null)
        {
            diceDisplay = FindObjectOfType<SimpleDiceDisplay>();
            if (diceDisplay == null)
            {
                Debug.LogError("No se encontró el componente SimpleDiceDisplay");
            }
        }
    }

    void Update()
    {
        // Para pruebas: presionar espacio para lanzar los dados
        if (Input.GetKeyDown(KeyCode.Space) && !isRolling)
        {
            if (playerManager.MustSkipNextTurn(gameManager.GetCurrentPlayerIndex()))
            {
                Debug.Log($"presiono tecla turno 1 : ${gameManager.GetCurrentPlayerIndex() + 1}");
                gameManager.ShowNotification($"JUGARDOR : ${gameManager.GetCurrentPlayerIndex() + 1 } PIERDE TURNO");
                playerManager.SetSkipNextTurn(gameManager.GetCurrentPlayerIndex(), false);
                gameManager.SetCurrentPlayerIndex((gameManager.GetCurrentPlayerIndex() + 1) % playerManager.GetPlayerCount());
                Debug.Log($"proximo turno 1 : ${gameManager.GetCurrentPlayerIndex() + 1 }");
                currentPlayerIndex = gameManager.GetCurrentPlayerIndex();
                gameManager.UpdateUI();
                // gameManager.NextPlayer();
                return;
            }
            else
            {
                Debug.Log($"presiono tecla turno 2 : ${gameManager.GetCurrentPlayerIndex() + 1}");
                RollDice();
            }
                
        }
    }

    // Método público para lanzar los dados
    public void RollDice()
    {
      

        if (!isRolling && playerTokens.Length > 0)
        {
            isRolling = true;
            GenerateDiceValues();

            // Usar el nuevo sistema de visualización
            diceDisplay.PlayDiceAnimation(diceValues);

            // Ya no iniciamos el movimiento de ficha aquí
            // Eso lo hará el SimpleDiceDisplay cuando termine su animación
        }
    }

    // Generar valores aleatorios para los dados
    private void GenerateDiceValues()
    {
        totalDiceValue = 0;

        for (int i = 0; i < numberOfDice; i++)
        {
            diceValues[i] = Random.Range(1, 7);
            totalDiceValue += diceValues[i];
        }

      //  Debug.Log("Valores generados para los dados: " + string.Join(", ", diceValues) + " (Total: " + totalDiceValue + ")");
    }

    // Método llamado por SimpleDiceDisplay cuando termina la animación
    public void OnDiceAnimationComplete(int finalValue)
    {
     //   Debug.Log("Animación de dados completada. Valor total: " + finalValue);

        // Verifica que el valor sea correcto
        if (finalValue != totalDiceValue)
        {
            Debug.LogWarning("Discrepancia entre el valor calculado (" + totalDiceValue +
                           ") y el valor de la animación (" + finalValue + ")");
            totalDiceValue = finalValue; // Usar el valor de la animación para estar seguros
        }

        // Mover la ficha
        MoveCurrentPlayerToken();

        // Iniciar el proceso de espera y cambio de turno
        StartCoroutine(WaitForMovementToComplete());
    }

    // Mover la ficha del jugador actual
    void MoveCurrentPlayerToken()
    {
        if (currentPlayerIndex >= 0 && currentPlayerIndex < playerTokens.Length)
        {
           //Debug.Log("Moviendo la ficha del jugador " + (currentPlayerIndex + 1)
         //   Debug.Log("Moviendo la ficha del jugador " + (currentPlayerIndex +1) +
         //           " un total de " + totalDiceValue + " casillas");

            playerTokens[currentPlayerIndex].MoveToken(totalDiceValue);
        }
    }

    // Esperar a que termine el movimiento antes de cambiar de turno
    IEnumerator WaitForMovementToComplete()
    {
        // Esperar hasta que la ficha termine de moverse
        while (playerTokens[currentPlayerIndex].isMoving)
        {
            yield return null;
        }

        // Esperar un momento adicional para que se puedan ver los efectos
        yield return new WaitForSeconds(1.5f);

        // Cambiar al siguiente jugador
        
        currentPlayerIndex = (currentPlayerIndex + 1) % playerTokens.Length;

        // Notificar cambio de turno
        //Debug.Log("Turno del jugador " + (currentPlayerIndex + 1));

        // Si tienes un controlador de cámara, actualízalo
        MonopolyCameraController cameraController = FindObjectOfType<MonopolyCameraController>();
        if (cameraController != null)
        {
            cameraController.OnTurnChange();
        }

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // Esta debería ser la ÚNICA llamada a NextPlayer()
            gameManager.NextPlayer();
        }

        // Finalizar el proceso de lanzamiento
        isRolling = false;
    }

    // Método para forzar un valor específico en los dados (útil para pruebas)
    public void SetDiceValue(int[] values)
    {
        if (values.Length != numberOfDice)
        {
            Debug.LogError("El número de valores no coincide con el número de dados");
            return;
        }

        totalDiceValue = 0;

        for (int i = 0; i < numberOfDice; i++)
        {
            if (values[i] < 1 || values[i] > 6)
            {
                Debug.LogError("Valor de dado inválido: " + values[i] + ". Debe estar entre 1 y 6.");
                return;
            }

            diceValues[i] = values[i];
            totalDiceValue += values[i];
        }

        // Actualizar la visualización directamente con los valores fijados
        if (diceDisplay != null)
        {
            diceDisplay.PlayDiceAnimation(diceValues);
        }
    }

    // Método público para obtener el valor actual de los dados
    public int GetDiceValue()
    {
        return totalDiceValue;
    }
}