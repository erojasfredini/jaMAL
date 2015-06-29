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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace jaMAL
{
    /// <summary>
    /// The MediaDataBase is a singleton class that have the responsability to obtain querys about the animes and mangas.
    /// Internaly it mantains a cache of the already searched animes and mangas
    /// </summary>
    public class MediaDataBase
    {
        #region Default User Account

        /// <summary>
        /// The default account to make searches
        /// </summary>
        public static Account UserAccount;

        #endregion

        #region Anime Query Methods

        /// <summary>
        /// Get the animes that match the expression asynchronously
        /// </summary>
        /// <param name="matchName">The name to match</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginGetAnimes(string matchName, AsyncCallback resultCallback)
        {
            MediaDataBase ins = _getInstance();

            IAsyncResult res = Service.BeginSearch(MediaDataBase.UserAccount.UserName, MediaDataBase.UserAccount.Password, matchName, MediaType.Anime, 
                result =>
                {
                     Service.SearchAnimeAsyncResult animQuery = (Service.SearchAnimeAsyncResult)result;
                    foreach (Anime a in animQuery.Animes)
                    {
                        ins._animeCache.GetOrAdd(a.Id, a);
                    }
                    resultCallback(animQuery);
                }
            );
            return res;
        }

        /// <summary>
        /// Get the animes that match the expression synchronously
        /// </summary>
        /// <param name="matchName">The name to match</param>
        /// <returns>The animes that match</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static List<Anime> GetAnimes(string matchName)
        {
            MediaDataBase ins = _getInstance();

            List<Anime> animeR = null;
            List<Media> mediaR;

            mediaR = Service.Search(MediaDataBase.UserAccount.UserName, MediaDataBase.UserAccount.Password, matchName, MediaType.Anime);

            animeR = new List<Anime>(mediaR.Count);

            foreach (Media m in mediaR)
            {
                ins._animeCache.GetOrAdd(m.Id, m as Anime);
                animeR.Add(m as Anime);
            }

            return animeR;
        }

        /// <summary>
        /// Get an anime with exactly that name asynchronously. The callback will obtain the found anime or null of not found
        /// </summary>
        /// <param name="name">The name of the anime</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call or null if could resolve synchronously (cache)</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginGetAnime(string name, AsyncCallback resultCallback)
        {
            MediaDataBase ins = _getInstance();

            try
            {
                var query = ins._animeCache.First(pair => pair.Value.Name.Equals(name));
                List<Anime> res = new List<Anime>(1);
                res.Add(query.Value);
                resultCallback(new Service.SearchAnimeAsyncResult(res));
                return null;
            }
            catch (InvalidOperationException)// if First couldnt find anything
            {
                IAsyncResult asyncRes = Service.BeginSearch(MediaDataBase.UserAccount.UserName, MediaDataBase.UserAccount.Password, name, MediaType.Anime,
                    result =>
                    {
                        Service.SearchAnimeAsyncResult animQuery = (Service.SearchAnimeAsyncResult)result;
                        foreach (Anime a in animQuery.Animes)
                        {
                            ins._animeCache.GetOrAdd(a.Id, a);
                        }
                        try
                        {
                            var query = animQuery.Animes.First(pair => pair.Name.Equals(name));
                            List<Anime> res = new List<Anime>(1);
                            res.Add(query);
                            resultCallback(new Service.SearchAnimeAsyncResult(res));
                        }
                        catch (InvalidOperationException)
                        {
                            resultCallback(new Service.SearchAnimeAsyncResult(null));
                        }
                    });
                return asyncRes;
            }
        }

        /// <summary>
        /// Get an anime with exactly that name synchronously
        /// </summary>
        /// <param name="name">The name of the anime</param>
        /// <returns>The anime</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static Anime GetAnime(string name)
        {
            MediaDataBase ins = _getInstance();
            
            //var query = ins.AnimeCache.Where(pair => System.Text.RegularExpressions.Regex.IsMatch(pair.Key, matchName, System.Text.RegularExpressions.RegexOptions.IgnoreCase)).Select(pair => pair.Value);
            try
            {
                var query = ins._animeCache.First(pair => pair.Value.Name.Equals(name));
                return query.Value;
            }
            catch (InvalidOperationException)// if First couldnt find anything
            {
                List<Anime> animes = GetAnimes(name);

                Anime match = animes.First(x => x.Name.Equals(name));
                return match;
            }
        }

        #endregion

        #region Manga Query Methods

        /// <summary>
        /// Get the mangas that match the expression asynchronously
        /// </summary>
        /// <param name="matchName">The name to match</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginGetMangas(string matchName, AsyncCallback resultCallback)
        {
            MediaDataBase ins = _getInstance();

            IAsyncResult res = Service.BeginSearch(MediaDataBase.UserAccount.UserName, MediaDataBase.UserAccount.Password, matchName, MediaType.Manga,
                result =>
                {
                    Service.SearchMangaAsyncResult mangaQuery = (Service.SearchMangaAsyncResult)result;
                    foreach (Manga m in mangaQuery.Mangas)
                    {
                        ins._mangaCache.GetOrAdd(m.Id, m);
                    }
                    resultCallback(mangaQuery);
                }
            );
            return res;
        }

        /// <summary>
        /// Get the mangas that match the expression synchronously
        /// </summary>
        /// <param name="matchName">The name to match</param>
        /// <returns>The mangas that match</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static List<Manga> GetMangas(string matchName)
        {
            MediaDataBase ins = _getInstance();

            List<Manga> mangaR = null;
            List<Media> mediaR;

            mediaR = Service.Search(MediaDataBase.UserAccount.UserName, MediaDataBase.UserAccount.Password, matchName, MediaType.Manga);

            mangaR = new List<Manga>(mediaR.Count);

            foreach (Media m in mediaR)
            {
                ins._mangaCache.GetOrAdd(m.Id, m as Manga);
                mangaR.Add(m as Manga);
            }

            return mangaR;
        }

        /// <summary>
        /// Get a manga with exactly that name asynchronously. The callback will obtain the found manga or null of not found
        /// </summary>
        /// <param name="name">The name of the manga</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call or null if could resolve synchronously (cache)</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static IAsyncResult BeginGetManga(string name, AsyncCallback resultCallback)
        {
            MediaDataBase ins = _getInstance();

            try
            {
                var query = ins._mangaCache.First(pair => pair.Value.Name.Equals(name));
                List<Manga> res = new List<Manga>(1);
                res.Add(query.Value);
                resultCallback(new Service.SearchMangaAsyncResult(res));
                return null;
            }
            catch (InvalidOperationException)// if First couldnt find anything
            {
                IAsyncResult asyncRes = Service.BeginSearch(MediaDataBase.UserAccount.UserName, MediaDataBase.UserAccount.Password, name, MediaType.Manga,
                    result =>
                    {
                        Service.SearchMangaAsyncResult mangaQuery = (Service.SearchMangaAsyncResult)result;
                        foreach (Manga m in mangaQuery.Mangas)
                        {
                            ins._mangaCache.GetOrAdd(m.Id, m);
                        }
                        try
                        {
                            var query = mangaQuery.Mangas.First(pair => pair.Name.Equals(name));
                            List<Manga> res = new List<Manga>(1);
                            res.Add(query);
                            resultCallback(new Service.SearchMangaAsyncResult(res));
                        }
                        catch (InvalidOperationException)
                        {
                            resultCallback(new Service.SearchMangaAsyncResult(null));
                        }
                    });
                return asyncRes;
            }
        }

        /// <summary>
        /// Get a manga with exactly that name synchronously
        /// </summary>
        /// <param name="name">The name of the manga</param>
        /// <returns>The manga</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public static Manga GetManga(string name)
        {
            MediaDataBase ins = _getInstance();

            try
            {
                var query = ins._mangaCache.First(pair => pair.Value.Name.Equals(name));
                return query.Value;
            }
            catch (InvalidOperationException)// if First couldnt find anything
            {
                List<Manga> mangas = GetMangas(name);

                Manga match = mangas.First(x => x.Name.Equals(name));
                return match;
            }
        }

        #endregion

        #region Internal Singleton

        /// <summary>
        /// The singleton instance of MediaDataBase
        /// </summary>
        private static MediaDataBase _instance = null;

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        /// <returns>The singleton instance</returns>
        private static MediaDataBase _getInstance()
        {
            if (_instance == null)
                _instance = new MediaDataBase();
            return _instance;
        }

        /// <summary>
        /// Default construction of a MediaDataBase
        /// </summary>
        private MediaDataBase()
        {
            _animeCache = new ConcurrentDictionary<uint, Anime>();
            _mangaCache = new ConcurrentDictionary<uint, Manga>();
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// The animes that have been cached by their id
        /// </summary>
        private ConcurrentDictionary<uint, Anime> _animeCache;

        /// <summary>
        /// The mangas that have been cached by their id
        /// </summary>
        private ConcurrentDictionary<uint, Manga> _mangaCache;

        #endregion
    }
}

