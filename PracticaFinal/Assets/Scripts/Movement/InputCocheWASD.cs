using UnityEngine;

public class InputCocheWASD : MonoBehaviour
{
    public MovimientoCoche coche;
    public CamaraCoche camara;
    void Update()
    {
        float acelerar = 0f;
        float giro = 0f;

        if (Input.GetKey(KeyCode.W)) acelerar = 1f;
        if (Input.GetKey(KeyCode.S)) acelerar = -1f;
        if (Input.GetKey(KeyCode.A)) giro = -1f;
        if (Input.GetKey(KeyCode.D)) giro = 1f;

        bool freno = Input.GetKey(KeyCode.Space);//frenar espacio
        bool derrape = Input.GetKey(KeyCode.LeftShift);//Derrapar shift izq
        if (Input.GetKeyDown(KeyCode.E))
            coche.TocarClaxon();

        if (Input.GetKeyDown(KeyCode.C))
            camara.CambiarVista();

        coche.SetInput(acelerar, giro, freno, derrape);
    }
}