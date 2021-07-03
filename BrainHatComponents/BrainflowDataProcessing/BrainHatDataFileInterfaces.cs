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

    public enum Gender
    {
        XX,
        XY,
    }

    public class FileHeaderInfo
    {
        const int NumberSubjectCharsAvailable = 62;
        const int NumberTechnicianCharsAvailable = 35;


        public FileHeaderInfo()
        {
            SessionName = "";
            SubjectName = "";
            SubjectCode = "";
            SubjectBirthday = DateTime.Today;
            SubjectAdditional = "";
            SubjectGender = Gender.XX;
            AdminCode = "";
            Technician = "";
            Device = "";
        }

        public FileHeaderInfo(FileHeaderInfo value)
        {
            SessionName = value.SessionName;
            SubjectName = value.SubjectName;
            SubjectCode = value.SubjectCode;
            SubjectBirthday = value.SubjectBirthday;
            SubjectAdditional = value.SubjectAdditional;
            SubjectGender = value.SubjectGender;
            AdminCode = value.AdminCode;
            Technician = value.Technician;
            Device = value.Device;
        }

        public void SetBoard(int boardId)
        {
            Device = boardId.GetEquipmentName();
        }


        public void ValidateForBdf()
        {
            // 72 bytes are available for:	patientname     patientcode     birthdate   patient_additional + 3 commas
            // 42 bytes are available for:	admincode       technician      equipment   recording_additional + 3 commas
            // Birthdate takes 10 bytes.
            // Recording additional is reserved 4 bytes

            if ( (SubjectName.Length + SubjectCode.Length + SubjectAdditional.Length) > NumberSubjectCharsAvailable)
            {
                int count = NumberSubjectCharsAvailable;
                SubjectName = SubjectName.Substring(0, count);
                count -= SubjectName.Length;
                SubjectCode = SubjectCode.Substring(0, count);
                count -= SubjectCode.Length;
                SubjectAdditional = SubjectAdditional.Substring(0, count);
            }

            if ( AdminCode.Length + Technician.Length + Device.Length > NumberTechnicianCharsAvailable)
            {
                int count = NumberTechnicianCharsAvailable;
                AdminCode = AdminCode.Substring(0, count);
                count -= AdminCode.Length;
                Technician = Technician.Substring(0, count);
                count -= Technician.Length;
                Device = Device.Substring(0, count);
            }
        }

        public int SubjectCharsRemaining()
        {
            return NumberSubjectCharsAvailable - (SubjectName.Length + SubjectCode.Length + SubjectAdditional.Length);
        }

        public int TechCharsRemaining()
        {
            return NumberTechnicianCharsAvailable - (AdminCode.Length + Technician.Length + Device.Length);
        }

        public bool ValidateStrings()
        {
            if (SessionName.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals(' ')) &&
                SubjectName.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals(' ')) &&
                SubjectCode.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals(' ')) &&
                SubjectAdditional.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals(' ')) &&
                AdminCode.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals(' ')) &&
                Technician.All(c => Char.IsLetterOrDigit(c) || c.Equals('_') || c.Equals(' ')))
                return true;
            return false;
        }

        public string SessionName { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public DateTime SubjectBirthday { get; set; }
        public string SubjectAdditional { get; set; }
        public Gender SubjectGender { get; set; }
        public string AdminCode { get; set; }
        public string Technician { get; set; }
        public string Device { get; set; }
        
    }


    public interface IBrainHatFileWriter
    {
        bool IsLogging { get; }

        double FileDuration { get; }
        
        string FileName { get; }

        int BoardId { get; }

        int SampleRate { get; }

        Task StartWritingToFileAsync(string path, string fileNameRoot);
        Task StartWritingToFileAsync(string path, string fileNameRoot, FileHeaderInfo info);


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
