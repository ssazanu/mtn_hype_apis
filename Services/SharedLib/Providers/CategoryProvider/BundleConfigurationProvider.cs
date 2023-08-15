using helpers.Database;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLib.Providers.CategoryProvider
{
    public class BundleConfigurationProvider : BaseConfigurationProvider
    {
        private readonly IDBHelper _dbHelper;

        public BundleConfigurationProvider(IDBHelper dbHelper)
        {
            _dbHelper = dbHelper;
            inMemoryCategoryList = SetupInMemoryCategoryList().Result;
            new PgSubscriber(dbHelper, "on_bundle_config_changed", new TimerCallback(UpdateConfig));
        }

        private void UpdateConfig(object state)
        {
            inMemoryCategoryList = SetupInMemoryCategoryList().Result;
            Console.WriteLine("Bundle Configuration reloaded");
        }

        protected async Task<List<Category>> SetupInMemoryCategoryList()
        {
            var categories = await _dbHelper.Fetch<Category>("GetBundleConfiguration", null);

            return categories;
        }
    }
}
