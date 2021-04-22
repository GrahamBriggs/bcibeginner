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
        CONTEC_KT88 = -50,
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
                case BrainhatBoardIds.CONTEC_KT88:
                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    return true;
                default:
                    return false;
            }
        }

        public static int GetNumberOfExgChannels(int boardId)
        {
            switch ( (BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.CONTEC_KT88:
                    return 16;

                default:
                    return BoardShim.get_exg_channels(boardId).Length;
            }
        }

        public static int GetNumberOfAccelChannels(int boardId)
        {
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.CONTEC_KT88:
                    return 0;

                default:
                    return BoardShim.get_accel_channels(boardId).Length;
            }
        }

        public static int GetNumberOfOtherChannels(int boardId)
        {
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.CONTEC_KT88:
                    return 4;

                default:
                    return BoardShim.get_other_channels(boardId).Length;
            }
        }

        public static int GetNumberOfAnalogChannels(int boardId)
        {
            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.CONTEC_KT88:
                    return 0;

                default:
                    return BoardShim.get_analog_channels(boardId).Length;
            }
        }

        /// <summary>
        /// Get equipment type description string for boardId
        /// </summary>
        public static string GetEquipmentName(this int value)
        {
            switch (value)
            {
                case 0:
                    return "Cyton";
                case 2:
                    return "Cyton+Daisy";
            }
            return "Unknown";
        }


        /// <summary>
        /// Get equipment type description string for boardId
        /// </summary>
        public static string GetSampleName(this int value)
        {
            switch (value)
            {
                case 0:
                    return "Cyton8_BFSample";
                case 2:
                    return "Cyton16_BFSample";
                case 1:
                    return "Ganglion_BFSample";
                default:
                    return "";
            }
        }


        /// <summary>
        /// Get equipment type description string for boardId
        /// </summary>
        public static string GetSampleNameShort(this int value)
        {
            switch (value)
            {
                case 0:
                    return "CY08";
                case 2:
                    return "CY16";
                case 1:
                    return "GAN4";
                default:
                    return "";
            }
        }


        /// <summary>
        /// Get board ID from the string
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
                default:
                    return -99;
            }
        }
    }

    
}
