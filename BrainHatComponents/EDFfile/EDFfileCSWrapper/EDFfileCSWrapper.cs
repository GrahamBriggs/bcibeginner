using System.Runtime.InteropServices;
using System.Text;


/// <summary>
/// EDF File CS Wrapper
/// EDFfile is a thin wrapper around the EDFlib code by Teunis van Beelen (https://gitlab.com/Teuniz/EDFlib)
/// The library does a simple pass through to the EDFlib functions with plain C declarations
/// The library provides functions with plain C declaration to get file property struct as a JSON string 
/// (annotations via JSON string coming soon ...)
/// </summary>
namespace EDFfile
{
    public class EDFfile
    {
        //  File Properties
        //
        #region FileProperties

        /// <summary>
        /// Get the EDF file header as a json blob
        /// </summary>
        public static string edfGetHeaderAsJson(int fileHandle)
        {
            int size = dll.edfGetHeaderAsJson(fileHandle, 0, null);
            byte[] headerAsJson = new byte[size];
            if (dll.edfGetHeaderAsJson(fileHandle, size, headerAsJson) == size)
            {
                return Encoding.ASCII.GetString(headerAsJson);
            }
            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        public static ulong edfDataRecoardsInFile(int fileHandle) { return dll.edfDataRecoardsInFile(fileHandle); }



        public static int edfSignalsInFile(int fileHandle) { return dll.edfSignalsInFile(fileHandle); }

        /// <summary>
        /// 
        /// </summary>
        public static int edfSignalSamplesPerRecordInFile(int fileHandle, int signal) { return dll.edfSignalSamplesPerRecordInFile(fileHandle, signal); }


        #endregion
        //  FileProperties


        //  Read File
        //
        #region ReadFile

        /// <summary>
        /// opens an existing file for reading 
        /// path is a null-terminated string containing the path to the file 
        /// hdr is a pointer to an edf_hdr_struct, all fields in this struct will be overwritten 
        /// the edf_hdr_struct will be filled with all the relevant header- and signalinfo/parameters 
        /// read_annotations must have one of the following values:      
        ///   EDFLIB_DO_NOT_READ_ANNOTATIONS      annotations will not be read (this saves time when opening a very large EDFplus or BDFplus file 
        ///   EDFLIB_READ_ANNOTATIONS             annotations will be read immediately, stops when an annotation has 
        ///                                       been found which contains the description "Recording ends"         
        ///   EDFLIB_READ_ALL_ANNOTATIONS         all annotations will be read immediately                           
        /// returns 0 on success, in case of an error it returns -1 and an errorcode will be set in the member "filetype" of struct edf_hdr_struct 
        /// This function is required if you want to read a file 
        /// </summary>
        public static int edfOpenFileReadOnly(string fileName) { return dll.edfOpenFileReadOnly(fileName); }


        /// <summary>
        /// reads n samples from edfsignal, starting from the current sample position indicator, into buf (edfsignal starts at 0) 
        /// the values are converted to their physical values e.g. microVolts, beats per minute, etc. 
        /// bufsize should be equal to or bigger than sizeof(double[n]) 
        /// the sample position indicator will be increased with the amount of samples read 
        /// returns the amount of samples read (this can be less than n or zero!) 
        /// or -1 in case of an error  
        /// </summary>
        public static double[] edfReadPhysicalSamples(int fileHandle, int signal, int numSamples)
        {
            double[] data = new double[numSamples];
            dll.edfReadPhysicalSamples(fileHandle, signal, numSamples, data);
            return data;
        }


        /// <summary>
        /// reads n samples from edfsignal, starting from the current sample position indicator, into buf (edfsignal starts at 0) 
        /// the values are the "raw" digital values 
        /// bufsize should be equal to or bigger than sizeof(int[n]) 
        /// the sample position indicator will be increased with the amount of samples read 
        /// returns the amount of samples read (this can be less than n or zero!) 
        /// or -1 in case of an error  
        /// </summary>
        public static int[] edfReadDigitalSamples(int fileHandle, int signal, int numSamples)
        {
            int[] data = new int[numSamples];
            dll.edfReadDigitalSamples(fileHandle, signal, numSamples, data);
            return data;
        }


        /// <summary>
        /// The edfseek() function sets the sample position indicator for the edfsignal pointed to by edfsignal. 
        /// The new position, measured in samples, is obtained by adding offset samples to the position specified by whence. 
        /// If whence is set to EDFSEEK_SET, EDFSEEK_CUR, or EDFSEEK_END, the offset is relative to the start of the file, 
        /// the current position indicator, or end-of-file, respectively. 
        /// Returns the current offset. Otherwise, -1 is returned. 
        /// note that every signal has it's own independent sample position indicator and edfseek() affects only one of them 
        /// </summary>
        public static ulong edfSeek(int fileHandle, int signal, ulong offset, int whence) { return dll.edfSeek(fileHandle, signal, offset, whence); }


        /// <summary>
        /// The edftell() function obtains the current value of the sample position indicator for the signal pointed to by edfsignal. 
        /// Returns the current offset. Otherwise, -1 is returned 
        /// note that every signal has it's own independent sample position indicator and edftell() affects only one of them  
        /// </summary>
        public static ulong edfTell(int fileHandle, int signal) { return dll.edfTell(fileHandle, signal); }


        /// <summary>
        /// The edfrewind() function sets the sample position indicator for the edfsignal pointed to by edfsignal to the beginning of the file. 
        /// It is equivalent to: (void) edfseek(int fileHandle, int signal, 0LL, EDFSEEK_SET) 
        /// note that every signal has it's own independent sample position indicator and edfrewind() affects only one of them 
        /// </summary>
        public static void edfRewind(int fileHandle, int signal) { dll.edfRewind(fileHandle, signal); }

        #endregion
        //  ReadFile


        //  WriteFile
        //
        #region WrieFile

        /// <summary>
        /// opens an new file for writing. warning, an already existing file with the same name will be silently overwritten without advance warning!! 
        /// path is a null-terminated string containing the path and name of the file 
        /// filetype must be EDFLIB_FILETYPE_EDFPLUS (1) or EDFLIB_FILETYPE_BDFPLUS (3) 
        /// returns a handle on success, you need this handle for the other functions 
        /// in case of an error it returns a negative number corresponding to one of the following values: 
        /// EDFLIB_MALLOC_ERROR                
        /// EDFLIB_NO_SUCH_FILE_OR_DIRECTORY   
        /// EDFLIB_MAXFILES_REACHED            
        /// EDFLIB_FILE_ALREADY_OPENED         
        /// EDFLIB_NUMBER_OF_SIGNALS_INVALID   
        /// This function is required if you want to write a file 
        /// </summary>
        public static int edfOpenFileWriteOnly(string path, int fileType, int numberOfSignals) { return dll.edfOpenFileWriteOnly(path, fileType, numberOfSignals); }


        /// <summary>
        /// Sets the datarecord duration. The default value is 1 second. 
        /// ATTENTION: the argument "duration" is expressed in units of 10 microSeconds! 
        /// So, if you want to set the datarecord duration to 0.1 second, you must give 
        /// the argument "duration" a value of "10000". 
        /// This function is optional, normally you don't need to change the default value. 
        /// The datarecord duration must be in the range 0.001 to 60 seconds. 
        /// Returns 0 on success, otherwise -1 
        /// This function is NOT REQUIRED but can be called after opening a 
        /// file in writemode and before the first sample write action. 
        /// This function can be used when you want to use a samplerate 
        /// which is not an integer. For example, if you want to use a samplerate of 0.5 Hz, 
        /// set the samplefrequency to 5 Hz and the datarecord duration to 10 seconds, 
        /// or set the samplefrequency to 1 Hz and the datarecord duration to 2 seconds. 
        /// Do not use this function if not necessary. 
        /// </summary>
        public static int  edfSetDatarecordDuration(int fileHandle, int duration) { return dll.edfSetDatarecordDuration(fileHandle, duration); }


        /// <summary>
        /// Sets the number of samples in a datarecord 
        /// By default data record is one secnd of data, so this is usually the sample frequency 
        /// If you are storing data records at different interval, you must do the math before clling this funciton 
        /// Returns 0 on success, otherwise -1 
        /// This function is required for every signal and can be called only after opening a 
        /// file in writemode and before the first sample write action 
        /// </summary>
        public static int edfSetSamplesInDataRecord(int fileHandle, int signal, int samplesInDataRecord) { return dll.edfSetSamplesInDataRecord(fileHandle, signal, samplesInDataRecord); }


        /// <summary>
        /// Sets the maximum physical value of signal edfsignal. (the value of the input of the ADC when the output equals the value of "digital maximum") 
        /// It is the highest value that the equipment is able to record. It does not necessarily mean the signal recorded reaches this level 
        /// Must be un-equal to physical minimum 
        /// Returns 0 on success, otherwise -1 
        /// This function is required for every signal and can be called only after opening a 
        /// file in writemode and before the first sample write action 
        /// </summary>
        public static int edfSetPhysicalMaximum(int fileHandle, int signal, double physicalMax) { return dll.edfSetPhysicalMaximum(fileHandle, signal, physicalMax); }


        /// <summary>
        /// Sets the minimum physical value of signal edfsignal. (the value of the input of the ADC when the output equals the value of "digital minimum") 
        /// It is the lowest value that the equipment is able to record. It does not necessarily mean the signal recorded reaches this level 
        /// Usually this will be (-(phys_max)) 
        /// Must be un-equal to physical maximum 
        /// Returns 0 on success, otherwise -1 
        /// This function is required for every signal and can be called only after opening a 
        /// file in writemode and before the first sample write action 
        /// </summary>
        public static int edfSetPhysicalMinimum(int fileHandle, int signal, double physicalMin) { return dll.edfSetPhysicalMinimum(fileHandle, signal, physicalMin); }


        /// <summary>
        /// Sets the maximum digital value of signal edfsignal. The maximum value is 32767 for EDF+ and 8388607 for BDF+ 
        /// It is the highest value that the equipment is able to record. It does not necessarily mean the signal recorded reaches this level 
        /// Usually it's the extreme output of the ADC 
        /// Must be higher than digital minimum 
        /// Returns 0 on success, otherwise -1 
        /// This function is required for every signal and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetDigitalMaximum(int fileHandle, int signal, int digitalMax) { return dll.edfSetDigitalMaximum(fileHandle, signal, digitalMax); }


        /// <summary>
        /// Sets the minimum digital value of signal edfsignal. The minimum value is -32768 for EDF+ and -8388608 for BDF+ 
        /// It is the lowest value that the equipment is able to record. It does not necessarily mean the signal recorded reaches this level 
        /// Usually it's the extreme output of the ADC 
        /// Usually this will be (-(dig_max + 1)) 
        /// Must be lower than digital maximum 
        /// Returns 0 on success, otherwise -1 
        /// This function is required for every signal and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetDigitalMinimum(int fileHandle, int signal, int digitalMin) { return dll.edfSetDigitalMinimum(fileHandle, signal, digitalMin); }


        /// <summary>
        /// Sets the label (name) of signal edfsignal. ("FP1", "SaO2", etc.) 
        /// label is a pointer to a NULL-terminated ASCII-string containing the label (name) of the signal edfsignal 
        /// Returns 0 on success, otherwise -1 
        /// This function is recommended for every signal when you want to write a file 
        /// and can be called only after opening a file in writemode and before the first sample write action 
        /// </summary>
        public static int edfSetLabel(int fileHandle, int signal, string label) { return dll.edfSetLabel(fileHandle, signal, label); }

        /// <summary>
        /// Sets the prefilter of signal edfsignal ("HP:0.1Hz", "LP:75Hz N:50Hz", etc.). 
        /// prefilter is a pointer to a NULL-terminated ASCII-string containing the prefilter text of the signal edfsignal 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode and before 
        /// the first sample write action 
        /// </summary>
        public static int edfSetPrefilter(int fileHandle, int signal, string prefilter) { return dll.edfSetPrefilter(fileHandle, signal, prefilter); }


        /// <summary>
        /// Sets the transducer of signal edfsignal ("AgAgCl cup electrodes", etc.). 
        /// transducer is a pointer to a NULL-terminated ASCII-string containing the transducer text of the signal edfsignal 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode and before 
        /// the first sample write action 
        /// </summary>
        public static int edfSetTransducer(int fileHandle, int signal, string transducer) { return dll.edfSetTransducer(fileHandle, signal, transducer); }


        /// <summary>
        /// Sets the physical dimension (unit) of signal edfsignal. ("uV", "BPM", "mA", "Degr.", etc.) 
        /// phys_dim is a pointer to a NULL-terminated ASCII-string containing the physical dimension of the signal edfsignal 
        /// Returns 0 on success, otherwise -1 
        /// This function is recommended for every signal when you want to write a file 
        /// and can be called only after opening a file in writemode and before the first sample write action 
        /// </summary>
        public static int edfSetPhysicalDimension(int fileHandle, int signal, string physicalDimension) { return dll.edfSetPhysicalDimension(fileHandle, signal, physicalDimension); }


        /// <summary>
        /// Sets the startdate and starttime. 
        /// year: 1985 - 2084, month: 1 - 12, day: 1 - 31 
        /// hour: 0 - 23, minute: 0 - 59, second: 0 - 59 
        /// If not called, the library will use the system date and time at runtime 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// Note: for anonymization purposes, the consensus is to use 1985-01-01 00:00:00 for the startdate and starttime. 
        /// </summary>
        public static int edfSetStartDatetime(int fileHandle, int year, int month, int day, int hour, int minute, int second) { return dll.edfSetStartDatetime(fileHandle, year, month, day, hour, minute, second); }

        /// <summary>
        /// Sets the subsecond starttime expressed in units of 100 nanoSeconds
        /// Valid range is 0 to 9999999 inclusive. Default is 0
        /// This function is optional and can be called only after opening a file in writemode
        /// and before the first sample write action
        /// Returns 0 on success, otherwise -1
        /// It is strongly recommended to use a maximum resolution of no more than 100 micro-Seconds.
        /// e.g. use 1234000  to set a starttime offset of 0.1234 seconds (instead of 1234567)
        /// in other words, leave the last 3 digits at zero
        /// </summary>
        public static int  edfSetSubsecondStarttime(int fileHandle, int subsecond) { return dll.edfSetSubsecondStarttime(fileHandle, subsecond); }

        /// <summary>
        /// Sets the patientname. patientname is a pointer to a null-terminated ASCII-string. 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetPatientName(int fileHandle, string patientName) { return dll.edfSetPatientName(fileHandle, patientName); }


        /// <summary>
        /// Sets the patientcode. patientcode is a pointer to a null-terminated ASCII-string. 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetPatientCode(int fileHandle, string patientCode) { return dll.edfSetPatientCode(fileHandle, patientCode); }


        /// <summary>
        /// Sets the patient biological gender. 1 is XY, 0 is XX. 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetPatientYChromosome(int fileHandle, int yChromosome) { return dll.edfSetPatientYChromosome(fileHandle, yChromosome); }


        /// <summary>
        /// Sets the birthdate. 
        /// year: 1800 - 3000, month: 1 - 12, day: 1 - 31 
        /// This function is optional 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetPatientBirthdate(int fileHandle, int year, int month, int day) { return dll.edfSetPatientBirthdate(fileHandle, year, month, day); }


        /// <summary>
        /// Sets the additional patientinfo. patient_additional is a pointer to a null-terminated ASCII-string. 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetPatientAdditional(int fileHandle, string patientAdditional) { return dll.edfSetPatientAdditional(fileHandle, patientAdditional); }


        /// <summary>
        /// Sets the admincode. admincode is a pointer to a null-terminated ASCII-string. 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetAdminCode(int fileHandle, string adminCode) { return dll.edfSetAdminCode(fileHandle, adminCode); }


        /// <summary>
        /// Sets the technicians name. technician is a pointer to a null-terminated ASCII-string. 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetTechnician(int fileHandle, string technician) { return dll.edfSetTechnician(fileHandle, technician); }


        /// <summary>
        /// Sets the name of the equipment used during the aquisition. equipment is a pointer to a null-terminated ASCII-string. 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetEquipment(int fileHandle, string equipment) { return dll.edfSetEquipment(fileHandle, equipment); }


        /// <summary>
        /// Sets the additional recordinginfo. recording_additional is a pointer to a null-terminated ASCII-string. 
        /// Returns 0 on success, otherwise -1 
        /// This function is optional and can be called only after opening a file in writemode 
        /// and before the first sample write action 
        /// </summary>
        public static int edfSetRecordingAdditional(int fileHandle, string recordingAdditional) { return dll.edfSetRecordingAdditional(fileHandle, recordingAdditional); }


        /// <summary>
        /// Writes n physical samples (uV, mA, Ohm) from *buf belonging to one signal 
        /// where n is the samplefrequency of that signal. 
        /// The physical samples will be converted to digital samples using the 
        /// values of physical maximum, physical minimum, digital maximum and digital minimum 
        /// The number of samples written is equal to the samplefrequency of the signal 
        /// Size of buf should be equal to or bigger than sizeof(double[samplefrequency]) 
        /// Call this function for every signal in the file. The order is important! 
        /// When there are 4 signals in the file,  the order of calling this function 
        /// must be: signal 0, signal 1, signal 2, signal 3, signal 0, signal 1, signal 2, etc. 
        /// Returns 0 on success, otherwise -1 
        /// </summary>
        public static int edfWritePhysicalSamples(int fileHandle, double[] buffer) { return dll.edfWritePhysicalSamples(fileHandle, buffer); }


        /// <summary>
        /// Writes physical samples (uV, mA, Ohm) from *buf 
        /// buf must be filled with samples from all signals, starting with n samples of signal 0, n samples of signal 1, n samples of signal 2, etc. 
        /// where n is the samplefrequency of that signal. 
        /// buf must be filled with samples from all signals, starting with signal 0, 1, 2, etc. 
        /// one block equals one second 
        /// The physical samples will be converted to digital samples using the 
        /// values of physical maximum, physical minimum, digital maximum and digital minimum 
        /// The number of samples written is equal to the sum of the samplefrequencies of all signals 
        /// Size of buf should be equal to or bigger than sizeof(double) multiplied by the sum of the samplefrequencies of all signals 
        /// Returns 0 on success, otherwise -1 
        /// </summary>
        public static int edfBlockWritePhysicalSamples(int fileHandle, double[] buffer) { return edfBlockWritePhysicalSamples(fileHandle, buffer); }


        /// <summary>
        /// Writes n "raw" digital samples from *buf belonging to one signal 
        /// where n is the samplefrequency of that signal. 
        /// The 16 (or 24 in case of BDF) least significant bits of the sample will be written to the 
        /// file without any conversion. 
        /// The number of samples written is equal to the samplefrequency of the signal 
        /// Size of buf should be equal to or bigger than sizeof(int[samplefrequency]) 
        /// Call this function for every signal in the file. The order is important! 
        /// When there are 4 signals in the file,  the order of calling this function 
        /// must be: signal 0, signal 1, signal 2, signal 3, signal 0, signal 1, signal 2, etc. 
        /// Returns 0 on success, otherwise -1 
        /// </summary>
        public static int edfWriteDigitalSamples(int fileHandle, int[] buffer) { return edfWriteDigitalSamples(fileHandle, buffer); }


        /// <summary>
        /// Writes "raw" digital samples from *buf. 
        /// buf must be filled with samples from all signals, starting with n samples of signal 0, n samples of signal 1, n samples of signal 2, etc. 
        /// where n is the samplefrequency of that signal. 
        /// One block equals one second. 
        /// The 16 (or 24 in case of BDF) least significant bits of the sample will be written to the 
        /// file without any conversion. 
        /// The number of samples written is equal to the sum of the samplefrequencies of all signals. 
        /// Size of buf should be equal to or bigger than sizeof(int) multiplied by the sum of the samplefrequencies of all signals 
        /// Returns 0 on success, otherwise -1 
        /// </summary>
        public static int edfBlockWriteDigitalSamples(int fileHandle, int[] buffer) { return edfBlockWriteDigitalSamples(fileHandle, buffer); }

        #endregion
        //  WriteFile



        /// closes (and in case of writing, finalizes) the file 
        /// returns -1 in case of an error, 0 on success 
        /// this function MUST be called when you are finished reading or writing 
        /// This function is required after reading or writing. Failing to do so will cause 
        /// unnessecary memory usage and in case of writing it will cause a corrupted and incomplete file 
        /// <summary>
        /// 
        /// </summary>
        public static int edfCloseFile(int fileHandle) { return dll.edfCloseFile(fileHandle); }


        /// <summary>
        /// 
        /// </summary>
        public static void edfShutDown() { dll.edfShutDown(); }
    }

