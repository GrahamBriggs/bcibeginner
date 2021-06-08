using brainflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowInterfaces
{
    public enum BrainhatBoardIds
    {
        UNDEFINED = -99,
        MENTALIUM = -51,
        CUSTOMBOARDS = -50,
        //
        PLAYBACK_FILE_BOARD = -3,
        STREAMING_BOARD = -2,
        SYNTHETIC_BOARD = -1,
        CYTON_BOARD = 0,
        GANGLION_BOARD = 1,
        CYTON_DAISY_BOARD = 2,
        GALEA_BOARD = 3,
        GANGLION_WIFI_BOARD = 4,
        CYTON_WIFI_BOARD = 5,
        CYTON_DAISY_WIFI_BOARD = 6,
        BRAINBIT_BOARD = 7,
        UNICORN_BOARD = 8,
        CALLIBRI_EEG_BOARD = 9,
        CALLIBRI_EMG_BOARD = 10,
        CALLIBRI_ECG_BOARD = 11,
        FASCIA_BOARD = 12,
        NOTION_1_BOARD = 13,
        NOTION_2_BOARD = 14,
        IRONBCI_BOARD = 15,
        GFORCE_PRO_BOARD = 16,
        FREEEEG32_BOARD = 17
    };



    public static class BrainhatBoardShim
    {
        public static bool IsSupportedBoard(int boardId)
        {
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return true;
                default:
                    return false;
            }
        }

        public static int GetNumberOfExgChannels(int boardId)
        {
            int useBoardId = boardId;
            switch ( (BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                    useBoardId = 0;
                    break;
            }

            switch ( (BrainhatBoardIds)useBoardId)
            {
                default:
                    return 0;

                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return BoardShim.get_exg_channels(useBoardId).Length;
            }
        }

        public static int GetNumberOfAccelChannels(int boardId)
        {
            int useBoardId = boardId;
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                    useBoardId = 0;
                    break;
            }

            switch ((BrainhatBoardIds)useBoardId)
            {
                default:
                    return 0;

                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return BoardShim.get_accel_channels(useBoardId).Length;
            }
        }

        public static int GetNumberOfOtherChannels(int boardId)
        {
            int useBoardId = boardId;
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                    useBoardId = 0;
                    break;
            }

            switch ((BrainhatBoardIds)useBoardId)
            {
                default:
                    return 0;

                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return BoardShim.get_other_channels(useBoardId).Length;
            }
        }

        public static int GetNumberOfAnalogChannels(int boardId)
        {
            int useBoardId = boardId;
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                    useBoardId = 0;
                    break;
            }

            switch ((BrainhatBoardIds)useBoardId)
            {
                default:
                    return 0;

                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return BoardShim.get_analog_channels(useBoardId).Length;
            }
        }

        public static int GetSampleRate(int boardId)
        {
            int useBoardId = boardId;
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                    useBoardId = 0;
                    break;
            }

            switch ((BrainhatBoardIds)useBoardId)
            {
                default:
                    return 0;

                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return BoardShim.get_sampling_rate(useBoardId);
            }
        }

        public static int GetTimestampChannel(int boardId)
        {
            int useBoardId = boardId;
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.MENTALIUM:
                    useBoardId = 0;
                    break;
            }

            switch ((BrainhatBoardIds)useBoardId)
            {
                default:
                    return 0;

                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return BoardShim.get_timestamp_channel(useBoardId);
            }
        }




        /// <summary>
        /// Get equipment type description string for boardId
        /// </summary>
        public static string GetSampleName(this int value)
        {
            switch ((BrainhatBoardIds)value)
            {
                case BrainhatBoardIds.CYTON_BOARD:
                    return "Cyton8_BFSample";
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return "Cyton16_BFSample";
                case BrainhatBoardIds.GANGLION_BOARD:
                    return "Ganglion_BFSample";
                case BrainhatBoardIds.MENTALIUM:
                    return "MENTALIUM8";
                default:
                    return "BFSample";
            }
        }


        /// <summary>
        /// Get equipment type description string for boardId
        /// </summary>
        public static string GetSampleNameShort(this int value)
        {
            switch ((BrainhatBoardIds)value)
            {
                case BrainhatBoardIds.CYTON_BOARD:
                    return "CY08";
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return "CY16";
                case BrainhatBoardIds.GANGLION_BOARD:
                    return "GAN4";
                case BrainhatBoardIds.MENTALIUM:
                    return "MT08";
                default:
                    return "BF";
            }
        }



        /// <summary>
        /// Get equipment type description string for boardId
        /// </summary>
        public static string GetEquipmentName(this int value)
        {
            switch ((BrainhatBoardIds)value)
            {
                case BrainhatBoardIds.CYTON_BOARD:
                    return "Cyton";
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return "Cyton+Daisy";
                case BrainhatBoardIds.GANGLION_BOARD:
                    return "Ganglion";
                case BrainhatBoardIds.MENTALIUM:
                    return "MENTALIUM";
            }
            return "Unknown";
        }


        /// <summary>
        /// Get board ID from the sample name string
        /// </summary>
        public static int GetBoardId(this string value)
        {
            switch (value)
            {
                case "CY08":
                case "Cyton8_BFSample":
                    return 0;
                case "CY16":
                case "Cyton16_BFSample":
                    return 2;
                case "GAN4":
                case "Ganglion_BFSample":
                    return 1;
                case "MT08":
                case "MENTALIUM8":
                    return -51;
                default:
                    return -99;
            }
        }
    }

    
}
