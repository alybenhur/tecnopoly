using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PlayerInfoUIManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfoPanel
    {
        public GameObject panelObject;
        public Image avatarImage;
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI creditsText;
        public GameObject turnIndicator;
        public Transform propertiesContainer;
        public GameObject propertyIconPrefab;
        public Image panelBackground;
        public GameObject expandedPanel;
        public TextMeshProUGUI detailedPropertiesText;
    }

    [Header("Referencias")]
    public PlayerManager playerManager;
    public GameManager gameManager;

    [Header("Configuración de UI")]
    public PlayerInfoPanel[] playerPanels;
    public Color[] playerColors;
    public Sprite[] avatarSprites;

    [Header("Efectos Visuales")]
    public float activePanelScale = 1.1f;
    public float pulseSpeed = 1.5f;
    public float pulseIntensity = 0.2f;
    public GameObject turnEffectPrefab;

    // Referencia al jugador activo
    private int currentPlayerIndex = -1;
    private Coroutine[] pulseCoroutines;
    private GameObject[] turnEffects;

    void Start()
    {
        // Inicializar referencias si no están asignadas
        if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>();
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();

        // Inicializar arrays
        pulseCoroutines = new Coroutine[playerPanels.Length];
        turnEffects = new GameObject[playerPanels.Length];

        // Configurar los paneles inicialmente
        SetupPlayerPanels();

        // Ocultar todos los paneles expandidos
        foreach (var panel in playerPanels)
        {
            if (panel.expandedPanel != null)
            {
                panel.expandedPanel.SetActive(false);
            }
        }

        // Actualizar la UI con los datos iniciales
        UpdateAllPlayerInfo();
    }

    void Update()
    {
        // Verificar si el jugador activo ha cambiado

        int newPlayerIndex = gameManager.GetCurrentPlayerIndex();
        //int newPlayerIndex = gameManager?.GetCurrentPlayerIndex() ?? 0;
        
       
        if (newPlayerIndex != currentPlayerIndex)
        {
            // Actualizar el jugador activo
            SetActivePlayer(newPlayerIndex);
            currentPlayerIndex = newPlayerIndex;
            
        }
    }

    // Configurar los paneles de jugador
    private void SetupPlayerPanels()
    {
        int playerCount = playerManager?.GetPlayerCount() ?? 0;

        // Configurar cada panel de jugador
        for (int i = 0; i < playerPanels.Length; i++)
        {
            // Activar solo los paneles necesarios según el número de jugadores
            if (i < playerCount)
            {
                playerPanels[i].panelObject.SetActive(true);

                // Asignar color al panel
                if (i < playerColors.Length && playerPanels[i].panelBackground != null)
                {
                    Color panelColor = playerColors[i];
                    panelColor.a = 0.8f; // Semitransparente
                    playerPanels[i].panelBackground.color = panelColor;
                }

                // Asignar avatar
                if (i < avatarSprites.Length && playerPanels[i].avatarImage != null)
                {
                    playerPanels[i].avatarImage.sprite = avatarSprites[i];
                }

                // Establecer nombre del jugador
                if (playerPanels[i].playerNameText != null)
                {
                    playerPanels[i].playerNameText.text = "Jugador " + (i + 1);
                }

                // Desactivar indicador de turno inicial
                if (playerPanels[i].turnIndicator != null)
                {
                    playerPanels[i].turnIndicator.SetActive(false);
                }

                // Configurar escuchadores de eventos
                ConfigurePanelListeners(i);
            }
            else
            {
                // Desactivar paneles no utilizados
                playerPanels[i].panelObject.SetActive(false);
            }
        }
    }

    // Configurar escuchadores de eventos para los paneles
    private void ConfigurePanelListeners(int index)
    {
        // Añadir escuchador de clic para expandir/contraer panel
        Button panelButton = playerPanels[index].panelObject.GetComponent<Button>();
        if (panelButton == null)
        {
            panelButton = playerPanels[index].panelObject.AddComponent<Button>();
            ColorBlock colors = panelButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            panelButton.colors = colors;
        }

        int capturedIndex = index; // Capturar índice para uso en lambda
        panelButton.onClick.RemoveAllListeners();
        panelButton.onClick.AddListener(() => ToggleExpandedPanel(capturedIndex));
    }

    // Expandir o contraer panel detallado
    private void ToggleExpandedPanel(int playerIndex)
    {
        if (playerPanels[playerIndex].expandedPanel != null)
        {
            bool isActive = !playerPanels[playerIndex].expandedPanel.activeSelf;
            playerPanels[playerIndex].expandedPanel.SetActive(isActive);

            if (isActive)
            {
                // Actualizar información detallada
                UpdateDetailedPlayerInfo(playerIndex);
            }
        }
    }

    // Actualizar información de todos los jugadores
    public void UpdateAllPlayerInfo()
    {
        for (int i = 0; i < playerPanels.Length; i++)
        {
            int playerCount = 0;
            if (playerManager != null)
            {
                playerCount = playerManager.GetPlayerCount();
            }

            if (i < playerCount)
            {
                UpdatePlayerInfo(i);
            }
        }
    }

    // Actualizar información de un jugador específico
    public void UpdatePlayerInfo(int playerIndex)
    {
       
        if (playerIndex < 0 || playerIndex >= playerPanels.Length)
            return;

        PlayerInfoPanel panel = playerPanels[playerIndex];

        // Actualizar créditos
        if (panel.creditsText != null)
        {
            int credits = playerManager.GetPlayerCredits(playerIndex);
            panel.creditsText.text = credits.ToString("N0") + " 💰";
        }

        // Actualizar propiedades
        UpdatePropertyIcons(playerIndex);
    }

    // Actualizar iconos de propiedades
    private void UpdatePropertyIcons(int playerIndex)
    {
        PlayerInfoPanel panel = playerPanels[playerIndex];

        if (panel.propertiesContainer == null || panel.propertyIconPrefab == null)
            return;

        // Limpiar contenedor de propiedades
        foreach (Transform child in panel.propertiesContainer)
        {
            Destroy(child.gameObject);
        }

        // Obtener propiedades del jugador
        List<int> playerProperties = playerManager.GetPlayerProperties(playerIndex);

        // Mostrar hasta 5 propiedades con iconos
        int maxIcons = Mathf.Min(playerProperties.Count, 5);
        for (int i = 0; i < maxIcons; i++)
        {
            int propertyIndex = playerProperties[i];
            GameObject iconObject = Instantiate(panel.propertyIconPrefab, panel.propertiesContainer);

            // Configurar tooltip o texto del icono
            TextMeshProUGUI iconText = iconObject.GetComponentInChildren<TextMeshProUGUI>();
            if (iconText != null)
            {
                iconText.text = propertyIndex.ToString();
            }

            // Opcionalmente, configurar color o imagen según el tipo de propiedad
        }

        // Si hay más propiedades, mostrar un indicador "+X"
        if (playerProperties.Count > maxIcons)
        {
            GameObject moreIconObject = Instantiate(panel.propertyIconPrefab, panel.propertiesContainer);
            TextMeshProUGUI moreText = moreIconObject.GetComponentInChildren<TextMeshProUGUI>();
            if (moreText != null)
            {
                moreText.text = "+" + (playerProperties.Count - maxIcons);
            }
        }
    }

    // Actualizar información detallada del jugador
    private void UpdateDetailedPlayerInfo(int playerIndex)
    {
        PlayerInfoPanel panel = playerPanels[playerIndex];

        if (panel.detailedPropertiesText == null)
            return;

        List<int> playerProperties = playerManager.GetPlayerProperties(playerIndex);

        // Construir texto detallado de propiedades
        string detailText = "Propiedades:\n";

        if (playerProperties.Count == 0)
        {
            detailText += "Ninguna propiedad adquirida";
        }
        else
        {
            foreach (int propertyIndex in playerProperties)
            {
                // Obtener nombre de la propiedad (si está disponible)
                TileAction tile = FindTileByIndex(propertyIndex);
                string propertyName = tile != null ? tile.tileName : "Propiedad " + propertyIndex;

                detailText += "• " + propertyName + "\n";
            }
        }

        panel.detailedPropertiesText.text = detailText;
    }

    // En PlayerInfoUIManager.cs, añadir el método para forzar la actualización:
    public void ForceUpdateActivePlayer(int playerIndex)
    {
        Debug.Log($"Forzando actualización del jugador activo a: {playerIndex}");

        // Actualizar el indicador de turno y los efectos visuales
        SetActivePlayer(playerIndex);
        currentPlayerIndex = playerIndex;
    }

    // Establecer jugador activo
    private void SetActivePlayer(int playerIndex)
    {
        // Desactivar efectos del jugador anterior
        for (int i = 0; i < playerPanels.Length; i++)
        {
            // Desactivar indicadores de turno
            if (playerPanels[i].turnIndicator != null)
            {
                playerPanels[i].turnIndicator.SetActive(i == playerIndex);
            }

            // Detener animaciones de pulso previas
            if (pulseCoroutines[i] != null)
            {
                StopCoroutine(pulseCoroutines[i]);
                pulseCoroutines[i] = null;
            }

            // Restablecer escala
            playerPanels[i].panelObject.transform.localScale = Vector3.one;

            // Destruir efectos visuales anteriores
            if (turnEffects[i] != null)
            {
                Destroy(turnEffects[i]);
                turnEffects[i] = null;
            }
        }

        // Activar efectos para el jugador actual
        if (playerIndex >= 0 && playerIndex < playerPanels.Length)
        {
            // Escalar panel
            playerPanels[playerIndex].panelObject.transform.localScale = new Vector3(activePanelScale, activePanelScale, 1);

            // Iniciar animación de pulso
            pulseCoroutines[playerIndex] = StartCoroutine(PulseAnimation(playerIndex));

            // Crear efecto visual adicional
            if (turnEffectPrefab != null)
            {
                turnEffects[playerIndex] = Instantiate(turnEffectPrefab, playerPanels[playerIndex].panelObject.transform);
            }

            // Reproducir sonido de inicio de turno
            // AudioManager.PlaySound("TurnStart");
        }

        // Actualizar información
        UpdateAllPlayerInfo();
    }

    // Animación de pulso para el panel activo
    private IEnumerator PulseAnimation(int playerIndex)
    {
        PlayerInfoPanel panel = playerPanels[playerIndex];

        if (panel.panelBackground == null)
            yield break;

        Color baseColor = panel.panelBackground.color;
        float time = 0f;

        while (true)
        {
            // Calcular factor de pulso
            time += Time.deltaTime * pulseSpeed;
            float pulseFactor = Mathf.Sin(time) * pulseIntensity + 1.0f;

            // Aplicar color pulsante
            Color pulseColor = baseColor * pulseFactor;
            pulseColor.a = baseColor.a; // Mantener transparencia original
            panel.panelBackground.color = pulseColor;

            yield return null;
        }
    }

    // Mostrar animación de cambio de créditos
    public void ShowCreditChange(int playerIndex, int amount)
    {
        if (playerIndex < 0 || playerIndex >= playerPanels.Length)
            return;

        PlayerInfoPanel panel = playerPanels[playerIndex];

        // Actualizar valor actual
        UpdatePlayerInfo(playerIndex);

        // Mostrar animación
        GameObject creditChangeObj = new GameObject("CreditChange");
        creditChangeObj.transform.SetParent(panel.panelObject.transform, false);

        // Añadir componentes
        RectTransform rt = creditChangeObj.AddComponent<RectTransform>();
        TextMeshProUGUI text = creditChangeObj.AddComponent<TextMeshProUGUI>();

        // Configurar texto
        text.text = (amount >= 0 ? "+" : "") + amount.ToString();
        text.color = amount >= 0 ? Color.green : Color.red;
        text.fontSize = 36;
        text.alignment = TextAlignmentOptions.Center;

        // Configurar posición
        rt.anchoredPosition = new Vector2(0, 50);
        rt.sizeDelta = new Vector2(100, 50);

        // Animar y destruir
        StartCoroutine(AnimateCreditChange(creditChangeObj));
    }

    // Animación para cambio de créditos
    private IEnumerator AnimateCreditChange(GameObject obj)
    {
        float duration = 2f;
        float elapsed = 0f;

        RectTransform rt = obj.GetComponent<RectTransform>();
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        Vector2 startPos = rt.anchoredPosition;
        Color startColor = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Mover hacia arriba
            rt.anchoredPosition = startPos + new Vector2(0, t * 50);

            // Desvanecer
            Color newColor = startColor;
            newColor.a = 1 - t;
            text.color = newColor;

            yield return null;
        }

        Destroy(obj);
    }

    // Método auxiliar para encontrar una casilla por su índice
    private TileAction FindTileByIndex(int index)
    {
        TileAction[] tiles = FindObjectsOfType<TileAction>();

        foreach (TileAction tile in tiles)
        {
            if (tile.tileIndex == index)
            {
                return tile;
            }
        }

        return null;
    }
}
