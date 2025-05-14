using UnityEngine;

public class DiceVisual : MonoBehaviour
{
    [Header("Materiales")]
    [Tooltip("Asigna materiales para cada cara (�ndice 0 = cara con valor 1, etc.)")]
    public Material[] faceMaterials = new Material[6];

    [Header("Configuraci�n Visual")]
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

    // Actualizar la apariencia visual del dado seg�n su valor
    public void SetValue(int value)
    {
        if (value < 1 || value > 6)
        {
            Debug.LogError("Valor de dado inv�lido: " + value);
            return;
        }

        currentValue = value;

        // Si tenemos un solo material para todo el dado, usamos la orientaci�n
        // para mostrar el valor. Este caso es manejado por DiceSystem.OrientDice()

        // Si tenemos materiales individuales para cada cara, aplicarlos aqu�
        ApplyMaterials();

        // Aplicar efecto de resaltado si est� activado
        if (useHighlight)
        {
            HighlightFace(value);
        }
    }

    // Aplicar los materiales correspondientes basados en el valor actual
    private void ApplyMaterials()
    {
        // Si el arreglo de materiales est� configurado completamente
        if (faceMaterials.Length == 6 && faceMaterials[0] != null)
        {
            Material[] materials = new Material[6];

            // Asignar los materiales seg�n la configuraci�n actual
            // Nota: Esto asume un orden espec�fico de caras en el cubo
            // Puede ser necesario ajustar este mapeo seg�n tu modelo espec�fico
            for (int i = 0; i < 6; i++)
            {
                materials[i] = faceMaterials[i];
            }

            // Aplicar los materiales
            meshRenderer.materials = materials;
        }
    }

    // M�todo para resaltar la cara actual
    public void HighlightFace(int value)
    {
        int faceIndex = value - 1; // Convertir valor (1-6) a �ndice (0-5)

        // Si no tenemos suficientes materiales, salir
        if (meshRenderer == null || meshRenderer.materials.Length <= faceIndex) return;

        // Crear copias de los materiales actuales para poder modificarlos
        Material[] tempMaterials = meshRenderer.materials;

        // Reset all materials to their original state
        for (int i = 0; i < tempMaterials.Length; i++)
        {
            if (i != faceIndex)
            {
                // Restaurar emisi�n original para caras no resaltadas
                tempMaterials[i].SetColor("_EmissionColor", Color.black);
                tempMaterials[i].DisableKeyword("_EMISSION");
            }
        }

        // Aplicar efecto de emisi�n para la cara actual
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