using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _70mai
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Array.ForEach(args, f => {
                    if (File.Exists(f))
                    {
                        if (Path.GetFileName(f).Contains("GPSData000001"))
                        {
                            new GPSData2Gpx(f).Convert();
                        }
                    }
                });
            }
        }
    }
}
