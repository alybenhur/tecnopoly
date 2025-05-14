using UnityEngine;
using System.Collections;

public class DiceSystem : MonoBehaviour
{
    [Header("Referencias")]
    public TokenMovement[] playerTokens; // Referencias a todas las fichas de los jugadores
    public SimpleDiceDisplay diceDisplay; // Referencia al nuevo sistema de visualizaci�n de dados
    public GameManager gameManager;
    public PlayerManager playerManager;

    [Header("Configuraci�n de Dados")]
    public int numberOfDice = 2; // Cantidad de dados (tradicionalmente 2)

    [Header("Sistema de Turnos")]
    public int currentPlayerIndex = 0; // �ndice del jugador actual
    public bool isRolling = false; // Indica si los dados est�n en animaci�n

    // Variables privadas
    private int[] diceValues;
    private int totalDiceValue;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();
        // Inicializar los dados
        diceValues = new int[numberOfDice];

        // Verificar que tenemos la referencia al sistema de visualizaci�n
        if (diceDisplay == null)
        {
            diceDisplay = FindObjectOfType<SimpleDiceDisplay>();
            if (diceDisplay == null)
            {
                Debug.LogError("No se encontr� el componente SimpleDiceDisplay");
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

    // M�todo p�blico para lanzar los dados
    public void RollDice()
    {
      

        if (!isRolling && playerTokens.Length > 0)
        {
            isRolling = true;
            GenerateDiceValues();

            // Usar el nuevo sistema de visualizaci�n
            diceDisplay.PlayDiceAnimation(diceValues);

            // Ya no iniciamos el movimiento de ficha aqu�
            // Eso lo har� el SimpleDiceDisplay cuando termine su animaci�n
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

    // M�todo llamado por SimpleDiceDisplay cuando termina la animaci�n
    public void OnDiceAnimationComplete(int finalValue)
    {
     //   Debug.Log("Animaci�n de dados completada. Valor total: " + finalValue);

        // Verifica que el valor sea correcto
        if (finalValue != totalDiceValue)
        {
            Debug.LogWarning("Discrepancia entre el valor calculado (" + totalDiceValue +
                           ") y el valor de la animaci�n (" + finalValue + ")");
            totalDiceValue = finalValue; // Usar el valor de la animaci�n para estar seguros
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

        // Si tienes un controlador de c�mara, actual�zalo
        MonopolyCameraController cameraController = FindObjectOfType<MonopolyCameraController>();
        if (cameraController != null)
        {
            cameraController.OnTurnChange();
        }

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // Esta deber�a ser la �NICA llamada a NextPlayer()
            gameManager.NextPlayer();
        }

        // Finalizar el proceso de lanzamiento
        isRolling = false;
    }

    // M�todo para forzar un valor espec�fico en los dados (�til para pruebas)
    public void SetDiceValue(int[] values)
    {
        if (values.Length != numberOfDice)
        {
            Debug.LogError("El n�mero de valores no coincide con el n�mero de dados");
            return;
        }

        totalDiceValue = 0;

        for (int i = 0; i < numberOfDice; i++)
        {
            if (values[i] < 1 || values[i] > 6)
            {
                Debug.LogError("Valor de dado inv�lido: " + values[i] + ". Debe estar entre 1 y 6.");
                return;
            }

            diceValues[i] = values[i];
            totalDiceValue += values[i];
        }

        // Actualizar la visualizaci�n directamente con los valores fijados
        if (diceDisplay != null)
        {
            diceDisplay.PlayDiceAnimation(diceValues);
        }
    }

    // M�todo p�blico para obtener el valor actual de los dados
    public int GetDiceValue()
    {
        return totalDiceValue;
    }
}