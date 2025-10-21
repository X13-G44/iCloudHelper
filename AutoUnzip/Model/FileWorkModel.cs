/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				06.10.2025
///
/// ////////////////////////////////////////////////////////////////////////
/// 
/// SPDX-License-Identifier: Apache-2.0
/// Copyright (c) 2025 Christian Harscher (alias X13-G44)
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the Apache License as
/// published by the Free Software Foundation, either version 2 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
/// Apache License for more details.
///
/// You should have received a copy of the Apache License
/// along with this program. If not, see <http://www.apache.org/licenses/LICENSE-2.0/>.
///      
/// ////////////////////////////////////////////////////////////////////////



using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;



namespace AutoUnzip.Model
{
    static class FileWorkModel
    {
        public enum FileProcessingCheckpoint
        {
            [System.ComponentModel.Description ("No action executed yet")]
            None = 0,

            [System.ComponentModel.Description ("Waiting for file handler of image archive file...")]
            WaitForFreeFileHandle,

            [System.ComponentModel.Description ("Checking folders...")]
            CheckFolders,

            [System.ComponentModel.Description ("Backup image archive file...")]
            BackupZipFile,

            [System.ComponentModel.Description ("Prepare temp folder...")]
            PrepareTempfolder,

            [System.ComponentModel.Description ("Extract image archive file...")]
            ExtractZipFile,

            [System.ComponentModel.Description ("Moving image files to extract file folder...")]
            MoveExtractFilesToTargetfolder,

            [System.ComponentModel.Description ("Remove temp folder...")]
            DeleteTempfolder,

            [System.ComponentModel.Description ("Clean up backup folder...")]
            CleanupBacklupFolder,

            [System.ComponentModel.Description ("Remove image archive file...")]
            DeleteZipFile,

            [System.ComponentModel.Description ("Done")]
            Finished,
        }



        public struct DoWorkResult
        {
            public bool WorkSuccess { get; private set; }

            public FileProcessingCheckpoint LastCheckpoint { get; private set; }

            public List<String> ExtractedFiles { get; private set; }

            public string ErrorMessage { get; private set; }



            public DoWorkResult (bool workSuccess, FileProcessingCheckpoint lastCheckpoint, List<String> extractedFiles, string errorMessage = "")
            {
                this.WorkSuccess = workSuccess;
                this.LastCheckpoint = lastCheckpoint;
                this.ExtractedFiles = extractedFiles;
                this.ErrorMessage = errorMessage;
            }
        }



        static public DoWorkResult DoWork (string srcArchiveFile)
        {
            FileProcessingCheckpoint lastCheckpoint = FileProcessingCheckpoint.None;
            string tempPath = String.Empty;
            List<String> extractedFiles = new List<string> ();


            try
            {
                lastCheckpoint = FileProcessingCheckpoint.WaitForFreeFileHandle;
                WaitForUnlockedFile (srcArchiveFile);

                lastCheckpoint = FileProcessingCheckpoint.CheckFolders;
                CheckFolder (true);

                lastCheckpoint = FileProcessingCheckpoint.BackupZipFile;
                MakeBackupFile (srcArchiveFile);

                lastCheckpoint = FileWorkModel.FileProcessingCheckpoint.PrepareTempfolder;
                tempPath = MakeTempFolder ();

                lastCheckpoint = FileProcessingCheckpoint.ExtractZipFile;
                ExtractZipFile (srcArchiveFile, tempPath);

                lastCheckpoint = FileProcessingCheckpoint.MoveExtractFilesToTargetfolder;
                extractedFiles = MoveExtractedFilesToTargetFolder (tempPath);

                lastCheckpoint = FileProcessingCheckpoint.DeleteTempfolder;
                DeleteTempFolder (tempPath);

                lastCheckpoint = FileProcessingCheckpoint.CleanupBacklupFolder;
                CleanupBackupFolder ();

                lastCheckpoint = FileProcessingCheckpoint.DeleteZipFile;
                DeleteZipFile (srcArchiveFile);

                lastCheckpoint = FileProcessingCheckpoint.Finished;

                return new DoWorkResult (true, lastCheckpoint, extractedFiles);
            }
            catch (Exception e)
            {
                string message = e.Message;


                if (e.InnerException != null)
                {
                    message += " (Inner Exception: " + e.InnerException.Message + ")";
                }

                return new DoWorkResult (false, lastCheckpoint, null, e.Message);
            }
        }



        static private void WaitForUnlockedFile (string srcArchiveFile)
        {
            // Wait until the files becomes unlocked.

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using (FileStream fs = File.Open (srcArchiveFile, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        break;
                    }
                }
                catch (IOException)
                {
                    Task.Delay (2500).Wait ();
                }
            }
        }



        static public bool CheckFolder (bool allowExceptionOnError)
        {
            if (Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath) == false)
            {
                if (allowExceptionOnError)
                {
                    throw new FileNotFoundException ("Extract directory \"" + ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath + "\" does not exists.");
                }

                return false;
            }

