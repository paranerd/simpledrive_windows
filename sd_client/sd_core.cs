using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using System.Net;

namespace sd_client
{
    public class Element
    {
        public string filename { get; set; }
        public string realpath { get; set; }
        public string type { get; set; }
        public string owner { get; set; }
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
                FileAttributes attr = File.GetAttributes(file);
                string md5 = ((attr & FileAttributes.Directory) == FileAttributes.Directory) ? "0" : get_md5(file).ToLower();
                string edit = ((attr & FileAttributes.Directory) == FileAttributes.Directory) ? "0" : "" + (int)File.GetLastWriteTimeUtc(file).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                json += "{\"filename\":\"" + Path.GetFileName(file) + "\",\"realpath\":\"" + path_raw + "\",\"owner\":\"" + username + "\",\"md5\":\"" + md5 + "\",\"edit\":\"" + edit + "\"},";
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
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

        public static async Task sync(string srv, string user, string pass, string folder)
        {
            server = srv;
            string success = login(server, user, pass);
            if (success == "")
            {
                return;
            }

            username = user;
            currDir = "{\"filename\":\"\",\"realpath\":\"\",\"type\":\"folder\",\"size\":\"\",\"owner\":\"" + user + "\",\"shareChild\":\"0\",\"hash\":\"0\"}";
            bool exists = Directory.Exists(folder);
            if (!exists)
            {
                Directory.CreateDirectory(folder);
            }
            userdir = folder;

            string all_elem = get_all_elements();
            string files_to_download = get_files_to_sync(all_elem, "download");
            string files_to_upload = get_files_to_sync(all_elem, "upload");
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<Element> dl_elements = ser.Deserialize<List<Element>>(files_to_download);
            foreach (Element elem in dl_elements)
            {
                await download(elem);
            }
            List<Element> ul_elements = ser.Deserialize<List<Element>>(files_to_upload);
            foreach (Element elem in ul_elements)
            {
                await upload(elem);
            }
        }

        static async Task upload(Element element)
        {
            string path = userdir + element.realpath + element.filename;
            FileAttributes attr = File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                // Don't upload empty folders
                return;
            }
            try
            {
                HttpClient client = new HttpClient(handler as HttpMessageHandler);
                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    var values = new[]
                    {
                        new KeyValuePair<string, string>("dir", currDir),
                        new KeyValuePair<string, string>("act", "upload"),
                        new KeyValuePair<string, string>("paths", element.realpath),
                    };

                    foreach (var keyValuePair in values)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value), String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    multipartFormDataContent.Add(new ByteArrayContent(File.ReadAllBytes(path)), '"' + "0" + '"', '"' + element.filename + '"');

                    var requestUri = "http://" + server + "/php/files_upload.php";
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
                Directory.CreateDirectory(userdir + element.realpath + element.filename);
                return;
            }

            try
            {
                HttpClient client = new HttpClient(handler as HttpMessageHandler);
                var values = new Dictionary<string, string>
                    {
                        { "action", "download" },
                        { "source", json },
                        { "file", currDir }
                    };
                var content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = await client.PostAsync("http://" + server + "/php/files_api.php", content);

                using (FileStream fs = new FileStream(userdir + element.realpath + element.filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
            catch (Exception exp)
            {
                // Do something
            }
        }

        public static void create_fav_link(string target)
        {
            Version vs = Environment.OSVersion.Version;
            string fav_path = (vs.Major == 6 && vs.Minor == 1 /* Win7 */) ? System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\Favorites\simpleDrive.lnk" : System.Environment.GetEnvironmentVariable("USERPROFILE") + @"\Links\simpleDrive.lnk";
            if (File.Exists(fav_path))
            {
                File.Delete(fav_path);
            }
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(fav_path);
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

        static string get_files_to_sync(string json, string act)
        {
            try
            {
                HttpClient client = new HttpClient(handler as HttpMessageHandler);
                var values = new Dictionary<string, string>
                {
                    { "action", "sync" },
                    { "file", currDir },
                    { "source", json },
                    { "act", act }
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
