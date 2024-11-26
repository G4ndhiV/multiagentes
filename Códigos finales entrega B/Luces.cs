using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public Light[] lights; // Luces a controlar
    public CameraSwitcher cameraSwitcher; // Referencia al script de cambio de cámaras
    private static readonly HttpClient client = new HttpClient();
    public string flaskUrl = "http://127.0.0.1:5000/process_image";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Camera activeCamera = cameraSwitcher.GetActiveCamera(); // Obtener la cámara activa
            if (activeCamera != null)
            {
                StartCoroutine(CaptureAndSendImage(activeCamera));
            }
        }
    }

    private IEnumerator CaptureAndSendImage(Camera activeCamera)
    {
        yield return new WaitForEndOfFrame();

        // Capturar imagen desde la cámara activa
        RenderTexture renderTexture = new RenderTexture(640, 480, 24);
        activeCamera.targetTexture = renderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        activeCamera.Render();
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        activeCamera.targetTexture = null;
        RenderTexture.active = null;

        byte[] imageBytes = texture.EncodeToJPG();
        Destroy(texture);

        // Enviar la imagen al servidor Flask
        Task task = SendImageToPythonAsync(imageBytes, activeCamera.GetComponent<CameraWithID>().CameraId);
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }

    private async Task SendImageToPythonAsync(byte[] imageBytes, int cameraId)
    {
        using (var content = new ByteArrayContent(imageBytes))
        {
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var form = new MultipartFormDataContent();
            form.Add(content, "image", "image.jpg");

            try
            {
                HttpResponseMessage response = await client.PostAsync(flaskUrl, form);
                string responseString = await response.Content.ReadAsStringAsync();
                HandleFlaskResponse(responseString, cameraId);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al enviar la imagen: {e.Message}");
            }
        }
    }

    private void HandleFlaskResponse(string responseString, int cameraId)
    {
        // Procesar la respuesta JSON del servidor Flask
        var response = JsonUtility.FromJson<DetectionResponse>(responseString);

        // Mensaje de detección
        if (response.lights > 0)
        {
            Debug.Log($"Cámara {cameraId} detectó {response.lights} ladrón/ladrones.");
        }
        else
        {
            Debug.Log($"Cámara {cameraId} no detectó nada.");
        }

        // Encender las luces correspondientes
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].enabled = i < response.lights;
        }

        // Iniciar la rutina para apagar las luces después de 5 segundos
        StartCoroutine(TurnOffLightsAfterDelay());
    }

    private IEnumerator TurnOffLightsAfterDelay()
    {
        yield return new WaitForSeconds(5);

        // Apagar todas las luces
        foreach (Light light in lights)
        {
            light.enabled = false;
        }

        Debug.Log("Luces apagadas después de 5 segundos.");
    }
}

// Clase auxiliar para deserializar la respuesta JSON
[System.Serializable]
public class DetectionResponse
{
    public string detections; // "Ladron" o "Nada"
    public int lights; // Número de luces que deben encenderse
}
