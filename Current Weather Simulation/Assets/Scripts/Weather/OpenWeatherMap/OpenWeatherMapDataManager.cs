using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class OpenWeatherMapDataManager : MonoBehaviour
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private const string API_KEY = "c25006e4f1f04463e8cd05a4d3d6008e"; //Insert your own API Key here
    private const string CITIES_FILEPATH = "../city.list.json"; //Filepath to the citylist Json file

    private List<City> cityList;
    private Timer updateTimer;
    private int weatherConditionCode;
    private SphericalCoordinate sphericalCoordinate;

    private void Start()
    {
        sphericalCoordinate = new SphericalCoordinate();

        //Initialize the list of available cities
        using (StreamReader sr = new StreamReader(CITIES_FILEPATH))
        {
            string json = sr.ReadToEnd();
            cityList = JsonConvert.DeserializeObject<List<City>>(json);
        }

        updateTimer = new Timer(
            e => fetchOpenWeatherMapParameters("Diseröd"),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(10)
        );
    }

    private void OnApplicationQuit()
    {
        updateTimer.Dispose();
    }

    public int getWeatherConditionCode() { return this.weatherConditionCode; }
    public SphericalCoordinate getSphericalCoordinate() { return this.sphericalCoordinate; }

    private void setWeatherConditionCode(int weatherConditionCode) { this.weatherConditionCode = weatherConditionCode; }

    private int isCityValid(string cityName)
    {
        return cityList.FindIndex(
            x => x.Name.ToLower().Equals(cityName.ToLower())
        );
    }

    private JObject getResponse(string cityName)
    {
        string url = string.Format("http://api.openweathermap.org/data/2.5/weather?id={0}&appid={1}", cityList[isCityValid(cityName)].Id, API_KEY);

        return JObject.Parse(new System.Net.WebClient().DownloadString(url));
    }

    private void fetchOpenWeatherMapParameters(string cityName)
    {
        JObject response = (JObject)getResponse(cityName);

        int weatherConditionCode = int.Parse(response.SelectToken("weather[0].id").ToString());

        float longitude = float.Parse(response.SelectToken("coord.lon").ToString());
        float latitude = float.Parse(response.SelectToken("coord.lat").ToString());

        this.weatherConditionCode = weatherConditionCode;
        this.sphericalCoordinate = new SphericalCoordinate(longitude, latitude);
    }
}