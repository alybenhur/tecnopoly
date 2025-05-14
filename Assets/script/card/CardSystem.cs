using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Card
{
    
    public enum CardType
    {
        Question,
        Event,
        Reward,
        Penalty
    }

    public string cardTitle;
    public string cardDescription;
    public CardType type;
    public Sprite cardImage;

    // Efectos específicos de la tarjeta
    public int creditEffect = 0; // Positivo para ganancias, negativo para pérdidas
    public int moveSpaces = 0; // Número de espacios a mover (positivo adelante, negativo atrás)
    public bool skipTurn = false;
    public bool collectFromAllPlayers = false;
    public bool moveToSpecificTile = false;
    public int targetTileIndex = -1; // Índice de casilla específica si moveToSpecificTile es true

   

}

public class CardSystem : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject cardPanel; // Panel UI que mostrará las cartas
    public TextMeshProUGUI cardTitleText;
    public TextMeshProUGUI cardDescriptionText;
    public Image cardImageDisplay;

    [Header("Panel de Preguntas")]
    public GameObject questionPanel; // Panel específico para preguntas
    public TextMeshProUGUI questionText;
    public Button[] answerButtons; // Botones para las opciones de respuesta
    public TextMeshProUGUI[] answerTexts; // Textos para las opciones de respuesta
    public TextMeshProUGUI explanationText; // Texto para mostrar la explicación

    [Header("Tarjetas")]
    public List<Card> eventCards = new List<Card>();
    public List<QuestionCard> questionCards = new List<QuestionCard>();
    public List<Card> rewardCards = new List<Card>();
    public List<Card> penaltyCards = new List<Card>();

    [Header("Configuración")]
    public float cardDisplayTime = 4.0f; // Tiempo que se muestra una carta (en segundos)
    public float explanationDisplayTime = 3.0f; // Tiempo que se muestra la explicación

    // Referencias a otros sistemas
    private GameManager gameManager;
    private PlayerManager playerManager;

    // Estado actual
    private Card currentCard;
    private QuestionCard currentQuestion;
    private bool waitingForAnswer = false;

    private int nextQuestionIndex = 0;
    void Start()
    {
        // Inicializar referencias
        gameManager = FindObjectOfType<GameManager>();
        playerManager = FindObjectOfType<PlayerManager>();

        // Ocultar paneles al inicio
        if (cardPanel) cardPanel.SetActive(false);
        if (questionPanel) questionPanel.SetActive(false);

        // Configurar botones de respuesta
        ConfigureAnswerButtons();

        // Mezclar las tarjetas de preguntas al inicio (opcional)
        ShuffleQuestionCards();
    }

    // Configurar listeners para los botones de respuesta
    private void ConfigureAnswerButtons()
    {
        if (answerButtons != null && answerButtons.Length > 0)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int buttonIndex = i; // Crear una variable local para usar en la lambda
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(buttonIndex));
            }
        }
    }

    // Método para mostrar una tarjeta de evento aleatorio
    public void DrawRandomEventCard()
    {
        if (eventCards.Count == 0)
        {
            Debug.LogWarning("No hay tarjetas de evento disponibles");
            return;
        }

        int randomIndex = Random.Range(0, eventCards.Count);
        Card card = eventCards[randomIndex];

        DisplayCard(card);
        ApplyCardEffect(card);
    }

    // Método para mostrar una pregunta
    public void DrawQuestionCard()
    {
        if (questionCards.Count == 0)
        {
            Debug.LogWarning("No hay tarjetas de pregunta disponibles");
            return;
        }

        if (nextQuestionIndex >= questionCards.Count)
        {
            Debug.Log("Reiniciando el mazo de preguntas");
            nextQuestionIndex = 0;

            // Mezclar el mazo (opcional)
            ShuffleQuestionCards();
        }

        // Obtener la siguiente pregunta en la pila
        currentQuestion = questionCards[nextQuestionIndex];

        // Avanzar al siguiente índice para la próxima vez
        nextQuestionIndex++;

        // int randomIndex = Random.Range(0, questionCards.Count);
        // currentQuestion = questionCards[randomIndex];

        DisplayQuestion(currentQuestion);
    }

    private void ShuffleQuestionCards()
    {
        System.Random rng = new System.Random();
        int n = questionCards.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            QuestionCard temp = questionCards[k];
            questionCards[k] = questionCards[n];
            questionCards[n] = temp;
        }

        Debug.Log("Mazo de preguntas mezclado");
    }

    // Método para mostrar una tarjeta de premio
    public void DrawRewardCard()
    {
        if (rewardCards.Count == 0)
        {
            Debug.LogWarning("No hay tarjetas de premio disponibles");
            return;
        }

        int randomIndex = Random.Range(0, rewardCards.Count);
        Card card = rewardCards[randomIndex];

        DisplayCard(card);
        ApplyCardEffect(card);
    }

    // Método para mostrar una tarjeta de castigo
    public void DrawPenaltyCard()
    {
        if (penaltyCards.Count == 0)
        {
            Debug.LogWarning("No hay tarjetas de castigo disponibles");
            return;
        }

        int randomIndex = Random.Range(0, penaltyCards.Count);
        Card card = penaltyCards[randomIndex];

        DisplayCard(card);
        ApplyCardEffect(card);
    }

    // Mostrar la tarjeta en la UI
    private void DisplayCard(Card card)
    {
        currentCard = card;

        if (cardPanel)
        {
            cardPanel.SetActive(true);

            if (cardTitleText) cardTitleText.text = card.cardTitle;
            if (cardDescriptionText) cardDescriptionText.text = card.cardDescription;
            if (cardImageDisplay && card.cardImage) cardImageDisplay.sprite = card.cardImage;

            // Ocultar la tarjeta después de un tiempo
            StartCoroutine(HideCardAfterDelay(cardDisplayTime));
        }
    }

    // Mostrar una pregunta en la UI
    private void DisplayQuestion(QuestionCard question)
    {
        currentQuestion = question;
        waitingForAnswer = true;

        if (questionPanel)
        {
            // Activar el panel
            questionPanel.SetActive(true);

            // Mostrar la pregunta
            if (questionText) questionText.text = question.questionText;

            // Ocultar la explicación inicialmente
            if (explanationText) explanationText.gameObject.SetActive(false);

            // Configurar las opciones de respuesta
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < question.answerOptions.Count)
                {
                    // Activar y configurar este botón
                    answerButtons[i].gameObject.SetActive(true);
                    answerTexts[i].text = question.answerOptions[i];
                    // Restablecer el color del botón
                    answerButtons[i].GetComponent<Image>().color = Color.white;
                }
                else
                {
                    // Ocultar botones extra si hay menos opciones que botones
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // Procesar la respuesta seleccionada
    private void OnAnswerSelected(int selectedIndex)
    {
        if (!waitingForAnswer || currentQuestion == null) return;

        waitingForAnswer = false;
        bool isCorrect = (selectedIndex == currentQuestion.correctAnswerIndex);

        // Cambiar el color del botón seleccionado según si es correcto o no
        Color buttonColor = isCorrect ? Color.green : Color.red;
        answerButtons[selectedIndex].GetComponent<Image>().color = buttonColor;

        // Mostrar la explicación
        if (explanationText)
        {
            explanationText.gameObject.SetActive(true);
            explanationText.text = isCorrect
                ? (string.IsNullOrEmpty(currentQuestion.correctExplanation)
                    ? "¡Respuesta correcta!"
                    : currentQuestion.correctExplanation)
                : (string.IsNullOrEmpty(currentQuestion.incorrectExplanation)
                    ? "Respuesta incorrecta. La respuesta correcta era: " + currentQuestion.answerOptions[currentQuestion.correctAnswerIndex]
                    : currentQuestion.incorrectExplanation);
        }

        // Dar efecto a la respuesta después de un breve retraso para ver la explicación
        StartCoroutine(ProcessAnswerAfterDelay(isCorrect, explanationDisplayTime));
    }

    // Procesamiento retrasado de la respuesta para dar tiempo a ver la explicación
    private IEnumerator ProcessAnswerAfterDelay(bool isCorrect, float delay)
    {
        yield return new WaitForSeconds(delay);

        int currentPlayerIndex = gameManager?.GetCurrentPlayerIndex() ?? 0;

        if (isCorrect)
        {
            // Recompensa por respuesta correcta
            int rewardAmount = currentQuestion.rewardCredits;
            playerManager?.AddCredits(currentPlayerIndex, rewardAmount);
            gameManager?.ShowNotification($"¡Respuesta correcta! Ganaste {rewardAmount} créditos");
            Debug.Log($"Jugador {currentPlayerIndex + 1} respondió correctamente y ganó {rewardAmount} créditos");
            
            PlayerInfoUIManager uiManager = FindObjectOfType<PlayerInfoUIManager>();
            if (uiManager != null)
            {
                uiManager.UpdatePlayerInfo(currentPlayerIndex);
                uiManager.ShowCreditChange(currentPlayerIndex, rewardAmount);
            }
        }
        else
        {
            // Penalización por respuesta incorrecta
            int penaltyAmount = currentQuestion.penaltyCredits;
            playerManager?.AddCredits(currentPlayerIndex, -penaltyAmount);
            gameManager?.ShowNotification($"Respuesta incorrecta. Perdiste {penaltyAmount} créditos");
            Debug.Log($"Jugador {currentPlayerIndex + 1} respondió incorrectamente y perdió {penaltyAmount} créditos");
            PlayerInfoUIManager uiManager = FindObjectOfType<PlayerInfoUIManager>();
            if (uiManager != null)
            {
                uiManager.UpdatePlayerInfo(currentPlayerIndex);
                uiManager.ShowCreditChange(currentPlayerIndex, -penaltyAmount);
            }
        }

        // Ocultar el panel de preguntas
        if (questionPanel) questionPanel.SetActive(false);

        // Actualizar la UI del juego
        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
    }

    // Aplicar el efecto de la tarjeta al jugador actual
    private void ApplyCardEffect(Card card)
    {
      //  Debug.Log("estamos aqui");
        // Obtener el jugador actual
        int currentPlayerIndex = gameManager?.GetCurrentPlayerIndex() ?? 0;

        // Aplicar efectos monetarios
        if (card.creditEffect != 0)
        {
            playerManager?.AddCredits(currentPlayerIndex, card.creditEffect);
            Debug.Log($"Jugador {currentPlayerIndex + 1} {(card.creditEffect > 0 ? "recibe" : "pierde")} {Mathf.Abs(card.creditEffect)} créditos");

            // NUEVO: Actualizar la UI inmediatamente
            PlayerInfoUIManager uiManager = FindObjectOfType<PlayerInfoUIManager>();
            if (uiManager != null)
            {
                uiManager.UpdatePlayerInfo(currentPlayerIndex);
                uiManager.ShowCreditChange(currentPlayerIndex, card.creditEffect);
            }

          }

        // Aplicar efecto de movimiento
        if (card.moveSpaces != 0)
        {
            Debug.Log("Intentando mover al jugador...");
            // Implementar lógica para mover al jugador
            

            // playerManager?.MovePlayer(currentPlayerIndex, card.moveSpaces);

            //Debug.Log($"Jugador {currentPlayerIndex + 1} se mueve {Mathf.Abs(card.moveSpaces)} espacios {(card.moveSpaces > 0 ? "adelante" : "atrás")}");
        }

        // Aplicar salto de turno
        if (card.skipTurn)
        {
          //  int turnsToSkip = 2;
            playerManager.SetSkipNextTurn(currentPlayerIndex, true);
            Debug.Log($"Jugador {currentPlayerIndex + 1} pierde su próximo turno");
        }

        // Aplicar cobro a todos los jugadores
        if (card.collectFromAllPlayers)
        {
            // Implementar lógica para cobrar a todos
            int playerCount = playerManager?.GetPlayerCount() ?? 0;
            for (int i = 0; i < playerCount; i++)
            {
                if (i != currentPlayerIndex)
                {
                    playerManager?.AddCredits(i, -20); // Cobrar 20 créditos a cada jugador
                    playerManager?.AddCredits(currentPlayerIndex, 20); // Darle esos créditos al jugador actual
                }
            }
            Debug.Log($"Jugador {currentPlayerIndex + 1} cobra 20 créditos a cada jugador");
        }

        // Mover a una casilla específica
        if (card.moveToSpecificTile && card.targetTileIndex >= 0)
        {
            if (playerManager != null)
            {
                // Primero verificar si el método existe
                Debug.Log("Intentando mover al jugador...");

                // Llamar al método MovePlayer directamente
                playerManager.MovePlayer(currentPlayerIndex, card.moveSpaces);

               
            }

            //playerManager?.MovePlayerToTile(currentPlayerIndex, card.targetTileIndex);
            Debug.Log($"Jugador {currentPlayerIndex + 1} se mueve a la casilla {card.targetTileIndex}");
        }

        // Actualizar la UI del juego
        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
    }

    // Ocultar la tarjeta después de un tiempo
    private IEnumerator HideCardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (cardPanel) cardPanel.SetActive(false);
    }

    // Método para procesar cuando un jugador cae en casilla con signo de interrogación rojo
    public void OnPlayerLandsOnEventTile()
    {
        DrawRandomEventCard();
    }

    // Método para procesar cuando un jugador cae en casilla de preguntas
    public void OnPlayerLandsOnQuestionTile()
    {
        DrawQuestionCard();
    }

    // Método para procesar cuando un jugador cae en la casilla del ladrón
    public void OnPlayerLandsOnThiefTile()
    {
        // Mostrar opciones: perder un artefacto o no jugar por 3 turnos
        // Esto requeriría una UI específica con botones o implementar un sistema de diálogo
        // Por simplicidad, aquí mostramos un mensaje de depuración
        Debug.Log("Jugador cayó en casilla del ladrón: Debe elegir entre perder un artefacto o no jugar por 3 turnos");

        // Aquí puedes mostrar un panel con botones para que el jugador elija
    }

    // Método para procesar cuando un jugador cae en la casilla del tesoro
    public void OnPlayerLandsOnTreasureTile()
    {
        // Implementar la lógica del comodín para mover a una casilla vacía y comprar el artefacto
        Debug.Log("Jugador cayó en casilla del tesoro: Puede moverse a una casilla vacía y comprar el artefacto");

        // Aquí necesitarías mostrar las casillas vacías disponibles para que el jugador elija
    }
}