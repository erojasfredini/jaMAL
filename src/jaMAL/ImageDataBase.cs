/****************************************************************************
 * jaMAL
 * Copyright (c) Emmanuel Rojas Fredini, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; 
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 ***************************************************************************/

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

            int hash = media.LogoImageURI.GetHashCode();

            if (ins._imageCache.ContainsKey(hash))
            {
                resultCallback(new Service.DownloadingImageAsyncResult(media, ins._imageCache[hash]));
                
            }else
            {
                asyncRes = Service.BeginDownloadImage(media, 
                    result =>
                    {
                        Service.DownloadingImageAsyncResult downloadResult = (Service.DownloadingImageAsyncResult)result;
                        if (ins._imageCache.TryAdd(hash, downloadResult.LogoImage))
                            ins._imageCache[hash] = downloadResult.LogoImage;
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

        #region Save/Load/Clear Cache

        /// <summary>
        /// Clear the stored cache
        /// </summary>
        /// <param name="onFinishCallback">The callback to execute at the end of the process</param>
        /// <returns>True if succeeded or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong</exception>
        public async static Task<bool> ClearCache(AsyncCallback onFinishCallback = null)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            Task<IFolder> tFolder = rootFolder.CreateFolderAsync("imageCache", CreationCollisionOption.OpenIfExists);
            tFolder.Wait();
            IFolder folder = tFolder.Result;

            IList<IFile> files = await folder.GetFilesAsync();

            ImageDataBase ins = _getInstance();
            ins._imageCache.Clear();
            List<Task> tasks = new List<Task>();
            foreach (IFile f in files)
            {
                tasks.Add(f.DeleteAsync());
            }
            foreach (Task t in tasks)
                await t;
            if( onFinishCallback != null )
                onFinishCallback(null);
            return true;
        }

        /// <summary>
        /// Load the stored images to the cache
        /// </summary>
        /// <param name="onFinishCallback">The callback to execute at the end of the process</param>
        /// <returns>True if succeeded or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong</exception>
        public async static Task<bool> LoadCache(AsyncCallback onFinishCallback = null)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            Task<IFolder> tFolder = rootFolder.CreateFolderAsync("imageCache", CreationCollisionOption.OpenIfExists);
            tFolder.Wait();
            IFolder folder = tFolder.Result;

            IList<IFile> files = await folder.GetFilesAsync();

            ImageDataBase ins = _getInstance();
            foreach (IFile f in files)
            {
                System.IO.Stream stream = await f.OpenAsync(FileAccess.ReadAndWrite);
                using (stream)
                {
                    byte[] aux = new byte[stream.Length];
                    int read = stream.Read(aux, 0, aux.Length);
                    string hashString = f.Name.Substring(0, f.Name.LastIndexOf('.'));
                    int hash;
                    if( int.TryParse(hashString, out hash) )// there could be thing other than our files... like Thumb file :/
                    {
                        if (ins._imageCache.TryAdd(hash, aux))
                            ins._imageCache[hash] = aux;
                    }
                }
            }
            if (onFinishCallback != null)
                onFinishCallback(null);
            return true;
        }

        /// <summary>
        /// Save the images of the cache to a file
        /// </summary>
        /// <param name="onFinishCallback">The callback to execute at the end of the process</param>
        /// <returns>True if succeeded or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong</exception>
        public async static Task<bool> SaveCache(AsyncCallback onFinishCallback = null)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            Task<IFolder> tFolder = rootFolder.CreateFolderAsync("imageCache", CreationCollisionOption.OpenIfExists);
            tFolder.Wait();
            IFolder folder = tFolder.Result;

            ImageDataBase ins = _getInstance();
            foreach (int fileHash in ins._imageCache.Keys)
            {
                string fileName = fileHash.ToString()+".jpg";
                IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                
                System.IO.Stream stream = await file.OpenAsync(FileAccess.ReadAndWrite);

                using (stream)
                {
                    stream.Write(ins._imageCache[fileHash], 0, ins._imageCache[fileHash].Length);
                }
            }
            if (onFinishCallback != null)
                onFinishCallback(null);
            return true;
        }

        #endregion

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
            _imageCache = new ConcurrentDictionary<int, byte[]>();
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// The images that have been cached by their URI hash
        /// </summary>
        private ConcurrentDictionary<int, byte[]> _imageCache;

        #endregion
    }
}
