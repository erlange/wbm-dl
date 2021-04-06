﻿// Copyright © 2018 eri.airlangga@gmail.com
//
// Do what you want with this program
// as long as the first line above is kept intact
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using CommandLine;
using System.Web;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading;

namespace com.erlange.wbmdl
{
    public class Program
    {
        static readonly string webUrl = "https://web.archive.org/web/";
        static readonly string cdcUrl = "web.archive.org/cdx/search/cdx";
        static readonly string subDir = "/websites/";
        static readonly string logSubDir = "/logs/";
        static readonly string defaultIndexFile = "index.html";
        static readonly object threadLocker = new object();
        static readonly object errorLocker = new object();
        static readonly object logLocker = new object();
        static int archiveCount, errorCount, totalCount;
        static List<Log> logs = new List<Log>();


        static void ShowBanner()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\twbm-dl (Wayback Machine Downloader) \n\t(C)2018 - eri.airlangga@gmail.com" );
            Console.WriteLine();
            Console.ResetColor();
        }


        static void Main(string[] args)
        {
            archiveCount = 0;
            ShowBanner();

            Parser parser = Parser.Default;
            var result = parser.ParseArguments<Options>(args);
            result.WithParsed<Options>((Options opts) =>
            {
                if (string.IsNullOrEmpty(opts.Url))
                {
                    Console.WriteLine("You must input the URL, for example: \n\t wbm-dl yoursite.com \n\t wbm-dl http:\\\\yoursite.com \n\t wbm-dl yoursite.com -o c:\\outputdir");
                    Console.WriteLine();
                    Console.WriteLine("Use --help to display help screen.");
                    return;
                }

                if (!opts.Url.IsValidURL())
                {

                    Console.WriteLine("Please enter a valid URL, for example: \n\t wbm-dl yoursite.com \n\t wbm-dl http:\\\\yoursite.com \n\t wbm-dl yoursite.com -o c:\\outputdir");
                    Console.WriteLine();
                    Console.WriteLine("Use --help to display help screen.");
                    return;
                }

                string url = BuildOptions(opts);

                //Console.WriteLine(url);
                List<Archive> archives = GetResponse(url );

                if (archives.Count == 0)
                    return;

                System.Uri uri = new Uri(archives.FirstOrDefault().Original);
                string hostName = opts.Url;
                string path;

                if (string.IsNullOrEmpty(opts.OutputDir))
                    path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/" + subDir + "/";
                else
                    path = opts.OutputDir + "/" + subDir + "/";

                List<Archive> archivesToDownload = opts.AllTimestamps ? archives : GetLatestOnly(archives);
                //List<Archive> archivesToDownload=null;
                //if (!string.IsNullOrWhiteSpace(opts.OnlyFilter))
                //    archivesToDownload = archivesToDownload.Where<Archive>(a => a.Original.IsMatch(opts.OnlyFilter)|| a.Filename.IsMatch(opts.OnlyFilter)).ToList<Archive>();
                //if (!string.IsNullOrWhiteSpace(opts.ExcludeFilter))
                //    archivesToDownload = archivesToDownload.Where<Archive>(a => !(a.Original.IsMatch(opts.ExcludeFilter) || a.Filename.IsMatch(opts.ExcludeFilter))).ToList<Archive>();


                totalCount = archivesToDownload.Count;

                if (opts.ListOnly)
                {
                    SaveList(archives, FileType.JSON);
                    SaveList(archives, FileType.JSON, path);
                }
                else
                {
                    //SaveList(archives, FileType.JSON,path);
                    Console.WriteLine("Downloading " + archivesToDownload.Count + " item(s)");
                    StartDownload(archivesToDownload, path, opts.Threadcount, opts.AllTimestamps, opts.AllHttpStatus);
                    SaveLog(logs, FileType.JSON, path);
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
            var groupByLatest = from a in archives
                         where a.Length != 0
                         group a by a.LocalPath
                         into g
                         select new
                         {
                             LocalPath = g.Key,
                             maxTimestamp = (from t2 in g select t2.Timestamp).Max()
                         };

            var latestArchives = from a in archives
                                  join f in groupByLatest
                                  on
                                  new { X1 = a.LocalPath, X2 = a.Timestamp } equals new { X1 = f.LocalPath, X2 = f.maxTimestamp }
                                  select a;
            return latestArchives.ToList<Archive>();
        }

        static void StartDownload(List<Archive> archives, string outDir, int threadCount, bool isAllTimestamps, bool isAllHttpStatus)
        {
            DateTime start, finish;
            if (threadCount <= 1)
            {
                start = DateTime.Now;
                DownloadArchives(archives.ToList<Archive>(), outDir, isAllTimestamps, isAllHttpStatus);
                finish = DateTime.Now;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Operation completed in " + finish.Subtract(start).TotalSeconds.ToString("0.#0") + "s. Total " + Directory.EnumerateFiles(Path.GetFullPath(outDir), "*.*", SearchOption.AllDirectories).Count() + " saved in " + Path.GetFullPath(outDir));
                Console.ResetColor();
            }
            else
            {
                start = DateTime.Now;
                if (threadCount > archives.Count)
                    threadCount = archives.Count;

                Thread[] threads = new Thread[threadCount];

                int pageSize;
                //int pageSize = (archives.Count / threadCount) + 1;
                if ((archives.Count % threadCount)==0)
                    pageSize = (archives.Count / threadCount) ;
                else
                    pageSize = (archives.Count / threadCount) + 1;

                for (int i = 0; i < threadCount; i++)
                {
                    List<Archive> splitArchives = archives.Skip(i * pageSize).Take(pageSize).ToList();
                    System.Threading.ThreadStart threadStart = new System.Threading.ThreadStart(() => DownloadArchives(splitArchives, outDir, isAllTimestamps, isAllHttpStatus));
                    threads[i] = new System.Threading.Thread(threadStart);
                    threads[i].Name = "T" + (i + 1);
                }
                for (int i = 0; i < threadCount; i++)
                    threads[i].Start();

                for (int i = 0; i < threadCount; i++)
                    threads[i].Join();

                finish = DateTime.Now;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Operation completed in " + finish.Subtract(start).TotalSeconds.ToString("0.#0") + "s." );
                //Console.WriteLine("Total " + Directory.EnumerateFiles(Path.GetFullPath(outDir), "*.*", SearchOption.AllDirectories).Count() + " item(s) saved in " + Path.GetFullPath(outDir));
                Console.WriteLine("Total " + archiveCount + " item(s) saved in " + Path.GetFullPath(outDir));
                Console.WriteLine("Error: " + errorCount + " item(s).");
                Console.WriteLine("Logs saved in: " + Path.GetFullPath(outDir + logSubDir));
                if (errorCount > 0)
                {
                    Console.WriteLine("In case of errors, you can manually download the file from the log file.");
                    Console.WriteLine("Below are some suggestions that may minimize risk of errors:");
                    Console.WriteLine("   - Decrease the number of threads (-c parameter)");
                    Console.WriteLine("   - Shorten the output directory name (-o parameter).");
                    Console.WriteLine("   - Use filters to limit the number of downloads (-l, -f, -t parameters).");
                }
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

                query["url"] =opts.Url + (opts.ExactUrl ? "" : "/*");
                query["fl"] = "urlkey,digest,timestamp,original,mimetype,statuscode,length";
                query["collapse"] = "digest";
                query["pageSize"] = "1";
                query["gzip"] = "false";

                if (!opts.AllHttpStatus)
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

                if (!String.IsNullOrWhiteSpace(opts.OnlyFilter))
                    resultUrl = builder.ToString() + "&filter=original:" + opts.OnlyFilter;
                if (!String.IsNullOrWhiteSpace(opts.ExcludeFilter))
                    resultUrl = builder.ToString() + "&filter=!original:" + opts.ExcludeFilter;

                //resultUrl = builder.ToString() + "&collapse=timestamp:" + opts.Collapse.ToString();

            }

            return resultUrl;
        }


        static void DisplayWaiting()
        {
            int yy = Console.CursorTop;
            string waitStatus = "Getting archived list. Please wait";
            int i = 0;
            const int max= 6;
            while (true)
            {
                Console.SetCursorPosition(0, yy);
                Console.Write(" ".PadRight(waitStatus.Length + max, ' '));
                //Console.Write(waitStatus.PadRight(waitStatus.Length + (i % max), ' '));
                Console.SetCursorPosition(0, yy);
                Console.Write(waitStatus.PadRight(waitStatus.Length + (i % max), '.'));
                Thread.Sleep(200);
                i++;
            }
        }

        static List<Archive> GetResponse(string url)
        {
            List<Archive> archives = new List<Archive>();
            int count = 0;
            int y = Console.CursorTop;
            int x = Console.CursorLeft;
            Thread t = new Thread(() => DisplayWaiting());
            try
            {
                t.Start();
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


                            if (urlId.EndsWith("/") || !fileName.Contains("."))
                                fileName = defaultIndexFile;

                            
                            if (!string.IsNullOrEmpty(response.GetResponseHeader("content-disposition")) )
                            {
                                System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition(response.GetResponseHeader("content-disposition"));
                                fileName = cd.FileName;
                            }

                            localPath = uri.Host + "/" + uri.AbsolutePath.Replace(fileName, "");
                            localPath += HttpUtility.UrlEncode(uri.Query.Replace("?", ""));
                            localPath += "/" + fileName;
                            localPath = localPath.Replace("//", "/");
                            localPath = string.Join("_", localPath.Split(':', '*', '?', '"', '<', '>', '|'));

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
                            count++;
                        }

                        Console.SetCursorPosition(x, y);
                        Console.Write(" ".PadRight(Console.WindowWidth, ' '));
                        Console.SetCursorPosition(x, y);
                        Console.WriteLine("Found " + archives.Count + " total item(s).       ");
                        Console.WriteLine(" with " + GetLatestOnly(archives).Count + " unique/latest item(s).       ");
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                t.Abort();
                t.Join();
                Console.ResetColor();
            }
            return archives;
        }

        static void SaveList(List<Archive> archives, FileType extension)
        {
            if (extension == FileType.CSV)
                Console.WriteLine(archives.ToCsv());
            else if (extension == FileType.JSON)
                Console.WriteLine(archives.ToJson());
        }

        static void SaveList(List<Archive> archives, FileType extension, string path)
        {
            System.Uri uri = new Uri(archives.FirstOrDefault().Original);
            string hostName = uri.Host;
            string logPath = path + logSubDir ;
            Directory.CreateDirectory(logPath);

            if (extension == FileType.CSV)
                File.WriteAllText(logPath + hostName + ".csv", archives.ToCsv());
            else if (extension == FileType.JSON)
                File.WriteAllText(logPath + hostName + ".json", archives.ToJson());
        }

        static void SaveLog(List<Log> logs, FileType extension)
        {
            if (extension == FileType.JSON)
                Console.WriteLine(logs.ToJson());
            if (extension == FileType.CSV)
                Console.WriteLine(logs.ToCsv());

        }
        static void SaveLog(List<Log> logs, FileType extension, string path)
        {
            System.Uri uri = new Uri(logs.FirstOrDefault().Original);
            string hostName = uri.Host;
            string logPath = path + logSubDir;
            Directory.CreateDirectory(logPath);

            if (extension == FileType.CSV)
                File.WriteAllText(logPath + hostName + ".log.csv", logs.ToCsv());
            else if (extension == FileType.JSON)
                File.WriteAllText(logPath + hostName + ".log.json", logs.ToJson());
        }

        static void SaveLog(Log log, FileType extension, string path)
        {
            System.Uri uri = new Uri(logs.FirstOrDefault().Original);
            string hostName = uri.Host;
            string logPath = path + logSubDir;
            Directory.CreateDirectory(logPath);

            //TODO: write log per line using StreamWriter
            //if (extension == FileType.CSV)
            //    File.WriteAllText(logPath + hostName + ".log.csv", logs.ToCsv());
            //else if (extension == FileType.JSON)
            //    File.WriteAllText(logPath + hostName + ".log.json", logs.ToJson());
        }

        enum FileType
        {
            CSV=1, JSON=2
        }

        static void DownloadArchives(List<Archive> archives, string path, bool isAllTimestamps, bool isAllHttpStatus)
        {
            string itemPath;
            System.Uri uri;
            if (archives !=null)
            {
                using (WebClient client = new WebClient())
                {
                    //client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    foreach (Archive archive in archives)
                    {
                        uri = new Uri(archive.Original);
                        itemPath = path + "/" + (isAllTimestamps ? archive.LocalPathTimestamp : archive.LocalPath);
                        DownloadSingleArchive(client, archive, itemPath, isAllHttpStatus);
                    }
                }
            }
        }

        private static void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        static void DownloadSingleArchive(WebClient client, Archive archive, string path, bool isAllHttpStatus)
        {

            
            string dirPath = path.Replace(archive.Filename, "");
            string filePath = path;
            try
            {
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                //Workaround for invalid certificates
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                client.DownloadFile(archive.UrlId, filePath);
                if (!string.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
                {
                    ContentDisposition cd = new ContentDisposition(client.ResponseHeaders["Content-Disposition"]);
                    string cdFilePath = System.IO.Path.Combine(dirPath, cd.FileName);
                    if (System.IO.File.Exists(cdFilePath))
                        File.Delete(cdFilePath);

                    System.IO.File.Move(filePath, cdFilePath);
                    filePath = cdFilePath;
                }

                lock (threadLocker)
                    archiveCount++;

                Console.WriteLine(archiveCount + "/" + totalCount + ". " + archive.Timestamp + " " + archive.Original + " --> " + Path.GetFullPath(filePath) + " " + DateTime.Now.ToString("yyyyMMdd hh:mm:ss"));

                logs.Add(new Log()
                {
                    Num = archiveCount,
                    Original = archive.Original,
                    Source = archive.UrlId,
                    Status = archive.StatusCode,
                    ErrorMsg = "",
                    Target = Path.GetFullPath(filePath),
                    Time = DateTime.Now.ToString("yyyyMMdd hh:mm:ss")
                });
            }
            catch (System.Net.WebException ex)
            {
                if (isAllHttpStatus)
                {
                    try
                    {
                        System.Net.HttpWebResponse r = (System.Net.HttpWebResponse)ex.Response;
                        using (r)
                        {
                            if (r != null)
                            {
                                filePath = path + "." + r.StatusCode.GetHashCode().ToString() + ".html";
                                using (StreamReader reader = new StreamReader(r.GetResponseStream()))
                                {
                                    lock (new object())
                                        File.CreateText(filePath).WriteLine(reader.ReadToEnd());
                                }
                            }
                        }

                        lock (threadLocker)
                            archiveCount++;

                        logs.Add(new Log()
                        {
                            Num = archiveCount,
                            Original = archive.Original,
                            Source = archive.UrlId,
                            Status = (r == null ? "(Not downloaded)" : r.StatusCode.GetHashCode().ToString() + " (" + r.StatusDescription + ")"),
                            ErrorMsg = (r == null ? "Null response from the server" : ""),
                            Target = Path.GetFullPath(filePath),
                            Time = DateTime.Now.ToString("yyyyMMdd hh:mm:ss")
                        });

                        Console.WriteLine(archiveCount + "/" + totalCount + ". " + archive.Timestamp + " " + archive.Original + " --> " + Path.GetFullPath(filePath) + " " + DateTime.Now.ToString("yyyyMMdd hh:mm:ss"));
                    }
                    catch (Exception exc)
                    {
                        LogError(archive, exc);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(archive, ex);
            }
        }

        static void LogError(Archive archive, Exception ex)
        {
            lock (errorLocker)
            {
                if (archive != null)
                {
                    errorCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("(Error not downloaded) " + archive.Timestamp + " " + archive.Original);
                    Console.WriteLine("Error message: " + ex.Message );
                    Console.ResetColor();
                    logs.Add(new Log()
                    {
                        Num = archiveCount,
                        Original = archive.Original,
                        Source = archive.UrlId,
                        Status = "",
                        ErrorMsg = "Failed. Digest: " + archive.Digest + ". StatusCode: " + archive.StatusCode + ". Source: " + ex.Message + "; " + ex.ToString(),
                        Target = "",
                        Time = DateTime.Now.ToString("yyyyMMdd hh:mm:ss")
                    });
                }
            }
        }
    }
}

