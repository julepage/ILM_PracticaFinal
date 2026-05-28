using UnityEngine;

public class SphereDent : MonoBehaviour
{
    public Material mat;

    public int maxImpacts = 8;

    Vector4[] pos = new Vector4[8];
    float[] radius = new float[8];
    float[] strength = new float[8];

    int index = 0;
    int count = 0;

    public float r = 1f;
    public float s = 0.3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Vector3 hit = transform.position + Random.onUnitSphere * 0.5f;

            pos[index] = hit;
            radius[index] = r;
            strength[index] = s;

            index = (index + 1) % maxImpacts;
            count = Mathf.Min(count + 1, maxImpacts);

            mat.SetInt("_ImpactCount", count);
            mat.SetVectorArray("_ImpactPos", pos);
            mat.SetFloatArray("_Radius", radius);
            mat.SetFloatArray("_Strength", strength);
        }
    }
}