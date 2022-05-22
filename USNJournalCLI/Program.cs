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
            NtfsUsnJournal _usnJournal = null;
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
                try
                {
                    _usnJournal = new NtfsUsnJournal(volumes[choice]);
                    // Creating USN Journal 
                    Console.WriteLine("Creating USN Journal for the drive {0}...", volumes[choice].Name);
                    NtfsUsnJournal.UsnJournalReturnCode rtn = _usnJournal.CreateUsnJournal(1000 * 1024, 16 * 1024);
                    if (rtn == NtfsUsnJournal.UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
                    {
                        Console.WriteLine("USN Journal Created Successfully:  {0}",rtn.ToString());
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
                            // Viewing USN Changes
                            //    if(journalState.UsnJournalID != 0)
                            //    {
                            //        // View Changes
                            //    }
                            //}
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
                        Console.WriteLine("Access Denied, please retry with adminstrative privielges");
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Invalid Input Given \n {0}",e);
            }

            // Selecting Volume
            // Console.Write("Select the Volume for querying USN journal");
            Console.WriteLine("Waiting for termination");
            Console.ReadLine();
        }
    }
    
}
