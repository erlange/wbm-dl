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

  -e, --exact    Downloads only the url provided and not the full site.

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

## Downloading Snapshots for All Timestamps
By default, your files are archived in different snapshot for each timestamp.  You can specify the `-a` parameter to download all snapshot versions for each of your file.

The `-a` parameter is not to be confused with `-A` parameter, although they both can also be used in conjunction.
```
-a             All timestamps. Retrieves snapshots for all timestamps.
```
Optional.  The  `-a` parameter will download the file versions all timestamps. The timestamp of each snapshot will be used as a directory.
```
wbm-dl yoursite.com -o c:/download  -a
```
Will download to the directory structure below:
```
c:/download/websites/yoursite.com/20180820202452/index.html
c:/download/websites/yoursite.com/20181019232937/index.html
c:/download/websites/yoursite.com/20190305194903/assets/logo.png
```

If this parameter is omitted the Wayback Machine Downloader will only download the latest snapshot version of each unique item.

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
wbm-dl yoursite.com -o c:/download -f 201707
```
Will download only the file versions **in** or **after** *July 2017*

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

## Limiting The Number of Files to Download
```
-l, --limit    Limits the first N or the last N results. Negative number limits the last N results.
```
Optional. You can limit the number of files to download by specifying the `-l` parameter followed by a positive or negative integer value.


This `-l` parameter is not to be confused with the `-L` parameter. They both can be used in conjunction though.


### Examples
```
wbm-dl yoursite.com -o c:/download -l 50
```
Will download only 50 files **since** the earliest timestamp. The earliest version is included.


```
wbm-dl yoursite.com -o c:/download -l -25
```
Will download only 25 files **on** and **before** the latest timestamp. The latest version is included.


## Exact URL
```
-e, --exact    Downloads only the url provided and not the full site.
```
Optional. Instead of downloading the entire websites you can use this `-e` flag to download only the file you specify as the URL.
### Examples
```
wbm-dl yoursite.com -o c:/download -e
```
Will download only the homepage html file of yoursite.com


## Download All HTTP Status Codes
```
-A, --All      Retrieves snapshots for all HTTP status codes.
               If omitted only retrieves the status code of 200
```
Optional. By default, the Wayback Machine Downloader will download the files responding only to the HTTP status code of 200 (HTTP status code for OK).  This `-A` flag will download responses with all HTTP status codes, such as 30x, 40x and 50x.
### Examples
```
wbm-dl yoursite.com -o c:/download -A
```



## Download Multiple Files at a Time
```
-c, --count    (Default: 1) Number of concurrent processes.
               Can speed up the process but requires more memory.
```
Optional. You can speed up the download process  significantly by specifying an (integer) number of concurrency with the `-c` parameter.


### Examples
```
wbm-dl yoursite.com -o c:/download -c 50
```
Will download maximum 50 files at a time.


## Displaying the File List Without Downloading
```
-L, --list     Displays only the list in a JSON format with the archived timestamps, does not download anything
```
Optional.  This option will only display the file list in JSON format and save it to the `/logs` directory.  It won't download anything else.


### Examples
```
wbm-dl yoursite.com -o c:/download -L
```
This will only display the file list on screen and save the list in the `c:/download/logs` directory.