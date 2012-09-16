using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlServerCe;
using System.Data;
using System.Threading;
using System.Collections.ObjectModel;

namespace MyPlayer
{
	public static class FolderProcessing
	{
		private struct folder_params
		{
			public string folderpath;
			public string filepath;
		}
		public static Queue<string> FoldersToAdd = new Queue<string>();
		private static List<string> folderList = new List<string>(); //List of the folders in the library

		private static List<string> GetFolderFiles(string folderPath)
		{
			List<string> filesList = new List<string>();
            try
            {
                filesList.AddRange(Directory.GetFiles(folderPath, "*.mp3"));
                filesList.AddRange(Directory.GetFiles(folderPath, "*.wav"));
                foreach (string s in Directory.GetDirectories(folderPath))
                {
                    filesList.AddRange(GetFolderFiles(s));
                }
            }
            catch
            {
            }
			return filesList;
		}

	private static SqlCeConnection CreateConnection (string dbfilepath)
	{
			string password = "";
			string connectionString = String.Format("DataSource=\"{0}\"; Password='{1}'",
					dbfilepath, password);
			SqlCeConnection con = new SqlCeConnection(connectionString);
		return con;

	}

		private static void AddFolderToDB(object folder_param)
		{
			string folderPath = ((folder_params)folder_param).folderpath;
		using (SqlCeConnection con = CreateConnection(((folder_params)folder_param).filepath))
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
			}
		}

