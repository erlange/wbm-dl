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


namespace com.erlange.wbmdl
{
    public class Program
    {
        private static OptionDictionary options = LoadOptions();
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
            BuildUrl(args);
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        static bool IsValidArg(string arg, string option)
        {
            bool isValid = false;
            isValid = isValid && arg.Trim().ToUpperInvariant() == option.Trim().ToUpperInvariant();
            return isValid;
        }

        static void ShowSimpleOption()
        {
            Console.WriteLine("  Usage: wbm-dl [options]");
            Console.WriteLine("  Usage: wbm-dl [url]");
            Console.WriteLine();
            Console.WriteLine ("  [options]:");
            for (int i = 0; i < options.Items.Count; i++)
            {
                Console.WriteLine("      {0}\t{1}", options.Items[i].Name, options.Items[i].ShortDescription);
            }
            Console.WriteLine();
            Console.WriteLine("  [url]:");
            Console.WriteLine("      URL of the archived web site.");
            Console.WriteLine();
        }

        static void ShowOptions()
        {
            Console.WriteLine("Usage:");
            
            for (int i = 0; i < options.Items.Count; i++)
            {

                //if (Console.CursorTop > 20)
                //{
                //    Console.ReadKey();
                //    Console.ReadKey();
                //}

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(options.Items[i].Name);
                Console.ResetColor();
                Console.WriteLine(options.Items[i].Description);
                //Console.WriteLine(optionDictionary.Options[i].IsRequired);
                Console.WriteLine("Example:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(options.Items[i].Example);
                Console.ResetColor();
                Console.WriteLine();

            }

        }

        public void ParseArgs(string[] args)
        {

        }

        static int BuildUrl(string[] args)
        {
            string url = "ukmdepok.co.id";

            if (args.Length == 0)
            {
                ShowSimpleOption();
                return 0;
            }

            if (args.Length == 1)
            {
                if (args[0].Trim().IsValidURL())
                {
                    UriBuilder builder = new System.UriBuilder(BaseUrl);
                    System.Collections.Specialized.NameValueCollection query = System.Web.HttpUtility.ParseQueryString(string.Empty);
                    query["url"] = args[0] + "/*";
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
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "GET";
                using ( System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                {
                    using ( System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex )
            {
                result = ex.Message;
            }
            return result;
        }

        private static OptionDictionary LoadOptions()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(a => a.EndsWith("params.json")).FirstOrDefault();
            System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OptionDictionary));
            OptionDictionary optionDictionary = (OptionDictionary)serializer.ReadObject(stream);
            return optionDictionary;
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

    [DataContract(Name ="Option")]
    internal class Option 
    {
        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Value", Order = 1)]
        public string Value { get; set; }

        [DataMember(Name = "ShortDescription", Order = 2)]
        public string ShortDescription { get; set; }

        [DataMember(Name = "Description", Order = 3)]
        public string Description { get; set; }

        [DataMember(Name = "IsRequired", Order = 4)]
        public bool IsRequired { get; set; }

        [DataMember(Name = "Example", Order = 5)]
        public string Example { get; set; }
    }

    [DataContract(Name ="OptionData")]
    internal class OptionDictionary
    {
        [DataMember(Name = "OptionName", Order = 0)]
        public string Name { get; set; }
        [DataMember(Name = "Options", Order = 1)]
        public IList<Option> Items { get; set; }

    }


    [CollectionDataContract]
    class OptionRepository : IEnumerable<Option>, IOptionRepository<Option>
    {
        IList<Option> options = null;

        public OptionRepository()
        {
            options=new List<Option>();
        }

        public OptionRepository(IList<Option> optionList)
        {
            options = optionList;
        }

        public void Add(Option option)
        {
            options.Add(option);
        }

        public IList<Option> GetAll()
        {
            return options;
        }

        public Option GetByName(string name)
        {
            return options.Where(a => a.Name == name).FirstOrDefault();
        }

        public IEnumerator<Option> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    interface IOptionRepository<T> where T : class
    {
        IList<T> GetAll();
        T GetByName(string name);
    }

}
namespace com.erlange.wbmdl.helper
{
    class Utility
    {

    }
}

