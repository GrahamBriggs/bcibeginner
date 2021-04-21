// ContecFileReader.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <string>
#include <fstream>
#include <sstream>
#include <vector>
#include <iomanip>

using namespace std;

void CalcBufferLittleEndian(vector<unsigned char> bytes)
{
	//  make 11 bit numbers with one sigh bit from the array of bytes
	for (int i = 1; i < 31; i += 3)
	{
		//  first number, set first 8 bits
		uint16_t num1 = bytes[i];
		//  add three more bits from the next byte
		num1 += ( (bytes[i + 1] & 0x07) * 256 );
		//  sign bit
		int value1 = num1;
		if (bytes[i + 1] & 0x08)
			value1 *= -1;

		//  set the first four bits of the second number using last four bits of second byte
		uint16_t num2 = ( (bytes[i + 1] & 0xF0) >> 4);
		//  set the remaining bits of the second number using the third byte
		num2 += (16* (bytes[i + 2] & 0x7F));
		//  sign bit
		int value2 = num2;
		if (bytes[i + 2] & 0x08)
			value2 *= -1;

		cout << setw(6) << num1 << " " <<  setw(6) << num2 << " ";
	}

	cout << " | " << (int)bytes[31];
	cout << endl;
}



void swapByteOrder(uint16_t& ui)
{
	ui =  ((ui << 8) & 0xFF00) | 		((ui >> 8) & 0x00FF) ;

}




void CalcBufferBigEndian(vector<unsigned char> bytes)
{
	//  make 11 bit numbers with one sigh bit from the array of bytes
	for (int i = 1; i < 31; i += 3)
	{
		//  first number, set first 8 bits
		uint16_t num1 = bytes[i];
		//  add three more bits from the next byte
		num1 += ((bytes[i + 1] & 0x07) * 256);
		//  endian
		swapByteOrder(num1);
		int value1 = num1 >> 4;
		if (bytes[i + 1] & 0x08)
			value1 *= -1;

		//  set the first four bits of the second number using last four bits of second byte
		uint16_t num2 = ((bytes[i + 1] & 0xF0) >> 4);
		//  set the remaining bits of the second number using the third byte
		num2 += (16 * (bytes[i + 2] & 0x7F));
		//  endian
		swapByteOrder(num2);
		//  sign bit
		int value2 = num2 >> 3;
		if (bytes[i + 2] & 0x08)
			value2 *= -1;

		cout << setw(6) << num1 << " " << setw(6) << num2 << " ";
	}

	cout << " | " << (int)bytes[31];
	cout << endl;
}



int main()
{
	ifstream dataFile;
	dataFile.open("../ContecFileReader/ContecDataRaw.txt");

	if (dataFile.is_open())
	{
		string readLine;
		vector<unsigned char> bytes;
		while (!dataFile.eof())
		{
			getline(dataFile, readLine);
			if (readLine.size() != 0)
			{
				int number = 0;
				stringstream ss(readLine); //convert my_string into string stream


				istringstream hex_chars_stream(readLine);
				unsigned int c;
				while (hex_chars_stream >> std::hex >> c)
				{
					bytes.push_back(c);
					if (bytes.size() == 32)
						break;
				}	

				if (bytes.size() == 32)
				{
					CalcBufferBigEndian(bytes);
					bytes.clear();
				}
			}
		}
	}

	getchar();

}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
