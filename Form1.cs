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
        private string pathWithEnv;
        private string dirpath;
        private string filename;
        public Form1()
        {
            InitializeComponent();
            pathWithEnv = @"%AllUsersProfile%\myplayer";
            dirpath = Environment.ExpandEnvironmentVariables(pathWithEnv);
            filename = "playerdb.sdf";
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
            FolderProcessing.Init(dirpath + "\\" + filename);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Музыкальный плейер.\nАвтор Константин Козько 2012", "О программе", MessageBoxButtons.OK);
        }

        private void добавитьПапкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderProcessing.FoldersToAdd.Enqueue(folderDialog.SelectedPath);
                Thread processThread = new Thread(FolderProcessing.ProcessAll);
                processThread.IsBackground = true;
                processThread.Start(dirpath + "\\" + filename);
            }
        }

        private void удалитьПапкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> folders = FolderProcessing.GetFolders(dirpath + "\\" + filename);
            Form f = new Form();
            f.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            ListBox l = new ListBox();
            Button bOk = new Button();
            Button bCancel = new Button();
            foreach (string s in folders)
            {
                l.Items.Add(s);
            }
            l.Left = 13;
            l.Top = 13;
            l.Width = f.ClientRectangle.Width - 13;
            l.Height = f.ClientRectangle.Height - 26 - bOk.Height;
            l.HorizontalScrollbar = true;
            l.SelectionMode = SelectionMode.One;
            bOk.Left = 13;
            bOk.Top = f.ClientRectangle.Height - bOk.Height;
            bOk.Text = "Ok";
            bOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            bCancel.Left = f.ClientRectangle.Width - bCancel.Width;
            bCancel.Top = f.ClientRectangle.Height - bOk.Height;
            bCancel.Text = "Cancel";
            bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            f.Controls.Add(l);
            f.Controls.Add(bOk);
            f.Controls.Add(bCancel);
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && l.SelectedItems.Count > 0)
            {
                FolderProcessing.DeleteFolderFromDB(Convert.ToString(l.SelectedItems[0]),
                    dirpath + "\\" + filename);
            }
        }
    }
}
