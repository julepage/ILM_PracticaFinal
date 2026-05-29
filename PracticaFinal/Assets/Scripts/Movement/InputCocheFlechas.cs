using UnityEngine;

public class InputCocheFlechas : MonoBehaviour
{
    public MovimientoCoche coche;
    public CamaraCoche camara;
    void Update()
    {
        float acelerar = 0f;
        float giro = 0f;

        if (Input.GetKey(KeyCode.UpArrow)) acelerar = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) acelerar = -1f;
        if (Input.GetKey(KeyCode.LeftArrow)) giro = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) giro = 1f;

        bool freno = Input.GetKey(KeyCode.RightControl);//Frenar ctrl drch
        bool derrape = Input.GetKey(KeyCode.RightShift);//derrapar shift drch
        if (Input.GetKeyDown(KeyCode.P))
            coche.TocarClaxon();
        if (Input.GetKeyDown(KeyCode.O))
            camara.CambiarVista();
        coche.SetInput(acelerar, giro, freno, derrape);
    }
}