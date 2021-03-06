﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrainflowInterfaces
{
    public enum SrbSet
    {
        Unknown = -1,
        Disconnected,
        Connected,
    }


    public interface ICytonChannelSettings
    {
        int ChannelNumber { get; set; }
        bool PowerDown { get; set; }
        ChannelGain Gain { get; set; }
        AdsChannelInputType InputType { get; set; }
        bool Bias { get; set; }
        bool LlofP { get; set; }
        bool LlofN { get; set; }
        bool Srb2 { get; set; }

    }
    //
    public class CytonChannelSettingsImplementation : ICytonChannelSettings
    {
        public CytonChannelSettingsImplementation()
        {
            ChannelNumber = 0;
            PowerDown = false;
            Gain = ChannelGain.x24;
            InputType = AdsChannelInputType.Normal;
            Bias = true;
            Srb2 = true;
        }

        public int ChannelNumber { get; set; }
        public bool PowerDown { get; set; }
        public ChannelGain Gain { get; set; }
        public AdsChannelInputType InputType { get; set; }
        public bool Bias { get; set; }
        public bool LlofP { get; set; }
        public bool LlofN { get; set; }
        public bool Srb2 { get; set; }

        public override string ToString()
        {
            return this.ChannelSettingsToString();
        }
    }

    public interface ICytonBoardSettings
    {
        bool Srb1Set { get; }
        ICytonChannelSettings[] Channels { get; }
    }
    //
    public class CytonBoardSettingsImplementation : ICytonBoardSettings
    {
        public CytonBoardSettingsImplementation()
        {
            _Channels = new List<ICytonChannelSettings>();
        }

        public bool Srb1Set { get; set; }
        List<ICytonChannelSettings> _Channels;
        public ICytonChannelSettings[] Channels => _Channels.ToArray();
        public void AddChannel(ICytonChannelSettings channel)
        {
            _Channels.Add(channel);
        }

    }

    public interface ICytonBoards
    {
        ICytonBoardSettings[] Boards { get; }
        bool IsValid { get; }
    }
    //
    public class CytonBoardsImplementation : ICytonBoards
    {
        public CytonBoardsImplementation()
        {
            _Boards = new List<CytonBoardSettingsImplementation>();
        }

        List<CytonBoardSettingsImplementation> _Boards;
        public ICytonBoardSettings[] Boards => _Boards.ToArray();

        public bool IsValid => _Boards.Count > 0;

        public void Invalidate()
        {
            _Boards = new List<CytonBoardSettingsImplementation>();
        }
        public void LoadFromRegistersString(string registerReport)
        {
            registerReport = registerReport.Replace('\r', ' ');
            registerReport = registerReport.Replace(" ", String.Empty);

            _Boards = new List<CytonBoardSettingsImplementation>();

            int boardChannelOffset = 0;

            try
            {
                var lines = registerReport.Split('\n');
                foreach (var nextLine in lines)
                {
                    if (nextLine.Contains("ADSRegisters"))
                    {
                        if (_Boards.Count > 0)
                        {
                            if (_Boards[_Boards.Count - 1].Channels.Count() != 8)
                            {
                                _Boards = new List<CytonBoardSettingsImplementation>();
                                throw new Exception("Board has less than 8 channels");
                            }
                            boardChannelOffset += _Boards.Last().Channels.Count();
                        }

                        _Boards.Add(new CytonBoardSettingsImplementation());
                    }
                    else if (nextLine.Length > "CHx".Length && nextLine.Substring(0, "CH".Length) == "CH")
                    {
                        var columns = nextLine.Split(',');
                        if (columns.Count() == 11)
                        {
                            var newChannel = new CytonChannelSettingsImplementation();

                            newChannel.ChannelNumber = int.Parse(nextLine.Substring("CH".Length, 1)) + boardChannelOffset;

                            if (_Boards.Last().Channels.Length > 0)
                            {
                                if (newChannel.ChannelNumber - _Boards.Last().Channels.Last().ChannelNumber != 1)
                                {
                                    _Boards = new List<CytonBoardSettingsImplementation>();
                                    throw new Exception("Board channels are not sequential");
                                }
                            }

                            newChannel.PowerDown = columns[7.IndexOfBit()] == "1" ? true : false;
                            newChannel.Gain = columns.GetChannelGain();
                            newChannel.InputType = columns.GetChannelInputType();
                            newChannel.Srb2 = columns[3.IndexOfBit()] == "1" ? true : false;

                            _Boards.Last().AddChannel(newChannel);

                        }
                    }
                    else if (nextLine.Length > "BIAS_SENSP".Length && nextLine.Substring(0, "BIAS_SENSP".Length) == "BIAS_SENSP")
                    {
                        ValidateBoardChannels();

                        var columns = nextLine.Split(',');
                        if (columns.Length == 11)
                        {
                            int bit = 0;
                            foreach (var nextChannel in _Boards.Last().Channels)
                            {
                                nextChannel.Bias = columns[bit.IndexOfBit()] == "1" ? true : false;
                                bit++;
                            }
                        }
                    }
                    else if (nextLine.Length > "LOFF_SENSP".Length && nextLine.Substring(0, "LOFF_SENSP".Length) == "LOFF_SENSP")
                    {
                        ValidateBoardChannels();

                        var columns = nextLine.Split(',');
                        if (columns.Length == 11)
                        {
                            int bit = 0;
                            foreach (var nextChannel in _Boards.Last().Channels)
                            {
                                nextChannel.LlofP = columns[bit.IndexOfBit()] == "1" ? true : false;
                                bit++;
                            }
                        }
                    }
                    else if (nextLine.Length > "LOFF_SENSN".Length && nextLine.Substring(0, "LOFF_SENSN".Length) == "LOFF_SENSN")
                    {
                        ValidateBoardChannels();

                        var columns = nextLine.Split(',');
                        if (columns.Length == 11)
                        {
                            int bit = 0;
                            foreach (var nextChannel in _Boards.Last().Channels)
                            {
                                nextChannel.LlofN = columns[bit.IndexOfBit()] == "1" ? true : false;
                                bit++;
                            }
                        }
                    }
                    else if (nextLine.Length > "MISC1".Length && nextLine.Substring(0, "MISC1".Length) == "MISC1")
                    {
                        ValidateBoardChannels();

                        var columns = nextLine.Split(',');
                        if (columns.Length == 11)
                        {
                            _Boards.Last().Srb1Set = columns[5.IndexOfBit()] == "1" ? true : false;
                        }
                    }
                }


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        void ValidateBoardChannels()
        {
            if (_Boards.Last().Channels.Length != 8)
            {
                _Boards = new List<CytonBoardSettingsImplementation>();
                throw new Exception("Fewer than 8 channels in the board");
            }
        }




    }

    public static class CytonBoardExtensionMethods
    {
        public static int IndexOfBit(this int value)
        {
            return 10 - value;
        }

        public static ChannelGain GetChannelGain(this string[] value)
        {
            if (value.Count() == 11)
            {
                int channelGain = 0;
                channelGain += value[4.IndexOfBit()] == "1" ? 1 : 0;
                channelGain += value[5.IndexOfBit()] == "1" ? 2 : 0;
                channelGain += value[6.IndexOfBit()] == "1" ? 4 : 0;

                return (ChannelGain)channelGain;
            }

            return ChannelGain.x1;
        }

        public static AdsChannelInputType GetChannelInputType(this string[] value)
        {
            if (value.Count() == 11)
            {
                int channelGain = 0;
                channelGain += value[0.IndexOfBit()] == "1" ? 1 : 0;
                channelGain += value[1.IndexOfBit()] == "1" ? 2 : 0;
                channelGain += value[2.IndexOfBit()] == "1" ? 4 : 0;

                return (AdsChannelInputType)channelGain;
            }

            return AdsChannelInputType.Normal;
        }
    }
}
