using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public enum FileWriterType
    {
        OpenBciTxt,
        Bdf,
    }


    public interface IBrainHatFileWriter
    {
        bool IsLogging { get; }

        double FileDuration { get; }
        
        string FileName { get; }

        int BoardId { get; }

        int SampleRate { get; }

        Task StartWritingToFileAsync(string path, string fileNameRoot);


        Task StopWritingToFileAsync();

        void AddData(object sender, BFSampleEventArgs e);
        void AddData(IBFSample data);
        void AddData(IEnumerable<IBFSample> chunk);
    }


    public interface IBrainHatFileReader
    {
        int BoardId { get; }

        int SampleRate { get; }

        int NumberOfChannels { get; }

        double? StartTime { get; }

        double? EndTime { get; }

        double Duration { get; }

        bool IsValidFile { get; }

        Task<bool> ReadFileForHeaderAsync(string fileName);

        Task<bool> ReadFileAsync(string fileName);

        IEnumerable<IBFSample> Samples { get; }
    }
}
