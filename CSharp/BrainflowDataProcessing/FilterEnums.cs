using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    /// <summary>
    /// BandStopRanges class
    /// </summary>
    /*
     public enum BandStopRanges
    {
        Sixty(60.0d),
        Fifty(50.0d),
        None(null);
    
        private Double freq;
    
        private static BandStopRanges[] vals = values();
     
        BandStopRanges(Double freq) {
            this.freq = freq;
        }
     
        public Double getFreq() {
            return freq;
        }
    
        public BandStopRanges next()
        {
            return vals[(this.ordinal() + 1) % vals.length];
        }
    
        public String getDescr() {
            if (freq == null) {
                return "None";
            }
            return freq.intValue() + "Hz";
        }
    }
     */


    //  Band Stop Range Item
    public class BandStopRange
    {
        public double? Frequency;

        public bool IsValid => Frequency.HasValue;

        public override string ToString()
        {
            if (IsValid)
            {
                return $"{Frequency.Value.ToString("N0")}";
            }
            else
            {
                return "NULL";
            }
        }
    }

    public enum BandStopRangeEnum
    {
        Sixty,
        Fifty,
        None,
    }

    // Band Stop Range Collection
    public class BandStopRanges
    {
        public BandStopRange next()
        {
            CurrentRange++;
            if(CurrentRange >= Ranges.Count)
                CurrentRange = 0;

            return Ranges[CurrentRange];
        }

        public double getFreq()
        {
            return Ranges[CurrentRange].Frequency.Value;
        }

        public string getDescr()
        {
            return Ranges[CurrentRange].ToString();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BandStopRanges()
        {
            Ranges = new List<BandStopRange>()
            {
               new BandStopRange() { Frequency = 60.0 },
                new BandStopRange() { Frequency = 50.0 },
                new BandStopRange() { Frequency = null }
            };

            CurrentRange = 0;
        }

        public BandStopRange SetRange(BandStopRangeEnum range)
        {
            CurrentRange = (int)range;
            return Ranges[CurrentRange];
        }

        public BandStopRange GetRange()
        {
            return Ranges[CurrentRange];
        }

        int CurrentRange { get; set; }
        List<BandStopRange> Ranges { get; set; }
    }



    /// <summary>
    /// BandPassRanges
    /// </summary>
    /*
    public enum BandPassRanges
    {
        FiveToFifty(5.0d, 50.0d),
        SevenToThirteen(7.0d, 13.0d),
        FifteenToFifty(15.0d, 50.0d),
        OneToFifty(1.0d, 50.0d),
        OneToHundred(1.0d, 100.0d),
        None(null, null);
    
        private Double start;
        private Double stop;
    
        private static BandPassRanges[] vals = values();
     
        BandPassRanges(Double start, Double stop) {
            this.start = start;
            this.stop = stop;
        }
     
        public Double getStart() {
            return start;
        }
    
        public Double getStop() {
            return stop;
        }
    
        public BandPassRanges next()
        {
            return vals[(this.ordinal() + 1) % vals.length];
        }
    
        public String getDescr() {
            if ((start == null) || (stop == null)) {
                return "None";
            }
            return start.intValue() + "-" + stop.intValue() + "Hz";
        }
    }
     */


    //  Band Pass Range Item
    public class BandPassRange
    {
        public double? Start { get; set; }
        public double? Stop { get; set; }

        public bool IsValid => Start.HasValue && Stop.HasValue;

        public override string ToString()
        {
            if (IsValid)
            {
                return $"{Start.Value.ToString("N0")} to {Stop.Value.ToString("N0")}";
            }
            else
            {
                return "NULL";
            }
        }
    }

    public enum BandPassRangesEnum
    {
        iveToFifty,
        SevenToThirteen,
        FifteenToFifty,
        OneToFifty,
        OneToHundred,
        None,
    }

    public class BandPassRanges
    {
        public double getStart()
        {
            return Ranges[CurrentRange].Start.Value;
        }


        public double getStop()
        {
            return Ranges[CurrentRange].Stop.Value;
        }


        public BandPassRange next()
        {
            CurrentRange++;
            if (CurrentRange >= Ranges.Count)
                CurrentRange = 0;
            return Ranges[CurrentRange];
        }

        public string getDescr()
        {
            return Ranges[CurrentRange].ToString();
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public BandPassRanges()
        {
            Ranges = new List<BandPassRange>()
            {
                new BandPassRange() { Start = 5.0, Stop = 50.0 },
                new BandPassRange(){ Start = 7.0, Stop = 13.0 },
                new BandPassRange(){ Start = 15.0, Stop = 50.0 },
                new BandPassRange(){ Start = 1.0, Stop = 50.0 },
                new BandPassRange(){ Start = 1.0, Stop = 100.0 },
                new BandPassRange(){ Start = null, Stop = null },
            };
            CurrentRange = 0;
        }


        public BandPassRange SetRange(BandPassRangesEnum range)
        {
            CurrentRange = (int)range;
            return Ranges[CurrentRange];
        }

        public BandPassRange GetRange()
        {
            return Ranges[CurrentRange];
        }


        private List<BandPassRange> Ranges { get; set; }
        private int CurrentRange = 0;
    }


  

}