            if (Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.BackupPath) == false)
            {
                if (allowExceptionOnError)
                {
                    throw new FileNotFoundException ("Backup directory \"" + ConfigurationStorage.ConfigurationStorageModel.BackupPath + "\" does not exists.");
                }

                return false;
            }

            if (Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.MonitoringPath) == false)
            {
                if (allowExceptionOnError)
                {
                    throw new FileNotFoundException ("Monitoring directory \"" + ConfigurationStorage.ConfigurationStorageModel.MonitoringPath + "\" does not exists.");
                }

                return false;
            }

            if (File.Exists (ConfigurationStorage.ConfigurationStorageModel.QuickSortApp) == false)
            {
                if (allowExceptionOnError)
                {
                    throw new FileNotFoundException ("Quicksort application \"" + ConfigurationStorage.ConfigurationStorageModel.QuickSortApp + "\" does not exists.");
                }

                return false;
            }

            return true;
        }



        static private string MakeTempFolder ()
        {
            string tempPath = Path.Combine (Path.GetTempPath (), ConfigurationStorage.ConfigurationStorageModel.TempFolderPrefix + Path.GetRandomFileName ().Replace (".", string.Empty).Substring (0, 8));


            if (Directory.Exists (tempPath))
            {
                Directory.Delete (tempPath, true);
            }
            Directory.CreateDirectory (tempPath);

            return tempPath;
        }



        static private void ExtractZipFile (string srcArchiveFile, string tempPath)
        {
            ZipFile.ExtractToDirectory (srcArchiveFile, tempPath);
        }



        static private void MakeBackupFile (string srcArchiveFile)
        {
            if (ConfigurationStorage.ConfigurationStorageModel.BackupEnabled)
            {
                string backupZipFileName = Path.GetFileNameWithoutExtension (srcArchiveFile) + "_" + DateTime.Now.ToString ("yyyyMMdd_HHmmss") + ".zip";
                string backupZipFile = Path.Combine (ConfigurationStorage.ConfigurationStorageModel.BackupPath, backupZipFileName);


                if (File.Exists (backupZipFile))
                {
                    File.Delete (backupZipFile);
                }
                File.Copy (srcArchiveFile, backupZipFile);
            }
        }



        static private List<String> MoveExtractedFilesToTargetFolder (string tempPath)
        {
            List<String> result = new List<String> ();
            string[] fileList = Directory.GetFiles (tempPath, "*", SearchOption.AllDirectories);


            foreach (var file in fileList)
            {
                string newTargetFile = Path.Combine (ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath, Path.GetFileName (file));


                if (File.Exists (newTargetFile))
                {
                    // Check, if the existing target file has the same data content or has only the same file name; with different content.
                    if (CompareFilesByHash (newTargetFile, file))
                    {
                        File.Delete (file); // TARTGET FILE EXISTS WITH THE SAME CONTENT, SO DELETE THE SOURCE FILE.

                        continue; // File already exists with the same content, so skip this file.
                    }
                    else
                    {
                        string randomSuffix = Path.GetRandomFileName ().Replace (".", string.Empty).Substring (0, 3); // Generate an random 3-Chars long suffix string.


                        newTargetFile = Path.Combine (ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath, $"{Path.GetFileNameWithoutExtension (file)}_{randomSuffix}{Path.GetExtension (file)}");
                    }
                }

                File.Move (file, Path.Combine (ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath, Path.GetFileName (newTargetFile)));

                result.Add (newTargetFile);
            }

            return result;
        }



        static private bool CompareFilesByHash (string file1, string file2)
        {
            try
            {
                using (var sha256 = SHA256.Create ())
                using (var stream1 = File.OpenRead (file1))
                using (var stream2 = File.OpenRead (file2))
                {
                    var hash1 = sha256.ComputeHash (stream1);
                    var hash2 = sha256.ComputeHash (stream2);


                    return StructuralComparisons.StructuralEqualityComparer.Equals (hash1, hash2);
                }
            }
            catch
            {
                return false;
            }
        }



        static private void DeleteTempFolder (string tempPath)
        {
            Directory.Delete (tempPath, true);
        }



        static private void CleanupBackupFolder ()
        {
            if (ConfigurationStorage.ConfigurationStorageModel.BackupRetentionCheckEnabled)
            {
                var files = Directory.GetFiles (ConfigurationStorage.ConfigurationStorageModel.BackupPath, "*", SearchOption.TopDirectoryOnly);


                foreach (var backupFile in files)
                {
                    try
                    {
                        if (File.GetCreationTime (backupFile) < DateTime.Now.AddDays (ConfigurationStorage.ConfigurationStorageModel.BackupRetentionPeriod * -1))
                        {
                            File.Delete (backupFile);
                        }
                    }
                    catch
                    {
                        ; // Ignore errors here.
                    }
                }
            }
        }



        static private void DeleteZipFile (string srcArchiveFile)
        {
            File.Delete (srcArchiveFile);
        }
    }
}
