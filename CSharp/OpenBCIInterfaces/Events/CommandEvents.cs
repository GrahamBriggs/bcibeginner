using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBCIInterfaces
{
    /// <summary>
    /// Command state
    /// </summary>
    public enum BsCommand
    {
        None,
        On,
        Off,
        Trigger2,
    }

    /// <summary>
    /// Command state event
    /// </summary>
    public class BsCommandEventArgs : EventArgs
    {
        public BsCommandEventArgs(BsCommand command)
        {
            Command = command;
        }

        public BsCommand Command { get; set; }

    }
    public delegate void BsCommandEventDelegate(object sender, BsCommandEventArgs e);
}
