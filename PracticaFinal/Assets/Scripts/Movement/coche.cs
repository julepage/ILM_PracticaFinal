using UnityEngine;
public class coche : MonoBehaviour
{
    public SoftMeshLight script;
    public float minImpactImpulse = 0.5f;
    public float strength = 0.05f;
    public float maxDistance = 1.0f;
    public bool updateCollider = true;
    public LayerMask deformLayers;
    public bool reset = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision c)
    {
        Debug.Log("COLISION DETECTADA: " + c.gameObject.name);

        if (c.contactCount == 0) return;

        float impulse = c.impulse.magnitude;

        if (impulse < minImpactImpulse) return;

        if ((deformLayers.value & (1 << c.gameObject.layer)) == 0)
            return;

        ContactPoint cp = c.GetContact(0);

        float dynamicStrength = strength * Mathf.Clamp(impulse * 0.1f, 1f, 5f);
        float dynamicDistance = maxDistance * Mathf.Clamp(impulse * 0.05f, 1f, 3f);

        script.DeformMeshAsync(
            cp.point,
            -cp.normal,
            dynamicStrength,
            dynamicDistance
        );
    }
}
