using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public static class BFDataProcessingExtensionMethods
    {
        public static string BandPowerKey(this double value)
        {
            return $"{value:N1}";
        }
    }

    public static class FileSystemExtensionMethods
    {
        public static async Task<FileStream> WaitForFileAsync(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (int numTries = 0; numTries < 10; numTries++)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullPath, mode, access, share);
                    return fs;
                }
                catch (IOException)
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }
                    await Task.Delay(500);
                }
            }

            return null;
        }
    }
}
