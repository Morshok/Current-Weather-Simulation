using System;
using UnityEngine;

public class SunPosition
{
    private const float Deg2Rad = Mathf.Deg2Rad;
    private const float Rad2Deg = Mathf.Rad2Deg;

    public SunPosition() { }

    /*!
     * Method for calculating the position of the sun in the sky
     * 
     * \param dateTime  | The time and date in local time
     * \param longitude | Current longitude in decimal degrees
     * \param latitude  | Current latitude in decimal degrees
     * \return          | Returns a Vector3 of the current solar position
     */
    public Vector3 calculateSunPosition(DateTime dateTime, float longitude, float latitude)
    {
        //Converts current date to UTC
        dateTime = dateTime.ToUniversalTime();

        //number of days from J2000.0
        float julianDate = 367 * dateTime.Year -
            (int)((7.0f / 4.0f) * (dateTime.Year +
            (int)((dateTime.Month + 9.0f) / 12.0f))) +
            (int)((275.0f * dateTime.Month) / 9.0f) +
            dateTime.Day - 730531.5f;

        float julianCenturies = (julianDate / 36525.0f);

        //Sidereal time
        float siderealTimeHours = (6.6974f + 2400.0513f * julianCenturies);
        float siderealTimeUT = siderealTimeHours + (366.2422f / 365.2422f) * (float)dateTime.TimeOfDay.TotalHours;
        float siderealTime = siderealTimeUT * 15 + longitude;

        //Refine to number of days (fractional) to specific time
        julianDate += (float)(dateTime.TimeOfDay.TotalHours / 24.0f);
        julianCenturies = julianDate / 36525.0f;

        //Calculate solar coordinates
        float meanLongitude = correctAngle(Deg2Rad * (280.466f + 36000.77f * julianCenturies));

        float meanAnomaly = correctAngle(Deg2Rad * (357.529f + 35999.05f * julianCenturies));

        float equationOfCenter = Deg2Rad * (
            (1.195f - 0.005f * julianCenturies) * Mathf.Sin(meanAnomaly) + 
            0.02f * Mathf.Sin(2 * meanAnomaly));

        float elipticalLongitude = correctAngle(meanLongitude + equationOfCenter);

        float obliquity = Deg2Rad * (23.439f - 0.013f * julianCenturies);
        
        float rightAscension = Mathf.Atan2(
                Mathf.Cos(obliquity) * Mathf.Sin(elipticalLongitude),
                Mathf.Cos(elipticalLongitude)
            );

        float declination = Mathf.Asin(Mathf.Sin(rightAscension) * Mathf.Sin(obliquity));

        float hourAngle = correctAngle(siderealTime * Deg2Rad) - rightAscension;

        if(hourAngle > Mathf.PI)
        {
            hourAngle -= (2 * Mathf.PI);
        }

        float altitude = Mathf.Asin(
                Mathf.Sin(latitude * Deg2Rad) * Mathf.Sin(declination) +
                Mathf.Cos(latitude * Deg2Rad) * Mathf.Cos(declination) * Mathf.Cos(hourAngle)
            );

        float solarAzimuthNominator = -Mathf.Sin(hourAngle);
        float solarAzimuthDenominator = (
                Mathf.Tan(declination) * Mathf.Cos(latitude * Deg2Rad) - 
                Mathf.Sin(latitude * Deg2Rad) * Mathf.Cos(hourAngle)
            );
        float solarAzimuth = Mathf.Atan(solarAzimuthNominator / solarAzimuthDenominator);

        if(solarAzimuthDenominator < 0)
        {
            solarAzimuth += Mathf.PI;
        }
        else if(solarAzimuthNominator < 0)
        {
            solarAzimuth += (2 * Mathf.PI);
        }

        Vector3 solarAngles = new Vector3();
        solarAngles.x = (float)(altitude * Rad2Deg);
        solarAngles.y = (float)(solarAzimuth * Rad2Deg);

        return solarAngles;
    }
    
    /*!
     * Corrects an angle to be in the range 0 to 2*PI
     * 
     * \param angleInRadians | The angle to be corrected, in radians
     * \return | An angle in the range 0 to 2*PI
     */ 
    private float correctAngle(float angleInRadians)
    {
        if(angleInRadians < 0)
        {
            return 2 * Mathf.PI - (Mathf.Abs(angleInRadians) % (2 * Mathf.PI));
        }
        else if(angleInRadians > (2 * Mathf.PI))
        {
            return angleInRadians % (2 * Mathf.PI);
        }
        else
        {
            return angleInRadians;
        }
    }
}