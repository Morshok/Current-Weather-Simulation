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

    private void Start()
    {
        isLeapYear = DateTime.IsLeapYear(DateTime.Now.Year);
        sphericalCoordinate = new SphericalCoordinate(0.0f, 0.0f);
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

            float fractionalYear = calculateFractionalYear();
            float solarDeclinationAngle = calculateSolarDeclinationAngle(fractionalYear);
            int equationOfTime = calculateEquationOfTime(fractionalYear);
            int timeOffset = calculateTimeOffset(equationOfTime);
            float trueSolarTime = calculateTrueSolarTime(timeOffset);
            float solarHourAngle = calculateSolarHourAngle(trueSolarTime);
            float latitude = sphericalCoordinate.getLatitude();
            float solarZenithAngle = calculateSolarZenithAngle(latitude, solarDeclinationAngle, solarHourAngle);
            float solarAzimuthAngle = calculateSolarAzimuthAngle(latitude, solarDeclinationAngle, solarHourAngle, solarZenithAngle);
            float elevationAngle = 90.0f - solarZenithAngle;


            DirectionalLight.transform.rotation = Quaternion.Euler(new Vector3(elevationAngle, solarAzimuthAngle, 0));
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

    #region Solar Position Calculations
    //Method for calculating the fractional year, which is 
    //then used to calculate the equation of time in minutes, 
    //and the suns declination in radians
    private float calculateFractionalYear()
    {
        int numDaysInYear = isLeapYear ? 366 : 365;
        int dayOfYear = System.DateTime.Now.Day;
        int currentHour = System.DateTime.Now.Hour;

        return ((2 * Mathf.PI) / numDaysInYear) * (dayOfYear - 1 + ((currentHour - 12) / 24));
    }

    //Method for calculating the equation of time
    private int calculateEquationOfTime(float fractionalYear)
    {
        return (int)(229.18f * (0.000075f + 0.001868f * Mathf.Cos(fractionalYear)             - 0.032077f * Mathf.Sin(fractionalYear) - 0.014615f * Mathf.Cos(2 * fractionalYear)             - 0.040849f * Mathf.Sin(2 * fractionalYear)));
    }

    //Method for calculating the solar declination angle
    private float calculateSolarDeclinationAngle(float fractionalYear)
    {
        return 0.006918f - 0.399912f * Mathf.Cos(fractionalYear) 
            + 0.070257f * Mathf.Sin(fractionalYear) - 0.006758f * Mathf.Cos(2 * fractionalYear) 
            + 0.000907f * Mathf.Sin(2 * fractionalYear)
            - 0.002697f * Mathf.Cos(3 * fractionalYear) + 0.00148f * Mathf.Sin(3 * fractionalYear);
    }
    
    //Method for calculating the time offset
    private int calculateTimeOffset(int equationOfTime)
    {
        int timezoneDifference = DateTime.UtcNow.Hour - DateTime.Now.Hour;

        return (int)(equationOfTime + (4 * sphericalCoordinate.getLongitude()) - (60 * timezoneDifference));
    }

    //Method for calculating the true solar time
    private float calculateTrueSolarTime(int timeOffset)
    {
        int currentHour = DateTime.Now.Hour;
        int currentMinute = DateTime.Now.Minute;
        int currentSecond = DateTime.Now.Second;

        return ((currentHour * 60) + currentMinute + (currentSecond / 60) + timeOffset);
    }

    //Method for calculating the solar hour angle in degrees
    private float calculateSolarHourAngle(float trueSolarTime)
    {
        return (trueSolarTime / 4) - 180.0f;
    }

    private float calculateSolarZenithAngle(float latitude, float solarDeclinationAngle, float solarHourAngle)
    {
        return Mathf.Acos((Mathf.Sin(latitude) * Mathf.Sin(solarDeclinationAngle)) + 
              (Mathf.Cos(latitude) * Mathf.Cos(solarDeclinationAngle) * Mathf.Cos(solarHourAngle)));
    }

    private float calculateSolarAzimuthAngle(float latitude, float solarDeclinationAngle, float solarHourAngle, float solarZenithAngle)
    {
        return 180.0f - Mathf.Acos(-((Mathf.Sin(latitude) * Mathf.Cos(solarZenithAngle) - Mathf.Sin(solarDeclinationAngle)) / (Mathf.Cos(latitude)*Mathf.Sin(solarZenithAngle))));
    }
    #endregion
}