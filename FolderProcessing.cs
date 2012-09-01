using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlServerCe;
using System.Data;
using System.Threading;

namespace myplayer
{
    public static class FolderProcessing
    {
        private struct folder_params
        {
            public string folderpath;
            public string filepath;
        }
        public static Queue<string> FoldersToAdd = new Queue<string>();
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

        private static void AddFolderToDB(object folder_param)
        {
            string folderPath = ((folder_params)folder_param).folderpath;
            string dbfilepath = ((folder_params)folder_param).filepath;
            string password = "";
            string connectionString = String.Format("DataSource=\"{0}\"; Password='{1}'",
                    dbfilepath, password);
            using (SqlCeConnection con = new SqlCeConnection(connectionString))
            {
                con.Open();
                string sql = "SELECT Count(*) FROM Folders WHERE name='" +
                    folderPath.Replace("'", "`") + "'";
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
                drAdd["name"] = folderPath.Replace("'", "`");
                dt.Rows.Add(drAdd);
                da.Update(ds, "Folders");
                List<string> removelist = new List<string>();
                removelist.Clear();
                List<string> list = GetFolderFiles(folderPath);
                foreach (string li in list)
                {
                    if (!SongProcessing.ProcessSong(li, folderPath))
                    {
                        removelist.Add(li);
                    }
                }
                int i = 0;
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
                    if (folderList[i] == folderPath.Replace("'", "`"))
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

        private static void UpdateSongInfoOnWork(object filepath)
        {
            string dbfilepath = Convert.ToString(filepath);
            int sleeptime = 1;
            while (true)
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
                        string sql = "SELECT id FROM Folders WHERE name='" +
                        song.rootdir + "'";
                        SqlCeCommand com = new SqlCeCommand(sql, con);
                        int folder_id = Convert.ToInt32(com.ExecuteScalar());
                        if (folder_id == 0)
                        {
                            sql = "INSERT INTO Folders (name) values(@Name);";
                            SqlCeCommand cmd = new SqlCeCommand(sql, con);
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar);
                            cmd.Parameters["@Name"].Value = song.rootdir;
                            cmd.ExecuteScalar();
                            sql = "SELECT id FROM Folders WHERE name='" +
                            song.rootdir + "'";
                            folder_id = Convert.ToInt32(com.ExecuteScalar());

                        }
                        if (String.IsNullOrEmpty(song.album))
                        {
                            song.album = "Unknown album";
                        }
                        sql = "SELECT id FROM Albums WHERE name='" +
                        song.album + "'";
                        SqlCeCommand com2 = new SqlCeCommand(sql, con);
                        int album_id = Convert.ToInt32(com2.ExecuteScalar());
                        if (album_id == 0)
                        {
                            sql = "INSERT INTO Albums (name) values(@Name); ";
                            SqlCeCommand cmd = new SqlCeCommand(sql, con);
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar);
                            cmd.Parameters["@Name"].Value = song.album;
                            cmd.ExecuteScalar();
                            sql = "SELECT id FROM Albums WHERE name='" +
                            song.album + "'";
                            album_id = Convert.ToInt32(com2.ExecuteScalar());
                        }
                        if (String.IsNullOrEmpty(song.artist))
                        {
                            song.artist = "Unknown artist";
                        }
                        sql = "SELECT id FROM Artists WHERE name='" +
                        song.artist + "'";
                        SqlCeCommand com3 = new SqlCeCommand(sql, con);
                        int artist_id = Convert.ToInt32(com3.ExecuteScalar());
                        if (artist_id == 0)
                        {
                            sql = "INSERT INTO Artists (name) values(@Name); ";
                            SqlCeCommand cmd = new SqlCeCommand(sql, con);
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar);
                            cmd.Parameters["@Name"].Value = song.artist;
                            cmd.ExecuteScalar();
                            sql = "SELECT id FROM Artists WHERE name='" +
                            song.album + "'";
                            artist_id = Convert.ToInt32(com3.ExecuteScalar());
                        }
                        sql = "SELECT id FROM Year WHERE year='" +
                        song.year.ToString() + "'";
                        SqlCeCommand com4 = new SqlCeCommand(sql, con);
                        int year_id = Convert.ToInt32(com4.ExecuteScalar());
                        if (year_id == 0)
                        {
                            sql = "INSERT INTO Year (year) values(@Year); ";
                            SqlCeCommand cmd = new SqlCeCommand(sql, con);
                            cmd.Parameters.Add("@Year", SqlDbType.Int);
                            cmd.Parameters["@Year"].Value = song.year;
                            cmd.ExecuteScalar();
                            sql = "SELECT id FROM Year WHERE year='" +
                            song.year.ToString() + "'";
                            year_id = Convert.ToInt32(com4.ExecuteScalar());
                        }
                        sql = "SELECT id FROM Songs WHERE path='" +
                            song.path + "'";
                        SqlCeCommand com5 = new SqlCeCommand(sql, con);
                        int song_id = Convert.ToInt32(com5.ExecuteScalar());
                        if (String.IsNullOrEmpty(song.name))
                        {
                            song.name = "Unknown Track";
                        }
                        if (song_id == 0)
                        {
                            sql = "INSERT INTO Songs (name, folder_id, artist_id, album_id, year_id, path)" +
                                "values (@Name, @Folder_id, @Artist_id, @Album_id, @Year_id, @Path)";
                            SqlCeCommand cmd = new SqlCeCommand(sql, con);
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar);
                            cmd.Parameters["@Name"].Value = song.name;
                            cmd.Parameters.Add("@Folder_id", SqlDbType.Int);
                            cmd.Parameters["@Folder_id"].Value = folder_id;
                            cmd.Parameters.Add("@Artist_id", SqlDbType.Int);
                            cmd.Parameters["@Artist_id"].Value = artist_id;
                            cmd.Parameters.Add("@Album_id", SqlDbType.Int);
                            cmd.Parameters["@Album_id"].Value = album_id;
                            cmd.Parameters.Add("@Year_id", SqlDbType.Int);
                            cmd.Parameters["@Year_id"].Value = year_id;
                            cmd.Parameters.Add("@Path", SqlDbType.NVarChar);
                            cmd.Parameters["@Path"].Value = song.path;
                            cmd.ExecuteScalar();
                        }
                        else
                        {
                            sql = "UPDATE Songs SET (name = @Name, folder_id = @Folder_id, "+
                            "artist_id = @Artist_id, album_id = @Album_id, year_id = @Year_id, path =  @Path)" +
                                "WHERE id = @Song_id";
                            SqlCeCommand cmd = new SqlCeCommand(sql, con);
                            cmd.Parameters.Add("@Name", SqlDbType.NVarChar);
                            cmd.Parameters["@Name"].Value = song.name;
                            cmd.Parameters.Add("@Folder_id", SqlDbType.Int);
                            cmd.Parameters["@Folder_id"].Value = folder_id;
                            cmd.Parameters.Add("@Artist_id", SqlDbType.Int);
                            cmd.Parameters["@Artist_id"].Value = artist_id;
                            cmd.Parameters.Add("@Album_id", SqlDbType.Int);
                            cmd.Parameters["@Album_id"].Value = album_id;
                            cmd.Parameters.Add("@Year_id", SqlDbType.Int);
                            cmd.Parameters["@Year_id"].Value = year_id;
                            cmd.Parameters.Add("@Path", SqlDbType.NVarChar);
                            cmd.Parameters["@Path"].Value = song.path;
                            cmd.Parameters.Add("@Song_id", SqlDbType.Int);
                            cmd.Parameters["@Song_id"].Value = song_id;
                            cmd.ExecuteScalar();
                        }
                    }
                }
                SongQueue.queueMutex.ReleaseMutex();
                if (SongQueue.items.Count == 0)
                {
                    MessageBox.Show("Nothing to do", sleeptime.ToString());
                    Thread.Sleep(sleeptime);
                    sleeptime *= 2;
                }
                else
                {
                    sleeptime = Math.Max(1, sleeptime / 2);
                }
            }
        }

        private static void UpdateSongListOnWork(string dbfilepath)
        {
        }

        public static void ProcessAll(object dbfilepath)
        {
            Thread songInfoUpdater = new Thread(FolderProcessing.UpdateSongInfoOnWork);
            songInfoUpdater.Start((string)dbfilepath);
            foreach (string s in FolderProcessing.FoldersToAdd)
            {
                Thread addfolder = new Thread(FolderProcessing.AddFolderToDB);
                folder_params fp = new folder_params();
                fp.filepath = (string)dbfilepath;
                fp.folderpath = s;
                addfolder.Start(fp);
                addfolder.Join();
            }
            MessageBox.Show("Job Done!");
        }
    }
}
