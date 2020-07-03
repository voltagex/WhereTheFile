# WhereTheFile
Hopefully a multi-machine file indexer for home users

# What
* This will be a tool that scans local filesystems and dumps the info to a database

# Why

* I lost a backup file on a drive somewhere and wanted to find it again

* https://github.com/shirosaidev/diskover looks cool but has too many moving parts (and didn't support ElasticSearch 7 when I looked at it)

# How
* For now, a .NET Core app that enumerates all files on (each?) drive, and then dumps them to a SQLite database. From there, files with the same filesize can be grouped and hashed if needed

# Challenges

* Drive identification - getting the drive serial on Windows is a real pain, https://github.com/unknownv2/CloudKit/blob/master/CloudKit.SteamKit/Util/Win32Helpers.cs one day. For the moment, I'm not identifying drives at all - just scanning and dumping filenames into a database

# What works now

* Scanning for files across multiple drives and paths, in Windows and Linux

* Searching in SQLite for scanned files

* Generation of statistics (largest file, top 10 largest files)

# To do

* Searching inside archives / Macrium Reflect backup files

* Hashing - originally I had planned to use XXHash, but this seems to cause problems on ARM devices (say, a NAS you might want to run this on)
  The other thing with hashing is it makes Windows Defender sad. I think the best way forward is to only hash on filesize + filetype collisions, and put an upper + lower limit on filesizes that'll be considered for hashing. Currently I'm thinking of just hashing files that are found as duplicates in the first pass (filesize+filename are the same). Could also experiment with only hashing the last megabyte of a file or something like that.

* Multiple machine support - probably just adding a database column to track which hostname the scan came from

* Better / different UIs - at the moment it's an old school console app, which I like but it might also need non-interactive modes and something like a web UI. https://github.com/TomaszRewak/C-sharp-console-gui-framework is really tempting.

* Separate out the scanning and UI code - e.g. anything that uses Console.Write must go

* Ability to update database without doing a full rescan

* The find duplicates function doesn't take filename into consideration, see [#1](https://github.com/voltagex/WhereTheFile/issues/1)
