using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset preset;

    [SerializeField] private OpenWeatherMapDataManager openWeatherMapDataManager;

    private bool isLeapYear;
    private SphericalCoordinate sphericalCoordinate;
    private SunPosition sunPosition;

    private void Start()
    {
        isLeapYear = DateTime.IsLeapYear(DateTime.Now.Year);
        sphericalCoordinate = new SphericalCoordinate(0.0f, 0.0f);
        sunPosition = new SunPosition();
    }

    private void Update()
    {
        if (preset == null)
            return;

        if(Application.isPlaying)
        {
            SphericalCoordinate currentSphericalCoordinate = openWeatherMapDataManager.getSphericalCoordinate();

            if (!currentSphericalCoordinate.Equals(sphericalCoordinate))
            {
                sphericalCoordinate = currentSphericalCoordinate;
            }

            TimeSpan sinceMidnight = DateTime.Now - DateTime.Today;
            float millisecondsSinceMidnight = (float)sinceMidnight.TotalMilliseconds;

            UpdateLighting(millisecondsSinceMidnight / 86400000.0f);
        }
    }

    private void UpdateLighting(float timePercentage)
    {
        
        RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePercentage);
        RenderSettings.fogColor = preset.FogColor.Evaluate(timePercentage);

        
        if(DirectionalLight != null)
        {
            DirectionalLight.color = preset.DirectionalColor.Evaluate(timePercentage);

            float currentLongitude = sphericalCoordinate.getLongitude();
            float currentLatitude = sphericalCoordinate.getLatitude();
            DateTime localTime = DateTime.Now;

            Vector3 solarAngles = sunPosition.calculateSunPosition(localTime, currentLongitude, currentLatitude);
            DirectionalLight.transform.localRotation = Quaternion.Euler(solarAngles);
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