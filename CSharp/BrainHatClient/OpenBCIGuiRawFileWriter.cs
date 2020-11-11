using BrainflowDataProcessing;
using LoggingInterface;
using OpenBCIInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainHatClient
{
    class OpenBCIGuiRawFileWriter
    {
        //  Events
        public event LogEventDelegate Log;

        public bool IsLogging =>  FileWriterCancelTokenSource != null;


        /// <summary>
        /// Start the file writer
        /// </summary>
        public async Task StartWritingToFileAsync(string fileNameRoot)
        {
            FileNameRoot = fileNameRoot;

            await StopWritingToFileAsync();
            Data.RemoveAll();

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
                FileWriterCancelTokenSource = null;
                FileWritingTask = null;
            }
        }


        /// <summary>
        /// Add data to the file writer
        /// </summary>
        public void AddData(object sender, HatRawDataReceivedEventArgs e)
        {
            AddData(e.Data);
        }


        /// <summary>
        /// Add data to the file writer
        /// </summary>
        public void AddData(OpenBciCyton8Reading data)
        {
            if (FileWritingTask != null)
            {
                Data.Enqueue(data);
                NotifyAddedData.Release();
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public OpenBCIGuiRawFileWriter()
        {
            Data = new ConcurrentQueue<OpenBciCyton8Reading>();
            NotifyAddedData = new SemaphoreSlim(0);
        }

        //  File writing task 
        protected CancellationTokenSource FileWriterCancelTokenSource;
        protected Task FileWritingTask;
        protected SemaphoreSlim NotifyAddedData;
        
        // Queue to hold data pending write
        ConcurrentQueue<OpenBciCyton8Reading> Data;

        //  File Name Root
        string FileNameRoot;


        /// <summary>
        /// Run function
        /// </summary>
        private async Task RunFileWriter(CancellationToken cancelToken)
        {
            try
            {
                //  generate test file name
                var timeNow = DateTimeOffset.Now;
                string fileName = Path.Combine( Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "hatClientRecordings"),  $"{FileNameRoot}_{timeNow.Year}{timeNow.Month.ToString("D02")}{timeNow.Day.ToString("D02")}-{timeNow.Hour.ToString("D02")}{timeNow.Minute.ToString("D02")}{timeNow.Second.ToString("D02")}.txt");

                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }


                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
                {
                    //  write header
                    file.WriteLine("%OpenBCI Raw EEG Data");
                    file.WriteLine("%Number of channels = 8");
                    file.WriteLine("%Sample Rate = 250 Hz");
                    file.WriteLine("%Board = OpenBCI_GUI$BoardCytonSerial");
                    file.WriteLine("%Logger = OTTOMH BCIDataLogger");
                    file.WriteLine("Sample Index, EXG Channel 0, EXG Channel 1, EXG Channel 2, EXG Channel 3, EXG Channel 4, EXG Channel 5, EXG Channel 6, EXG Channel 7, Accel Channel 0, Accel Channel 1, Accel Channel 2, Other, Other, Other, Other, Other, Other, Other, Analog Channel 0, Analog Channel 1, Analog Channel 2, Timestamp, Timestamp (Formatted)");

                    try
                    {

                        while (!cancelToken.IsCancellationRequested)
                        {
                            await NotifyAddedData.WaitAsync(cancelToken);

                            try
                            {
                                Data.TryDequeue(out var nextReading);
                                WriteToFile(file, nextReading);
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
                        file.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", e, LogLevel.FATAL));
            }
        }


        /// <summary>
        /// Write data to file
        /// </summary>
        private void WriteToFile(StreamWriter file, OpenBciCyton8Reading nextReading)
        {
            if ( nextReading == null )
            {
                Log?.Invoke(this, new LogEventArgs(this, "WriteToFile", $"Null reading writing to file.", LogLevel.WARN));
                return;
            }

            var seconds = (long)Math.Truncate(nextReading.TimeStamp);
            var time = DateTimeOffset.FromUnixTimeSeconds(seconds);
            var microseconds = nextReading.TimeStamp - seconds;

            // file.WriteLine("Sample Index, EXG Channel 0, EXG Channel 1, EXG Channel 2, EXG Channel 3, EXG Channel 4, EXG Channel 5, EXG Channel 6, EXG Channel 7, Accel Channel 0, Accel Channel 1, Accel Channel 2, Other, Other, Other, Other, Other, Other, Other, Analog Channel 0, Analog Channel 1, Analog Channel 2, Timestamp, Timestamp (Formatted)");
            var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}",
                nextReading.SampleIndex.ToString("F3"),
                nextReading.ExgCh0.ToString("F4"),
                nextReading.ExgCh1.ToString("F4"),
                nextReading.ExgCh2.ToString("F4"),
                nextReading.ExgCh3.ToString("F4"),
                nextReading.ExgCh4.ToString("F4"),
                nextReading.ExgCh5.ToString("F4"),
                nextReading.ExgCh6.ToString("F4"),
                nextReading.ExgCh7.ToString("F4"),
                nextReading.AcelCh0.ToString("F3"),
                nextReading.AcelCh1.ToString("F3"),
                nextReading.AcelCh2.ToString("F3"),
                nextReading.Other0.ToString("F3"),
                nextReading.Other1.ToString("F3"),
                nextReading.Other2.ToString("F3"),
                nextReading.Other3.ToString("F3"),
                nextReading.Other4.ToString("F3"),
                nextReading.Other5.ToString("F3"),
                nextReading.Other6.ToString("F3"),
                nextReading.AngCh0.ToString("F3"),
                nextReading.AngCh1.ToString("F3"),
                nextReading.AngCh2.ToString("F3"),
                nextReading.TimeStamp.ToString("F6"),
                string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",time.LocalDateTime.Year.ToString("D2"), time.LocalDateTime.Month.ToString("D2"), time.LocalDateTime.Day.ToString("D2"), time.LocalDateTime.Hour.ToString("D2"), time.LocalDateTime.Minute.ToString("D2"), time.LocalDateTime.Second.ToString("D2"), ((int)(microseconds*1000000)).ToString("D6"))


                );

            file.WriteLine(line);
        }
    }
}
