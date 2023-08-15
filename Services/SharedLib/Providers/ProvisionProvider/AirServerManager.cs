using Newtonsoft.Json;
using SharedLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ucip_lib_v5.models;

namespace SharedLib.Providers.ProvisionProvider
{
    public class AirServerManager : IAirServerManager
    {
        private const string NULL_AIRSERVERS_MESSAGE = "No AIR Servers Found";
        private const string NO_ACTIVE_AIRSERVERS_MESSAGE = "No AIR Active Server Found";
        protected List<AirServer> inMemoryAirServerList;

        public AirServerManager()
        {
            SetupInMemoryJSONAirServerList();
        }
        public AirServer GetRandomActiveAir()
        {
            if (inMemoryAirServerList == null || !inMemoryAirServerList.Any()) throw new InvalidOperationException(NULL_AIRSERVERS_MESSAGE);

            var rand = new Random();
            var allActiveAirServer = inMemoryAirServerList.Where(air => air.Active);

            if (allActiveAirServer == null) throw new InvalidOperationException(NO_ACTIVE_AIRSERVERS_MESSAGE);

            var randomActiveAIRServer = allActiveAirServer
                                                .Skip(rand.Next(0, allActiveAirServer.Count() - 1))
                                                .FirstOrDefault();
            return randomActiveAIRServer;
        }
        public AirServer GetDifferentActiveAir(AirServer airServer)
        {
            if (inMemoryAirServerList == null || !inMemoryAirServerList.Any()) throw new InvalidOperationException(NULL_AIRSERVERS_MESSAGE);

            var differentActiveAIRServer = inMemoryAirServerList
                                                .FirstOrDefault(air => air.Active && air.IPAddress != airServer.IPAddress);
            return differentActiveAIRServer;
        }

        protected void SetupInMemoryJSONAirServerList()
        {
            string airServersFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "AIRServers.json");

            using (StreamReader r = new StreamReader(airServersFile))
            {
                string json = r.ReadToEnd();
                inMemoryAirServerList = JsonConvert.DeserializeObject<List<AirServer>>(json);
            }

        }
    }
}
