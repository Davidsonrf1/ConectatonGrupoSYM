using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using static Hl7.Fhir.Model.Bundle;

namespace ConectatonGrupoSYM
{

    static class RndsNameSystem
    {
        public const string BRResultadoExameLaboratorial = "http://www.saude.gov.br/fhir/r4/StructureDefinition/BRResultadoExameLaboratorial-1.0";
        public const string BRCpfPaciente = "http://rnds.saude.gov.br/fhir/r4/NamingSystem/cpf";
    }

    static class RndsCodeSystem
    {
        public const string BRTipoDocumento = "http://www.saude.gov.br/fhir/r4/CodeSystem/BRTipoDocumento";
        public const string BRSubgrupoTabelaSUS = "http://www.saude.gov.br/fhir/r4/CodeSystem/BRSubgrupoTabelaSUS";
        public const string BRNomeExameGAL = "http://www.saude.gov.br/fhir/r4/CodeSystem/BRNomeExameGAL";
        public const string BRNomeExameLOINC = "http://www.saude.gov.br/fhir/r4/CodeSystem/BRNomeExameLOINC";
        public const string BRTipoAmostraGAL = "http://www.saude.gov.br/fhir/r4/CodeSystem/BRTipoAmostraGAL";
        public const string BRTipoAmostraLOINC = "http://www.saude.gov.br/fhir/r4/CodeSystem/BRTipoAmostraLOINC";
        public const string BRResultadoQualitativoExame = "http://www.saude.gov.br/fhir/r4/CodeSystem/BRResultadoQualitativoExame";
    }

    public class Hl7Client
    {
        public string Authorization { get; set; }
        public string Author { get; set; }

        string ParsePatient(string json)
        {
            try
            {
                var parser = new FhirJsonParser();
                var bundle = parser.Parse<Bundle>(json);

                foreach (var child in bundle.Children)
                {
                    if (child is EntryComponent)
                    {
                        var entry = (EntryComponent)child;

                        if (entry.Resource is Patient)
                        {
                            var patient = (Patient)entry.Resource;
                            return patient.Id;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static string SslGet(string hostName, string path, int port, X509Certificate2Collection certificates)
        {
            /* 
             * 
             * Querido programador do futuro. Eu sei que o código abaixo está esquisito. Mas só assim que funcionou em um cliente.
             * Portanto, não mexa.
             * 
             * Obrigado pela compreensão!
             * */
            using (var client = new TcpClient(hostName, port))
            {

                using (var sslStream = new SslStream(client.GetStream(), false, (a, b, c, d) => true))
                {
                    sslStream.AuthenticateAsClient(hostName, certificates, SslProtocols.Default, false);

                    byte[] messsage = Encoding.UTF8.GetBytes($@"GET {path}
"); // <= Principalmente nessa linha!

                    sslStream.Write(messsage);
                    sslStream.Flush();

                    string response = ReadResponse(sslStream);

                    client.Close();

                    return response;
                }
            }
        }

        static string ReadResponse(SslStream sslStream)
        {
            byte[] buffer = new byte[8192];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;

            using (var ms = new MemoryStream())
            {
                do
                {
                    bytes = sslStream.Read(buffer, 0, buffer.Length);

                    if (bytes > 0)
                        ms.Write(buffer, 0, bytes);
                } while (bytes != 0);

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public string UrlConsultaPaciente { get; set; } = "http://hapi.gointerop.com/hapi-fhir-server-connectathon/fhir/Patient";

        public string FindPatient(string cpf)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    return true;
                };

                var urlParam = $"identifier={RndsNameSystem.BRCpfPaciente}|{cpf}";
                var http = (HttpWebRequest)WebRequest.Create($"{UrlConsultaPaciente}?{urlParam}");

                http.KeepAlive = true;
                http.Accept = "*.*";
                http.Method = "GET";

                string respContent = "";

                using (var resp = http.GetResponse())
                {
                    using (var sr = new StreamReader(resp.GetResponseStream()))
                    {
                        respContent = sr.ReadToEnd();
                        return ParsePatient(respContent);
                    }
                }
            }
            catch (WebException wex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string UrlEnvioBundle { get; set; }

        public bool EnviaBundle(RndsBundle bundle)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) =>
            {
                return true;
            };

            try
            {
                var http = (HttpWebRequest)WebRequest.Create($"{UrlEnvioBundle}");

                http.KeepAlive = true;
                http.Accept = "*.*";
                http.Method = "POST";
                http.ContentType = "application/json";
                http.ServerCertificateValidationCallback = (a, b, c, d) =>
                {
                    return true;
                };

                string bundleJson = bundle.GetJsonString();
                var bundleBytes = Encoding.UTF8.GetBytes(bundleJson);
                http.ContentLength = bundleBytes.Length;

                using (var sw = http.GetRequestStream())
                {
                    sw.Write(bundleBytes, 0, bundleBytes.Length);
                    sw.Flush();
                }

                using (var resp = (HttpWebResponse)http.GetResponse())
                {
                    if (resp.StatusCode == HttpStatusCode.Created)
                    {
                        bundle.ContentLocation = resp.Headers[HttpResponseHeader.ContentLocation];

                        return true;
                    }
                }
            }

            catch (Exception ex)
            {
                
            }

            return false;
        }
    }
}
