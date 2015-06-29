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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace jaMAL
{
    /// <summary>
    /// Class for accessing myanimelist.net. Methods are thread-safe. Properties are not.
    /// </summary>
    public static class Service
    {
        #region Public Properties

        /// <summary>
        /// The user agent to use in the http requests. If null then not using user-agent
        /// </summary>
        public static string UserAgent = null;

        #endregion

        #region Private Properties

        /// <summary>
        /// Service to retrieve a user animelist or mangalist
        /// </summary>
        private readonly static string _getMediaListURL = "http://myanimelist.net/malappinfo.php?u={0}&type={1}";

        /// <summary>
        /// Service to verify an account
        /// </summary>
        private readonly static string _verifyCredentialsURL = "http://myanimelist.net/api/account/verify_credentials.xml";

        /// <summary>
        /// Service to delete an anime entry from the animelist
        /// </summary>
        private readonly static string _deleteAnimeURL = "http://myanimelist.net/api/animelist/delete/{0}.xml";

        /// <summary>
        /// Service to update an anime entry from the animelist
        /// </summary>
        private readonly static string _updateAnimeURL = "http://myanimelist.net/api/animelist/update/{0}.xml";

        /// <summary>
        /// Service to add an anime entry to the animelist
        /// </summary>
        private readonly static string _addAnimeURL = "http://myanimelist.net/api/animelist/add/{0}.xml";

        /// <summary>
        /// Service to delete a manga entry from the mangalist
        /// </summary>
        private readonly static string _deleteMangaURL = "http://myanimelist.net/api/mangalist/delete/{0}.xml";

        /// <summary>
        /// Service to update a manga entry from the mangalist
        /// </summary>
        private readonly static string _updateMangaURL = "http://myanimelist.net/api/mangalist/update/{0}.xml";

        /// <summary>
        /// Service to add a manga entry to the mangalist
        /// </summary>
        private readonly static string _addMangaURL = "http://myanimelist.net/api/mangalist/add/{0}.xml";

        /// <summary>
        /// Service to query for an anime info
        /// </summary>
        private readonly static string _searchAnimeURL = "http://myanimelist.net/api/anime/search.xml?q={0}";

        /// <summary>
        /// Service to query for a manga info
        /// </summary>
        private readonly static string _searchMangaURL = "http://myanimelist.net/api/manga/search.xml?q={0}";

        #endregion

        #region Public Types

        /// <summary>
        /// Result of async add manga to manga list
        /// </summary>
        public class VerifyCredentialsAsyncResult : IAsyncResult
        {
            public VerifyCredentialsAsyncResult(Account account, bool couldVerify, Nullable<uint> accountID)
            {
                _account = account;
                _couldVerify = couldVerify;
                _accountId = accountID;
            }

            private Account _account;

            private bool _couldVerify;

            private Nullable<uint> _accountId;

            /// <summary>
            /// Account
            /// </summary>
            public Account Account
            {
                get { return _account; }
            }

            /// <summary>
            /// True if could verify acount credentials or false otherwise
            /// </summary>
            public bool CouldVerify
            {
                get { return _couldVerify; }
            }

            /// <summary>
            /// The account id if could verify the account or null otherwise
            /// </summary>
            public Nullable<uint> AccountId
            {
                get { return _accountId; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return (object)_account; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion


            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Result of async add manga to manga list
        /// </summary>
        public class DeleteMangaAsyncResult : IAsyncResult
        {
            public DeleteMangaAsyncResult(uint mangaId, bool couldDelete)
            {
                _mangaId = mangaId;
                _couldDelete = couldDelete;
            }

            private uint _mangaId;

            private bool _couldDelete;

            /// <summary>
            /// Deleted anime id
            /// </summary>
            public uint MangaId
            {
                get { return _mangaId; }
            }

            /// <summary>
            /// True if the anime could be deleted or false otherwise
            /// </summary>
            public bool CouldDelete
            {
                get { return _couldDelete; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return (object)_mangaId; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion


            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Result of async delete anime from anime list
        /// </summary>
        public class DeleteAnimeAsyncResult : IAsyncResult
        {
            public DeleteAnimeAsyncResult(uint animeId, bool couldDelete)
            {
                _animeId = animeId;
                _couldDelete = couldDelete;
            }

            private uint _animeId;

            private bool _couldDelete;

            /// <summary>
            /// Deleted anime id
            /// </summary>
            public uint AnimeId
            {
                get { return _animeId; }
            }

            /// <summary>
            /// True if the anime could be deleted or false otherwise
            /// </summary>
            public bool CouldDelete
            {
                get { return _couldDelete; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return (object)_animeId; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion


            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Result of async add anime to anime list
        /// </summary>
        public class AddUpdateAnimeAsyncResult : IAsyncResult
        {
            public AddUpdateAnimeAsyncResult(AnimeEntry animeEntry, bool couldAdd)
            {
                _animeEntry = animeEntry;
                _couldAdd = couldAdd;
            }

            private AnimeEntry _animeEntry;

            private bool _couldAdd;

            /// <summary>
            /// Added anime entry
            /// </summary>
            public AnimeEntry AnimeEntry
            {
                get { return _animeEntry; }
            }

            /// <summary>
            /// True if the anime could be added or false otherwise
            /// </summary>
            public bool CouldDo
            {
                get { return _couldAdd; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return (object)_animeEntry; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion


            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Result of async add manga to manga list
        /// </summary>
        public class AddUpdateMangaAsyncResult : IAsyncResult
        {
            public AddUpdateMangaAsyncResult(MangaEntry mangaEntry, bool couldAdd)
            {
                _mangaEntry = mangaEntry;
                _couldAdd = couldAdd;
            }

            private MangaEntry _mangaEntry;

            private bool _couldAdd;

            /// <summary>
            /// Added anime entry
            /// </summary>
            public MangaEntry MangaEntry
            {
                get { return _mangaEntry; }
            }

            /// <summary>
            /// True if the anime could be added or false otherwise
            /// </summary>
            public bool CouldDo
            {
                get { return _couldAdd; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return (object)_mangaEntry; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion


            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Result of async user anime list
        /// </summary>
        public class SearchUserMediaListAsyncResult : IAsyncResult
        {
            public SearchUserMediaListAsyncResult(uint userID, string userName, uint watching, uint completed, uint onHold, uint dropped, uint planToConsume, float daysSpentConsuming)
            {
                _userID = userID;
                _userName = userName;
                _watching = watching;
                _completed = completed;
                _onHold = onHold;
                _dropped = dropped;
                _planToConsume = planToConsume;
                _daysSpentConsuming = daysSpentConsuming;
            }

            private uint _userID;
            public uint UserID
            {
                get { return _userID; }
            }

            private string _userName;
            public string UserName
            {
                get { return _userName; }
            }

            private uint _watching;
            public uint Watching
            {
                get { return _watching; }
            }

            private uint _completed;
            public uint Completed
            {
                get { return _completed; }
            }

            private uint _onHold;
            public uint OnHold
            {
                get { return _onHold; }
            }

            private uint _dropped;
            public uint Dropped
            {
                get { return _dropped; }
            }

            private uint _planToConsume;
            public uint PlanToConsume
            {
                get { return _planToConsume; }
            }

            private float _daysSpentConsuming;
            public float DaysSpentConsuming
            {
                get { return _daysSpentConsuming; }
            }

            #region IAsyncResult Members

            public virtual object AsyncState
            {
                get { return (object)_userID; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion


            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Result of async user anime list
        /// </summary>
        public class SearchUserAnimeListAsyncResult : SearchUserMediaListAsyncResult
        {
            public SearchUserAnimeListAsyncResult(uint userID, string userName, uint watching, uint completed, uint onHold, uint dropped, uint planToConsume, float daysSpentConsuming, List<AnimeEntry> animeList): 
                base(userID, userName, watching, completed, onHold, dropped, planToConsume, daysSpentConsuming)
            {
                _animeList = animeList;
            }

            private List<AnimeEntry> _animeList;

            /// <summary>
            /// Found anime list
            /// </summary>
            public List<AnimeEntry> AnimeList
            {
                get { return _animeList; }
            }

            #region IAsyncResult Members

            public override object AsyncState
            {
                get { return (object)_animeList; }
            }

            #endregion
        }

        /// <summary>
        /// Result of async user manga list
        /// </summary>
        public class SearchUserMangaListAsyncResult : SearchUserMediaListAsyncResult
        {
            public SearchUserMangaListAsyncResult(uint userID, string userName, uint watching, uint completed, uint onHold, uint dropped, uint planToConsume, float daysSpentConsuming, List<MangaEntry> mangaList):
                base(userID, userName, watching, completed, onHold, dropped, planToConsume, daysSpentConsuming)
            {
                _mangaList = mangaList;
            }

            private List<MangaEntry> _mangaList;

            /// <summary>
            /// Found manga list
            /// </summary>
            public List<MangaEntry> MangaList
            {
                get { return _mangaList; }
            }

            #region IAsyncResult Members

            public override object AsyncState
            {
                get { return (object)_mangaList; }
            }

            #endregion
        }

        /// <summary>
        /// Result of async anime search query
        /// </summary>
        public class SearchAnimeAsyncResult : IAsyncResult
        {
            public SearchAnimeAsyncResult(List<Anime> animes)
            {
                _animes = animes;
            }

            private List<Anime> _animes;

            /// <summary>
            /// Found anime
            /// </summary>
            public List<Anime> Animes
            {
                get { return _animes; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return (object)_animes; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion


            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// Result of async manga search query
        /// </summary>
        public class SearchMangaAsyncResult : IAsyncResult
        {
            public SearchMangaAsyncResult(List<Manga> mangas)
            {
                _mangas = mangas;
            }

            private List<Manga> _mangas;

            /// <summary>
            /// Found manga
            /// </summary>
            public List<Manga> Mangas
            {
                get { return _mangas; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return (object)_mangas; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        /// <summary>
        /// Result of async image download
        /// </summary>
        public class DownloadingImageAsyncResult : IAsyncResult
        {
            public DownloadingImageAsyncResult(Media media, byte[] logoImage)
            {
                _media = media;
                _logoImage = logoImage;
            }

            private Media _media;

            private byte[] _logoImage;

            /// <summary>
            /// The image
            /// </summary>
            public byte[] LogoImage
            {
                get
                {
                    return _logoImage;
                }
            }

            /// <summary>
            /// The media that downloaded the image
            /// </summary>
            public Media Media
            {
                get { return _media; }
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { return (object)_logoImage; }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }

            #endregion


            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            object IAsyncResult.AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        #endregion

        /// <summary>
        /// Async download of the image logo of the media
        /// </summary>
        /// <param name="resultCallback">The callback to handle the result</param>
        /// <returns>The async result of the webrequest</returns>
        public static IAsyncResult BeginDownloadImage(Media media, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;
            try
            {
                HttpWebRequest webRequestURI = (HttpWebRequest)WebRequest.Create(media.LogoImageURI);
                res = webRequestURI.BeginGetResponse(
                    re =>
                    {
                        byte[] logoBuffer;
                        byte[] logoImage;

                        HttpWebResponse response = (HttpWebResponse)webRequestURI.EndGetResponse(res);
                        using (BinaryReader lxBR = new BinaryReader(response.GetResponseStream()))
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                logoBuffer = lxBR.ReadBytes(1024);
                                while (logoBuffer.Length > 0)
                                {
                                    memoryStream.Write(logoBuffer, 0, logoBuffer.Length);
                                    logoBuffer = lxBR.ReadBytes(1024);
                                }
                                logoImage = new byte[(int)memoryStream.Length];
                                memoryStream.Position = 0;
                                memoryStream.Read(logoImage, 0, logoImage.Length);
                            }
                        }
                        DownloadingImageAsyncResult result = new DownloadingImageAsyncResult(media, logoImage);
                        
                        resultCallback(result);

                    }, webRequestURI);
            }
            catch (Exception)
            {
            }
            return res;
        }

        #region Account Query

        /// <summary>
        /// Sync verifies if the user account name and password are valid
        /// </summary>
        /// <param name="account">The account to verify credentials</param>
        /// <param name="timeout">The miliseconds to wait for an answer from the service (-1 to wait forever). Default 5 seconds</param>
        /// <returns>True if could verify the credentials or false otherwise</returns>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static bool VerifyCredentials(Account account, out Nullable<uint> userId, int timeout = 5000)
        {
            Nullable<uint> id = null;
            bool couldVerify = false;
            try
            {
                bool finishVerifyCredentialsCallback = false;
                IAsyncResult res = BeginVerifyCredentials(account,
                    result =>
                    {
                        Service.VerifyCredentialsAsyncResult verifyResult = (Service.VerifyCredentialsAsyncResult)result;
                        couldVerify = verifyResult.CouldVerify;
                        if (couldVerify)
                            id = verifyResult.AccountId;
                        finishVerifyCredentialsCallback = true;
                    });

                bool finish = false;
                finish = res.AsyncWaitHandle.WaitOne(timeout);

                if (!finish)
                    throw new jaMALException("Timeout waiting for connection");

                while (!finishVerifyCredentialsCallback)
                { }
            }
            catch (WebException)
            {
            }

            userId = id;
            return couldVerify;
        }
        
        /// <summary>
        /// Async verifies if the user account name and password are valid
        /// </summary>
        /// <param name="account">The account to verify credentials</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginVerifyCredentials(Account account, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;
            try
            {
                // query CURL
                string webRequestURL = _verifyCredentialsURL;

                HttpWebRequest webRequest = WebRequest.Create(webRequestURL) as HttpWebRequest;
                webRequest.Method = "GET";
                webRequest.Credentials = new NetworkCredential(account.UserName, account.Password);

                _asyncQueryData data = new _asyncQueryData();
                data.ResultCallback = resultCallback;
                data.WebRequest = webRequest;
                data.Data = account;

                // Send the request and get back an XML response.
                res = webRequest.BeginGetResponse(new AsyncCallback(_getVerifyCredentialsResponseCallback), data);
            }
            catch (WebException)
            { }

            return res;
        }

        public static void _getVerifyCredentialsResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                _asyncQueryData data = (_asyncQueryData)asynchronousResult.AsyncState;
                Account account = data.Data as Account;
                HttpWebRequest webRequest = data.WebRequest;
                HttpWebResponse response;
                // get response
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                IAsyncResult result = null;

                // check the return status code
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:// account verifid :D
                        Stream streamResponse = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(streamResponse);
                        string Response = streamReader.ReadToEnd();
                        
                        // your response will be available in "Response"                       
                        // the response have the user info (user id is interesting)
                        Nullable<uint> id;
                        string name;
                        _parseUserVerificationXML(Response, out id, out name);

                        // the name of the account verified and the one that we have must ve the same!
                        Debug.Assert(name == account.UserName, "Verified name (" + name + ") is not equal to the desired username (" + account.UserName+")");

                        result = new VerifyCredentialsAsyncResult(account, true, id);
                        data.ResultCallback(result);

                        break;

                    case HttpStatusCode.NoContent:// couldnt verify account :(

                        // no response value

                        result = new VerifyCredentialsAsyncResult(account, false, null);
                        data.ResultCallback(result);

                        break;

                    default:

                        // wtf!!! this is not suppose to happen

                        break;
                }
            }
            catch (WebException e)
            {
                throw e;
            }
        }

        #endregion

        #region Anime and Manga Query

        /// <summary>
        /// Sync get user anime/manga list
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="type">If gets anime or manga list</param>
        /// <param name="timeout">The miliseconds to wait for an answer from the service (-1 to wait forever). Default 5 seconds</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static List<MediaEntry> GetUserList(string username, string password, MediaType type, int timeout = 5000)
        {
            string userName;
            uint userID, watching, completed, onHold, dropped, planToConsume;
            float daysSpentConsuming;
            // frack the list info! :P
            return GetUserList(username, password, type, out userID, out userName, out watching, out completed, out onHold, out dropped, out planToConsume, out daysSpentConsuming, timeout);
        }

        /// <summary>
        /// Sync get user anime/manga list
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="type">If gets anime or manga list</param>
        /// <param name="userID">The id of the account owner of the list</param>
        /// <param name="userName">The name of the account owner of the list</param>
        /// <param name="watching">The number of anime/manga currently consuming</param>
        /// <param name="completed">The number of anime/manga completed</param>
        /// <param name="onHold">The number of anime/manga on hold</param>
        /// <param name="dropped">The number of anime/manga dropped</param>
        /// <param name="planToConsume">The number of anime/manga planed to consume</param>
        /// <param name="daysSpentConsuming">The number of days to consume all the media</param>
        /// <param name="timeout">The miliseconds to wait for an answer from the service (-1 to wait forever). Default 5 seconds</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static List<MediaEntry> GetUserList(string username, string password, MediaType type, out uint userID, out string userName, out uint watching, out uint completed, out uint onHold, out uint dropped, out uint planToConsume, out float daysSpentConsuming, int timeout = 5000)
        {
            string _userName;
            uint _userID, _watching, _completed, _onHold, _dropped, _planToConsume;
            float _daysSpentConsuming;
            _userID = 0;
            _userName = string.Empty;
            _watching = 0;
            _completed = 0;
            _onHold = 0;
            _dropped = 0;
            _planToConsume = 0;
            _daysSpentConsuming = 0;
            List<MediaEntry> list = new List<MediaEntry>();
            try
            {
                bool finishGetListCallback = false;
                IAsyncResult res = BeginGetUserList(username, password, type,
                    result =>
                    {
                        switch (type)
                        {
                            case MediaType.Anime:
                                {
                                    Service.SearchUserAnimeListAsyncResult getlistResult = (Service.SearchUserAnimeListAsyncResult)result;
                                    list.AddRange(getlistResult.AnimeList);
                                }
                                break;
                            case MediaType.Manga:
                                { 
                                    Service.SearchUserMangaListAsyncResult getlistResult = (Service.SearchUserMangaListAsyncResult)result;
                                    list.AddRange(getlistResult.MangaList);
                                }
                                break;
                        }
                        Service.SearchUserMediaListAsyncResult getMediaListResult = (Service.SearchUserMediaListAsyncResult)result;
                        _userID = getMediaListResult.UserID;
                        _userName = getMediaListResult.UserName;
                        _watching = getMediaListResult.Watching;
                        _completed = getMediaListResult.Completed;
                        _onHold = getMediaListResult.OnHold;
                        _dropped = getMediaListResult.Dropped;
                        _planToConsume = getMediaListResult.PlanToConsume;
                        _daysSpentConsuming = getMediaListResult.DaysSpentConsuming;
                        finishGetListCallback = true;
                    });

                bool finish = false;
                finish = res.AsyncWaitHandle.WaitOne(timeout);
                
                if (!finish)
                    throw new jaMALException("Timeout waiting for connection");

                while (!finishGetListCallback)
                { }
            }
            catch (Exception e)
            {
                throw e;
            }
            userID = _userID;
            userName = _userName;
            watching = _watching;
            completed = _completed;
            onHold = _onHold;
            dropped = _dropped;
            planToConsume = _planToConsume;
            daysSpentConsuming = _daysSpentConsuming;
            return list;
        }

        /// <summary>
        /// Async get user anime/manga list
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="type">If gets anime or manga list</param>
        /// <param name="userID">The id of the account owner of the list</param>
        /// <param name="userName">The name of the account owner of the list</param>
        /// <param name="watching">The number of anime/manga currently consuming</param>
        /// <param name="completed">The number of anime/manga completed</param>
        /// <param name="onHold">The number of anime/manga on hold</param>
        /// <param name="dropped">The number of anime/manga dropped</param>
        /// <param name="planToConsume">The number of anime/manga planed to consume</param>
        /// <param name="daysSpentConsuming">The number of days to consume all the media</param>
        /// <param name="timeout">The miliseconds to wait for an answer from the service (-1 to wait forever). Default 5 seconds</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginGetUserList(string username, string password, MediaType type, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;
            try
            {
                string webRequestURL = string.Empty;
                // query CURL
                switch (type)
                {
                    case MediaType.Anime:
                        webRequestURL = string.Format(_getMediaListURL, username, "anime");
                        break;

                    case MediaType.Manga:
                        webRequestURL = string.Format(_getMediaListURL, username, "manga");
                        break;
                }

                HttpWebRequest webRequest = WebRequest.Create(webRequestURL) as HttpWebRequest;
                webRequest.Method = "GET";
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                // try to set user agent in this platform - if cannot set it does nothing
                // NOTE: This won't work in all platforms but is the only way in portable librarys :(
                _trySetUserAgent(ref webRequest, Service.UserAgent);

                _asyncQueryData data = new _asyncQueryData();
                data.ResultCallback = resultCallback;
                data.WebRequest = webRequest;
                data.Type = type;

                // Send the request and get back an XML response.
                res = webRequest.BeginGetResponse(new AsyncCallback(_getUserListResponseCallback), data);
            }
            catch (Exception e)
            {
                throw e;
            }

            return res;
        }

        public static void _getUserListResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                _asyncQueryData data = (_asyncQueryData)asynchronousResult.AsyncState;
                HttpWebRequest webRequest = data.WebRequest;
                HttpWebResponse response;
                // get response
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);

                // check the return status code
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:// there are results :D
                        Stream streamResponse = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(streamResponse);
                        string Response = streamReader.ReadToEnd();

                        // your response will be available in "Response"                       
                        List<AnimeEntry> animeList;
                        List<MangaEntry> mangaList;
                        string userName;
                        uint userID, watching, completed, onHold, dropped, planToConsume;
                        float daysSpentConsuming;
                        _parseMediaListXML(Response, data.Type, out userID, out userName, out watching, out completed, out onHold, out dropped, out planToConsume, out daysSpentConsuming, out animeList, out mangaList);

                        IAsyncResult result = null;
                        switch (data.Type)
                        {
                            case MediaType.Anime:
                                result = new SearchUserAnimeListAsyncResult(userID, userName, watching, completed, onHold, dropped, planToConsume, daysSpentConsuming, animeList);
                                break;

                            case MediaType.Manga:
                                result = new SearchUserMangaListAsyncResult(userID, userName, watching, completed, onHold, dropped, planToConsume, daysSpentConsuming, mangaList);
                                break;
                        }

                        data.ResultCallback(result);

                        break;

                    case HttpStatusCode.NoContent:// there isnt any result :(

                        // no response value

                        break;

                    default:

                        // wtf!!! this is not suppose to happen
                        throw new jaMALException("Cannot interpret get list information from the service");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Sync delete anime/manga
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="id">The id of the anime/manga</param>
        /// <param name="type">If the query is about anime or manga</param>
        /// <param name="timeout">The miliseconds to wait for an answer from the service (-1 to wait forever). Default 5 seconds</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static bool Delete(string username, string password, uint id, MediaType type, int timeout = 5000)
        {
            bool couldDelete = false;
            try
            {
                bool finishDeleteCallback = false;
                IAsyncResult res = BeginDelete(username, password, id, type,
                    ar =>
                    {
                        switch (type)
                        {
                            case MediaType.Anime:
                                couldDelete = (ar as DeleteAnimeAsyncResult).CouldDelete;
                                break;
                            case MediaType.Manga:
                                couldDelete = (ar as DeleteMangaAsyncResult).CouldDelete;
                                break;
                        }
                        finishDeleteCallback = true;
                    });

                bool finish = false;
                finish = res.AsyncWaitHandle.WaitOne(timeout);

                if (!finish)
                    throw new jaMALException("Timeout waiting for connection");

                while (!finishDeleteCallback)
                { }
            }
            catch (WebException e)
            {
                throw e;
            }

            return couldDelete;
        }

        /// <summary>
        /// Async delete anime/manga
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="id">The id of the anime/manga</param>
        /// <param name="type">If the query is about anime or manga</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginDelete(string username, string password, uint id, MediaType type, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;
            try
            {
                string webRequestURL = string.Empty;

                // query CURL
                switch (type)
                {
                    case MediaType.Anime:
                        webRequestURL = string.Format(_deleteAnimeURL, id);
                        break;

                    case MediaType.Manga:
                        webRequestURL = string.Format(_deleteMangaURL, id);
                        break;
                }

                HttpWebRequest webRequest = WebRequest.Create(webRequestURL) as HttpWebRequest;
                webRequest.Method = "POST";
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.ContentType = "application/x-www-form-urlencoded";

                _asyncQueryData data = new _asyncQueryData();
                data.ResultCallback = resultCallback;
                data.WebRequest = webRequest;
                data.Type = type;
                data.Data = id;

                // send the request and get back an XML response.
                res = webRequest.BeginGetResponse(new AsyncCallback(_getDeleteResponseCallback), data);

            } catch (WebException e)
            {
                throw e;
            }

            return res;
        }

        public static void _getDeleteResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                _asyncQueryData data = (_asyncQueryData)asynchronousResult.AsyncState;
                HttpWebRequest webRequest = data.WebRequest;
                HttpWebResponse response;
                // get response
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);

                // check the return status code
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:// deleted the entry :D
                        // the response just say Deleted
                        Stream streamResponse = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(streamResponse);
                        string Response = streamReader.ReadToEnd();

                        IAsyncResult result = null;
                        switch(data.Type)
                        {
                            case MediaType.Anime:
                                result = new DeleteAnimeAsyncResult((uint)data.Data, true);
                                break;
                            case MediaType.Manga:
                                result = new DeleteMangaAsyncResult((uint)data.Data, true);
                                break;
                        }
                        
                        data.ResultCallback(result);

                        break;

                    default:

                        // wtf!!! this is not suppose to happen
                        throw new jaMALException("Cannot interpret delete message from the service");
                }
            }
            catch (WebException e)
            {
                throw e;
            }
        }
        
        /// <summary>
        /// Sync add anime/manga
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="entry">The anime/manga entry to add</param>
        /// <param name="type">If the query is about anime or manga</param>
        /// <param name="timeout">The miliseconds to wait for an answer from the service (-1 to wait forever). Default 5 seconds</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static bool Add(string username, string password, MediaEntry entry, MediaType type, int timeout = 5000)
        {
            bool couldAdd = false;
            try
            {
                bool finishAddCallback = false;
                IAsyncResult res = BeginAdd(username, password, entry, type,
                    ar =>
                    {
                        switch(type)
                        {
                            case MediaType.Anime:
                                couldAdd = (ar as AddUpdateAnimeAsyncResult).CouldDo;
                                break;
                            case MediaType.Manga:
                                couldAdd = (ar as AddUpdateMangaAsyncResult).CouldDo;
                                break;
                        }
                        finishAddCallback = true;
                    });

                bool finish = false;
                finish = res.AsyncWaitHandle.WaitOne(timeout);

                if (!finish)
                    throw new jaMALException("Timeout waiting for connection");

                while (!finishAddCallback)
                { }
            }
            catch (WebException e)
            {
                throw e;
            }

            return couldAdd;
        }

        /// <summary>
        /// Sync update anime/manga
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="entry">The anime/manga entry to update</param>
        /// <param name="type">If the query is about anime or manga</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static bool Update(string username, string password, MediaEntry entry, MediaType type, int timeout = 5000)
        {
            bool couldUpdate = false;
            try
            {
                bool finishUpdateCallback = false;
                IAsyncResult res = BeginAdd(username, password, entry, type,
                    ar =>
                    {
                        switch (type)
                        {
                            case MediaType.Anime:
                                couldUpdate = (ar as AddUpdateAnimeAsyncResult).CouldDo;
                                break;
                            case MediaType.Manga:
                                couldUpdate = (ar as AddUpdateMangaAsyncResult).CouldDo;
                                break;
                        }
                        finishUpdateCallback = true;
                    });

                bool finish = false;
                finish = res.AsyncWaitHandle.WaitOne(timeout);

                if (!finish)
                    throw new jaMALException("Timeout waiting for connection");

                while (!finishUpdateCallback)
                { }
            }
            catch (WebException e)
            {
                throw e;
            }

            return couldUpdate;
        }

        /// <summary>
        /// Async update anime/manga
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="entry">The anime/manga entry to update</param>
        /// <param name="type">If the query is about anime or manga</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginUpdate(string username, string password, MediaEntry entry, MediaType type, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;
            try
            {
                string webRequestURL = string.Empty;

                // query CURL
                switch (type)
                {
                    case MediaType.Anime:
                        webRequestURL = string.Format(_updateAnimeURL, (entry as AnimeEntry).Anime.Id);
                        break;

                    case MediaType.Manga:
                        webRequestURL = string.Format(_updateMangaURL, (entry as MangaEntry).Manga.Id);
                        break;
                }

                HttpWebRequest webRequest = WebRequest.Create(webRequestURL) as HttpWebRequest;
                webRequest.Method = "POST";
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.ContentType = "application/x-www-form-urlencoded";

                _asyncQueryData data = new _asyncQueryData();
                data.ResultCallback = resultCallback;
                data.WebRequest = webRequest;
                data.Type = type;
                data.Data = entry;

                bool finishWritting = false;
                // send the data
                webRequest.BeginGetRequestStream(
                        ar =>
                        {
                            try
                            {
                                using (var requestStream = webRequest.EndGetRequestStream(ar))
                                using (var writer = new StreamWriter(requestStream))
                                {
                                    // write to the request stream
                                    //string postString = string.Format("id={0}&data={1}", id.ToString(), entry.ExportXML());
                                    string postString = string.Format("data={0}", entry.ExportXML());
                                    writer.Write(postString);
                                    writer.Flush();
                                }
                            }
                            catch (WebException)
                            { }
                            finishWritting = true;
                        }, data);

                while (!finishWritting)
                { }

                // send the request and get back an XML response.
                res = webRequest.BeginGetResponse(new AsyncCallback(_getAddUpdateResponseCallback), data);

            }
            catch (WebException e)
            {
                throw e;
            }

            return res;
        }

        /// <summary>
        /// Async add anime/manga
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="entry">The anime/manga entry to add</param>
        /// <param name="type">If the query is about anime or manga</param>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginAdd(string username, string password, MediaEntry entry, MediaType type, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;
            try
            {
                string webRequestURL = string.Empty;

                // query CURL
                switch (type)
                {
                    case MediaType.Anime:
                        webRequestURL = string.Format(_addAnimeURL, entry.Id);
                        break;

                    case MediaType.Manga:
                        webRequestURL = string.Format(_addMangaURL, entry.Id);
                        break;
                }

                HttpWebRequest webRequest = WebRequest.Create(webRequestURL) as HttpWebRequest;
                webRequest.Method = "POST";
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.ContentType = "application/x-www-form-urlencoded";

                _asyncQueryData data = new _asyncQueryData();
                data.ResultCallback = resultCallback;
                data.WebRequest = webRequest;
                data.Type = type;
                data.Data = entry;

                bool finishWritting = false;
                // send the data
                webRequest.BeginGetRequestStream(
                        ar =>
                        {
                            try
                            {
                                using (var requestStream = webRequest.EndGetRequestStream(ar))
                                using (var writer = new StreamWriter(requestStream))
                                {
                                    // write to the request stream
                                    //string postString = string.Format("id={0}&data={1}", id.ToString(), entry.ExportXML());
                                    string postString = string.Format("data={0}", entry.ExportXML());
                                    writer.Write(postString);
                                    writer.Flush();
                                }
                            } catch (WebException)
                            { }
                            finishWritting = true;
                        }, data);

                while (!finishWritting)
                { }

                // send the request and get back an XML response.
                res = webRequest.BeginGetResponse(new AsyncCallback(_getAddUpdateResponseCallback), data);

            } catch (WebException e)
            {
                throw e;
            }

            return res;
        }

        public static void _getAddUpdateResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                _asyncQueryData data = (_asyncQueryData)asynchronousResult.AsyncState;
                MediaEntry entry = data.Data as MediaEntry;
                HttpWebRequest webRequest = data.WebRequest;
                HttpWebResponse response;
                // get response
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);

                // check the return status code
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:// created the entry :D
                        // we dont read the response because it is a html page telling that the entry was addes
                        // if theere were an error it tells in the html that an SQL error ocurred
                        //Stream streamResponse = response.GetResponseStream();
                        //StreamReader streamReader = new StreamReader(streamResponse);
                        //string Response = streamReader.ReadToEnd();

                        IAsyncResult result = null;
                        switch(data.Type)
                        {
                            case MediaType.Anime:
                                result = new AddUpdateAnimeAsyncResult(entry as AnimeEntry, true);
                                break;

                            case MediaType.Manga:
                                result = new AddUpdateMangaAsyncResult(entry as MangaEntry, true);
                                break;
                        }
                        data.ResultCallback(result);

                        break;

                    default:
                        // wtf!!! this is not suppose to happen
                        throw new jaMALException("Cannot interpret add or update message from the service");
                }
            }
            catch (WebException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Sync search query for anime/manga. The result contains the list of anime/manga entries
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="searchName">The name of the anime/manga</param>
        /// <param name="type">If the query is about anime or manga</param>
        /// <param name="timeout">The miliseconds to wait for an answer from the service (-1 to wait forever). Default 5 seconds</param>
        /// <returns>A list with the found media</returns>
        ///<exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static List<Media> Search(string username, string password, string name, MediaType type, int timeout = 5000)
        {
            List<Media> media = null;
            try
            {
                media = new List<Media>();
                IAsyncResult mediaQuery = null;
                bool finishSearchCallback = false;
                IAsyncResult res = Service.BeginSearch(username, password, name, type,
                    ar =>
                    {
                        mediaQuery = ar;
                        finishSearchCallback = true;
                    });
                bool finish = false;
                finish = res.AsyncWaitHandle.WaitOne(timeout);

                if (!finish)
                    throw new jaMALException("Timeout waiting for connection");

                while (!finishSearchCallback)
                { }
                
                switch (type)
                {
                    case MediaType.Anime:
                        media.AddRange(((Service.SearchAnimeAsyncResult)mediaQuery).Animes);
                        break;

                    case MediaType.Manga:
                        media.AddRange(((Service.SearchMangaAsyncResult)mediaQuery).Mangas);
                        break;
                }
            } catch(WebException e)
            {
                throw e;
            }

            return media;
        }

        /// <summary>
        /// Async search query for anime/manga. The result contains the list of anime/manga entries
        /// </summary>
        /// <param name="username">The account username</param>
        /// <param name="password">The account password</param>
        /// <param name="searchName">The name of the anime/manga</param>
        /// <param name="type">If the query is about anime or manga</param>
        /// <param name="resultCallback">The callback to handle the result</param>
        ///<exception cref="jaMal.Exception">Something went wrong with the web request</exception>
        public static IAsyncResult BeginSearch(string username, string password, string name, MediaType type, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;
            try
            {
                string webRequestURL = string.Empty;
                // query CURL
                switch (type)
                {
                    case MediaType.Anime:
                        webRequestURL = string.Format(_searchAnimeURL, name);
                        break;

                    case MediaType.Manga:
                        webRequestURL = string.Format(_searchMangaURL, name);
                        break;
                }

                HttpWebRequest webRequest = WebRequest.Create(webRequestURL) as HttpWebRequest;
                webRequest.Method = "GET";
                webRequest.Credentials = new NetworkCredential(username, password);

                _asyncQueryData data = new _asyncQueryData();
                data.ResultCallback = resultCallback;
                data.WebRequest = webRequest;
                data.Type = type;

                // Send the request and get back an XML response.
                res = webRequest.BeginGetResponse(new AsyncCallback(_getSearchResponseCallback), data);
            } catch (WebException e)
            {
                throw e;
            }

            return res;
        }

        public static void _getSearchResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                _asyncQueryData data = (_asyncQueryData)asynchronousResult.AsyncState;
                HttpWebRequest webRequest = data.WebRequest;
                HttpWebResponse response;
                // get response
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);

                // check the return status code
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:// there are results :D
                        Stream streamResponse = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(streamResponse);
                        string Response = streamReader.ReadToEnd();
                        //streamResponse.Close();
                        //streamReader.Close();
                        //response.Close();

                        //Your response will be available in "Response"                        

                        List<Anime> animes;
                        List<Manga> manga;
                        _parseSearchXML(Response, out animes, out manga);

                        IAsyncResult result = null;
                        switch (data.Type)
                        {
                            case MediaType.Anime:
                                result = new SearchAnimeAsyncResult(animes);
                                break;

                            case MediaType.Manga:
                                result = new SearchMangaAsyncResult(manga);
                                break;
                        }

                        data.ResultCallback(result);

                        break;

                    case HttpStatusCode.NoContent:// there isnt any result :(

                        // no response value

                        break;

                    default:

                        // wtf!!! this is not suppose to happen
                        throw new jaMALException("Cannot interpret add or update message from the service");
                }
            } catch (WebException e)
            {
                throw e;
            }
        }

        #endregion

        #region Parse XML Methods

        private static void _parseUserVerificationXML(string xmlString, out Nullable<uint> userId, out string userName)
        {
            userId = null;
            userName = null;

            try
            {
                // Create an XmlReader
                using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
                {
                    // Parse the file
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (reader.Name)
                                {
                                    case "user":
                                        {
                                            _parseUserInfo(reader, out userId, out userName);
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            catch (XmlException)
            { }
        }


        private static void _parseMediaListXML(string xmlString, MediaType type, out uint userID, out string userName, out uint watching, out uint completed, out uint onHold, out uint dropped, out uint planToConsume, out float daysSpentConsuming, out List<AnimeEntry> animeList, out List<MangaEntry> mangaList)
        {
            userName = string.Empty;
            userID = watching = completed = onHold = dropped = planToConsume = 0;
            daysSpentConsuming = 0.0f;
            animeList = new List<AnimeEntry>();
            mangaList = new List<MangaEntry>();

            try
            {
                // Create an XmlReader
                using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
                {
                    // Parse the file
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (reader.Name)
                                {
                                    case "myinfo":
                                        {
                                            _parseUserListInfo(reader, out userID, out userName, out watching, out completed, out onHold, out dropped, out planToConsume, out daysSpentConsuming);
                                        }
                                        break;

                                    case "anime":
                                        {
                                            AnimeEntry entry = _parseAnimeEntry(reader);
                                            animeList.Add(entry);
                                        }
                                        break;

                                    case "manga":
                                        {
                                            MangaEntry entry = _parseMangaEntry(reader);
                                            mangaList.Add(entry);
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            catch (XmlException)
            { }
        }

        private static MangaEntry _parseMangaEntry(XmlReader reader)
        {
            MangaEntry aux = new MangaEntry(new Manga(0, ""), 0, 0, 0, 0);

            // Parse the file
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "series_animedb_id":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Manga.Id = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "series_title":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Manga.Name = reader.Value;
                                    }
                                }
                                break;

                            case "series_synonyms":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Manga.Synonyms = reader.Value;
                                    }
                                }
                                break;

                            case "series_type":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;

                                        switch (s)
                                        {
                                            case "Movie":
                                                aux.Manga.Type = Media.MediaType.Movie;
                                                break;
                                            case "Special":
                                                aux.Manga.Type = Media.MediaType.Special;
                                                break;
                                            case "OVA":
                                                aux.Manga.Type = Media.MediaType.OVA;
                                                break;
                                            case "TV":
                                                aux.Manga.Type = Media.MediaType.TV;
                                                break;
                                            case "Manga":
                                                aux.Manga.Type = Media.MediaType.Manga;
                                                break;
                                            case "One Shot":
                                                aux.Manga.Type = Media.MediaType.OneShot;
                                                break;
                                            case "Novel":
                                                aux.Manga.Type = Media.MediaType.Novel;
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "series_status":
                                {
                                    if (reader.Read())
                                    {
                                        uint status = uint.Parse(reader.Value);

                                        switch (status)
                                        {
                                            case 1:
                                                aux.Manga.Status = Media.MediaStatus.Airing;
                                                break;

                                            case 2:
                                                aux.Manga.Status = Media.MediaStatus.Finish;
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "series_start":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;
                                        string[] dateString = s.Split('-');
                                        int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                        int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                        int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                        aux.Manga.StartDate = new DateTime(year, month, day);
                                    }
                                }
                                break;

                            case "series_end":
                                {
                                    string s = reader.Value;
                                    string[] dateString = s.Split('-');
                                    int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                    int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                    int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                    aux.Manga.EndDate = new DateTime(year, month, day);
                                }
                                break;

                            case "series_chapters":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Manga.Chapter = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "series_volumes":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Manga.Volume = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "series_image":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Manga.LogoImageURI = reader.Value;
                                    }
                                }
                                break;

                            case "my_id":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Id = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_read_chapters":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Chapter = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_read_volumes":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Volume = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_start_date":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;
                                        string[] dateString = s.Split('-');
                                        int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                        int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                        int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                        aux.StartDate = new DateTime(year, month, day);
                                    }
                                }
                                break;

                            case "my_finish_date":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;
                                        string[] dateString = s.Split('-');
                                        int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                        int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                        int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                        aux.StartDate = new DateTime(year, month, day);
                                    }
                                }
                                break;

                            case "my_score":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Score = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_status":
                                {
                                    if (reader.Read())
                                    {
                                        uint status = uint.Parse(reader.Value);
                                        switch (status)
                                        {
                                            case 1:
                                                aux.Status = MediaEntry.EntryStatus.Currently;
                                                break;
                                            case 2:
                                                aux.Status = MediaEntry.EntryStatus.Completed;
                                                break;
                                            case 3:
                                                aux.Status = MediaEntry.EntryStatus.OnHold;
                                                break;
                                            case 4:
                                                aux.Status = MediaEntry.EntryStatus.Dropped;
                                                break;
                                            case 5:
                                                aux.Status = MediaEntry.EntryStatus.PlanToWatch;
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "my_rereadingg":// the double g is not an error... al least not mine :P
                                {
                                    if (reader.Read())
                                    {
                                        aux.Rereading = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_rereading_chap":
                                {
                                    if (reader.Read())
                                    {
                                        aux.RereadingChapter = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_last_updated":
                                {
                                    if (reader.Read())
                                    {
                                        aux.LastUpdated = uint.Parse(reader.Value);
                                    }
                                }
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "manga":
                                {
                                    return aux;
                                }
                        }
                        break;
                }
            }

            return aux;
        }

        private static AnimeEntry _parseAnimeEntry(XmlReader reader)
        {
            AnimeEntry aux = new AnimeEntry(new Anime(0,""), 0, 0, 0);

            // Parse the file
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "series_animedb_id":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Anime.Id = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "series_title":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Anime.Name = reader.Value;
                                    }
                                }
                                break;

                            case "series_synonyms":
                                {
                                    if (reader.Read())
                                    {
                                        // sometimes the series_synonyms is <series_synonyms/> :/
                                        aux.Anime.Synonyms = reader.Value;
                                    }
                                }
                                break;

                            case "series_type":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;

                                        switch (s)
                                        {
                                            case "Movie":
                                                aux.Anime.Type = Media.MediaType.Movie;
                                                break;
                                            case "Special":
                                                aux.Anime.Type = Media.MediaType.Special;
                                                break;
                                            case "OVA":
                                                aux.Anime.Type = Media.MediaType.OVA;
                                                break;
                                            case "TV":
                                                aux.Anime.Type = Media.MediaType.TV;
                                                break;
                                            case "Manga":
                                                aux.Anime.Type = Media.MediaType.Manga;
                                                break;
                                            case "One Shot":
                                                aux.Anime.Type = Media.MediaType.OneShot;
                                                break;
                                            case "Novel":
                                                aux.Anime.Type = Media.MediaType.Novel;
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "series_status":
                                {
                                    if (reader.Read())
                                    {
                                        uint status = uint.Parse(reader.Value);

                                        switch (status)
                                        {
                                            case 1:
                                                aux.Anime.Status = Media.MediaStatus.Airing;
                                                break;

                                            case 2:
                                                aux.Anime.Status = Media.MediaStatus.Finish;
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "series_start":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;
                                        string[] dateString = s.Split('-');
                                        int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                        int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                        int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                        aux.Anime.StartDate = new DateTime(year, month, day);
                                    }
                                }
                                break;

                            case "series_end":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;
                                        string[] dateString = s.Split('-');
                                        int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                        int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                        int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                        aux.Anime.EndDate = new DateTime(year, month, day);
                                    }
                                }
                                break;

                            case "series_episodes":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Anime.Episodes = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "series_image":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Anime.LogoImageURI = reader.Value;
                                    }
                                }
                                break;
                            // my_id is always 0 :/
                                /*
                            case "my_id":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Id = uint.Parse(reader.Value);
                                    }
                                }
                                break;
                                */
                            case "my_watched_episodes":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Episode = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_start_date":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;
                                        string[] dateString = s.Split('-');
                                        int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                        int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                        int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                        aux.StartDate = new DateTime(year, month, day);
                                    }
                                }
                                break;

                            case "my_finish_date":
                                {
                                    if (reader.Read())
                                    {
                                        string s = reader.Value;
                                        string[] dateString = s.Split('-');
                                        int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                        int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                        int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                        aux.StartDate = new DateTime(year, month, day);
                                    }
                                }
                                break;

                            case "my_score":
                                {
                                    if (reader.Read())
                                    {
                                        aux.Score = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_status":
                                {
                                    if (reader.Read())
                                    {
                                        uint status = uint.Parse(reader.Value);
                                        switch(status)
                                        {
                                            case 1:
                                                aux.Status = MediaEntry.EntryStatus.Currently;
                                                break;
                                            case 2:
                                                aux.Status = MediaEntry.EntryStatus.Completed;
                                                break;
                                            case 3:
                                                aux.Status = MediaEntry.EntryStatus.OnHold;
                                                break;
                                            case 4:
                                                aux.Status = MediaEntry.EntryStatus.Dropped;
                                                break;
                                            case 5:
                                                aux.Status = MediaEntry.EntryStatus.PlanToWatch;
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "my_rewatching":
                                {
                                    if (reader.Read())
                                    {
                                        // sometimes the my_rewatching is <my_rewatching/> :/
                                        if (reader.Value != string.Empty)
                                            aux.Rewatching = uint.Parse(reader.Value);
                                        else
                                            aux.Rewatching = 0;
                                    }
                                }
                                break;

                            case "my_rewatching_ep":
                                {
                                    if (reader.Read())
                                    {
                                        aux.RewatchingEpisode = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "my_last_updated":
                                {
                                    if (reader.Read())
                                    {
                                        aux.LastUpdated = uint.Parse(reader.Value);
                                    }
                                }
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "anime":
                                {
                                    return aux;
                                }
                        }
                        break;
                }
            }

            return aux;
        }

        private static void _parseUserInfo(XmlReader reader, out Nullable<uint> userId, out string userName)
        {
            userId = null;
            userName = null;
            // Parse the file
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "id":
                                {
                                    if (reader.Read())
                                    {
                                        userId = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "username":
                                {
                                    if (reader.Read())
                                    {
                                        userName = reader.Value;
                                    }
                                }
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "user":
                                {
                                    return;
                                }
                        }
                        break;
                }
            }
        }

        private static void _parseUserListInfo(XmlReader reader, out uint userID, out string userName, out uint watching, out uint completed, out uint onHold, out uint dropped, out uint planToConsume, out float daysSpentConsuming)
        {
            userName = string.Empty;
            userID = watching = completed = onHold = dropped = planToConsume = 0;
            daysSpentConsuming = 0.0f;
            // Parse the file
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "user_id":
                                {
                                    if (reader.Read())
                                    {
                                        userID = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "user_name":
                                {
                                    if (reader.Read())
                                    {
                                        userName = reader.Value;
                                    }
                                }
                                break;

                            case "user_reading":
                            case "user_watching":
                                {
                                    if (reader.Read())
                                    {
                                        watching = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "user_completed":
                                {
                                    if (reader.Read())
                                    {
                                        completed = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "user_onhold":
                                {
                                    if (reader.Read())
                                    {
                                        onHold = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "user_dropped":
                                {
                                    if (reader.Read())
                                    {
                                        dropped = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "user_plantoread":
                            case "user_plantowatch":
                                {
                                    if (reader.Read())
                                    {
                                        planToConsume = uint.Parse(reader.Value);
                                    }
                                }
                                break;

                            case "user_days_spent_watching":
                                {
                                    if (reader.Read())
                                    {
                                        daysSpentConsuming = float.Parse(reader.Value);
                                    }
                                }
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "myinfo":
                                {
                                    return;
                                }
                        }
                        break;
                }
            }
        }

        private static void _parseSearchXML(string xmlString, out List<Anime> anime, out List<Manga> manga)
        {
            anime = new List<Anime>();
            manga = new List<Manga>();

            try
            {
                // Create an XmlReader
                using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
                {
                    // Parse the file
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (reader.Name)
                                {
                                    case "anime":
                                        {
                                            _parseSearchAnime(reader, ref anime);
                                        }
                                        break;

                                    case "manga":
                                        {
                                            _parseSearchManga(reader, ref manga);
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }
            } catch (XmlException)
            { }
        }

        private static void _parseSearchAnime(XmlReader reader, ref List<Anime> anime)
        {
            // Parse the file
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "entry":
                                {
                                    anime.Add(_parseSearchAnimeEntry(reader));
                                }
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "anime":
                                {
                                    return;
                                }
                        }
                        break;
                }
            }
        }

        private static Anime _parseSearchAnimeEntry(XmlReader reader)
        {
            Anime aux = new Anime(0,"");

            // Parse the file
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "id":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.Id = uint.Parse(reader.Value);
                                        }
                                    }
                                }
                                break;

                            case "title":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.Name = reader.Value;
                                        }
                                    }
                                }
                                break;

                            case "english":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.EnglishName = reader.Value;
                                        }
                                    }
                                }
                                break;

                            case "synonyms":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.Synonyms = reader.Value;
                                        }
                                    }
                                }
                                break;

                            case "score":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.Score = float.Parse(reader.Value);
                                        }
                                    }
                                    
                                }
                                break;

                            case "type":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            string s = reader.Value;

                                            switch (s)
                                            {
                                                case "Movie":
                                                    aux.Type = Media.MediaType.Movie;
                                                    break;
                                                case "Special":
                                                    aux.Type = Media.MediaType.Special;
                                                    break;
                                                case "OVA":
                                                    aux.Type = Media.MediaType.OVA;
                                                    break;
                                                case "TV":
                                                    aux.Type = Media.MediaType.TV;
                                                    break;
                                                case "Manga":
                                                    aux.Type = Media.MediaType.Manga;
                                                    break;
                                                case "One Shot":
                                                    aux.Type = Media.MediaType.OneShot;
                                                    break;
                                                case "Novel":
                                                    aux.Type = Media.MediaType.Novel;
                                                    break;
                                            }
                                        }
                                    }
                                }
                                break;

                            case "status":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            string s = reader.Value;

                                            switch(s)
                                            {
                                                case "Currently Airing":
                                                    aux.Status = Media.MediaStatus.Airing;
                                                    break;

                                                case "Finished Airing":
                                                    aux.Status = Media.MediaStatus.Finish;
                                                    break;
                                            }
                                        }
                                    }
                                }
                                break;

                            case "start_date":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            string s = reader.Value;
                                            string[] dateString = s.Split('-');
                                            int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                            int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                            int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                            aux.StartDate = new DateTime(year, month, day);
                                        }
                                    }
                                }
                                break;

                            case "end_date":
                                {
                                    if (reader.NodeType == XmlNodeType.Text)
                                    {
                                        string s = reader.Value;
                                        string[] dateString = s.Split('-');
                                        int year = Math.Max(Math.Min(int.Parse(dateString[0]), 9999), 1);
                                        int month = Math.Max(Math.Min(int.Parse(dateString[1]), 12), 1);
                                        int day = Math.Max(Math.Min(int.Parse(dateString[2]), 31), 1);
                                        aux.EndDate = new DateTime(year, month, day);
                                    }
                                }
                                break;

                            case "synopsis":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.Synopsis = reader.Value;
                                        }
                                    }
                                }
                                break;

                            case "image":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.LogoImageURI = reader.Value;
                                        }
                                    }
                                }
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "entry":
                                {
                                    return aux;
                                }
                        }
                        break;
                }
            }

            return aux;
        }

        private static void _parseSearchManga(XmlReader reader, ref List<Manga> manga)
        {
            // Parse the file
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "entry":
                                {
                                    manga.Add(_parseSearchMangaEntry(reader));
                                }
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "anime":
                                {
                                    return;
                                }
                        }
                        break;
                }
            }
        }

        private static Manga _parseSearchMangaEntry(XmlReader reader)
        {
            Manga aux = new Manga(0, "");

            // Parse the file
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "id":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.Id = uint.Parse(reader.Value);
                                        }
                                    }
                                }
                                break;

                            case "title":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.Name = reader.Value;
                                        }
                                    }
                                }
                                break;

                            case "status":
                                {

                                }
                                break;

                            case "synopsis":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.Synopsis = reader.Value;
                                        }
                                    }
                                }
                                break;

                            case "image":
                                {
                                    if (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            aux.LogoImageURI = reader.Value;
                                        }
                                    }
                                }
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        {
                            switch (reader.Name)
                            {
                                case "entry":
                                    {
                                        return aux;
                                    }
                            }
                        }
                        break;
                }
            }

            return aux;
        }

        #endregion

        #region Private Types

        private class _asyncQueryData
        {
            public MediaType Type;
            public AsyncCallback ResultCallback;
            public HttpWebRequest WebRequest;
            public Object Data;
        }

        #endregion

        #region Auxilary Methods

        private static void _trySetUserAgent(ref HttpWebRequest httpRequest, string userAgentValue)
        {
            if (string.IsNullOrEmpty(userAgentValue))
                return;

            try
            {
                var userAgent = httpRequest.GetType().GetTypeInfo().DeclaredProperties.FirstOrDefault(e => e.Name.Equals("UserAgent"));
                if (userAgent != null)
                {
                    userAgent.SetValue(httpRequest, userAgentValue, null);
                }
            }
            catch (Exception)
            { }
        }

        #endregion
    }
}
