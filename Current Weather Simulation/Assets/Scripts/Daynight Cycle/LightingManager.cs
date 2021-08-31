using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset preset;

    [SerializeField, Range(0, 24)] private float TimeOfDay;

    private void Update()
    {
        if (preset == null)
            return;

        if(Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime;
            TimeOfDay %= 24;

            int currentTimeInSeconds = System.DateTime.Now.Second;
        }

        UpdateLighting(TimeOfDay / 24f);
    }

    private void UpdateLighting(float timePercentage)
    {
        RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePercentage);
        RenderSettings.fogColor = preset.FogColor.Evaluate(timePercentage);

        if(DirectionalLight != null)
        {
            DirectionalLight.color = preset.DirectionalColor.Evaluate(timePercentage);
            DirectionalLight.transform.rotation = Quaternion.Euler(new Vector3((timePercentage * 360f) - 90f, 180f, 0));
        }
    }

    private void OnValidate()
    {
        if(DirectionalLight != null)
            return;

        if (RenderSettings.sun != null)
            DirectionalLight = RenderSettings.sun;
        else
        {
            Light[] ligths = GameObject.FindObjectsOfType<Light>();

            foreach(Light light in ligths)
            {
                if(light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }

    }
}