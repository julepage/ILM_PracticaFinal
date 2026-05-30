using UnityEngine;

public class SelectorCoche : MonoBehaviour
{
    public GameObject[] coches;
    public Transform spawnPoint;

    public int numeroJugador;

    private int indiceActual = 0;
    private GameObject cocheActual;

    void Start()
    {
        if (coches == null || coches.Length == 0) return;
        ActualizarCoche();
    }

    public void Siguiente()
    {
        if (coches == null || coches.Length == 0) return;

        indiceActual = (indiceActual + 1) % coches.Length;
        ActualizarCoche();
    }

    public void Anterior()
    {
        if (coches == null || coches.Length == 0) return;

        indiceActual = (indiceActual - 1 + coches.Length) % coches.Length;
        ActualizarCoche();
    }

    void ActualizarCoche()
    {
        if (cocheActual != null)
            Destroy(cocheActual);

        cocheActual = Instantiate(coches[indiceActual], spawnPoint.parent);
        cocheActual.transform.localPosition = Vector3.zero;
       // cocheActual.transform.localRotation = Quaternion.identity;

        if (numeroJugador == 1)
        {
            GameManager.instance.personajeJugador1 = indiceActual;
        }
        else if (numeroJugador == 2)
        {
            GameManager.instance.personajeJugador1 = indiceActual;
        }
    }
}