    class dll
    {
        const string libname = "EDFfile";

        //  Properties
        //
        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfGetHeaderAsJson(int fileHandle, int size, byte[] headerAsJson);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern ulong edfDataRecoardsInFile(int fileHandle);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSignalsInFile(int fileHandle);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSignalSamplesPerRecordInFile(int fileHandle, int signal);


        //  Read
        //
        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfOpenFileReadOnly(string fileName);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfReadPhysicalSamples(int fileHandle, int signal, int numSamples, double[] buffer);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfReadDigitalSamples(int fileHandle, int signal, int numSamples, int[] buffer);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern ulong edfSeek(int fileHandle, int signal, ulong offset, int whence);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern ulong edfTell(int fileHandle, int signal);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern void edfRewind(int fileHandle, int signal);


        //  Write
        //
        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfOpenFileWriteOnly(string path, int fileType, int numberOfSignals);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetDatarecordDuration(int fileHandle, int duration);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetSamplesInDataRecord(int fileHandle, int signal, int samplesInDataRecord);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPhysicalMaximum(int fileHandle, int signal, double physicalMax);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPhysicalMinimum(int fileHandle, int signal, double physicalMin);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetDigitalMaximum(int fileHandle, int signal, int digitalMax);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetDigitalMinimum(int fileHandle, int signal, int digitalMin);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetLabel(int fileHandle, int signal, string label);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPrefilter(int fileHandle, int signal, string prefilter);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetTransducer(int fileHandle, int signal, string transducer);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPhysicalDimension(int fileHandle, int signal, string physicalDimension);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetStartDatetime(int fileHandle, int year, int month, int day, int hour, int minute, int second);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetSubsecondStarttime(int fileHandle, int subsecond);



        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPatientName(int fileHandle, string patientName); 

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPatientCode(int fileHandle, string patientCode);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPatientYChromosome(int fileHandle, int yChromosome);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPatientBirthdate(int fileHandle, int year, int month, int day);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetPatientAdditional(int fileHandle, string patientAdditional);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetAdminCode(int fileHandle, string adminCode);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetTechnician(int fileHandle, string technician);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetEquipment(int fileHandle, string equipment);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfSetRecordingAdditional(int fileHandle, string recordingAdditional);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfWritePhysicalSamples(int fileHandle, double[] buffer);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfBlockWritePhysicalSamples(int fileHandle, double[] buffer);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfWriteDigitalSamples(int fileHandle, int[] buffer);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfBlockWriteDigitalSamples(int fileHandle, int[] buffer);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int edfCloseFile(int fileHandle);

        [DllImport(libname, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern void edfShutDown();
    }
}
