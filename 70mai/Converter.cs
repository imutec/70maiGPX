using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _70mai
{
    class GPSData2Gpx
    {
        string GPSData         = null;
        private const int SPAN = 3600; //デフォルトのセグメント区切り間隔

        public GPSData2Gpx(string file)
        {
            this.GPSData = file;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="span">セグメント区切りの間隔。
        /// タイムスタンプの間隔がこの値を超える場合はトラックセグメントを区切る
        /// </param>
        public void Convert(int span = SPAN)
        {
            //変換後のgpxファイル
            var output = Path.ChangeExtension(GPSData, ".gpx");

            // 動作しているPCに設定されているタイムゾーンを得る
            var tzLocal = TimeZoneInfo.Local;

            //1h = 3600
            //8h = 28800
            //9h = 36400
            var adjust = 28800 - (long)tzLocal.BaseUtcOffset.TotalSeconds;

            using (var writer = new StreamWriter(output))
            {
                writer.WriteLine(@"<gpx version = ""1.1"" creator = ""70maiDashcam"">");
                using (var reader = new StreamReader(GPSData))
                {
                    long segmenttime = 0;
                    long lasttimestamp = 0;
                    bool fSegement = false;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var fields = line.Split(',');
                        if (fields.Length > 3)
                        {
                            if (fields[1] == "V") continue; //AとVがある。Vは無効っぽいのでcontinue

                            var unixTimestamp = fields[0]; //1時間遅れている?
                            var latitude      = fields[2]; //緯度
                            var longitude     = fields[3]; //経度
                            
                            if ((latitude == "0.000000") || (longitude == "0.000000"))
                            {
                                Console.WriteLine("Skip lat 0.000000, lon 0.000000");
                                continue;
                            }

                            var timestamp = long.Parse(unixTimestamp) + adjust + (long)tzLocal.BaseUtcOffset.TotalSeconds;

                            var offsetdt = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                            var dateTime = offsetdt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                            //var localt = offsetdt.ToLocalTime();

                            var dateString = offsetdt.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                            //3600 = 1H
                            if ((timestamp - lasttimestamp) > span)
                            {
                                segmenttime = timestamp;
                                if (fSegement)
                                {
                                    writer.WriteLine("\t\t</trkseg>");
                                    writer.WriteLine("\t</trk>");
                                }

                                writer.WriteLine($"\t<trk><name>{dateString}</name>");
                                writer.WriteLine($"\t\t<trkseg>");
                                fSegement = true;
                            }

                            writer.Write("\t\t\t");
                            writer.WriteLine($@"<trkpt lat=""{latitude}"" lon=""{ longitude}""><time>{dateTime}</time></trkpt>");

                            lasttimestamp = timestamp;
                        }
                    }
                }
                writer.WriteLine("\t\t</trkseg>");
                writer.WriteLine("\t</trk>");
                writer.WriteLine("</gpx>");
            }
        }
    }
}
