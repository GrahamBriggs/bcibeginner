﻿using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static EDFfile.EDFfile;

namespace BrainflowDataProcessing
{
    public class BDFFormatFileWriter : IBrainHatFileWriter
    {
        //  Events
        public event LogEventDelegate Log;

        public int BoardId { get; private set; }

        public int SampleRate { get; private set; }

        //  Public Properties
        public bool IsLogging => FileWriterCancelTokenSource != null;

        public string FileName { get; private set; }

        public double FileDuration => FileTimer.Elapsed.TotalSeconds;

        public FileHeaderInfo Info { get; protected set; }

        /// <summary>
        /// Start the file writer
        /// </summary>
        public async Task StartWritingToFileAsync(string path, string fileNameRoot)
        {
            await StopWritingToFileAsync();
            Data.RemoveAll();

            var timeNow = DateTimeOffset.Now;
            FileName = Path.Combine(path, $"{fileNameRoot}_{timeNow.Year}{timeNow.Month.ToString("D02")}{timeNow.Day.ToString("D02")}-{timeNow.Hour.ToString("D02")}{timeNow.Minute.ToString("D02")}{timeNow.Second.ToString("D02")}.bdf");

            FileWriterCancelTokenSource = new CancellationTokenSource();
            FileWritingTask = RunFileWriter(FileWriterCancelTokenSource.Token);
        }


        /// <summary>
        /// Start the file writer
        /// </summary>
        public async Task StartWritingToFileAsync(string path, string fileNameRoot, FileHeaderInfo info)
        {
            Info = info;
            await StartWritingToFileAsync(path, fileNameRoot);
        }


        /// <summary>
        /// Stop the file writer
        /// </summary>
        public async Task StopWritingToFileAsync()
        {
            if (FileWriterCancelTokenSource != null)
            {
                FileWriterCancelTokenSource.Cancel();
                await FileWritingTask;

                FileWriterCancelTokenSource = null;
                FileWritingTask = null;
            }
        }


        /// <summary>
        /// Add data to the file writer
        /// </summary>
        public void AddData(object sender, BFSampleEventArgs e)
        {
            AddData(e.Sample);
        }


        /// <summary>
        /// Add data to the file writer
        /// </summary>
        public void AddData(IBFSample data)
        {
            if (FileWritingTask != null)
            {
                Data.Enqueue(data);
                NotifyAddedData.Release();
            }
        }

