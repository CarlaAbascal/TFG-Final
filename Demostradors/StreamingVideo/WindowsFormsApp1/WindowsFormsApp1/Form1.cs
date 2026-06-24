using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using csDronLink;

//Streaming video 
using OpenCvSharp;
using OpenCvSharp.Extensions;

// >>> WEBRTC
using Microsoft.Web.WebView2.WinForms;
using System.IO;
using System.Diagnostics;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Dron miDron = new Dron();

        // Variables para streaming
        private VideoCapture capPC;
        private VideoCapture capDron;
        private bool running = false;

        // >>> WEBRTC
        private WebView2 webViewRTC;


        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false; // Para evitar problemas de hilos con los labels

            // >>> WEBRTC: crear WebView2 en el mismo sitio que el pictureBoxPC
            webViewRTC = new WebView2();
            webViewRTC.Left = pictureBoxPC.Left;
            webViewRTC.Top = pictureBoxPC.Top;
            webViewRTC.Width = pictureBoxPC.Width;
            webViewRTC.Height = pictureBoxPC.Height;
            webViewRTC.Anchor = pictureBoxPC.Anchor;

            this.Controls.Add(webViewRTC);
            webViewRTC.BringToFront(); // lo pone por delante del pictureBox
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Recorrer la lista con los datos de telemetría y mostrar la altitud en un label
        private void ProcesarTelemetria(byte id, List<(string nombre, float valor)> telemetria)
        {
            foreach (var t in telemetria)
            {
                if (t.nombre == "Alt")
                {
                    altLbl.Text = t.valor.ToString();
                    break;
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            miDron.Conectar("simulacion");
            miDron.EnviarDatosTelemetria(ProcesarTelemetria);
        }

        private void EnAire(byte id, object param)
        {
            button2.BackColor = Color.Green;
            button2.ForeColor = Color.White;
            button2.Text = (string)param;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            miDron.Despegar(20, bloquear: false, f: EnAire, param: "Volando");
            button2.BackColor = Color.Yellow;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            miDron.Aterrizar();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBoxDron_Click(object sender, EventArgs e)
        {

        }

        //Boton para iniciar streaming
        private async void button4_Click(object sender, EventArgs e)
        {
            await webViewRTC.EnsureCoreWebView2Async();
            //Para que se abra el DevTools y ver los errores
            //webViewRTC.CoreWebView2.OpenDevToolsWindow();

            webViewRTC.Source = new Uri("http://127.0.0.1:8080/");
        }

        //Parar el streaming al cerrar el formulario
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            running = false;
           // capPC?.Release();
           // capDron?.Release();

            // >>> WEBRTC
            webViewRTC?.Dispose();

            base.OnFormClosing(e);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
