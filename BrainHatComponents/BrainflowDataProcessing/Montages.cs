using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public interface ISignalMontage
    {
        string Name { get; }

        IBFSample[] ApplyMontage(IBFSample[] samples, SignalFilter filter, int boardId, int numberOfChannels, int sampleRate);

        string GetChannelName(int channel);

        int GetNumberOfMontageChannels(int boardId);
    }


    public class SignalMontage : ISignalMontage
    {
        public SignalMontage(string name)
        {
            Name = name;
         
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

       public virtual int GetNumberOfMontageChannels(int boardId)
        {
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                case BrainhatBoardIds.CYTON_BOARD:
                    return 8;
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return 16;
                default:
                    return 0;
            }
        }
    }


    public class MonopolarMontage : SignalMontage
    {
        public MonopolarMontage(string name) : base(name)
        {
            Name = "Monopolar";
        }

        public override string GetChannelName(int channel)
        {
            switch (channel)
            {
                case 0:
                case 8:
                    return "F3-C3";
                case 1:
                case 9:
                    return "C3-T3";
                case 2:
                case 10:
                    return "T3-O1";
                case 3:
                case 11:
                    return "F4-C4";
                case 4:
                case 12:
                    return "C4-T4";
                case 5:
                case 13:
                    return "T4-O2";
                case 6:
                case 14:
                    return "F3-F4";
                case 7:
                case 15:
                    return "O1-O2";
            }
            return "";
        }



    }

    public class BipolarMontage : SignalMontage
    {
        public BipolarMontage() :
            base("")
        {
            Name = "Bipolar";
        }

        public override string GetChannelName(int channel)
        {
            switch(channel)
            {
                case 0:
                case 8:
                    return "F3-C3";
                case 1:
                case 9:
                    return "C3-T3";
                case 2:
                case 10:
                    return "T3-O1";
                case 3:
                case 11:
                    return "F4-C4";
                case 4:
                case 12:
                    return "C4-T4";
                case 5:
                case 13:
                    return "T4-O2";
                case 6:
                case 14:
                    return "F3-F4";
                case 7:
                case 15:
                    return "O1-O2";
            }
            return "";
        }


        public override int GetNumberOfMontageChannels(int boardId)
        {
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                case BrainhatBoardIds.CYTON_BOARD:
                    return 8;
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return 16;
                default:
                    return 0;
            }
        }

        public override IBFSample[] ApplyMontage(IBFSample[] samples, SignalFilter filter, int boardId, int numberOfChannels, int sampleRate)
        {
           var montagedSignal = new List<IBFSample>();

            foreach ( var nextSample in samples )
            {
                var newSample = new BFSampleImplementation(nextSample);

                int boardIndex = numberOfChannels / 8;
                for (int i = 0; i < boardIndex; i++)
                {
                    newSample.SetExgDataForChannel(0+i, nextSample.GetExgDataForChannel(1+i) - nextSample.GetExgDataForChannel(0+i));
                    newSample.SetExgDataForChannel(1+i, nextSample.GetExgDataForChannel(2+i) - nextSample.GetExgDataForChannel(1+i));
                    newSample.SetExgDataForChannel(2+i, nextSample.GetExgDataForChannel(3+i) - nextSample.GetExgDataForChannel(2+i));
                    //
                    newSample.SetExgDataForChannel(3+i, nextSample.GetExgDataForChannel(5+i) - nextSample.GetExgDataForChannel(4+i));
                    newSample.SetExgDataForChannel(4+i, nextSample.GetExgDataForChannel(6+i) - nextSample.GetExgDataForChannel(5+i));
                    newSample.SetExgDataForChannel(5+i, nextSample.GetExgDataForChannel(7+i) - nextSample.GetExgDataForChannel(6+i));
                    // 
                    newSample.SetExgDataForChannel(6+i, nextSample.GetExgDataForChannel(4+i) - nextSample.GetExgDataForChannel(0+i));
                    newSample.SetExgDataForChannel(7+i, nextSample.GetExgDataForChannel(7+i) - nextSample.GetExgDataForChannel(3+i));
                }

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
        public void LoadMontages(string defaultMontageName)
        {
            Montages = new Dictionary<string, SignalMontage>();

            var defaultMontage = new SignalMontage(defaultMontageName);
            Montages.Add(defaultMontage.Name, defaultMontage);

            var bipolar = new BipolarMontage();
            Montages.Add(bipolar.Name, bipolar);

            var monopolar = new MonopolarMontage("");
            Montages.Add(monopolar.Name, monopolar);
        }

        public IEnumerable<string> GetMontageNames()
        {
            return Montages.Keys.ToArray();
        }

        public ISignalMontage GetDefaultMontage()
        {
            if (Montages.Count > 0)
                return Montages.First().Value;
            return null; 
        }


        public ISignalMontage GetMontage(string name)
        {
            if (Montages.ContainsKey(name))
                return Montages[name];

            return null;
        }

        //  The filter collection
        static Dictionary<string, SignalMontage> Montages;
    }
}
