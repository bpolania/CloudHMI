﻿/*
OSHMI - Open Substation HMI
OSHMI OPC UA / DA DRIVER

	Copyright 2008-2019 - Ricardo L. Olsen
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Threading;
using Hylasoft.Opc.Ua;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Hylasoft.Opc.Da;
using System.Collections.Concurrent;
using System.Net;
using System.Web.Script.Serialization;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Opc.Ua;

namespace OSHMI_OPC_Client
{
    class Program
    {
        static public string Version = "OSHMI OPC UA/DA Driver Version 0.2";
        static public string ConfigFile = "c:\\oshmi\\conf\\opc_client.conf";
        static public int UDPPortSend = 9100;
        static public int UDPPortListen = 9101;
        static public bool logevent = true;
        static public bool logread = true;
        static public bool logcommand = true;

        static public ConcurrentQueue<OSHMI_control> ControlQueue = new ConcurrentQueue<OSHMI_control>();
        public const int bufSize = 8 * 1024;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public struct OSHMI_control
        {
            public string oshmi_cmd_tag;
            public double value;
            public long timestamp;
            public int countdown;
            public List<string> uris_checked;
        }

        public struct OPC_entry
        {
            public string opc_path;
            public string oshmi_tag;
            public string oshmi_cmd_tag;
            public string opc_type;
            public bool subscribe;
        }

        public struct OPC_server
        {
            public string application_name;
            public string certificate_file_name;
            public string certificate_password;
            public string opc_url;
            public List<OPC_entry> entries;
            public int read_period;
            public int is_opc_ua;
        }

        /// <summary>
        /// Extracts the application URI specified in the certificate.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <returns>The application URI.</returns>
        public static string GetApplicationUriFromCertificate(X509Certificate2 certificate)
        {
            // extract the alternate domains from the subject alternate name extension.
            X509SubjectAltNameExtension alternateName = null;

            foreach (X509Extension extension in certificate.Extensions)
            {
                if (extension.Oid.Value == X509SubjectAltNameExtension.SubjectAltNameOid || extension.Oid.Value == X509SubjectAltNameExtension.SubjectAltName2Oid)
                {
                    alternateName = new X509SubjectAltNameExtension(extension, extension.Critical);
                    break;
                }
            }

            // get the application uri.
            if (alternateName != null && alternateName.Uris.Count > 0)
            {
                return alternateName.Uris[0];
            }

            return string.Empty;
        }


        static void SendUdp(byte[] data)
        {
            using (UdpClient c = new UdpClient())
                c.Send(data, data.Length, "127.0.0.1", UDPPortSend);
        }

        static void WriteControl(UaClient client, OPC_entry entry, Double value)
        {
            try
            {
                string stype;
                if (entry.opc_type == "")
                    stype = client.GetDataType(entry.opc_path).ToString().ToLower();
                else
                    stype = entry.opc_type.ToLower();

                switch (stype)
                {
                    case "bool":
                    case "boolean":
                    case "system.boolean":
                        client.Write(entry.opc_path, Convert.ToBoolean(value));
                        break;
                    case "float":
                    case "single":
                    case "system.single":
                        client.Write(entry.opc_path, Convert.ToSingle(value));
                        break;
                    case "double":
                    case "system.double":
                        client.Write(entry.opc_path, value);
                        break;
                    case "system.decimal":
                        client.Write(entry.opc_path, Convert.ToDecimal(value));
                        break;
                    case "byte":
                    case "system.byte":
                        client.Write(entry.opc_path, Convert.ToByte(value));
                        break;
                    case "sbyte":
                    case "system.sbyte":
                        client.Write(entry.opc_path, Convert.ToSByte(value));
                        break;
                    case "int16":
                    case "system.int16":
                        client.Write(entry.opc_path, Convert.ToInt16(value));
                        break;
                    case "uint16":
                    case "system.uint16":
                        client.Write(entry.opc_path, Convert.ToUInt16(value));
                        break;
                    case "integer":
                    case "int32":
                    case "system.int32":
                        client.Write(entry.opc_path, Convert.ToInt32(value));
                        break;
                    case "uint32":
                    case "system.uint32":
                        client.Write(entry.opc_path, Convert.ToUInt32(value));
                        break;
                    case "int64":
                    case "system.int64":
                        client.Write(entry.opc_path, Convert.ToInt64(value));
                        break;
                    case "uint64":
                    case "system.uint64":
                        client.Write(entry.opc_path, Convert.ToUInt64(value));
                        break;
                    case "time":
                    case "date":
                    case "datetime":
                    case "system.datetime":
                        client.Write(entry.opc_path, Convert.ToDateTime(value));
                        break;
                    default:
                        Console.Write("TYPE OF OPC UA PATH NOT SUPPORTED " + stype);
                        break;
                }
                if (logcommand) Console.Write("WRITE " + entry.opc_path + " " + entry.oshmi_tag + " " + value);
            }
            catch (Exception)
            {
                Console.WriteLine("Exception on command");
            }
        }

        static void WriteControl(DaClient client, OPC_entry entry, Double value)
        {
            try
            {
                string stype;
                if (entry.opc_type == "")
                    stype = client.GetDataType(entry.opc_path).ToString().ToLower();
                else
                    stype = entry.opc_type.ToLower();

                switch (stype)
                {
                    case "vt_bool":
                    case "bool":
                    case "system.boolean":
                        client.Write(entry.opc_path, Convert.ToBoolean(value));
                        break;
                    case "vt_r4":
                    case "system.single":
                        client.Write(entry.opc_path, Convert.ToSingle(value));
                        break;
                    case "vt_r8":
                    case "float":
                    case "system.double":
                        client.Write(entry.opc_path, value);
                        break;
                    case "vt_i1":
                    case "byte":
                    case "system.byte":
                        client.Write(entry.opc_path, Convert.ToByte(value));
                        break;
                    case "vt_i2":
                    case "int16":
                    case "system.int16":
                        client.Write(entry.opc_path, Convert.ToInt16(value));
                        break;
                    case "state":
                    case "vt_i4":
                    case "int32":
                    case "integer":
                    case "system.int32":
                        client.Write(entry.opc_path, Convert.ToInt32(value));
                        break;
                    case "vt_date":
                    case "date":
                    case "datetime":
                    case "system.datetime":
                        client.Write(entry.opc_path, Convert.ToDateTime(value));
                        break;
                    default:
                        Console.Write("TYPE OF OPC DA PATH NOT SUPPORTED " + stype);
                        break;
                }
                if (logcommand) Console.Write("COMMAND " + entry.opc_path + " " + entry.oshmi_tag + " " + value);
            }
            catch (Exception)
            {
                Console.WriteLine("Exception on command");
            }
        }

        static void ProcessUa(String URI, List<OPC_entry> entries, int readperiod, string application_name, string cert_file_name, string cert_password)
        {
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            do
            {
                try
                {
                    System.Random rnd = new Random();
                    string sn = application_name + "_" + rnd.Next();

                    var options = new UaClientOptions
                    {
                        SessionTimeout = 30000U,
                        SessionName = sn,
                        UseMessageSecurity = false,
                        AutoAcceptUntrustedCertificates = true,
                        MaxSubscriptionCount = 500,
                        MaxPublishRequestCount = 100,
                        MaxNotificationQueueSize = 500,
                        MaxMessageQueueSize = 50,
                        ApplicationName = application_name,
                        ConfigSectionName = application_name
                    };

                    if (cert_file_name != "")
                    {
                        if (File.Exists(cert_file_name))
                        {
                            options.ApplicationCertificate = new X509Certificate2(cert_file_name, cert_password, X509KeyStorageFlags.MachineKeySet);
                            //options.ApplicationCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2();
                            //options.ApplicationCertificate.Import(cert_file_name);

                            Console.WriteLine("Application URI: " + GetApplicationUriFromCertificate(options.ApplicationCertificate));

                            //Print to console information contained in the certificate.
                            Console.WriteLine("{0}Certificate to string: {1}{0}", Environment.NewLine, options.ApplicationCertificate.ToString(true));
                        }
                        else
                            Console.WriteLine("Certificate file not found: " + cert_file_name);
                    }

                    using (var client = new UaClient(new Uri(URI), options))
                    {
                        client.Connect();
                        Console.WriteLine("Connect UA " + URI);
                        
                        foreach (OPC_entry entry in entries)
                        {
                            if (!entry.subscribe)
                                continue;

                            string tag;
                            if (entry.oshmi_tag == "")
                                tag = entry.opc_path;
                            else
                                tag = entry.oshmi_tag;

                            string stype;
                            if (entry.opc_type == "")
                                stype = client.GetDataType(entry.opc_path).ToString().ToLower();
                            else
                                stype = entry.opc_type.ToLower();

                            switch (stype)
                            {
                                case "bool":
                                case "boolean":
                                case "system.boolean":
                                    {
                                        client.Monitor<Boolean>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            bool bval = readEvent.Value;
                                            string sval = bval.ToString().ToLower();
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + sval +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                ",\"timestamp\": " + (Int32)(readEvent.SourceTimestamp.Subtract(new DateTime(1970, 1, 1))).TotalSeconds +
                                                ",\"ms\":" + readEvent.SourceTimestamp.Millisecond + 
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + sval);
                                        });
                                    }
                                    break;
                                case "float":
                                case "single":
                                case "system.single":
                                    {
                                        client.Monitor<Single>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value.ToString("G", CultureInfo.CreateSpecificCulture("en-US")) +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value.ToString("G", CultureInfo.CreateSpecificCulture("en-US")));
                                        });
                                    }
                                    break;
                                case "double":
                                case "system.double":
                                    {
                                        client.Monitor<Double>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {                                           
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag + 
                                                "\",\"value\":" + readEvent.Value.ToString("G", CultureInfo.CreateSpecificCulture("en-US")) + 
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false" ) +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value.ToString("G", CultureInfo.CreateSpecificCulture("en-US")));
                                        });
                                    }
                                    break;
                                case "system.decimal":
                                    {
                                        client.Monitor<Decimal>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            Decimal dval = readEvent.Value;
                                            string sval = dval.ToString("G", CultureInfo.CreateSpecificCulture("en-US"));
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + sval +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + sval);
                                        });
                                    }
                                    break;
                                case "byte":
                                case "system.byte":
                                    {
                                        client.Monitor<Byte>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "sbyte":
                                case "system.sbyte":
                                    {
                                        client.Monitor<SByte>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "int16":
                                case "system.int16":
                                    {
                                        client.Monitor<Int16>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "uint16":
                                case "system.uint16":
                                    {
                                        client.Monitor<UInt16>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "integer":
                                case "int32":
                                case "system.int32":
                                    {
                                        client.Monitor<Int32>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "uint32":
                                case "system.uint32":
                                    {
                                        client.Monitor<UInt32>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "int64":
                                case "system.int64":
                                    {
                                        client.Monitor<Int64>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "uint64":
                                case "system.uint64":
                                    {
                                        client.Monitor<UInt64>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "time":
                                case "date":
                                case "datetime":
                                case "system.datetime":
                                    {
                                        client.Monitor<DateTime>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            DateTime dt = readEvent.Value;
                                            string sval = dt.Ticks.ToString();
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + sval +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + sval);
                                        });
                                    }
                                    break;
                            }
                        }
                        
                        do
                        {
                            foreach (OPC_entry entry in entries)
                            {
                                string tag;
                                if (entry.oshmi_tag == "")
                                    tag = entry.opc_path;
                                else
                                    tag = entry.oshmi_tag;

                                string stype;
                                if (entry.opc_type == "")
                                    // this function fails for opc DA returning only "System.Int16", so plese define the type manually
                                    stype = client.GetDataType(entry.opc_path).ToString().ToLower();
                                else
                                    stype = entry.opc_type.ToLower();

                                double val = 0;
                                string sval = "";
                                bool fail = false;
                                Hylasoft.Opc.Common.Quality quality = Hylasoft.Opc.Common.Quality.Bad;

                                switch (stype)
                                {
                                    case "bool":
                                    case "boolean":
                                    case "system.boolean":
                                        {
                                            var task = client.ReadAsync<Boolean>(entry.opc_path);
                                            task.Wait();
                                            bool bval = task.Result.Value;
                                            sval = bval.ToString().ToLower();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "float":
                                    case "single":
                                    case "system.single":
                                        {
                                            var task = client.ReadAsync<Single>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "double":
                                    case "system.double":
                                        {
                                            var task = client.ReadAsync<double>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "system.decimal":
                                        {
                                            var task = client.ReadAsync<Decimal>(entry.opc_path);
                                            task.Wait();
                                            Decimal dval = task.Result.Value;
                                            sval = dval.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "byte":
                                    case "system.byte":
                                        {
                                            var task = client.ReadAsync<Byte>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "sbyte":
                                    case "system.sbyte":
                                        {
                                            var task = client.ReadAsync<SByte>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "int16":
                                    case "system.int16":
                                        {
                                            var task = client.ReadAsync<Int16>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "uint16":
                                    case "system.uint16":
                                        {
                                            var task = client.ReadAsync<UInt16>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "integer":
                                    case "int32":
                                    case "system.int32":
                                        {
                                            var task = client.ReadAsync<Int32>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "uint32":
                                    case "system.uint32":
                                        {
                                            var task = client.ReadAsync<UInt32>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "int64":
                                    case "system.int64":
                                        {
                                            var task = client.ReadAsync<Int64>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "uint64":
                                    case "system.uint64":
                                        {
                                            var task = client.ReadAsync<UInt64>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "time":
                                    case "date":
                                    case "datetime":
                                    case "system.datetime":
                                        {
                                            var task = client.ReadAsync<DateTime>(entry.opc_path);
                                            task.Wait();
                                            DateTime dt = task.Result.Value;
                                            val = dt.Ticks;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    default:
                                        if (logread) Console.WriteLine("READ UNSUPPORTED TYPE: " + URI + " " + entry.opc_path + " " + stype);
                                        fail = true;
                                        break;
                                }

                                if (!fail)
                                {
                                    SendUdp(Encoding.ASCII.GetBytes(
                                        "[{\"tag\":\"" + tag +
                                        "\",\"value\":" + sval +
                                        ",\"failed\":" + (quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                        "}]"));
                                    if (logread) Console.WriteLine("READ  " + URI + " " + entry.opc_path + " " + tag + " " + sval);
                                }
                            }

                            for (int i = 0; i < readperiod; i++) {
                                // check for controls
                                OSHMI_control oc;
                                bool found = false;
                                if (ControlQueue.TryDequeue(out oc))
                                {
                                    DateTime dt = DateTime.Now;

                                    if (dt.Ticks / 10000000 < oc.timestamp / 10000000 + 10 ) // past ten seconds expires the command (discards it)
                                    {
                                        if (!oc.uris_checked.Contains(URI))
                                        {
                                            foreach (OPC_entry entry in entries)
                                            {
                                                if (entry.oshmi_cmd_tag == oc.oshmi_cmd_tag)
                                                {
                                                    found = true;
                                                    WriteControl(client, entry, oc.value);
                                                    break;
                                                }
                                            }
                                            if (!found)
                                            {
                                                oc.countdown--;
                                                if (oc.countdown > 0)
                                                {
                                                    oc.uris_checked.Add(URI); // mark as checked
                                                    ControlQueue.Enqueue(oc); // return command object to the queue if not the last to check
                                                }
                                                else
                                                   if (logcommand) Console.Write("DISCARDED COMMAND (NOT FOUND) ON " + oc.oshmi_cmd_tag + " " + oc.value);
                                            }
                                        }
                                        else
                                        {
                                            ControlQueue.Enqueue(oc); // return command object to the queue if not the last to check
                                        }
                                    }
                                    else
                                      if (logcommand) Console.Write("DISCARDED COMMAND (TIMEOUT) ON " + oc.oshmi_cmd_tag + " " + oc.value);
                                }
                                // wait a second
                                System.Threading.Thread.Sleep(1000);
                                if (found) // command sent, go on update readings
                                    break;
                            }

                        } while (true);
                    }
                }
                catch (Exception e)
                {
                    // EXCEPTION HANDLER
                    Console.WriteLine("Exception UA " + URI);
                    Console.WriteLine(e);
                    System.Threading.Thread.Sleep(15000);
                }
            } while (true);
        }

        static void ProcessDa(String URI, List<OPC_entry> entries, int readperiod)
        {
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            do
            {
                try
                {
                    using (var client = new DaClient(new Uri(URI)))
                    {
                        client.Connect();
                        Console.WriteLine("Connect DA " + URI);
                        
                        foreach (OPC_entry entry in entries)
                        {
                            if (!entry.subscribe)
                                continue;
                            string tag;
                            if (entry.oshmi_tag == "")
                                tag = entry.opc_path;
                            else
                                tag = entry.oshmi_tag;

                            string stype;
                            if (entry.opc_type == "")
                                // this function fails for opc DA returning only "System.Int16", so plese define the type manually
                                stype = client.GetDataType(entry.opc_path).ToString().ToLower();
                            else
                                stype = entry.opc_type.ToLower();

                            switch (stype)
                            {
                                case "vt_bool":
                                case "bool":
                                case "system.boolean":
                                    {
                                        client.Monitor<Boolean>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            bool bval = readEvent.Value;
                                            string sval = bval.ToString().ToLower();
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + sval +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                ",\"timestamp\": " + (Int32)(readEvent.SourceTimestamp.Subtract(new DateTime(1970, 1, 1))).TotalSeconds +
                                                ",\"ms\":" + readEvent.SourceTimestamp.Millisecond +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + sval );
                                        });
                                    }
                                    break;
                                case "vt_r4":
                                case "system.double":
                                    {
                                        client.Monitor<Single>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value.ToString("G", CultureInfo.CreateSpecificCulture("en-US")) +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value.ToString("G", CultureInfo.CreateSpecificCulture("en-US")));
                                        });
                                    }
                                    break;
                                case "vt_r8":
                                case "float":
                                case "System.Double":
                                    {
                                        client.Monitor<double>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value.ToString("G", CultureInfo.CreateSpecificCulture("en-US")) +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value.ToString("G", CultureInfo.CreateSpecificCulture("en-US")));
                                        });
                                    }
                                    break;
                                case "vt_i1":
                                case "byte":
                                case "system.byte":
                                    {
                                        client.Monitor<Byte>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "vt_i2":
                                case "int16":
                                case "system.int16":
                                    {
                                        client.Monitor<Int16>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "state":
                                case "vt_i4":
                                case "int32":
                                case "integer":
                                case "system.int32":
                                    {
                                        client.Monitor<Int32>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + tag +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + readEvent.Value);
                                        });
                                    }
                                    break;
                                case "vt_date":
                                case "date":
                                case "datetime":
                                case "system.datetime":
                                    {
                                        client.Monitor<DateTime>(entry.opc_path, (readEvent, unsubscribe) =>
                                        {
                                            DateTime dt = readEvent.Value;
                                            string sval = dt.Ticks.ToString();
                                            SendUdp(Encoding.ASCII.GetBytes(
                                                "[{\"tag\":\"" + sval +
                                                "\",\"value\":" + readEvent.Value +
                                                ",\"failed\":" + (readEvent.Quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                                "}]"));
                                            if (logevent) Console.WriteLine("EVENT " + URI + " " + entry.opc_path + " " + tag + " " + sval);
                                        });
                                    }
                                    break;
                            }
                        }
                        
                        do
                        {
                            foreach (OPC_entry entry in entries)
                            {
                                string tag;
                                if (entry.oshmi_tag == "")
                                    tag = entry.opc_path;
                                else
                                    tag = entry.oshmi_tag;

                                string stype;
                                if (entry.opc_type == "")
                                    // this function fails for opc DA returning only "System.Int16", so plese define the type manually
                                    stype = client.GetDataType(entry.opc_path).ToString().ToLower();
                                else
                                    stype = entry.opc_type.ToLower();

                                double val = 0;                                
                                string sval = "";
                                bool fail = false;
                                Hylasoft.Opc.Common.Quality quality = Hylasoft.Opc.Common.Quality.Bad;

                                switch (stype)
                                {
                                    case "vt_bool":
                                    case "bool":
                                    case "system.boolean":
                                        {
                                            var task = client.ReadAsync<Boolean>(entry.opc_path);
                                            task.Wait();
                                            bool bval = task.Result.Value;
                                            sval = bval.ToString().ToLower();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "vt_r4":
                                    case "system.single":
                                        {
                                            var task = client.ReadAsync<Single>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "vt_r8":
                                    case "float":
                                    case "system.double":
                                        {
                                            var task = client.ReadAsync<double>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "vt_i1":
                                    case "byte":
                                    case "system.byte":
                                        {
                                            var task = client.ReadAsync<Byte>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "vt_i2":
                                    case "int16":
                                    case "system.int16":
                                        {
                                            var task = client.ReadAsync<Int16>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "state":
                                    case "vt_i4":
                                    case "int32":
                                    case "integer":
                                    case "system.int32":
                                        {
                                            var task = client.ReadAsync<Int32>(entry.opc_path);
                                            task.Wait();
                                            val = task.Result.Value;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    case "vt_date":
                                    case "date":
                                    case "datetime":
                                    case "system.datetime":
                                        {
                                            var task = client.ReadAsync<DateTime>(entry.opc_path);
                                            task.Wait();
                                            DateTime dt = task.Result.Value;
                                            val = dt.Ticks;
                                            sval = val.ToString();
                                            quality = task.Result.Quality;
                                        }
                                        break;
                                    default:
                                        if (logread) Console.WriteLine("READ UNSUPPORTED TYPE: " + URI + " " + entry.opc_path + " " + stype);
                                        fail = true;
                                        break;
                                }

                                if (!fail)
                                {
                                    SendUdp(Encoding.ASCII.GetBytes(
                                        "[{\"tag\":\"" + tag +
                                        "\",\"value\":" + sval +
                                        ",\"failed\":" + (quality != Hylasoft.Opc.Common.Quality.Good ? "true" : "false") +
                                        "}]"));
                                    if (logread) Console.WriteLine("READ  " + URI + " " + entry.opc_path + " " + tag  + " " + sval);
                                }
                            }

                            for (int i = 0; i < readperiod; i++)
                            {
                                // check for controls
                                OSHMI_control oc;
                                bool found = false;
                                if (ControlQueue.TryDequeue(out oc))
                                {
                                    DateTime dt = DateTime.Now;

                                    if (dt.Ticks / 10000000 < oc.timestamp / 10000000 + 10) // past ten seconds expires the command (discards it)
                                    {
                                        if (!oc.uris_checked.Contains(URI))
                                        {
                                            foreach (OPC_entry entry in entries)
                                            {
                                                if (entry.oshmi_cmd_tag == oc.oshmi_cmd_tag)
                                                {
                                                    found = true;
                                                    WriteControl(client, entry, oc.value);
                                                    break;
                                                }
                                            }
                                            if (!found)
                                            {
                                                oc.countdown--;
                                                if (oc.countdown > 0)
                                                {
                                                    oc.uris_checked.Add(URI); // mark as checked
                                                    ControlQueue.Enqueue(oc); // return command object to the queue if not the last to check
                                                }
                                                else
                                                   if (logcommand) Console.Write("DISCARDED COMMAND (NOT FOUND) ON " + oc.oshmi_cmd_tag + " " + oc.value);
                                            }
                                        }
                                        else
                                        {
                                            ControlQueue.Enqueue(oc); // return command object to the queue if not the last to check
                                        }
                                    }
                                    else
                                      if (logcommand) Console.Write("DISCARDED COMMAND (TIMEOUT) ON " + oc.oshmi_cmd_tag + " " + oc.value);
                                }
                                // wait a second
                                System.Threading.Thread.Sleep(1000);
                                if (found) // command sent, go on update readings
                                    break;
                            }

                        } while (true);
                    }
                }
                catch (Exception e)
                {
                    // EXCEPTION HANDLER
                    Console.WriteLine("Exception DA " + URI);
                    Console.WriteLine(e);
                    System.Threading.Thread.Sleep(15000);
                }
            } while (true);
        }

        static void Main(string[] args)
        {
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Console.WriteLine(Version);

            int number_of_servers = 3;
            List<OPC_server> servers = new List<OPC_server>();

            // Read file using StreamReader. Reads file line by line  
            using (StreamReader file = new StreamReader(ConfigFile))
            {
                int counter = 0;
                string ln;
                int cnt_entries = -1;
                int cnt_servers = -1;

                while ((ln = file.ReadLine()) != null)
                {
                    counter++;
                    if (ln.Trim() == "")
                        continue;

                    var result = ln.Split(',');

                    //var result = ln.Split('"')
                    //                     .Select((element, index) => index % 2 == 0  // If even index
                    //                                           ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)  // Split the item
                    //                                           : new string[] { element })  // Keep the entire item
                    //                     .SelectMany(element => element).ToList();
                    if (result[0][0] == '#' || result.Count()==0) // comment or empty line
                        continue;
                    if (result[0].ToLower().Contains("opc.tcp://") && result.Count()>=2)
                    { // new opc ua server
                        Console.WriteLine("NEW UA SERVER");
                        string an = "OSHMI", cfn = "", passwd = "";
                        if (result.Count() >= 3)
                            an = result[2].Trim();
                        if (result.Count() >= 4)
                            cfn = result[3].Trim();
                        if (result.Count() >= 5)
                            passwd = result[4].Trim();
                        cnt_entries = -1;
                        cnt_servers++;
                        OPC_server opcserv = new OPC_server
                        {
                            application_name = an,
                            certificate_file_name = cfn,
                            certificate_password = passwd,
                            opc_url = result[0].Trim(),
                            read_period = System.Convert.ToInt32(result[1].Trim()),
                            is_opc_ua = 1,
                            entries = new List<OPC_entry>()
                        };
                        servers.Add(opcserv);
                    }
                    else
                    if (result[0].ToLower().Contains("opcda://") && result.Count()>=2)
                    { // new opc da server
                        Console.WriteLine("NEW DA SERVER");
                        cnt_entries = -1;
                        cnt_servers++;
                        OPC_server opcserv = new OPC_server
                        {
                            application_name = "",
                            certificate_file_name = "",
                            certificate_password = "",
                            opc_url = result[0].Trim(),
                            read_period = System.Convert.ToInt32(result[1].Trim()),
                            is_opc_ua = 0,
                            entries = new List<OPC_entry>()
                        };
                        servers.Add(opcserv);
                    }
                    else
                    if (result.Count()==5)
                    { // must be a tag entry
                        Console.WriteLine("NEW TAG");
                        cnt_entries++;
                        OPC_entry opcentry = new OPC_entry()
                        {
                            opc_path = result[0].Trim(),
                            opc_type = result[1].Trim(),
                            subscribe = result[2].Trim() == "Y" ? true : false,
                            oshmi_tag = result[3].Trim(),
                            oshmi_cmd_tag = result[4].Trim()
                        };
                        servers[cnt_servers].entries.Add(opcentry);
                    }
                    else
                    {
                        Console.WriteLine("Invalid config line: " + counter);
                    }

                    Console.WriteLine(ln);
                }
                file.Close();
                Console.WriteLine($"Config file has {counter} lines.");
            }

//          Console.ReadKey();

            foreach (OPC_server srv in servers)
            {
                if (srv.is_opc_ua != 0)
                {
                    Thread t = new Thread(() => ProcessUa(srv.opc_url, srv.entries, srv.read_period, srv.application_name, srv.certificate_file_name, srv.certificate_password));
                    t.Start();
                }
                else
                {
                    Thread t = new Thread(() => ProcessDa(srv.opc_url, srv.entries, srv.read_period));
                    t.Start();
                }
            }

            UdpClient listener = new UdpClient(UDPPortListen);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, UDPPortListen);

            try
            {
                while (true)
                {
                    // Waiting for messages
                    byte[] bytes = listener.Receive(ref groupEP);

                    Console.WriteLine($"Received message from {groupEP} : ");
                    string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length-1).Trim(); // bytes.Length-1 is to remove the final \0
                    Console.WriteLine(s);

                    try
                    {
                        string tag = "";
                        double dval = 0;
                        JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        dynamic result = serializer.DeserializeObject(s);
                        foreach (KeyValuePair<string, object> entry in result)
                        {
                            var key = entry.Key;
                            var value = entry.Value;
                            Console.WriteLine(String.Format("{0} : {1}", key, value));
                            if (key  == "tag")
                              tag = value.ToString();
                            if (key == "value")
                              dval = Double.Parse(value.ToString());
                        }
                        DateTime currentDate = DateTime.Now;
                        OSHMI_control oc = new OSHMI_control() { oshmi_cmd_tag = tag, value = dval, timestamp = currentDate.Ticks, countdown = number_of_servers, uris_checked = new List<string>() };
                        ControlQueue.Enqueue(oc);
                    }
                    catch (Exception) {
                        Console.WriteLine("ERROR: Invalid JSON Message!");
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }

            // Console.ReadKey();

        }
        
    }
}
