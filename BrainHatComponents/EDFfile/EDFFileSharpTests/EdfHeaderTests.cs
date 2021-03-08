using EDFfileSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static EDFfileSharp.EDFfile;

namespace EDFFileSharpTests
{
    [TestClass]
    public class EdfHeaderTests
    {
        [TestMethod]
        public async Task EdfFileHeader()
        {
            var fileHandle = edfOpenFileWriteOnly("TestFile0.bdf", 3, 8);
            edfSetDatarecordDuration(fileHandle, 100000);
            int numChannels = 8;
            int samplesInDataRecord = 10;
            for (int i = 0; i < numChannels; i++)
            {
                edfSetSamplesInDataRecord(fileHandle, i, samplesInDataRecord);
                edfSetPhysicalMaximum(fileHandle, i, 1000.0);
                edfSetPhysicalMinimum(fileHandle, i, -1000);
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

            edfCloseFile(fileHandle);

            await Task.Delay(2000);

            var readFile = edfOpenFileReadOnly("TestFile0.bdf");
           
            var header = JsonConvert.DeserializeObject<EdfHeaderStruct>(edfGetHeaderAsJson(readFile));

            Assert.AreEqual(8, header.edfsignals);
            Assert.AreEqual(2021, header.startdate_year);
            Assert.AreEqual(3, header.startdate_month);
            Assert.AreEqual(7, header.startdate_day);
            Assert.AreEqual(12, header.starttime_hour);
            Assert.AreEqual(13, header.starttime_minute);
            Assert.AreEqual(14, header.starttime_second);
            Assert.AreEqual("PatientX", header.patient_name.Trim());
            Assert.AreEqual("PatientCodeX", header.patientcode.Trim());
            Assert.AreEqual("Male", header.gender.Trim());
            Assert.AreEqual("07 mar 2021", header.birthdate.Trim());
            Assert.AreEqual("PatientAdditionalX", header.patient_additional.Trim());
            Assert.AreEqual("AdminCodeX", header.admincode.Trim());
            Assert.AreEqual("TechX", header.technician.Trim());
            Assert.AreEqual("EquipmentX", header.equipment.Trim());
            Assert.AreEqual("MyField", header.recording_additional.Trim());

            for( int i = 0; i < 8; i++)
            {
                Assert.AreEqual(10, header.signalparam[i].smp_in_datarecord);
                Assert.AreEqual(1000.0, header.signalparam[i].phys_max, 0.000000001);
                Assert.AreEqual(-1000.0, header.signalparam[i].phys_min, 0.000000001);
                Assert.AreEqual(8388607, header.signalparam[i].dig_max);
                Assert.AreEqual(-8388608, header.signalparam[i].dig_min);
                Assert.AreEqual($"Channel {i}", header.signalparam[i].label.Trim());
                Assert.AreEqual($"Prefilter {i}", header.signalparam[i].prefilter.Trim());
                Assert.AreEqual($"Transducer {i}", header.signalparam[i].transducer.Trim());
                Assert.AreEqual($"Phys", header.signalparam[i].physdimension.Trim());
            }
        }
    }
}
