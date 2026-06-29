using csDronLink;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// >>> WEBRTC
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;

// >>> MQTT
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Dron miDron = new Dron();

        // >>> WEBRTC: viewer dentro del formulario
        private WebView2 webViewRTC;

        // >>> Procesos Python
        private Process webrtcServerProcess;
        private Process webrtcPublisherProcess;
        private Process procesoGestos;
        private Process procesoObjetos;

        // >>> MQTT
        private IMqttClient mqttClient;
        private bool mqttConnected = false;

        // Estados
        private bool modoGestosActivo = false;
        private bool modoObjetosActivo = false;

        // Evita iniciar dos veces server/publisher si se pulsa varias veces
        private bool webRtcIniciandose = false;

        // Evita iniciar dos grabaciones de vídeo al mismo tiempo
        private bool grabandoVideo = false;

        // Ruta temporal usada por WebView2 cuando JavaScript descarga el WebM generado
        private string rutaDescargaWebmPendiente = null;

        // >>> VOZ
        private SpeechRecognitionEngine recognizer;
        private SpeechSynthesizer voz = new SpeechSynthesizer();
        private bool vozActiva = false;

        private bool hablando = false;
        private bool esperandoConfirmacion = false;
        private ResultadoConversacion ordenPendienteConfirmacion = null;

        // Filtro para evitar dobles detecciones muy seguidas
        private string ultimaFrase = "";
        private DateTime ultimaFraseTiempo = DateTime.MinValue;

        private const float CONFIANZA_MINIMA_COMANDO = 0.30f;
        private const float CONFIANZA_MINIMA_DICTADO = 0.55f;


        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            CrearWebViewRtc();

            this.Load += Form1_Load;

            voz.Rate = 0;
            voz.SetOutputToDefaultAudioDevice();
            voz.SpeakCompleted += Voz_SpeakCompleted;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            IniciarMQTT();

            try
            {
                await webViewRTC.EnsureCoreWebView2Async();
                webViewRTC.CoreWebView2.DownloadStarting += WebViewRTC_DownloadStarting;
                webViewRTC.Source = new Uri("about:blank");
            }
            catch (Exception ex)
            {
                AñadirLog("⚠️ Error inicializando WebView2: " + ex.Message);
            }

            ActualizarEstadoVoz("Voz: desactivada", Color.FromArgb(255, 213, 79));
            ActualizarBotonVoz(false);
            ActualizarEstadoModo("Modo: preparado");
        }

        // =========================================================
        //                 WEBVIEW2 / ESTADOS DE INTERFAZ
        // =========================================================

        private void CrearWebViewRtc()
        {
            webViewRTC = new WebView2();

            webViewRTC.Left = pictureBoxPC.Left;
            webViewRTC.Top = pictureBoxPC.Top;
            webViewRTC.Width = pictureBoxPC.Width;
            webViewRTC.Height = pictureBoxPC.Height;
            webViewRTC.Anchor = pictureBoxPC.Anchor;

            this.Controls.Add(webViewRTC);
            webViewRTC.BringToFront();
        }

        private void ActualizarEstadoSistema(string texto)
        {
            try
            {
                if (lblEstadoSistema == null) return;

                if (lblEstadoSistema.InvokeRequired)
                {
                    lblEstadoSistema.BeginInvoke(new Action(() => lblEstadoSistema.Text = texto));
                }
                else
                {
                    lblEstadoSistema.Text = texto;
                }
            }
            catch { }
        }

        private void ActualizarEstadoMqtt(string texto, Color color)
        {
            ActualizarLabelEstado(lblEstadoMqtt, texto, color);
        }

        private void ActualizarEstadoVoz(string texto, Color color)
        {
            ActualizarLabelEstado(lblEstadoVoz, texto, color);
        }

        private void ActualizarEstadoModo(string texto)
        {
            ActualizarLabelEstado(lblEstadoModo, texto, Color.FromArgb(180, 220, 255));
        }

        private void ActualizarLabelEstado(Label label, string texto, Color color)
        {
            try
            {
                if (label == null) return;

                if (label.InvokeRequired)
                {
                    label.BeginInvoke(new Action(() =>
                    {
                        label.Text = texto;
                        label.ForeColor = color;
                    }));
                }
                else
                {
                    label.Text = texto;
                    label.ForeColor = color;
                }
            }
            catch { }
        }

        private void ActualizarUltimoGesto(string gesto)
        {
            try
            {
                if (lblUltimoGesto == null) return;

                string texto = "Último gesto: " + gesto;

                if (lblUltimoGesto.InvokeRequired)
                    lblUltimoGesto.BeginInvoke(new Action(() => lblUltimoGesto.Text = texto));
                else
                    lblUltimoGesto.Text = texto;
            }
            catch { }
        }

        // =========================================================
        //                       VOZ
        // =========================================================

        private void InicializarReconocimientoVoz()
        {
            if (vozActiva)
                return;

            try
            {
                try
                {
                    recognizer = new SpeechRecognitionEngine(new CultureInfo("es-ES"));
                }
                catch
                {
                    recognizer = new SpeechRecognitionEngine();
                }

                Choices commands = new Choices();
                commands.Add(Conversacion.ObtenerFrasesReconocibles());

                GrammarBuilder gb = new GrammarBuilder();
                gb.Culture = recognizer.RecognizerInfo.Culture;
                gb.Append(commands);

                Grammar grammar = new Grammar(gb);
                grammar.Name = "ComandosControlados";
                recognizer.LoadGrammar(grammar);

                // Apoyo para frases no exactas. Si te da problemas, puedes comentar este bloque.
                try
                {
                    DictationGrammar dictado = new DictationGrammar();
                    dictado.Name = "DictadoLibre";
                    recognizer.LoadGrammar(dictado);
                }
                catch
                {
                    // No todos los equipos tienen dictado disponible para es-ES.
                }

                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                recognizer.SpeechRecognitionRejected += Recognizer_SpeechRecognitionRejected;

                recognizer.SetInputToDefaultAudioDevice();
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

                vozActiva = true;
                ActualizarBotonVoz(true);

                AñadirLog("[OK] Reconocimiento de voz iniciado.");
                ActualizarEstadoVoz("Voz: activa", Color.FromArgb(0, 230, 180));
                Hablar("Reconocimiento de voz activado.");
            }
            catch (Exception ex)
            {
                vozActiva = false;
                ActualizarBotonVoz(false);
                AñadirLog("[ERROR] Voz: " + ex.Message);
                ActualizarEstadoVoz("Voz: error", Color.FromArgb(255, 82, 82));
            }
        }

        private void DesactivarReconocimientoVoz()
        {
            try
            {
                vozActiva = false;
                LimpiarConfirmacionPendiente();

                if (recognizer != null)
                {
                    try { recognizer.RecognizeAsyncCancel(); } catch { }
                    try { recognizer.RecognizeAsyncStop(); } catch { }
                    try { recognizer.Dispose(); } catch { }
                    recognizer = null;
                }

                AñadirLog("[INFO] Reconocimiento de voz desactivado.");
                ActualizarEstadoVoz("Voz: desactivada", Color.FromArgb(255, 213, 79));
                ActualizarBotonVoz(false);

                if (voz != null)
                    voz.SpeakAsyncCancelAll();
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Desactivar voz: " + ex.Message);
            }
        }

        private void ActualizarBotonVoz(bool activa)
        {
            try
            {
                if (btnVoz == null) return;

                if (btnVoz.InvokeRequired)
                {
                    btnVoz.BeginInvoke(new Action(() => ActualizarBotonVoz(activa)));
                    return;
                }

                btnVoz.Text = activa ? "Desactivar voz" : "Activar voz";
                btnVoz.BackColor = activa
                    ? Color.FromArgb(198, 40, 40)
                    : Color.FromArgb(57, 73, 171);
            }
            catch { }
        }

        private void btnVoz_Click(object sender, EventArgs e)
        {
            if (vozActiva)
            {
                DesactivarReconocimientoVoz();
            }
            else
            {
                InicializarReconocimientoVoz();
            }
        }

        private void Hablar(string texto)
        {
            try
            {
                if (voz == null)
                    return;

                hablando = true;
                voz.SpeakAsyncCancelAll();
                voz.SpeakAsync(texto);
            }
            catch
            {
                hablando = false;
            }
        }

        private void Voz_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            hablando = false;
        }

        private void Recognizer_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            if (!vozActiva)
                return;

            try
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    ActualizarEstadoSistema("Voz: orden no reconocida");
                });
            }
            catch { }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (!vozActiva || hablando)
                return;

            string frase = e.Result.Text;
            float confianza = (float)e.Result.Confidence;
            string nombreGramatica = e.Result.Grammar != null ? e.Result.Grammar.Name : "";

            try
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    float confianzaMinima = nombreGramatica == "ComandosControlados"
                        ? CONFIANZA_MINIMA_COMANDO
                        : CONFIANZA_MINIMA_DICTADO;

                    AñadirLog("[VOZ] " + frase + " | confianza: " + confianza.ToString("F2") + " | " + nombreGramatica);

                    if (confianza < confianzaMinima)
                    {
                        AñadirLog("[VOZ] Ignorado por baja confianza.");
                        return;
                    }

                    if (frase == ultimaFrase &&
                        (DateTime.Now - ultimaFraseTiempo).TotalMilliseconds < 1200)
                    {
                        return;
                    }

                    ultimaFrase = frase;
                    ultimaFraseTiempo = DateTime.Now;

                    ProcesarOrdenVoz(frase);
                });
            }
            catch { }
        }

        private void ProcesarOrdenVoz(string frase)
        {
            if (esperandoConfirmacion)
            {
                if (Conversacion.EsRespuestaConfirmacion(frase))
                {
                    Hablar("Confirmado.");
                    EjecutarAccion(ordenPendienteConfirmacion);
                    LimpiarConfirmacionPendiente();
                    return;
                }

                if (Conversacion.EsRespuestaCancelacion(frase))
                {
                    Hablar("Cancelado.");
                    LimpiarConfirmacionPendiente();
                    return;
                }

                Hablar("Responde sí para confirmar o no para cancelar.");
                return;
            }

            ResultadoConversacion resultado = Conversacion.Interpretar(frase);

            if (resultado.Accion == "confirmar" || resultado.Accion == "cancelar")
            {
                Hablar("No hay ninguna orden pendiente.");
                return;
            }

            if (resultado.Accion == "ninguna")
            {
                Hablar(resultado.Mensaje);
                return;
            }

            if (!resultado.ComandoCompleto)
            {
                Hablar(resultado.Mensaje);
                return;
            }

            if (resultado.RequiereConfirmacion)
            {
                esperandoConfirmacion = true;
                ordenPendienteConfirmacion = resultado;
                Hablar(resultado.Mensaje);
                return;
            }

            Hablar(resultado.Mensaje);
            EjecutarAccion(resultado);
        }

        private void LimpiarConfirmacionPendiente()
        {
            esperandoConfirmacion = false;
            ordenPendienteConfirmacion = null;
        }

        private void EjecutarAccion(ResultadoConversacion resultado)
        {
            if (resultado == null)
            {
                Hablar("No hay ninguna acción pendiente.");
                return;
            }

            switch (resultado.Accion)
            {
                case "conectar":
                    button1.PerformClick();
                    break;

                case "despegar":
                    button2.PerformClick();
                    break;

                case "aterrizar":
                    button3.PerformClick();
                    break;

                case "girar":
                    EjecutarGiro(resultado.Sentido, resultado.Grados);
                    break;

                case "avanzar":
                    EjecutarAvance(resultado.Metros);
                    break;

                case "subir":
                    EjecutarMovimientoVertical("Up", "Subiendo.");
                    break;

                case "bajar":
                    EjecutarMovimientoVertical("Down", "Bajando.");
                    break;

                case "gestos":
                    btnGestos.PerformClick();
                    break;

                case "objetos":
                    btnObjetos.PerformClick();
                    break;

                case "detener":
                    btnDetener.PerformClick();
                    break;

                default:
                    Hablar("Acción no reconocida.");
                    break;
            }
        }

        private void EjecutarGiro(string sentido, int? grados)
        {
            if (grados == null || string.IsNullOrEmpty(sentido))
            {
                Hablar("No se puede girar porque faltan grados o sentido.");
                return;
            }

            try
            {
                int heading;

                if (sentido == "derecha")
                    heading = grados.Value % 360;
                else
                    heading = (360 - grados.Value) % 360;

                miDron.CambiarHeading(heading, bloquear: false);
                AñadirLog("[VOZ] Girando " + grados.Value + " grados a la " + sentido + ".");
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Giro voz: " + ex.Message);
                Hablar("No he podido girar. Revisa que el dron esté conectado.");
            }
        }

        private bool EjecutarAvance(int? metros)
        {
            if (metros == null)
            {
                Hablar("No se puede avanzar porque faltan los metros.");
                return false;
            }

            try
            {
                miDron.Mover("Forward", metros.Value, bloquear: false);
                AñadirLog("[VOZ] Avanzando " + metros.Value + " metros.");
                return true;
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Avance voz: " + ex.Message);
                Hablar("No he podido avanzar. Revisa que el dron esté conectado.");
                return false;
            }
        }

        private void EjecutarMovimientoVertical(string direccion, string mensaje)
        {
            try
            {
                miDron.Mover(direccion, 2, bloquear: false);
                AñadirLog("[VOZ] " + mensaje);
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Movimiento vertical voz: " + ex.Message);
                Hablar("No he podido ejecutar el movimiento.");
            }
        }

        // =========================================================
        //                       RUTAS
        // =========================================================

        private string GetAppPath()
        {
            // ...\WindowsFormsApp1\WindowsFormsApp1\bin\Debug
            return Application.StartupPath;
        }

        private string GetProjectRoot()
        {
            // ...\TFG-App\WindowsFormsApp1
            return Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\.."));
        }

        private string GetRepoRoot()
        {
            // ...\TFG-App
            return Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\.."));
        }

        private string GetPythonGestos()
        {
            return Path.Combine(GetRepoRoot(), "gestos_env310", "Scripts", "python.exe");
        }

        private string GetPythonWebRtcObjetos()
        {
            return Path.Combine(GetRepoRoot(), "mp_env", "Scripts", "python.exe");
        }

        private string GetFeatureWebRtcPath()
        {
            return Path.Combine(GetProjectRoot(), "feature-webrtc");
        }

        private string GetServerPath()
        {
            return Path.Combine(GetFeatureWebRtcPath(), "server.py");
        }

        private string GetPublisherPath()
        {
            return Path.Combine(GetFeatureWebRtcPath(), "script_publisher.py");
        }

        private string GetGestosScriptPath()
        {
            return Path.Combine(GetAppPath(), "detectar_mano_mp.py");
        }

        private string GetObjetosScriptPath()
        {
            return Path.Combine(GetAppPath(), "detectarObjetos.py");
        }

        // =========================================================
        //                       LOG
        // =========================================================

        private void AñadirLog(string texto)
        {
            try
            {
                if (listBox1.InvokeRequired)
                {
                    listBox1.Invoke(new Action(() =>
                    {
                        listBox1.Items.Add(texto);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }));
                }
                else
                {
                    listBox1.Items.Add(texto);
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                }
            }
            catch
            {
                // Evita que un fallo de log rompa la aplicación
            }
        }

        private async Task<bool> EsperarVideoDisponible(int timeoutMs)
        {
            using (HttpClient client = new HttpClient())
            {
                DateTime inicio = DateTime.Now;

                while ((DateTime.Now - inicio).TotalMilliseconds < timeoutMs)
                {
                    try
                    {
                        string respuesta = await client.GetStringAsync("http://127.0.0.1:8080/status");

                        if (respuesta.Contains("\"video\": true") || respuesta.Contains("\"video\":true"))
                            return true;
                    }
                    catch
                    {
                        // El servidor encara no està llest
                    }

                    await Task.Delay(700);
                }
            }

            return false;
        }

        private bool OcultarLogPython(string tag, string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
                return true;

            // Logs molt repetitius del servidor
            if (tag == "SERVER")
            {
                if (linea.Contains("aiohttp.access"))
                    return true;

                if (linea.Contains("aioice.ice"))
                    return true;

                if (linea.Contains("Check CandidatePair"))
                    return true;

                if (linea.Contains("State.FROZEN"))
                    return true;

                if (linea.Contains("State.WAITING"))
                    return true;

                if (linea.Contains("State.IN_PROGRESS"))
                    return true;

                if (linea.Contains("State.SUCCEEDED"))
                    return true;

                if (linea.Contains("State.FAILED"))
                    return true;

                if (linea.Contains("ICE completed"))
                    return true;

                if (linea.StartsWith("ROUTE:"))
                    return true;

                if (linea.Contains("Cannot write to closing transport"))
                    return true;
            }

            // Logs molestos de MediaPipe/TensorFlow
            if (tag == "GESTOS")
            {
                if (linea.Contains("DeprecationWarning"))
                    return true;

                if (linea.Contains("TensorFlow Lite XNNPACK"))
                    return true;

                if (linea.Contains("WARNING: All log messages"))
                    return true;

                if (linea.Contains("inference_feedback_manager"))
                    return true;

                if (linea.Contains("landmark_projection_calculator"))
                    return true;

                if (linea.StartsWith("W0000"))
                    return true;
            }

            return false;
        }

        // =========================================================
        //                       TELEMETRÍA
        // =========================================================

        private void ProcesarTelemetria(byte id, List<(string nombre, float valor)> telemetria)
        {
            foreach (var t in telemetria)
            {
                if (t.nombre == "Alt")
                {
                    altLbl.Text = t.valor.ToString("0.00");
                    break;
                }
            }
        }

        // =========================================================
        //                       BOTÓN CONECTAR
        // =========================================================

        private async void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                AñadirLog("[INFO] Intentando conectar al dron en modo simulacion...");
                ActualizarEstadoSistema("Sistema: conectando dron");
                miDron.Conectar("simulacion");
                AñadirLog("[OK] Conexión solicitada al dron.");

                miDron.EnviarDatosTelemetria(ProcesarTelemetria);
                AñadirLog("[OK] Telemetría solicitada.");

                // AQUÍ arrancamos server.py y script_publisher.py
                AñadirLog("[INFO] Iniciando servidor WebRTC y publisher de vídeo...");
                bool webRtcOk = await IniciarServerYPublisher();

                if (webRtcOk)
                {
                    AñadirLog("[OK] Server y publisher preparados.");
                    ActualizarEstadoSistema("Sistema: dron conectado + vídeo activo");

                    if (webViewRTC.CoreWebView2 == null)
                        await webViewRTC.EnsureCoreWebView2Async();

                    webViewRTC.Source = new Uri("http://localhost:8080/");
                }
                else
                {
                    AñadirLog("[ERROR] No se pudo preparar server/publisher.");
                    ActualizarEstadoSistema("Sistema: error en vídeo");
                }
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Conectar: " + ex.Message);
                Hablar("No he podido conectar con el dron.");
                ActualizarEstadoSistema("Sistema: error al conectar");
            }
        }

        // =========================================================
        //                       BOTONES DRON
        // =========================================================

        private void EnAire(byte id, object param)
        {
            try
            {
                if (button2.InvokeRequired)
                {
                    button2.BeginInvoke(new Action(() => EnAire(id, param)));
                    return;
                }

                button2.BackColor = Color.Green;
                button2.ForeColor = Color.White;
                button2.Text = (string)param;
                ActualizarEstadoSistema("Sistema: dron en aire");
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                miDron.Despegar(20, bloquear: false, f: EnAire, param: "Volando");
                button2.BackColor = Color.Yellow;
                button2.ForeColor = Color.Black;
                AñadirLog("[INFO] Despegando.");
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Despegar: " + ex.Message);
                Hablar("No he podido despegar. Revisa que el dron esté conectado.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                miDron.Aterrizar(bloquear: false);
                AñadirLog("[INFO] Aterrizando.");
                ActualizarEstadoSistema("Sistema: aterrizando");
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Aterrizar: " + ex.Message);
                Hablar("No he podido aterrizar.");
            }
        }

        // =========================================================
        //                       MQTT
        // =========================================================

        private async void IniciarMQTT()
        {
            try
            {
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("127.0.0.1", 1883)
                    .Build();

                mqttClient.UseConnectedHandler(async e =>
                {
                    mqttConnected = true;
                    AñadirLog("MQTT conectado al broker.");
                    ActualizarEstadoMqtt("MQTT: conectado", Color.FromArgb(0, 230, 180));

                    await mqttClient.SubscribeAsync("gestos");
                    AñadirLog("Subscrita al tema 'gestos'.");

                    await mqttClient.SubscribeAsync("objetos");
                    AñadirLog("Subscrita al tema 'objetos'.");
                });

                mqttClient.UseDisconnectedHandler(e =>
                {
                    mqttConnected = false;
                    AñadirLog("MQTT desconectado.");
                    ActualizarEstadoMqtt("MQTT: desconectado", Color.FromArgb(255, 82, 82));
                });

                mqttClient.UseApplicationMessageReceivedHandler(e =>
                {
                    string topic = e.ApplicationMessage.Topic;
                    string mensaje = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    if (topic == "gestos")
                    {
                        AñadirLog("Gesto recibido por MQTT: " + mensaje);
                        EjecutarAccionPorGesto(mensaje);
                    }
                    else if (topic == "objetos")
                    {
                        AñadirLog("Objeto detectado por MQTT: " + mensaje);
                    }
                });

                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                AñadirLog("❌ Error connectando MQTT: " + ex.Message);
                ActualizarEstadoMqtt("MQTT: error", Color.FromArgb(255, 82, 82));
            }
        }

        // =========================================================
        //              START PROCESS PYTHON
        // =========================================================

        private Process StartProcess(string pythonExe, string scriptPath, string tag, string workingDir)
        {
            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = "-u \"" + scriptPath + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDir
            };

            psi.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";

            var p = new Process();
            p.StartInfo = psi;
            p.EnableRaisingEvents = true;

            p.OutputDataReceived += (s, e) =>
            {
                if (!OcultarLogPython(tag, e.Data))
                    AñadirLog("[" + tag + "] " + e.Data);
            };

            p.ErrorDataReceived += (s, e) =>
            {
                if (!OcultarLogPython(tag, e.Data))
                    AñadirLog("⚠️ [" + tag + "] " + e.Data);
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            return p;
        }

        private void DetenerProceso(ref Process proceso, string nombre)
        {
            try
            {
                if (proceso != null && !proceso.HasExited)
                {
                    proceso.Kill();
                    proceso.WaitForExit(1000);
                    proceso.Dispose();
                    proceso = null;
                    AñadirLog("[INFO] " + nombre + " detenido.");
                }
            }
            catch (Exception ex)
            {
                AñadirLog("[WARN] No se pudo detener " + nombre + ": " + ex.Message);
            }
        }

        // =========================================================
        //              COMPROBAR PUERTO Y URL
        // =========================================================

        private bool PuertoAbierto(string host, int puerto)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(host, puerto, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));

                    if (!success)
                        return false;

                    client.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> EsperarUrlOk(string url, int timeoutMs)
        {
            using (HttpClient client = new HttpClient())
            {
                DateTime inicio = DateTime.Now;

                while ((DateTime.Now - inicio).TotalMilliseconds < timeoutMs)
                {
                    try
                    {
                        using (HttpResponseMessage response = await client.GetAsync(
                            url,
                            HttpCompletionOption.ResponseHeadersRead
                        ))
                        {
                            if (response.IsSuccessStatusCode)
                                return true;
                        }
                    }
                    catch
                    {
                        // Todavía no está disponible
                    }

                    await Task.Delay(500);
                }
            }

            return false;
        }

        // =========================================================
        //          INICIAR SERVER + PUBLISHER DESDE CONECTAR
        // =========================================================

        private async Task<bool> IniciarServerYPublisher()
        {
            if (webRtcIniciandose)
            {
                AñadirLog("[INFO] WebRTC ya se está iniciando. Espera...");
                await Task.Delay(3000);
                return PuertoAbierto("127.0.0.1", 8080);
            }

            webRtcIniciandose = true;

            try
            {
                string pythonWebRtc = GetPythonWebRtcObjetos();
                string featurePath = GetFeatureWebRtcPath();
                string serverPath = GetServerPath();
                string publisherPath = GetPublisherPath();

                AñadirLog("[INFO] Repo root: " + GetRepoRoot());
                AñadirLog("[INFO] feature-webrtc: " + featurePath);

                if (!File.Exists(pythonWebRtc))
                {
                    AñadirLog("[ERROR] No existe python mp_env: " + pythonWebRtc);
                    return false;
                }

                if (!File.Exists(serverPath))
                {
                    AñadirLog("[ERROR] No existe server.py: " + serverPath);
                    return false;
                }

                if (!File.Exists(publisherPath))
                {
                    AñadirLog("[ERROR] No existe script_publisher.py: " + publisherPath);
                    return false;
                }

                // 1. SERVER.PY
                if (PuertoAbierto("127.0.0.1", 8080))
                {
                    AñadirLog("[OK] El puerto 8080 ya está activo. No se inicia otro server.py.");
                }
                else
                {
                    AñadirLog("[INFO] Iniciando server.py...");

                    webrtcServerProcess = StartProcess(
                        pythonWebRtc,
                        serverPath,
                        "SERVER",
                        featurePath
                    );

                    bool serverOk = false;

                    for (int i = 0; i < 20; i++)
                    {
                        if (PuertoAbierto("127.0.0.1", 8080))
                        {
                            serverOk = true;
                            break;
                        }

                        await Task.Delay(500);
                    }

                    if (!serverOk)
                    {
                        AñadirLog("[ERROR] server.py no ha abierto el puerto 8080.");
                        return false;
                    }

                    AñadirLog("[OK] server.py iniciado en puerto 8080.");
                }

                // 2. PUBLISHER.PY
                bool streamYaActivo = await EsperarVideoDisponible(1500);

                if (streamYaActivo)
                {
                    AñadirLog("[OK] Stream MJPEG ya disponible.");
                    return true;
                }

                if (webrtcPublisherProcess == null || webrtcPublisherProcess.HasExited)
                {
                    AñadirLog("[INFO] Iniciando script_publisher.py...");

                    webrtcPublisherProcess = StartProcess(
                        pythonWebRtc,
                        publisherPath,
                        "PUBLISHER",
                        featurePath
                    );
                }
                else
                {
                    AñadirLog("[OK] script_publisher.py ya estaba iniciado.");
                }

                bool streamOk = await EsperarVideoDisponible(30000);

                if (!streamOk)
                {
                    AñadirLog("[ERROR] El publisher no ha publicado vídeo dentro del tiempo esperado.");
                    AñadirLog("[INFO] Revisa si la cámara está ocupada o si script_publisher.py falla.");
                    return false;
                }

                AñadirLog("[OK] Stream WebRTC/MJPEG disponible.");
                return true;
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] IniciarServerYPublisher: " + ex.Message);
                return false;
            }
            finally
            {
                webRtcIniciandose = false;
            }
        }

        // =========================================================
        //                       BOTÓN GESTOS
        // =========================================================

        private async void btnGestos_Click(object sender, EventArgs e)
        {
            try
            {
                DetenerProceso(ref procesoObjetos, "Script de objetos");
                DetenerProceso(ref procesoGestos, "Script de gestos");

                modoGestosActivo = true;
                modoObjetosActivo = false;

                AñadirLog("[INFO] Cargando reconocimiento de gestos...");
                ActualizarEstadoModo("Modo: gestos");

                // Ya NO iniciamos server.py ni publisher aquí.
                // Solo comprobamos que el stream exista.
                bool streamOk = await EsperarUrlOk("http://127.0.0.1:8080/stream.mjpg", 3000);

                if (!streamOk)
                {
                    AñadirLog("[ERROR] No hay vídeo en http://127.0.0.1:8080/stream.mjpg");
                    AñadirLog("[INFO] Pulsa primero el botón Conectar para iniciar server.py y script_publisher.py.");
                    return;
                }

                string pythonGestos = GetPythonGestos();
                string gestosPath = GetGestosScriptPath();
                string appPath = GetAppPath();

                if (!File.Exists(pythonGestos))
                {
                    AñadirLog("[ERROR] No existe python gestos: " + pythonGestos);
                    return;
                }

                if (!File.Exists(gestosPath))
                {
                    AñadirLog("[ERROR] No existe detectar_mano_mp.py: " + gestosPath);
                    return;
                }

                AñadirLog("[INFO] Iniciando detectar_mano_mp.py...");

                procesoGestos = StartProcess(
                    pythonGestos,
                    gestosPath,
                    "GESTOS",
                    appPath
                );

                bool gestosWebOk = await EsperarUrlOk("http://127.0.0.1:8090/", 12000);

                if (!gestosWebOk)
                {
                    AñadirLog("[ERROR] El servidor de gestos no responde en http://127.0.0.1:8090/");
                    AñadirLog("[INFO] Revisa el log de [GESTOS].");
                    return;
                }

                if (webViewRTC.CoreWebView2 == null)
                    await webViewRTC.EnsureCoreWebView2Async();

                webViewRTC.Source = new Uri("http://127.0.0.1:8090/");
                AñadirLog("[INFO] Mostrando vídeo de gestos en el formulario.");
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] btnGestos_Click: " + ex.Message);
            }
        }

        // =========================================================
        //                       ACCIONES POR GESTO
        // =========================================================

        private void EjecutarAccionPorGesto(string gesto)
        {
            try
            {
                ActualizarUltimoGesto(gesto);

                switch (gesto.ToLower())
                {
                    case "palm":
                        miDron.Despegar(20, bloquear: false, f: EnAire, param: "Volando");
                        break;

                    case "puño":
                    case "punyo":
                    case "puno":
                        miDron.Aterrizar(bloquear: false);
                        break;

                    case "uno":
                        miDron.Mover("Forward", 10, bloquear: false);
                        break;

                    case "dos":
                        miDron.CambiarHeading(90, bloquear: false);
                        break;

                    case "tres":
                        miDron.CambiarHeading(270, bloquear: false);
                        break;

                    default:
                        AñadirLog("Gesto no reconocido: " + gesto);
                        break;
                }
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Acción por gesto '" + gesto + "': " + ex.Message);
            }
        }

        // =========================================================
        //                       BOTÓN OBJETOS
        // =========================================================

        private async void btnObjetos_Click(object sender, EventArgs e)
        {
            try
            {
                DetenerProceso(ref procesoGestos, "Script de gestos");
                DetenerProceso(ref procesoObjetos, "Script de objetos");

                modoGestosActivo = false;
                modoObjetosActivo = true;

                AñadirLog("[INFO] Cargando reconocimiento de objetos...");
                ActualizarEstadoModo("Modo: objetos");

                // Ya NO iniciamos server.py ni publisher aquí.
                // Solo comprobamos que el stream exista.
                bool streamOk = await EsperarUrlOk("http://127.0.0.1:8080/stream.mjpg", 3000);

                if (!streamOk)
                {
                    AñadirLog("[ERROR] No hay vídeo en http://127.0.0.1:8080/stream.mjpg");
                    AñadirLog("[INFO] Pulsa primero el botón Conectar para iniciar server.py y script_publisher.py.");
                    return;
                }

                string pythonObjetos = GetPythonWebRtcObjetos();
                string objetosPath = GetObjetosScriptPath();
                string appPath = GetAppPath();

                if (!File.Exists(pythonObjetos))
                {
                    AñadirLog("[ERROR] No existe python objetos/mp_env: " + pythonObjetos);
                    return;
                }

                if (!File.Exists(objetosPath))
                {
                    AñadirLog("[ERROR] No existe detectarObjetos.py: " + objetosPath);
                    return;
                }

                AñadirLog("[INFO] Iniciando detectarObjetos.py...");

                procesoObjetos = StartProcess(
                    pythonObjetos,
                    objetosPath,
                    "OBJETOS",
                    appPath
                );

                if (webViewRTC.CoreWebView2 == null)
                    await webViewRTC.EnsureCoreWebView2Async();

                webViewRTC.Source = new Uri("http://localhost:8080/");
                AñadirLog("[INFO] Mostrando vídeo base en el formulario.");
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] btnObjetos_Click: " + ex.Message);
            }
        }

        // =========================================================
        //                       BOTÓN DETENER
        // =========================================================

        private void btnDetener_Click(object sender, EventArgs e)
        {
            try
            {
                DetenerProceso(ref procesoGestos, "Script de gestos");
                DetenerProceso(ref procesoObjetos, "Script de objetos");
                DetenerProceso(ref webrtcPublisherProcess, "script_publisher.py");
                DetenerProceso(ref webrtcServerProcess, "server.py");

                modoGestosActivo = false;
                modoObjetosActivo = false;

                if (webViewRTC != null)
                    webViewRTC.Source = new Uri("about:blank");

                ActualizarEstadoModo("Modo: detenido");
                ActualizarEstadoSistema("Sistema: scripts detenidos");

                AñadirLog("[INFO] Todos los scripts detenidos.");
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] btnDetener_Click: " + ex.Message);
            }
        }


        // =========================================================
        //              CAPTURA JPG Y GRABACIÓN WEBM
        // =========================================================

        private string GetCarpetaMultimedia()
        {
            // Guardem captures JPG i vídeos WEBM directament a Descargas.
            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        private async void btnCaptura_Click(object sender, EventArgs e)
        {
            try
            {
                if (webViewRTC == null)
                {
                    AñadirLog("[ERROR] No existe el visor WebView2.");
                    return;
                }

                if (webViewRTC.CoreWebView2 == null)
                    await webViewRTC.EnsureCoreWebView2Async();

                string carpeta = GetCarpetaMultimedia();
                string rutaCaptura = Path.Combine(
                    carpeta,
                    "captura_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg"
                );

                using (FileStream fs = new FileStream(rutaCaptura, FileMode.Create, FileAccess.Write))
                {
                    await webViewRTC.CoreWebView2.CapturePreviewAsync(
                        CoreWebView2CapturePreviewImageFormat.Jpeg,
                        fs
                    );
                }

                AñadirLog("[OK] Captura JPG guardada en Descargas: " + rutaCaptura);
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] Captura JPG: " + ex.Message);
            }
        }

        private async void btnGrabarVideo_Click(object sender, EventArgs e)
        {
            if (grabandoVideo)
            {
                AñadirLog("[INFO] Ya hay una grabación en curso.");
                return;
            }

            try
            {
                int segundos = (int)numSegundosVideo.Value;

                if (webViewRTC == null)
                {
                    AñadirLog("[ERROR] No existe el visor WebView2.");
                    return;
                }

                if (webViewRTC.CoreWebView2 == null)
                    await webViewRTC.EnsureCoreWebView2Async();

                string carpeta = GetCarpetaMultimedia();
                string nombreArchivo = "video_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".webm";
                string rutaVideo = Path.Combine(carpeta, nombreArchivo);

                rutaDescargaWebmPendiente = rutaVideo;
                grabandoVideo = true;
                btnGrabarVideo.Enabled = false;
                btnGrabarVideo.Text = "Grabando...";

                AñadirLog("[INFO] Iniciando grabación WEBM de " + segundos + " segundos...");

                string resultado = await GrabarWebView2WebmAsync(segundos, nombreArchivo);

                if (resultado == "OK")
                {
                    AñadirLog("[OK] Vídeo WEBM guardado en Descargas: " + rutaVideo);
                }
                else
                {
                    rutaDescargaWebmPendiente = null;
                    AñadirLog("[ERROR] Grabación WEBM: " + resultado);
                }
            }
            catch (Exception ex)
            {
                rutaDescargaWebmPendiente = null;
                AñadirLog("[ERROR] Grabación WEBM: " + ex.Message);
            }
            finally
            {
                grabandoVideo = false;
                btnGrabarVideo.Enabled = true;
                btnGrabarVideo.Text = "Grabar";
            }
        }

        private async Task<string> GrabarWebView2WebmAsync(int segundos, string nombreArchivo)
        {
            string nombreSeguro = nombreArchivo.Replace("\\", "\\\\").Replace("'", "\\'");

            string script = @"
                (async function() {
                    try {
                        const video = document.querySelector('video');
                        const img = document.querySelector('img');

                        let stream = null;
                        let intervaloCanvas = null;

                        if (video && video.readyState >= 2) {
                            stream = video.captureStream
                                ? video.captureStream()
                                : (video.mozCaptureStream ? video.mozCaptureStream() : null);
                        }
                        else if (img) {
                            const ancho = img.naturalWidth || img.clientWidth || 640;
                            const alto = img.naturalHeight || img.clientHeight || 480;

                            const canvas = document.createElement('canvas');
                            canvas.width = ancho;
                            canvas.height = alto;

                            const ctx = canvas.getContext('2d');

                            intervaloCanvas = setInterval(function() {
                                try {
                                    ctx.drawImage(img, 0, 0, ancho, alto);
                                } catch (e) { }
                            }, 50);

                            stream = canvas.captureStream(20);
                        }
                        else {
                            return 'No se ha encontrado ningún elemento <video> ni <img> para grabar.';
                        }

                        if (!stream) {
                            return 'El navegador no permite capturar el stream actual.';
                        }

                        const mimeType = MediaRecorder.isTypeSupported('video/webm;codecs=vp8')
                            ? 'video/webm;codecs=vp8'
                            : 'video/webm';

                        const chunks = [];
                        const recorder = new MediaRecorder(stream, { mimeType: mimeType });

                        recorder.ondataavailable = function(e) {
                            if (e.data && e.data.size > 0) {
                                chunks.push(e.data);
                            }
                        };

                        const stopped = new Promise(resolve => {
                            recorder.onstop = resolve;
                        });

                        recorder.start(250);
                        await new Promise(resolve => setTimeout(resolve, " + segundos.ToString(System.Globalization.CultureInfo.InvariantCulture) + @" * 1000));
                        recorder.stop();
                        await stopped;

                        if (intervaloCanvas) {
                            clearInterval(intervaloCanvas);
                        }

                        const blob = new Blob(chunks, { type: 'video/webm' });
                        const url = URL.createObjectURL(blob);
                        const a = document.createElement('a');
                        a.href = url;
                        a.download = '" + nombreSeguro + @"';
                        document.body.appendChild(a);
                        a.click();

                        setTimeout(function() {
                            URL.revokeObjectURL(url);
                            a.remove();
                        }, 1000);

                        return 'OK';
                    } catch (err) {
                        return err.message;
                    }
                })();";

            string resultadoJson = await webViewRTC.CoreWebView2.ExecuteScriptAsync(script);
            return DecodificarResultadoJavascript(resultadoJson);
        }

        private string DecodificarResultadoJavascript(string resultadoJson)
        {
            if (string.IsNullOrWhiteSpace(resultadoJson))
                return "Sin respuesta del navegador.";

            string resultado = resultadoJson.Trim();

            if (resultado.StartsWith("\"") && resultado.EndsWith("\""))
                resultado = resultado.Substring(1, resultado.Length - 2);

            return System.Text.RegularExpressions.Regex.Unescape(resultado);
        }

        private void WebViewRTC_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(rutaDescargaWebmPendiente))
                {
                    e.ResultFilePath = rutaDescargaWebmPendiente;
                    AñadirLog("[INFO] Descarga WEBM dirigida a: " + rutaDescargaWebmPendiente);
                    rutaDescargaWebmPendiente = null;
                }
            }
            catch (Exception ex)
            {
                AñadirLog("[ERROR] DownloadStarting WEBM: " + ex.Message);
            }
        }

        

        // =========================================================
        //                       FORM CLOSING
        // =========================================================

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            vozActiva = false;

            try
            {
                if (recognizer != null)
                {
                    recognizer.RecognizeAsyncCancel();
                    recognizer.RecognizeAsyncStop();
                    recognizer.Dispose();
                    recognizer = null;
                }

                if (voz != null)
                {
                    voz.SpeakAsyncCancelAll();
                    voz.Dispose();
                    voz = null;
                }
            }
            catch { }

            try
            {
                if (mqttClient != null && mqttConnected)
                    mqttClient.DisconnectAsync().Wait();
            }
            catch { }

            DetenerProceso(ref procesoGestos, "Script de gestos");
            DetenerProceso(ref procesoObjetos, "Script de objetos");
            DetenerProceso(ref webrtcPublisherProcess, "script_publisher.py");
            DetenerProceso(ref webrtcServerProcess, "server.py");

            base.OnFormClosing(e);
        }

        private void lblAltitudTitulo_Click(object sender, EventArgs e)
        {

        }

        private void lblLeyendaGestos_Click(object sender, EventArgs e)
        {

        }

        private void lblTituloApp_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
