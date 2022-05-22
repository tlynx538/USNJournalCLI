using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsnJournal;
using PInvoke;
using System.IO;
namespace USNJournalCLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Variables
            // creating empty UsnJournal object
            NtfsUsnJournal _usnJournal = null;
            // variables to store usn state and entries
            Win32Api.USN_JOURNAL_DATA _usnCurrentJournalState = new Win32Api.USN_JOURNAL_DATA();
            Win32Api.USN_JOURNAL_DATA newUsnState;
            List<Win32Api.UsnEntry> usnEntries;
            //bool flag = false;
            // USN Reasons
            uint reasonMask = Win32Api.USN_REASON_DATA_OVERWRITE |
                Win32Api.USN_REASON_DATA_EXTEND |
                Win32Api.USN_REASON_NAMED_DATA_OVERWRITE |
                Win32Api.USN_REASON_NAMED_DATA_TRUNCATION |
                Win32Api.USN_REASON_FILE_CREATE |
                Win32Api.USN_REASON_FILE_DELETE |
                Win32Api.USN_REASON_EA_CHANGE |
                Win32Api.USN_REASON_SECURITY_CHANGE |
                Win32Api.USN_REASON_RENAME_OLD_NAME |
                Win32Api.USN_REASON_RENAME_NEW_NAME |
                Win32Api.USN_REASON_INDEXABLE_CHANGE |
                Win32Api.USN_REASON_BASIC_INFO_CHANGE |
                Win32Api.USN_REASON_HARD_LINK_CHANGE |
                Win32Api.USN_REASON_COMPRESSION_CHANGE |
                Win32Api.USN_REASON_ENCRYPTION_CHANGE |
                Win32Api.USN_REASON_OBJECT_ID_CHANGE |
                Win32Api.USN_REASON_REPARSE_POINT_CHANGE |
                Win32Api.USN_REASON_STREAM_CHANGE |
                Win32Api.USN_REASON_CLOSE;

            // Listing NTFS Drives
            Console.WriteLine("Listing Drives: ");
            DriveInfo[] volumes = DriveInfo.GetDrives();
            for(int i=0;i<volumes.Length;i++)
            {
                if (volumes[i].IsReady && 0 == string.Compare(volumes[i].DriveFormat, "ntfs", true))
                {
                    Console.WriteLine("{0}. {1}",i+1,volumes[i].Name);
                }
            }
            try
            {
                // Selecting Volume
                Console.WriteLine("Please enter the volume to be chosen: ");
                int choice = Convert.ToInt32(Console.ReadLine()) - 1;
                Console.WriteLine("Selecting the volume: {0}", volumes[choice]);
                //Console.WriteLine("Please enter the time for listing the modified files: ");
                //string inputTime = Console.ReadLine();
                try
                {
                    _usnJournal = new NtfsUsnJournal(volumes[choice]);
                    // Creating USN Journal 
                    Console.WriteLine("Creating USN Journal for the drive {0}...", volumes[choice].Name);
                    NtfsUsnJournal.UsnJournalReturnCode rtn = _usnJournal.CreateUsnJournal(1000 * 1024, 16 * 1024);
                    if (rtn == NtfsUsnJournal.UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
                    {
                        Console.WriteLine("USN Journal Created Successfully:  {0}",rtn.ToString());
                        // Querying the USN Journal State
                        Console.WriteLine();
                        Console.WriteLine("Querying the USN Journal State.. ");
                        Win32Api.USN_JOURNAL_DATA journalState = new Win32Api.USN_JOURNAL_DATA();
                        NtfsUsnJournal.UsnJournalReturnCode returnCode = _usnJournal.GetUsnJournalState(ref journalState);
                        if (returnCode == NtfsUsnJournal.UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
                        {
                            // Print Journal State
                            Console.WriteLine("==============================================================");
                            Console.WriteLine("Journal ID: {0}", journalState.UsnJournalID.ToString("X"));
                            Console.WriteLine("First USN: {0}", journalState.FirstUsn.ToString("X"));
                            Console.WriteLine("Next USN: {0}", journalState.NextUsn.ToString("X"));
                            Console.WriteLine();
                            Console.WriteLine("Lowest Valid USN: {0}", journalState.LowestValidUsn.ToString("X"));
                            Console.WriteLine("Max USN: {0}", journalState.MaxUsn.ToString("X"));
                            Console.WriteLine("Max Size: {0}", journalState.MaximumSize.ToString("X"));
                            Console.WriteLine("Allocation Delta: {0}", journalState.AllocationDelta.ToString("X"));
                            Console.WriteLine("==============================================================");
                            // Saving USN State 
                            _usnCurrentJournalState = journalState;
                            // List files
                            Console.WriteLine();
                            Console.WriteLine("Listing Files...");
                            // Regex pattern here
                            string fileFilter = "*";
                            // Saving USN State 
                            _usnCurrentJournalState = journalState;
                            List<Win32Api.UsnEntry> fileList;
                            List<Win32Api.UsnEntry> folders;
                            NtfsUsnJournal.UsnJournalReturnCode rtnFileCode = _usnJournal.GetFilesMatchingFilter(fileFilter, out fileList);
                            // Saving USN State 
                            _usnCurrentJournalState = journalState;
                            // List folders
                            Console.WriteLine("Listing Folders...");
                            NtfsUsnJournal.UsnJournalReturnCode rtnCode1 = _usnJournal.GetNtfsVolumeFolders(out folders);
                            // Saving USN State 
                            _usnCurrentJournalState = journalState;
                            // name of the file to be searched
                            // folders for finding folder
                            // fileList for finding files
                            //for (int i = 0; i < fileList.Count; i++)
                            //{
                            //    //if (folders[i].Name == "test-folder")
                            //    //    Console.WriteLine("Folder found");
                            //    Console.WriteLine(fileList[i].Name);
                            //    //Console.WriteLine(fileList[i].Reason);
                            //}
                            Console.WriteLine("Current File Count for {0}: {1}", volumes[choice].Name, fileList.Count);
                            Console.WriteLine("Current Folder Count for {0} : {1}", volumes[choice].Name, folders.Count);
                            //Viewing USN Changes
                            if (journalState.UsnJournalID != 0)
                            {
                                NtfsUsnJournal.UsnJournalReturnCode rtnCode = _usnJournal.GetUsnJournalEntries(_usnCurrentJournalState, reasonMask, out usnEntries, out newUsnState);
                                // Saving USN State 
                                _usnCurrentJournalState = journalState;
                                Console.WriteLine(usnEntries);
                                Console.WriteLine("USN Entries Count {0}",usnEntries.Count);
                                
                                if (usnEntries.Count > 0)
                                {
                                    Console.WriteLine("Listing latest file(s) with changes");
                                    Console.WriteLine("Current File Changes Count: {0}", usnEntries.Count);
                                    Console.WriteLine();
                                    for (int i = 0; i < usnEntries.Count; i++)
                                    {
                                        Console.WriteLine("File Name: {0}", usnEntries[i].Name);
                                        
                                        if (usnEntries[i].IsFolder)
                                        {
                                            Console.WriteLine("{0} is a folder", usnEntries[i].Name);
                                            Console.ReadLine();
                                        }
                                        if (usnEntries[i].IsFile)
                                        {
                                            string path;
                                            NtfsUsnJournal.UsnJournalReturnCode usnRtnCode = _usnJournal.GetPathFromFileReference(usnEntries[i].ParentFileReferenceNumber, out path);
                                            string fullPath = System.IO.Path.Combine(path, usnEntries[i].Name);

                                            if (File.Exists(fullPath))
                                            {
                                                FileInfo fi = new FileInfo(fullPath);
                                                //if (fi.LastWriteTime.ToShortTimeString() == inputTime)
                                                //{
                                                //flag = true;
                                                Console.WriteLine("File Attributes: ");
                                                Console.WriteLine("File Path: {0}", fullPath);
                                                Console.WriteLine("File Length:   {0} (bytes)", fi.Length);
                                                Console.WriteLine("Creation Time: {0} - {1}", fi.CreationTime.ToShortDateString(), fi.CreationTime.ToShortTimeString());
                                                Console.WriteLine("Last Modify:   {0} - {1}", fi.LastWriteTime.ToShortDateString(), fi.LastWriteTime.ToShortTimeString());
                                                Console.WriteLine("Last Access:   {0} - {1}", fi.LastAccessTime.ToShortDateString(), fi.LastAccessTime.ToShortTimeString());
                                                //}
                                                }
                                            }
                                        Console.WriteLine();
                                                //if(flag)
                                                //{
                                                Console.WriteLine("Reason Codes: ");
                                                Console.WriteLine("-------------------------");
                                                uint value = usnEntries[i].Reason & Win32Api.USN_REASON_DATA_OVERWRITE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("DATA OVERWRITE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_DATA_EXTEND;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("DATA EXTEND");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_DATA_TRUNCATION;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("DATA TRUNCATION");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_NAMED_DATA_OVERWRITE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("NAMED DATA OVERWRITE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_NAMED_DATA_EXTEND;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("NAMED DATA EXTEND");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_NAMED_DATA_TRUNCATION;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("NAMED DATA TRUNCATION");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_FILE_CREATE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("FILE CREATE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_FILE_DELETE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("FILE DELETE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_EA_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("EA CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_SECURITY_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("SECURITY CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_RENAME_OLD_NAME;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("RENAME OLD NAME");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_RENAME_NEW_NAME;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("RENAME NEW NAME");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_INDEXABLE_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("INDEXABLE CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_BASIC_INFO_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("BASIC INFO CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_HARD_LINK_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("HARD LINK CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_COMPRESSION_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("COMPRESSION CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_ENCRYPTION_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("ENCRYPTION CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_OBJECT_ID_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("OBJECT ID CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_REPARSE_POINT_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("REPARSE POINT CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_STREAM_CHANGE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("STREAM CHANGE");
                                                }
                                                value = usnEntries[i].Reason & Win32Api.USN_REASON_CLOSE;
                                                if (0 != value)
                                                {
                                                    Console.WriteLine("CLOSE");
                                                }
                                                Console.WriteLine("-------------------------");
                                                Console.WriteLine();
                                                //}
                                    }
                                    // End of program
                                    // Deleting USN Journal
                                    rtn = _usnJournal.DeleteUsnJournal(_usnCurrentJournalState);
                                    if (rtn == NtfsUsnJournal.UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
                                    {
                                        Console.WriteLine("USN Journal Deleted Successfully...");
                                    }
                                    Console.WriteLine("Waiting for termination");
                                    Console.ReadLine();

                                }
                                else
                                    Console.WriteLine("No Journal Entries Found");
                            }

                        }
                        else
                        {
                            Console.WriteLine("Querying unsuccessful.");
                        }   

                    }
                    else
                    {
                        Console.WriteLine("An Error Occured: {0}", rtn.ToString());
                    }
                }
                catch(Exception e)
                {
                    if(e.Message.Contains("Access is Denied"))
                    {
                        Console.WriteLine("Access Denied, please retry with adminstrative privileges");
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Invalid Input Given \n {0}",e);
            }
        }
    }
    
}