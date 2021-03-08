#pragma once


// defined with this macro as being exported.
#ifdef _WIN32
#define SHARED_EXPORT __declspec(dllexport)
#define CALLING_CONVENTION __cdecl
#else
#define SHARED_EXPORT __attribute__ ((visibility ("default")))
#define CALLING_CONVENTION
#endif


#ifdef __cplusplus
extern "C" {
#endif



/*  Properties of an open file */

SHARED_EXPORT int CALLING_CONVENTION edfGetHeaderAsJson(int fileHandle, int size, char* headerAsJson );

/* Number of data records (pages) in the file */
SHARED_EXPORT long long CALLING_CONVENTION edfDataRecoardsInFile(int fileHandle);
/* Number of signals in the file */
SHARED_EXPORT int CALLING_CONVENTION edfSignalsInFile(int fileHandle);
/* Number of samples per page for this signal */
SHARED_EXPORT int CALLING_CONVENTION edfSignalSamplesPerRecordInFile(int fileHandle, int signal);



/* opens an existing file for reading */
/* path is a null-terminated string containing the path to the file */
/* hdr is a pointer to an edf_hdr_struct, all fields in this struct will be overwritten */
/* the edf_hdr_struct will be filled with all the relevant header- and signalinfo/parameters */
/* read_annotations must have one of the following values:      */
/*   EDFLIB_DO_NOT_READ_ANNOTATIONS      annotations will not be read (this saves time when opening a very large EDFplus or BDFplus file */
/*   EDFLIB_READ_ANNOTATIONS             annotations will be read immediately, stops when an annotation has */
/*                                       been found which contains the description "Recording ends"         */
/*   EDFLIB_READ_ALL_ANNOTATIONS         all annotations will be read immediately                           */
/* returns 0 on success, in case of an error it returns -1 and an errorcode will be set in the member "filetype" of struct edf_hdr_struct */
/* This function is required if you want to read a file */
SHARED_EXPORT int CALLING_CONVENTION edfOpenFileReadOnly(const char* fileName);


/* reads n samples from edfsignal, starting from the current sample position indicator, into buf (edfsignal starts at 0) */
/* the values are converted to their physical values e.g. microVolts, beats per minute, etc. */
/* bufsize should be equal to or bigger than sizeof(double[n]) */
/* the sample position indicator will be increased with the amount of samples read */
/* returns the amount of samples read (this can be less than n or zero!) */
/* or -1 in case of an error */
SHARED_EXPORT int CALLING_CONVENTION edfReadPhysicalSamples(int fileHandle, int signal, int numSamples, double* buffer  );


/* reads n samples from edfsignal, starting from the current sample position indicator, into buf (edfsignal starts at 0) */
/* the values are the "raw" digital values */
/* bufsize should be equal to or bigger than sizeof(int[n]) */
/* the sample position indicator will be increased with the amount of samples read */
/* returns the amount of samples read (this can be less than n or zero!) */
/* or -1 in case of an error */
SHARED_EXPORT int CALLING_CONVENTION edfReadDigitalSamples(int fileHandle, int signal, int numSamples, int* buffer);



/* The edfseek() function sets the sample position indicator for the edfsignal pointed to by edfsignal. */
/* The new position, measured in samples, is obtained by adding offset samples to the position specified by whence. */
/* If whence is set to EDFSEEK_SET, EDFSEEK_CUR, or EDFSEEK_END, the offset is relative to the start of the file, */
/* the current position indicator, or end-of-file, respectively. */
/* Returns the current offset. Otherwise, -1 is returned. */
/* note that every signal has it's own independent sample position indicator and edfseek() affects only one of them */
SHARED_EXPORT long long CALLING_CONVENTION edfSeek(int fileHandle, int signal, long long offset, int whence);


/* The edftell() function obtains the current value of the sample position indicator for the signal pointed to by edfsignal. */
/* Returns the current offset. Otherwise, -1 is returned */
/* note that every signal has it's own independent sample position indicator and edftell() affects only one of them */
SHARED_EXPORT long long CALLING_CONVENTION edfTell(int fileHandle, int signal);


/* The edfrewind() function sets the sample position indicator for the edfsignal pointed to by edfsignal to the beginning of the file. */
/* It is equivalent to: (void) edfseek(int fileHandle, int signal, 0LL, EDFSEEK_SET) */
/* note that every signal has it's own independent sample position indicator and edfrewind() affects only one of them */
SHARED_EXPORT void CALLING_CONVENTION edfRewind(int fileHandle, int signal);


/* Fills the edf_annotation_struct with the annotation n, returns 0 on success, otherwise -1 */
/* The string that describes the annotation/event is encoded in UTF-8 */
/* To obtain the number of annotations in a file, check edf_hdr_struct -> annotations_in_file. */
/* returns 0 on success or -1 in case of an error */
// TODO
//  int edf_get_annotation(int fileHandle, int n, struct edf_annotation_struct* annot);



/* opens an new file for writing. warning, an already existing file with the same name will be silently overwritten without advance warning!! */
/* path is a null-terminated string containing the path and name of the file */
/* filetype must be EDFLIB_FILETYPE_EDFPLUS (1) or EDFLIB_FILETYPE_BDFPLUS (3) */
/* returns a handle on success, you need this handle for the other functions */
/* in case of an error it returns a negative number corresponding to one of the following values: */
/* EDFLIB_MALLOC_ERROR                */
/* EDFLIB_NO_SUCH_FILE_OR_DIRECTORY   */
/* EDFLIB_MAXFILES_REACHED            */
/* EDFLIB_FILE_ALREADY_OPENED         */
/* EDFLIB_NUMBER_OF_SIGNALS_INVALID   */
/* This function is required if you want to write a file */
SHARED_EXPORT int CALLING_CONVENTION edfOpenFileWriteOnly(const char* path, int fileType, int numberOfSignals);


/* Sets the datarecord duration. The default value is 1 second. */
/* ATTENTION: the argument "duration" is expressed in units of 10 microSeconds! */
/* So, if you want to set the datarecord duration to 0.1 second, you must give */
/* the argument "duration" a value of "10000". */
/* This function is optional, normally you don't need to change the default value. */
/* The datarecord duration must be in the range 0.001 to 60 seconds. */
/* Returns 0 on success, otherwise -1 */
/* This function is NOT REQUIRED but can be called after opening a */
/* file in writemode and before the first sample write action. */
/* This function can be used when you want to use a samplerate */
/* which is not an integer. For example, if you want to use a samplerate of 0.5 Hz, */
/* set the samplefrequency to 5 Hz and the datarecord duration to 10 seconds, */
/* or set the samplefrequency to 1 Hz and the datarecord duration to 2 seconds. */
/* Do not use this function if not necessary. */
SHARED_EXPORT int CALLING_CONVENTION  edfSetDatarecordDuration(int fileHandle, int duration);




/* Sets the number of samples in a datarecord */
/* By default data record is one secnd of data, so this is usually the sample frequency */
/* If you are storing data records at different interval, you must do the math before clling this funciton */
/* Returns 0 on success, otherwise -1 */
/* This function is required for every signal and can be called only after opening a */
/* file in writemode and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetSamplesInDataRecord(int fileHandle, int signal, int samplesInDataRecord);



/* Sets the maximum physical value of signal edfsignal. (the value of the input of the ADC when the output equals the value of "digital maximum") */
/* It is the highest value that the equipment is able to record. It does not necessarily mean the signal recorded reaches this level */
/* Must be un-equal to physical minimum */
/* Returns 0 on success, otherwise -1 */
/* This function is required for every signal and can be called only after opening a */
/* file in writemode and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPhysicalMaximum(int fileHandle, int signal, double physicalMax);



/* Sets the minimum physical value of signal edfsignal. (the value of the input of the ADC when the output equals the value of "digital minimum") */
/* It is the lowest value that the equipment is able to record. It does not necessarily mean the signal recorded reaches this level */
/* Usually this will be (-(phys_max)) */
/* Must be un-equal to physical maximum */
/* Returns 0 on success, otherwise -1 */
/* This function is required for every signal and can be called only after opening a */
/* file in writemode and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPhysicalMinimum(int fileHandle, int signal, double physicalMin);



/* Sets the maximum digital value of signal edfsignal. The maximum value is 32767 for EDF+ and 8388607 for BDF+ */
/* It is the highest value that the equipment is able to record. It does not necessarily mean the signal recorded reaches this level */
/* Usually it's the extreme output of the ADC */
/* Must be higher than digital minimum */
/* Returns 0 on success, otherwise -1 */
/* This function is required for every signal and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetDigitalMaximum(int fileHandle, int signal, int digitalMax);



/* Sets the minimum digital value of signal edfsignal. The minimum value is -32768 for EDF+ and -8388608 for BDF+ */
/* It is the lowest value that the equipment is able to record. It does not necessarily mean the signal recorded reaches this level */
/* Usually it's the extreme output of the ADC */
/* Usually this will be (-(dig_max + 1)) */
/* Must be lower than digital maximum */
/* Returns 0 on success, otherwise -1 */
/* This function is required for every signal and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetDigitalMinimum(int fileHandle, int signal, int digitalMin);




/* Sets the label (name) of signal edfsignal. ("FP1", "SaO2", etc.) */
/* label is a pointer to a NULL-terminated ASCII-string containing the label (name) of the signal edfsignal */
/* Returns 0 on success, otherwise -1 */
/* This function is recommended for every signal when you want to write a file */
/* and can be called only after opening a file in writemode and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetLabel(int fileHandle, int signal, const char* label);


/* Sets the prefilter of signal edfsignal ("HP:0.1Hz", "LP:75Hz N:50Hz", etc.). */
/* prefilter is a pointer to a NULL-terminated ASCII-string containing the prefilter text of the signal edfsignal */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode and before */
/* the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPrefilter(int fileHandle, int signal, const char* prefilter);




/* Sets the transducer of signal edfsignal ("AgAgCl cup electrodes", etc.). */
/* transducer is a pointer to a NULL-terminated ASCII-string containing the transducer text of the signal edfsignal */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode and before */
/* the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetTransducer(int fileHandle, int signal, const char* transducer);


/* Sets the physical dimension (unit) of signal edfsignal. ("uV", "BPM", "mA", "Degr.", etc.) */
/* phys_dim is a pointer to a NULL-terminated ASCII-string containing the physical dimension of the signal edfsignal */
/* Returns 0 on success, otherwise -1 */
/* This function is recommended for every signal when you want to write a file */
/* and can be called only after opening a file in writemode and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPhysicalDimension(int fileHandle, int signal, const char* physicalDimension);




/* Sets the startdate and starttime. */
/* year: 1985 - 2084, month: 1 - 12, day: 1 - 31 */
/* hour: 0 - 23, minute: 0 - 59, second: 0 - 59 */
/* If not called, the library will use the system date and time at runtime */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
/* Note: for anonymization purposes, the consensus is to use 1985-01-01 00:00:00 for the startdate and starttime. */
SHARED_EXPORT int CALLING_CONVENTION edfSetStartDatetime(int fileHandle, int year, int month, int day, int hour, int minute, int second);


/* Sets the patientname. patientname is a pointer to a null-terminated ASCII-string. */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPatientName(int fileHandle, const char* patientName);



/* Sets the patientcode. patientcode is a pointer to a null-terminated ASCII-string. */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPatientCode(int fileHandle, const char* patientCode);



/* Sets the patient biological gender. 1 is XY, 0 is XX. */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPatientYChromosome(int fileHandle, int yChromosome);




/* Sets the birthdate. */
/* year: 1800 - 3000, month: 1 - 12, day: 1 - 31 */
/* This function is optional */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPatientBirthdate(int fileHandle, int year, int month, int day);



/* Sets the additional patientinfo. patient_additional is a pointer to a null-terminated ASCII-string. */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetPatientAdditional(int fileHandle, const char* patientAdditional);



/* Sets the admincode. admincode is a pointer to a null-terminated ASCII-string. */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetAdminCode(int fileHandle, const char* adminCode);



/* Sets the technicians name. technician is a pointer to a null-terminated ASCII-string. */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetTechnician(int fileHandle, const char* technician);



/* Sets the name of the equipment used during the aquisition. equipment is a pointer to a null-terminated ASCII-string. */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetEquipment(int fileHandle, const char* equipment);



/* Sets the additional recordinginfo. recording_additional is a pointer to a null-terminated ASCII-string. */
/* Returns 0 on success, otherwise -1 */
/* This function is optional and can be called only after opening a file in writemode */
/* and before the first sample write action */
SHARED_EXPORT int CALLING_CONVENTION edfSetRecordingAdditional(int fileHandle, const char* recordingAdditional);


/* Writes n physical samples (uV, mA, Ohm) from *buf belonging to one signal */
/* where n is the samplefrequency of that signal. */
/* The physical samples will be converted to digital samples using the */
/* values of physical maximum, physical minimum, digital maximum and digital minimum */
/* The number of samples written is equal to the samplefrequency of the signal */
/* Size of buf should be equal to or bigger than sizeof(double[samplefrequency]) */
/* Call this function for every signal in the file. The order is important! */
/* When there are 4 signals in the file,  the order of calling this function */
/* must be: signal 0, signal 1, signal 2, signal 3, signal 0, signal 1, signal 2, etc. */
/* Returns 0 on success, otherwise -1 */
SHARED_EXPORT int CALLING_CONVENTION edfWritePhysicalSamples(int fileHandle, double* buffer);


/* Writes physical samples (uV, mA, Ohm) from *buf */
/* buf must be filled with samples from all signals, starting with n samples of signal 0, n samples of signal 1, n samples of signal 2, etc. */
/* where n is the samplefrequency of that signal. */
/* buf must be filled with samples from all signals, starting with signal 0, 1, 2, etc. */
/* one block equals one second */
/* The physical samples will be converted to digital samples using the */
/* values of physical maximum, physical minimum, digital maximum and digital minimum */
/* The number of samples written is equal to the sum of the samplefrequencies of all signals */
/* Size of buf should be equal to or bigger than sizeof(double) multiplied by the sum of the samplefrequencies of all signals */
/* Returns 0 on success, otherwise -1 */
SHARED_EXPORT int CALLING_CONVENTION edfBlockWritePhysicalSamples(int fileHandle, double* buffer);



/* Writes n "raw" digital samples from *buf belonging to one signal */
/* where n is the samplefrequency of that signal. */
/* The 16 (or 24 in case of BDF) least significant bits of the sample will be written to the */
/* file without any conversion. */
/* The number of samples written is equal to the samplefrequency of the signal */
/* Size of buf should be equal to or bigger than sizeof(int[samplefrequency]) */
/* Call this function for every signal in the file. The order is important! */
/* When there are 4 signals in the file,  the order of calling this function */
/* must be: signal 0, signal 1, signal 2, signal 3, signal 0, signal 1, signal 2, etc. */
/* Returns 0 on success, otherwise -1 */
SHARED_EXPORT int CALLING_CONVENTION edfWriteDigitalSamples(int fileHandle, int* buffer);


/* Writes "raw" digital samples from *buf. */
/* buf must be filled with samples from all signals, starting with n samples of signal 0, n samples of signal 1, n samples of signal 2, etc. */
/* where n is the samplefrequency of that signal. */
/* One block equals one second. */
/* The 16 (or 24 in case of BDF) least significant bits of the sample will be written to the */
/* file without any conversion. */
/* The number of samples written is equal to the sum of the samplefrequencies of all signals. */
/* Size of buf should be equal to or bigger than sizeof(int) multiplied by the sum of the samplefrequencies of all signals */
/* Returns 0 on success, otherwise -1 */
SHARED_EXPORT int CALLING_CONVENTION edfBlockWriteDigitalSamples(int fileHandle, int* buffer);






/* closes (and in case of writing, finalizes) the file */
/* returns -1 in case of an error, 0 on success */
/* this function MUST be called when you are finished reading or writing */
/* This function is required after reading or writing. Failing to do so will cause */
/* unnessecary memory usage and in case of writing it will cause a corrupted and incomplete file */
SHARED_EXPORT int CALLING_CONVENTION edfCloseFile(int fileHandle);


/*  Shut down the library, will close any open files */
SHARED_EXPORT void CALLING_CONVENTION edfShutDown();


#ifdef __cplusplus
}
#endif
