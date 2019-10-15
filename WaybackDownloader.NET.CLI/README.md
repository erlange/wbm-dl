> [!WARNING]
> This `README` file is currently under construction.  It may change over times.
# Wayback Machine Downloader
A C# implementation of wayback machine downloader.  Download an entire website from the Internet Archive Wayback Machine.

## Requirements
1. .NET Framework 4.0 
2. This tool uses [Command Line Parser 2.6.0](http://github.com/commandlineparser/commandline) library

## Basic Usage
Run `wbm-dl` with `-u` parameter followed by the website name, for example `http://yoursite.com` :
```
wbm-dl -u yoursite.com
```
Issuing the above command will download the website to the `./yoursite.com` directory.

## Advanced Usage

```
Wayback Downloader.NET Console for Windows 1.0.0.0
http://erlange.github.com 

  -u, --url      Required. The URL of the archived web site

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