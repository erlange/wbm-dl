// Copyright © 2018 eri.airlangga@gmail.com
//
// Do what you want with this program
// as long as the first line above is kept intact
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Json;

namespace com.erlange.wbmdl
{
    public static class ArgsExtensions
    {
        public static bool IsValidURL(this string URL)
        {
            string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";

            //https://regexr.com/3e6m0
            //string Pattern = @"(http(s)?:\/\/.)?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";

            Regex Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Rgx.IsMatch(System.Uri.UnescapeDataString(URL));
        }

        public static bool IsMatch(this string url, string pattern)
        {
            Regex rgx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            //string fileTypePattern = @"^.*\.(jpg|JPG|gif|GIF|doc|DOC|pdf|PDF|)$";
            return rgx.IsMatch(url);
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
        public static string ToJson(this List<Log> value)
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Log>));
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
        public static string ToCsv(this List<Log> value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Log a in value)
            {
                builder.Append(a.Num);
                builder.Append(',');
                builder.Append(a.Original);
                builder.Append(',');
                builder.Append(a.Source);
                builder.Append(',');
                builder.Append(a.Status);
                builder.Append(',');
                builder.Append(a.ErrorMsg);
                builder.Append(',');
                builder.Append(a.Target);
                builder.Append(',');
                builder.Append(a.Time);
                builder.Append(',');
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
