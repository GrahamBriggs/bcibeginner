﻿using brainflow;
using BrainflowInterfaces;
using LoggingInterfaces;
using LSL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainHatNetwork
{
    public class LSLDataBroadcast
    {
        public event LogEventDelegate Log;

        public async Task StartLslBroadcastAsyc(int boardId, int sampleRate)
        {
            await StopLslBroadcastAsync();

            BoardId = boardId;
            SampleRate = sampleRate;

            SetupLslOutletForBoard();

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunDataBroadcast(CancelTokenSource.Token);
        }



        public async Task StopLslBroadcastAsync()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                if (RunTask != null)
                {
                    await RunTask;
                }

                CancelTokenSource = null;
                RunTask = null;
            }

            DataToBroadcast.RemoveAll();
        }

        public void AddData(IEnumerable<IBFSample> chunk)
        {
            DataToBroadcast.AddRange(chunk);
        }

        public void AddData(IBFSample sample)
        {
            DataToBroadcast.Enqueue(sample);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public LSLDataBroadcast()
        {
            DataToBroadcast = new ConcurrentQueue<IBFSample>();

        }

        public int BoardId { get; protected set; }
        public int SampleRate { get; protected set; }
        protected int SampleSize { get; set; }


        //  Thread run objects
        CancellationTokenSource CancelTokenSource;
        Task RunTask;

        ConcurrentQueue<IBFSample> DataToBroadcast { get; set; }

        liblsl.StreamInfo StreamInfo { get; set; }

        private void SetupLslOutletForBoard()
        {

            var numChannels = BoardShim.get_exg_channels(BoardId).Length;
            var numAccelChannels = BoardShim.get_accel_channels(BoardId).Length;
            var numOtherChannels = BoardShim.get_other_channels(BoardId).Length;
            var numAnalogChannels = BoardShim.get_analog_channels(BoardId).Length;

            SampleSize = 2 + numChannels + numAccelChannels + numOtherChannels + numAnalogChannels;

            StreamInfo = new liblsl.StreamInfo(StreamName(), "BFSample", SampleSize, SampleRate, liblsl.channel_format_t.cf_double64, BrainHatNetwork.NetworkUtilities.GetHostName());

            StreamInfo.desc().append_child_value("manufacturer", "OpenBCI");
            StreamInfo.desc().append_child_value("boardId", $"{BoardId}");
            liblsl.XMLElement chns = StreamInfo.desc().append_child("channels");

            chns.append_child("channel")
                .append_child_value("label", "SampleIndex")
                .append_child_value("unit", "0-255")
                .append_child_value("type", "index");

            for (int k = 0; k < numChannels; k++)
                chns.append_child("channel")
                .append_child_value("label", $"ExgCh{k}")
                .append_child_value("unit", "uV")
                .append_child_value("type", "EEG");

            for (int k = 0; k < numAccelChannels; k++)
                chns.append_child("channel")
                .append_child_value("label", $"AcelCh{k}")
                .append_child_value("unit", "1.0")
                .append_child_value("type", "Accelerometer");

            for (int k = 0; k < numOtherChannels; k++)
                chns.append_child("channel")
                .append_child_value("label", $"Other{k}");

            for (int k = 0; k < numAnalogChannels; k++)
                chns.append_child("channel")
                .append_child_value("label", $"AngCh{k}");

            chns.append_child("channel")
                .append_child_value("label", "TimeStamp")
                .append_child_value("unit", "s");
        }



        //  Stream name for board
        private string StreamName()
        {
            switch (BoardId)
            {
                case 0:
                    return "Cyton8_BFSample";
                case 2:
                    return "Cyton16_BFSample";
                case 1:
                    return "Ganglion_BFSample";
                default:
                    return "BFSample";
            }
        }


        private async Task RunDataBroadcast(CancellationToken cancelToken)
        {
            try
            {
                liblsl.StreamOutlet outlet = new liblsl.StreamOutlet(StreamInfo);

                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(10);

                    while (!DataToBroadcast.IsEmpty)
                    {
                        try
                        {
                            DataToBroadcast.TryDequeue(out var sample);
                            if (outlet.have_consumers())
                                outlet.push_sample(sample.AsRawSample());

                        }
                        catch (Exception ex)
                        {
                            Log?.Invoke(this, new LogEventArgs(this, "RunBroadcastServerAsync", ex, LogLevel.ERROR));
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunBroadcastServerAsync", e, LogLevel.ERROR));
            }
        }


    }
}
