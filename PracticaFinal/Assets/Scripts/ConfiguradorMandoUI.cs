using UnityEngine;
using TMPro;

public class ConfiguradorMandosUI : MonoBehaviour
{
    [Header("Referencias de la UI")]
    public TMP_Text textoIndicador;
    public GameObject panelConfigurador;

    private int pasoAsignacion = 1;

    void Start()
    {
        if (panelConfigurador != null) panelConfigurador.SetActive(true);
        ActualizarTextoUI("JUGADOR 1: Pulsa 'A' en tu mando");
    }

    void Update()
    {
        if (pasoAsignacion > 2) return;

        // ESCANEAMOS LOS 8 PUERTOS (Crucial para detectar mandos en cualquier PC)
        for (int i = 1; i <= 8; i++)
        {
            if (Input.GetKeyDown($"joystick {i} button 0"))
            {
                // Evitamos que el J2 elija el mismo mando que el J1
                if (pasoAsignacion == 2 && i == DatosControladores.idFisicoJ1) continue;

                ProcesarAsignacion(i);
                break;
            }
        }
    }

    void ProcesarAsignacion(int idMandoDetectado)
    {
        if (pasoAsignacion == 1)
        {
            DatosControladores.idFisicoJ1 = idMandoDetectado;
            Debug.Log($"Mando físico {idMandoDetectado} asignado al Jugador 1");

            pasoAsignacion = 2;
            ActualizarTextoUI("JUGADOR 2: Pulsa 'A' en tu mando");
        }
        else if (pasoAsignacion == 2)
        {
            DatosControladores.idFisicoJ2 = idMandoDetectado;
            Debug.Log($"Mando físico {idMandoDetectado} asignado al Jugador 2");

            pasoAsignacion = 3;
            FinalizarConfiguracion();
        }
    }

    void FinalizarConfiguracion()
    {
        ActualizarTextoUI("¡Mandos Listos!");
        if (panelConfigurador != null)
            panelConfigurador.SetActive(false);
    }

    void ActualizarTextoUI(string mensaje)
    {
        if (textoIndicador != null)
            textoIndicador.text = mensaje;
    }
}