using System;
using System.Collections.Generic;
using System.Text;

namespace BrainHatNetwork
{
    public class UriArgParser
    {
        public UriArgParser(string uri)
        {
            Args = new Dictionary<string, string>();
            Request = "";

            var strings = uri.Split('?');
            if (strings.Length > 1)
            {
                Request = strings[0];

                var args = strings[1].Split('&');
                foreach (var nextArg in args)
                {
                    var arg = nextArg.Split('=');
                    if ( arg.Length > 1 )
                        Args.Add(arg[0], arg[1]);
                }
            }
        }

        public string GetArg(string key)
        {
            if (Args.ContainsKey(key))
                return Args[key];
            else
                return "";
        }

        public string Request { get; protected set; }
        protected Dictionary<string, string> Args;
    }
}
