using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Trurl.Plugins.Weather
{
    public class WeatherCommand : ICommand
    {
        private readonly string _googleMapsApiurl;
        private readonly string _googleMapsApikey;
        
        public string Verb => "weather";

        public List<string> Usage => new List<string>()
        {
            "`weather` - gets weather for the default location",
            "`weather [location]` - gets the weather for the specified location"
        };

        public WeatherCommand()
        {
            _googleMapsApiurl = Configuration.Configuration.GetConfigurationValue("GOOGLE_MAPS_APIURL");
            _googleMapsApikey = Configuration.Configuration.GetConfigurationValue("GOOGLE_MAPS_APIKEY");
        }

        public ICommandResult Execute()
        {
            return Execute("");
        }

        public ICommandResult Execute(string args)
        {
            try
            {
                var address = Uri.EscapeDataString(args);
                if (string.IsNullOrWhiteSpace(address))
                {
                    address = Configuration.Configuration.GetConfigurationValue("WEATHER_LOCATION");
                }
                var googleEndpoint = $"{ _googleMapsApiurl }?key={ _googleMapsApikey }&address={address}";
                var googleResponse = new StreamReader(
                        WebRequest.Create(googleEndpoint)
                            .GetResponse()
                            .GetResponseStream())
                    .ReadToEnd();

                dynamic geocode = JsonConvert.DeserializeObject(googleResponse);

                var lat = geocode.results[0].geometry.location.lat.Value;
                var lng = geocode.results[0].geometry.location.lng.Value;

                var darksky_apikey = "e657f5bcd0a7bf280a4a04cd809c3d01";
                var darkskyApiurl = $"https://api.darksky.net/forecast/{darksky_apikey}/{lat},{lng}";
                var darkskyResponse = new StreamReader(
                        WebRequest.Create(darkskyApiurl)
                            .GetResponse()
                            .GetResponseStream())
                    .ReadToEnd();

                dynamic weather = JsonConvert.DeserializeObject(darkskyResponse);

                return new CommandResult()
                {
                    Status = Status.Ok,
                    Message =
                        $"{geocode.results[0].formatted_address} feels like {weather.currently.apparentTemperature}. {weather.minutely.summary}"
                };
            }
            catch (Exception e)
            {
                return new CommandResult()
                {
                    Status = Status.Error,
                    Message = $"{e.Message}"
                };
            }

        }

        public ICommandResult Execute(IEnumerable<string> args)
        {
            return Execute(string.Join(" ", args));
        }
    }
}