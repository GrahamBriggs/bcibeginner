// EDFfile.cpp : Defines the exported functions for the DLL.
//

#include <iostream>
#include <map>
#include "EDFfile.h"
#include "edflib.h"
#include "EdfFileHeader.h"


using namespace std;

#define EDF_PARAM_STRUCT_LABELSIZE (17)
#define EDF_PARAM_STRUCT_PHYSDIMSIZE (9)
#define EDF_STRUCT_FULLSTRINGSIZE (81)
#define EDF_STRUCT_GENDERSIZE (16)
#define EDF_STRUCT_BIRTHDAYSIZE (16)

//  helper functions
void remove_padding_trailing_spaces(char* str);
void SetStructString(char* dest, const char* source, int size);

map<int, edf_hdr_struct*> OpenFiles;


//  Get Properties of an open file
//
#pragma region FileProperties


int edfGetHeaderAsJson(int fileHandle, int size, char* headerAsJson)
{
	auto fileOpen = OpenFiles.find(fileHandle);
	if (fileOpen != OpenFiles.end())
	{
		auto header = HeaderAsJson(*fileOpen->second);
        if (headerAsJson != 0x00 && header.length() >= size)
        {
            strcpy(headerAsJson, header.c_str());
        }

        return header.size();
    }
}


/// <summary>
/// Get the number of data records (pages) in the file
/// </summary>
long long edfDataRecoardsInFile(int fileHandle)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen != OpenFiles.end())
    {
        return fileOpen->second->datarecords_in_file;
    }
    return -1;
}


/// <summary>
/// Get the number of signals in the file
/// </summary>
int edfSignalsInFile(int fileHandle)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen != OpenFiles.end())
    {
        return fileOpen->second->edfsignals;
    }
    return -1;
}


/// <summary>
/// Get the number of samples per page for this signal in this file
/// </summary>
int edfSignalSamplesPerRecordInFile(int fileHandle, int signal)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen != OpenFiles.end())
    {
        return (fileOpen->second)->signalparam[signal].smp_in_datarecord;
    }
    return -1;
}

#pragma endregion   
//  File properties


//  EDFLib Read File Functions
//
#pragma region ReadFile
/// <summary>
/// Open File for reading
/// </summary>
int edfOpenFileReadOnly(const char* fileName)
{
    edf_hdr_struct* headerStruct = new edf_hdr_struct;
    auto res = edfopen_file_readonly(fileName, headerStruct, 0);

    if (res == 0)
    {
        OpenFiles[headerStruct->handle] = headerStruct;

        return headerStruct->handle;
    }
    return res;
}


/// <summary>
/// Read physical samples, reads the samples into buffer and advances the read pointer
/// </summary>
int edfReadPhysicalSamples(int fileHandle, int signal, int numSamples, double* buffer)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    return edfread_physical_samples(fileHandle, signal, numSamples, buffer);
}


/// <summary>
/// Read digital samples
/// </summary>
int edfReadDigitalSamples(int fileHandle, int signal, int numSamples, int* buffer)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    return edfread_digital_samples(fileHandle, signal, numSamples, buffer);
}


/// <summary>
/// File seek
/// </summary>
long long edfSeek(int fileHandle, int signal, long long offset, int whence)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    return edfseek(fileHandle, signal, offset, whence);
}


/// <summary>
/// File tell
/// </summary>
long long edfTell(int fileHandle, int signal)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    return edftell(fileHandle, signal);
}


/// <summary>
/// File rewind
/// </summary>
void edfRewind(int fileHandle, int signal)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return;

    edfrewind(fileHandle, signal);
}

#pragma endregion   
//  ReadFile


//  EDF File Write functions
//
#pragma region WriteFile
/// <summary>
/// Open file to write
/// </summary>
int edfOpenFileWriteOnly(const char* path, int fileType, int numberOfSignals)
{
    auto res = edfopen_file_writeonly(path, fileType, numberOfSignals);
    if (res >= 0)
    {
        edf_hdr_struct* fileHeader = new edf_hdr_struct;
        fileHeader->handle = res;
        OpenFiles[res] = fileHeader;
    }
    return res;
}


/// <summary>
/// Data record duration, default is to use 1 second
/// </summary>
int edfSetDatarecordDuration(int fileHandle, int duration)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_datarecord_duration(fileHandle, duration);
    if (res == 0)
    {
        OpenFiles[fileHandle]->datarecord_duration = duration;
    }
    return res;
}


