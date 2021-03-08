using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EDFfileSharp;
using static EDFfileSharp.EDFfile;

namespace EDFfileSharpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileHandle = edfOpenFileReadOnly("C:/Users/grahambriggs/Documents/OpenBCI_GUI/Recordings/OpenBCI-BDF-2021-03-06_08-56-28.bdf");

			for (ulong i = 0; i < edfDataRecoardsInFile(fileHandle); i++)
			{
				Console.WriteLine($"Reading {i}");

				for (int j = 0; j < edfSignalsInFile(fileHandle); j++)
				{
					int samplesPerRecord = edfSignalSamplesPerRecordInFile(fileHandle, j);
					

					var data = edfReadPhysicalSamples(fileHandle, j, samplesPerRecord);

					//Console.WriteLine($"Channel {j}");

					//for (int k = 0; k < samplesPerRecord; k++)
					//{
					//	Console.Write($" {data[k]:F4}");
					//}
					//Console.Write("\n");
				}
			}

			var header = edfGetHeaderAsJson(fileHandle);

			edfCloseFile(fileHandle);

			Console.WriteLine("Finished");
			Console.ReadKey();
		}
    }
}
