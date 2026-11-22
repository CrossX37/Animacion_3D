
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class ObjetoCinematico : MonoBehaviour
{
    [Header("Cinematica del objeto")]
    [SerializeField] private VideoClip video;

    [Header("Configuracion")]
    [SerializeField] private bool unaSolaVez = true;

    [Header("Acciones al finalizar")]
    [SerializeField] private UnityEvent alFinalizar;

    private bool completado = false;

    public VideoClip Video => video;

    public bool Disponible =>
        video != null && (!unaSolaVez || !completado);

    public void NotificarFin()
    {
        completado = true;
        alFinalizar?.Invoke();
    }
}
