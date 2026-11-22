
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class GestorCinematicas : MonoBehaviour
{
    [Header("Camara principal")]
    [SerializeField] private Camera camaraPrincipal;

    [Header("Controles que se bloquearan")]
    [SerializeField] private MonoBehaviour[] controles;

    [Header("Transicion cinematografica")]
    [SerializeField] private CanvasGroup pantallaNegra;
    [SerializeField] private float duracionFundido = 0.7f;
    [SerializeField] private float margenSalida = 0.2f;
    [SerializeField] private GameObject canvasFondoNegro;

    private VideoPlayer reproductor;
    private ObjetoCinematico objetoActual;
    private bool[] estadosAnteriores;

    private bool videoTerminado;
    private bool errorVideo;

    public bool Ocupado { get; private set; }

    void Awake()
    {
        reproductor = GetComponent<VideoPlayer>();

        if (camaraPrincipal == null)
            camaraPrincipal = Camera.main;

        reproductor.playOnAwake = false;
        reproductor.isLooping = false;
        reproductor.source = VideoSource.VideoClip;

        // Sistema original: video directamente en la camara
        reproductor.renderMode = VideoRenderMode.CameraNearPlane;
        reproductor.targetCamera = camaraPrincipal;
        reproductor.targetCameraAlpha = 1f;

        if (pantallaNegra != null)
        {
            pantallaNegra.alpha = 0f;
            pantallaNegra.blocksRaycasts = false;
            pantallaNegra.interactable = false;
        }
        // NUEVO: ocultar el fondo negro al iniciar
        if (canvasFondoNegro != null)
        {
            canvasFondoNegro.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (reproductor == null)
            reproductor = GetComponent<VideoPlayer>();

        reproductor.loopPointReached += VideoFinalizado;
        reproductor.errorReceived += ErrorReproduccion;
    }

    void OnDisable()
    {
        if (reproductor != null)
        {
            reproductor.loopPointReached -= VideoFinalizado;
            reproductor.errorReceived -= ErrorReproduccion;
        }

        StopAllCoroutines();

        if (Ocupado)
        {
            reproductor.Stop();
            RestaurarControles();

            Ocupado = false;
            objetoActual = null;
        }

        if (pantallaNegra != null)
            pantallaNegra.alpha = 0f;
    }

    public void Reproducir(ObjetoCinematico objeto)
    {
        if (Ocupado || objeto == null || !objeto.Disponible)
            return;

        if (camaraPrincipal == null || pantallaNegra == null)
        {
            Debug.LogError(
                "Falta asignar la camara o PantallaNegra."
            );
            return;
        }

        Ocupado = true;
        objetoActual = objeto;

        videoTerminado = false;
        errorVideo = false;

        int cantidad = controles != null ? controles.Length : 0;
        estadosAnteriores = new bool[cantidad];

        // Bloquear los controles del personaje
        for (int i = 0; i < cantidad; i++)
        {
            if (controles[i] == null)
                continue;

            estadosAnteriores[i] = controles[i].enabled;
            controles[i].enabled = false;
        }

        StartCoroutine(SecuenciaCinematica());
    }

    IEnumerator SecuenciaCinematica()
    {
        reproductor.clip = objetoActual.Video;
        reproductor.Prepare();

        // 1. Oscurecer el juego
        yield return Fundir(1f);

        // 2. Esperar que se prepare el video
        while (!reproductor.isPrepared && !errorVideo)
            yield return null;

        if (!errorVideo)
        {
            // 3. Reproducir video
            if (canvasFondoNegro != null)
                canvasFondoNegro.SetActive(true);

            reproductor.Play();

            // Retirar gradualmente el negro
            yield return Fundir(0f);

            // 4. Esperar hasta cerca del final
            double inicioSalida = reproductor.length > 0
                ? Math.Max(
                    0.0,
                    reproductor.length
                    - duracionFundido
                    - margenSalida
                )
                : double.PositiveInfinity;

            while (!videoTerminado &&
                   !errorVideo &&
                   reproductor.time < inicioSalida)
            {
                yield return null;
            }

            // 5. Fundido a negro
            yield return Fundir(1f);
        }

        // 6. Detener el video
        reproductor.Stop();
        if (canvasFondoNegro != null)
            canvasFondoNegro.SetActive(false);
        // IMPORTANTE: ejecutar eventos del objeto
        if (!errorVideo && objetoActual != null)
        {
            objetoActual.NotificarFin();
        }

        // 7. Mostrar nuevamente el juego
        yield return Fundir(0f);

        // 8. Restaurar los controles de Bonny
        RestaurarControles();

        objetoActual = null;
        Ocupado = false;
    }

    IEnumerator Fundir(float alphaFinal)
    {
        float alphaInicial = pantallaNegra.alpha;
        float tiempo = 0f;

        float duracion = Mathf.Max(
            0.01f,
            duracionFundido
        );

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracion
            );

            pantallaNegra.alpha = Mathf.Lerp(
                alphaInicial,
                alphaFinal,
                progreso
            );

            yield return null;
        }

        pantallaNegra.alpha = alphaFinal;
    }

    void VideoFinalizado(VideoPlayer vp)
    {
        videoTerminado = true;
    }

    void ErrorReproduccion(VideoPlayer vp, string error)
    {
        Debug.LogError(
            "Error en cinematica: " + error
        );

        errorVideo = true;
    }

    void RestaurarControles()
    {
        if (estadosAnteriores == null)
            return;

        for (int i = 0; i < estadosAnteriores.Length; i++)
        {
            if (controles[i] != null)
            {
                controles[i].enabled = estadosAnteriores[i];
            }
        }

        estadosAnteriores = null;
    }
}
