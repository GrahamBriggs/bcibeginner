using Accord.Math;
using brainflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBoardRead
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var inputParams = new BrainFlowInputParams()
            {
                serial_port = "/dev/ttyUSB0",
            };

            int boardId = 0;

            var board = new BoardShim(boardId, inputParams);
            board.prepare_session();
            board.start_stream();

            await Task.Delay(TimeSpan.FromSeconds(7));

            while ( true )
            {
                var rawData = board.get_board_data();
                Console.WriteLine($"Read {rawData.Rows()} by {rawData.Columns()}.");

                await Task.Delay(50);
            }
        }
    }
}
