using UnityEngine;

public class AsignadorRetrovisor : MonoBehaviour
{
    private Camera camaraEspejo;
    private RenderTexture texturaEspejo;

    [Header("Carpeta donde quieres que se cree la camara del retrovisor")]
    public Transform carpetaDondeCrearLaCamara;

    [Header("Material del espejo")]
    public Material materialEspejo;

    void Start()
    {
        //excepcion
        if (carpetaDondeCrearLaCamara == null || materialEspejo == null)
        {
            Debug.LogError("Arrastra material y carpeta en inspector.", this);
            return;
        }

        //render texture para camara
        texturaEspejo = new RenderTexture(1024, 512, 16, RenderTextureFormat.ARGB32);
        texturaEspejo.Create();

        //camara nueva para shader
        GameObject objetoCamara = new GameObject("CamaraTraseraRetrovisor");
        camaraEspejo = objetoCamara.AddComponent<Camera>();

        //pos camara un poco mal puesta pero va bien asi asi que
        objetoCamara.transform.SetParent(carpetaDondeCrearLaCamara);
        objetoCamara.transform.localPosition = new Vector3(-3.41f, 1.34f, -0.0099f);
        objetoCamara.transform.localRotation = Quaternion.Euler(10.0f, 180f, 0f);
        objetoCamara.transform.localScale = Vector3.one;

        //para que vaya como quiero la camara
        camaraEspejo.targetTexture = texturaEspejo;
        camaraEspejo.fieldOfView = 45f;//fov si no se ve bien 35
        camaraEspejo.clearFlags = CameraClearFlags.Skybox;

        //clono material paar q cada coche use uno y no se vea lo miismo en todos los retrovisores
        Material materialUnicoDelCoche = new Material(materialEspejo);

        //textura al material
        materialUnicoDelCoche.SetTexture("_ReflectionTex", texturaEspejo);

        //nuevo mat
        Renderer rendererEspejo = GetComponent<Renderer>();
        if (rendererEspejo != null)
        {
            //busco mats
            Material[] listadoMateriales = rendererEspejo.materials;
            for (int i = 0; i < listadoMateriales.Length; i++)
            {
                //espejo
                if (listadoMateriales[i].name.Contains(materialEspejo.name))
                {
                    //lo cambio por el bueno
                    listadoMateriales[i] = materialUnicoDelCoche;
                    break;
                }
            }
            //nuevo mat al coche
            rendererEspejo.materials = listadoMateriales;
        }
    }

    void OnDestroy()
    {
        //limpieza 
        if (texturaEspejo != null)
        {
            texturaEspejo.Release();
            Destroy(texturaEspejo);
        }
    }
}