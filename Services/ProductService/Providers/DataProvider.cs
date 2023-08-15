using System;
using helpers.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using helpers.Database;
using Serilog;
using SharedLib.Models;
using NpgsqlTypes;
using helpers;

namespace ProductService.Providers
{
    public class DataProvider : IDataProvider
    {
        private readonly IDBHelper _dbHelper;
        public DataProvider(IDBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

    }
}

