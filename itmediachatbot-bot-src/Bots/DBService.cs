 
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    public class DBService
    {
        private SqlConnection connection;
        public static DBService Instance = new DBService();
        private DBService()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = DBSources.Instance.GetDataSource();
            builder.UserID = DBSources.Instance.GetUserId();
            builder.Password = DBSources.Instance.GetPassword();
            builder.InitialCatalog = DBSources.Instance.GetCatalog();
            connection = new SqlConnection(builder.ConnectionString);
        }

        public SqlDataReader GetQueryResult(string query)
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    connection.Close();
                    return reader;
                }
            }

            
        }
        
        public void PostQueryAction(string query)
        {
            System.Data.SqlClient.SqlConnection sqlConnection1 =
    new System.Data.SqlClient.SqlConnection("Server=tcp:softvisionchats.database.windows.net,1433;Initial Catalog=ContestData;Persist Security Info=False;User ID=admin2019;Password=iesiafara123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand
            {
                CommandType = System.Data.CommandType.Text,
                CommandText = query,
                Connection = sqlConnection1
            };

            sqlConnection1.Open();
            cmd.ExecuteNonQuery();
            sqlConnection1.Close();
        }
    }
}
