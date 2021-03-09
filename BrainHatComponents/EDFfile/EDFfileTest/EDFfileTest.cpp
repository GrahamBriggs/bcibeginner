// EDFfileTest.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <chrono>
#include <thread>
#include <EDFfile.h>
#include "StringExtensions.h"

using namespace std;

int main()
{

	/*auto fileHandle = edfOpenFileReadOnly("C:/Users/grahambriggs/Documents/OpenBCI_GUI/Recordings/OpenBCI-BDF-2021-03-06_08-56-28.bdf");

	for (int i = 0; i < edfDataRecoardsInFile(fileHandle); i++)
	{
		cout << "Reading " << i << endl;

		for (int j = 0; j < edfSignalsInFile(fileHandle); j++)
		{
			int samplesPerRecord = edfSignalSamplesPerRecordInFile(fileHandle, j);
			double* buffer = new double[samplesPerRecord];

			edfReadPhysicalSamples(fileHandle, j, samplesPerRecord, buffer);

			cout << "Channel " << j << " : ";

			for (int k = 0; k < samplesPerRecord; k++)
			{
				cout << " " << buffer[k];
			}
			cout << endl;

			delete buffer;
		}
	}*/

	for (int number = 0; number < 2; number++)
	{
		auto fileHandle = edfOpenFileWriteOnly(format("TestFile%d.bdf", number).c_str(), 3, 8);
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
			edfSetLabel(fileHandle, i, format("Channel %d", i).c_str());
			edfSetPrefilter(fileHandle, i, format("Prefilter %d", i).c_str());
			edfSetTransducer(fileHandle, i, format("Transducer %d", i).c_str());
			edfSetPhysicalDimension(fileHandle, i, format("Phys%d", i).c_str());
		}

		edfSetStartDatetime(fileHandle, 2021, 03, 07, 12, 13, 14);
		edfSetPatientName(fileHandle, "PatientX");
		edfSetPatientCode(fileHandle, "PatientCodeX");
		edfSetPatientYChromosome(fileHandle, 1);
		edfSetPatientBirthdate(fileHandle, 2021, 03, 07);
		edfSetPatientAdditional(fileHandle, "PatientAdditionalX");
		edfSetAdminCode(fileHandle, "AdminCodeX");
		edfSetTechnician(fileHandle, "TechX");
		edfSetEquipment(fileHandle, format("EquipmentX%d",number).c_str());
		edfSetRecordingAdditional(fileHandle, "MyField");

		//  write three seconds worth of data
		for (int i = 0; i < 1000; i++)
		{
			for (int j = 0; j < numChannels; j++)
			{
				double* fakeData = new double[samplesInDataRecord];
				for (int k = 0; k < samplesInDataRecord; k++)
				{
					fakeData[k] = j + 0.01 * k;
				}

				edfWritePhysicalSamples(fileHandle, fakeData);

				delete fakeData;
			}

			//this_thread::sleep_for(std::chrono::milliseconds(1));
		}


		edfCloseFile(fileHandle);
	}

	this_thread::sleep_for(std::chrono::seconds(2));

	auto readFile1 = edfOpenFileReadOnly("TestFile0.bdf");
	auto readFile2 = edfOpenFileReadOnly("TestFile1.bdf");


	edfCloseFile(readFile1);
	edfCloseFile(readFile2);

	std::cout << "Finshed reading file!\n";

	getchar();


}
