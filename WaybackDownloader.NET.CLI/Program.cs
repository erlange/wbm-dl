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

namespace com.erlange.wbmdl
{
    public class Program
    {
        private static string BaseUrl = "web.archive.org/cdx/search/cdx";

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

            var parser = Parser.Default;
            var result = parser.ParseArguments<Options>(args).MapResult(
                (Options opts) => RunCommand(opts)
                ,(parserErrors)=> 1
                );
                 
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        static int RunCommand(Options opts)
        {
            return 0;
        }

        static bool IsValidArg(string arg, string option)
        {
            bool isValid = false;
            isValid = isValid && arg.Trim().ToUpperInvariant() == option.Trim().ToUpperInvariant();
            return isValid;
        }



        public void ParseArgs(string[] args)
        {

        }

        static int BuildUrl(string[] args)
        {
            string url = string.Empty;

            if (args.Length == 0)
            {
                return 0;
            }

            if (args.Length == 1)
            {
                if (args[0].Trim().IsValidURL() && !args[0].Trim().ToLowerInvariant().Equals("-url"))
                {
                    UriBuilder builder = new System.UriBuilder(BaseUrl);
                    System.Collections.Specialized.NameValueCollection query = System.Web.HttpUtility.ParseQueryString(string.Empty);
                    query["url"] = args[0] + "/*";
                    query["fl"] = "timestamp,original,statuscode";
                    //query["collapse"] = "digest";
                    query["collapse"] = "urlkey";
                    query["filter"] = "statuscode:200";
                    builder.Query = query.ToString();
                    string resultUrl = builder.ToString();
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine(System.Web.HttpUtility.UrlDecode(resultUrl));
                    Console.ResetColor();


                    Console.WriteLine(GetResponseString(resultUrl));

                    return 1;
                }
            }
            return 0;

        }



        static string GetResponseString(string url)
        {
            string result = string.Empty;
            int count = 0;
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "GET";
                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                {
                    using ( System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        while (reader.ReadLine() != null)
                        {
                            count++;
                        }
                        result = count.ToString() + " item(s) archived.";
                        

                        
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
        public static Boolean IsValidArgs(this string[] args)
        {
            Boolean isValid = false;
            if (args.Length == 0)
            {
                return false;
            }
            return isValid;
        }

        public static bool IsValidURL(this string URL)
        {
            string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";

            //https://regexr.com/3e6m0
            //string Pattern = @"(http(s)?:\/\/.)?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";

            Regex Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Rgx.IsMatch(URL);
        }


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

        [Option('a',"all", HelpText = "Retrieves all snapshots for each timestamp. \nIf omitted only retrieves one snapshot per each timestamp.")]
        public bool All { get; set; }

        [Option('x', "exact", HelpText = "Download only the url provied and not the full site.")]
        public bool ExactUrl { get; set; }

        [Option("only", HelpText = "Restrict downloading to urls that match this filter.")]
        public string OnlyFilter { get; set; }

        [Option("exclude", HelpText = "Skip downloading of urls that match this filter.")]
        public string ExcludeFilter { get; set; }

    }




}

