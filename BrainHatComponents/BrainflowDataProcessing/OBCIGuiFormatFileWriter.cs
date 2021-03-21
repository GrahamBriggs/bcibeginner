using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class OBCIGuiFormatFileWriter : IBrainHatFileWriter
    {
        //  Events
        public event LogEventDelegate Log;

        public int BoardId { get; protected set; }

        public int SampleRate { get; protected set; }

        public bool IsLogging => FileWriterCancelTokenSource != null;


        /// <summary>
        /// Start the file writer
        /// </summary>
        public async Task StartWritingToFileAsync(string path, string fileNameRoot)
        {
            FileNameRoot = fileNameRoot;

            await StopWritingToFileAsync();
            Data.RemoveAll();

            var timeNow = DateTimeOffset.Now;
            FileName = Path.Combine(path, $"{FileNameRoot}_{timeNow.Year}{timeNow.Month.ToString("D02")}{timeNow.Day.ToString("D02")}-{timeNow.Hour.ToString("D02")}{timeNow.Minute.ToString("D02")}{timeNow.Second.ToString("D02")}.txt");

            FileWriterCancelTokenSource = new CancellationTokenSource();
            FileWritingTask = RunFileWriter(FileWriterCancelTokenSource.Token);
        }

        public string FileName { get; protected set; }

        protected Stopwatch FileTimer { get; set; }
        public double FileDuration => FileTimer.Elapsed.TotalSeconds;

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

        public void AddData(IEnumerable<IBFSample> chunk)
        {
            if ( FileWritingTask != null )
            {
                Data.AddRange(chunk);
                NotifyAddedData.Release();
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public OBCIGuiFormatFileWriter(int boardId, int sampleRate)
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
        protected CancellationTokenSource FileWriterCancelTokenSource;
        protected Task FileWritingTask;
        protected SemaphoreSlim NotifyAddedData;

        // Queue to hold data pending write
        ConcurrentQueue<IBFSample> Data;

        //  File Name Root
        string FileNameRoot;
      

        //OpenBCI_GUI$BoardCytonSerialDaisy
        //OpenBCI_GUI$BoardCytonSerial

        private string FileBoardDescription()
        {
            switch (BoardId)
            {
                case 0:
                    return "OpenBCI_GUI$BoardCytonSerial";
                case 2:
                    return "OpenBCI_GUI$BoardCytonSerialDaisy";
                default:
                    return "Unknown?";
            }
        }
        /// <summary>
        /// Run function
        /// </summary>
        private async Task RunFileWriter(CancellationToken cancelToken)
        {
            try
            {
                //  generate test file name
              
                if (!Directory.Exists(Path.GetDirectoryName(FileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FileName));
                }

                using (var fileStream = await FileSystemExtensionMethods.WaitForFileAsync(FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileStream))
                {
                    Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", $"Started recording file {FileName}.", LogLevel.INFO));

                    //  write header
                    file.WriteLine("%OpenBCI Raw EEG Data");
                    file.WriteLine($"%Number of channels = {brainflow.BoardShim.get_exg_channels(BoardId).Length}");
                    file.WriteLine($"%Sample Rate = {SampleRate} Hz");
                    file.WriteLine($"%Board = {FileBoardDescription()}");
                    file.WriteLine("%Logger = brainHat");
                    bool writeHeader = false;


                    try
                    {
                        FileTimer.Restart();

                        while (!cancelToken.IsCancellationRequested)
                        {
                            await NotifyAddedData.WaitAsync(cancelToken);

                            try
                            {
                                while (Data.Count > 0)
                                {
                                    Data.TryDequeue(out var nextReading);
                                    if (nextReading == null)
                                    {
                                        Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", $"Null sample.", LogLevel.WARN));
                                        continue;
                                    }

                                    if (!writeHeader)
                                    {
                                        WriteHeaderToFile(file, nextReading);
                                        writeHeader = true;
                                    }

                                    WriteToFile(file, nextReading);
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
                        file.Close();
                        FileTimer.Stop();
                        Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", $"Closed recording file {FileName}.", LogLevel.INFO));
                    }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunFileWriter", e, LogLevel.FATAL));
            }
        }


        /// <summary>
        /// Write header to file based on the first sample recorded
        /// </summary>
        private void WriteHeaderToFile(StreamWriter file, IBFSample nextReading)
        {
            string header = "Sample Index";

            //  exg channels
            for (int i = 0; i < nextReading.NumberExgChannels; i++)
            {
                header += $", EXG Channel {i}";
            }

            //  accelerometer channels
            for (int i = 0; i < nextReading.NumberExgChannels; i++)
            {
                header += $", Accel Channel {i}";
            }

            //  other channels
            for (int i = 0; i < nextReading.NumberOtherChannels; i++)
            {
                header += $", Other";
            }

            //  analog channels
            for (int i = 0; i < nextReading.NumberAnalogChannels; i++)
            {
                header += $", Analog Channel {i}";
            }

            //  time stamps
            header += ", Timestamp, Timestamp (Formatted)";

            file.WriteLine(header);
        }


        /// <summary>
        /// Write a sample to file
        /// </summary>
        private void WriteToFile(StreamWriter file, IBFSample nextSample)
        {
            var seconds = (long)Math.Truncate(nextSample.TimeStamp);
            var time = DateTimeOffset.FromUnixTimeSeconds(seconds);
            var microseconds = nextSample.TimeStamp - seconds;

            //  sample index
            var writeLine = nextSample.SampleIndex.ToString("F3");

            //  exg channels
            foreach (var nextExg in nextSample.ExgData)
            {
                writeLine += $",{nextExg:F4}";
            }

            //  accelerometer channels
            foreach (var nextAcel in nextSample.AccelData)
            {
                writeLine += $",{nextAcel:F4}";
            }

            //  other channels
            foreach (var nextOther in nextSample.OtherData)
            {
                writeLine += $",{nextOther:F4}";
            }

            //  analog channels
            foreach (var nextAnalog in nextSample.AnalogData)
            {
                writeLine += $",{nextAnalog:F4}";
            }

            //  raw time stamp
            writeLine += $",{nextSample.TimeStamp:F6}";

            //  formatted time stamp
            writeLine += string.Format(",{0}-{1}-{2} {3}:{4}:{5}.{6}", time.LocalDateTime.Year.ToString("D2"), time.LocalDateTime.Month.ToString("D2"), time.LocalDateTime.Day.ToString("D2"), time.LocalDateTime.Hour.ToString("D2"), time.LocalDateTime.Minute.ToString("D2"), time.LocalDateTime.Second.ToString("D2"), ((int)(microseconds * 1000000)).ToString("D6"));

            file.WriteLine(writeLine);
        }
    }
}
