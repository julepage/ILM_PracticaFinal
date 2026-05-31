using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //personajes
    public int personajeJugador1 = 0;
    public int personajeJugador2 = 0;
    public GameObject[] personajes;//aqui metemos los prefabs de coches distintos en orden

    void Awake()
    {
        //singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        //para salir luego de la build
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            //solo desde menu
            if (SceneManager.GetActiveScene().name == "Menu")
            {
                CambiarEscena("Arena");
            }
        }
    }

    //cambio de escena
    public void CambiarEscena(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }
}