using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class SoftMeshLight : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody rb;

    public float minImpactImpulse = 0.5f;
    public float strength = 0.05f;
    public float maxDistance = 1.0f;
    public bool updateCollider = true;
    public LayerMask deformLayers;
    public bool reset = false;

    MeshFilter mf;
    MeshCollider mc;
    Mesh mesh;

    Vector3[] originalVertices;
    Vector3[] deformedVertices;

    volatile bool hasResult;
    Vector3[] resultVertices;

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();

        mesh = Instantiate(mf.sharedMesh);
        mf.sharedMesh = mesh;

        originalVertices = mesh.vertices;
        deformedVertices = (Vector3[])originalVertices.Clone();

        mc.sharedMesh = mesh;
    }

    void Update()
    {
        if (reset)
        {
            deformedVertices = (Vector3[])originalVertices.Clone();
            ApplyToMesh(deformedVertices);
            reset = false;
        }

        if (hasResult)
        {
            hasResult = false;
            deformedVertices = resultVertices;
            ApplyToMesh(deformedVertices);
        }
    }

    public void DeformMeshAsync(Vector3 impactPointWS, Vector3 impactNormalWS, float strength, float maxDistance)
    {
        Matrix4x4 l2w = transform.localToWorldMatrix;
        Matrix4x4 w2l = transform.worldToLocalMatrix;
        Vector3 nWS = impactNormalWS.normalized;

        Vector3[] baseVerts = (Vector3[])deformedVertices.Clone();

        Task.Run(() =>
        {
            for (int i = 0; i < baseVerts.Length; i++)
            {
                Vector3 vWS = l2w.MultiplyPoint3x4(baseVerts[i]);
                float dist = Vector3.Distance(vWS, impactPointWS);
                if (dist > maxDistance) continue;

                float t = Mathf.Clamp01(dist / maxDistance);
                float w = Mathf.SmoothStep(1f, 0f, t);

                Vector3 vWSDef = vWS - nWS * (strength * w);

                baseVerts[i] = w2l.MultiplyPoint3x4(vWSDef);
            }

            resultVertices = baseVerts;
            hasResult = true;
        });
    }



    void ApplyToMesh(Vector3[] verts)
    {
        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (updateCollider)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }
    }


}