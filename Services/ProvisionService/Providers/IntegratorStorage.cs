using helpers.Database;
using helpers.Engine;
using helpers.Interfaces;
using models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProvisionService.Providers
{
    public class IntegratorStorage : IIntegratorStorage
    {
        protected List<Integrator> _inMemoryIntegratorList;
        private readonly IDataProvider _dataProvider;

        public IntegratorStorage(IDataProvider dataProvider, IDBHelper dbHelper)
        {
            _dataProvider = dataProvider;
            UpdateIntegratorsList().Wait();
            new PgSubscriber(dbHelper, "on_integrator_changed", new TimerCallback(UpdateIntegratorList), 500);
        }

        private void UpdateIntegratorList(object state)
        {
            UpdateIntegratorsList().Wait();
            Console.WriteLine("Integrators reloaded");
        }

        public async Task<List<Integrator>> GetIntegrators()
        {
            if (_inMemoryIntegratorList == null) await UpdateIntegratorsList();

            return _inMemoryIntegratorList;
        }

        private async Task<List<Integrator>>UpdateIntegratorsList()
        {
            var integrators = await _dataProvider.GetIntegrators();
            if (integrators == null || !integrators.Any()) return new List<Integrator> { };
           
            _inMemoryIntegratorList = integrators.Select(x => new Integrator
            {
                Active = true,
                IntegratorCode = x.IntegratorCode,
                IntegratorName = x.IntegratorName,
                IntegratorToken = x.IntegratorToken,
                Data = x,
            })?.ToList();

            return _inMemoryIntegratorList;
        }

    }
}
