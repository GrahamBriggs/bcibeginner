using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDFfile
{
    public class EdfParamaterStruct
    {
        public string label { get; set; }              /* label (name) of the signal, null-terminated public string */
        public ulong smp_in_file { get; set; }         /* number of samples of this signal in the file */
        public double phys_max { get; set; }               /* physical maximum, usually the maximum input of the ADC */
        public double phys_min { get; set; }               /* physical minimum, usually the minimum input of the ADC */
        public int dig_max { get; set; }                /* digital maximum, usually the maximum output of the ADC, can not not be higher than 32767 for EDF or 8388607 for BDF */
        public int dig_min { get; set; }                /* digital minimum, usually the minimum output of the ADC, can not not be lower than -32768 for EDF or -8388608 for BDF */
        public int smp_in_datarecord { get; set; }      /* number of samples of this signal in a datarecord, if the datarecord has a duration of one second (default), then it equals the samplerate */
        public string physdimension { get; set; }       /* physical dimension (uV, bpm, mA, etc.), null-terminated public string */
        public string prefilter { get; set; }          /* null-terminated public string */
        public string transducer { get; set; }         /* null-terminated public string */
    }


    public class EdfHeaderStruct
    {
        public int handle { get; set; }                        /* a handle (identifier) used to distinguish the different files */
        public int filetype { get; set; }                      /* 0: EDF, 1: EDFplus, 2: BDF, 3: BDFplus, a negative number means an error */
        public int edfsignals { get; set; }                    /* number of EDF signals in the file, annotation channels are NOT included */
        public ulong file_duration { get; set; }                 /* duration of the file expressed in units of 100 nanoSeconds */
        public int startdate_day { get; set; }
        public int startdate_month { get; set; }
        public int startdate_year { get; set; }
        public ulong starttime_subsecond { get; set; }                          /* starttime offset expressed in units of 100 nanoSeconds. Is always less than 10000000 (one second). Only used by EDFplus and BDFplus */
        public int starttime_second { get; set; }
        public int starttime_minute { get; set; }
        public int starttime_hour { get; set; }
        public string patient { get; set; }                                  /* null-terminated public string, contains patientfield of header, is always empty when filetype is EDFPLUS or BDFPLUS */
        public string recording { get; set; }                                /* null-terminated public string, contains recordingfield of header, is always empty when filetype is EDFPLUS or BDFPLUS */
        public string patientcode { get; set; }                              /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public string gender { get; set; }                                   /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public string birthdate { get; set; }                                /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public string patient_name { get; set; }                             /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public string patient_additional { get; set; }                       /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public string admincode { get; set; }                                /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public string technician { get; set; }                               /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public string equipment { get; set; }                                /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public string recording_additional { get; set; }                     /* null-terminated public string, is always empty when filetype is EDF or BDF */
        public ulong datarecord_duration { get; set; }                          /* duration of a datarecord expressed in units of 100 nanoSeconds */
        public ulong datarecords_in_file { get; set; }                          /* number of datarecords in the file */
        public ulong annotations_in_file { get; set; }                          /* number of annotations in the file */
        public List<EdfParamaterStruct> signalparam { get; set; }  /* array of structs which contain the relevant signal parameters */
    }
}
