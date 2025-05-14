using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonopolyCameraController : MonoBehaviour
{

    [Header("Cámara")]
    public Camera mainCamera;

    [Header("Configuración General")]
    public float transitionSpeed = 2.0f;
    public float defaultHeight = 15.0f;
    public float defaultAngle = 45.0f;
    public float zoomOutMultiplier = 1.2f;

    [Header("Zoom a Casillas")]
    public float tileZoomHeight = 5.0f;
    public float tileZoomAngle = 55.0f;
    public float tileZoomDuration = 2.0f;
    public float tileViewDuration = 3.0f;

    [Header("Referencias")]
    public Transform boardCenter;
    public List<Transform> playerTokens = new List<Transform>();

    // Variables privadas
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isTransitioning = false;
    private Transform currentActiveToken;
   
    void Start()
    {
        // Si no hay referencia a la cámara, usar la cámara principal
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Si no hay referencia al centro del tablero, crear un punto en el origen
        if (boardCenter == null)
        {
            GameObject center = new GameObject("BoardCenter");
            center.transform.position = Vector3.zero;
            boardCenter = center.transform;
        }

        // Establecer posición inicial de la cámara
        SetOverviewPosition();
    }

    // Método para cambiar a la vista general cuando cambia el turno
    public void OnTurnChange()
    {
        //  Debug.Log("Cambio de turno registrado en la cámara");
        //  SetOverviewPosition();
        //StartCoroutine(TransitionCamera());
        

    }

    // Método para zoom a la casilla donde terminó de moverse una ficha
    public void OnTokenFinishedMoving(Transform token, Transform tile)
    {
        if (isTransitioning)
            StopAllCoroutines();

        currentActiveToken = token;
        StartCoroutine(ZoomToTile(token, tile));
    }

    // Corutina para acercamiento a la casilla

    private IEnumerator ZoomToTile(Transform token, Transform tile)
    {
        isTransitioning = true;

        // PASO 1: Hacer zoom a la casilla donde está la ficha
       // Debug.Log("Iniciando zoom a la casilla");
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        Vector3 tokenPosition = token.position;
        Vector3 zoomPosition = new Vector3(tokenPosition.x, tokenPosition.y + tileZoomHeight, tokenPosition.z - tileZoomHeight * 0.7f);
        Vector3 lookDirection = tokenPosition - zoomPosition;
        Quaternion zoomRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

        // Animación hacia la casilla
        float startTime = Time.time;
        float zoomDuration = tileZoomDuration;

        while (Time.time - startTime < zoomDuration)
        {
            float progress = (Time.time - startTime) / zoomDuration;
            mainCamera.transform.position = Vector3.Lerp(startPosition, zoomPosition, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, zoomRotation, progress);
            yield return null;
        }

        // Asegurar posición exacta
        mainCamera.transform.position = zoomPosition;
        mainCamera.transform.rotation = zoomRotation;

        // PASO 2: IMPORTANTE - Esperar aquí durante el tiempo configurado
       // Debug.Log("Cámara enfocando la casilla durante " + tileViewDuration + " segundos");
        float waitStartTime = Time.time;

        // Esperar explícitamente el tiempo configurado
        while (Time.time - waitStartTime < tileViewDuration)
        {
            // Mostrar el tiempo transcurrido para depuración
            if (Mathf.FloorToInt(Time.time - waitStartTime) != Mathf.FloorToInt(Time.time - waitStartTime - Time.deltaTime))
            {
             //   Debug.Log("Tiempo transcurrido en zoom: " + Mathf.FloorToInt(Time.time - waitStartTime) + "s");
            }

            // Mantener la cámara fija en la casilla
            mainCamera.transform.position = zoomPosition;
            mainCamera.transform.rotation = zoomRotation;

            yield return null;
        }

        // PASO 3: Regresar a la vista general
      //  Debug.Log("Tiempo de espera completado. Volviendo a vista general");

        // Calcular la posición para la vista general
        SetOverviewPosition(); // Esto actualiza targetPosition y targetRotation

        startPosition = mainCamera.transform.position; // Posición actual (zoom a la casilla)
        startRotation = mainCamera.transform.rotation;
        startTime = Time.time;
        float returnDuration = 2.0f; // Duración fija para el regreso

        // Animación de regreso a vista general
        while (Time.time - startTime < returnDuration)
        {
            float progress = (Time.time - startTime) / returnDuration;
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            yield return null;
        }

        // Asegurar posición final exacta
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

      //  Debug.Log("Cámara en posición general");
        isTransitioning = false;
    }
    private IEnumerator ReturnToOverview()
    {
        // Guardar posición actual
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // Calcular posición de vista general
        SetOverviewPosition(); // Esto actualiza targetPosition y targetRotation

        // Transición a la vista general
        float startTime = Time.time;
        float duration = 12.0f; // Duración fija para la transición de vuelta

        while (Time.time - startTime < duration)
        {
            float progress = (Time.time - startTime) / duration;
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            yield return null;
        }

        // Asegurar posición final
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
    }

    // Calcular posición que muestre todas las fichas
    private void SetOverviewPosition()
    {
        if (playerTokens.Count == 0)
            return;

        // Encontrar el centro de todas las fichas
        Vector3 tokensCenter = Vector3.zero;
        foreach (Transform token in playerTokens)
        {
            if (token != null)
                tokensCenter += token.position;
        }
        tokensCenter /= playerTokens.Count;

        // Calcular la distancia máxima desde el centro a cualquier ficha
        float maxDistance = 0;
        foreach (Transform token in playerTokens)
        {
            if (token != null)
            {
                float distance = Vector3.Distance(tokensCenter, token.position);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
        }

        // Asegurar que la altura de la cámara sea suficiente para ver todas las fichas
        float adjustedHeight = Mathf.Max(defaultHeight, maxDistance * zoomOutMultiplier);

        // Establecer posición objetivo a una altura que permita ver todas las fichas
        targetPosition = tokensCenter + new Vector3(0, adjustedHeight, -adjustedHeight);

        // Rotar la cámara para mirar al centro del tablero
        Vector3 lookDirection = tokensCenter - targetPosition;
        targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }

    // Transición suave a la nueva posición
    private IEnumerator TransitionCamera()
    {
        isTransitioning = true;

        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.05f)
        {
            float distanceCovered = (Time.time - startTime) * transitionSpeed;
            float fractionOfJourney = Mathf.Clamp01(distanceCovered / journeyLength);

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, fractionOfJourney);

            yield return null;
        }

        // Asegurar que la cámara llegue exactamente a la posición objetivo
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        isTransitioning = false;
    }

    // Método para añadir fichas de jugador al seguimiento
    public void RegisterPlayerToken(Transform token)
    {
        if (!playerTokens.Contains(token))
        {
            playerTokens.Add(token);
        }
    }

    // Método para eliminar fichas del seguimiento
    public void UnregisterPlayerToken(Transform token)
    {
        if (playerTokens.Contains(token))
        {
            playerTokens.Remove(token);
        }
    }
}