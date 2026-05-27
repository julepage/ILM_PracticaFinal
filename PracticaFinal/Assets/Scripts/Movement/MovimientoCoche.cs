using UnityEngine;

public class MovimientoCoche : MonoBehaviour
{
    private Rigidbody rb;

    public WheelCollider wcDelanteraIzquierda;
    public WheelCollider wcDelanteraDerecha;
    public WheelCollider wcTraseraIzquierda;
    public WheelCollider wcTraseraDerecha;

    public Transform meshDelanteraIzquierda;
    public Transform meshDelanteraDerecha;
    public Transform meshTraseraIzquierda;
    public Transform meshTraseraDerecha;

    [Header("Configuración del Motor")]
    public float fuerzaMotorAdelante = 1250f; 
    public float fuerzaMotorAtras = 500f;   
    public float maxAnguloGiro = 40f;
    public float fuerzaFreno = 4000f;
    public Vector3 centroDeMasaOffset = new Vector3(0f, -0.5f, 0f);

    public Vector3 offsetRotacionRueda = new Vector3(0f, 0f, 90f);

    private float inputAcelerar;
    private float inputGiro;
    private bool estaFrenando;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centroDeMasaOffset;
    }

    void Update()
    {
        inputAcelerar = Input.GetAxis("Vertical");
        inputGiro = Input.GetAxis("Horizontal");

        estaFrenando = Input.GetKey(KeyCode.Space);

        ActualizarVisualRueda(wcDelanteraIzquierda, meshDelanteraIzquierda);
        ActualizarVisualRueda(wcDelanteraDerecha, meshDelanteraDerecha);
        ActualizarVisualRueda(wcTraseraIzquierda, meshTraseraIzquierda);
        ActualizarVisualRueda(wcTraseraDerecha, meshTraseraDerecha);
    }

    void FixedUpdate()
    {
        if (estaFrenando)
        {
            wcTraseraIzquierda.brakeTorque = fuerzaFreno;
            wcTraseraDerecha.brakeTorque = fuerzaFreno;
            wcDelanteraIzquierda.brakeTorque = fuerzaFreno;
            wcDelanteraDerecha.brakeTorque = fuerzaFreno;

            wcTraseraIzquierda.motorTorque = 0f;
            wcTraseraDerecha.motorTorque = 0f;
        }
        else
        {
            wcTraseraIzquierda.brakeTorque = 0f;
            wcTraseraDerecha.brakeTorque = 0f;
            wcDelanteraIzquierda.brakeTorque = 0f;
            wcDelanteraDerecha.brakeTorque = 0f;

            float torqueActual = 0f;

            if (inputAcelerar > 0f)
            {
                torqueActual = inputAcelerar * fuerzaMotorAdelante;
            }
            else if (inputAcelerar < 0f)
            {
                torqueActual = inputAcelerar * fuerzaMotorAtras;
            }

            wcTraseraIzquierda.motorTorque = torqueActual;
            wcTraseraDerecha.motorTorque = torqueActual;
        }

        float anguloActual = inputGiro * maxAnguloGiro;
        wcDelanteraIzquierda.steerAngle = anguloActual;
        wcDelanteraDerecha.steerAngle = anguloActual;
    }

    void ActualizarVisualRueda(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;

        Vector3 posicion;
        Quaternion rotacion;

        col.GetWorldPose(out posicion, out rotacion);

        mesh.position = posicion;
        mesh.rotation = rotacion * Quaternion.Euler(offsetRotacionRueda);
    }
}