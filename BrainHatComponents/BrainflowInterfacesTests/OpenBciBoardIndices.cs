using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using brainflow;

namespace BrainflowInterfacesTests
{
    [TestClass]
    public class OpenBciBoardIndices
    {
        [TestMethod]
        public void OpenBciBoardIndicesTest()
        {
            var board = (int)BoardIds.GANGLION_BOARD;
            try
            {
                var exg = BoardShim.get_exg_channels(board);
                System.Diagnostics.Debug.WriteLine($"EXG: {string.Join(",", exg)}");
            }
            catch ( Exception e )
            {
                System.Diagnostics.Debug.WriteLine("No EXG");
            }

            try
            {
                var ecg = BoardShim.get_ecg_channels(board);
                System.Diagnostics.Debug.WriteLine($"ECG: {string.Join(",", ecg)}");
              
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No ECG");
            }

           

            try
            {
                var emg = BoardShim.get_emg_channels(board);
                System.Diagnostics.Debug.WriteLine($"EMG: {string.Join(",", emg)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No EMG");
            }

            try
            {
                var eog = BoardShim.get_eog_channels(board);
                System.Diagnostics.Debug.WriteLine($"EOG: {string.Join(",", eog)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No EOG");
            }

            try
            {
                var accel = BoardShim.get_accel_channels(board);
                System.Diagnostics.Debug.WriteLine($"ACCEL: {string.Join(",", accel)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No ACCEL");
            }

            try
            {
                var other = BoardShim.get_other_channels(board);
                System.Diagnostics.Debug.WriteLine($"OTHER: {string.Join(",", other)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No OTHER");
            }

            try
            {
                var ang = BoardShim.get_analog_channels(board);
                System.Diagnostics.Debug.WriteLine($"ANG: {string.Join(",", ang)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No ANG");
            }

            try
            {
                var time = BoardShim.get_timestamp_channel(board);
                System.Diagnostics.Debug.WriteLine($"TIME: {time}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No TIME");
            }

            try
            {
                var eda = BoardShim.get_eda_channels(board);
                System.Diagnostics.Debug.WriteLine($"EDA: {string.Join(",", eda)}");

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No EDA");
            }

            try
            {
                var bat = BoardShim.get_battery_channel(board);
                System.Diagnostics.Debug.WriteLine($"BAT: {string.Join(",", bat)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No BAT");
            }

            try
            {
                var gyro = BoardShim.get_gyro_channels(board);
                System.Diagnostics.Debug.WriteLine($"GYRO: {string.Join(",", gyro)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No GYRO");
            }

           

            try
            {
                var pkg = BoardShim.get_package_num_channel(board);
                System.Diagnostics.Debug.WriteLine($"PKG: {string.Join(",", pkg)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No PKG");
            }

            try
            {
                var ppg = BoardShim.get_ppg_channels(board);
                System.Diagnostics.Debug.WriteLine($"PPG: {string.Join(",", ppg)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No PPG");
            }

            try
            {
                var res = BoardShim.get_resistance_channels(board);
                System.Diagnostics.Debug.WriteLine($"RES: {string.Join(",", res)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No RES");
            }

            try
            {
                var tmp = BoardShim.get_temperature_channels(board);
                System.Diagnostics.Debug.WriteLine($"TMP: {string.Join(",", tmp)}");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("No TMP");
            }

           




        }
    }
}
