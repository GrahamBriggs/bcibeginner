using brainflow;
using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brainHatSharpGUI
{
    interface IBoardDataReader
    {
        event LogEventDelegate Log;
        event ConnectBoBoardEventDelegate ConnectToBoard;
        event BFChunkEventDelegate BoardReadData;

        //  SRB1 for cyton board setting
        SrbSet CytonSRB1 { get; }

        //  SRB1 for daisy board setting
        SrbSet DaisySRB1 { get; }

        int BoardId { get;  }
        int SampleRate { get; }

        int BoardReadDelayMilliseconds { get; set; }

        bool IsStreaming { get; }

        void RequestEnableStreaming(bool enable);

        bool UserPausedStream { get;  }

        bool RequestSetSrb1(int board, bool enable);

        Task StartBoardDataReaderAsync(int boardId, BrainFlowInputParams inputParams, bool startSrb1Set);

        Task StopBoardDataReaderAsync();
    }
}
