using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlServerCe;
using System.Data;

namespace myplayer
{
    public static class FolderProcessing
    {
        private static List<string> folderList = new List<string>();

        private static List<string> GetFolderFiles(string folderPath)
        {
            List<string> filesList = new List<string>();
            foreach (string s in Directory.GetFiles(folderPath))
            {
                filesList.Add(s);
            }
            foreach (string s in Directory.GetDirectories(folderPath))
            {
                foreach (string filename in GetFolderFiles(s))
                {
                    filesList.Add(filename);
                }
            }
            return filesList;
        }

        public static void AddFolderToDB(string folderPath, string dbfilepath)
        {
            string password = "";
            string connectionString = String.Format("DataSource=\"{0}\"; Password='{1}'",
                    dbfilepath, password);
            using (SqlCeConnection con = new SqlCeConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT Count(*) FROM Folders WHERE name='" +
                    folderPath+"'";
                SqlCeCommand com = new SqlCeCommand(sql, con);
                if (Convert.ToInt32(com.ExecuteScalar()) != 0)
                {
                    return;
                }
                SqlCeDataAdapter da = new SqlCeDataAdapter("Select * FROM Folders", con);
                da.InsertCommand = new SqlCeCommand(
                    "INSERT INTO Folders (name) values(@folder_name)");
                da.InsertCommand.Parameters.Add("@folder_name", SqlDbType.NVarChar, 255, "name");
                da.InsertCommand.Connection = con;
                DataSet ds = new DataSet("Folder");
                DataTable dt = new DataTable("Folders");
                dt.Columns.Add(new DataColumn("id", typeof(int)));
                dt.Columns.Add(new DataColumn("name", typeof(string)));
                ds.Tables.Add(dt);
                da.Fill(ds, "Folders");
                if (null != folderList)
                {
                    folderList.Clear();
                }
                foreach (DataRow dr in ds.Tables["Folders"].Rows)
                {
                    folderList.Add(dr["name"].ToString());
                }
                DataRow drAdd = dt.NewRow();
                int RowCount = ds.Tables["Folders"].Rows.Count - 1;
                if (RowCount >= 0)
                {
                    drAdd["id"] = int.Parse(ds.Tables["Folders"].Rows[RowCount]["id"].ToString()) + 1;
                }
                else
                {
                    drAdd["id"] = 1;
                }
                drAdd["name"] = folderPath;
                dt.Rows.Add(drAdd);
                da.Update(ds, "Folders");
                List<string> removelist = new List<string>();
                removelist.Clear();
                List<string> list = GetFolderFiles(folderPath);
                foreach (string li in list)
                {
                    if (!SongProcessing.ProcessSong(li))
                    {
                        removelist.Add(li);
                    }
                }
                foreach (string li in removelist)
                {
                    list.Remove(li);
                }
            }
        }

        public static void DeleteFolderFromDB(string folderPath, string dbfilepath)
        {
            string password = "";
            string connectionString = String.Format("DataSource=\"{0}\"; Password='{1}'",
                    dbfilepath, password);
            using (SqlCeConnection con = new SqlCeConnection(connectionString))
            {
                con.Open();
                SqlCeDataAdapter da = new SqlCeDataAdapter("Select * FROM Folders", con);
                da.DeleteCommand = new SqlCeCommand(
                    "DELETE FROM Folders WHERE id = @original_id " +
                    "and name = @original_name");
                da.DeleteCommand.Parameters.Add("@original_id", SqlDbType.Int, 0, "id");
                da.DeleteCommand.Parameters.Add("@original_name", SqlDbType.NVarChar, 255, "name");
                da.DeleteCommand.Connection = con;
                DataSet ds = new DataSet("Folder");
                DataTable dt = new DataTable("Folders");
                dt.Columns.Add(new DataColumn("id", typeof(int)));
                dt.Columns.Add(new DataColumn("name", typeof(string)));
                ds.Tables.Add(dt);
                da.Fill(ds, "Folders");
                folderList.Clear();
                foreach (DataRow dr in ds.Tables["Folders"].Rows)
                {
                    folderList.Add(dr["name"].ToString());
                }
                int ind = -1;
                for (int i = 0; i < folderList.Count; i++)
                {
                    if (folderList[i] == folderPath)
                    {
                        ind = i;
                        break;
                    }
                }
                string folderid = ds.Tables["Folders"].Rows[ind]["id"].ToString();
                string sql = "DELETE FROM Songs WHERE folder_id = " + folderid;
                SqlCeCommand com = new SqlCeCommand(sql, con);
                com.ExecuteNonQuery();
                dt.Rows[ind].Delete();
                da.Update(ds, "Folders");
            }
        }

        private static void GetFoldersFromDB(string dbfilepath)
        {
            string password = "";
            string connectionString = String.Format("DataSource=\"{0}\"; Password='{1}'",
                    dbfilepath, password);
            using (SqlCeConnection con = new SqlCeConnection(connectionString))
            {
                con.Open();
                SqlCeDataAdapter da = new SqlCeDataAdapter("Select * FROM Folders", con);
                DataSet ds = new DataSet("Folder");
                DataTable dt = new DataTable("Folders");
                dt.Columns.Add(new DataColumn("id", typeof(int)));
                dt.Columns.Add(new DataColumn("name", typeof(string)));
                ds.Tables.Add(dt);
                da.Fill(ds, "Folders");
                foreach (DataRow dr in ds.Tables["Folders"].Rows)
                {
                    folderList.Add(dr["name"].ToString());
                }
            }
        }

        private static void UpdateSongInfoOnWork(string dbfilepath)
        {
            SongQueue.queueMutex.WaitOne();
            SongDbItems song = new SongDbItems();
            if (SongQueue.items.Count > 0)
            {
                song = SongQueue.items.Dequeue();
                string password = "";
                string connectionString = String.Format("DataSource=\"{0}\"; Password='{1}'",
                    dbfilepath, password);
                using (SqlCeConnection con = new SqlCeConnection(connectionString))
                {
                    con.Open();
                    string filedir = Path.GetDirectoryName(song.path);
                    string sql = 
                }
            }
            SongQueue.queueMutex.ReleaseMutex();
        }

        private static void UpdateSongListOnWork(string dbfilepath)
        {
        }

        public static void ProcessAll()
        {
        }
    }
}
