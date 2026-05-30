using UnityEngine;

public class InputCocheMandoFinal : MonoBehaviour
{
    public MovimientoCoche coche;
    public CamaraCoche camara;

    [Header("Configuración del Prefab")]
    [Range(1, 2)] public int numeroJugador = 1;

    private int idMandoFisico = -1;
    private string sufijoInputManager = "";

    void Start()
    {
        // Recuperamos el puerto físico real detectado en el menú
        if (numeroJugador == 1)
        {
            idMandoFisico = DatosControladores.idFisicoJ1;
        }
        else if (numeroJugador == 2)
        {
            idMandoFisico = DatosControladores.idFisicoJ2;
        }

        if (idMandoFisico != -1)
        {
            sufijoInputManager = "_M" + idMandoFisico;
        }
    }

    void Update()
    {
        if (idMandoFisico == -1 || coche == null) return;

        // Ejes analógicos
        float evaluarAcelerar = Input.GetAxis("Acelerar" + sufijoInputManager);
        float evaluarGiro = Input.GetAxis("Giro" + sufijoInputManager);

        bool freno = Input.GetAxis("Freno" + sufijoInputManager) > 0.1f;
        bool derrape = Input.GetAxis("Derrape" + sufijoInputManager) > 0.1f;

        // ==========================================
        // ¡BOTONES INVERTIDOS AQUÍ PARA SOLUCIONAR EL FALLO!
        // ==========================================

        // Ahora el botón 0 (Botón A de Xbox) maneja la Cámara
        if (Input.GetKeyDown($"joystick {idMandoFisico} button 0") && camara != null)
            camara.CambiarVista();

        // Ahora el botón 9 (Pulsar Joystick Derecho / R3) toca el Claxon
        if (Input.GetKeyDown($"joystick {idMandoFisico} button 9"))
            coche.TocarClaxon();

        // ==========================================

        coche.SetInput(evaluarAcelerar, evaluarGiro, freno, derrape);
    }
}