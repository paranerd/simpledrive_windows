﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using System.Net;
using System.Windows.Forms;

namespace sd_client
{
    public class Element
    {
        public string id { get; set; }
        public string path { get; set; }
        public string type { get; set; }
        public string md5 { get; set; }
        public string action { get; set; }
    }

    public class Response
    {
        public string status { get; set; }
        public string msg { get; set; }
    }

    public class NewResponse
    {
        public List<Element> msg { get; set; }
    }

    public class simpledrive
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymLinkFileName, string lpTargetFileName, int dwFlags);
        static string userdir = "";
        static string username;
        static string currDir;
        static string server;
        static string token;

        static CookieContainer cookies;
        static HttpClientHandler handler;

        static string get_md5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        static string list_win_dir(string path_raw)
        {
            string json = "";
            string path = userdir + path_raw;

            string[] files = Directory.GetFileSystemEntries(path);
            foreach (string file in files)
            {
                string md5 = (Directory.Exists(file)) ? "0" : get_md5(file).ToLower();
                int access = (Directory.Exists(file)) ? (int)Directory.GetLastAccessTimeUtc(file).Subtract(new DateTime(1970, 1, 1)).TotalSeconds : (int)File.GetLastAccessTimeUtc(file).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                int edit = (Directory.Exists(file)) ? (int)Directory.GetLastWriteTimeUtc(file).Subtract(new DateTime(1970, 1, 1)).TotalSeconds : (int)File.GetLastWriteTimeUtc(file).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                string last = "" + Math.Max(access, edit);
                string type = (Directory.Exists(file)) ? "folder" : "unknown";

                json += "{\"path\":\"" + path_raw + Path.GetFileName(file) + "\",\"type\":\"" + type + "\",\"md5\":\"" + md5 + "\",\"edit\":\"" + last + "\"},";
                if (Directory.Exists(file))
                {
                    json += list_win_dir(path_raw + Path.GetFileName(file) + "/");
                    continue;
                }
            }
            return json;
        }

        static string get_all_elements()
        {
            string json = list_win_dir("/");
            json = (json == "") ? "[]" : "[" + json.Remove(json.Length - 1) + "]";
            return json;
        }

        public static async Task<string> sync(string srv, string user, string pass, string folder, string lastsync)
        {
            MessageBox.Show("start sync?");
            server = srv;
            string result = login(server, user, pass);
            if (result == null)
            {
                return null;
            }

            JavaScriptSerializer jss = new JavaScriptSerializer();
            Response res = jss.Deserialize<Response>(result);

            token = res.msg;
            username = user;
            currDir = "0";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            userdir = folder;

            string all_elem = get_all_elements();
            string files_to_download = get_files_to_sync(all_elem, lastsync);

            MessageBox.Show("res: " + files_to_download);

            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<Element> dl_elements = ser.Deserialize<List<Element>>(files_to_download);
            NewResponse dl_test = ser.Deserialize<NewResponse>(files_to_download);

            if (dl_test.msg == null)
            {
                return null;
            }

            foreach (Element elem in dl_test.msg)
            {
                MessageBox.Show(elem.action + " " + elem.path);

                if (elem.action == "download")
                {
                    await download(elem);
                }
                else if (elem.action == "upload")
                {
                    await upload(elem);
                }
                else if (elem.action == "delete")
                {
                    delete(elem);
                }
            }
            return "OK";
        }

        static void delete(Element element)
        {
            string path = userdir + element.path;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if(Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        static async Task upload(Element element)
        {
            string path = userdir + element.path;
            string parent = Directory.GetParent(path).FullName;
            string relative_parent = parent.Substring(userdir.Length);
            relative_parent = relative_parent.Replace("\\", "/");
            if (Directory.Exists(path) || !File.Exists(path))
            {
                // Don't upload empty folders or try to upload non-existing files
                return;
            }
            try
            {
                HttpClient client = new HttpClient(handler as HttpMessageHandler);
                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    var values = new[]
                    {
                        new KeyValuePair<string, string>("target", currDir),
                        new KeyValuePair<string, string>("paths", relative_parent),
                        new KeyValuePair<string, string>("token", token),
                    };

                    foreach (var keyValuePair in values)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value), String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    multipartFormDataContent.Add(new ByteArrayContent(File.ReadAllBytes(path)), '"' + "0" + '"', '"' + Path.GetFileName(element.path) + '"');

                    var requestUri = "http://" + server + "/api/files/upload";
                    var result = await client.PostAsync(requestUri, multipartFormDataContent);
                }
            }
            catch (Exception exp)
            {
                // Do something
            }
        }

        private static bool createFolderIfNeeded(string path)
        {
            string parent = Directory.GetParent(path).FullName;

            if (!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }

            return Directory.Exists(parent);
        }

        public static async Task download(Element element)
        {
            createFolderIfNeeded(userdir + element.path);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            string json = "[" + ser.Serialize(element.id) + "]";

            if (element.type == "folder")
            {
                Directory.CreateDirectory(userdir + element.path);
                return;
            }

            try
            {
                HttpClient client = new HttpClient(handler as HttpMessageHandler);
                var values = new Dictionary<string, string>
                    {
                        { "token", token },
                        { "target", json }
                    };
                var content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = await client.PostAsync("http://" + server + "/api/files/get", content);

                using (FileStream fs = new FileStream(userdir + element.path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
            catch (Exception exp)
            {
                // Do something
            }
        }

        public static void create_link(string path, string target)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(path);
                try
                {
                    lnk.TargetPath = target;
                    lnk.IconLocation = "shell32.dll, 158";
                    lnk.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(lnk);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        public static void prep_cookiecontainer()
        {
            cookies = new CookieContainer();
            handler = new HttpClientHandler()
            {
                CookieContainer = cookies
            };
            handler.UseCookies = true;
            handler.UseDefaultCredentials = false;
        }

        public static string login(string server, string user, string pass)
        {
            try
            {
                if (handler == null)
                {
                    prep_cookiecontainer();
                }
                HttpClient client = new HttpClient(handler as HttpMessageHandler);

                var values = new Dictionary<string, string>
                    {
                        { "user", user },
                        { "pass", pass },
                        { "token", token }
                    };
                var content = new FormUrlEncodedContent(values);

                HttpResponseMessage response = client.PostAsync("http://" + server + "/api/core/login", content).Result;
                string res = response.Content.ReadAsStringAsync().Result;
                if ((int)response.StatusCode == 404)
                {
                    return null;
                }
                return res;
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        static string get_files_to_sync(string json, string lastsync)
        {
            try
            {
                HttpClient client = new HttpClient(handler as HttpMessageHandler);
                var values = new Dictionary<string, string>
                {
                    { "token", token },
                    { "target", "0" },
                    { "source", json },
                    { "lastsync", lastsync }
                };

                var content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = client.PostAsync("http://" + server + "/api/files/sync", content).Result;
                var res = response.Content.ReadAsStringAsync().Result;
                return res; //.Substring(1, res.Length - 1);
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        /*static void Main(string[] args)
        {
            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
        }*/
    }
}
