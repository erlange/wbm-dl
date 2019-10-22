> [!WARNING] :warning: This `README` file is currently under construction.  It may change over times.
# Wayback Machine Downloader
A C# implementation of wayback machine downloader.  Download an entire archived website from the [Internet Archive Wayback Machine](http://web.archive.org/).

## Requirements
1. .NET Framework 4.0 
2. This tool uses [Command Line Parser 2.6.0](http://github.com/commandlineparser/commandline) library

## Basic Usage
Run `wbm-dl` followed by the website name, for example `http://yoursite.com` :
```
wbm-dl http://yoursite.com
```
or just
```
wbm-dl yoursite.com
```

Issuing the above command will download the website to the `./websites/yoursite.com` directory.

## Advanced Usage
The additional parameter list will display when run without any parameters:
```
Wayback Downloader.NET Console for Windows 1.0.0.0
http://erlange.github.com 

  -o, --out      Output/destination directory

  -f, --from     From timestamp. Limits the archived result SINCE this timestamp.
                 Use 1 to 14 digit with the format: yyyyMMddhhmmss
                 If omitted, retrieves results since the earliest timestamp available.

  -t, --to       To timestamp. Limits the archived result  UNTIL this timestamps.
                 Use 1 to 14 digit with the format: yyyyMMddhhmmss
                 If omitted, retrieves results until the latest timestamp available.

  -l, --limit    Limits the first N or the last N results. Negative number limits the last N results.

  -a             All timestamps. Retrieves snapshots for all timestamps.

  -c, --count    (Default: 1) Number of concurrent processes.
                 Can speed up the process but requires more memory.

  -A, --All      Retrieves snapshots for all HTTP status codes.
                 If omitted only retrieves the status code of 200

  -X, --exact    Downloads only the url provided and not the full site.

  -L, --list     Displays only the list in a JSON format with the archived timestamps, does not download anything

  --help         Display this help screen.

  --version      Display version information.
```
## Specifying the URL to Download
You must supply a valid URL address to download.
### Examples
Some valid examples URL are shown below:
```
wbm-dl yoursite.com 
```
```
wbm-dl http://yoursite.com 
```
```
wbm-dl https://yoursite.com 
```
 

## Output/Destination Directory
```
-o, --out      Output/destination directory
```
Optional.  The `-o` or `--out` option specifies the directory in which you want the websites to be saved.   A sub-directory called `/websites` will be created under the specified directory.

### Examples
```
wbm-dl yoursite.com -o c:/download
```
Will download to `c:/download/websites` directory.
```
wbm-dl yoursite.com -o ./myFolder/web
```
Will download to `[Current Directory]/myFolder/web/websites` directory.

### Log Files
Upon completion, a `/logs` directory containing a log file will be created under the `/websites` directory.
The JSON-formatted log file contains completion status of each downloaded item.  If errors occured the log files can further be examined to accommodate manual download with the source URL for each item.

## From Timestamp
```
-f, --from     From timestamp. 
```
Optional. You can limit the result by specifying **the earliest** timestamp in the *yyyyMMddhhmmss* format. This parameter is inclusive, in which the value is included to the result. The Wayback Machine Downloader will only fetch the file versions **on** or **after** the timestamp specified.

### Examples
```
wbm-dl yoursite.com -o c:/download -f 20171101210000
```
Will download only the file versions **on** or **after** *November 01, 2017* at *21:00:00*
```
wbm-dl yoursite.com -o c:/download -f 2017
```
Will download only the file versions **in** or **after** the year of *2017*
```
wbm-dl yoursite.com -o c:/download -f 201701
```
Will download only the file versions **in** or **after** *January 2017*

## To Timestamp
```
-t, --to     To timestamp. 
```
Optional. You can limit the result by specifying **the latest** timestamp in the *yyyyMMddhhmmss* format. This parameter is inclusive, in which the value is included to the result. The Wayback Machine Downloader will only fetch the file versions **on** or **before** the timestamp specified.

### Examples
```
wbm-dl yoursite.com -o c:/download -t 20180915220000
```
Will download only the file versions **on** or **before** *September 15, 2018* at *22:00:00*
```
wbm-dl yoursite.com -o c:/download -t 2018
```
Will download only the file versions **in** or **before** the year of *2018*
```
wbm-dl yoursite.com -o c:/download -t 201804
```
Will download only the file versions **in** or **before** *April 2018*.

## Limiting Between Two Timestamps
You can combine both `-f` and `-t` parameters to limit the result between two timestamps.  Since both parameters are inclusive, the from and to parameter values are included to the result.

### Examples
```
wbm-dl yoursite.com -o c:/download -f 20171101210000 -t 20180915220000
```
Will download only the file versions **between** *November 01, 2017 21:00:00* **and** *September 15, 2018 22:00:00*.

```
wbm-dl yoursite.com -o c:/download  -f 2017 -t 201804
```
Will download only the file versions **between** the year of *2017* **and** *April 2018*.

```
wbm-dl yoursite.com -o c:/download  -f 2017 -t 2017
```
Will download only the file versions **in** the year of *2017*.
