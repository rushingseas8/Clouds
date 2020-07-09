using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CloudController : MonoBehaviour
{
    [Tooltip("The GameObject that we'll use for the cloud")]
    public GameObject CloudLayer;
    private Material CloudLayerMaterial;

    [Tooltip("The GameObject that's being used for the cloud shadows")]
    public GameObject CloudLayerShadow;
    private Material CloudLayerShadowMaterial;

    [Tooltip("X offset for the texture. Used for wind.")]
    public float OffsetX;
    [Tooltip("Y offset for the texture. Used for wind.")]
    public float OffsetY;

    [Tooltip("How dense should the clouds be? 0 = clear, 1 = overcast. Default 0.5.")]
    [Range(0.0f, 1.0f)]
    public float CloudDensity = 0.5f;

    [Tooltip("Final color multiply. Higher = brighter clouds. Default 1.0.")]
    [Range(0.001f, 5.0f)]
    public float CloudMultiply = 1.0f;

    [Tooltip("How stormy is it? 0 = none, 1 = max")]
    [Range(0.0f, 1.0f)]
    public float Storminess = 0.0f;

    [Tooltip("Fresnel power term")]
    [Range(0.001f, 10.0f)]
    public float FresnelPower = 2.0f;
    [Tooltip("Fresnel scal term")]
    [Range(0.001f, 1.0f)]
    public float FresnelScale = 0.1f;

    [Tooltip("Alpha cutoff for shadows. Default 0.4.")]
    [Range(0.0f, 1.0f)]
    public float ShadowCutoff = 0.4f;

    public Color ClearColor = Color.clear;
    public Color MaxColor = Color.white;
    public Color OverMaxColor = new Color(0.33f, 0.33f, 0.33f);
    public Color StartEdgeColor = new Color(1.0f, 0f, 0f); // Sunrise/sunset starting color
    public Color EndEdgeColor = new Color(1.0f, 0.667f, 0f); // Sunrise/sunset ending color

    public DayNightManager DayNightManager;


    // Start is called before the first frame update
    void Start()
    {
        // Transform and scale the layers appropriately
        //CloudLayer.transform.position = this.transform.position;
        //CloudLayerShadow.transform.position = this.transform.position;

        //CloudLayer.transform.localScale = this.transform.localScale;
        //CloudLayerShadow.transform.localScale = this.transform.localScale;

        // Cache materials
        CloudLayerMaterial = CloudLayer.GetComponent<MeshRenderer>().sharedMaterial;
        CloudLayerShadowMaterial = CloudLayerShadow.GetComponent<MeshRenderer>().sharedMaterial;

        //CloudColorGradient.
    }

    // Update is called once per frame
    [ExecuteAlways]
    void Update()
    {
        //OffsetX = Time.time / 50f;
        //OffsetY = Time.time / 250f;
        // [.1, .75]
        //CloudDensity = (Mathf.Cos(Time.time / 10f) * 0.325f) + 0.1f + 0.325f;
        //Storminess = (Mathf.Cos(Time.time / 10f) * 0.325f) + 0.1f + 0.325f;

        // Update both materials with the same parameters so they stay in sync
        CloudLayerMaterial.SetFloat("_OffsetX", OffsetX);
        CloudLayerMaterial.SetFloat("_OffsetY", OffsetY);
        CloudLayerMaterial.SetFloat("_CloudDensity", CloudDensity);
        CloudLayerMaterial.SetFloat("_CloudMultiply", CloudMultiply);
        CloudLayerMaterial.SetFloat("_Storminess", Storminess);
        CloudLayerMaterial.SetFloat("_Cutoff", ShadowCutoff);
        CloudLayerMaterial.SetFloat("_Power", FresnelPower);
        CloudLayerMaterial.SetFloat("_Scale", FresnelScale);
        CloudLayerMaterial.SetColor("_ClearColor", ClearColor);
        CloudLayerMaterial.SetColor("_FullColor", MaxColor);
        CloudLayerMaterial.SetColor("_OverColor", OverMaxColor);
        CloudLayerMaterial.SetColor("_StartEdgeColor", StartEdgeColor);
        CloudLayerMaterial.SetColor("_EndEdgeColor", EndEdgeColor);
        if (DayNightManager != null)
        {
            CloudLayerMaterial.SetFloat("_SunAngle", DayNightManager.TimeOfDay);
        }

        CloudLayerShadowMaterial.SetFloat("_OffsetX", OffsetX);
        CloudLayerShadowMaterial.SetFloat("_OffsetY", OffsetY);
        CloudLayerShadowMaterial.SetFloat("_CloudDensity", CloudDensity);
        CloudLayerShadowMaterial.SetFloat("_CloudMultiply", CloudMultiply);
        CloudLayerShadowMaterial.SetFloat("_Cutoff", ShadowCutoff);
        CloudLayerShadowMaterial.SetColor("_ClearColor", ClearColor);
        CloudLayerShadowMaterial.SetColor("_FullColor", MaxColor);
        CloudLayerShadowMaterial.SetColor("_OverColor", OverMaxColor);
    }
}
