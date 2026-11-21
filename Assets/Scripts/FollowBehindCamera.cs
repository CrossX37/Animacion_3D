using UnityEngine;

public class FollowBehindCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Posición y suavizado")]
    public Vector3 offset = new Vector3(0f, 2.5f, -6f);
    public float positionSmoothTime = 0.15f;
    public float rotationSmoothSpeed = 8f;
    public bool followYawOnly = true;

    private Vector3 _currentVelocity;

    private void LateUpdate()
    {
        if (target == null) return;

        // Calcula la rotación base (solo giro horizontal para evitar mareos al saltar)
        float yaw = target.eulerAngles.y;
        Quaternion targetRotation = followYawOnly
            ? Quaternion.Euler(0f, yaw, 0f)
            : target.rotation;

        // Posición deseada detrás del personaje según su orientación
        Vector3 desiredPosition = target.position + (targetRotation * offset);

        // Movimiento suave hacia la posición deseada
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref _currentVelocity,
            positionSmoothTime
        );

        // Mira ligeramente por encima de los pies/centro del personaje
        Vector3 lookTarget = target.position + Vector3.up * 1.5f;
        Quaternion desiredLookRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredLookRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }
}