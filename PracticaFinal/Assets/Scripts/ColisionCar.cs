using UnityEngine;

public class CollisionCar : MonoBehaviour
{
    public Material mat;
    public float radius = 1f;
    public float strength = 0.2f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 hitPoint = transform.position + transform.forward * 2f;

            mat.SetVector("_ImpactPos", hitPoint);
            mat.SetFloat("_Radius", radius);
            mat.SetFloat("_Strength", strength);
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.contactCount == 0) return;

        Vector3 p = c.GetContact(0).point;

        mat.SetVector("_ImpactPos", p);
        mat.SetFloat("_Radius", radius);
        mat.SetFloat("_Strength", strength);
    }
}