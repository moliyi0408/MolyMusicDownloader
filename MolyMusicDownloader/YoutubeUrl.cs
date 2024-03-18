using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MolyMusicDownloader
{
    public class YoutubeUrl
    {
        public string Url { get; set; }
        public string Title { get; set; }
        //ToString() 方法來決定如何顯示在列表中，默認情況下，ToString() 方法返回對象的完全限定類型名稱
        public override string ToString()
        {
            return $"{Title}";
        }
    }
}
