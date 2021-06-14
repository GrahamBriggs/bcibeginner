using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public interface IMontage
    {
        string Name { get; }

        IBFSample[] ApplyMontage(IBFSample[] samples, SignalFilter filter, int boardId, int numberOfChannels, int sampleRate);

        string GetChannelName(int channel);
    }


    public class Montage : IMontage
    {
        public Montage()
        {
            Name = "Defacto";
         
        }

        public string Name { get; protected set; }

        public virtual IBFSample[] ApplyMontage(IBFSample[] samples, SignalFilter filter, int boardId, int numberOfChannels, int sampleRate)
        {
            if (filter != null)
                samples = FilterBrainflowSample.FilterChunk(filter, samples, boardId, numberOfChannels, sampleRate);

            return samples;
        }

        public virtual string GetChannelName(int channel)
        {
            return $"EXG{channel}";
        }
    }


    public class BipolarMontage : Montage
    {
        public BipolarMontage() :
            base()
        {
            Name = "Bipolar";
        }

        public override string GetChannelName(int channel)
        {
            switch(channel)
            {
                case 0:
                    return "F3-C3";
                case 1:
                    return "C3-T3";
                case 2:
                    return "T3-O1";
                case 3:
                    return "F4-C4";
                case 4:
                    return "C4-T4";
                case 5:
                    return "T4-O2";
                case 6:
                    return "F3-F4";
                case 7:
                    return "O1-O2";
            }
            return "";
        }


        public override IBFSample[] ApplyMontage(IBFSample[] samples, SignalFilter filter, int boardId, int numberOfChannels, int sampleRate)
        {
           var montagedSignal = new List<IBFSample>();

            foreach ( var nextSample in samples )
            {
                var newSample = new BFSampleImplementation(nextSample);

                newSample.SetExgDataForChannel(0, nextSample.GetExgDataForChannel(1) - nextSample.GetExgDataForChannel(0));
                newSample.SetExgDataForChannel(1, nextSample.GetExgDataForChannel(2) - nextSample.GetExgDataForChannel(1));
                newSample.SetExgDataForChannel(2, nextSample.GetExgDataForChannel(3) - nextSample.GetExgDataForChannel(2));
                //
                newSample.SetExgDataForChannel(3, nextSample.GetExgDataForChannel(5) - nextSample.GetExgDataForChannel(4));
                newSample.SetExgDataForChannel(4, nextSample.GetExgDataForChannel(6) - nextSample.GetExgDataForChannel(5));
                newSample.SetExgDataForChannel(5, nextSample.GetExgDataForChannel(7) - nextSample.GetExgDataForChannel(6));
                // 
                newSample.SetExgDataForChannel(6, nextSample.GetExgDataForChannel(0) - nextSample.GetExgDataForChannel(4));
                newSample.SetExgDataForChannel(7, nextSample.GetExgDataForChannel(7) - nextSample.GetExgDataForChannel(3));

                montagedSignal.Add(newSample);
            }

            samples = montagedSignal.ToArray();

            if (filter != null)
                samples = FilterBrainflowSample.FilterChunk(filter, samples, boardId, numberOfChannels, sampleRate);

            return samples;
        }
    }


    public class SignalMontages
    {
        public void LoadMontages()
        {
            Montages = new Dictionary<string, Montage>();

            var defaultMontage = new Montage();
            Montages.Add(defaultMontage.Name, defaultMontage);

            var bipolar = new BipolarMontage();
            Montages.Add(bipolar.Name, bipolar);

            
        }

        public IEnumerable<string> GetMontageNames()
        {
            return Montages.Keys.ToArray();
        }

        public IMontage GetDefaultMontage()
        {
            if (Montages.Count > 0)
                return Montages.First().Value;
            return null; 
        }


        public IMontage GetMontage(string name)
        {
            if (Montages.ContainsKey(name))
                return Montages[name];

            return null;
        }

        //  The filter collection
        static Dictionary<string, Montage> Montages;
    }
}
