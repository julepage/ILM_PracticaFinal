using UnityEngine;
using System.Collections.Generic;

public class CollisionCar : MonoBehaviour
{
    [Header("Configuración Base")]
    public float baseRadius = 0.5f;
    public float baseStrength = 0.15f;
    public float minDamageForce = 5f;
    public float maxStrengthLimit = 0.6f;

    [Header("Ajustes de Realismo")]
    [Range(0.1f, 2f)] public float shockwaveExpansion = 0.5f;
    [Range(0.1f, 1f)] public float fusionDistanceFactor = 0.7f;

    private List<Material> materialesHijos = new List<Material>();

    // Sincronizado exactamente con las 64 posiciones del shader
    private const int MAX_IMPACTS = 64;

    private List<Vector3> padreLocalImpacts = new List<Vector3>();
    private List<Vector3> padreLocalDirections = new List<Vector3>();
    private List<float> impactRadii = new List<float>();
    private List<float> impactStrengths = new List<float>();

    private Vector4[] worldPositionsArray = new Vector4[MAX_IMPACTS];
    private Vector4[] worldDirectionsArray = new Vector4[MAX_IMPACTS];
    private Vector4[] datosImpacto = new Vector4[MAX_IMPACTS];

    void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (rend != null && rend.material != null)
            {
                Material matInstance = rend.material;
                matInstance.SetInt("_ImpactCount", 0);
                materialesHijos.Add(matInstance);
            }
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.contactCount == 0 || materialesHijos.Count == 0) return;

        float collisionForce = c.relativeVelocity.magnitude;
        if (collisionForce < minDamageForce) return;

        float forceFactor = collisionForce / minDamageForce;

        float dynamicRadius = baseRadius * Mathf.Clamp(1f + (forceFactor * shockwaveExpansion), 1f, 3.5f);
        float dynamicStrength = Mathf.Min(baseStrength * forceFactor, maxStrengthLimit);

        ContactPoint contact = c.GetContact(0);
        Vector3 worldHitPoint = contact.point;
        Vector3 worldKeyDirection = contact.normal;

        Vector3 puntoLocalPadre = transform.InverseTransformPoint(worldHitPoint);
        Vector3 dirLocalPadre = transform.InverseTransformDirection(worldKeyDirection);

        bool golpeFusionado = false;
        float distanciaMinimaFusion = baseRadius * fusionDistanceFactor;

        // Buscamos si el golpe actual reactiva o empeora una abolladura existente
        for (int i = 0; i < padreLocalImpacts.Count; i++)
        {
            if (Vector3.Distance(padreLocalImpacts[i], puntoLocalPadre) < distanciaMinimaFusion)
            {
                impactStrengths[i] = Mathf.Min(impactStrengths[i] + (dynamicStrength * 0.4f), maxStrengthLimit);
                impactRadii[i] = Mathf.Max(impactRadii[i], dynamicRadius);
                padreLocalDirections[i] = (padreLocalDirections[i] + dirLocalPadre).normalized;

                golpeFusionado = true;
                break;
            }
        }

        // Si es un bollo nuevo en un área limpia del coche
        if (!golpeFusionado)
        {
            if (padreLocalImpacts.Count >= MAX_IMPACTS)
            {
                // Solo si el coche es golpeado en más de 64 sitios completamente distintos, 
                // empezará a reciclar la abolladura más vieja.
                padreLocalImpacts.RemoveAt(0);
                padreLocalDirections.RemoveAt(0);
                impactRadii.RemoveAt(0);
                impactStrengths.RemoveAt(0);
            }

            padreLocalImpacts.Add(puntoLocalPadre);
            padreLocalDirections.Add(dirLocalPadre);
            impactRadii.Add(dynamicRadius);
            impactStrengths.Add(dynamicStrength);
        }
    }

    void Update()
    {
        if (materialesHijos.Count == 0 || padreLocalImpacts.Count == 0) return;

        for (int i = 0; i < MAX_IMPACTS; i++)
        {
            if (i < padreLocalImpacts.Count)
            {
                Vector3 worldPos = transform.TransformPoint(padreLocalImpacts[i]);
                Vector3 worldDir = transform.TransformDirection(padreLocalDirections[i]);

                worldPositionsArray[i] = new Vector4(worldPos.x, worldPos.y, worldPos.z, 1f);
                worldDirectionsArray[i] = new Vector4(worldDir.x, worldDir.y, worldDir.z, 0f);
                datosImpacto[i] = new Vector4(impactRadii[i], impactStrengths[i], 0f, 0f);
            }
            else
            {
                worldPositionsArray[i] = Vector4.zero;
                worldDirectionsArray[i] = Vector4.zero;
                datosImpacto[i] = Vector4.zero;
            }
        }

        foreach (Material mat in materialesHijos)
        {
            mat.SetInt("_ImpactCount", padreLocalImpacts.Count);
            mat.SetVectorArray("_ImpactPos", worldPositionsArray);
            mat.SetVectorArray("_ImpactDir", worldDirectionsArray);
            mat.SetVectorArray("_ImpactData", datosImpacto);
        }
    }
}