		public static void DeleteFolderFromDB(string folderPath, string dbFilePath)
		{
			using (SqlCeConnection con = CreateConnection(dbFilePath))
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
				dt.Rows[ind].Delete();
				da.Update(ds, "Folders");
				string sql = "DELETE FROM Songs WHERE folder_id = " + folderid;
				SqlCeCommand com = new SqlCeCommand(sql, con);
				com.ExecuteNonQuery();
			}
		}

        public static List<SongDbItems> GetFilesFromDB(string dbFilePath, string filter="", string order="Name",
            string orderType = "ASC")
		{
			List<SongDbItems> dblist = new List<SongDbItems>();
            string OrderStr = "Songs.Name";
            switch(order)
            {
                case "Name":
                case "name":
                    OrderStr = "Songs.name";
                    break;
                case "Artist":
                case "artist":
                    OrderStr = "Artists.name";
                    break;
                case "Album":
                case "album":
                    OrderStr = "Albums.name";
                    break;
                case "Year":
                case "year":
                    OrderStr = "Year.year";
                    break;
            }
			using (SqlCeConnection con = CreateConnection(dbFilePath))
			{
				con.Open();
                SqlCeDataAdapter da = new SqlCeDataAdapter(
                    "Select Songs.id, " +
                    "Songs.name, Folders.name " +
                    "AS rootdir, " +
                    "Artists.name AS artist, " +
                    "Albums.name AS album, " +
                    "Year.year AS year, " +
                    "Songs.path " +
                    "FROM Songs " +
                    "INNER JOIN Folders ON Folders.id = Songs.folder_id " +
                    "INNER JOIN Artists ON Artists.id = Songs.artist_id " +
                    "INNER JOIN Albums ON Albums.id = Songs.album_id " +
                    "INNER JOIN Year ON Year.id = Songs.year_id " +
                    "WHERE Songs.name LIKE '%" + filter + "%'" +
                    "OR Artists.name LIKE '%" + filter + "%'" +
                    "OR Albums.name LIKE '%" + filter + "%'" +
                    "ORDER BY " + OrderStr + " " + orderType, con);
				DataSet ds = new DataSet("Song");
				DataTable dt = new DataTable("Songs");
				dt.Columns.Add(new DataColumn("id", typeof(int)));
				dt.Columns.Add(new DataColumn("name", typeof(string)));
				dt.Columns.Add(new DataColumn("rootdir", typeof(string)));
				dt.Columns.Add(new DataColumn("artist", typeof(string)));
				dt.Columns.Add(new DataColumn("album", typeof(string)));
				dt.Columns.Add(new DataColumn("year", typeof(int)));
				dt.Columns.Add(new DataColumn("path", typeof(string)));
				ds.Tables.Add(dt);
				da.Fill(ds, "Songs");
				foreach (DataRow dr in ds.Tables["Songs"].Rows)
				{
					SongDbItems item = new SongDbItems();
					item.id = (int) dr["id"];
					item.name = dr["name"].ToString();
					item.rootdir = dr["rootdir"].ToString();
					item.artist = dr["artist"].ToString();
					item.album = dr["album"].ToString();
					if (dr["year"] != null)
					{
						item.year = (int)dr["year"];
					}
					else
					{
						item.year = 0;
					}
					item.path = dr["path"].ToString();
					dblist.Add(item);
				}
			}
			return dblist;

		}

        public static ObservableCollection<SongDbItems> GetObservableFromDB(string dbFilePath, string filter = "",
            string order = "Name", string orderType = "ASC")
        {
            return new ObservableCollection<SongDbItems>(GetFilesFromDB(dbFilePath, filter, order, orderType));
        }

		private static void GetFoldersFromDB(string dbfilepath)
		{
			using (SqlCeConnection con = CreateConnection(dbfilepath))
			{
				con.Open();
				SqlCeDataAdapter da = new SqlCeDataAdapter("Select * FROM Folders", con);
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
			}
		}

		private static void UpdateSongInfoOnWork(object filepath)
		{
			int sleeptime = 1;
			while (true)
			{
				try
				{
					SongQueue.queueMutex.WaitOne();
					SongDbItems song = new SongDbItems();
					if (SongQueue.items.Count > 0)
					{
						song = SongQueue.items.Dequeue();
						using (SqlCeConnection con = CreateConnection(Convert.ToString(filepath)))
						{
							con.Open();
							string filedir = Path.GetDirectoryName(song.path);
							string sql = "SELECT id FROM Folders WHERE name='" +
							song.rootdir + "'";
							SqlCeCommand com = new SqlCeCommand(sql, con);
							int folder_id = Convert.ToInt32(com.ExecuteScalar());
							if (folder_id == 0)
							{
                                return;
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
								sql = "UPDATE Songs SET name = @Name, folder_id = @Folder_id, " +
								"artist_id = @Artist_id, album_id = @Album_id, year_id = @Year_id, path =  @Path " +
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
				}
				catch
				{
					try
					{
						SongQueue.queueMutex.ReleaseMutex();
					}
					catch
					{

					}
				}
				if (SongQueue.items.Count == 0)
				{
					Thread.Sleep(sleeptime);
					sleeptime = Math.Min(1000, sleeptime * 2);
				}
				else
				{
					sleeptime = Math.Max(1, sleeptime / 2);
				}
			}
		}

		private static void UpdateSongListOnWork(object filepath)
		{
			
			int sleeptime = 1;
			while (true)
			{
				GetFoldersFromDB(Convert.ToString(filepath));
				foreach (string folder in folderList)
				{
					List<string> filelist = GetFolderFiles(folder);
					using (SqlCeConnection con = CreateConnection(Convert.ToString(filepath)))
					{
						con.Open();
						List<string> dblist = new List<string>();
						SqlCeDataAdapter da = new SqlCeDataAdapter("Select * FROM Songs", con);
						DataSet ds = new DataSet("Song");
						DataTable dt = new DataTable("Songs");
						dt.Columns.Add(new DataColumn("id", typeof(int)));
						dt.Columns.Add(new DataColumn("name", typeof(string)));
						dt.Columns.Add(new DataColumn("folder_id", typeof(int)));
						dt.Columns.Add(new DataColumn("artist_id", typeof(int)));
						dt.Columns.Add(new DataColumn("album_id", typeof(int)));
						dt.Columns.Add(new DataColumn("year_id", typeof(int)));
						dt.Columns.Add(new DataColumn("path", typeof(string)));
						ds.Tables.Add(dt);
						da.Fill(ds, "Songs");
						foreach (DataRow dr in ds.Tables["Songs"].Rows)
						{
							dblist.Add(dr["path"].ToString());
						}
						foreach (string li in filelist)
						{
							if (!dblist.Contains(li.Replace("'", "`")))
							{
								SongProcessing.ProcessSong(li, folder);
							}
						}
						foreach (string li in dblist)
						{
							if (!filelist.Contains(li.Replace("`", "'")))
							{
								string sql = "SELECT Folders.name FROM " +
									"Songs INNER JOIN Folders ON Songs.folder_id = Folders_id " +
									"WHERE Songs.path ='" + li + "'";
								string folder_name = "";
								try
								{
									SqlCeCommand cmd = new SqlCeCommand(sql, con);
                                    object o = cmd.ExecuteScalar();
									folder_name = Convert.ToString(o);
								}
								catch
								{

								}
								if (folder == folder_name)
								{
									sql = "Delete FROM Songs WHERE path='" +
										li + "'";
									try
									{
										SqlCeCommand cmd = new SqlCeCommand(sql, con);
										cmd.ExecuteNonQuery();
									}
									catch
									{

									}
								}
							}
						}

					}
				}
				if (SongQueue.items.Count == 0)
				{
					Thread.Sleep(sleeptime);
					sleeptime = Math.Min(1000, sleeptime * 2);
				}
				else
				{
					sleeptime = Math.Max(1, sleeptime / 2);
				}
			}
		}

		public static void Init(object filepath)
		{
            string dbfilepath = filepath as string;
            GetFoldersFromDB(dbfilepath);
			Thread songInfoUpdater = new Thread(FolderProcessing.UpdateSongInfoOnWork);
			songInfoUpdater.IsBackground = true;
            songInfoUpdater.Start(dbfilepath);
			Thread songListUpdater = new Thread(FolderProcessing.UpdateSongListOnWork);
			songListUpdater.IsBackground = true;
			songListUpdater.Start(dbfilepath);
		}

		public static void ProcessAll(object dbfilepath)
		{
			foreach (string s in FolderProcessing.FoldersToAdd)
			{
				Thread addfolder = new Thread(FolderProcessing.AddFolderToDB);
				folder_params fp = new folder_params();
                fp.filepath = dbfilepath as string;
				fp.folderpath = s;
				addfolder.IsBackground = true;
				addfolder.Start(fp);
				addfolder.Join();
			}
		}

		public static List<string> GetFolders
		{
			get
			{
				return folderList;
			}
		}
	}
}
