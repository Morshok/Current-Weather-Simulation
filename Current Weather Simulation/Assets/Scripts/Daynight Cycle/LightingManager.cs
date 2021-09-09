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
            float latitude = sphericalCoordinate.getLatitude();
            float longitude = sphericalCoordinate.getLongitude();
            int timeOffset = calculateTimeOffset(equationOfTime, longitude);
            float trueSolarTime = calculateTrueSolarTime(timeOffset);
            float solarHourAngle = calculateSolarHourAngle(trueSolarTime);
            float solarZenithAngle = calculateSolarZenithAngle(latitude, solarDeclinationAngle, solarHourAngle);
            float solarAzimuthAngle = Mathf.Rad2Deg * calculateSolarAzimuthAngle(latitude, solarDeclinationAngle, solarHourAngle, solarZenithAngle);
            float elevationAngle = Mathf.Rad2Deg * (Mathf.PI - solarZenithAngle);

            /*
            Debug.Log("Fractional Year: " + fractionalYear + "\n" +
                      "Solar Declination Angle: " + solarDeclinationAngle + "\n" +
                      "Equation of Time: " + equationOfTime + "\n" +
                      "Latitude: " + latitude + "\n" +
                      "Longitude: " + longitude + "\n" +
                      "Time Offset: " + timeOffset + "\n" +
                      "True Solar Time: " + trueSolarTime + "\n" +
                      "Solar Hour Angle: " + solarHourAngle + "\n" +
                      "Solar Zenith Angle: " + Mathf.Rad2Deg * solarZenithAngle + "\n" +
                      "Solar Azimuth Angle: " + solarAzimuthAngle + "\n");
            */


            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercentage * 360.0f) - elevationAngle, solarAzimuthAngle, 0.0f));
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
        int dayOfYear = DateTime.Now.Day;
        int currentHour = DateTime.Now.Hour;

        return ((2 * Mathf.PI) / numDaysInYear) * (dayOfYear - 1 + ((currentHour - 12) / 24));
    }

    //Method for calculating the equation of time, in minutes
    private int calculateEquationOfTime(float fractionalYear)
    {
        return (int)(229.18f * (0.000075f + 0.001868f * Mathf.Cos(fractionalYear)             - 0.032077f * Mathf.Sin(fractionalYear) - 0.014615f * Mathf.Cos(2 * fractionalYear)             - 0.040849f * Mathf.Sin(2 * fractionalYear)));
    }

    //Method for calculating the solar declination angle, in radians
    private float calculateSolarDeclinationAngle(float fractionalYear)
    {
        return 0.006918f - 0.399912f * Mathf.Cos(fractionalYear) 
            + 0.070257f * Mathf.Sin(fractionalYear) - 0.006758f * Mathf.Cos(2 * fractionalYear) 
            + 0.000907f * Mathf.Sin(2 * fractionalYear)
            - 0.002697f * Mathf.Cos(3 * fractionalYear) + 0.00148f * Mathf.Sin(3 * fractionalYear);
    }
    
    //Method for calculating the time offset, in minutes
    private int calculateTimeOffset(int equationOfTime, float longitude)
    {
        int timezoneDifference = DateTime.UtcNow.Hour - DateTime.Now.Hour;

        return (int)(equationOfTime + (4.0f * longitude) - (60 * timezoneDifference));
    }

    //Method for calculating the true solar time, in minutes
    private float calculateTrueSolarTime(int timeOffset)
    {
        int currentHour = DateTime.Now.Hour;
        int currentMinute = DateTime.Now.Minute;
        int currentSecond = DateTime.Now.Second;

        return ((currentHour * 60.0f) + currentMinute + (currentSecond / 60.0f) + timeOffset);
    }

    //Method for calculating the solar hour angle, in radians
    private float calculateSolarHourAngle(float trueSolarTime)
    {
        return Mathf.Deg2Rad * ((trueSolarTime / 4.0f) - 180.0f);
    }

    //Method for calculating the solar zenith angle, in radians
    private float calculateSolarZenithAngle(float latitude, float solarDeclinationAngle, float solarHourAngle)
    {
        float summand1 = Mathf.Sin(Mathf.Deg2Rad * latitude) * Mathf.Sin(solarDeclinationAngle);
        float summand2 = Mathf.Cos(Mathf.Deg2Rad * latitude) * Mathf.Cos(solarDeclinationAngle) * Mathf.Cos(solarHourAngle);

        return Mathf.Acos(summand1 + summand1);
    }

    //Method for calculating the solar azimuth angle, in radians
    private float calculateSolarAzimuthAngle(float latitude, float solarDeclinationAngle, float solarHourAngle, float solarZenithAngle)
    {
        float nominator = Mathf.Sin((Mathf.Deg2Rad * latitude)) * Mathf.Cos(solarZenithAngle) - Mathf.Sin(solarDeclinationAngle);
        float denominator = Mathf.Cos((Mathf.Deg2Rad * latitude)) * Mathf.Sin(solarZenithAngle);

        return Mathf.PI - Mathf.Acos(-nominator / denominator); 
    }
    #endregion
}