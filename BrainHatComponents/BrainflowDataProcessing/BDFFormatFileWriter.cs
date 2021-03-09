using BrainflowInterfaces;
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

        //  Public Properties
        public bool IsLogging => FileWriterCancelTokenSource != null;

        public string FileName { get; protected set; }

        public double FileDuration => FileTimer.Elapsed.TotalSeconds;


        /// <summary>
        /// Start the file writer
        /// </summary>
        public async Task StartWritingToFileAsync(string fileNameRoot, int boardId, int sampleRate)
        {
            BoardId = boardId;
            SampleRate = sampleRate;

            await StopWritingToFileAsync();
            Data.RemoveAll();

            var timeNow = DateTimeOffset.Now;
            FileName = Path.Combine(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "brainHatRecordings"), $"{fileNameRoot}_{timeNow.Year}{timeNow.Month.ToString("D02")}{timeNow.Day.ToString("D02")}-{timeNow.Hour.ToString("D02")}{timeNow.Minute.ToString("D02")}{timeNow.Second.ToString("D02")}.bdf");

            FileWriterCancelTokenSource = new CancellationTokenSource();
            FileWritingTask = RunFileWriter(FileWriterCancelTokenSource.Token);
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
                FileName = "";
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
        public BDFFormatFileWriter()
        {
            Data = new ConcurrentQueue<IBFSample>();
            NotifyAddedData = new SemaphoreSlim(0);

            FileName = "";
            FileTimer = new Stopwatch();
            FileTimer.Reset();
        }

        //  File writing task 
        protected CancellationTokenSource FileWriterCancelTokenSource;
        protected Task FileWritingTask;
        protected SemaphoreSlim NotifyAddedData;

        // Queue to hold data pending write
        ConcurrentQueue<IBFSample> Data;

        //  BDF File
        bool WroteHeader = false;
        int FileHandle;
        protected Stopwatch FileTimer { get; set; }

        //  Board propertites
        int BoardId;
        int SampleRate;

        //  Signal Properties
        int NumberOfExgChannels;
        int NumberOfAcelChannels;
        int NumberOfOtherChannels;
        int NumberOfAnalogChannels;
        double FirstTimeStamp;


        /// <summary>
        /// Run function
        /// </summary>
        private async Task RunFileWriter(CancellationToken cancelToken)
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
                    int test = edfCloseFile(FileHandle);
                }
                FileTimer.Reset();
            }
        }


        /// <summary>
        /// Write a chunk of samples to the file
        /// we always use page size = sample rate, so each chunk represents one second of data
        /// </summary>
        private void WriteToFile(List<IBFSample> data)
        {
            if ( data.Count > 0 )
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
        private void WriteHeader(IBFSample firstSample)
        {
            if (firstSample == null || WroteHeader)
                return; //  invalid first sample or we already wrote the header 

            //  open the file for writing
            FileHandle = edfOpenFileWriteOnly(FileName, 3, firstSample.SampleSize);
            if ( FileHandle < 0)
            {
                throw new Exception("Unable to open the file.");
            }

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
            for(int i = 0; i < firstSample.NumberExgChannels; i++)
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
            for(int i = 0; i < firstSample.NumberAccelChannels; i++)
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
            edfSetPhysicalMaximum(FileHandle, signalCount, 999999.0);
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
            edfSetStartDatetime(FileHandle, firstSample.ObservationTime.Year, firstSample.ObservationTime.Month, firstSample.ObservationTime.Day, firstSample.ObservationTime.Hour, firstSample.ObservationTime.Minute, firstSample.ObservationTime.Second);
            edfSetPatientName(FileHandle, "Set Patient Name");
            edfSetPatientCode(FileHandle, "Set patient code");
            edfSetPatientYChromosome(FileHandle, 1);
            edfSetPatientBirthdate(FileHandle, 2021, 03, 07);
            edfSetPatientAdditional(FileHandle, "Set patient additional");
            edfSetAdminCode(FileHandle, "Set Admin Code");
            edfSetTechnician(FileHandle, "Set Technician");
            edfSetEquipment(FileHandle, BoardId.GetEquipmentName());
            edfSetRecordingAdditional(FileHandle, BoardId.GetSampleName());

            //  we are ready to write data
            FileTimer.Restart();
            WroteHeader = true;
        }
    }
}
