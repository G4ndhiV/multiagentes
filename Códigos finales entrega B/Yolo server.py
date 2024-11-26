from flask import Flask, request, jsonify
import cv2
import numpy as np
import torch

# Configuración del modelo YOLO
MODEL_PATH = r"C:\Users\gandh\OneDrive\Escritorio\dataset\yolov5\runs\train\worker_thief_guard_model\weights\best.pt"
model = torch.hub.load('ultralytics/yolov5', 'custom', path=MODEL_PATH)

app = Flask(__name__)

@app.route('/process_image', methods=['POST'])
def process_image():
    try:
        # Leer imagen enviada por Unity
        file = request.files['image'].read()
        np_arr = np.frombuffer(file, np.uint8)
        image = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

        # Procesar imagen con YOLO
        results = model(image)
        detections = results.pandas().xyxy[0]

        # Filtrar detecciones por el nombre "Ladron"
        ladron_detections = detections[detections['name'] == 'Ladron']
        num_ladrones = len(ladron_detections)

        # Enviar respuesta a Unity
        response = {
            "detections": "Ladron" if num_ladrones > 0 else "Nada",
            "lights": num_ladrones
        }
        return jsonify(response), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
