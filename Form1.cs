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
using System.Runtime.InteropServices;

namespace MyPlayer
{

    public partial class Form1 : Form
    {
        private string pathWithEnv;
        private string dirpath;
        private string filename;
        private string nowPlaying;
        private string commandString;
        private string filter = "";

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string lpstrCommand, StringBuilder lpstrReturnString,
        int uReturnLength, IntPtr hwndCallback);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Альбом")]
        private void Init()
        {
            pathWithEnv = @"%AllUsersProfile%\myplayer";
            dirpath = Environment.ExpandEnvironmentVariables(pathWithEnv);
            filename = "playerdb.sdf";
            nowPlaying = "";
            if (!DBInitializer.InitDB(dirpath, filename))
            {
                MessageBox.Show("Database cannot be initialized. " +
                    "Program will be now closed", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
            if (!DBInitializer.InitTables(dirpath, filename))
            {
                MessageBox.Show("Database tables cannot be initialized. " +
                    "Program will be now closed", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
            FolderProcessing.Init(dirpath + "\\" + filename);
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
            listBox1.Items.Add("Вся музыка");
            listView1.Top = listBox1.Top;
            listView1.Columns.Add("id");
            listView1.Columns.Add("Название");
            listView1.Columns.Add("rootdir");
            listView1.Columns.Add("Исполнитель");
            listView1.Columns.Add("Альбом");
            listView1.Columns.Add("Год");
            listView1.Columns.Add("path");
            listView1.View = View.Details;
        }

        public Form1()
        {
            InitializeComponent();
            Init();
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
            List<string> folders = FolderProcessing.GetFolders;
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
            listBox1.Height = ClientRectangle.Height - listBox1.Top - statusStrip1.Height + 10;
            listBox1.Width = Math.Max(150, ClientRectangle.Width >> 2);
            listView1.Left = listBox1.Width;
            listView1.Height = listBox1.Height - 10;
            listView1.Width = ClientRectangle.Width - listBox1.Width;
            listView1.Columns[0].Width = 0;
            listView1.Columns[1].Width = (listView1.Width - 50) / (listView1.Columns.Count - 4);
            listView1.Columns[2].Width = 0;
            listView1.Columns[3].Width = (listView1.Width - 50) / (listView1.Columns.Count - 4);
            listView1.Columns[4].Width = (listView1.Width - 50) / (listView1.Columns.Count - 4);
            listView1.Columns[5].Width = 50;
            listView1.Columns[6].Width = 0;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            pictureBox2.Focus();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image.Tag.ToString() == "Play")
            {
                if (String.IsNullOrEmpty(nowPlaying) || 
                    (listView1.SelectedItems.Count>0 &&
                    listView1.SelectedItems[0].SubItems
                    [listView1.SelectedItems[0].SubItems.Count - 1].Text != nowPlaying))
                {
                    if (!String.IsNullOrEmpty(nowPlaying))
                    {
                        switch (Path.GetExtension(nowPlaying))
                        {
                            case ".mp3":
                                commandString = "close Mp3File";
                                break;
                            case ".wav":
                                commandString = "close WavFile";
                                break;
                        }
                        mciSendString(commandString, null, 0, IntPtr.Zero);
                    }
                    if (listView1.Items.Count > 0)
                    {
                        if (listView1.SelectedItems.Count > 0)
                        {
                            nowPlaying = listView1.SelectedItems[0].SubItems
                                [listView1.SelectedItems[0].SubItems.Count - 1].Text;
                        }
                        else
                        {
                            nowPlaying = listView1.Items[0].SubItems
                                [listView1.Items[0].SubItems.Count - 1].Text;
                            listView1.Items[0].Selected = true;
                        }
                        label1.Text = listView1.SelectedItems[0].SubItems[3].Text +
                            " - " + listView1.SelectedItems[0].SubItems[1].Text;
                        label1.Left = ((pictureBox3.Left + pictureBox3.Width) - label1.Width >> 1) +
                (textBox1.Left - (pictureBox3.Left + pictureBox3.Width)) >> 1;
                        switch (Path.GetExtension(nowPlaying))
                        {
                            case ".mp3":
                                commandString = "open " + "\"" + nowPlaying + "\"" +
                                " type MPEGVideo alias Mp3File";
                                break;
                            case ".wav":
                                commandString = "open " + "\"" + nowPlaying + "\"" +
                                " type waveaudio alias WavFile";
                                break;
                        }
                        mciSendString(commandString, null, 0, IntPtr.Zero);

                    }
                    else
                    {
                        return;
                    }
                }
                listView1.Focus();
                pictureBox2.Image = Properties.Resources.pauseButton;
                pictureBox2.Image.Tag = "Pause";
                switch (Path.GetExtension(nowPlaying))
                {
                    case ".mp3":
                        commandString = "play Mp3File";
                        break;
                    case ".wav":
                        commandString = "play WavFile";
                        break;
                }
                mciSendString(commandString, null, 0, IntPtr.Zero);
            }
            else
            {
                pictureBox2.Image = Properties.Resources.playButton;
                pictureBox2.Image.Tag = "Play";
                switch (Path.GetExtension(nowPlaying))
                {
                    case ".mp3":
                        commandString = "pause Mp3File";
                        break;
                    case ".wav":
                        commandString = "pause WavFile";
                        break;
                }
                mciSendString(commandString, null, 0, IntPtr.Zero);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            List<SongDbItems> list = FolderProcessing.GetFilesFromDB(dirpath + "\\" + filename, filter);
            string selected_id = "";
            if (list.Count != listView1.Items.Count)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    selected_id = listView1.SelectedItems[0].Text;
                }
                listView1.Items.Clear();
                foreach (SongDbItems si in list)
                {
                    ListViewItem it = new ListViewItem(si.id.ToString());
                    it.SubItems.Add(si.name);
                    it.SubItems.Add(si.rootdir);
                    it.SubItems.Add(si.artist);
                    it.SubItems.Add(si.album);
                    if (si.year != 0)
                    {
                        it.SubItems.Add(si.year.ToString());
                    }
                    else
                    {
                        it.SubItems.Add(" ");
                    }
                    it.SubItems.Add(si.path);
                    listView1.Items.Add(it);
                }
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].Text == selected_id)
                    {
                        listView1.Items[i].Selected = true;
                        break;
                    }
                }
            }
            statusStrip1.Items.Clear();
            statusStrip1.Items.Add("Песен: " + listView1.Items.Count.ToString());
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string selected_id = listView1.SelectedItems[0].Text;
                for (int i = 0; i < listView1.Items.Count-1; i++)
                {
                    if (listView1.Items[i].Text == selected_id)
                    {
                        listView1.Items[i].Selected = false;
                        listView1.Items[i+1].Selected = true;
                        break;
                    }
                }
            }
            else
            {
                listView1.Items[0].Selected = true;
            }
            listView1.EnsureVisible(listView1.SelectedIndices[0]);
            pictureBox2.Image.Tag = "Play";
            pictureBox2_Click(sender, e);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string selected_id = listView1.SelectedItems[0].Text;
                for (int i = 1; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].Text == selected_id)
                    {
                        listView1.Items[i].Selected = false;
                        listView1.Items[i - 1].Selected = true;
                        break;
                    }
                }
            }
            else
            {
                listView1.Items[0].Selected = true;
            }
            listView1.EnsureVisible(listView1.SelectedIndices[0]);
            pictureBox2.Image.Tag = "Play";
            pictureBox2_Click(sender, e);
        }

        private void создатьПлейлистToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("not realized yet");
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            string str = "";
            switch (Path.GetExtension(nowPlaying))
            {
                case ".mp3":
                    str = "Mp3File";
                    break;
                case ".wav":
                    str = "WavFile";
                    break;
            }
            StringBuilder buffer = new StringBuilder(128);
            commandString = "Status " + str + " position";
            mciSendString(commandString, buffer, 128, IntPtr.Zero);
            int currPos = 0;
            int length = 0;
            if (!String.IsNullOrEmpty(buffer.ToString()))
            {
                currPos = int.Parse(buffer.ToString()) / 1000;
            }
            commandString = "Status " + str + " length";
            mciSendString(commandString, buffer, 128, IntPtr.Zero);
            if (!String.IsNullOrEmpty(buffer.ToString()))
            {
                length = int.Parse(buffer.ToString()) / 1000;
            }
            if (currPos == length && length != 0)
            {
                pictureBox3_Click(sender, e);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text) && 
                listBox1.Items.Count == 1 || listBox1.Items[1] != "Результаты поиска")
            {
                listBox1.Items.Insert(1, "Результаты поиска");
                listBox1.SelectedIndex = 1;
            }
            filter = textBox1.Text;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == 0)
            {
                listBox1.Items.Remove("Результаты поиска");
                filter = "";
                textBox1.Text = "";
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            pictureBox2.Image.Tag = "Play";
            pictureBox2_Click(sender, e);
        }

        private void изменитьДанныеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            f.Text = "Сведения о композиции";
            f.Height += 20;
            f.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            TextBox tbName = new TextBox();
            TextBox tbArtist = new TextBox();
            TextBox tbAlbum = new TextBox();
            TextBox tbYear = new TextBox();
            Label lbName = new Label();
            Label lbArtist = new Label();
            Label lbAlbum = new Label();
            Label lbYear = new Label();
            Button bOk = new Button();
            Button bCancel = new Button();

            tbName.Left = 13;
            tbAlbum.Left = 13;
            tbArtist.Left = 13;
            tbYear.Left = 13;
            lbName.Left = 13;
            lbAlbum.Left = 13;
            lbArtist.Left = 13;
            lbYear.Left = 13;
            bOk.Left = 13;
            bCancel.Left = f.ClientRectangle.Width - bCancel.Width - 13;

            lbName.Text = "Название";
            lbAlbum.Text = "Альбом";
            lbArtist.Text = "Исполнитель";
            lbYear.Text = "Год";
            tbName.Text = listView1.SelectedItems[0].SubItems[1].Text;
            tbArtist.Text = listView1.SelectedItems[0].SubItems[3].Text;
            tbAlbum.Text = listView1.SelectedItems[0].SubItems[4].Text;
            tbYear.Text = listView1.SelectedItems[0].SubItems[5].Text;
            bOk.Text = "Ok";
            bCancel.Text = "Отмена";

            lbName.Top = 13;
            tbName.Top = lbName.Top + lbName.Height + 5;
            lbArtist.Top = tbName.Top + tbName.Height + 13;
            tbArtist.Top = lbArtist.Top + lbArtist.Height + 5;
            lbAlbum.Top = tbArtist.Top + tbArtist.Height + 13;
            tbAlbum.Top = lbAlbum.Top + lbAlbum.Height + 5;
            lbYear.Top = tbAlbum.Top + tbAlbum.Height + 13;
            tbYear.Top = lbYear.Top + tbYear.Height + 5;
            bOk.Top = f.ClientRectangle.Height - bOk.Height;
            bCancel.Top = bOk.Top;

            tbName.Width = f.ClientRectangle.Width - 13;
            tbAlbum.Width = tbName.Width;
            tbArtist.Width = tbName.Width;
            tbYear.Width = tbName.Width;

            bOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            f.Controls.Add(tbName);
            f.Controls.Add(tbAlbum);
            f.Controls.Add(tbArtist);
            f.Controls.Add(tbYear);
            f.Controls.Add(lbName);
            f.Controls.Add(lbAlbum);
            f.Controls.Add(lbArtist);
            f.Controls.Add(lbYear);
            f.Controls.Add(bOk);
            f.Controls.Add(bCancel);

            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SongDbItems song = new SongDbItems();
                song.name = tbName.Text;
                song.album = tbAlbum.Text;
                song.artist = tbArtist.Text;
                song.year = Convert.ToInt32(tbYear.Text);
                song.rootdir = listView1.SelectedItems[0].SubItems[2].Text;
                song.path = listView1.SelectedItems[0].SubItems
                    [listView1.Columns.Count - 1].Text;
                SongProcessing.SetSongParams(song.path, song);
                timer1.Enabled = false;
                Thread.Sleep(1500);
                listView1.Items[listView1.SelectedIndices[0]].Remove();
                timer1_Tick(sender, e);
                timer1.Enabled = true;
            }
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[listView1.Columns.Count-1].Text
                    == nowPlaying)
                {
                    listView1.Items[i].Selected = true;
                    listView1.EnsureVisible(listView1.SelectedIndices[0]);
                    break;
                }
            }
        }
    }
}