/// <summary>
/// Set number of samples per data record (default 1s = sample frequency
/// </summary>
int edfSetSamplesInDataRecord(int fileHandle, int signal, int samplesInDataRecord)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res =  edf_set_samplefrequency(fileHandle, signal, samplesInDataRecord);
    if (res == 0)
    {
        OpenFiles[fileHandle]->signalparam[signal].smp_in_datarecord = samplesInDataRecord;
    }
    return res;
}


/// <summary>
/// Set signal physical max
/// </summary>
int edfSetPhysicalMaximum(int fileHandle, int signal, double physicalMax)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res= edf_set_physical_maximum(fileHandle, signal, physicalMax);
    if (res == 0)
    {
        OpenFiles[fileHandle]->signalparam[signal].phys_max = physicalMax;
    }
    return res;
}


/// <summary>
/// Set physical minimum
/// </summary>
int edfSetPhysicalMinimum(int fileHandle, int signal, double physicalMin)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    OpenFiles[fileHandle]->signalparam[signal].phys_min;

    auto res = edf_set_physical_minimum(fileHandle, signal, physicalMin);
    if (res == 0)
    {
        OpenFiles[fileHandle]->signalparam[signal].phys_min = physicalMin;
    }
    return res;
}


/// <summary>
/// Set digital max
/// </summary>
int edfSetDigitalMaximum(int fileHandle, int signal, int digitalMax)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_digital_maximum(fileHandle, signal, digitalMax);
    if (res == 0)
    {
        OpenFiles[fileHandle]->signalparam[signal].dig_max = digitalMax;
    }
    return res;
}


/// <summary>
/// Set digital min
/// </summary>
int edfSetDigitalMinimum(int fileHandle, int signal, int digitalMin)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_digital_minimum(fileHandle, signal, digitalMin);
    if (res == 0)
    {
        OpenFiles[fileHandle]->signalparam[signal].dig_min = digitalMin;
    }
    return res;
}


/// <summary>
/// Set label string
/// </summary>
int edfSetLabel(int fileHandle, int signal, const char* label)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_label(fileHandle, signal, label);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->signalparam[signal].label, label, EDF_PARAM_STRUCT_LABELSIZE);
    }
    return res;
}


/// <summary>
/// Set prefilter string
/// </summary>
int edfSetPrefilter(int fileHandle, int signal, const char* prefilter)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_prefilter(fileHandle, signal, prefilter);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->signalparam[signal].prefilter, prefilter, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}


/// <summary>
/// Set transducer string
/// </summary>
int edfSetTransducer(int fileHandle, int signal, const char* transducer)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_transducer(fileHandle, signal, transducer);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->signalparam[signal].transducer, transducer, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}


/// <summary>
/// Set physical dimensions
/// </summary>
int edfSetPhysicalDimension(int fileHandle, int signal, const char* physicalDimension)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_physical_dimension(fileHandle, signal, physicalDimension);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->signalparam[signal].physdimension, physicalDimension, EDF_PARAM_STRUCT_PHYSDIMSIZE);
    }
    return res;
}


/// <summary>
/// Set start time
/// </summary>
int edfSetStartDatetime(int fileHandle, int year, int month, int day, int hour, int minute, int second)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_startdatetime(fileHandle, year, month, day, hour, minute, second);
    if (res == 0)
    {
        OpenFiles[fileHandle]->startdate_year = year;
        OpenFiles[fileHandle]->startdate_month = month;
        OpenFiles[fileHandle]->startdate_day = day;
        OpenFiles[fileHandle]->starttime_hour = hour;
        OpenFiles[fileHandle]->starttime_minute = minute;
        OpenFiles[fileHandle]->starttime_second = second;
    }
    return res;
}


/// <summary>
/// Set patient name string
/// </summary>
int edfSetPatientName(int fileHandle, const char* patientName)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_patientname(fileHandle, patientName);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->patient_name, patientName, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}


/// <summary>
/// Set patient code stirng
int edfSetPatientCode(int fileHandle, const char* patientCode)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_patientcode(fileHandle, patientCode);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->patientcode, patientCode, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}


