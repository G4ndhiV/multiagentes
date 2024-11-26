using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class YOLODetection : MonoBehaviour
{
    public Camera activeCamera; // Cámara activa (asignada dinámicamente)
    private static readonly HttpClient client = new HttpClient();

    private void Update()
    {
        // Detectar solo cuando se presiona la barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space) && activeCamera != null)
        {
            StartCoroutine(CaptureAndSendImage());
        }
    }

    private IEnumerator CaptureAndSendImage()
    {
        yield return new WaitForEndOfFrame();

        // Capturar una imagen de la cámara activa
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
        Task task = SendImageToPythonAsync(imageBytes);
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception != null)
        {
            Debug.LogError($"Error al enviar la imagen: {task.Exception.InnerException.Message}");
        }
    }

    private async Task SendImageToPythonAsync(byte[] imageBytes)
    {
        string url = "http://127.0.0.1:5000/process_image";
        using (var content = new ByteArrayContent(imageBytes))
        {
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var form = new MultipartFormDataContent();
            form.Add(content, "image", "image.jpg");

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, form);
                string responseString = await response.Content.ReadAsStringAsync();
                Debug.Log($"Respuesta del servidor: {responseString}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al enviar la imagen: {e.Message}");
            }
        }
    }
}
