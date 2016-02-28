using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Security.AccessControl;
using MonoTorrent.Client;
using MonoTorrent.Tracker;
using MonoTorrent.Common;
using MonoTorrent.Client.Encryption;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Series\\serie_cat.txt";
        string rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Series\\";
        string UrlHtml = "https://kat.cr/usearch/";

        public Form1()
        {
            InitializeComponent();
            add.Click += new EventHandler(this.add_click);

            clear_file.Click += new EventHandler(Clear_File);
            warning.Visible = false;
            string line;

           /* ClientEngine engine = new ClientEngine(new EngineSettings());

             Torrent torrent = Torrent.Load("https://torcache.net/torrent/DB78E4B00DE2FC479F27B4920E2ABC447E9229B9.torrent?title=[kat.cr]arrow.s04e14.hdtv.x264.lol.ettv");
             //TorrentManager manager = new TorrentManager(torrent, System.AppDomain.CurrentDomain.BaseDirectory, new TorrentSettings());
             TorrentManager manager = new TorrentManager(InfoHash.FromHex("DB78E4B00DE2FC479F27B4920E2ABC447E9229B9"), downloadsPath, torrentDefaults, downloadsPathForTorrent);
             engine.Register(manager);

             manager.Start();*/
            if (File.Exists(path))
            {

                using (StreamReader sr = File.OpenText(path))
                {

                    while (((line = sr.ReadLine()) != null))
                    {

                        Series.Items.Add(line);
                    }

                    sr.Close();
                }
            }

            List<string> ser = new List<string>();
            foreach (string s in Series.Items)
            {

                ser.Add(s);
            }

            backgroundWorker1.RunWorkerAsync(ser);
        }


        void add_click(Object sender, EventArgs e)//a function that adds through the textbox add_serie a new element in listbox Series,also an eventhandler for clicking button add
        {

            warning.Visible = false;
            if (add_serie.Text != "")
            {

                if (!Series.Items.Contains(add_serie.Text))
                {

                    string subFolderPath = Path.Combine(rootFolder, add_serie.Text);
                    Directory.CreateDirectory(subFolderPath);
                    Series.Items.Add(add_serie.Text);
                    
                    backgroundWorker2.RunWorkerAsync(add_serie.Text);
                    add.Enabled = false;

                    if (!File.Exists(path))
                    {

                        using (StreamWriter sw = File.CreateText(path))
                        {

                            sw.WriteLine(add_serie.Text);
                            sw.Close();
                        }
                    }else {

                        using (StreamWriter sw = File.AppendText(path))
                        {

                            sw.WriteLine(add_serie.Text);
                            sw.Close();
                        }
                    }
                }else
                {

                    warning.Visible = true;
                    warning.Text = "This serie already exists";
                }
                add_serie.Text = "";
            }
            else
            {

                warning.Visible = true;
                warning.Text = "Name a Serie";
            }
        }

        void Clear_File(Object sender, EventArgs e)//a function that deletes all files and folders in rootFolder,also a eventhandler for clicking button clear_file
        {
            DirectoryInfo di = new DirectoryInfo(rootFolder);
            int firstTime = 1;
            
            while (di.FullName + "\\" != rootFolder || firstTime == 1) {
                firstTime = 0;
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                if (di.GetDirectories().Length != 0) {

                    di = di.GetDirectories().First();
                }else
                {
                    DirectoryInfo temp = di;
                    di = di.Parent;
                    temp.Delete(true);
                }
            }
            File.Delete(path);
            Series.Items.Clear();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {



            List<string> ser = e.Argument as List<string>;

            //non used var for progress count
            int i = 1;
            int j = 0;
            List<string> magnets = new List<string>();//the lists where the magnets are saved
            MyWebClient f1 = new MyWebClient();//make a modified webclient 
            foreach (string s in ser)//for every show
            {
                i = 1;
                while (i < 3)
                {
                    j = 0;
                    while (j < 23)
                    {
                        try//download html,find the magnet with most seeders and save it to list magnets
                        {

                            string html = f1.DownloadString(UrlHtml + s + " s" + (i).ToString("00") + "e" + (++j).ToString("00") + "?field=seeders&sorder=desc");

                            Match m1 = Regex.Match(html, "href=\"magnet:\\s*(.+?)\\s*\"", RegexOptions.Singleline);

                            magnets.Add("magnet:" + m1.Groups[1].Value);
                            f1.Dispose();

                        }
                        catch (Exception f)
                        {

                            magnets.Add(f.Message);

                        }
                    }
                    i++;
                }

            }
            while (magnets.Count != ser.Count * (i - 1) * j)
            {

            }
            e.Result = magnets;




        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listBox1.Items.Add("egw");
            if (e.Result is string)
            {
                string a = e.Result as string;
                warning.Visible = true;
                warning.Text = a;
            }
            else {
                List<string> mas = e.Result as List<string>;
                foreach (string a in mas)
                {
                    try
                    {
                        listBox1.Items.Add(a);
                        
                    }
                    catch (Exception f)
                    {
                        warning.Text = f.Message;
                    }

                }
            }
            // add.Enabled = true;
            // clear_file.Enabled = true;
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(rootFolder + Series.SelectedItem);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            di.Delete();
            Series.Items.Remove(Series.SelectedItem);
            File.Delete(path);

            using (StreamWriter sw = File.AppendText(path))
            {
                foreach (string str in Series.Items)
                {

                    sw.WriteLine(str);
                }

                sw.Close();
            }


        }


        class MyWebClient : WebClient
        {

            public MyWebClient()
            {

            }


            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            string se = e.Argument as string;

            
            int i = 1;
            int j = 0;
            List<string> magnets = new List<string>();//the lists where the magnets are saved
            MyWebClient f2 = new MyWebClient();//make a modified webclient 
            
            
                i = 1;
                while (i < 3)
                {
                    j = 0;
                    while (j < 23)
                    {
                        try//download html,find the magnet with most seeders and save it to list magnets
                        {

                            string html = f2.DownloadString(UrlHtml + se + " s" + (i).ToString("00") + "e" + (++j).ToString("00") + "?field=seeders&sorder=desc");

                            Match m1 = Regex.Match(html, "href=\"magnet:\\s*(.+?)\\s*\"", RegexOptions.Singleline);

                            magnets.Add("magnet:" + m1.Groups[1].Value);
                            f2.Dispose();

                        }
                        catch (Exception ex)
                        {

                            magnets.Add(ex.Message);
                        }
                    }
                    i++;
                }

            
            while (magnets.Count != (i - 1) * j)
            {

            }
            e.Result = magnets;

        }

        private void backgroundWorker2_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
        {
            listBox1.Items.Add("egw");
            if (e.Result is string)
            {
                string a = e.Result as string;
                warning.Visible = true;
                warning.Text = a;
            }
            else {
                List<string> mas = e.Result as List<string>;
                foreach (string a in mas)
                {
                    try
                    {
                        listBox1.Items.Add(a);
                    }
                    catch (Exception f)
                    {
                        warning.Text = f.Message;
                    }

                }
            }
            add.Enabled = true;
        }
        
    }
}


