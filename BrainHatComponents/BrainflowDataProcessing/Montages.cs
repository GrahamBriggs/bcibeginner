using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BrainflowDataProcessing
{
    /// <summary>
    /// Montage Interface
    /// </summary>
    public interface ISignalMontage
    {
        string Name { get; }

        IBFSample[] ApplyMontage(IBFSample[] samples, SignalFilter filter, int boardId, int numberOfChannels, int sampleRate);

        string GetDerivationLabel(int channel);
        string GetDerivationColour(int channel);

        int NumberOfDerivations { get; }
    }


    /// <summary>
    /// Signal derivation channel
    /// a single channel in the derivation
    /// </summary>
    public class SignalDerivationChannel
    {
        public SignalDerivationChannel(int rawChannelNumber, double factor)
        {
            RawChannelNumber = rawChannelNumber;
            Factor = factor;
        }

        public string Label { get; protected set; }
        public int RawChannelNumber { get; protected set; }
        public double Factor { get; protected set; }
    }


    public class SignalDerivation
    {
        public SignalDerivation(string label, string colour)
        {
            _Channels = new List<SignalDerivationChannel>();
            Label = label;
            Colour = colour;
        }

        public string Label { get; protected set; }
        public string Colour { get; protected set; }

        public void AddDerivationChannel(SignalDerivationChannel channel)
        {
            _Channels.Add(channel);
        }

        List<SignalDerivationChannel> _Channels;
        public IEnumerable<SignalDerivationChannel> Channels => _Channels;
    }

    public class SignalMontage : ISignalMontage
    {
        public SignalMontage(string name)
        {
            Derivations = new List<SignalDerivation>();
            Name = name;
         
        }

        public void AddDerivation(SignalDerivation derivation)
        {
            Derivations.Add(derivation);
        }

        public string Name { get; protected set; }

        public virtual IBFSample[] ApplyMontage(IBFSample[] samples, SignalFilter filter, int boardId, int numberOfChannels, int sampleRate)
        {
            var montagedSignal = new List<IBFSample>();

            foreach (var nextSample in samples)
            {
                var newSample = new BFSampleImplementation(nextSample, NumberOfDerivations, 0, 0, 0);

                for(int i = 0; i < Derivations.Count; i++)
                {
                    var result = 0.0;
                    foreach ( var nextChannel in Derivations[i].Channels)
                    {
                        result += nextSample.GetExgDataForChannel(nextChannel.RawChannelNumber) * nextChannel.Factor;
                    }
                    
                    newSample.SetExgDataForChannel(i, result);
                }

                montagedSignal.Add(newSample);
            }

            samples = montagedSignal.ToArray();

            if (filter != null)
                samples = FilterBrainflowSample.FilterChunk(filter, samples, boardId, numberOfChannels, sampleRate);

            return samples;
        }

        public string GetDerivationLabel(int channel)
        {
            if (Derivations.Count > channel)
                return Derivations[channel].Label;

            return "NA";
        }

        public string GetDerivationColour(int channel)
        {
            if (Derivations.Count > channel)
                return Derivations[channel].Colour;

            return null;
        }

        public int NumberOfDerivations => Derivations == null ? 0 : Derivations.Count;

        List<SignalDerivation> Derivations;
    }


 
  


    public class SignalMontages
    {
        public void LoadMontages(string xmlFilePath)
        {
            Montages = new Dictionary<string, SignalMontage>();

            using (var reader = new StreamReader(xmlFilePath))
            {
                var doc = XDocument.Load(reader);

                var montages = doc.Element("brainHatSignalMontages")?.Element("Montages")?.Elements("Montage");
                if (montages == null)
                {
                    throw new Exception("Document does not hae a <Montages> element or any montages.");
                }

                foreach (var nextMontage in montages)
                {
                    var montageName = nextMontage.Element("Name")?.Value;
                    if (montageName == null || Montages.ContainsKey(montageName))
                    {
                        throw new Exception("Filter does not have a name or name is duplicated.");
                    }

                    var montage = new SignalMontage(montageName);

                    var derivations = nextMontage.Elements("Derivation");
                    if (derivations == null)
                    {
                        throw new Exception($"Filter {montageName} does not have any derivations.");
                    }

                    try
                    {
                        foreach (var nextDerivation in derivations)
                        {
                            var derivation = new SignalDerivation(nextDerivation.Element("Label")?.Value, nextDerivation.Element("Color")?.Value);

                            var channels = nextDerivation.Elements("Channel");
                            foreach (var nextChannel in channels)
                            {
                                var rawChannel = int.Parse(nextChannel.Element("Number").Value);
                                var factor = double.Parse(nextChannel.Element("Factor").Value);

                                derivation.AddDerivationChannel(new SignalDerivationChannel(rawChannel, factor));
                                
                            }

                            montage.AddDerivation(derivation);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Filter {montageName} does not have any derivations.", e);
                    }


                    Montages.Add(montage.Name,montage);

                }
            }
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

        public static ISignalMontage MakeDefaultMontage(int channels)
        {
            var signalMontage = new SignalMontage($"Default{channels}");
            for(int i = 0; i < channels; i++)
            {
                var derivation = new SignalDerivation($"EXG {i}",null);
                derivation.AddDerivationChannel(new SignalDerivationChannel(i, 1.0));
                signalMontage.AddDerivation(derivation);
            }

            return signalMontage;
        }


        //  The filter collection
        static Dictionary<string, SignalMontage> Montages;
    }
}
