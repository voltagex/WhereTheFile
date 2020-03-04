# WhereTheFile
Hopefully a multi-machine file indexer for home users

# What
* This will be a tool that scans local filesystems, computes a fast hash of each file and dumps the info to a database

# Why
* I lost a backup file on a drive somewhere and want to find it again

# How
* For now, a .NET Core app that enumerates all files on (each?) drive, and then dumps them to a SQLite database. From there, files with the same filesize can be grouped and hashed if needed

# Challenges

* Drive identification - getting the drive serial on Windows is a real pain, https://github.com/unknownv2/CloudKit/blob/master/CloudKit.SteamKit/Util/Win32Helpers.cs one day. For the moment, I'm not identifying drives at all - just scanning and dumping filenames into a database

* RAM usage - I've seen upwards of 2GB of RAM usage for a complete scan.

# To do

* Hashing - originally I had planned to use XXHash, but this seems to cause problems on ARM devices (say, a NAS you might want to run this on)
  The other thing with hashing is it makes Windows Defender sad. I think the best way forward is to only hash on filesize + filetype collisions, and put an upper + lower limit on filesizes that'll be considered for hashing. Currently I'm thinking of just hashing files that are found as duplicates in the first pass (filesize+filename are the same). Could also experiment with only hashing the last megabyte of a file or something like that.


* Linux support - There aren't too many changes to make but I wanted to get this up and running on my main Windows desktop first

* Multiple machine support - probably just adding a database column to track which hostname the scan came from

* Better / different UIs - at the moment it's an old school console app, which I like but it might also need non-interactive modes and something like a web UI. https://github.com/TomaszRewak/C-sharp-console-gui-framework is really tempting.

* Separate out the scanning and UI code

* Ability to update database without doing a full rescan

* The find duplicates function doesn't take filename into consideration, see [#1](https://github.com/voltagex/WhereTheFile/issues/1)
