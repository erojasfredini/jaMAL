using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCLStorage;

namespace jaMAL
{
    /// <summary>
    /// The ImageDataBase is a singleton class that have the responsability to obtain images and maintain a cache of them
    /// </summary>
    public class ImageDataBase
    {
        #region Image Query Methods

        /// <summary>
        /// Get an image for a media asynchronously
        /// </summary>
        /// <param name="media">The media whose image we want</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call or null if could resolve synchronously (cache)</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginGetImage(Media media, AsyncCallback resultCallback)
        {
            ImageDataBase ins = _getInstance();
            IAsyncResult asyncRes = null;

            if( ins._imageCache.ContainsKey(media.LogoImageURI) )
            { 
                resultCallback(new Service.DownloadingImageAsyncResult(media, ins._imageCache[media.LogoImageURI]));
                
            }else
            {
                asyncRes = Service.BeginDownloadImage(media, 
                    result =>
                    {
                        Service.DownloadingImageAsyncResult downloadResult = (Service.DownloadingImageAsyncResult)result;
                        if (ins._imageCache.TryAdd(media.LogoImageURI, downloadResult.LogoImage))
                            ins._imageCache[media.LogoImageURI] = downloadResult.LogoImage;
                        if (resultCallback != null)
                            resultCallback(downloadResult);
                    });
            }
            return asyncRes;
        }

        /// <summary>
        /// Get an image for a media synchronously
        /// </summary>
        /// <param name="media">The media whose image we want</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call or null if could resolve synchronously (cache)</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static byte[] GetImage(Media media)
        {
            ImageDataBase ins = _getInstance();
            byte[] image = null;

            bool finishGettingImage = false;
            IAsyncResult res = BeginGetImage(media, 
                result =>
                    {
                        Service.DownloadingImageAsyncResult downloadResult = (Service.DownloadingImageAsyncResult)result;
                        image = downloadResult.LogoImage;
                        finishGettingImage = true;
                    });
            
            res.AsyncWaitHandle.WaitOne(-1);

            while(!finishGettingImage)
            {}

            return image;
        }

        #endregion

        /// <summary>
        /// Saves the images of the cache to a file
        /// </summary>
        /// <returns>True if could save or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public async static void SaveCache()
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            Task<IFolder> tFolder = rootFolder.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists);
            tFolder.Wait();
            IFolder folder = tFolder.Result;

            ImageDataBase ins = _getInstance();
            foreach (string fileName in ins._imageCache.Keys)
            {
                int lastSlash = fileName.LastIndexOf('/');
                string name = fileName.Substring(lastSlash + 1, fileName.Length - (lastSlash+1));
                IFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
                //tFile.Wait();
                //IFile file = tFile.Result;
                System.IO.Stream stream = await file.OpenAsync(FileAccess.ReadAndWrite);
                //tStream.Wait();
                //System.IO.Stream stream = tStream.Result;

                using (stream)
                {
                    stream.Write(ins._imageCache[fileName], 0, ins._imageCache[fileName].Length);
                }
            }
        }

        #region Internal Singleton

        /// <summary>
        /// The singleton instance of ImageDataBase
        /// </summary>
        private static ImageDataBase _instance = null;

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        /// <returns>The singleton instance</returns>
        private static ImageDataBase _getInstance()
        {
            if (_instance == null)
                _instance = new ImageDataBase();
            return _instance;
        }

        /// <summary>
        /// Default construction of a ImageDataBase
        /// </summary>
        private ImageDataBase()
        {
            _imageCache = new ConcurrentDictionary<string, byte[]>();
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// The images that have been cached by their URI
        /// </summary>
        private ConcurrentDictionary<string, byte[]> _imageCache;

        #endregion
    }
}
