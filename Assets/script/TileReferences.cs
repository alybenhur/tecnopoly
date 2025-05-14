using UnityEngine;

public class TileReferences : MonoBehaviour
{
    public Transform[] tiles;

    // Método de ayuda para obtener todas las casillas
    public Transform[] GetAllTiles()
    {
        return tiles;
    }
}
