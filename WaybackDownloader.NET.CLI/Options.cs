// Copyright © 2018 eri.airlangga@gmail.com
//
// Do what you want with this program
// as long as the first line above is kept intact
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace com.erlange.wbmdl
{
    class Options
    {

        [Value(0, HelpText = "The URL of the archived web site", MetaName = "url", Required = true)]
        public string Url { get; set; }

        [Option('o', "out", HelpText = "Output/destination directory")]
        public string OutputDir { get; set; }

        [Option('f', "from", HelpText = "From timestamp. Limits the archived result SINCE this timestamp. \nUse 1 to 14 digit with the format: yyyyMMddhhmmss \nIf omitted, retrieves results since the earliest timestamp available. ")]
        public string From { get; set; }

        [Option('t', "to", HelpText = "To timestamp. Limits the archived result  UNTIL this timestamps. \nUse 1 to 14 digit with the format: yyyyMMddhhmmss \nIf omitted, retrieves results until the latest timestamp available. ")]
        public string To { get; set; }

        ////Experimental Collapse by field
        //[Option(shortName: 'C', longName: "Collapse", Default = 14,HelpText = "Collapse by timestamp's number of digit", Hidden = true)]
        //public int Collapse { get; set; }

        [Option('l', "limit", HelpText = "Limits the first N or the last N results. Negative number limits the last N results.")]
        public string Limit { get; set; }

        [Option('a', HelpText = "All timestamps. Retrieves snapshots for all timestamps.")]
        public bool AllTimestamps { get; set; }

        [Option(shortName: 'c', longName: "count", Default = 1, HelpText = "Number of concurrent processes. \nCan speed up the process but requires more memory.")]
        public int Threadcount { get; set; }

        [Option('A', "All", HelpText = "Retrieves snapshots for all HTTP status codes. \nIf omitted only retrieves the status code of 200.")]
        public bool AllHttpStatus { get; set; }

        [Option('e', "exact", HelpText = "Downloads only the url provided and not the full site.")]
        public bool ExactUrl { get; set; }

        [Option('O', "Only", HelpText = "Restrict downloading to urls that match this filter.")]
        public string OnlyFilter { get; set; }

        [Option('X', "eXclude", HelpText = "Skip downloading of urls that match this filter.")]
        public string ExcludeFilter { get; set; }

        [Option('L', "List", HelpText = "Displays only the list in a JSON format with the archived timestamps, does not download anything.")]
        public bool ListOnly { get; set; }

    }
}
