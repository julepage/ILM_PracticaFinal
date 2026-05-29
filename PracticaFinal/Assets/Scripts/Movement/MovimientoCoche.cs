using UnityEngine;
using UnityEngine.Audio;

public class MovimientoCoche : MonoBehaviour
{
    public AudioSource source;
    public AudioClip claxonClip;

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

    [Header("Configuracion Arcade")]
    public float fuerzaMotorAdelante = 3500f;
    public float fuerzaMotorAtras = 2000f;
    public float maxAnguloGiro = 40f;
    public float fuerzaFrenoDePie = 40000f;
    public float fuerzaFrenoMano = 80000f;
    public Vector3 centroDeMasaOffset = new Vector3(0f, -1.2f, 0f);

    [Header("Ajustes Visuales")]
    public float multiplicadorGiroVolante = 10f;
    public Vector3 offsetRotacionRueda = new Vector3(0f, 0f, 90f);

    private Quaternion rotInicialVolante;

    // INPUTS (externos)
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

   
    public void SetInput(float acelerar, float giro, bool freno, bool derrape)//input desde scripts input
    {
        inputAcelerar = acelerar;
        inputGiro = giro;
        estaFrenando = freno;
        estaDerrapando = derrape;
    }

    void Update()
    {
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

        bool tocandoSuelo =
            wcDelanteraIzquierda.isGrounded ||
            wcDelanteraDerecha.isGrounded ||
            wcTraseraIzquierda.isGrounded ||
            wcTraseraDerecha.isGrounded;

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
                if (inputAcelerar < 0f && velocidadLocalZ > 0.2f ||
                    inputAcelerar > 0f && velocidadLocalZ < -0.2f)
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
                        torqueMotor = inputAcelerar * fuerzaMotorAdelante;
                    else if (inputAcelerar < 0f)
                        torqueMotor = inputAcelerar * fuerzaMotorAtras;
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
            anguloGiroActual *= 1.4f;
        else if (velocidadLocalZ < -0.5f)
            anguloGiroActual *= 0.6f;

        float anguloActual = inputGiro * anguloGiroActual;

        wcDelanteraIzquierda.steerAngle = anguloActual;
        wcDelanteraDerecha.steerAngle = anguloActual;

        if (estaDerrapando && tocandoSuelo)
        {
            if (inputGiro != 0f)
            {
                float factor = (velocidadLocalZ >= -0.5f) ? 1f : 0.2f;
                rb.AddTorque(transform.up * inputGiro * 28000f * factor);
            }
            else
            {
                Vector3 angVel = rb.angularVelocity;
                angVel.y = Mathf.MoveTowards(angVel.y, 0f, Time.fixedDeltaTime * 20f);
                rb.angularVelocity = angVel;
            }

            if (inputAcelerar > 0f)
                rb.AddForce(transform.forward * inputAcelerar * 15000f);
        }

        float velocidadFactor = Mathf.InverseLerp(0f, 140f, velocidadKMH);
        float targetPeso = inputGiro * velocidadFactor;

        pesoLateral = Mathf.MoveTowards(pesoLateral, targetPeso, Time.fixedDeltaTime * 3.5f);

        if (!estaDerrapando && velocidadKMH > 110f && tocandoSuelo)
            rb.AddTorque(transform.forward * pesoLateral * velocidadKMH * 580f);

        Vector3 velAngular = rb.angularVelocity;
        velAngular.y = Mathf.Clamp(velAngular.y, -3f, 3f);
        rb.angularVelocity = velAngular;
    }

    void ActualizarFriccionesArcade()
    {
        float forwardStiff = estaDerrapando ? 1.5f : 3.5f;
        float sidewaysDel = estaDerrapando ? 0.6f : 4.0f;
        float sidewaysTra = estaDerrapando ? 0.5f : 4.0f;

        ConfigurarCurvaFriccion(wcDelanteraIzquierda, forwardStiff, sidewaysDel);
        ConfigurarCurvaFriccion(wcDelanteraDerecha, forwardStiff, sidewaysDel);
        ConfigurarCurvaFriccion(wcTraseraIzquierda, forwardStiff, sidewaysTra);
        ConfigurarCurvaFriccion(wcTraseraDerecha, forwardStiff, sidewaysTra);
    }

    void ConfigurarCurvaFriccion(WheelCollider wc, float forwardStiff, float sidewaysStiff)
    {
        WheelFrictionCurve f = wc.forwardFriction;
        f.extremumSlip = 0.15f;
        f.extremumValue = estaDerrapando ? 1.0f : 1.6f;
        f.asymptoteSlip = 0.4f;
        f.asymptoteValue = estaDerrapando ? 0.8f : 1.1f;
        f.stiffness = forwardStiff;
        wc.forwardFriction = f;

        WheelFrictionCurve s = wc.sidewaysFriction;
        s.extremumSlip = 0.18f;
        s.extremumValue = estaDerrapando ? 1.0f : 1.8f;
        s.asymptoteSlip = 0.45f;
        s.asymptoteValue = estaDerrapando ? 0.8f : 1.2f;
        s.stiffness = sidewaysStiff;
        wc.sidewaysFriction = s;
    }

    void ActualizarVisualRueda(WheelCollider col, Transform mesh)
    {
        if (mesh == null) return;

        col.GetWorldPose(out Vector3 pos, out Quaternion rot);

        mesh.position = pos;
        mesh.rotation = rot * Quaternion.Euler(offsetRotacionRueda);
    }

    void ActualizarVolante()
    {
        if (mallaVolante == null) return;

        float giroVolante = wcDelanteraIzquierda.steerAngle * multiplicadorGiroVolante;
        mallaVolante.localRotation = rotInicialVolante * Quaternion.Euler(giroVolante, 0f, 0f);
    }

    public void TocarClaxon()
    {
        if (source == null || claxonClip == null) return;

       source.PlayOneShot(claxonClip);
    }
}