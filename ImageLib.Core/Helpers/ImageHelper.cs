using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ImageLib.Helpers
{
    public class ImageHelper
    {
        /// <summary>
        /// 是否是Gif图片
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static bool IsGif(byte[] header)
        {
            bool isGif = false;

            if (header.Length >= 6)
            {
                isGif =
                    header[0] == 0x47 && // G
                    header[1] == 0x49 && // I
                    header[2] == 0x46 && // F
                    header[3] == 0x38 && // 8
                   (header[4] == 0x39 || header[4] == 0x37) && // 9 or 7
                    header[5] == 0x61;   // a
            }

            return isGif;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="savedPath"></param>
        /// <returns></returns>
        public static async Task<bool> Save(string url, string savedPath = null)
        {
            var uri = new Uri(url);
            var bytes = await uri.GetBytesFromUri();
            var extension = IsGif(bytes) ? ".gif" : ".jpg";
            string text = DateTime.Now.Ticks + extension;
            StorageFile storageFile = null;
            if (savedPath == null)
            {
                FileSavePicker fileSavePicker = new FileSavePicker();
                fileSavePicker.FileTypeChoices.Add("image", new List<string> { extension });
                fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                fileSavePicker.SuggestedFileName = text;
                storageFile = await fileSavePicker.PickSaveFileAsync();
            }
            else
            {
                StorageFolder storageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(savedPath, CreationCollisionOption.OpenIfExists);
                storageFile = await storageFolder.CreateFileAsync(text, CreationCollisionOption.ReplaceExisting);
            }
            if (storageFile == null)
            {
                return false;
            }
            await FileIO.WriteBytesAsync(storageFile, bytes);
            return true;
        }
    }
}
