using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class BrainHatFileWriter :IBrainHatFileWriter
    {
        //  Events
        public event LogEventDelegate Log;

        public int BoardId => FileWriter.BoardId;

        public int SampleRate => FileWriter.SampleRate;

        public bool IsLogging => FileWriter.IsLogging;

        public double FileDuration => FileWriter.FileDuration;

        public string FileName => Path.GetFileName(FileWriter.FileName);


        public async Task StartWritingToFileAsync(string path, string fileNameRoot)
        {
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


        public BrainHatFileWriter(int boardId, int sampleRate, int format)
        {
            switch ( format )
            {
                case 0:
                    FileWriter = new OBCIGuiFormatFileWriter(boardId, sampleRate);
                    break;
                case 1:
                    FileWriter = new BDFFormatFileWriter(boardId, sampleRate);
                    break;
            }
        }


        protected IBrainHatFileWriter FileWriter;
    }
}
