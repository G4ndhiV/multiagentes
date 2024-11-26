using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public List<Camera> cameras; // Lista de cámaras en la escena
    private int currentCameraIndex = 0; // Índice de la cámara activa

    void Start()
    {
        if (cameras.Count == 0)
        {
            Debug.LogError("No hay cámaras asignadas al CameraSwitcher.");
            return;
        }

        // Activar la cámara inicial y desactivar las demás
        SetActiveCamera(currentCameraIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Cambiar a la siguiente cámara
            currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;
            SetActiveCamera(currentCameraIndex);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Cambiar a la cámara anterior
            currentCameraIndex = (currentCameraIndex - 1 + cameras.Count) % cameras.Count;
            SetActiveCamera(currentCameraIndex);
        }
    }

    void SetActiveCamera(int index)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].gameObject.SetActive(i == index);
        }

        Debug.Log($"Cámara activa: {cameras[index].name}");
    }

    public Camera GetActiveCamera()
    {
        return cameras[currentCameraIndex];
    }
}
