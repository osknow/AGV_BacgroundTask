using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGV_BackgroundTask.SubPrograms
{
    public class MyLoger : TextWriter
    {
        private static string LogPath { get; set; } = Environment.CurrentDirectory + $"\\logs";

        public MyLoger()
        {

        }
        public MyLoger(string path)
        {
            LogPath = path;
        }
        public override void WriteLine(string value)
        {
            Directory.CreateDirectory(LogPath);
            File.AppendAllText($"{LogPath}\\{DateTime.Now.ToString("dddd_dd_MMMM_yyyy")}_log2.txt", $"{DateTime.Now.ToString("HH:mm:ss")} {value} {Environment.NewLine}");
        }
        public override Encoding Encoding
        {
            get
            {
                return Encoding.ASCII;
            }
        }
    }
}

