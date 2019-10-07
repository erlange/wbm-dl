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

namespace com.erlange.wbmdl
{
    public class Program
    {
        //private static string finalUrl = string.Empty;
        static readonly string webUrl = "http://web.archive.org/web/";
        static readonly string cdcUrl = "web.archive.org/cdx/search/cdx";

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
            ShowBanner();

            Parser parser = Parser.Default;
            var result = parser.ParseArguments<Options>(args);
           
            result.WithParsed<Options>((Options opts) =>
            {
                string url = BuildOptions(opts);
                Console.WriteLine(url);
                Console.WriteLine(GetResponseString(url));
            });


            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        static void ParseArgs(string[] args)
        {
            Parser parser = Parser.Default;
            var result = parser.ParseArguments<Options>(args).MapResult(
                (Options opts) => BuildOptions(opts)
                , (parserErrors) => 1.ToString()
                );
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

                if (!opts.All)
                    query["filter"] = "statuscode:200";

                if (opts.From.IsInteger())
                    query["from"] = opts.From.Trim();

                if (opts.To.IsInteger())
                    query["to"] = opts.To.Trim();

                if(opts.ListOnly)
                    query["output"] = "json";

                if (opts.Limit.IsInteger())
                {
                    query["limit"] = opts.Limit.Trim();
                }


                builder.Query = query.ToString();
                resultUrl = builder.ToString();
            }

            return resultUrl;
        }



        static string GetResponseString(string url)
        {
            string result = string.Empty;
            int count = 0;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using ( StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        string line, urlId, fileName;
                        List<Archive> archives = new List<Archive>();
                        while ((line = reader.ReadLine()) != null)
                        {
                            urlId = @webUrl + line.Split(' ')[2] + "id_/" + @line.Split(' ')[3];
                            fileName = urlId.Split('/')[urlId.Split('/').Length - 1].Split('?')[0];
                            if (fileName.Length == 0)
                                fileName = "index.html";

                            archives.Add(new Archive()
                            {
                                UrlKey = line.Split(' ')[0],
                                Timestamp = long.Parse(line.Split(' ')[2]),
                                Original = @line.Split(' ')[3],
                                Digest = line.Split(' ')[1],
                                MimeType = line.Split(' ')[4],
                                StatusCode = line.Split(' ')[5],
                                Length = int.Parse(line.Split(' ')[6]),
                                UrlId = urlId,
                                Filename = fileName
                            });
                            Console.WriteLine(line);
                            count++;
                        }
                        result = archives.Count + " item(s) archived.";
                        SaveLog(archives, FileExtension.CSV);
                        SaveLog(archives, FileExtension.JSON);
                    }
                }
            }
            catch (Exception ex )
            {
                result = ex.Message;
            }
            return result;
        }

        static void SaveLog(List<Archive> archives, FileExtension extension)
        {
            System.Uri uri = new Uri(archives.FirstOrDefault().Original);
            string hostName = uri.Host;
            
            if (extension == FileExtension.CSV)
                File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/" + hostName + ".csv", archives.ToCsv());
            else if (extension == FileExtension.JSON)
                File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/" + hostName + ".json", archives.ToJson());
        }

        enum FileExtension
        {
            CSV=1, JSON=2
        }

        void DownloadFiles(List<Archive> archives, string outDir)
        {
            foreach(Archive archive in archives)
            {
                DownloadFile(archive.UrlId, outDir);
            }
        }
        void DownloadFile(string url, string path)
        {
            WebClient client = new WebClient();
            Uri uri = new Uri(url);
            client.DownloadFile(url, path);
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

        [Option('A',"all", HelpText = "Retrieves snapshots for all HTTP status codes. \nIf omitted only retrieves the status code of 200")]
        public bool All { get; set; }

        [Option('X', "exact", HelpText = "Download only the url provided and not the full site.")]
        public bool ExactUrl { get; set; }

        [Option('L', "list", HelpText = "Only list the urls in a JSON format with the archived timestamps, won't download anything")]
        public bool ListOnly { get; set; }

        //[Option("only", HelpText = "Restrict downloading to urls that match this filter.")]
        //public string OnlyFilter { get; set; }

        //[Option("exclude", HelpText = "Skip downloading of urls that match this filter.")]
        //public string ExcludeFilter { get; set; }

    }




}

