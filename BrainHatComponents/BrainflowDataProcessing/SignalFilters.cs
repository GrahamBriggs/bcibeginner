﻿using brainflow;
using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace BrainflowDataProcessing
{
    /// <summary>
    /// Signal filter function base class
    /// </summary>
    public abstract class SignalFilterFunction
    {
        public SignalFilterFunction(object methodObject, MethodInfo method, object[] parameters)
        {
            MethodObject = methodObject;
            Method = method;
            Parameters = parameters;
        }

        public abstract double[] RunFilterFunction(double[] data, int sampleRate);
                
        protected object MethodObject;
        protected MethodInfo Method;
        protected object[] Parameters;
    }


    /// <summary>
    /// Signal filter function with dynamic parameters of data[] and sample rate
    /// </summary>
    public class SignalFilterFunctionDataSampleRate : SignalFilterFunction
    {
        public SignalFilterFunctionDataSampleRate(object methodObject, MethodInfo method, object[] parameters)
            : base(methodObject, method, parameters)
        {

        }

        public override double[] RunFilterFunction(double[] data, int sampleRate)
        {
            Parameters[0] = data;
            Parameters[1] = sampleRate;

            return (double[])Method.Invoke(MethodObject, Parameters);
        }
    }


    /// <summary>
    /// Signal filter function with dynamic parameter of data only
    /// </summary>
    public class SignalFilterFunctionData : SignalFilterFunction
    {
        public SignalFilterFunctionData(object methodObject, MethodInfo method, object[] parameters)
            : base(methodObject, method, parameters)
        {

        }

        public override double[] RunFilterFunction(double[] data, int sampleRate)
        {
            Parameters[0] = data;

            return (double[])Method.Invoke(MethodObject, Parameters);
        }
    }


    /// <summary>
    /// Signal filter
    /// </summary>
    public class SignalFilter
    {
        public double[] ApplyFilter(double[] data, int sampleRate)
        {
            foreach (var nextFilterFunction in FilterFunctions)
            {
                data = nextFilterFunction.RunFilterFunction(data, sampleRate);
            }

            return data;
        }

        public void AddFunction(SignalFilterFunction function)
        {
            FilterFunctions.Add(function);
        }

        public SignalFilter(string name)
        {
            Name = name;
            FilterFunctions = new List<SignalFilterFunction>();
        }

        public string Name { get; private set; }

        List<SignalFilterFunction> FilterFunctions;

    }


    /// <summary>
    /// Signal filters
    /// </summary>
    public class SignalFilters
    {
        /// <summary>
        /// Load filter definitions from XML file
        /// </summary>
        public void LoadSignalFilters(string xmlFilePath)
        {
            try
            {
                Filters.Clear();

                using (var reader = new StreamReader(xmlFilePath))
                {
                    var doc = XDocument.Load(reader);

                    var filters = doc.Element("brainHatSignalFilters")?.Element("Filters")?.Elements("Filter");
                    if (filters == null)
                    {
                        throw new Exception("Document does not hae a <Filters> element.");
                    }

                    foreach (var nextFilter in filters)
                    {
                        var filterName = nextFilter.Element("Name")?.Value;
                        if (filterName == null || Filters.ContainsKey(filterName))
                        {
                            throw new Exception("Filter does not have a name or name is duplicated.");
                        }

                        var newFilter = new SignalFilter(filterName);

                        var functions = nextFilter.Element("Functions")?.Elements("Function");
                        if (functions == null)
                        {
                            throw new Exception($"Filter {filterName} does not have any functions.");
                        }

                        foreach (var nextFunction in nextFilter.Element("Functions")?.Elements("Function"))
                        {
                            //  get function name from XML
                            var functionName = nextFunction.Attribute("Name")?.Value;
                            //  get method from DataFilter class
                            MethodInfo mi = typeof(DataFilter).GetMethod(functionName, BindingFlags.Public | BindingFlags.Static);

                            if (mi == null)
                            {
                                throw new Exception($"Filter {filterName} specifies an invalid function");
                            }

                            //  get dictionary of parameters from XML 
                            var paramDict = nextFunction.Elements("Parameter").ToDictionary(d => d.Attribute("Name").Value, d => d.Attribute("Value").Value);
                            //  create object array from parameters, casting to proper type
                            object[] parameters = mi.GetParameters().Select(p => paramDict[p.Name].Length > 0 ? Convert.ChangeType(paramDict[p.Name], p.ParameterType) : null).ToArray();

                            AddSignalFilterFunction(newFilter, mi, paramDict, parameters);
                        }

                        Filters.Add(filterName, newFilter);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// Add a function to the filter
        /// will determine if this function has dynamic parameters of data[] only, or data[] and sampling_rate
        /// </summary>
        static void AddSignalFilterFunction(SignalFilter filter, MethodInfo mi, Dictionary<string, string> paramDict, object[] parameters)
        {
            if (paramDict.ContainsKey("sampling_rate"))
                filter.AddFunction(new SignalFilterFunctionDataSampleRate(typeof(DataFilter).Assembly, mi, parameters));
            else
                filter.AddFunction(new SignalFilterFunctionData(typeof(DataFilter).Assembly, mi, parameters));
        }


        /// <summary>
        /// Load a default filter into the signal filters
        /// </summary>
        public void LoadDefaultFilter()
        {
            Filters.Clear();

            CreateDefaultFilterNotch60();

            CreateDefaultFilterNotch50();
        }


        /// <summary>
        /// Create a default filter for 60Hz mains noise
        /// </summary>
        static void CreateDefaultFilterNotch60()
        {
            var newFilter = new SignalFilter("default60");

            MethodInfo mi = typeof(DataFilter).GetMethod("perform_bandstop", BindingFlags.Public | BindingFlags.Static);
            var paramDict = new Dictionary<string, string>
            {
                ["data"] = "",
                ["sampling_rate"] = "0",
                ["center_freq"] = "60.0",
                ["band_width"] = "2.0",
                ["order"] = "6",
                ["filter_type"] = "1",
                ["ripple"] = "1.0"
            };

            //  create object array from parameters, casting to proper type
            object[] parameters = mi.GetParameters().Select(p => paramDict[p.Name].Length > 0 ? Convert.ChangeType(paramDict[p.Name], p.ParameterType) : null).ToArray();
            newFilter.AddFunction(new SignalFilterFunctionDataSampleRate(typeof(DataFilter).Assembly, mi, parameters));

            mi = typeof(DataFilter).GetMethod("perform_bandpass", BindingFlags.Public | BindingFlags.Static);
            paramDict = new Dictionary<string, string>
            {
                ["data"] = "",
                ["sampling_rate"] = "0",
                ["center_freq"] = "25.5",
                ["band_width"] = "47",
                ["order"] = "2",
                ["filter_type"] = "0",
                ["ripple"] = "0.0"
            };

            //  create object array from parameters, casting to proper type
            parameters = mi.GetParameters().Select(p => paramDict[p.Name].Length > 0 ? Convert.ChangeType(paramDict[p.Name], p.ParameterType) : null).ToArray();
            newFilter.AddFunction(new SignalFilterFunctionDataSampleRate(typeof(DataFilter).Assembly, mi, parameters));

            mi = typeof(DataFilter).GetMethod("perform_wavelet_denoising", BindingFlags.Public | BindingFlags.Static);
            paramDict = new Dictionary<string, string>
            {
                ["data"] = "",
                ["wavelet"] = "db12",
                ["decomposition_level"] = "3",
            };

            //  create object array from parameters, casting to proper type
            parameters = mi.GetParameters().Select(p => paramDict[p.Name].Length > 0 ? Convert.ChangeType(paramDict[p.Name], p.ParameterType) : null).ToArray();
            newFilter.AddFunction(new SignalFilterFunctionData(typeof(DataFilter).Assembly, mi, parameters));

            Filters.Add(newFilter.Name, newFilter);
        }


        /// <summary>
        /// Create a default filter for 60Hz mains noise
        /// </summary>
        static void CreateDefaultFilterNotch50()
        {
            var newFilter = new SignalFilter("default50");

            MethodInfo mi = typeof(DataFilter).GetMethod("perform_bandstop", BindingFlags.Public | BindingFlags.Static);
            var paramDict = new Dictionary<string, string>
            {
                ["data"] = "",
                ["sampling_rate"] = "0",
                ["center_freq"] = "50.0",
                ["band_width"] = "2.0",
                ["order"] = "6",
                ["filter_type"] = "1",
                ["ripple"] = "1.0"
            };

            //  create object array from parameters, casting to proper type
            object[] parameters = mi.GetParameters().Select(p => paramDict[p.Name].Length > 0 ? Convert.ChangeType(paramDict[p.Name], p.ParameterType) : null).ToArray();
            newFilter.AddFunction(new SignalFilterFunctionDataSampleRate(typeof(DataFilter).Assembly, mi, parameters));

            mi = typeof(DataFilter).GetMethod("perform_bandpass", BindingFlags.Public | BindingFlags.Static);
            paramDict = new Dictionary<string, string>
            {
                ["data"] = "",
                ["sampling_rate"] = "0",
                ["center_freq"] = "25.5",
                ["band_width"] = "47",
                ["order"] = "2",
                ["filter_type"] = "0",
                ["ripple"] = "0.0"
            };

            //  create object array from parameters, casting to proper type
            parameters = mi.GetParameters().Select(p => paramDict[p.Name].Length > 0 ? Convert.ChangeType(paramDict[p.Name], p.ParameterType) : null).ToArray();
            newFilter.AddFunction(new SignalFilterFunctionDataSampleRate(typeof(DataFilter).Assembly, mi, parameters));

            mi = typeof(DataFilter).GetMethod("perform_wavelet_denoising", BindingFlags.Public | BindingFlags.Static);
            paramDict = new Dictionary<string, string>
            {
                ["data"] = "",
                ["wavelet"] = "db12",
                ["decomposition_level"] = "3",
            };

            //  create object array from parameters, casting to proper type
            parameters = mi.GetParameters().Select(p => paramDict[p.Name].Length > 0 ? Convert.ChangeType(paramDict[p.Name], p.ParameterType) : null).ToArray();
            newFilter.AddFunction(new SignalFilterFunctionData(typeof(DataFilter).Assembly, mi, parameters));

            Filters.Add(newFilter.Name, newFilter);
        }

        /// <summary>
        /// Get collection of all filter names
        /// </summary>
        public IEnumerable<string> GetFilterNames()
        {
            return Filters.Keys.ToArray();
        }


        /// <summary>
        /// Get a filter by name
        /// </summary>
        public SignalFilter GetFilter(string name)
        {
            if (Filters.ContainsKey(name))
                return Filters[name];

            return null;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public SignalFilters()
        {
            Filters = new Dictionary<string, SignalFilter>();
        }

        //  The filter collection
        static Dictionary<string, SignalFilter> Filters;

    }


    public static class FilterBrainflowSample
    {
        public static IBFSample[] FilterChunk(SignalFilter filter, IEnumerable<IBFSample> chunk, int boardId, int numberOfChannels, int sampleRate)
        {
            try
            {
                if (chunk == null || chunk.Count() == 0)
                {
                    throw new ArgumentException("Invalid chunk");
                }

                //  copy the data for filtering
                var filteredSamples = new List<IBFSample>(chunk.Select(x => new BFSampleImplementation(x)));

                for (int i = 0; i < numberOfChannels; i++)
                {
                    var filtered = filter.ApplyFilter(chunk.GetExgDataForChannel(i), sampleRate);

                    for (int j = 0; j < chunk.Count(); j++)
                    {
                        filteredSamples[j].SetExgDataForChannel(i, filtered[j]);
                    }
                }

                var lastTimeStamp = chunk.First().TimeStamp;
                int lastSampleIndex = (int)chunk.First().SampleIndex;
                for (int i = 0; i < filteredSamples.Count; i++)
                {
                    filteredSamples[i].TimeStamp = lastTimeStamp + filteredSamples[i].SampleIndex.TimeBetweenSamples(lastSampleIndex, boardId, sampleRate);
                    lastTimeStamp = filteredSamples[i].TimeStamp;
                    lastSampleIndex = (int)filteredSamples[i].SampleIndex;
                }

                return filteredSamples.ToArray();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}
