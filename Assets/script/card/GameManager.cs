using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Sistemas de Juego")]
    public CardSystem cardSystem;
    public DiceSystem diceSystem;
    public PlayerManager playerManager;

    [Header("Configuraci�n General")]
    public int initialCredits = 1000; // Cr�ditos iniciales por jugador
    public int salaryAmount = 200; // Cr�ditos al pasar por la casilla de Salida

    [Header("UI General")]
    public TextMeshProUGUI currentPlayerText;
    public TextMeshProUGUI playerCreditsText;
    public TextMeshProUGUI gameStatusText;
    public GameObject gameOverPanel;

    [Header("UI de Propiedades")]
    public GameObject purchasePanel;
    public TextMeshProUGUI propertyNameText;
    public TextMeshProUGUI propertyCostText;
    public Button buyButton;
    public Button declineButton;

    [Header("UI de Renta")]
    public GameObject rentPanel;
    public TextMeshProUGUI rentTitleText;
    public TextMeshProUGUI ownerNameText;
    public TextMeshProUGUI rentAmountText;
    public Button payRentButton;

    [Header("UI de Notificaciones")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;

    // Estado del juego
    private int currentPlayerIndex = 0;
    private bool gameIsOver = false;
    private int totalArtefacts = 0; // Total de artefactos en el juego
    private int purchasedArtefacts = 0; // Artefactos ya comprados

    void Start()
    {
        // Inicializar referencias si no est�n asignadas
        if (cardSystem == null) cardSystem = FindObjectOfType<CardSystem>();
        if (diceSystem == null) diceSystem = FindObjectOfType<DiceSystem>();
        if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();

        // Inicializar el juego
        InitializeGame();

        // Ocultar paneles
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (purchasePanel) purchasePanel.SetActive(false);
        if (rentPanel) rentPanel.SetActive(false);
        if (notificationPanel) notificationPanel.SetActive(false);

        // Configurar los listeners de botones
        ConfigureButtonListeners();
    }

    private void ConfigureButtonListeners()
    {
        // Botones del panel de compra
        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => {
                // Se asignar� din�micamente en OfferPropertyPurchase
            });
        }

        if (declineButton)
        {
            declineButton.onClick.RemoveAllListeners();
            declineButton.onClick.AddListener(() => {
                DeclinePurchase();
            });
        }

        // Bot�n del panel de renta
        if (payRentButton)
        {
            payRentButton.onClick.RemoveAllListeners();
            payRentButton.onClick.AddListener(() => {
                // Se asignar� din�micamente en PayRent
            });
        }
    }

    private void InitializeGame()
    {
        // Contar el total de artefactos en el juego
        TileAction[] tiles = FindObjectsOfType<TileAction>();
        foreach (TileAction tile in tiles)
        {
            if (tile.tileType == TileAction.TileType.Property)
            {
                totalArtefacts++;
            }
        }

        // Inicializar jugadores
        int playerCount = playerManager.GetPlayerCount();
        for (int i = 0; i < playerCount; i++)
        {
            playerManager.SetPlayerCredits(i, initialCredits);
        }

        // Establecer el jugador inicial
        currentPlayerIndex = 0;
        UpdateUI();

        Debug.Log($"Juego inicializado con {playerCount} jugadores y {totalArtefacts} artefactos tecnol�gicos");
    }

    // Actualizar la UI del juego
    public void UpdateUI()
    {
        if (currentPlayerText)
        {
         //   currentPlayerText.text = $"Turno del Jugador {currentPlayerIndex + 1}";
        }

        if (playerCreditsText)
        {
            playerCreditsText.text = $"Cr�ditos: {playerManager.GetPlayerCredits(currentPlayerIndex)}";
        }

        if (gameStatusText)
        {
            int remainingArtefacts = totalArtefacts - purchasedArtefacts;
            gameStatusText.text = $"Artefactos disponibles: {remainingArtefacts}/{totalArtefacts}";
        }
    }

    // Avanzar al siguiente jugador
    /* public void NextPlayer()
     {
         Debug.Log("gamer manager");
         // Verificar si el juego ha terminado
         if (gameIsOver) return;

         // Avanzar al siguiente jugador


         Debug.Log($"Jugador nextplayer {currentPlayerIndex } ");
         // Verificar si el jugador debe saltarse el turno
           if (playerManager.ShouldSkipTurn(currentPlayerIndex))
           {
               Debug.Log($"Jugador {currentPlayerIndex + 1} se salta su turno");
               playerManager.DecrementSkipTurnCounter(currentPlayerIndex);
               NextPlayer(); // Pasar al siguiente jugador
               return;
           }
         currentPlayerIndex = (currentPlayerIndex + 1) % playerManager.GetPlayerCount();
         // Actualizar la UI
         UpdateUI();

         Debug.Log($"Turno del Jugador {currentPlayerIndex + 1}");
     }
    */
    public void NextPlayer()
    {
        // Verificar si el juego ha terminado
        if (gameIsOver) return;

        // Avanzar al siguiente jugador
        //   currentPlayerIndex = (currentPlayerIndex + 1) % playerManager.GetPlayerCount();

        // bool validPlayerFound = false;
        // int startingIndex = currentPlayerIndex;
        //int loopCount = 0;
        /*
        while (!validPlayerFound)
        {
            // Avanzar al siguiente jugador
            currentPlayerIndex = (currentPlayerIndex + 1) % playerManager.GetPlayerCount();
            Debug.Log($"JUGARDOR DEL TURNO PROXIMO ${currentPlayerIndex + 1}");
            // Control de seguridad para evitar bucles infinitos
            loopCount++;
            if (loopCount > playerManager.GetPlayerCount() * 2)
            {
                Debug.LogError("Posible bucle infinito en NextPlayer(). Forzando salida.");
                break;
            }

            // Verificar si el jugador debe saltarse el turno
            if (playerManager.MustSkipNextTurn(currentPlayerIndex))
            {
                Debug.Log($"Jugador {currentPlayerIndex + 1} se salta su turno");

                // Mostrar notificaci�n
                ShowNotification($"Jugador {currentPlayerIndex + 1} pierde su turno");

                // Quitar la marca porque el jugador realmente ha perdido su turno
                Debug.Log($"antes : ${currentPlayerIndex}");
                playerManager.SetSkipNextTurn(currentPlayerIndex, false);
                currentPlayerIndex = (currentPlayerIndex + 1) % playerManager.GetPlayerCount();
                Debug.Log($"despues : ${currentPlayerIndex}");
                // Continuar el bucle para buscar el siguiente jugador v�lido
                continue;
            }

            // Si llegamos aqu�, hemos encontrado un jugador v�lido
            validPlayerFound = true;
        }

        // Si dimos la vuelta completa y volvimos al mismo jugador, asegurarse de actualizar la UI
        if (currentPlayerIndex == startingIndex)
        {
            Debug.Log("Vuelta completa sin encontrar jugadores v�lidos, permaneciendo en el mismo jugador");
        }
*/
        currentPlayerIndex = (currentPlayerIndex + 1) % playerManager.GetPlayerCount();
        // Actualizar la UI
        UpdateUI();
        Debug.Log($"Turno del Jugador nextplayer {currentPlayerIndex + 1}");
        
    }
    // M�todo para procesar la compra de un artefacto
    public void PurchaseArtefact(int tileIndex, int cost)
    {
        // Verificar si el jugador puede permitirse la compra

        int act = currentPlayerIndex;
        
        if (act > 0)
            act--;
        else
        {
            int playerCount = playerManager.GetPlayerCount() - 1;
            act = playerCount;
        }



        // int playerCredits = playerManager.GetPlayerCredits(currentPlayerIndex);
        int playerCredits = playerManager.GetPlayerCredits(act);

        if (playerCredits >= cost)
        {
            // Realizar la compra
          //  playerManager.AddCredits(currentPlayerIndex, -cost);
           // playerManager.AssignPropertyToPlayer(currentPlayerIndex, tileIndex);
            playerManager.AddCredits(act, -cost);
            playerManager.AssignPropertyToPlayer(act, tileIndex);

            purchasedArtefacts++;

            ShowNotification($"�Artefacto adquirido por {cost} cr�ditos!");
           // Debug.Log($"Jugador {currentPlayerIndex + 1} compr� artefacto en casilla {tileIndex} por {cost} cr�ditos");
            Debug.Log($"Jugador {act + 1} compr� artefacto en casilla {tileIndex} por {cost} cr�ditos");

            // Actualizar UI
            UpdateUI();

            PlayerInfoUIManager uiManager = FindObjectOfType<PlayerInfoUIManager>();
            if (uiManager != null)
            {
              /*  uiManager.UpdatePlayerInfo(currentPlayerIndex);
                // A�ADIR ESTO: Forzar la actualizaci�n del indicador de turno
                uiManager.ForceUpdateActivePlayer(currentPlayerIndex);*/
                uiManager.UpdatePlayerInfo(act);
                // A�ADIR ESTO: Forzar la actualizaci�n del indicador de turno
                uiManager.ForceUpdateActivePlayer(act);


                // Opcional: mostrar animaci�n de cambio de cr�ditos
                //   uiManager.ShowCreditChange(currentPlayerIndex, -cost);
                uiManager.ShowCreditChange(act, -cost);
            }

            // Verificar si el juego ha terminado
            CheckGameOver();
        }
        else
        {
            ShowNotification("No tienes suficientes cr�ditos para comprar");
          //  Debug.Log($"Jugador {currentPlayerIndex + 1} no tiene suficientes cr�ditos para comprar");
            Debug.Log($"Jugador {act + 1} no tiene suficientes cr�ditos para comprar");
        }

        // Cerrar panel de compra
        if (purchasePanel)
            purchasePanel.SetActive(false);
    }

    // M�todo para ofrecer comprar una propiedad
    private void OfferPropertyPurchase(int tileIndex, int cost)
    {
        TileAction tile = GetTileAtIndex(tileIndex);
        if (tile == null) return;

        // Activar panel UI para compra
        if (purchasePanel != null)
        {
            purchasePanel.SetActive(true);

            // Configurar textos en el panel
            if (propertyNameText != null)
                propertyNameText.text = tile.tileName;

            if (propertyCostText != null)
                propertyCostText.text = cost.ToString() + " Cr�ditos";

            // Configurar bot�n de compra con su callback
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => ConfirmPurchase(tileIndex, cost));
            }
        }
    }

    // Confirmar la compra de un artefacto
    private void ConfirmPurchase(int tileIndex, int cost)
    {
        PurchaseArtefact(tileIndex, cost);
    }

    // Rechazar la compra
    private void DeclinePurchase()
    {
        // Cerrar panel de compra
        if (purchasePanel != null)
            purchasePanel.SetActive(false);

        ShowNotification("Compra rechazada");
    }

    // M�todo para pagar renta a otro jugador
    public void PayRent(int tileIndex)
    {
        // Obtener propietario de la casilla
        int ownerIndex = playerManager.GetPropertyOwner(tileIndex);
        TileAction tile = GetTileAtIndex(tileIndex);

        // Verificar que el propietario no sea el jugador actual
        if (ownerIndex != -1 && ownerIndex != currentPlayerIndex)
        {
            // Determinar la tarifa de uso (entre 20 y 50 cr�ditos)
            int rentAmount = DetermineRentAmount(tileIndex);

            // Mostrar panel de renta
            if (rentPanel != null)
            {
                rentPanel.SetActive(true);

                if (rentTitleText != null)
                    rentTitleText.text = "Tarifa de uso: " + tile.tileName;

                if (ownerNameText != null)
                    ownerNameText.text = "Propietario: Jugador " + (ownerIndex + 1);

                if (rentAmountText != null)
                    rentAmountText.text = rentAmount.ToString() + " Cr�ditos";

                // Configurar bot�n para continuar
                if (payRentButton != null)
                {
                    payRentButton.onClick.RemoveAllListeners();
                    payRentButton.onClick.AddListener(() => ConfirmRentPayment(ownerIndex, rentAmount));
                }
            }
        }
    }

    // Confirmar el pago de renta
    private void ConfirmRentPayment(int ownerIndex, int rentAmount)
    {
        // Realizar el pago
        playerManager.AddCredits(currentPlayerIndex, -rentAmount);
        playerManager.AddCredits(ownerIndex, rentAmount);

        ShowNotification($"Has pagado {rentAmount} cr�ditos al Jugador {ownerIndex + 1}");
        Debug.Log($"Jugador {currentPlayerIndex + 1} pag� {rentAmount} cr�ditos al Jugador {ownerIndex + 1} por uso del artefacto");

        // Actualizar UI
        UpdateUI();

        // A�ADIR ESTO: Notificar al PlayerInfoUIManager para actualizar los paneles de jugador
        PlayerInfoUIManager uiManager = FindObjectOfType<PlayerInfoUIManager>();
        if (uiManager != null)
        {
            // Actualizar informaci�n de ambos jugadores involucrados
            uiManager.UpdatePlayerInfo(currentPlayerIndex); // Jugador que paga
            uiManager.UpdatePlayerInfo(ownerIndex);         // Jugador que recibe

            // Mostrar animaciones de cambio de cr�ditos
            uiManager.ShowCreditChange(currentPlayerIndex, -rentAmount);
            uiManager.ShowCreditChange(ownerIndex, rentAmount);
        }
        // Cerrar panel de renta
        if (rentPanel != null)
            rentPanel.SetActive(false);
    }

    // Determinar la cantidad de renta para una casilla espec�fica
    private int DetermineRentAmount(int tileIndex)
    {
        TileAction tile = GetTileAtIndex(tileIndex);

        // Si la casilla tiene un valor de renta definido, usarlo
        if (tile != null && tile.rentCost > 0)
        {
            return tile.rentCost;
        }

        // Por defecto, usar un valor aleatorio entre 20 y 50
        return Random.Range(20, 51);
    }

    // Verificar si el juego ha terminado
    private void CheckGameOver()
    {
        if (purchasedArtefacts >= totalArtefacts)
        {
            EndGame();
        }
    }

    // Finalizar el juego
    private void EndGame()
    {
        gameIsOver = true;

        // Determinar el ganador
        int winnerIndex = DetermineWinner();

        // Mostrar mensaje de fin de juego
        if (gameStatusText)
        {
            gameStatusText.text = $"�Juego terminado! Ganador: Jugador {winnerIndex + 1}";
        }

        // Mostrar panel de fin de juego
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);

            // Configurar texto del panel si existe
            TextMeshProUGUI[] texts = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.Contains("Winner"))
                {
                    text.text = $"�El Jugador {winnerIndex + 1} ha ganado!";
                }
                else if (text.name.Contains("Stats"))
                {
                    text.text = $"Artefactos: {playerManager.GetPlayerProperties(winnerIndex).Count}\n" +
                                $"Cr�ditos: {playerManager.GetPlayerCredits(winnerIndex)}";
                }
            }
        }

        Debug.Log($"�Juego terminado! Jugador {winnerIndex + 1} es el ganador");
    }

    // Determinar el ganador basado en la cantidad de artefactos
    private int DetermineWinner()
    {
        int winnerIndex = 0;
        int maxArtefacts = 0;

        for (int i = 0; i < playerManager.GetPlayerCount(); i++)
        {
            int playerArtefacts = playerManager.GetPlayerProperties(i).Count;

            if (playerArtefacts > maxArtefacts)
            {
                maxArtefacts = playerArtefacts;
                winnerIndex = i;
            }
            else if (playerArtefacts == maxArtefacts)
            {
                // Desempate basado en cr�ditos
                if (playerManager.GetPlayerCredits(i) > playerManager.GetPlayerCredits(winnerIndex))
                {
                    winnerIndex = i;
                }
            }
        }

        return winnerIndex;
    }
    public void  SetCurrentPlayerIndex(int turno)
    {
        currentPlayerIndex = turno;
    }
    // Obtener el �ndice del jugador actual
    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    // Procesar cuando un jugador pasa por la casilla de Salida
    public void OnPlayerPassedGo()
    {
        playerManager.AddCredits(currentPlayerIndex, salaryAmount);
        ShowNotification($"Recibiste {salaryAmount} cr�ditos por pasar por Salida");
        Debug.Log($"Jugador {currentPlayerIndex + 1} recibi� {salaryAmount} cr�ditos por pasar por Salida");

        // Actualizar UI
        UpdateUI();
    }

    // Procesar cuando un jugador cae en una casilla espec�fica
    public void OnPlayerLandedOnTile(int tileIndex)
    {
        // Obtener la casilla
        TileAction tile = GetTileAtIndex(tileIndex);

        if (tile == null)
        {
            Debug.LogError($"No se encontr� casilla con �ndice {tileIndex}");
            return;
        }

       // Debug.Log($"Jugador {currentPlayerIndex + 1} cay� en casilla {tileIndex}: {tile.tileName} (Tipo: {tile.tileType})");

        // Procesar seg�n el tipo de casilla
        switch (tile.tileType)
        {
            case TileAction.TileType.Property:
                // Verificar si la propiedad ya tiene due�o

                int ownerIndex = playerManager.GetPropertyOwner(tileIndex);
               // Debug.Log($"Jugador {currentPlayerIndex + 1} ");
                if (ownerIndex == -1)
                {
                    // Ofrecer comprar la propiedad
                  //  Debug.Log($"Jugador {currentPlayerIndex + 1} puede comprar el artefacto por {tile.propertyCost} cr�ditos");
                    OfferPropertyPurchase(tileIndex, tile.propertyCost);
                }
                else if (ownerIndex != currentPlayerIndex)
                {
                    // Pagar renta al propietario
                    PayRent(tileIndex);
                }
                else
                {
                    // El jugador ya es propietario
                    ShowNotification("Ya eres propietario de este artefacto");
                }
                break;

            case TileAction.TileType.Chance:
                // Activar tarjeta de evento
                cardSystem.OnPlayerLandsOnEventTile();
                break;

            case TileAction.TileType.CommunityChest:
                // Activar pregunta
                cardSystem.OnPlayerLandsOnQuestionTile();
                break;

            case TileAction.TileType.GoToJail:
                // Activar ladr�n
                cardSystem.OnPlayerLandsOnThiefTile();
                break;

            case TileAction.TileType.FreeParking:
                // Activar tesoro
                cardSystem.OnPlayerLandsOnTreasureTile();
                break;

            case TileAction.TileType.Go:
                // Casilla de Salida
                ShowNotification("�Pasaste por la casilla de Salida!");
                OnPlayerPassedGo();
                break;

            case TileAction.TileType.Tax:
                // Casilla de impuesto
                int taxAmount = tile.taxAmount > 0 ? tile.taxAmount : 100;
                playerManager.AddCredits(currentPlayerIndex, -taxAmount);
                ShowNotification($"Pagaste {taxAmount} cr�ditos de impuestos");
                UpdateUI();
                break;
        }
    }

    // Mostrar una notificaci�n temporal
    public void ShowNotification(string message, float duration = 3.0f)
    {
        if (notificationPanel != null && notificationText != null)
        {
            // Establecer el mensaje
            notificationText.text = message;

            // Mostrar el panel
            notificationPanel.SetActive(true);

            // Ocultar despu�s de la duraci�n especificada
            StartCoroutine(HideNotificationAfterDelay(duration));
        }
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    // Obtener una casilla por su �ndice
    private TileAction GetTileAtIndex(int index)
    {
        TileAction[] tiles = FindObjectsOfType<TileAction>();

        foreach (TileAction tile in tiles)
        {
            if (tile.tileIndex == index)
            {
                return tile;
            }
        }

        Debug.LogError($"No se encontr� ninguna casilla con �ndice {index}");
        return null;
    }
}