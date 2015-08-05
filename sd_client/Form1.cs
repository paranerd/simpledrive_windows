using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace sd_client
{
    public partial class Form1 : Form
    {
        static string config_path = System.Reflection.Assembly.GetExecutingAssembly().Location + ".config";
        private static System.Timers.Timer timer = new System.Timers.Timer();
        private static Properties.Settings settings;

        static string userdir = "";
        static bool sync_in_progress = false;

        public Form1()
        {
            InitializeComponent();
            settings = Properties.Settings.Default;
            Version vs = Environment.OSVersion.Version;
            string path = Environment.GetEnvironmentVariable("USERPROFILE") + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\simpleDrive.lnk";
            simpledrive.create_link(path, System.Reflection.Assembly.GetExecutingAssembly().Location);
            notifyIcon1.Text = "simpleDrive Sync Client";

            if (!File.Exists(config_path))
            {
                status.Text = "Config not found";
                blockText(true);
                return;
            }

            // Preset values
            server_input.Text = (settings.server != "") ? settings.server : "";
            user_input.Text = (settings.user != "") ? settings.user : "";
            pass_input.Text = (settings.pass != "") ? settings.pass : "";
            folder_input.Text = (settings.folder != "") ? new FileInfo(settings.folder).Name : "";
            userdir = (settings.folder != "") ? settings.folder : "";

            timer.Interval = 3000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);

            if(server_input.Text != "" && user_input.Text != "" && pass_input.Text != "" && userdir != "")
            {
                timer.Start();
            }

            notifyIcon1.BalloonTipTitle = "simpleDrive Sync Client";
            if (timer.Enabled)
            {
                blockText(true);
                status.ForeColor = Color.Green;
                status.Text = "Sync service running...";
                connect.Text = "Disconnect";

                notifyIcon1.BalloonTipText = "Sync client started";
            }
            else
            {
                connect.Text = "Connect";
                notifyIcon1.BalloonTipText = "Setup your connection";
            }
            notifyIcon1.ShowBalloonTip(2000);
        }

        public async void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            await sync();
        }

        public async Task sync()
        {
            if (!sync_in_progress)
            {
                notifyIcon1.Text = "Sync in progress...";
                sync_in_progress = true;
                if (!File.Exists(config_path))
                {
                    notifyIcon1.Text = "simpleDrive Sync Client";
                    sync_in_progress = false;
                    return;
                }

                string server = settings.server;
                string user = settings.user;
                string pass = settings.pass;
                string folder = settings.folder;

                if(server == null || user == null || pass == null || folder == null)
                {
                    notifyIcon1.Text = "Config not complete";
                    status.Text = "Config not complete";
                    sync_in_progress = false;
                    return;
                }

                if (!Directory.Exists(folder))
                {
                    notifyIcon1.Text = "Sync folder does not exist";
                    status.Text = "Sync folder does not exist";
                    sync_in_progress = false;
                    return;
                }

                string lastsync = (settings.lastsync != null) ? settings.lastsync : "0";
                string success = await simpledrive.sync(server, user, pass, folder, lastsync);
                notifyIcon1.Text = (success == "") ? "Login failed" : (success == null) ? "Connection error" : "Last sync " + DateTime.Now.ToString("dd.MM.yyy - hh:mm");
                if (success != "" && success != null)
                {
                    var settings = new Dictionary<string, string>
                     {
                         { "lastsync", "" + (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}
                     };
                    writeSettings(settings);
                }
                sync_in_progress = false;
            }
        }

        private void writeSettings(Dictionary<string, string> keyvalue)
        {
            try
            {
                foreach (KeyValuePair<string, string> entry in keyvalue)
                {
                    settings[entry.Key] = entry.Value;
                }
                settings.Save();
            }
            catch (Exception exp)
            {
                // Do something
            }
        }

        private void blockText(bool block)
        {
            BorderStyle style = (block) ? BorderStyle.None : BorderStyle.FixedSingle;
            server_input.ReadOnly = block;
            user_input.ReadOnly = block;
            pass_input.ReadOnly = block;
            server_input.BorderStyle = style;
            user_input.BorderStyle = style;
            pass_input.BorderStyle = style;
        }

        private void browse_Click(object sender, EventArgs e)
        {
            if (!server_input.ReadOnly && folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                userdir = folderBrowserDialog1.SelectedPath;
                folder_input.Text = new FileInfo(userdir).Name;
            }
        }

        private void connect_Click(object sender, EventArgs e)
        {
            if (!File.Exists(config_path))
            {
                status.Text = "Config not found";
                blockText(true);
                return;
            }
            status.Text = "";
            status.ForeColor = Color.Red;

            if (timer.Enabled)
            {
                status.Text = "Sync service stopped";
                connect.Text = "Connect";
                blockText(false);
                timer.Stop();
            }
            else
            {
                if (server_input.Text == "" || user_input.Text == "" || pass_input.Text == "" || userdir == "")
                {
                    status.Text = "No blank fields!";
                    return;
                }

                blockText(true);
                status.Text = "Connecting...";
                string login = simpledrive.login(server_input.Text, user_input.Text, pass_input.Text);
                if (login == null)
                {
                    status.Text = "Connection error";
                    blockText(false);
                }
                else if (login == "1")
                {
                    var settings = new Dictionary<string, string>
                    {
                        { "server", server_input.Text },
                        { "user", user_input.Text },
                        { "pass", pass_input.Text },
                        { "folder", userdir }
                    };
                    writeSettings(settings);
                    timer.Start();
                    status.ForeColor = Color.Green;
                    status.Text = "Sync service running...";
                    connect.Text = "Disconnect";
                    Version vs = Environment.OSVersion.Version;
                    string path = (vs.Major == 6 && vs.Minor == 1 /* Win7 */) ? System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\Favorites\simpleDrive.lnk" : System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\Links\simpleDrive.lnk";
                    simpledrive.create_link(path, userdir);
                }
                else
                {
                    status.Text = "Login failed";
                    blockText(false);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Show();
            }
        }

        private async void synchronizeNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await sync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void simpleDriveInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", userdir);
        }
    }
}
