using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePatrol : MonoBehaviour
{
    public List<GameObject> waypoints; // Lista de waypoints
    public List<GameObject> ladrones; // Lista de objetos "Ladron"
    public float minSpeed = 2f; // Velocidad mínima
    public float maxSpeed = 4f; // Velocidad máxima
    private float currentSpeed; // Velocidad actual del dron
    public float rotationSpeed = 2f; // Velocidad de rotación
    public float yRotationOffset = 90f; // Offset de rotación en Y
    public float hoverHeight = 8f; // Altura sobre el ladrón
    public float hoverTime = 5f; // Tiempo de espera sobre el ladrón
    public Light droneLight; // Luz del dron para encender/apagar
    public Guardia guardia; // Referencia al guardia

    private bool waitingAtTarget = false; // Indica si está esperando sobre un objetivo
    private GameObject priorityTarget = null; // Waypoint temporal (ladrón detectado)

    void Start()
    {
        AssignRandomSpeed(); // Asignar velocidad inicial aleatoria

        if (droneLight != null)
        {
            droneLight.enabled = false; // Asegurarse de que la luz comience apagada
        }
    }

    void Update()
    {
        if (waitingAtTarget) return;

        if (priorityTarget != null)
        {
            MoveTowardsPriorityTarget();
        }
        else if (waypoints.Count > 0)
        {
            MoveTowardsWaypoint();
        }
    }

    void MoveTowardsWaypoint()
    {
        Vector3 target = waypoints[0].transform.position;

        // Mover el dron hacia el waypoint actual
        transform.position = Vector3.MoveTowards(transform.position, target, currentSpeed * Time.deltaTime);

        // Calcular la dirección y rotar hacia el waypoint
        Vector3 direction = target - transform.position;
        RotateTowards(direction);

        // Verificar si el dron ha llegado al waypoint actual
        if (Vector3.Distance(transform.position, target) <= 0.1f)
        {
            waypoints.RemoveAt(0); // Eliminar el waypoint alcanzado
        }
    }

    void MoveTowardsPriorityTarget()
    {
        if (priorityTarget == null) return;

        // Posición objetivo: sobre el ladrón, con un ajuste en Y
        Vector3 target = priorityTarget.transform.position + Vector3.up * hoverHeight;

        // Mover el dron hacia el waypoint temporal
        transform.position = Vector3.MoveTowards(transform.position, target, currentSpeed * Time.deltaTime);

        // Calcular la dirección y rotar hacia el waypoint temporal
        Vector3 direction = target - transform.position;
        RotateTowards(direction);

        // Verificar si el dron ha llegado al waypoint temporal
        if (Vector3.Distance(transform.position, target) <= 0.1f)
        {
            StartCoroutine(HoverOverTarget());
        }
    }

    IEnumerator HoverOverTarget()
    {
        waitingAtTarget = true;
        Debug.Log($"Dron: Hovering sobre el ladrón {priorityTarget.GetComponent<Ladron>().LadronId} por {hoverTime} segundos.");

        // Esperar el tiempo definido
        yield return new WaitForSeconds(hoverTime);

        // Encender y apagar la luz
        if (droneLight != null)
        {
            droneLight.enabled = true;
            Debug.Log("Luz del dron encendida.");
            yield return new WaitForSeconds(1f);
            droneLight.enabled = false;
            Debug.Log("Luz del dron apagada.");
        }

        // Notificar al guardia y eliminar al ladrón
        if (priorityTarget != null)
        {
            int ladronId = priorityTarget.GetComponent<Ladron>().LadronId;
            guardia.NotifyLadronEliminated(ladronId);
            Debug.Log($"Dron: Ladrón {ladronId} eliminado.");
            ladrones.Remove(priorityTarget); // Eliminar de la lista de ladrones
            Destroy(priorityTarget); // Destruir el objeto
        }

        priorityTarget = null; // Resetear el objetivo
        waitingAtTarget = false;
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, yRotationOffset, 0);
            Vector3 eulerRotation = targetRotation.eulerAngles;
            eulerRotation.x = 0; // Bloquear rotación en X
            eulerRotation.z = 0; // Bloquear rotación en Z
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(eulerRotation), rotationSpeed * Time.deltaTime);
        }
    }

    public void SetPriorityTarget(int ladronId)
    {
        foreach (GameObject ladron in ladrones)
        {
            Ladron ladronComponent = ladron.GetComponent<Ladron>();
            if (ladronComponent != null && ladronComponent.LadronId == ladronId)
            {
                Debug.Log($"Dron: Prioridad asignada para el ladrón con ID {ladronId}.");
                priorityTarget = ladron; // Establecer el ladrón como waypoint temporal
                return;
            }
        }

        Debug.LogWarning($"Dron: No se encontró un ladrón con el ID {ladronId}.");
    }

    void AssignRandomSpeed()
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        Debug.Log($"Drone: Nueva velocidad asignada: {currentSpeed:F2} unidades/seg.");
    }
}
