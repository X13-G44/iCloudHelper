/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				14.03.2026
///
/// ////////////////////////////////////////////////////////////////////////
/// 
/// SPDX-License-Identifier: Apache-2.0
/// Copyright (c) 2026 Christian Harscher (alias X13-G44)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUnzip.Help
{
    public class OwnFileSystemWatcher : IDisposable
    {
        public bool IsMonitoring => _Task != null && _Task.IsCompleted == false;
        public string MonitoredPath { get; private set; } = String.Empty;



        private Task _Task = null;
        private CancellationTokenSource _CTS = null;



        /// <summary>
        /// Starts monitoring the specified directory for newly added files and invokes a callback when new files are detected.
        /// </summary>
        /// <remarks>Monitoring runs asynchronously and can be cancelled by calling the Stop method.
        /// The callback is triggered only after the file count in the directory remains stable for a short period (~3sec), ensuring
        /// that all new files have been added. This method is not thread-safe and should not be called concurrently.</remarks>
        /// <param name="path">The full path of the directory to monitor for new files. Cannot be null or empty.</param>
        /// <param name="triggerOnce">A value indicating whether the monitoring should stop after the first detection of new files. If <see
        /// langword="true"/>, monitoring stops after the first trigger; otherwise, it continues until cancelled.</param>
        /// <param name="filesAdded">A callback action that is invoked when new files are detected. The parameter passed to the callback is <see
        /// langword="true"/> if the monitoring was cancelled; otherwise, <see langword="false"/>.</param>
        public OwnFileSystemWatcher (string path, bool triggerOnce, Action<bool> filesAdded)
        {
            this.MonitoredPath = path;

            _CTS = new CancellationTokenSource ();
            _Task = Task.Run (() =>
            {
                do
                {
                    bool newFilesDetected = false;
                    int fileCount = Directory.GetFiles (path).Length;   // Get the current file count in the directory. If there are new files, this count will be changed; so we can detect new files by watching the file count; files like "desktop.ini" will be ignored, because they are not loaded into the file title list and therefore not relevant for us.
                    int loopsWithoutFileCountChanges = 0;


                    while (true)
                    {
                        if (newFilesDetected == false)
                        {
                            // Check for new added files.

                            int currentFileCount = Directory.GetFiles (path).Length;


                            if (currentFileCount > fileCount)
                            {
                                newFilesDetected = true;
                            }
                            else if (currentFileCount < fileCount)
                            {
                                fileCount = currentFileCount;
                            }
                        }
                        else
                        {
                            // At least one new file was added to the watched directory.
                            // Now wait until the file count is stable, which means that all new files have been added to the directory and no more files are currently being added.
                            // This is important, because we should not start the refresh of the file title list while new files are still being added to the directory,
                            // because this can cause errors in the loading process of the file title list.

                            if (fileCount != Directory.GetFiles (path).Length)
                            {
                                fileCount = Directory.GetFiles (path).Length;
                                loopsWithoutFileCountChanges = 0;
                            }
                            else
                            {
                                loopsWithoutFileCountChanges++;
                            }

                            if (loopsWithoutFileCountChanges > 30)   // 30 Loops à 100msec => 3 seconds without file count changes; which should be a good indication that the file count is now stable.
                            {
                                break;
                            }
                        }

                        Task.Delay (100).Wait ();

                        if (_CTS.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    // Fire action delegate.
                    filesAdded.Invoke (_CTS.IsCancellationRequested);

                } while (triggerOnce == false && _CTS.IsCancellationRequested == false);
            });
        }



        /// <summary>
        /// Requests cancellation of the monitoring operation.
        /// </summary>
        /// <remarks>Calling this method signals any ongoing operation to stop as soon as possible. Subsequent calls have
        /// no effect if cancellation has already been requested.</remarks>
        public void Stop ()
        {
            _CTS?.Cancel ();
        }



        public void Dispose ()
        {
            this.Stop ();
        }
    }
}
