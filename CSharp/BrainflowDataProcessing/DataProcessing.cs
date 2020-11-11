using brainflow;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using static BrainflowDataProcessing.OpenBCI_GUI;

namespace BrainflowDataProcessing
{
    class BoardFake
    {
        public int getSampleRate()
        {
            return 250;
        }
    }

    class Scratch
    {
        /*
         * Convert data from uV to V
Bandpass Filter: (1-51 Hz, 3rd order)
Bandstop Filter: (59.5-60.5 Hz, 4th order)
Denoising (Undecided method)
Convert to RMS value (What sized window is uses in the GUI?)
         */

        //void ProcessData(double[] data)
        //{
        //    var bandPass = brainflow.DataFilter.perform_bandpass(data, 250, 26, 25, 3, (int)FilterTypes.BUTTERWORTH, 0);
        //    var bandStop = brainflow.DataFilter.perform_bandstop(bandPass, 250, 60, 1, 4, (int)FilterTypes.CHEBYSHEV_TYPE_1, 1.0);

        //    var rms = brainflow.DataFilter.R
        //}

    }

    class DataProcessing
    {
        //  fake for current board
        BoardFake currentBoard = new BoardFake();

        //  globals from OpenBCI_GUI.pde
        double[,] dataProcessingFilteredBuffer;

        BandPassRanges bpRange;
        BandStopRanges bsRange;

        Complex[] fftBuff;


        //  DataProcessing.pde
        float fs_Hz;  //sample rate
        int nchan;

        double[] data_std_uv;
        double[] ploarity;
        bool newDataToSend;

       

        int[] processing_band_low_Hz = new int[]{ 1, 4, 8, 13, 30 };
        int[] processing_band_high_Hz = new int[] { 4, 8, 13, 30, 55 };

        double[,] avgPowerInBins;
        double[] headWidePower;

        public DataProcessing(int NCHAN, float sample_rate_Hz)
        {
            bpRange = new BandPassRanges();
            bsRange = new BandStopRanges();

            fftBuff = new Complex[nchan];

            nchan = NCHAN;
            fs_Hz = sample_rate_Hz;
            data_std_uv = new double[nchan];
            ploarity = new double[nchan];
            newDataToSend = false;
            avgPowerInBins = new double[nchan,processing_band_low_Hz.Length];
            headWidePower = new double[processing_band_high_Hz.Length];
        }


        public String getFilterDescription()
        {
            return bpRange.getDescr();
        }
        public String getShortFilterDescription()
        {
            return bpRange.getDescr();
        }
        public String getShortNotchDescription()
        {
            return bsRange.getDescr();
        }


        //  defered for now
        //public synchronized void incrementFilterConfiguration()
        //{
        //    bpRange = bpRange.next();
        //}

        //public synchronized void incrementNotchConfiguration()
        //{
        //    bsRange = bsRange.next();
        //}

