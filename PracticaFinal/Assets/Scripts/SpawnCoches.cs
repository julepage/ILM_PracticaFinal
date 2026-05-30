using UnityEngine;

public class SpawnCoches : MonoBehaviour
{
    void Start()
    {
        GameManager gm = GameManager.instance;

        //jugador 1
        GameObject p1 = Instantiate(gm.personajes[gm.personajeJugador1], new Vector3(-1.2f, 0.429f, -50.2f), Quaternion.identity);

        //jugador 2
        GameObject p2 = Instantiate(gm.personajes[gm.personajeJugador2], new Vector3(-1.5f, 0.43f, 52.79f), new Quaternion(0, -180.0f, 0,1));

        Camera cam1 = p1.GetComponentInChildren<Camera>();
        Camera cam2 = p2.GetComponentInChildren<Camera>();

        cam1.rect = new Rect(0f, 0f, 0.5f, 1f);//izquierda
        cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);//derecha

        // PLAYER 1 (WASD)
        p1.GetComponentInChildren<InputCocheWASD>().enabled = true;
        p1.GetComponentInChildren<InputCocheFlechas>().enabled = false;

        // PLAYER 2 (FLECHAS)
        p2.GetComponentInChildren<InputCocheWASD>().enabled = false;
        p2.GetComponentInChildren<InputCocheFlechas>().enabled = true;
    }
}