using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;

namespace G930Helper
{
    public partial class Form1 : Form
    {

        public TimeSpan TimeoutToHide { get; private set; }
        public bool IsSet { get; private set; }
        public uint LastIdleTime { get; private set; }
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        public Form1()
        {
            this.components = new Container();
            this.contextMenu1 = new ContextMenu();
            this.menuItem1 = new MenuItem();
            this.notifyIcon = new NotifyIcon(this.components);


            this.contextMenu1.MenuItems.AddRange(
                        new MenuItem[] { this.menuItem1 });

            this.menuItem1.Index = 0;
            this.menuItem1.Text = "E&xit";
            menuItem1.Click += new EventHandler(menuItem1_Click);

            this.notifyIcon = new NotifyIcon(this.components);


            notifyIcon.Icon = G930Helper.Properties.Resources.logihelp;

            notifyIcon.ContextMenu = this.contextMenu1;

            notifyIcon.Text = "Logitech G930 Helper";

            notifyIcon.Click += new EventHandler(notifyIcon_Click);
            InitializeComponent();
            TimeoutToHide = TimeSpan.FromMinutes(5);
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(timer1_Tick);
            aTimer.Interval = 5000;
            aTimer.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.SizeChanged += new EventHandler(frmMain_Resize);
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk.GetValue("LTHelper") != null)
            {
                checkBox1.Checked = true;
                if (rk.GetValue("LTHelper").ToString() != System.Reflection.Assembly.GetExecutingAssembly().Location)
                {
                    rk.SetValue("LTHelper", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                }
            }
        }


        private void SetInterval(string time)
        {
            // instantiate XmlDocument and load XML from file
            XmlDocument doc = new XmlDocument();
            doc.Load(@"C:\Program Files\Logitech Gaming Software\Resources\G930\Manifest\Device_Manifest.xml");

            XmlNodeList elementList = doc.GetElementsByTagName("battery");
            elementList[0].Attributes["turnOffInterval"].Value = time;
            // save the XmlDocument back to disk
            doc.Save(@"C:\Program Files\Logitech Gaming Software\Resources\G930\Manifest\Device_Manifest.xml");
            RestartSoftware();
        }

        private void RestartSoftware()
        {
            foreach (var process in Process.GetProcessesByName("lcore"))
            {
                process.Kill();
            }

            Process process2 = new Process();
            // Configure the process using the StartInfo properties.
            process2.StartInfo.FileName = "C:/Program Files/Logitech Gaming Software/LCore.exe";
            process2.StartInfo.Arguments = "";
            process2.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process2.Start();
            process2.WaitForExit(5000);
            process2.CloseMainWindow();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Win32.GetIdleTime() >= 1000 * 60 * 5 && !IsSet)
            {
                IsSet = true;
                SetInterval("300");

                LastIdleTime = Win32.GetIdleTime();
            }
            else if (IsSet && Win32.GetIdleTime() < LastIdleTime)
            {
                IsSet = false;
                SetInterval("0");
                LastIdleTime = 0;
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();

        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }
        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon.Visible = true;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon.Visible = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            RegistryKey rk = Registry.CurrentUser.OpenSubKey
("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if(checkBox1.Checked)
            {
                rk.SetValue("LTHelper", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }else
            {
                rk.DeleteValue("LTHelper");
            }

        }
    }
}
