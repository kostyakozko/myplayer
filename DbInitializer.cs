using System;
using System.Data.SqlServerCe;
using System.IO;

namespace MyPlayer
{
    public static class DBInitializer
    {
        static public bool InitDB(string directory, string fileName)
        {
            try
            {
                string filepath = String.Format("{0}\\{1}", directory, fileName);
                if (!File.Exists(filepath))
                {
                    string connectionString;
                    string password = "";
                    connectionString = String.Format("DataSource=\"{0}\"; Password='{1}'",
                        filepath, password);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    using (SqlCeEngine en = new SqlCeEngine(connectionString))
                    {
                        en.CreateDatabase();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        static public bool InitTables(string directory, string fileName)
        {
            try
            {
                string connectionString;
                string password = "";
                string filepath = String.Format("{0}\\{1}", directory, fileName);
                connectionString = String.Format("DataSource=\"{0}\"; Password='{1}'",
                    filepath, password);
                using (SqlCeConnection con = new SqlCeConnection(connectionString))
                {
                    string sql = "SELECT Count(*) " +
                    "FROM INFORMATION_SCHEMA.TABLES " +
                    "WHERE TABLE_NAME = 'Folders' ";
                    SqlCeCommand com = new SqlCeCommand(sql, con);
                    con.Open();
                    if (Convert.ToInt32(com.ExecuteScalar()) == 0)
                    {
                        sql = "CREATE TABLE Folders(" +
                        "id int primary key identity(1,1)," +
                        "name nvarchar(255)" +
                        ");";
                        SqlCeCommand com2 = new SqlCeCommand(sql, con);
                        com2.ExecuteScalar();
                    }
                    sql = "SELECT Count(*) " +
                   "FROM INFORMATION_SCHEMA.TABLES " +
                   "WHERE TABLE_NAME = 'Albums' ";
                    SqlCeCommand com3 = new SqlCeCommand(sql, con);
                    if (Convert.ToInt32(com3.ExecuteScalar()) == 0)
                    {
                        sql = "CREATE TABLE Albums(" +
                        "id int primary key identity(1,1)," +
                        "name nvarchar(255)" +
                        ");";
                        SqlCeCommand com4 = new SqlCeCommand(sql, con);
                        com4.ExecuteScalar();
                    }
                    sql = "SELECT Count(*) " +
                   "FROM INFORMATION_SCHEMA.TABLES " +
                   "WHERE TABLE_NAME = 'Artists' ";
                    SqlCeCommand com5= new SqlCeCommand(sql, con);
                    if (Convert.ToInt32(com5.ExecuteScalar()) == 0)
                    {
                        sql = "CREATE TABLE Artists(" +
                        "id int primary key identity(1,1)," +
                        "name nvarchar(255)" +
                        ");";
                        SqlCeCommand com6 = new SqlCeCommand(sql, con);
                        com6.ExecuteScalar();
                    }
                    sql = "SELECT Count(*) " +
                   "FROM INFORMATION_SCHEMA.TABLES " +
                   "WHERE TABLE_NAME = 'Year' ";
                    SqlCeCommand com7 = new SqlCeCommand(sql, con);
                    if (Convert.ToInt32(com7.ExecuteScalar()) == 0)
                    {
                        sql = "CREATE TABLE Year(" +
                        "id int primary key identity(1,1)," +
                        "year int" +
                        ");";
                        SqlCeCommand com8 = new SqlCeCommand(sql, con);
                        com8.ExecuteScalar();
                    }
                    sql = "SELECT Count(*) " +
                   "FROM INFORMATION_SCHEMA.TABLES " +
                   "WHERE TABLE_NAME = 'Songs' ";
                    SqlCeCommand com9 = new SqlCeCommand(sql, con);
                    if (Convert.ToInt32(com9.ExecuteScalar()) == 0)
                    {
                        sql = "CREATE TABLE Songs(" +
                        "id int primary key identity(1,1)," +
                        "name nvarchar(255)," +
                        "folder_id int," +
                        "artist_id int," +
                        "album_id int," +
                        "year_id int," +
                        "path nvarchar(255)" +
                        ");";
                        SqlCeCommand com10 = new SqlCeCommand(sql, con);
                        com10.ExecuteScalar();
                    }
                }
                return true;
            }
            catch 
            {
                
                return false;
            }
        }
    }
}
