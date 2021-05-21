using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class BrainHatFileWriter
    {
        //  Events
        public event LogEventDelegate Log;

        public int BoardId => FileWriter != null ? FileWriter.BoardId : (int)BrainhatBoardIds.UNDEFINED;

        public int SampleRate => FileWriter != null ? FileWriter.SampleRate : 0;

        public bool IsLogging => FileWriter != null ? FileWriter.IsLogging : false;

        public double FileDuration => FileWriter != null ? FileWriter.FileDuration : 0;

        public string FileName => FileWriter != null ? Path.GetFileName(FileWriter.FileName) : "";


        public async Task StartWritingToFileAsync(string path, string fileNameRoot, int boardId, int sampleRate, FileWriterType format)
        {
            switch (format)
            {
                case FileWriterType.OpenBciTxt:
                    {
                        var newWriter = new OBCIGuiFormatFileWriter(boardId, sampleRate);
                        newWriter.Log += OnLog;
                        FileWriter = newWriter;
                    }
                    break;
                case FileWriterType.Bdf:
                    {
                        var newWriter = new BDFFormatFileWriter(boardId, sampleRate);
                        newWriter.Log += OnLog;
                        FileWriter = newWriter;
                    }
                    break;
            }

            await FileWriter.StartWritingToFileAsync(path, fileNameRoot);
        }

        void OnLog(object sender, LogEventArgs e)
        {
            Log?.Invoke(sender, e);
        }

        public async Task StopWritingToFileAsync()
        {
            await FileWriter.StopWritingToFileAsync();
        }

        public void AddData(object sender, BFSampleEventArgs e)
        {
            FileWriter.AddData(sender, e);
        }

        public void AddData(IBFSample data)
        {
            FileWriter.AddData(data);
        }

        public void AddData(IEnumerable<IBFSample> chunk)
        {
            FileWriter.AddData(chunk);
        }


        public BrainHatFileWriter()
        {

        }

 
        IBrainHatFileWriter FileWriter;
    }
}
