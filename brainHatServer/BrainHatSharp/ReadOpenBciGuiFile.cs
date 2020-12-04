using OpenBCIInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainHatSharp
{
    public static class ReadOpenBciGuiFile
    {
        public static List<OpenBciCyton8Reading> ParseFile(string fileName)
        {
            var data = new List<OpenBciCyton8Reading>();

            try
            {
                using (var reader = new StreamReader(fileName))
                {

                    var nextLine = reader.ReadLine();
                    while (nextLine != null)
                    {
                        if (char.IsNumber(nextLine, 0))
                        {
                            data.Add(new OpenBciCyton8Reading(nextLine));
                        }

                        nextLine = reader.ReadLine();
                    }

                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return data;
        }
    }
}
