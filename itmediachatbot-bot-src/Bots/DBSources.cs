using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.BotBuilderSamples
{
    public class DBSources
    {
        private string datasource = "softvisionchats.database.windows.net";
        private string userid = "admin2019";
        private string password = "iesiafara123!";
        private string catalog = "ContestData";
        public static DBSources Instance = new DBSources();

        private DBSources() { }

        public string GetDataSource()
        {
            return datasource;
        }

        public string GetUserId()
        {
            return userid;
        }

        public string GetPassword()
        {
            return password;
        }

        public string GetCatalog()
        {
            return catalog;
        }
    }
}
