using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.Streams;

namespace MolyMusicDownloader
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            YoutubeUrls = new ObservableCollection<YoutubeUrl>();
            FileLoadCommand = new RelayCommand<string>(parameter => FileLoadFuc(parameter));
            FolderLoadCommand = new RelayCommand<string>(parameter => FolderLoadFuc(parameter));
            DownloadMp3Command = new RelayCommand<string>(DownloadMp3Func);
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        public ICommand FileLoadCommand { get; set; }
        private void FileLoadFuc(object parameter)
        {
            var dlg = new CommonOpenFileDialog();
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = dlg.FileName;
                LoadUrlsFromTextFile(FilePath);
            }
        }

        //目標資料夾
        private string _folderPath;
        public string FolderPath
        {
            get => _folderPath;
            set
            {
                _folderPath = value;
                OnPropertyChanged("FolderPath");
            }
        }
        public ICommand FolderLoadCommand { set; get; }
        private void FolderLoadFuc(object parameter)
        {
            var dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FolderPath = dlg.FileName;
            }
        }


        public ObservableCollection<YoutubeUrl> YoutubeUrls { get; set; }

        //載入文字檔並分析
        private void LoadUrlsFromTextFile(string filePath)
        {
            try
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    // 傳入多個值時切割
                    var parts = line.Split(' ');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("https://youtu.be/"))
                        {
                            
                            var url = part;
                            
                            var title = GetTitle(url); // Consider using HtmlAgilityPack for faster parsing
                            // 判斷影片下架或找不到的情況，則跳過
                            if (string.IsNullOrEmpty(title))
                            {
                                continue;
                            }
                            YoutubeUrls.Add(new YoutubeUrl { Url = url, Title = title });
                            LogTool.WriteLog($"{url} {title}");
                        }
                    }
                }
              
            }
            catch (Exception ex)
            {
                // 記錄錯誤並顯示友好的錯誤訊息
                MessageBox.Show($"讀取檔案時發生錯誤：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //下載檔案
        public ICommand DownloadMp3Command { get; set; }
        private async void DownloadMp3Func(object parameter)
        {
            //install YoutubeExplode
            var youtube = new YoutubeClient();
            var allDownloaded = true;
            foreach (var youtubeUrl in YoutubeUrls)
            {
                try
                {
                    var url = youtubeUrl.Url;
                    var title = GetValidFileName(youtubeUrl.Title);

                    if (FolderPath != null)
                    {
                        var manifest = await youtube.Videos.Streams.GetManifestAsync(url);
                        var streamInfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                        if (streamInfo != null)
                        {
                            // 下载音频流
                            var fileName = Path.Combine(FolderPath, $"{title}.mp3");
                            await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName);


                            // 檢查是否下載完成
                            if (!File.Exists(fileName))
                            {
                                allDownloaded = false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("無法下載");
                        }

                    }
                    else
                    {
                        MessageBox.Show("請選取儲存位置");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // 影片已下架或找不到
                    MessageBox.Show($"影片 {youtubeUrl.Url} 已下架或找不到", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            // 如果所有文件都已下载，则显示消息
            if (allDownloaded)
            {
                MessageBox.Show("所有音樂皆已下载完成");
            }

        }
        //
        private string GetTitle(string url)
        {
            WebRequest request = HttpWebRequest.Create(url);
            using (WebResponse response = request.GetResponse())
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                string html = streamReader.ReadToEnd();
                int startIndex = html.IndexOf("<title>") + 7;
                int endIndex = html.IndexOf("</title>", startIndex);
                return html.Substring(startIndex, endIndex - startIndex);
            }
        }
        private string GetValidFileName(string title)
        {
            // 檢查檔案是否符合規範
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                title = title.Replace(invalidChar, '_');
            }
            return title;
        }


    }
}
