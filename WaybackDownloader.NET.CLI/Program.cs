using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using CommandLine;
using System.Web;
using System.IO;
using System.Net;
using System.Threading;

namespace com.erlange.wbmdl
{
    public class Program
    {
        static readonly string webUrl = "http://web.archive.org/web/";
        static readonly string cdcUrl = "web.archive.org/cdx/search/cdx";
        static readonly string subDir = "/";
        static readonly string logSubDir = "/logs/";
        static readonly object threadLocker = new object();
        static int archiveCount;
        static int errorCount;

        //delegate void PrintCallback(object what);

        static void ShowBanner()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\twbm-dl (Wayback Machine Downloader) \n\t(C)2016 - eri.airlangga@gmail.com");
            Console.WriteLine();
            Console.ResetColor();
        }



        static void Main(string[] args)
        {
            archiveCount = 0;
            ShowBanner();

            //PrintCallback printCallback = new PrintCallback(Print);
            Parser parser = Parser.Default;
            var result = parser.ParseArguments<Options>(args);
            result.WithParsed<Options>((Options opts) =>
            {
                string url = BuildOptions(opts);
                List<Archive> archives = GetResponse(url);

                if (archives.Count == 0)
                    return;

                System.Uri uri = new Uri(archives.FirstOrDefault().Original);
                string hostName = opts.Url;
                string path;

                if (string.IsNullOrEmpty(opts.OutputDir))
                    path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + subDir + "/";
                else
                    path = opts.OutputDir + "/" + subDir + "/";

                var archivesToDownload = opts.AllTimestamps ? archives : GetLatestOnly(archives);

                if (!opts.ListOnly)
                    StartDownload(archivesToDownload, path, opts.Threadcount, opts.AllTimestamps);

                if (!opts.ListOnly)
                    SaveLog(archives, FileExtension.JSON, path);
                else
                {
                    SaveLog(archives, FileExtension.JSON);
                    SaveLog(archives, FileExtension.JSON, path);
                }

            });


            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        static List<Archive> GetLatestOnly(List<Archive> archives)
        {
            var latest = from a in archives
                         where a.Length != 0
                         group a by a.LocalPath
                         into g
                         select new
                         {
                             LocalPath = g.Key,
                             maxTimestamp = (from t2 in g select t2.Timestamp).Max()
                         };

            var latestArchives = from a in archives
                                  join f in latest
                                  on
                                  new { X1 = a.LocalPath, X2 = a.Timestamp } equals new { X1 = f.LocalPath, X2 = f.maxTimestamp }
                                  select a;
            return latestArchives.ToList<Archive>();
        }

        static void StartDownload(List<Archive> archives, string outDir, int threadCount, bool isAllTimestamps)
        {
            DateTime start, finish;
            if (threadCount <= 1)
            {
                start = DateTime.Now;
                DownloadArchives(archives.ToList<Archive>(), outDir, isAllTimestamps);
                finish = DateTime.Now;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation completed in " + finish.Subtract(start).TotalSeconds.ToString("0.#0") + "s. Total " + Directory.EnumerateFiles(Path.GetFullPath(outDir), "*.*", SearchOption.AllDirectories).Count() + " saved in " + Path.GetFullPath(outDir));
                Console.ResetColor();
            }
            else
            {
                start = DateTime.Now;
                Thread[] threads = new Thread[threadCount];

                int pageSize = (archives.Count / threadCount) + 1;
                for (int i = 0; i < threadCount; i++)
                {
                    List<Archive> splitArchives = archives.Skip(i * pageSize).Take(pageSize).ToList();
                    System.Threading.ThreadStart threadStart = new System.Threading.ThreadStart(() => DownloadArchives(splitArchives, outDir, isAllTimestamps));
                    threads[i] = new System.Threading.Thread(threadStart);
                    threads[i].Name = "T" + (i + 1);
                }
                for (int i = 0; i < threadCount; i++)
                    threads[i].Start();

                for (int i = 0; i < threadCount; i++)
                    threads[i].Join();

                finish = DateTime.Now;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation/thread completed in " + finish.Subtract(start).TotalSeconds.ToString("0.#0") + "s." );
                Console.WriteLine("Total " + Directory.EnumerateFiles(Path.GetFullPath(outDir), "*.*", SearchOption.AllDirectories).Count() + " saved in " + Path.GetFullPath(outDir));
                Console.WriteLine("Error: " + errorCount + " item(s).");
                Console.ResetColor();

            }
        }


        static string BuildOptions(Options opts)
        {
            string resultUrl = string.Empty;
            if (opts.Url.IsValidURL())
            {
                UriBuilder builder = new System.UriBuilder(cdcUrl);
                var query = HttpUtility.ParseQueryString(string.Empty);

                query["url"] = opts.Url + (opts.ExactUrl ? "" : "/*");
                query["fl"] = "urlkey,digest,timestamp,original,mimetype,statuscode,length";
                query["collapse"] = "digest";
                query["pageSize"] = "1";
                query["gzip"] = "false";

                if (!opts.AllStatus)
                    query["filter"] = "statuscode:200";

                if (opts.From.IsLong())
                    query["from"] = opts.From.Trim();

                if (opts.To.IsLong())
                    query["to"] = opts.To.Trim();

                //if(opts.ListOnly)
                //    query["output"] = "json";

                if (opts.Limit.IsInteger())
                    query["limit"] = opts.Limit.Trim();


                builder.Query = query.ToString();
                resultUrl = builder.ToString();
            }

            return resultUrl;
        }

        static List<Archive> GetResponse(string url)
        {
            List<Archive> archives = new List<Archive>();
            int count = 0;
            int y = Console.CursorTop;
            int x = Console.CursorLeft;
            Console.WriteLine("Getting archived list...");
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        string line, original, urlId, timestamp, fileName, localPath, localPathTimestamp;
                        Uri uri;
                        while ((line = reader.ReadLine()) != null)
                        {
                            urlId = @webUrl + line.Split(' ')[2] + "id_/" + @line.Split(' ')[3];
                            timestamp = line.Split(' ')[2];
                            fileName = urlId.Split('/')[urlId.Split('/').Length - 1].Split('?')[0];
                            original = @line.Split(' ')[3];

                            uri = new Uri(original);

                            if (fileName.Length == 0)
                                fileName = "index.html";

                            localPath = uri.Host + "/" + uri.AbsolutePath.Replace(fileName, "");
                            localPath += HttpUtility.UrlEncode(uri.Query.Replace("?", ""));
                            localPath += "/" + fileName;
                            localPath= localPath.Replace("//","/");

                            localPathTimestamp = uri.Host + "/" + timestamp + uri.AbsolutePath.Replace(fileName, "");
                            localPathTimestamp += HttpUtility.UrlEncode(uri.Query.Replace("?", ""));
                            localPathTimestamp += "/" + fileName;
                            localPathTimestamp = localPathTimestamp.Replace("//", "/");

                            archives.Add(new Archive()
                            {
                                UrlKey = line.Split(' ')[0],
                                Timestamp = long.Parse(timestamp),
                                Original = original,
                                Digest = line.Split(' ')[1],
                                MimeType = line.Split(' ')[4],
                                StatusCode = line.Split(' ')[5],
                                Length = int.Parse(line.Split(' ')[6]),
                                UrlId = urlId,
                                Filename = fileName,
                                LocalPath = localPath,
                                LocalPathTimestamp = localPathTimestamp
                            });
                            //Console.WriteLine(line);
                            count++;
                        }
                        Console.SetCursorPosition(x, y);
                        Console.WriteLine("Found " + archives.Count + " total item(s).       ");
                        Console.WriteLine(" with " + GetLatestOnly(archives).Count + " unique/latest item(s).       ");
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ResetColor();
            }
            return archives;
        }

