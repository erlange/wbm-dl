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
        public static void Main(string[] args)
        {
            Debug.WriteLine(args);

            if (args.Length == 0)
            {
                //ShowHelp();
                //tt();
                ReadResource();
                
            }
            else
            {
                
                Console.WriteLine(args.Length);
                Console.WriteLine(args.FirstOrDefault<string>());
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

        }

        public static void ShowHelp()
        {
            Console.WriteLine(Resources.url.Replace("@@", "\t"));
            Console.WriteLine(Resources.o.Replace("@@", "\t"));
            Console.WriteLine(Resources.from.Replace("@@", "\t"));
            Console.WriteLine(Resources.to.Replace("@@", "\t"));
            Console.WriteLine(Resources.list.Replace("@@", "\t"));

            List<Option> options = new List<Option>();
            options.Add(new Option()
            {
                Name = "-'",
                Value = "daasd",
                IsRequired = true,
                Description = "ada"
            });
            
            var c = options.Select((a) => a.Name = "sdsdf").ToList();
            OptionRepository r = new OptionRepository();
        }

        public void ParseArgs(string[] args)
        {


        }

        public static void ReadResource()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(a => a.EndsWith("params.json")).FirstOrDefault();
            System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OptionDictionary));
            OptionDictionary optionDictionary = (OptionDictionary)serializer.ReadObject(stream);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\twbm-dl (Wayback Machine Downloader) \n\t(C)2016 - eri.airlangga@gmail.com");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("Usage:");
            for (int i=0;i<optionDictionary.Options.Count;i++)
            {

                if (Console.CursorTop > 20)
                {
                    Console.ReadKey();
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(optionDictionary.Options[i].Name);
                Console.ResetColor();
                Console.WriteLine(optionDictionary.Options[i].Description);
                //Console.WriteLine(optionDictionary.Options[i].IsRequired);
                Console.WriteLine("Example:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(optionDictionary.Options[i].Example);
                Console.ResetColor();
                Console.WriteLine();

            }
        }


        public static void tt()
        {
            IList<Option> options = new List<Option>
            {
                new Option() { Name = "-o", Description = "oo osdf osdfs", IsRequired = true, Value = "ovalue" },
                new Option() { Name = "-url", Description = "Ini adalah URL nya", IsRequired = false, Value = "http://google.com" },
                new Option() { Name = "-From", Description = "From tangggal", IsRequired = false, Value = "20180212132031" }
            };

            OptionDictionary optionDictionary = new OptionDictionary() { Name = "Options", Options = options };

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof (OptionDictionary));

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ser.WriteObject(ms, optionDictionary);
            

            //using ( var jsonrw = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8))
            //{
            //    ser.WriteObject(ms, optionDictionary);
            //    jsonrw.Flush();
            //    Console.WriteLine("ms: " + ms.Length);

            //    //Console.WriteLine("aaa: " + rd.CurrentEncoding.GetString(ms.ToArray()));

            //}

            System.IO.StreamReader rd = new System.IO.StreamReader(ms, Encoding.UTF8);
            ms.Position = 0;
            Console.WriteLine("asd: " + rd.ReadToEnd());


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
            if (args.Length == 1)
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                
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
    class Option 
    {
        [DataMember(Name = "Name", Order = 0)]
        public string Name { get; set; }

        [DataMember(Name = "Value", Order = 1)]
        public string Value { get; set; }

        [DataMember(Name = "Description", Order = 2)]
        public string Description { get; set; }

        [DataMember(Name = "IsRequired", Order = 3)]
        public bool IsRequired { get; set; }

        [DataMember(Name = "Example", Order = 4)]
        public string Example { get; set; }
    }

    [DataContract(Name ="OptionData")]
    class OptionDictionary
    {
        [DataMember(Name = "OptionName", Order = 0)]
        public string Name { get; set; }
        [DataMember(Name = "Options", Order = 1)]
        public IList<Option> Options { get; set; }

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

