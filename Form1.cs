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

        private void Init()
        {
            pictureBox1.Left = 3;
            pictureBox1.Top = 3;
            pictureBox1.Image.Tag = "Backward";
            pictureBox2.Top = 3;
            pictureBox2.Left = pictureBox1.Left + pictureBox1.Width + 3;
            pictureBox1.Height = pictureBox1.Image.Height + 6;
            pictureBox2.Height = pictureBox1.Height;
            pictureBox2.Image.Tag = "Play";
            pictureBox3.Top = 3;
            pictureBox3.Left = 3 + pictureBox2.Left + pictureBox2.Width;
            pictureBox3.Height = pictureBox1.Height;
            pictureBox3.Image.Tag = "Forward";
            panel1.Top = menuStrip1.Top + menuStrip1.Height;
            panel1.Left = 0;
            panel1.Height = pictureBox1.Height;
            textBox1.Top = (panel1.Height - textBox1.Height) >> 1;
            label1.Top = (panel1.Height - label1.Height) >> 1;
            listBox1.Top = panel1.Top + panel1.Height - 1;
            listBox1.Left = 0;
            listBox1.Items.Add("All music");
            listBox2.Top = listBox1.Top;
        }

        public Form1()
        {
            InitializeComponent();
            Init();
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

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            panel1.Width = this.ClientRectangle.Width;
            textBox1.Left = this.ClientRectangle.Width - textBox1.Width-10;
            label1.Left = ((pictureBox3.Left + pictureBox3.Width) - label1.Width >> 1) +
                (textBox1.Left - (pictureBox3.Left + pictureBox3.Width)) >> 1;
            this.MinimumSize = new Size(pictureBox3.Left + pictureBox3.Width + textBox1.Width + 100, 
                panel1.Height+menuStrip1.Height+(this.Bounds.Height-ClientRectangle.Height)+ 50);
            listBox1.Height = Bounds.Height - (this.Location.Y - listBox1.Location.Y);
            listBox1.Width = Math.Max(150, ClientRectangle.Width >> 2);
            listBox2.Left = listBox1.Width;
            listBox2.Height = listBox1.Height;
            listBox2.Width = ClientRectangle.Width - listBox1.Width;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            pictureBox2.Focus();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image.Tag.ToString() == "Play")
            {
                pictureBox2.Image = Properties.Resources.pauseButton;
                pictureBox2.Image.Tag = "Pause";
            }
            else
            {
                pictureBox2.Image = Properties.Resources.playButton;
                pictureBox2.Image.Tag = "Play";
            }
        }
    }
}
