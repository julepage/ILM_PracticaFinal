using UnityEngine;

public class MovimientoCoche : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Referencias F�sicas")]
    public WheelCollider wcDelanteraIzquierda;
    public WheelCollider wcDelanteraDerecha;
    public WheelCollider wcTraseraIzquierda;
    public WheelCollider wcTraseraDerecha;

    [Header("Referencias Visuales (Ruedas)")]
    public Transform meshDelanteraIzquierda;
    public Transform meshDelanteraDerecha;
    public Transform meshTraseraIzquierda;
    public Transform meshTraseraDerecha;

    [Header("Referencias del Interior (Habit�culo)")]
    public Transform mallaVolante;
    public Transform mallaVarillaVelocidad;

    [Header("Configuraci�n del Motor")]
    public float fuerzaMotorAdelante = 1250f;
    public float fuerzaMotorAtras = 500f;
    public float maxAnguloGiro = 40f;
    public float fuerzaFreno = 4000f;
    public Vector3 centroDeMasaOffset = new Vector3(0f, -0.5f, 0f);

    [Header("Ajustes de Rotaci�n Interior")]
    public float multiplicadorGiroVolante = 10f; 
    public float maxVelocidadTablero = 200f;     
    public float maxGiroVarilla = 180f;         
    public Vector3 offsetRotacionRueda = new Vector3(0f, 0f, 90f);

    private Quaternion rotInicialVolante;
    private Quaternion rotInicialVarillas;

    private float inputAcelerar;
    private float inputGiro;
    private bool estaFrenando;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centroDeMasaOffset;

        rotInicialVolante = Quaternion.Euler(0f, -62.5f, -90f);
        rotInicialVarillas = Quaternion.Euler(0f, 90f, -90f);
    }

    void Update()
    {
        inputAcelerar = Input.GetAxis("Vertical");
        inputGiro = Input.GetAxis("Horizontal");

        estaFrenando = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0);

        ActualizarVisualRueda(wcDelanteraIzquierda, meshDelanteraIzquierda);
        ActualizarVisualRueda(wcDelanteraDerecha, meshDelanteraDerecha);
        ActualizarVisualRueda(wcTraseraIzquierda, meshTraseraIzquierda);
        ActualizarVisualRueda(wcTraseraDerecha, meshTraseraDerecha);

        ActualizarVolante();

        ActualizarVelocimetro();
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

    void ActualizarVolante()
    {
        if (mallaVolante == null) return;

        float anguloRuedas = wcDelanteraIzquierda.steerAngle;

        float giroVolante = anguloRuedas * multiplicadorGiroVolante;

        mallaVolante.localRotation = rotInicialVolante * Quaternion.Euler(0f, 0f, giroVolante);
    }

    void ActualizarVelocimetro()
    {
        if (mallaVarillaVelocidad == null) return;

        float velocidadKMH = rb.linearVelocity.magnitude * 3.6f;

        float porcentajeVelocidad = Mathf.InverseLerp(0f, maxVelocidadTablero, velocidadKMH);

        float rotacionAguja = porcentajeVelocidad * maxGiroVarilla;

        mallaVarillaVelocidad.localRotation = rotInicialVarillas * Quaternion.Euler(0f, 0f, -rotacionAguja);
    }
}