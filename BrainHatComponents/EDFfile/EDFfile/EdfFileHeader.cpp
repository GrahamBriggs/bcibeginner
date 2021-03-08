#include "json.hpp"
#include "EdfFileHeader.h"


using namespace nlohmann;

std::string HeaderAsJson(edf_hdr_struct& header)
{
	json hdr;
	hdr["handle"] = header.handle;
	hdr["filetype"] = header.filetype;
	hdr["edfsignals"] = header.edfsignals;
	hdr["file_duration"] = header.file_duration;
	hdr["startdate_day"] = header.startdate_day;
	hdr["startdate_month"] = header.startdate_month;
	hdr["startdate_year"] = header.startdate_year;
	hdr["starttime_subsecond"] = header.starttime_subsecond;
	hdr["starttime_second"] = header.starttime_second;
	hdr["starttime_minute"] = header.starttime_minute;
	hdr["starttime_hour"] = header.starttime_hour;
	hdr["patient"] = header.patient;
	hdr["recording"] = header.recording;
	hdr["patientcode"] = header.patientcode;
	hdr["gender"] = header.gender;
	hdr["birthdate"] = header.birthdate;
	hdr["patient_name"] = header.patient_name;
	hdr["patient_additional"] = header.patient_additional;
	hdr["admincode"] = header.admincode;
	hdr["technician"] = header.technician;
	hdr["equipment"] = header.equipment;
	hdr["recording_additional"] = header.recording_additional;
	hdr["datarecord_duration"] = header.datarecord_duration;
	hdr["datarecords_in_file"] = header.datarecords_in_file;
	hdr["annotations_in_file"] = header.annotations_in_file;

	auto paramStruct = json::array();
	for (int i = 0; i < header.edfsignals; i++)
	{
		json param;
		param["label"] = header.signalparam[i].label;
		param["smp_in_file"] = header.signalparam[i].smp_in_file;
		param["phys_max"] = header.signalparam[i].phys_max;
		param["phys_min"] = header.signalparam[i].phys_min;
		param["dig_max"] = header.signalparam[i].dig_max;
		param["dig_min"] = header.signalparam[i].dig_min;
		param["smp_in_datarecord"] = header.signalparam[i].smp_in_datarecord;
		param["physdimension"] = header.signalparam[i].physdimension;
		param["prefilter"] = header.signalparam[i].prefilter;
		param["transducer"] = header.signalparam[i].transducer;

		paramStruct.push_back(param);
	}

	hdr["signalparam"] = paramStruct;
	
	return hdr.dump();
}
