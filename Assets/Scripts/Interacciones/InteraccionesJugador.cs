
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InteraccionJugador : MonoBehaviour
{
    [Header("Sistema de Cinematicas")]
    public GestorCinematicas gestor;

    [Header("Distancia de Interaccion")]
    public float distanciaInteraccion = 2.5f;

    [Header("Interfaz")]
    public GameObject textoInteraccion;

    void Start()
    {
        MostrarTexto(false);
    }

    void Update()
    {
        if (gestor == null || gestor.Ocupado)
        {
            MostrarTexto(false);
            return;
        }

        // Buscar constantemente objetos cercanos
        ObjetoCinematico objeto = BuscarObjeto();

        // Mostrar texto si hay un objeto disponible
        MostrarTexto(objeto != null);

        // Interactuar al presionar E
        if (objeto != null && PresionoE())
        {
            MostrarTexto(false);

            Debug.Log("Interactuando con: " + objeto.name);

            gestor.Reproducir(objeto);
        }
    }

    void MostrarTexto(bool mostrar)
    {
        if (textoInteraccion != null &&
            textoInteraccion.activeSelf != mostrar)
        {
            textoInteraccion.SetActive(mostrar);
        }
    }

    bool PresionoE()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }

    ObjetoCinematico BuscarObjeto()
    {
        Collider[] objetos = Physics.OverlapSphere(
            transform.position,
            distanciaInteraccion,
            ~0,
            QueryTriggerInteraction.Collide
        );

        ObjetoCinematico cercano = null;
        float distanciaMinima = float.MaxValue;

        foreach (Collider col in objetos)
        {
            ObjetoCinematico objeto =
                col.GetComponentInParent<ObjetoCinematico>();

            if (objeto == null || !objeto.Disponible)
                continue;

            float distancia = (
                col.ClosestPoint(transform.position)
                - transform.position
            ).sqrMagnitude;

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                cercano = objeto;
            }
        }

        return cercano;
    }

    void OnDisable()
    {
        MostrarTexto(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            distanciaInteraccion
        );
    }
}