        /// <summary>
        /// Add a chunk of data to the file writer
        /// </summary>
        public void AddData(IEnumerable<IBFSample> chunk)
        {
            if (FileWritingTask != null)
            {
                Data.AddRange(chunk);
                NotifyAddedData.Release();
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public BDFFormatFileWriter(int boardId, int sampleRate)
        {
            Data = new ConcurrentQueue<IBFSample>();
            NotifyAddedData = new SemaphoreSlim(0);

            BoardId = boardId;
            SampleRate = sampleRate;

            FileName = "";
            FileTimer = new Stopwatch();
            FileTimer.Reset();
        }

        //  File writing task 
        CancellationTokenSource FileWriterCancelTokenSource;
        Task FileWritingTask;
        SemaphoreSlim NotifyAddedData;

        // Queue to hold data pending write
        ConcurrentQueue<IBFSample> Data;

        //  BDF File
        bool WroteHeader = false;
        int FileHandle;
        Stopwatch FileTimer;

        //  Board propertites


        //  Signal Properties
        int NumberOfExgChannels;
        int NumberOfAcelChannels;
        int NumberOfOtherChannels;
        int NumberOfAnalogChannels;
        double FirstTimeStamp;


        /// <summary>
        /// Run function
        /// </summary>
        async Task RunFileWriter(CancellationToken cancelToken)
        {
            try
            {
                //  make sure directory exists for this file
                if (!Directory.Exists(Path.GetDirectoryName(FileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FileName));
                }

                WroteHeader = false;
                FileHandle = -1;

                while (!cancelToken.IsCancellationRequested)
                {
                    await NotifyAddedData.WaitAsync(cancelToken);

                    try
                    {
                        while (Data.Count >= SampleRate)
                        {
                            var data = new List<IBFSample>();
                            while (data.Count < SampleRate)
                            {
                                Data.TryDequeue(out var nextReading);
                                data.Add(nextReading);
                            }

                            WriteHeader(data.FirstOrDefault());
                            WriteToFile(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", ex, LogLevel.ERROR));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", e, LogLevel.FATAL));
            }
            finally
            {
                if (FileHandle >= 0)
                {
                    if (edfCloseFile(FileHandle) == 0)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", $"Closed file {FileName}.", LogLevel.INFO));
                    }
                    else
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", $"Error closing file {FileName}.", LogLevel.ERROR));
                    }
                }
                FileTimer.Stop();
            }
        }


        /// <summary>
        /// Write a chunk of samples to the file
        /// we always use page size = sample rate, so each chunk represents one second of data
        /// </summary>
        void WriteToFile(List<IBFSample> data)
        {
            if (data.Count > 0)
            {
                //  sample index
                var nextData = data.Select(x => x.SampleIndex).ToArray();
                var result = edfWritePhysicalSamples(FileHandle, nextData);

                //  EXG channels
                for (int i = 0; i < NumberOfExgChannels; i++)
                {
                    nextData = data.GetExgDataForChannel(i);
                    result = edfWritePhysicalSamples(FileHandle, nextData);
                }

                //  Acel channels
                for (int i = 0; i < NumberOfAcelChannels; i++)
                {
                    nextData = data.GetAcelDataForChannel(i);
                    edfWritePhysicalSamples(FileHandle, nextData);
                }

                //  Other channels
                for (int i = 0; i < NumberOfOtherChannels; i++)
                {
                    nextData = data.GetOtherDataForChannel(i);
                    edfWritePhysicalSamples(FileHandle, nextData);
                }

                //  Analog channels
                for (int i = 0; i < NumberOfAnalogChannels; i++)
                {
                    nextData = data.GetAnalogDataForChannel(i);
                    edfWritePhysicalSamples(FileHandle, nextData);
                }

                //  Time stamp
                nextData = data.Select(x => x.TimeStamp - FirstTimeStamp).ToArray();
                edfWritePhysicalSamples(FileHandle, nextData);
            }
        }


        /// <summary>
        /// Open the file for writing and write the header information
        /// </summary>
        void WriteHeader(IBFSample firstSample)
        {
            if (firstSample == null || WroteHeader)
                return; //  invalid first sample or we already wrote the header 

            //  open the file for writing
            FileHandle = edfOpenFileWriteOnly(FileName, 3, firstSample.SampleSize);
            if (FileHandle < 0)
            {
                throw new Exception("Unable to open the file.");
            }

            Log?.Invoke(this, new LogEventArgs(this, "WriteHeader", $"Started recording file {FileName}.", LogLevel.INFO));

            int signalCount = 0;

            //  Signal Properties
            //
            //  sample index
            edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
            edfSetPhysicalMaximum(FileHandle, signalCount, 255);
            edfSetPhysicalMinimum(FileHandle, signalCount, 0);
            edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
            edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
            edfSetLabel(FileHandle, signalCount, "SampleIndex");
            edfSetPrefilter(FileHandle, signalCount, "");
            edfSetTransducer(FileHandle, signalCount, "");
            edfSetPhysicalDimension(FileHandle, signalCount, "counter");
            signalCount++;
            //
            //  exg channels
            for (int i = 0; i < firstSample.NumberExgChannels; i++)
            {
                edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
                edfSetPhysicalMaximum(FileHandle, signalCount, 187500.000);
                edfSetPhysicalMinimum(FileHandle, signalCount, -187500.000);
                edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
                edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
                edfSetLabel(FileHandle, signalCount, $"EXG{i}");
                edfSetPrefilter(FileHandle, signalCount, "");
                edfSetTransducer(FileHandle, signalCount, "");
                edfSetPhysicalDimension(FileHandle, signalCount, "uV");
                signalCount++;
            }
            NumberOfExgChannels = firstSample.NumberExgChannels;
            //
            //  acel channels
            for (int i = 0; i < firstSample.NumberAccelChannels; i++)
            {
                edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
                edfSetPhysicalMaximum(FileHandle, signalCount, 1.0);
                edfSetPhysicalMinimum(FileHandle, signalCount, -1.0);
                edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
                edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
                edfSetLabel(FileHandle, signalCount, $"Acel{i}");
                edfSetPrefilter(FileHandle, signalCount, "");
                edfSetTransducer(FileHandle, signalCount, "");
                edfSetPhysicalDimension(FileHandle, signalCount, "unit");
                signalCount++;
            }
            NumberOfAcelChannels = firstSample.NumberAccelChannels;
            //
            //  other channels
            for (int i = 0; i < firstSample.NumberOtherChannels; i++)
            {
                edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
                edfSetPhysicalMaximum(FileHandle, signalCount, 9999.0);
                edfSetPhysicalMinimum(FileHandle, signalCount, -9999.0);
                edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
                edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
                edfSetLabel(FileHandle, signalCount, $"Other{i}");
                edfSetPrefilter(FileHandle, signalCount, "");
                edfSetTransducer(FileHandle, signalCount, "");
                edfSetPhysicalDimension(FileHandle, signalCount, "other");
                signalCount++;
            }
            NumberOfOtherChannels = firstSample.NumberOtherChannels;
            //
            //  analog channels
            for (int i = 0; i < firstSample.NumberAnalogChannels; i++)
            {
                edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
                edfSetPhysicalMaximum(FileHandle, signalCount, 9999.0);
                edfSetPhysicalMinimum(FileHandle, signalCount, -9999.0);
                edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
                edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
                edfSetLabel(FileHandle, signalCount, $"Analog{i}");
                edfSetPrefilter(FileHandle, signalCount, "");
                edfSetTransducer(FileHandle, signalCount, "");
                edfSetPhysicalDimension(FileHandle, signalCount, "analog");
                signalCount++;
            }
            NumberOfAnalogChannels = firstSample.NumberAnalogChannels;
            //
            //  timestamp
            edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
            edfSetPhysicalMaximum(FileHandle, signalCount, 43200.0);
            edfSetPhysicalMinimum(FileHandle, signalCount, 0);
            edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
            edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
            edfSetLabel(FileHandle, signalCount, "TestTime");
            edfSetPrefilter(FileHandle, signalCount, "");
            edfSetTransducer(FileHandle, signalCount, "");
            edfSetPhysicalDimension(FileHandle, signalCount, "seconds");
            FirstTimeStamp = firstSample.TimeStamp;

            //  File Header Properties
            //
            Info.ValidateForBdf();
            edfSetStartDatetime(FileHandle, firstSample.ObservationTime.Year, firstSample.ObservationTime.Month, firstSample.ObservationTime.Day, firstSample.ObservationTime.Hour, firstSample.ObservationTime.Minute, firstSample.ObservationTime.Second);
            edfSetSubsecondStarttime(FileHandle, firstSample.ObservationTime.Millisecond * 10_000);
            edfSetPatientName(FileHandle, Info.SubjectName);
            edfSetPatientCode(FileHandle, Info.SubjectCode);
            edfSetPatientYChromosome(FileHandle, (int)Info.SubjectGender);
            edfSetPatientBirthdate(FileHandle, Info.SubjectBirthday.Year, Info.SubjectBirthday.Month, Info.SubjectBirthday.Day);
            edfSetPatientAdditional(FileHandle, Info.SubjectAdditional);
            edfSetAdminCode(FileHandle, Info.AdminCode);
            edfSetTechnician(FileHandle, Info.Technician);
            edfSetEquipment(FileHandle, Info.Device);
            edfSetRecordingAdditional(FileHandle, BoardId.GetSampleNameShort());

            //  we are ready to write data
            FileTimer.Restart();
            WroteHeader = true;
        }

      
    }
}
