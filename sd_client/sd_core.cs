using System;
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
        public string filename { get; set; }
        public string parent { get; set; }
        public string type { get; set; }
        public string owner { get; set; }
        public string action { get; set; }
    }

    public class simpledrive
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymLinkFileName, string lpTargetFileName, int dwFlags);
        static string userdir = "";
        static string username;
        static string currDir;
        static string server;

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

                json += "{\"filename\":\"" + Path.GetFileName(file) + "\",\"parent\":\"" + path_raw + "\",\"type\":\"" + type + "\",\"owner\":\"" + username + "\",\"md5\":\"" + md5 + "\",\"edit\":\"" + last + "\"},";
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
            server = srv;
            string success = login(server, user, pass);
            if (success == "" || success == null)
            {
                return success;
            }

            username = user;
            currDir = "{\"filename\":\"\",\"parent\":\"\",\"type\":\"folder\",\"size\":\"\",\"owner\":\"" + user + "\",\"rootshare\":\"0\",\"hash\":\"0\"}";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            userdir = folder;

            string all_elem = get_all_elements();
            string files_to_download = get_files_to_sync(all_elem, lastsync);
            //MessageBox.Show("files_to_download: " + files_to_download);

            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<Element> dl_elements = ser.Deserialize<List<Element>>(files_to_download);
            foreach (Element elem in dl_elements)
            {
                if(elem.action == "download")
                {
                    await download(elem);
                }
                else if(elem.action == "upload")
                {
                    await upload(elem);
                }
                else if(elem.action == "delete")
                {
                    delete(elem);
                }
            }
            return "OK";
        }

        static void delete(Element element)
        {
            string path = userdir + element.parent + element.filename;
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
            string path = userdir + element.parent + element.filename;
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
                        new KeyValuePair<string, string>("action", "upload"),
                        new KeyValuePair<string, string>("paths", element.parent),
                    };

                    foreach (var keyValuePair in values)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value), String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    multipartFormDataContent.Add(new ByteArrayContent(File.ReadAllBytes(path)), '"' + "0" + '"', '"' + element.filename + '"');

                    var requestUri = "http://" + server + "/php/files_api.php";
                    var result = await client.PostAsync(requestUri, multipartFormDataContent);
                }
            }
            catch (Exception exp)
            {
                // Do something
            }
        }

        public static async Task download(Element element)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            string json = "[" + ser.Serialize(element) + "]";

            if (element.type == "folder")
            {
                Directory.CreateDirectory(userdir + element.parent + element.filename);
                return;
            }

            try
            {
                HttpClient client = new HttpClient(handler as HttpMessageHandler);
                var values = new Dictionary<string, string>
                    {
                        { "action", "download" },
                        { "source", json },
                        { "target", currDir }
                    };
                var content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = await client.PostAsync("http://" + server + "/php/files_api.php", content);

                using (FileStream fs = new FileStream(userdir + element.parent + element.filename, FileMode.Create, FileAccess.Write, FileShare.None))
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
                        { "user", user},
                        { "pass", pass}
                    };
                var content = new FormUrlEncodedContent(values);

                HttpResponseMessage response = client.PostAsync("http://" + server + "/php/core_login.php", content).Result;
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
                    { "action", "sync" },
                    { "file", currDir },
                    { "source", json },
                    { "lastsync", lastsync }
                };

                var content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = client.PostAsync("http://" + server + "/php/files_api.php", content).Result;
                var res = response.Content.ReadAsStringAsync().Result;
                return res;
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
