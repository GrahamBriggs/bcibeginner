using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public interface IBrainHatFileWriter
    {
        bool IsLogging { get; }
        double FileDuration { get; }
        string FileName { get; }

        Task StartWritingToFileAsync(string fileNameRoot, int boardId, int sampleRate);
        Task StopWritingToFileAsync();

        void AddData(object sender, BFSampleEventArgs e);
        void AddData(IBFSample data);
        void AddData(IEnumerable<IBFSample> chunk);

    }
}
