using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace JumpInNet
{
    class Program
    {
        private static String DirPath = @"d:/c#/";
        private static String BaseUrl = @"http://www.hacg.lol";
        private static String FecthUrl = @"http://www.hacg.lol/wp/";
        private static HashSet<String> Links = new HashSet<string>();
        private static Object locker = new object();

        public static void run()
        {
            while (true) { }
        }

        static void Main(string[] args)
        {

            String url = FecthUrl;
            InsertAndDownloadLink(url);
            Console.WriteLine(url + " fecth Done!");
            run();
        }

        public static void TasktMethod(object data)
        {
            string datastr = data as string;
            InsertAndDownloadLink(datastr);
        }

        public static void InsertAndDownloadLink(String link)
        {
            lock (locker)
            {
                if (UrlAvailable(link))
                {
                    if (Links.Add(link))
                    {
                        Console.WriteLine("link:" + link);
                        try
                        {
                            byte[] url_base = Encoding.Default.GetBytes(link);
                            String ContentHtml = GetUrltoHtml(link, "utf-8");
                            // Console.WriteLine("Save Html To File: ");

                            SaveHtmlToFile(GetFilePathFronmUrl(link), ContentHtml);

                            String[] getLinks = GetAllLinks(ContentHtml);

                            Console.WriteLine("getLinks length: " + getLinks.Length);

                            foreach (String iter in getLinks)
                            {
                                try
                                {
                                    bool ok = ThreadPool.QueueUserWorkItem(TasktMethod, iter);
                                    if (ok)
                                    {
                                        Console.WriteLine("this link can't append in tasks queue : " + iter);
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                                /*
                                Thread worker = new Thread( delegate ()
                                {
                                    Console.WriteLine("new Thread: " + iter);
                                    InsertAndDownloadLink(iter);
                                });
                                try
                                {
  
                                    worker.Start();
                                } catch(System.OutOfMemoryException ex) {
                                    // Console.WriteLine(ex.ToString());
                                } */

                            }
                            // 开启线程
                        }
                        catch (System.Exception ex)
                        {
                            Console.Write(ex.ToString());
                        }
                    }
                    else {
                        // Console.WriteLine("link was ready add in task :" + link);
                    }
                }
                else {
                    //  Console.WriteLine("link not available :"+link);
                }
            }
        }

        public static void SaveHtmlToFile(String mFileFullname, String htmlContent)
        {
            if (htmlContent.Length == 0)
                return;
            System.IO.StreamWriter mStreamWriter;
            mStreamWriter = new System.IO.StreamWriter(mFileFullname, false, System.Text.Encoding.UTF8);
            mStreamWriter.Write(htmlContent);
            //用完StreamWriter的对象后一定要及时销毁 
            mStreamWriter.Close();
            mStreamWriter.Dispose();
            mStreamWriter = null;
        }

        public static string GetUrltoHtml(string Url, string type)
        {
            try
            {
                System.Net.WebRequest wReq = System.Net.WebRequest.Create(Url);
                System.Net.WebResponse wResp = wReq.GetResponse();
                System.IO.Stream respStream = wResp.GetResponseStream();
                // Dim reader As StreamReader = New StreamReader(respStream)
                using (System.IO.StreamReader reader =
                    new System.IO.StreamReader(respStream, Encoding.GetEncoding(type))
                    )
                { return reader.ReadToEnd(); }
            }
            catch (System.Exception ex)
            {
                //errorMsg = ex.Message;
                //Console.Write(ex.ToString());
            }
            return "";
        }

        // 去除图片，外链
        public static bool UrlAvailable(string url)
        {
            /*
            jpg
            xml
            png
            gif
            js
            css
            php
            */
            if (IsBaseUrl(url))
            {
                if (url.Contains(".jpg")
                    || url.Contains(".gif")
                   || url.Contains(".png")
                   || url.Contains(".css")
                   || url.Contains(".js")

                   || url.Contains(".xml")
                   || url.Contains(".php")
                   )
                {
                    return false; //去掉一些图片之类的资源
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool IsBaseUrl(String url)
        {
            // FecthUrl;
            return url.Contains(BaseUrl);
        }

        public static string[] GetAllLinks(string html)
        {
            const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase); //新建正则模式
            MatchCollection m = r.Matches(html); //获得匹配结果
            string[] links = new string[m.Count];

            for (int i = 0; i < m.Count; i++)
            {
                links[i] = m[i].ToString(); //提取出结果
            }
            return links;
        }

        public static string GetFilePathFronmUrl(String url)
        {
            // remove url 中的 http:// 最后的 / 
            String fileName = url.Replace("http://", "").Replace("https://", "").TrimEnd('/');
            // 再把网页中的 / 替换为 - 
            String result = DirPath + fileName.Replace("/", "-").Replace("?", "-") + ".html";
            Console.WriteLine("file path: " + result);
            return result;
        }
    }
}
