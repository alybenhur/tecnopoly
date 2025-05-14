using UnityEngine;

public class DiceVisual : MonoBehaviour
{
    [Header("Materiales")]
    [Tooltip("Asigna materiales para cada cara (índice 0 = cara con valor 1, etc.)")]
    public Material[] faceMaterials = new Material[6];

    [Header("Configuración Visual")]
    public bool useHighlight = false;
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 1.5f;

    // Referencias internas
    private MeshRenderer meshRenderer;
    private Material[] originalMaterials;
    private int currentValue = 1;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Guardar referencia a los materiales originales
        if (meshRenderer != null)
        {
            originalMaterials = meshRenderer.sharedMaterials;
        }
    }

    // Actualizar la apariencia visual del dado según su valor
    public void SetValue(int value)
    {
        if (value < 1 || value > 6)
        {
            Debug.LogError("Valor de dado inválido: " + value);
            return;
        }

        currentValue = value;

        // Si tenemos un solo material para todo el dado, usamos la orientación
        // para mostrar el valor. Este caso es manejado por DiceSystem.OrientDice()

        // Si tenemos materiales individuales para cada cara, aplicarlos aquí
        ApplyMaterials();

        // Aplicar efecto de resaltado si está activado
        if (useHighlight)
        {
            HighlightFace(value);
        }
    }

    // Aplicar los materiales correspondientes basados en el valor actual
    private void ApplyMaterials()
    {
        // Si el arreglo de materiales está configurado completamente
        if (faceMaterials.Length == 6 && faceMaterials[0] != null)
        {
            Material[] materials = new Material[6];

            // Asignar los materiales según la configuración actual
            // Nota: Esto asume un orden específico de caras en el cubo
            // Puede ser necesario ajustar este mapeo según tu modelo específico
            for (int i = 0; i < 6; i++)
            {
                materials[i] = faceMaterials[i];
            }

            // Aplicar los materiales
            meshRenderer.materials = materials;
        }
    }

    // Método para resaltar la cara actual
    public void HighlightFace(int value)
    {
        int faceIndex = value - 1; // Convertir valor (1-6) a índice (0-5)

        // Si no tenemos suficientes materiales, salir
        if (meshRenderer == null || meshRenderer.materials.Length <= faceIndex) return;

        // Crear copias de los materiales actuales para poder modificarlos
        Material[] tempMaterials = meshRenderer.materials;

        // Reset all materials to their original state
        for (int i = 0; i < tempMaterials.Length; i++)
        {
            if (i != faceIndex)
            {
                // Restaurar emisión original para caras no resaltadas
                tempMaterials[i].SetColor("_EmissionColor", Color.black);
                tempMaterials[i].DisableKeyword("_EMISSION");
            }
        }

        // Aplicar efecto de emisión para la cara actual
        tempMaterials[faceIndex].EnableKeyword("_EMISSION");
        tempMaterials[faceIndex].SetColor("_EmissionColor", highlightColor * highlightIntensity);

        // Aplicar los materiales modificados
        meshRenderer.materials = tempMaterials;
    }

    // Restaurar materiales originales
    public void ResetMaterials()
    {
        if (meshRenderer != null && originalMaterials != null)
        {
            meshRenderer.sharedMaterials = originalMaterials;
        }
    }

    // Limpiar referencias al destruir
    void OnDestroy()
    {
        ResetMaterials();
    }
}