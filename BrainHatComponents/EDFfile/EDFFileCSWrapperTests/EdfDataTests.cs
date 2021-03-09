using EDFfile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static EDFfile.EDFfile;

namespace EDFFileSharpTests
{
    [TestClass]
    public class EdfDataTests
    {
        [TestMethod]
        public void EdfData()
        {
            var fileHandle = edfOpenFileWriteOnly("EdfDataTestFile0.bdf", 3, 8);
            edfSetDatarecordDuration(fileHandle, 100000);
            int numChannels = 8;
            int samplesInDataRecord = 10;
            for (int i = 0; i < numChannels; i++)
            {
                edfSetSamplesInDataRecord(fileHandle, i, samplesInDataRecord);
                edfSetPhysicalMaximum(fileHandle, i, 100);
                edfSetPhysicalMinimum(fileHandle, i, 0);
                edfSetDigitalMaximum(fileHandle, i, 8388607);
                edfSetDigitalMinimum(fileHandle, i, -8388608);
                edfSetLabel(fileHandle, i, $"Channel {i}");
                edfSetPrefilter(fileHandle, i, $"Prefilter {i}");
                edfSetTransducer(fileHandle, i, $"Transducer {i}");
                edfSetPhysicalDimension(fileHandle, i, $"Phys");
            }

            edfSetStartDatetime(fileHandle, 2021, 03, 07, 12, 13, 14);
            edfSetPatientName(fileHandle, "PatientX");
            edfSetPatientCode(fileHandle, "PatientCodeX");
            edfSetPatientYChromosome(fileHandle, 1);
            edfSetPatientBirthdate(fileHandle, 2021, 03, 07);
            edfSetPatientAdditional(fileHandle, "PatientAdditionalX");
            edfSetAdminCode(fileHandle, "AdminCodeX");
            edfSetTechnician(fileHandle, "TechX");
            edfSetEquipment(fileHandle, "EquipmentX");
            edfSetRecordingAdditional(fileHandle, "MyField");

            //  write five seconds worth of data
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < numChannels; j++)
                {
                    double[] fakeData = new double[samplesInDataRecord];
                    for (int k = 0; k < samplesInDataRecord; k++)
                    {
                        fakeData[k] = j + 0.01 * k;
                    }

                    edfWritePhysicalSamples(fileHandle, fakeData);
                }
            }

            //  close the file
            edfCloseFile(fileHandle);

            //  open the file to read
            fileHandle = edfOpenFileReadOnly("EdfDataTestFile0.bdf");
            Assert.IsTrue(fileHandle >= 0);

            //  check that the data matches from above
            for (ulong i = 0; i < edfDataRecoardsInFile(fileHandle); i++)
            {
                for (int j = 0; j < edfSignalsInFile(fileHandle); j++)
                {
                    int samplesPerRecord = edfSignalSamplesPerRecordInFile(fileHandle, j);


                    var data = edfReadPhysicalSamples(fileHandle, j, samplesPerRecord);

                    for(int k = 0; k < samplesPerRecord; k++)
                    {
                        Assert.AreEqual((j + .01 * k), data[k], 0.00001);
                    }
                }
            }
        }
    }
}
