using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TokenMovement : MonoBehaviour
{
    [Header("Referencias")]
    public Transform[] boardTiles; // Array de todas las casillas del tablero en orden
    public MonopolyCameraController cameraController; // Referencia al controlador de cámara

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f; // Velocidad de movimiento
    public float heightAboveTile = 0.3f; // Altura de la ficha sobre la casilla
    public float rotationSpeed = 540f; // Velocidad de rotación durante el movimiento
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curva para suavizar el movimiento

    [Header("Efectos")]
    public bool useRotationEffect = true; // Activar/desactivar efecto de rotación
    public bool useBouncingEffect = true; // Activar/desactivar efecto de rebote
    public float bounceHeight = 0.2f; // Altura del rebote
    public float bounceSpeed = 3f; // Velocidad del rebote

    // Variables
    private int currentTileIndex = 0; // Índice de la casilla actual
    [HideInInspector] public bool isMoving = false; // Ahora es pública pero oculta en el inspector

    void Start()
    {
        // Posicionar la ficha en la casilla inicial (Salida)
        if (boardTiles.Length > 0)
        {
            PositionTokenOnTile(currentTileIndex);
        }
        else
        {
            Debug.LogError("¡No hay casillas asignadas al movimiento de la ficha!");
        }
    }

    // Método público para mover la ficha según el valor de los dados
    public void MoveToken(int diceValue)
    {
        if (!isMoving && boardTiles.Length > 0)
        {
           
           
                StartCoroutine(MoveTokenStepByStep(diceValue));
        }
        else
        {
            Debug.Log("La ficha ya está en movimiento o no hay casillas configuradas.");
        }
    }

    public void ForceMove(int spaces)
    {
        // Detener cualquier movimiento en curso
        StopAllCoroutines();
        isMoving = false;

        Debug.Log($"Forzando movimiento de {spaces} espacios");

        // Iniciar nuevo movimiento
        MoveToken(spaces);
    }


    // Corutina para mover la ficha paso a paso
    private IEnumerator MoveTokenStepByStep(int steps)
    {
        isMoving = true;
       // Debug.Log($"spacion ${steps}");
      //  Debug.Log(steps);
        // Mover la ficha una casilla a la vez
        for (int i = 0; i < steps; i++)
        {
            // Calcular la siguiente casilla
            
            int nextTileIndex = (currentTileIndex + 1) % boardTiles.Length;
          
            // Esperar a que termine el movimiento a la siguiente casilla
            yield return StartCoroutine(MoveToNextTile(nextTileIndex));

            // Actualizar la casilla actual
            currentTileIndex = nextTileIndex;

            // Pequeña pausa entre movimientos
            yield return new WaitForSeconds(0.1f);

            // Comprobar si ha pasado por la casilla de Salida
            if (currentTileIndex == 0 && i != steps - 1)
            {
                // Aquí puedes añadir lógica para cuando pasa por la Salida
                Debug.Log("¡La ficha ha pasado por la Salida!");
            }
        }

        // Notificar a la cámara que la ficha ha terminado de moverse
        if (cameraController != null)
        {
            cameraController.OnTokenFinishedMoving(this.transform, boardTiles[currentTileIndex]);
        }

        // Activar la lógica específica de la casilla
        TriggerTileAction();

        isMoving = false;
    }

    // El resto del código permanece igual...

    // Corutina para mover la ficha a la siguiente casilla con animación suave
    private IEnumerator MoveToNextTile(int nextTileIndex)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = GetPositionAboveTile(nextTileIndex);

        float journeyLength = Vector3.Distance(startPosition, endPosition);
        float startTime = Time.time;

        while (Time.time - startTime < journeyLength / moveSpeed)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            // Aplicar curva de animación para movimiento más natural
            float curvedFraction = movementCurve.Evaluate(fractionOfJourney);

            // Calcular posición intermedia
            Vector3 intermediatePosition = Vector3.Lerp(startPosition, endPosition, curvedFraction);

            // Añadir efecto de rebote si está activado
            if (useBouncingEffect)
            {
                float bounceOffset = Mathf.Sin(curvedFraction * Mathf.PI) * bounceHeight;
                intermediatePosition.y += bounceOffset;
            }

            // Aplicar la posición
            transform.position = intermediatePosition;

            // Añadir efecto de rotación si está activado
            if (useRotationEffect)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // Asegurar que la ficha termina exactamente en la posición correcta
        transform.position = endPosition;
    }

    // Método para posicionar la ficha en una casilla específica
    private void PositionTokenOnTile(int tileIndex)
    {
        if (tileIndex >= 0 && tileIndex < boardTiles.Length)
        {
            transform.position = GetPositionAboveTile(tileIndex);
        }
    }

    // Obtener la posición por encima de una casilla
    private Vector3 GetPositionAboveTile(int tileIndex)
    {
        Vector3 tilePosition = boardTiles[tileIndex].position;
        float tileHeight = GetTileHeight(boardTiles[tileIndex]);

        return new Vector3(tilePosition.x, tilePosition.y + tileHeight + heightAboveTile, tilePosition.z);
    }

    // Obtener la altura de una casilla
    private float GetTileHeight(Transform tile)
    {
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.y;
        }
        return 0.2f; // Valor predeterminado si no hay Renderer
    }

    // Activar la acción asociada a la casilla actual
    private void TriggerTileAction()
    {
        // Aquí implementarás la lógica específica de cada tipo de casilla
        // Por ejemplo, comprar propiedades, pagar rentas, ir a la cárcel, etc.
       // Debug.Log("La ficha ha llegado a la casilla: " + currentTileIndex);

        // Ejemplo: Detectar el tipo de casilla y ejecutar acción correspondiente
        TileAction tileAction = boardTiles[currentTileIndex].GetComponent<TileAction>();
        //Debug.Log(tileAction);
        if (tileAction != null)
        {
            tileAction.OnPlayerLanded(0);
        }

      
    }

    // Método público para establecer la posición directamente (útil para la inicialización)
    public void SetPosition(int tileIndex)
    {
        if (tileIndex >= 0 && tileIndex < boardTiles.Length)
        {
            currentTileIndex = tileIndex;
            PositionTokenOnTile(currentTileIndex);
        }
    }

    // Método para obtener el índice de la casilla actual
    public int GetCurrentTileIndex()
    {
        return currentTileIndex;
    }
}