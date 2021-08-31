using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class OpenWeatherMapDataManager : MonoBehaviour
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private const string API_KEY = "c25006e4f1f04463e8cd05a4d3d6008e"; //Insert your own API Key here

    private string CITIES_FILEPATH = "../city.list.json";

    private List<City> cityList;

    private OpenWeatherMapDataManager()
    {
        //Initialize the list of available cities
        using (StreamReader sr = new StreamReader(CITIES_FILEPATH))
        {
            string json = sr.ReadToEnd();
            cityList = JsonConvert.DeserializeObject<List<City>>(json);
        }

        Debug.Log(getWeatherCondition("Kungälv"));
    }

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

    private string getWeatherCondition(string cityName)
    {
        JObject response = (JObject)getResponse(cityName);

        string weatherMain = response.SelectToken("weather[0].main").ToString();
        string weatherDescription = response.SelectToken("weather[0].description").ToString();

        return "Current weather in " + cityName + ": " + weatherMain + "; " + weatherDescription;
    }
}