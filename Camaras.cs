using UnityEngine;

public class CameraWithID : MonoBehaviour
{
    [SerializeField] private int cameraId; // ID único de la cámara
    public Ladron[] ladrones; // Referencia a los ladrones en el rango de la cámara
    private Guardia guardia; // Referencia al guardia

    public void SetGuardia(Guardia guardia)
    {
        this.guardia = guardia;
    }

    public int CameraId
    {
        get { return cameraId; }
        set
        {
            if (value > 0)
            {
                cameraId = value;
                Debug.Log($"Cámara ID actualizado a: {cameraId}");
            }
            else
            {
                Debug.LogWarning("El ID debe ser un número positivo.");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DetectLadron();
        }
    }

    void DetectLadron()
    {
        foreach (Ladron ladron in ladrones)
        {
            if (ladron != null && ladron.LadronId == cameraId) // Simula detección basada en ID
            {
                Debug.Log($"Cámara {cameraId}: Detectó al ladrón con ID {ladron.LadronId}.");
                guardia.NotifyLadronDetected(cameraId, ladron.LadronId);
                return;
            }
        }

        Debug.Log($"Cámara {cameraId}: No se detectaron ladrones.");
    }
}
