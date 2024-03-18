using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MolyMusicDownloader
{
    class LogTool
    {
        public static void WriteLog(string Msg)
        {
    
            string LogPath = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                DateTime date = DateTime.Now;
                string Second = date.ToString("yyyy-MM-dd HH:mm:ss");
                string Tody = date.ToString("yyyy-MM-dd");
                //如果此路徑沒有資料夾
                if (!Directory.Exists(LogPath + "\\Log"))
                {
                    Directory.CreateDirectory(LogPath + "\\Log");
                }

                //將收到的內容寫道檔案裡
                File.AppendAllText(LogPath + "\\Log\\" + Tody + ".txt", "\r\n" + Second +" "+ Msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
