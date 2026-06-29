using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public class ResultadoConversacion
    {
        public string Accion { get; set; } = "ninguna";
        public string Mensaje { get; set; } = "No he entendido la orden.";
        public bool RequiereConfirmacion { get; set; } = false;
        public bool ComandoCompleto { get; set; } = true;
        public string Sentido { get; set; } = "";
        public int? Grados { get; set; } = null;
        public int? Metros { get; set; } = null;
    }

    public static class Conversacion
    {
        private static readonly Dictionary<string, int> Numeros = new Dictionary<string, int>
        {
            { "cero", 0 },
            { "uno", 1 }, { "un", 1 }, { "una", 1 },
            { "dos", 2 }, { "tres", 3 }, { "cuatro", 4 }, { "cinco", 5 },
            { "seis", 6 }, { "siete", 7 }, { "ocho", 8 }, { "nueve", 9 },
            { "diez", 10 }, { "once", 11 }, { "doce", 12 }, { "trece", 13 },
            { "catorce", 14 }, { "quince", 15 }, { "dieciseis", 16 },
            { "diecisiete", 17 }, { "dieciocho", 18 }, { "diecinueve", 19 },
            { "veinte", 20 }, { "veintiuno", 21 }, { "veintidos", 22 },
            { "veintitres", 23 }, { "veinticuatro", 24 }, { "veinticinco", 25 },
            { "veintiseis", 26 }, { "veintisiete", 27 }, { "veintiocho", 28 },
            { "veintinueve", 29 },
            { "treinta", 30 }, { "cuarenta", 40 }, { "cincuenta", 50 },
            { "sesenta", 60 }, { "setenta", 70 }, { "ochenta", 80 }, { "noventa", 90 },
            { "cien", 100 }, { "ciento", 100 },
            { "doscientos", 200 }, { "trescientos", 300 }, { "cuatrocientos", 400 }
        };

        public static string[] ObtenerFrasesReconocibles()
        {
            return new string[]
            {
                "conectar", "conectate", "conéctate", "conecta", "conectarse",
                "despegar", "despega", "quiero despegar", "creo que ya podemos despegar",
                "aterrizar", "aterriza", "quiero aterrizar",
                "si", "sí", "vale", "ok", "adelante", "confirmar",
                "no", "cancelar", "cancela",
                "girar", "gira", "gira a la derecha", "gira a la izquierda",
                "girar a la derecha", "girar a la izquierda",
                "derecha", "izquierda",
                "avanzar", "avanza", "adelante", "avanza diez metros", "avanza cinco metros",
                "subir", "sube", "bajar", "baja",
                "gestos", "activar gestos", "reconocimiento de gestos",
                "objetos", "activar objetos", "reconocimiento de objetos",
                "detener", "parar", "deten todo", "detener scripts"
            };
        }

        public static bool EsRespuestaConfirmacion(string frase)
        {
            string f = Normalizar(frase);
            return f == "si" || f == "sí" || f == "vale" || f == "ok" ||
                   f == "adelante" || f == "confirmar" || f == "confirmo" ||
                   f.Contains("confirmar") || f.Contains("adelante");
        }

        public static bool EsRespuestaCancelacion(string frase)
        {
            string f = Normalizar(frase);
            return f == "no" || f == "cancelar" || f == "cancela" ||
                   f.Contains("cancelar") || f.Contains("cancela");
        }

        public static ResultadoConversacion Interpretar(string fraseOriginal)
        {
            string frase = Normalizar(fraseOriginal);

            if (string.IsNullOrWhiteSpace(frase))
                return Ninguna("No he entendido la orden.");

            if (EsRespuestaConfirmacion(frase))
                return new ResultadoConversacion { Accion = "confirmar", Mensaje = "Confirmado." };

            if (EsRespuestaCancelacion(frase))
                return new ResultadoConversacion { Accion = "cancelar", Mensaje = "Cancelado." };

            if (Contiene(frase, "conectar", "conectate", "conecta", "conectarse", "conexion", "conexión"))
            {
                return new ResultadoConversacion
                {
                    Accion = "conectar",
                    Mensaje = "¿Quieres conectar el dron?",
                    RequiereConfirmacion = true
                };
            }

            if (Contiene(frase, "despegar", "despega"))
            {
                return new ResultadoConversacion
                {
                    Accion = "despegar",
                    Mensaje = "¿Seguro que quieres despegar?",
                    RequiereConfirmacion = true
                };
            }

            if (Contiene(frase, "aterrizar", "aterriza"))
            {
                return new ResultadoConversacion
                {
                    Accion = "aterrizar",
                    Mensaje = "¿Seguro que quieres aterrizar?",
                    RequiereConfirmacion = true
                };
            }

            if (Contiene(frase, "gestos", "gesto"))
            {
                return new ResultadoConversacion
                {
                    Accion = "gestos",
                    Mensaje = "Activando reconocimiento de gestos."
                };
            }

            if (Contiene(frase, "objetos", "objeto"))
            {
                return new ResultadoConversacion
                {
                    Accion = "objetos",
                    Mensaje = "Activando reconocimiento de objetos."
                };
            }

            if (Contiene(frase, "detener", "parar", "para", "deten"))
            {
                return new ResultadoConversacion
                {
                    Accion = "detener",
                    Mensaje = "Deteniendo los procesos."
                };
            }

            if (Contiene(frase, "subir", "sube"))
            {
                return new ResultadoConversacion
                {
                    Accion = "subir",
                    Mensaje = "Subiendo."
                };
            }

            if (Contiene(frase, "bajar", "baja"))
            {
                return new ResultadoConversacion
                {
                    Accion = "bajar",
                    Mensaje = "Bajando."
                };
            }

            if (Contiene(frase, "girar", "gira") || frase == "derecha" || frase == "izquierda")
            {
                return InterpretarGiro(frase);
            }

            if (Contiene(frase, "avanzar", "avanza", "adelante"))
            {
                return InterpretarAvance(frase);
            }

            return Ninguna("No he entendido la orden.");
        }

        private static ResultadoConversacion InterpretarGiro(string frase)
        {
            string sentido = "";

            if (frase.Contains("derecha"))
                sentido = "derecha";
            else if (frase.Contains("izquierda"))
                sentido = "izquierda";

            int? grados = ExtraerNumero(frase);

            if (grados != null && (grados < 1 || grados > 360))
            {
                return new ResultadoConversacion
                {
                    Accion = "girar",
                    ComandoCompleto = false,
                    Mensaje = "Los grados deben estar entre 1 y 360. ¿Cuántos grados quieres girar?"
                };
            }

            if (grados == null && string.IsNullOrEmpty(sentido))
            {
                return new ResultadoConversacion
                {
                    Accion = "girar",
                    ComandoCompleto = false,
                    Mensaje = "¿Cuántos grados quieres girar y en qué sentido?"
                };
            }

            if (grados == null)
            {
                return new ResultadoConversacion
                {
                    Accion = "girar",
                    ComandoCompleto = false,
                    Sentido = sentido,
                    Mensaje = "Los grados deben estar entre 1 y 360. ¿Cuántos grados quieres girar?"
                };
            }

            if (string.IsNullOrEmpty(sentido))
            {
                return new ResultadoConversacion
                {
                    Accion = "girar",
                    ComandoCompleto = false,
                    Grados = grados,
                    Mensaje = "¿Quieres girar a la derecha o a la izquierda?"
                };
            }

            return new ResultadoConversacion
            {
                Accion = "girar",
                Sentido = sentido,
                Grados = grados,
                Mensaje = "Girando " + grados.Value + " grados a la " + sentido + "."
            };
        }

        private static ResultadoConversacion InterpretarAvance(string frase)
        {
            int? metros = ExtraerNumero(frase);

            if (metros == null)
            {
                return new ResultadoConversacion
                {
                    Accion = "avanzar",
                    ComandoCompleto = false,
                    Mensaje = "La distancia debe estar entre 1 y 100 metros. ¿Cuántos metros quieres avanzar?"
                };
            }

            if (metros < 1 || metros > 100)
            {
                return new ResultadoConversacion
                {
                    Accion = "avanzar",
                    ComandoCompleto = false,
                    Mensaje = "La distancia debe estar entre 1 y 100 metros. ¿Cuántos metros quieres avanzar?"
                };
            }

            return new ResultadoConversacion
            {
                Accion = "avanzar",
                Metros = metros,
                Mensaje = "Avanzando " + metros.Value + " metros."
            };
        }

        private static int? ExtraerNumero(string frase)
        {
            Match m = Regex.Match(frase, @"\b\d+\b");
            if (m.Success)
            {
                int valor;
                if (int.TryParse(m.Value, out valor))
                    return valor;
            }

            string[] tokens = frase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];

                if (Numeros.ContainsKey(token))
                {
                    int valor = Numeros[token];

                    // treinta y cinco, cuarenta y dos, etc.
                    if ((valor == 30 || valor == 40 || valor == 50 || valor == 60 ||
                         valor == 70 || valor == 80 || valor == 90) &&
                        i + 2 < tokens.Length && tokens[i + 1] == "y" && Numeros.ContainsKey(tokens[i + 2]))
                    {
                        int unidad = Numeros[tokens[i + 2]];
                        if (unidad >= 1 && unidad <= 9)
                            return valor + unidad;
                    }

                    // ciento veinte, doscientos treinta, etc. útil para grados.
                    if ((valor == 100 || valor == 200 || valor == 300 || valor == 400) && i + 1 < tokens.Length)
                    {
                        int siguiente;
                        if (Numeros.TryGetValue(tokens[i + 1], out siguiente))
                            return valor + siguiente;
                    }

                    return valor;
                }
            }

            return null;
        }

        private static bool Contiene(string frase, params string[] palabras)
        {
            foreach (string palabra in palabras)
            {
                if (frase.Contains(palabra))
                    return true;
            }
            return false;
        }

        private static ResultadoConversacion Ninguna(string mensaje)
        {
            return new ResultadoConversacion
            {
                Accion = "ninguna",
                Mensaje = mensaje
            };
        }

        private static string Normalizar(string texto)
        {
            if (texto == null)
                return "";

            string s = texto.Trim().ToLowerInvariant();

            s = s.Replace("á", "a")
                 .Replace("é", "e")
                 .Replace("í", "i")
                 .Replace("ó", "o")
                 .Replace("ú", "u")
                 .Replace("ü", "u")
                 .Replace("ñ", "ñ");

            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    sb.Append(c);
                else
                    sb.Append(' ');
            }

            return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }
    }
}