        static void SaveLog(List<Archive> archives, FileExtension extension)
        {
            System.Uri uri = new Uri(archives.FirstOrDefault().Original);
            string hostName = uri.Host;
            string logPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + subDir + hostName + logSubDir ;
            Directory.CreateDirectory(logPath);
            Console.WriteLine(archives.ToJson());
        }

        static void SaveLog(List<Archive> archives, FileExtension extension, string path)
        {
            System.Uri uri = new Uri(archives.FirstOrDefault().Original);
            string hostName = uri.Host;
            string logPath = path + logSubDir ;
            Directory.CreateDirectory(logPath);

            if (extension == FileExtension.CSV)
                File.WriteAllText(logPath + hostName + ".csv", archives.ToCsv());
            else if (extension == FileExtension.JSON)
                File.WriteAllText(logPath + hostName + ".json", archives.ToJson());
        }

        enum FileExtension
        {
            CSV=1, JSON=2
        }

        static void DownloadArchives(List<Archive> archives, string path, bool allTimestamps)
        {
            string itemPath;
            System.Uri uri;
            using (WebClient client = new WebClient())
            {
                foreach (Archive archive in archives)
                {
                    uri = new Uri(archive.Original);
                    itemPath = path + "/" + (allTimestamps ? archive.LocalPathTimestamp : archive.LocalPath);
                    DownloadSingleArchive(client, archive, itemPath);
                }
            }
        }
        static void DownloadSingleArchive(WebClient client, Archive archive, string path)
        {
            string dirPath = path.Replace(archive.Filename, "");
            string filePath = path;

            try
            {
                Directory.CreateDirectory(dirPath);
                client.DownloadFile(archive.UrlId, filePath);

                lock (threadLocker)
                    archiveCount++;

                Console.WriteLine(Thread.CurrentThread.Name + " " + archiveCount + ". " + archive.Timestamp + " " + archive.Original + " --> " + Path.GetFullPath(filePath));
            }
            catch (Exception ex)
            {
                lock (threadLocker)
                {
                    errorCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("(Error not downloaded) " + archive.Timestamp + " " + archive.Original);
                    Console.WriteLine("Error message: " + ex.Message);
                    Console.ResetColor();
                }
            }
        }




    }

    public static class ArgsExtensions
    {
        public static bool IsValidURL(this string URL)
        {
            string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";

            //https://regexr.com/3e6m0
            //string Pattern = @"(http(s)?:\/\/.)?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";

            Regex Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Rgx.IsMatch(URL);
        }

        public static bool IsLong(this string value)
        {
            long longValue = -1;
            return long.TryParse(value, out longValue);
        }

        public static bool IsInteger(this string value)
        {
            int intValue = -1;
            return int.TryParse(value, out intValue);
        }
        public static string ToJson(this List<Archive> value)
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Archive>));
            serializer.WriteObject(stream, value);
            return Encoding.Default.GetString(stream.ToArray());
        }

        public static string ToCsv(this List<Archive> value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Archive a in value)
            {
                builder.Append(a.UrlKey);
                builder.Append(',');
                builder.Append(a.Digest);
                builder.Append(',');
                builder.Append(a.Timestamp);
                builder.Append(',');
                builder.Append(a.Original);
                builder.Append(',');
                builder.Append(a.MimeType);
                builder.Append(',');
                builder.Append(a.StatusCode);
                builder.Append(',');
                builder.Append(a.Length);
                builder.Append(',');
                builder.Append(a.UrlId);
                builder.Append(',');
                builder.Append(a.Filename);
                builder.Append(',');
                builder.Append(a.LocalPath);
                builder.Append(',');
                builder.Append(a.LocalPathTimestamp);
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }

    public class Archive
    {
        public string UrlKey { get; set; }
        public long Timestamp { get; set; }
        public string Original { get; set; }
        public string Digest { get; set; }
        public string UrlId { get; set; }
        public string MimeType{ get; set; }
        public string StatusCode { get; set; }
        public long Length { get; set; }
        public string Filename { get; set; }
        public string LocalPath { get; set; }
        public string LocalPathTimestamp { get; set; }
    }


    class Options
    {
        [Option('u', "url", HelpText = "The URL of the archived web site", Required = true)]
        public string Url { get; set; }

        [Option('o', "out", HelpText = "Output/destination directory")]
        public string OutputDir { get; set; }

        [Option('f',"from", HelpText = "From timestamp. Limits the archived result SINCE this timestamp. \nUse 1 to 14 digit with the format: yyyyMMddhhmmss \nIf omitted, retrieves results since the earliest timestamp available. ")]
        public string From { get; set; }

        [Option('t',"to", HelpText = "To timestamp. Limits the archived result  UNTIL this timestamps. \nUse 1 to 14 digit with the format: yyyyMMddhhmmss \nIf omitted, retrieves results until the latest timestamp available. ")]
        public string To { get; set; }

        [Option('l', "limit", HelpText = "Limits the first N or the last N results. Negative number limits the last N results.")]
        public string Limit { get; set; }

        [Option('a',  HelpText = "All timestamps. Retrieves snapshots for all timestamps.")]
        public bool AllTimestamps { get; set; }

        [Option(shortName: 'c', longName: "count", Default = 1, HelpText = "Number of concurrent processes. \nCan speed up the process but requires more memory.")]
        public int Threadcount { get; set; }

        [Option('A',"All", HelpText = "Retrieves snapshots for all HTTP status codes. \nIf omitted only retrieves the status code of 200")]
        public bool AllStatus { get; set; }

        [Option('X', "exact", HelpText = "Downloads only the url provided and not the full site.")]
        public bool ExactUrl { get; set; }

        [Option('L', "list", HelpText = "Displays only the list in a JSON format with the archived timestamps, does not download anything")]
        public bool ListOnly { get; set; }

        //[Option("only", HelpText = "Restrict downloading to urls that match this filter.")]
        //public string OnlyFilter { get; set; }

        //[Option("exclude", HelpText = "Skip downloading of urls that match this filter.")]
        //public string ExcludeFilter { get; set; }

    }




}

