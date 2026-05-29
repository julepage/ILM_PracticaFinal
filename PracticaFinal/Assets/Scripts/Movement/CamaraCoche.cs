using UnityEngine;

public class CamaraCoche : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetivo; // el coche

    [Header("Offsets")]
    public Vector3 posicionExterior = new Vector3(0f, 6.43f, -7.99f);
    public Vector3 posicionInterior = new Vector3(-0.68f, 1.33f, 0.5f);

    public Vector3 rotacionExterior = new Vector3(10f, 0f, 0f);
    public Vector3 rotacionInterior = new Vector3(0f, 0f, 0f);

    private bool vistaInterior = false;

    void LateUpdate()
    {
        if (objetivo == null) return;

        if (vistaInterior)
        {
            transform.position = objetivo.TransformPoint(posicionInterior);
            transform.rotation = objetivo.rotation * Quaternion.Euler(rotacionInterior);
        }
        else
        {
            transform.position = objetivo.TransformPoint(posicionExterior);
            transform.rotation = objetivo.rotation * Quaternion.Euler(rotacionExterior);
        }
    }

    public void CambiarVista()
    {
        vistaInterior = !vistaInterior;
    }
}