> [!WARNING] :warning:
> This `README` file is currently under construction.  It may change over times.
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

## Output/Destination Directory
```
  -o, --out      Output/destination directory
```
The '-o' or `--out` option specifies the directory in which you want the websites to be saved.  A sub-directory called `/websites` will be created under the specified directory.

### Examples
```
    wbm-dl yoursite.com -o c:/download
```

```
    wbm-dl yoursite.com -o ./myFolder/web
```

