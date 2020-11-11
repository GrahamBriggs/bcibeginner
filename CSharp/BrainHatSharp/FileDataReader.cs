
using LoggingInterface;
using OpenBCIInterfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainHatSharp
{
    class FileDataReader
    {
        public event LogEventDelegate Log;
        public event OpenBciCyton8DataEventDelegate BoardReadData;


        public async Task<bool> StartFileDataReaderAsync(string fileName)
        {
            await StopFileDataReaderAsync();
  
            try
            {
                RawData = ReadOpenBciGuiFile.ParseFile(fileName);

                DataFileStartTime = RawData.First().TimeStamp;
                DataFileDuration = RawData.Last().TimeStamp - RawData.First().TimeStamp;
                DemoFileName = fileName;
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "StartFileDataReaderAsync", $"Unable to open file {fileName}. {e}.", LogLevel.ERROR));
                return false;
            }

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunFileReaderAsync(CancelTokenSource.Token);

            return true;
        }

      

        public async Task StopFileDataReaderAsync()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                if (RunTask != null)
                    await RunTask;

                CancelTokenSource = null;
                RunTask = null;
            }
        }


        public FileDataReader()
        {

        }

        //  Thread run objects
        CancellationTokenSource CancelTokenSource;
        Task RunTask;

        string DemoFileName { get; set; }

        List<OpenBciCyton8Reading> RawData { get; set; }
        double DataFileStartTime { get; set; }
        double DataFileDuration { get; set; }
        double RealStartTime { get; set; }


        private async Task RunFileReaderAsync(CancellationToken token)
        {
            try
            {
                double loopCounter = 0.0;
                RealStartTime = DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds();
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                while (!token.IsCancellationRequested)
                {   
                    var readings = new List<OpenBciCyton8Reading>();
                    foreach ( var nextData in RawData)
                    {
                        var newData = new OpenBciCyton8Reading(nextData);
                        var newTimeStamp = (RealStartTime + (loopCounter * DataFileDuration)) + (newData.TimeStamp - DataFileStartTime);
                        newData.TimeStamp = newTimeStamp;
                        readings.Add(newData);

                        //  fake this as one read every 50 ms (20 hz read like the board reader class)
                        if (readings.Count > 12)
                        {
                            double sleep = newData.TimeStamp - DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds();
                            if (sleep > 0)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(sleep));
                            }

                            if ( sw.Elapsed.TotalSeconds > 5)
                            {
                                Log?.Invoke(this, new LogEventArgs(this, "RunFileReaderAsync", $"Simulating with {Path.GetFileName(DemoFileName)}: Test time {(nextData.TimeStamp - DataFileStartTime).ToString("F4")}", LogLevel.INFO));
                                sw.Restart();
                            }
                            
                            BoardReadData?.Invoke(this, new OpenBciCyton8DataEventArgs(readings));
                            readings.Clear();
                        }

                        if (CancelTokenSource.IsCancellationRequested)
                            break;
                    }

                    loopCounter += 1.0;
                    Log?.Invoke(this, new LogEventArgs(this, "RunFileReaderAsync", $"Simulating with {Path.GetFileName(DemoFileName)}: Rolling over at test time {DataFileDuration.ToString("F4")}", LogLevel.INFO));
                }
            }
            catch ( OperationCanceledException )
            { }
            catch ( Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunFileReaderAsync", e, LogLevel.ERROR));
            }
        }


    }
}
