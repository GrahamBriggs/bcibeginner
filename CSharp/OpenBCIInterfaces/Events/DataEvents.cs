using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBCIInterfaces
{
    //  Open BCI Data (collection of readings) from Cyton 8 channel board Event
    public class OpenBciCyton8DataEventArgs : EventArgs
    {
        public OpenBciCyton8DataEventArgs(IEnumerable<OpenBciCyton8Reading> data)
        {
            Data = new List<OpenBciCyton8Reading>(data);
        }

        public List<OpenBciCyton8Reading> Data;
    }
    public delegate void OpenBciCyton8DataEventDelegate(object sender, OpenBciCyton8DataEventArgs e);




    //  Open BCI Reading from Cyton 8 channel board Event
    public class OpenBciCyton8ReadingEventArgs : EventArgs
    {
        public OpenBciCyton8ReadingEventArgs(OpenBciCyton8Reading reading)
        {
            Reading = reading;
        }

        public OpenBciCyton8Reading Reading;
    }
    public delegate void OpenBciCyton8ReadingEventDelegate(object sender, OpenBciCyton8ReadingEventArgs e);


    //  Get Data Delegate
    public delegate IEnumerable<OpenBciCyton8Reading> GetOpenBciCyton8DataDelegate(double seconds);

    //  Get Reading Delegate
    public delegate OpenBciCyton8Reading GetOpenBciCyton8ReadingDelegate();

    //  Get Band Power Delegate
    public delegate OpenBciCyton8Reading GetOpenBciCyton8BandPowerDelegate(int band);
}
