using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlServerCe;
using System.Threading;

namespace myplayer
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string pathWithEnv = @"%AllUsersProfile%\myplayer";
            string dirpath = Environment.ExpandEnvironmentVariables(pathWithEnv);
            string filename = "playerdb.sdf";
            if (!DbInitializer.InitDb(dirpath, filename))
            {
                MessageBox.Show("Database cannot be initialized. " +
                    "Program will be now closed", "Warning", MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                this.Close();
            }
            if (!DbInitializer.InitTables(dirpath, filename))
            {
                MessageBox.Show("Database tables cannot be initialized. " +
                    "Program will be now closed", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
            FolderProcessing.FoldersToAdd.Enqueue("F:\\netbook\\mydocs\\My Music");
            Thread processThread = new Thread(FolderProcessing.ProcessAll);
            processThread.Start(dirpath + "\\" + filename);
            //FolderProcessing.ProcessAll(dirpath + "\\" + filename);
            //FolderProcessing.DeleteFolderFromDB("F:\\vm\\", dirpath + "\\" + filename);
            //SongProcessing.ProcessSong(
                //"F:\\netbook\\mydocs\\My Music\\+ Кипелов\\2005 - Реки Времен\\03 - Пророк.mp3");
        }
    }
}
