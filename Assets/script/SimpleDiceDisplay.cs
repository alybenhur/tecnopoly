using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimpleDiceDisplay : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI[] diceValueTexts; // Referencias a los textos que mostrar�n los valores de los dados
    public TextMeshProUGUI totalValueText; // Texto para mostrar la suma total
    public Image[] diceBackgrounds; // Fondos para los dados (opcional)

    [Header("Configuraci�n de Animaci�n")]
    public float animationDuration = 2.0f; // Duraci�n de la animaci�n
    public float numberChangeInterval = 0.1f; // Intervalo entre cambios de n�mero durante la animaci�n
    public Color highlightColor = Color.yellow; // Color de resaltado para el resultado final
    public Color normalColor = Color.white; // Color normal
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curva para suavizar la animaci�n

    [Header("Sonidos")]
    public AudioClip diceRollSound; // Sonido de dados rodando
    public AudioClip diceResultSound; // Sonido de resultado final

    // Referencias privadas
    private AudioSource audioSource;
    private int[] finalValues; // Valores finales de los dados
    private int totalValue; // Valor total final

    // Referencia al sistema de dados principal
    private DiceSystem diceSystem;

    void Awake()
    {
        // Inicializar componentes
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Encontrar la referencia al sistema de dados
        diceSystem = FindObjectOfType<DiceSystem>();
        if (diceSystem == null)
        {
            Debug.LogError("No se pudo encontrar el componente DiceSystem");
        }
    }

    // En DiceSystem.cs, despu�s de que se completa el movimiento

    public void OnMovementCompleted()
    {
        // Procesar la acci�n de la casilla donde cay� el jugador
        // ...

        // IMPORTANTE: Cambiar al siguiente jugador
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            //            gameManager.NextPlayer();
          //  Debug.Log("Cambiando al siguiente jugador despu�s del movimiento");
        }
        else
        {
            Debug.LogError("No se encontr� GameManager para cambiar de turno");
        }
    }
    // M�todo p�blico para iniciar la animaci�n con valores espec�ficos
    public void PlayDiceAnimation(int[] values)
    {
       
        if (values == null || values.Length == 0 || values.Length != diceValueTexts.Length)
        {
            Debug.LogError("Valores de dados inv�lidos");
            return;
        }

        finalValues = values;
        totalValue = 0;
        foreach (int val in values)
        {
            totalValue += val;
        }

        StopAllCoroutines();
        StartCoroutine(AnimateDiceRoll());
        OnMovementCompleted();
    }

   

    // Corutina para animar el lanzamiento de dados
    private IEnumerator AnimateDiceRoll()
    {
        // Reproducir sonido de lanzamiento
        if (diceRollSound != null)
        {
            audioSource.clip = diceRollSound;
            audioSource.Play();
        }

        // Inicializar colores
        foreach (TextMeshProUGUI text in diceValueTexts)
        {
            text.color = normalColor;
        }
        if (totalValueText != null)
        {
            totalValueText.color = normalColor;
            totalValueText.text = "";
        }

        // Animar cambios r�pidos de n�meros
        float startTime = Time.time;

        while (Time.time - startTime < animationDuration)
        {
            // Calcular factor de tiempo curvo para ralentizar gradualmente
            float progress = (Time.time - startTime) / animationDuration;
            float curvedProgress = animationCurve.Evaluate(progress);

            // Ralentizar los cambios a medida que avanza la animaci�n
            float currentInterval = numberChangeInterval * (1 + curvedProgress * 5);

            // Mostrar n�meros aleatorios
            for (int i = 0; i < diceValueTexts.Length; i++)
            {
                int randomValue = Random.Range(1, 7);
                diceValueTexts[i].text = randomValue.ToString();

                // Animar el tama�o del texto
                float scale = 1 + Mathf.Sin(progress * Mathf.PI * 10) * 0.2f * (1 - curvedProgress);
                diceValueTexts[i].transform.localScale = new Vector3(scale, scale, 1);

                // Si estamos cerca del final, empezar a mostrar los valores finales m�s frecuentemente
                if (curvedProgress > 0.7f && Random.value > 0.5f)
                {
                    diceValueTexts[i].text = finalValues[i].ToString();
                }
            }

            // Para el texto del valor total, mostrar la suma de los valores actuales
            if (totalValueText != null)
            {
                int currentTotal = 0;
                for (int i = 0; i < diceValueTexts.Length; i++)
                {
                    currentTotal += int.Parse(diceValueTexts[i].text);
                }
                totalValueText.text = currentTotal.ToString();
            }

            yield return new WaitForSeconds(currentInterval);
        }

        // Mostrar los valores finales
        for (int i = 0; i < diceValueTexts.Length; i++)
        {
            diceValueTexts[i].text = finalValues[i].ToString();
            diceValueTexts[i].color = highlightColor;
            diceValueTexts[i].transform.localScale = Vector3.one;
        }

        // Mostrar el valor total final
        if (totalValueText != null)
        {
            totalValueText.text = totalValue.ToString();
            totalValueText.color = highlightColor;

            // Animar el tama�o del texto total
            StartCoroutine(PulseAnimation(totalValueText.transform, 1.5f, 0.5f));
        }

        // Reproducir sonido de resultado
        if (diceResultSound != null)
        {
            audioSource.clip = diceResultSound;
            audioSource.Play();
        }

        // Informar al sistema de dados que la animaci�n ha terminado
        if (diceSystem != null)
        {
            // Pasa el valor total al sistema de dados para mover la ficha
            // diceSystem.OnDiceAnimationComplete(totalValue);
            diceSystem.OnDiceAnimationComplete(totalValue);
        }
    }

    // Animaci�n de pulso para resaltar el resultado final
    private IEnumerator PulseAnimation(Transform target, float maxScale, float duration)
    {
        Vector3 originalScale = target.localScale;
        float startTime = Time.time;

        // Ampliar
        while (Time.time - startTime < duration / 2)
        {
            float progress = (Time.time - startTime) / (duration / 2);
            float scale = Mathf.Lerp(1, maxScale, progress);
            target.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }

        // Reducir
        startTime = Time.time;
        while (Time.time - startTime < duration / 2)
        {
            float progress = (Time.time - startTime) / (duration / 2);
            float scale = Mathf.Lerp(maxScale, 1, progress);
            target.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }

        target.localScale = originalScale;
    }


}