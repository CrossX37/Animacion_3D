using UnityEngine.InputSystem;
using UnityEngine;

public class FollowBehindCamera : MonoBehaviour
{
    [Header("Personaje")]
    public Transform target;

    [Header("Posicion de camara")]
    public Vector3 offset = new Vector3(0f, 2.5f, -5f);

    [Header("Enfoque")]
    public float alturaMirada = 1.5f;

    [Header("Suavizado")]
    public float suavizadoVertical = 0.10f;
    public float suavizadoRotacion = 10f;

    private float velocidadY;

    [Header("Rotacion manual de camara")]
    public float velocidadGiro = 100f;

    [Header("Direccion inicial de camara")]
    private float anguloInicial = 90f;
    private float anguloCamara;

    void Start()
    {
        anguloCamara = anguloInicial;

        if (target == null) return;

        transform.position =
            target.position +
            Quaternion.Euler(0f, anguloCamara, 0f) * offset;

        transform.LookAt(
            target.position + Vector3.up * alturaMirada
        );
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Keyboard.current != null)
{
    if (Keyboard.current.leftArrowKey.isPressed)
        anguloCamara -= velocidadGiro * Time.deltaTime;

    if (Keyboard.current.rightArrowKey.isPressed)
        anguloCamara += velocidadGiro * Time.deltaTime;
}

        // Posicion independiente del giro de Bonny
        Vector3 posicionDeseada = target.position + Quaternion.Euler(0f, anguloCamara, 0f) * offset;

        // Seguimiento inmediato horizontal:
        // evita retraso y alejamiento durante sprint
        float x = posicionDeseada.x;
        float z = posicionDeseada.z;

        // Suavizado solo para cambios de altura
        float y = Mathf.SmoothDamp(
            transform.position.y,
            posicionDeseada.y,
            ref velocidadY,
            Mathf.Max(0.01f, suavizadoVertical)
        );

        transform.position = new Vector3(x, y, z);

        // Mirar al personaje sin copiar su rotacion
        Vector3 puntoMirada =
            target.position + Vector3.up * alturaMirada;

        Vector3 direccion = puntoMirada - transform.position;

        if (direccion.sqrMagnitude > 0.0001f)
        {
            Quaternion rotacionDeseada =
                Quaternion.LookRotation(direccion, Vector3.up);

            float factor = 1f - Mathf.Exp(
                -suavizadoRotacion * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionDeseada,
                factor
            );
        }
    }
}

