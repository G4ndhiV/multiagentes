using UnityEngine;

public class Guardia : MonoBehaviour
{
    [SerializeField] private int guardiaId; // ID único para el guardia
    public DronePatrol drone; // Referencia al dron
    public CameraWithID[] cameras; // Referencia a las cámaras controladas por el guardia

    private void Start()
    {
        Debug.Log($"Guardia inicializado con ID: {guardiaId}");

        // Vincular cámaras con el guardia
        foreach (CameraWithID camera in cameras)
        {
            camera.SetGuardia(this);
        }
    }

    public void NotifyLadronDetected(int cameraId, int ladronId)
    {
        Debug.Log($"Guardia {guardiaId}: Cámara {cameraId} detectó al ladrón con ID {ladronId}. Enviando dron...");
        drone.SetPriorityTarget(ladronId); // Enviar dron al ladrón
    }

    public void NotifyLadronEliminated(int ladronId)
    {
        Debug.Log($"Guardia {guardiaId}: Ladrón con ID {ladronId} eliminado por el dron.");
    }
}
