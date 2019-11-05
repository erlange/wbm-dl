// Copyright © 2018 eri.airlangga@gmail.com
//
// Do what you want with this program
// as long as the first line above is kept intact
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.erlange.wbmdl
{
    public class Archive
    {
        public string UrlKey { get; set; }
        public long Timestamp { get; set; }
        public string Original { get; set; }
        public string Digest { get; set; }
        public string UrlId { get; set; }
        public string MimeType { get; set; }
        public string StatusCode { get; set; }
        public long Length { get; set; }
        public string Filename { get; set; }
        public string LocalPath { get; set; }
        public string LocalPathTimestamp { get; set; }
    }

}
