using UnityEngine;

public class ControlNiebla : MonoBehaviour
{
    public KeyCode teclaNiebla = KeyCode.F;

    public Material materialNiebla;

    public float distanciaConNiebla = 5000f;
    public float distanciaSinNiebla = 100000000f;

    private bool estaActiva = true;

    void Start()
    {
        if (materialNiebla == null)
        {
            enabled = false;
            return;
        }

        AplicarDistancia(distanciaConNiebla);
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaNiebla))
        {
            estaActiva = !estaActiva;

            if (estaActiva)
            {
                AplicarDistancia(distanciaConNiebla);
            }
            else
            {
                AplicarDistancia(distanciaSinNiebla);
            }
        }
    }

    void AplicarDistancia(float valor)
    {
        materialNiebla.SetFloat("_DistanciaNiebla", valor);
    }
}