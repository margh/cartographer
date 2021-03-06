﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO;

using EARTHLib;

namespace cartographer
{
    public partial class Cartographer : Form
    {
        public EARTHLib.ApplicationGE ge = null; //new ApplicationGEClass();
        public List<Electorate> m_Electorates = null;
        #region COM Hacks

        [DllImport("user32.dll")]
        private static extern int SetParent(
        int hWndChild,
        int hWndParent);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(
        int hWnd,
        int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(
        int hWnd,
        uint Msg,
        int wParam,
        int lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(
        int hWnd,
        int hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

        [DllImport("user32.dll")]
        private static extern int SendMessage(
        int hWnd,
        uint Msg,
        int wParam,
        int lParam);

        private const int HWND_TOP = 0x0;
        private const int WM_COMMAND = 0x0112;
        private const int WM_QT_PAINT = 0xC2DC;
        private const int WM_PAINT = 0x000F;
        private const int WM_SIZE = 0x0005;
        private const int SWP_FRAMECHANGED = 0x0020;



        #endregion

        public Cartographer()
        {
            m_Electorates = new List<Electorate>();
            ge = new ApplicationGEClass();
            ShowWindowAsync(ge.GetMainHwnd(), 0);
            InitializeComponent();
            SetParent(ge.GetRenderHwnd(), this.Handle.ToInt32());
            ResizeGoogleControl();
            ElectorateImporter g_elecImporter = new ElectorateImporter();
            g_elecImporter.ParseXLS();
            g_elecImporter.ParseMID("data/QLD_Federal_Electoral_Boundaries.mid");
            g_elecImporter.ParseMIF("data/QLD_Federal_Electoral_Boundaries.mif");
            m_Electorates = g_elecImporter.MergeData();

            Dms _test = new Dms(-27.579269);
            Console.Out.WriteLine(_test.m_minutes);
        }

        private void ResizeGoogleControl()
        {
            SendMessage(ge.GetMainHwnd(), WM_COMMAND, WM_PAINT, 0);
            PostMessage(ge.GetMainHwnd(), WM_QT_PAINT, 0, 0);

            SetWindowPos(
            ge.GetMainHwnd(),
            HWND_TOP,
            0,
            0,
            (int)this.Width,
            (int)this.Height,
            SWP_FRAMECHANGED);

            SendMessage(ge.GetRenderHwnd(), WM_COMMAND, WM_SIZE, 0);
        }

        private void Cartographer_Resize(object sender, EventArgs e)
        {
            ResizeGoogleControl();
        }

        private void loadKML_Click(object sender, EventArgs e)
        {

            String textFile = kmlData.Text;
            string keeper = "";
            if (File.Exists(textFile))
            {
                StreamReader sr = File.OpenText(textFile);
                string lewl;
                while ((lewl = sr.ReadLine()) != null)
                {
                    keeper += lewl;
                }
                ge.LoadKmlData(ref keeper);
            }
            else
            {
                MessageBox.Show("File doesn't exist");
            }

        }

        [DllImport("user32")]
        public static extern int GetWindowThreadProcessId(int hwnd, ref int lpdwProcessId);

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            int PID = 0;

            int handle = ge.GetMainHwnd();

            GetWindowThreadProcessId(handle, ref PID);

            Process process = null;
            try
            {
                process = Process.GetProcessById(PID);
            }
            catch
            {
                //process not found
            }
            if (process != null)
            {
                process.Kill();
            }

        }



    }
}
