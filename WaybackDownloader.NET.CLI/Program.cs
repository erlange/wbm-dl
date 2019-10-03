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
        static readonly string cdcUrl = "web.archive.org/cdx/search/cdx";
        static readonly string webUrl = "http://web.archive.org/web/";

        static void ShowBanner()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\twbm-dl (Wayback Machine Downloader) \n\t(C)2016 - eri.airlangga@gmail.com");
            Console.WriteLine();
            Console.ResetColor();
        }



        public static void Main(string[] args)
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
                //query["collapse"] = "digest";
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
                        string line;
                        List<Archive> archives = new List<Archive>();
                        while ((line = reader.ReadLine()) != null)
                        {
                            archives.Add(new Archive() {
                                UrlKey = line.Split(' ')[0],
                                Timestamp = long.Parse(line.Split(' ')[2]),
                                Original = line.Split(' ')[3],
                                Digest = line.Split(' ')[1],
                                MimeType = line.Split(' ')[4],
                                StatusCode =  line.Split(' ')[5],
                                Length= int.Parse(line.Split(' ')[6]),
                                UrlId =webUrl + line.Split(' ')[2] + "id_/" + line.Split(' ')[3]  
                            });
                            Console.WriteLine(line);
                            count++;
                        }

                        result = archives.Count + " item(s) archived.";
                        File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +"/apa.csv", archives.ToCsv());
                        
                        archives.ToString();

                    }
                }
            }
            catch (Exception ex )
            {
                result = ex.Message;
            }
            return result;
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

        public static string ToCsv(this List<Archive> value)
        {
            StringBuilder csv = new StringBuilder();
            foreach (Archive a in value)
            {
                csv.Append(a.UrlKey);
                csv.Append(';');
                csv.Append(a.Digest);
                csv.Append(';');
                csv.Append(a.Timestamp);
                csv.Append(';');
                csv.Append(a.Original);
                csv.Append(';');
                csv.Append(a.MimeType);
                csv.Append(';');
                csv.Append(a.StatusCode);
                csv.Append(';');
                csv.Append(a.Length);
                csv.Append(';');
                csv.Append(a.UrlId);
                csv.AppendLine();
            }
            return csv.ToString();
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
        public int Limit { get; set; }

        [Option('A',"all", HelpText = "Retrieves snapshots for all HTTP status codes. \nIf omitted only retrieves the status code of 200")]
        public bool All { get; set; }

        [Option('X', "exact", HelpText = "Download only the url provied and not the full site.")]
        public bool ExactUrl { get; set; }

        [Option('L', "list", HelpText = "Only list the urls in a JSON format with the archived timestamps, won't download anything")]
        public bool ListOnly { get; set; }

        //[Option("only", HelpText = "Restrict downloading to urls that match this filter.")]
        //public string OnlyFilter { get; set; }

        //[Option("exclude", HelpText = "Skip downloading of urls that match this filter.")]
        //public string ExcludeFilter { get; set; }

    }




}

