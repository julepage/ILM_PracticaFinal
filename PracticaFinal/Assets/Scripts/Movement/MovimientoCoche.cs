using UnityEngine;

public class MovimientoCoche : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Referencias Fisicas")]
    public WheelCollider wcDelanteraIzquierda;
    public WheelCollider wcDelanteraDerecha;
    public WheelCollider wcTraseraIzquierda;
    public WheelCollider wcTraseraDerecha;

    [Header("Referencias Visuales (Ruedas)")]
    public Transform meshDelanteraIzquierda;
    public Transform meshDelanteraDerecha;
    public Transform meshTraseraIzquierda;
    public Transform meshTraseraDerecha;

    [Header("Referencias del Interior (Habitaculo)")]
    public Transform mallaVolante;

    [Header("Configuracion Arcade (Rocket League Style)")]
    public float fuerzaMotorAdelante = 3500f;
    public float fuerzaMotorAtras = 2000f;
    public float maxAnguloGiro = 40f;
    public float fuerzaFrenoDePie = 40000f;
    public float fuerzaFrenoMano = 80000f;
    public Vector3 centroDeMasaOffset = new Vector3(0f, -1.2f, 0f);

    [Header("Ajustes de Rotacion Interior")]
    public float multiplicadorGiroVolante = 10f;
    public Vector3 offsetRotacionRueda = new Vector3(0f, 0f, 90f);

    private Quaternion rotInicialVolante;
    private float inputAcelerar;
    private float inputGiro;
    private bool estaFrenando;
    private bool estaDerrapando;
    private float pesoLateral = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centroDeMasaOffset;

        rotInicialVolante = Quaternion.Euler(27.5f, 90f, 90f);
    }

    void Update()
    {
        inputAcelerar = Input.GetAxis("Vertical");
        inputGiro = Input.GetAxis("Horizontal");

        estaFrenando = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0);
        estaDerrapando = Input.GetKey(KeyCode.LeftShift);

        ActualizarVisualRueda(wcDelanteraIzquierda, meshDelanteraIzquierda);
        ActualizarVisualRueda(wcDelanteraDerecha, meshDelanteraDerecha);
        ActualizarVisualRueda(wcTraseraIzquierda, meshTraseraIzquierda);
        ActualizarVisualRueda(wcTraseraDerecha, meshTraseraDerecha);

        ActualizarVolante();
    }

    void FixedUpdate()
    {
        float velocidadLocalZ = transform.InverseTransformDirection(rb.linearVelocity).z;
        float velocidadMS = rb.linearVelocity.magnitude;
        float velocidadKMH = velocidadMS * 3.6f;
        bool tocandoSuelo = wcDelanteraIzquierda.isGrounded || wcDelanteraDerecha.isGrounded || wcTraseraIzquierda.isGrounded || wcTraseraDerecha.isGrounded;

        ActualizarFriccionesArcade();

        float frenoDelantero = 0f;
        float frenoTrasero = 0f;
        float torqueMotor = 0f;

        if (tocandoSuelo)
        {
            if (estaFrenando)
            {
                frenoDelantero = fuerzaFrenoMano;
                frenoTrasero = fuerzaFrenoMano;

                Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
                localVel.z = Mathf.MoveTowards(localVel.z, 0f, Time.fixedDeltaTime * 55f);
                rb.linearVelocity = transform.TransformDirection(localVel);
            }
            else
            {
                if (inputAcelerar < 0f && velocidadLocalZ > 0.2f)
                {
                    Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
                    localVel.z = Mathf.MoveTowards(localVel.z, 0f, Time.fixedDeltaTime * 65f);
                    rb.linearVelocity = transform.TransformDirection(localVel);
                    frenoDelantero = fuerzaFrenoDePie;
                    frenoTrasero = fuerzaFrenoDePie;
                }
                else if (inputAcelerar > 0f && velocidadLocalZ < -0.2f)
                {
                    Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
                    localVel.z = Mathf.MoveTowards(localVel.z, 0f, Time.fixedDeltaTime * 65f);
                    rb.linearVelocity = transform.TransformDirection(localVel);
                    frenoDelantero = fuerzaFrenoDePie;
                    frenoTrasero = fuerzaFrenoDePie;
                }
                else
                {
                    if (inputAcelerar > 0f)
                    {
                        torqueMotor = inputAcelerar * fuerzaMotorAdelante;
                    }
                    else if (inputAcelerar < 0f)
                    {
                        torqueMotor = inputAcelerar * fuerzaMotorAtras;
                    }
                }
            }
        }

        wcTraseraIzquierda.brakeTorque = frenoTrasero;
        wcTraseraDerecha.brakeTorque = frenoTrasero;
        wcDelanteraIzquierda.brakeTorque = frenoDelantero;
        wcDelanteraDerecha.brakeTorque = frenoDelantero;

        wcTraseraIzquierda.motorTorque = torqueMotor;
        wcTraseraDerecha.motorTorque = torqueMotor;

        float anguloGiroActual = maxAnguloGiro;
        if (estaDerrapando)
        {
            anguloGiroActual *= 1.4f;
        }
        else if (velocidadLocalZ < -0.5f)
        {
            anguloGiroActual *= 0.6f;
        }

        float anguloActual = inputGiro * anguloGiroActual;
        wcDelanteraIzquierda.steerAngle = anguloActual;
        wcDelanteraDerecha.steerAngle = anguloActual;

        if (estaDerrapando && tocandoSuelo)
        {
            if (inputGiro != 0f)
            {
                float factorGiroMarchaAtras = (velocidadLocalZ >= -0.5f) ? 1f : 0.2f;
                rb.AddTorque(transform.up * inputGiro * 28000f * factorGiroMarchaAtras);
            }
            else
            {
                Vector3 angVel = rb.angularVelocity;
                angVel.y = Mathf.MoveTowards(angVel.y, 0f, Time.fixedDeltaTime * 20f);
                rb.angularVelocity = angVel;
            }

            if (inputAcelerar > 0f)
            {
                rb.AddForce(transform.forward * inputAcelerar * 15000f);
            }
        }

        float velocidadFactor = Mathf.InverseLerp(0f, 140f, velocidadKMH);
        float targetPeso = inputGiro * velocidadFactor;
        pesoLateral = Mathf.MoveTowards(pesoLateral, targetPeso, Time.fixedDeltaTime * 3.5f);

        if (!estaDerrapando && velocidadKMH > 110f && tocandoSuelo)
        {
            rb.AddTorque(transform.forward * pesoLateral * velocidadKMH * 580f);
        }

        Vector3 velocidadAngular = rb.angularVelocity;
        velocidadAngular.y = Mathf.Clamp(velocidadAngular.y, -3.0f, 3.0f);
        rb.angularVelocity = velocidadAngular;
    }

    void ActualizarFriccionesArcade()
    {
        float forwardStiff = estaDerrapando ? 1.5f : 3.5f;
        float sidewaysStiffDelantero = estaDerrapando ? 0.6f : 4.0f;
        float sidewaysStiffTrasero = estaDerrapando ? 0.5f : 4.0f;

        ConfigurarCurvaFriccion(wcDelanteraIzquierda, forwardStiff, sidewaysStiffDelantero, estaDerrapando);
        ConfigurarCurvaFriccion(wcDelanteraDerecha, forwardStiff, sidewaysStiffDelantero, estaDerrapando);
        ConfigurarCurvaFriccion(wcTraseraIzquierda, forwardStiff, sidewaysStiffTrasero, estaDerrapando);
        ConfigurarCurvaFriccion(wcTraseraDerecha, forwardStiff, sidewaysStiffTrasero, estaDerrapando);
    }

    void ConfigurarCurvaFriccion(WheelCollider wc, float forwardStiff, float sidewaysStiff, bool derrapando)
    {
        WheelFrictionCurve f = wc.forwardFriction;
        f.extremumSlip = 0.15f;
        f.extremumValue = derrapando ? 1.0f : 1.6f;
        f.asymptoteSlip = 0.4f;
        f.asymptoteValue = derrapando ? 0.8f : 1.1f;
        f.stiffness = forwardStiff;
        wc.forwardFriction = f;

        WheelFrictionCurve s = wc.sidewaysFriction;
        s.extremumSlip = 0.18f;
        s.extremumValue = derrapando ? 1.0f : 1.8f;
        s.asymptoteSlip = 0.45f;
        s.asymptoteValue = derrapando ? 0.8f : 1.2f;
        s.stiffness = sidewaysStiff;
        wc.sidewaysFriction = s;
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

        mallaVolante.localRotation = rotInicialVolante * Quaternion.Euler(giroVolante, 0f, 0f);
    }
}