/// <summary>
/// Set patient gender flag
/// </summary>
int edfSetPatientYChromosome(int fileHandle, int yChromosome)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_gender(fileHandle, yChromosome);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->gender, yChromosome == 1 ? "M": "F", EDF_STRUCT_GENDERSIZE);
    }
    return res;
}


/// <summary>
/// Set patient birthdate
/// </summary>
int edfSetPatientBirthdate(int fileHandle, int year, int month, int day)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_birthdate(fileHandle, year, month, day);
    if (res == 0)
    {
        memset(OpenFiles[fileHandle]->birthdate, 0x00, EDF_STRUCT_BIRTHDAYSIZE);
        sprintf(OpenFiles[fileHandle]->birthdate, "%02i.%02i.%02i%02i", day, month, year / 100, year % 100);
    }
    return res;
}



/// <summary>
/// Set patient additional string
/// </summary>
int edfSetPatientAdditional(int fileHandle, const char* patientAdditional)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_patient_additional(fileHandle, patientAdditional);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->patient_additional, patientAdditional, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}



/// <summary>
/// Set admin code string
/// </summary>
int edfSetAdminCode(int fileHandle, const char* adminCode)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_admincode(fileHandle, adminCode);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->admincode, adminCode, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}



/// <summary>
/// Set technician string
/// </summary>
int edfSetTechnician(int fileHandle, const char* technician)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_technician(fileHandle, technician);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->technician, technician, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}



/// <summary>
/// Set equipment string
/// </summary>
int edfSetEquipment(int fileHandle, const char* equipment)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_equipment(fileHandle, equipment);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->equipment, equipment, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}



/// <summary>
/// Set additional recording info string
/// </summary>
int edfSetRecordingAdditional(int fileHandle, const char* recordingAdditional)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    auto res = edf_set_recording_additional(fileHandle, recordingAdditional);
    if (res == 0)
    {
        SetStructString(OpenFiles[fileHandle]->recording_additional, recordingAdditional, EDF_STRUCT_FULLSTRINGSIZE);
    }
    return res;
}


/// <summary>
/// Write physical samles
/// </summary>
int  edfWritePhysicalSamples(int fileHandle, double* buffer)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    return edfwrite_physical_samples(fileHandle, buffer);
}


/// <summary>
/// Block write physical samples
/// </summary>
int edfBlockWritePhysicalSamples(int fileHandle, double* buffer)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    return edf_blockwrite_physical_samples(fileHandle, buffer);
}



/// <summary>
/// Write digial samples
/// </summary>
int edfWriteDigitalSamples(int fileHandle, int* buffer)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    return edfwrite_digital_samples(fileHandle, buffer);
}


/// <summary>
/// Block write digital samples
/// </summary>
int edfBlockWriteDigitalSamples(int fileHandle, int* buffer)
{
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen == OpenFiles.end())
        return -1;

    return edf_blockwrite_digital_samples(fileHandle, buffer);
}



#pragma endregion 
// WriteFile






/// <summary>
/// Close the file
/// </summary>
int edfCloseFile(int fileHandle)
{
    //  see if this handle exists in our map
    auto fileOpen = OpenFiles.find(fileHandle);
    if (fileOpen != OpenFiles.end())
    {
       return edfclose_file(fileHandle);
       delete fileOpen->second;
       OpenFiles.erase(fileHandle);
       return 0;
    }
    return -1;
}


/// <summary>
/// Shut down the library
/// </summary>
void edfShutDown()
{
    for (auto it = OpenFiles.begin(); it != OpenFiles.end(); ++it)
    {
        edfclose_file(it->second->handle);
        delete it->second;  
    }
}


//  Helper Functions

/// <summary>
/// Remove padding and trailing spaces, as per method used in EDFlib
/// </summary>
void remove_padding_trailing_spaces(char* str)
{
    int i;
    while (str[0] == ' ')
    {
        for (i = 0; ; i++)
        {
            if (str[i] == 0)
                break;
            str[i] = str[i + 1];
        }
    }
    for (i = strlen(str); i > 0; i--)
    {
        if (str[i - 1] == ' ')
            str[i - 1] = 0;
        else
            break;
    }
}

/// <summary>
/// Set a string, and then fix up the char array in the struct
/// </summary>
void SetStructString(char* dest, const char* source, int size)
{
    strncpy(dest, source, size);

    dest[size - 1] = 0;

    remove_padding_trailing_spaces(dest);
}


