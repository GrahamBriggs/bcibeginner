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

        public int BoardId => FileWriter != null ? FileWriter.BoardId : -99;

        public int SampleRate => FileWriter != null ? FileWriter.SampleRate : 0;

        public bool IsLogging => FileWriter != null ? FileWriter.IsLogging : false;

        public double FileDuration => FileWriter != null ? FileWriter.FileDuration : 0;

        public string FileName => FileWriter != null ? Path.GetFileName(FileWriter.FileName): "";


        public async Task StartWritingToFileAsync(string path, string fileNameRoot, int boardId, int sampleRate, FileWriterType format)
        {
            switch (format)
            {
                case FileWriterType.OpenBciTxt:
                    FileWriter = new OBCIGuiFormatFileWriter(boardId, sampleRate);
                    break;
                case FileWriterType.Bdf:
                    FileWriter = new BDFFormatFileWriter(boardId, sampleRate);
                    break;
            }

            await FileWriter.StartWritingToFileAsync(path, fileNameRoot);
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


        protected IBrainHatFileWriter FileWriter;
    }
}
