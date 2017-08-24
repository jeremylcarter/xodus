using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Newtonsoft.Json;

namespace Xodus
{
    public class OpenWeatherMapAPI
    {
        private static readonly string API_KEY = "be0d4dcbaceaa8c3c99db5348a3cd166";
        private CurrentTemperatureResponse currentTemperatureResponse;

        public async Task GetCurrentWeather()
        {
            try
            {
                var gl = new Geolocator();
                gl.AllowFallbackToConsentlessPositions();
                var location = await gl.GetGeopositionAsync();

                var uri =
                    $"http://api.openweathermap.org/data/2.5/weather?lat={location.Coordinate.Point.Position.Latitude}&lon={location.Coordinate.Point.Position.Longitude}&appid=";
                uri += API_KEY;
                var endPoint = new Uri(uri);

                var client = Utilities.GetHttpClient();
                var response = await client.GetStringAsync(endPoint);

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                currentTemperatureResponse =
                    JsonConvert.DeserializeObject<CurrentTemperatureResponse>(response, settings);
            }
            catch (Exception)
            {
            }
        }

        public string GetWeatherIcon()
        {
            if (currentTemperatureResponse?.weather?.Count > 0)
            {
                var icon = "ms-appx:///Assets/" + currentTemperatureResponse.weather[0]?.icon;
                icon += ".png";
                return icon;
            }

            return "ms-appx:///Assets/01d.png";
        }

        private double TempAsCelsius()
        {
            return Math.Round(currentTemperatureResponse.main.temp - 273.15, 3);
        }

        public string GetCurrentTemperatureFarhenheit()
        {
            var country = "US";

            try
            {
                var name = CultureInfo.CurrentCulture.Name.Split('-')[1];
                country = name;
            }
            catch (Exception)
            {
            }
            if (null == currentTemperatureResponse)
                return "";


            if (country.ToLower() == "us" || country.ToLower() == "bs" || country.ToLower() == "bz" ||
                country.ToLower() == "ky" || country.ToLower() == "pw" || country.ToLower() == "pw" ||
                country.ToLower() == "gu" || country.ToLower() == "vi" || country.ToLower() == "us")
            {
                var current = (int) Math.Round(9.0 / 5.0 * TempAsCelsius() + 32, 3);
                return string.Format("{0}° F", current.ToString());
            }
            else
            {
                var current = (int) TempAsCelsius();
                return string.Format("{0}° C", current.ToString());
            }
        }
    }
}