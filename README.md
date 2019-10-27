> :warning: This `README` file is currently under construction.  It may change over times.
# Wayback Machine Downloader
A C# implementation of wayback machine downloader.  Download an entire archived website from the [Internet Archive Wayback Machine](http://web.archive.org/).


##### Table of Contents


* [**Requirements**](#Requirements)
* [**Installation**](#Installation)
* [**Basic Usage**](#basic-usage)
  * [Specifying the URL to Download](#specifying-the-url-to-download)
  * [Output Directory](#output-directory)
* [**Advanced Usage**](#Advanced-Usage)
  * [Case Sensitive Parameter Names](#Case-Sensitive-Parameter-Names)
  * [Downloading Snapshots for All Timestamps](#Downloading-Snapshots-for-All-Timestamps)
  * [From Timestamp](#From-Timestamp)
  * [To Timestamp](#To-Timestamp)
  * [Limiting Between Two Timestamps](#Limiting-Between-Two-Timestamps)
  * [Limiting The Number of Files to Download](#Limiting-The-Number-of-Files-to-Download)
  * [Exact URL](#Exact-URL)
  * [Download All HTTP Status Codes](#Download-All-HTTP-Status-Codes)
  * [Download Multiple Files at a Time](#Download-Multiple-Files-at-a-Time)
  * [Displaying the File List Without Downloading](#Displaying-the-File-List-Without-Downloading)
* [**Log Files**](#Log-Files)
  * [Log File Metadata](#Log-File-Metadata)
* [**Contributing**](#Contributing)

## Requirements
1. .NET Framework 4.0 or newer.
2. For development use Visual Studio 2010 or newer.
3. This tool uses [Command Line Parser 2.6.0](http://github.com/commandlineparser/commandline) library.

## Installation
* Download the latest executable [here](https://github.com/erlange/wbm-dl/releases/download/v0.1/wbm-dl.zip) or chose from the available versions [here](https://github.com/erlange/wbm-dl/releases) 
* Download the [ZIP file](https://github.com/erlange/wbm-dl/archive/master.zip) or clone this repository:
    ```
    mkdir [your-directory]
    cd [your-directory]
    git clone https://github.com/erlange/wbm-dl.git
    cd wbm-dl
    dir
    ```
    Then you can open the `.sln` and build the solution file with Visual Studio.
* From Visual Studio, run this command from the Package Manager Console window:
    ```
    PM> Install-Package CommandLineParser -Version 2.6.0
    ```


## Basic Usage
At the very basic, you should run `wbm-dl` followed by the website name, for example `http://yoursite.com` :
```
wbm-dl http://yoursite.com
```
or just
```
wbm-dl yoursite.com
```

Issuing the above command will download the website to the `./websites/yoursite.com` directory.

## Specifying the URL to Download
You must supply a valid URL address to download.
### Examples
Some valid URL examples are shown below:
```
wbm-dl yoursite.com 
```
```
wbm-dl http://yoursite.com 
```
```
wbm-dl https://yoursite.com 
```


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

#### Case Sensitive Parameter Names
The Wayback Machine Downloader uses case sensitive parameter names, such as `-a` is different from `-A`. Careful consideration should be taken when typing such parameter names.
 

## Output Directory
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


## Downloading Snapshots for All Timestamps
By default, your files are archived in different snapshots for each timestamp.  You can specify the `-a` parameter to download all snapshot versions for each file.

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
Optional. You can limit the result by specifying **the earliest** timestamp in the *yyyyMMddhhmmss* format. This parameter is inclusive, in which the value is included to the result. The Wayback Machine Downloader will only fetch the snapshots since the timestamp specified.

### Examples
```
wbm-dl yoursite.com -o c:/download -f 20171101210000
```
Will download only the snapshots since *November 01, 2017* at *21:00:00*

```
wbm-dl yoursite.com -o c:/download -f 2017
```
Will download only the snapshots since the year of *2017*

```
wbm-dl yoursite.com -o c:/download -f 201707
```
Will download only the snapshots since *July 2017*

## To Timestamp
```
-t, --to     To timestamp. 
```
Optional. You can limit the result by specifying **the latest** timestamp in the *yyyyMMddhhmmss* format. This parameter is inclusive, in which the value is included to the result. The Wayback Machine Downloader will only fetch the snapshots until the timestamp specified.

### Examples
```
wbm-dl yoursite.com -o c:/download -t 20180915220000
```
Will download only the snapshots until *September 15, 2018* at *22:00:00*

```
wbm-dl yoursite.com -o c:/download -t 2018
```
Will download only the snapshots until the year of *2018*

```
wbm-dl yoursite.com -o c:/download -t 201804
```
Will download only the snapshots until *April 2018*.

## Limiting Between Two Timestamps
You can combine both `-f` and `-t` parameters to limit the result between two timestamps.  Since both parameters are inclusive, the from and to parameter values are included to the result.

### Examples
```
wbm-dl yoursite.com -o c:/download -f 20171101210000 -t 20180915220000
```
Will download only the snapshots since *November 01, 2017 21:00:00* until *September 15, 2018 22:00:00*.


```
wbm-dl yoursite.com -o c:/download  -f 2017 -t 201804
```
Will download only the snapshots since *2017* until *April 2018*.


```
wbm-dl yoursite.com -o c:/download  -f 2017 -t 2017
```
Will download only the snapshots during *2017*.

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
Will download only 50 files **since** the earliest timestamp. The earliest timestamp is included to the result.


```
wbm-dl yoursite.com -o c:/download -l -25
```
Will download only 25 files until the latest timestamp. The latest timestamp is included to the result.


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

## Download Only Specific File Type
```
-O, --Only
```
Optional. You can download only specific file type (e.g., .jpg, .pdf, .doc, etc) using `-O` parameter. This parameter needs a string or a regex.

### Examples
```
wbm-dl yoursite.com -o c:/download -O "^.*\.(jpg|gif|png|)$"
```
This will download only image files of .jpg, .gif and .png types.



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

## Log Files
Upon completion, a `/logs` directory containing a log file will be created under the `/websites` directory.
The JSON-formatted log file contains completion status of each downloaded item.  If errors occured the log files can further be examined to accommodate manual download with the source URL for each item.

This log file will not be generated when using the `-L` or `--list` flag.

The generated log filename will be `yoursite.com.log.json`

### Log File Metadata
The JSON-formatted log file contains metadata as follows:
* `ErrorMsg`    
    Contains the error message if error occured.
* `Num`    
    Line number.
* `Original`    
    Contains the original location of the item.
* `Source`    
    Contains the archived location of the item in the Internet Wayback Archive Machine.  You can use the value for manually downloading.
* `Status`    
    Contains the HTTP status code.  If flag `-A` is omitted and no error occured the value will be `200 (OK)`.  If this value is empty an error might have occured.  You can then consult the `ErrorMsg` to examine the error and use the `Source` to manually download the individual file.
* `Target`    
    Contains full path in the output directory where the file is saved.  If this value is empty an error might have occured.  You can then consult the `ErrorMsg` to examine the error and use the `Source` to manually download the individual file.
* `Time`    
    The time the `Source` responds to the request. The time is in `yyyyMMdd hh:mm:ss` .NET format and might not conform to the standard JSON datetime format.

## Contributing
Contributions are welcome.  Just pull an issue or pull request from GitHub.


