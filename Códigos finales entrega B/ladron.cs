using UnityEngine;

public class Ladron : MonoBehaviour
{
    [SerializeField] private int ladronId; // ID del ladrón, configurable manualmente

    public int LadronId
    {
        get { return ladronId; }
        set
        {
            if (value > 0)
            {
                ladronId = value;
                Debug.Log($"Ladrón ID actualizado a: {ladronId}");
            }
            else
            {
                Debug.LogWarning("El ID debe ser un número positivo.");
            }
        }
    }

    private void Start()
    {
        Debug.Log($"Ladrón inicializado con ID: {ladronId}");
    }
}