        private void processChannel(int Ichan, double[] data_for_display_uV, double[] prevFFTdata)
        {
            int Nfft = getNfftSafe(currentBoard.getSampleRate());
            double foo;

            //filter the data in the time domain
            // todo use double arrays here and convert to float only to plot data
            try
            {
                //    double[] tempArray = floatToDoubleArray(data_forDisplay_uV[Ichan]);
                if (bsRange.GetRange().IsValid)
                {
                    DataFilter.perform_bandstop(data_for_display_uV, currentBoard.getSampleRate(), bsRange.getFreq(), 4.0, 2, (int)FilterTypes.BUTTERWORTH, (double)0.0);
                }
                if (bpRange.GetRange().IsValid)
                {
                    double centerFreq = (bpRange.getStart() + bpRange.getStop()) / 2.0;
                    double bandWidth = bpRange.getStop() - bpRange.getStart();
                    DataFilter.perform_bandpass(data_for_display_uV, currentBoard.getSampleRate(), centerFreq, bandWidth, 2, (int)FilterTypes.BUTTERWORTH, (double)0.0);
                }
                //    doubleToFloatArray(tempArray, data_forDisplay_uV[Ichan]);
            }
            catch (Exception e)
            {
                //  TODO - handle error
            }

           
            //compute the standard deviation of the filtered signal...this is for the head plot
            //float[] fooData_filt = dataProcessingFilteredBuffer[Ichan];  //use the filtered data
            //fooData_filt = Arrays.copyOfRange(fooData_filt, fooData_filt.length - ((int)fs_Hz), fooData_filt.length);   //just grab the most recent second of data
            //data_std_uV[Ichan] = std(fooData_filt); //compute the standard deviation for the whole array "fooData_filt"

            
            ////copy the previous FFT data...enables us to apply some smoothing to the FFT data
            //for (int I = 0; I < fftBuff[Ichan].specSize(); I++)
            //{
            //    prevFFTdata[I] = fftBuff[Ichan].getBand(I); //copy the old spectrum values
            //}

            ////prepare the data for the new FFT
            //float[] fooData;
            //if (isFFTFiltered == true)
            //{
            //    fooData = dataProcessingFilteredBuffer[Ichan];  //use the filtered data for the FFT
            //}
            //else
            //{
            //    fooData = dataProcessingRawBuffer[Ichan];  //use the raw data for the FFT
            //}
            //fooData = Arrays.copyOfRange(fooData, fooData.length - Nfft, fooData.length);   //trim to grab just the most recent block of data
            //float meanData = mean(fooData);  //compute the mean
            //for (int I = 0; I < fooData.length; I++) fooData[I] -= meanData; //remove the mean (for a better looking FFT

            ////compute the FFT
            //fftBuff[Ichan].forward(fooData); //compute FFT on this channel of data

            //// FFT ref: https://www.mathworks.com/help/matlab/ref/fft.html
            //// first calculate double-sided FFT amplitude spectrum
            //for (int I = 0; I <= Nfft / 2; I++)
            //{
            //    fftBuff[Ichan].setBand(I, (float)(fftBuff[Ichan].getBand(I) / Nfft));
            //}
            //// then convert into single-sided FFT spectrum: DC & Nyquist (i=0 & i=N/2) remain the same, others multiply by two.
            //for (int I = 1; I < Nfft / 2; I++)
            //{
            //    fftBuff[Ichan].setBand(I, (float)(fftBuff[Ichan].getBand(I) * 2));
            //}

            ////average the FFT with previous FFT data so that it makes it smoother in time
            //double min_val = 0.01d;
            //for (int I = 0; I < fftBuff[Ichan].specSize(); I++)
            //{   //loop over each fft bin
            //    if (prevFFTdata[I] < min_val) prevFFTdata[I] = (float)min_val; //make sure we're not too small for the log calls
            //    foo = fftBuff[Ichan].getBand(I);
            //    if (foo < min_val) foo = min_val; //make sure this value isn't too small

            //    if (true)
            //    {
            //        //smooth in dB power space
            //        foo = (1.0d - smoothFac[smoothFac_ind]) * java.lang.Math.log(java.lang.Math.pow(foo, 2));
            //        foo += smoothFac[smoothFac_ind] * java.lang.Math.log(java.lang.Math.pow((double)prevFFTdata[I], 2));
            //        foo = java.lang.Math.sqrt(java.lang.Math.exp(foo)); //average in dB space
            //    }
            //    else
            //    {
            //        //smooth (average) in linear power space
            //        foo = (1.0d - smoothFac[smoothFac_ind]) * java.lang.Math.pow(foo, 2);
            //        foo += smoothFac[smoothFac_ind] * java.lang.Math.pow((double)prevFFTdata[I], 2);
            //        // take sqrt to be back into uV_rtHz
            //        foo = java.lang.Math.sqrt(foo);
            //    }
            //    fftBuff[Ichan].setBand(I, (float)foo); //put the smoothed data back into the fftBuff data holder for use by everyone else
            //                                           // fftBuff[Ichan].setBand(I, 1.0f);  // test
            //} //end loop over FFT bins

            //// calculate single-sided psd by single-sided FFT amplitude spectrum
            //// PSD ref: https://www.mathworks.com/help/dsp/ug/estimate-the-power-spectral-density-in-matlab.html
            //// when i = 1 ~ (N/2-1), psd = (N / fs) * mag(i)^2 / 4
            //// when i = 0 or i = N/2, psd = (N / fs) * mag(i)^2

            //for (int i = 0; i < processing_band_low_Hz.length; i++)
            //{
            //    float sum = 0;
            //    // int binNum = 0;
            //    for (int Ibin = 0; Ibin <= Nfft / 2; Ibin++)
            //    { // loop over FFT bins
            //        float FFT_freq_Hz = fftBuff[Ichan].indexToFreq(Ibin);   // center frequency of this bin
            //        float psdx = 0;
            //        // if the frequency matches a band
            //        if (FFT_freq_Hz >= processing_band_low_Hz[i] && FFT_freq_Hz < processing_band_high_Hz[i])
            //        {
            //            if (Ibin != 0 && Ibin != Nfft / 2)
            //            {
            //                psdx = fftBuff[Ichan].getBand(Ibin) * fftBuff[Ichan].getBand(Ibin) * Nfft / currentBoard.getSampleRate() / 4;
            //            }
            //            else
            //            {
            //                psdx = fftBuff[Ichan].getBand(Ibin) * fftBuff[Ichan].getBand(Ibin) * Nfft / currentBoard.getSampleRate();
            //            }
            //            sum += psdx;
            //            // binNum ++;
            //        }
            //    }
            //    avgPowerInBins[Ichan][i] = sum;   // total power in a band
            //                                      // println(i, binNum, sum);
            //}
        }



    }
}
