# USN Journal Viewer CLI

The API was adopted from [StCroixSkipper's NTFS USN Journal Explorer](https://www.donationcoder.com/forum/index.php?topic=22695.0) and was Upgraded to .NET Framework 4.0 to support modern Windows Operating Systems.

Note: Some methods were changed to their modern implementation to suit the new version of .NET Framework.

This API allows to create, view, change or delete [USN Journals](https://en.wikipedia.org/wiki/USN_Journal) from NTFS Partitions. This API works effectively on C drives, but fails to work on other partitions.

The USN Journal displays the USN Reason Codes from the files that has been modified. Using this USN Reason Codes it can be determined if this file can be backed up or not.

The CLI implementation only includes the following operations:
- Listing the drives
  ```
  The GetDrives method from DriveInfo retrieves the NTFS partitions
  DriveInfo[] volumes = DriveInfo.GetDrives();
  ```
- Creation of the USN Journal

   The NtfsUsnJournal constructor is used in specifying the drive where the journal is selected to be created where choice contains the array index.
   ```
   _usnJournal = new NtfsUsnJournal(volumes[choice]);
   ```
   The CreateUsnJournal actually creates the USN Journal, the parameters mentioned are the maxSize and the allocationData (the sizes mentioned in the snippet below are adopted from the actual implementation from StCroixSkipper's)
   ```
   NtfsUsnJournal.UsnJournalReturnCode rtn = _usnJournal.CreateUsnJournal(1000 * 1024, 16 * 1024);
   ```
- Querying the USN Journal state 
   We can pass the _usnJournal object as a reference to the Win32 Api Class(Win32Api.cs) to retrieve the state of the USN Journal, as this API uses P/Invoke to translate between native component and the C# code (managed). We are specfically performing the communication using`System.Runtime.InteropServices;`
   ```
    Win32Api.USN_JOURNAL_DATA journalState = new Win32Api.USN_JOURNAL_DATA();
    NtfsUsnJournal.UsnJournalReturnCode returnCode = _usnJournal.GetUsnJournalState(ref journalState);
   ```
- Updating the USN Journal by Listing Files and Folders within the directory
- Deleting the Journal
    ```
    rtn = _usnJournal.DeleteUsnJournal(_usnCurrentJournalState);
    ```

### Sample Output -
```
Listing Drives:
1. C:\
2. D:\
Please enter the volume to be chosen:
1
Selecting the volume: C:\
GetVolumeSerialNumber() function entered for drive 'C:\'
Creating USN Journal for the drive C:\...
USN Journal Created Successfully:  USN_JOURNAL_SUCCESS

Querying the USN Journal State..
==============================================================
Journal ID: 1D86DA1708DC4AA
First USN: 0
Next USN: 17998

Lowest Valid USN: 0
Max USN: 7FFFFFFFFFFF0000
Max Size: 2000000
Allocation Delta: 800000
==============================================================

Listing Files...
Listing Folders...
Current File Count for C:\: 263722
Current Folder Count for C:\ : 95313
System.Collections.Generic.List`1[PInvoke.Win32Api+UsnEntry]
USN Entries Count 27
Listing latest file(s) with changes
Current File Changes Count: 27

File Name: 9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-0.bin

Reason Codes:
-------------------------
SECURITY CHANGE
-------------------------

File Name: 9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-1.bin
File Attributes:
File Path: \ProgramData\Microsoft\Windows Defender\Scans\History\CacheManager\9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-1.bin
File Length:   319488 (bytes)
Creation Time: 22-05-2022 - 11:33
Last Modify:   22-05-2022 - 11:33
Last Access:   22-05-2022 - 11:33

Reason Codes:
-------------------------
FILE CREATE
-------------------------

File Name: 9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-1.bin
File Attributes:
File Path: \ProgramData\Microsoft\Windows Defender\Scans\History\CacheManager\9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-1.bin
File Length:   319488 (bytes)
Creation Time: 22-05-2022 - 11:33
Last Modify:   22-05-2022 - 11:33
Last Access:   22-05-2022 - 11:33

Reason Codes:
-------------------------
DATA EXTEND
FILE CREATE
-------------------------

File Name: 9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-1.bin
File Attributes:
File Path: \ProgramData\Microsoft\Windows Defender\Scans\History\CacheManager\9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-1.bin
File Length:   319488 (bytes)
Creation Time: 22-05-2022 - 11:33
Last Modify:   22-05-2022 - 11:33
Last Access:   22-05-2022 - 11:33

Reason Codes:
-------------------------
DATA EXTEND
FILE CREATE
SECURITY CHANGE
-------------------------

File Name: 9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-0.bin

Reason Codes:
-------------------------
FILE DELETE
SECURITY CHANGE
CLOSE
-------------------------

File Name: 9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-1.bin
File Attributes:
File Path: \ProgramData\Microsoft\Windows Defender\Scans\History\CacheManager\9D4902A6-1BB2-4AB1-BAEE-EB0E910CCA47-1.bin
File Length:   319488 (bytes)
Creation Time: 22-05-2022 - 11:33
Last Modify:   22-05-2022 - 11:33
Last Access:   22-05-2022 - 11:33

Reason Codes:
-------------------------
DATA OVERWRITE
DATA EXTEND
FILE CREATE
SECURITY CHANGE
-------------------------

File Name: USNJOURNALCLI.EXE-641C599C.pf
File Attributes:
File Path: \Windows\Prefetch\USNJOURNALCLI.EXE-641C599C.pf
File Length:   7258 (bytes)
Creation Time: 22-05-2022 - 11:17
Last Modify:   22-05-2022 - 11:33
Last Access:   22-05-2022 - 11:33

Reason Codes:
-------------------------
DATA EXTEND
DATA TRUNCATION
-------------------------

File Name: USNJOURNALCLI.EXE-641C599C.pf
File Attributes:
File Path: \Windows\Prefetch\USNJOURNALCLI.EXE-641C599C.pf
File Length:   7258 (bytes)
Creation Time: 22-05-2022 - 11:17
Last Modify:   22-05-2022 - 11:33
Last Access:   22-05-2022 - 11:33

Reason Codes:
-------------------------
DATA EXTEND
DATA TRUNCATION
CLOSE
-------------------------

File Name: MSVSMON.EXE-9A249E80.pf
File Attributes:
File Path: \Windows\Prefetch\MSVSMON.EXE-9A249E80.pf
File Length:   17213 (bytes)
Creation Time: 22-05-2022 - 11:17
Last Modify:   22-05-2022 - 11:33
Last Access:   22-05-2022 - 11:33

Reason Codes:
-------------------------
DATA EXTEND
DATA TRUNCATION
-------------------------

File Name: MSVSMON.EXE-9A249E80.pf
File Attributes:
File Path: \Windows\Prefetch\MSVSMON.EXE-9A249E80.pf
File Length:   17213 (bytes)
Creation Time: 22-05-2022 - 11:17
Last Modify:   22-05-2022 - 11:33
Last Access:   22-05-2022 - 11:33
```


### Final Notes:
To know the changes that is occuring to the files throughout the period of the time. A service needs to be created that maintains the records of the files (by creating the Journal) and continously maintains the state (by Updating by every minute or an hour or so).



## Dependencies
- .NET Framework 4.0
- Win32 APIs 
- PInvoke 
