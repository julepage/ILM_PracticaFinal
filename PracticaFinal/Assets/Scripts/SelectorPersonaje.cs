using UnityEngine;
using UnityEngine.UI;

public class SelectorPersonaje : MonoBehaviour
{
    public Image imagenPersonaje;
    public Sprite[] personajes;
    public int numeroJugador;// 1 o 2

    private int indiceActual = 0;

    void Start()
    {
        ActualizarImagen();
    }

    public void siguiente()
    {
        if (personajes == null || personajes.Length == 0) return;
        indiceActual = (indiceActual + 1) % personajes.Length;//ciclo
        ActualizarImagen();
    }

    public void Anterior()
    {
        if (personajes == null || personajes.Length == 0) return;
        indiceActual = (indiceActual - 1 + personajes.Length) % personajes.Length;
        ActualizarImagen();
    }

    void ActualizarImagen()
    {
        imagenPersonaje.sprite = personajes[indiceActual];

        if (numeroJugador == 1)
        {
            GameManager.instance.personajeJugador1 = indiceActual;
        }
        else if (numeroJugador == 2)
        {
            GameManager.instance.personajeJugador2 = indiceActual;
        }
    }

}