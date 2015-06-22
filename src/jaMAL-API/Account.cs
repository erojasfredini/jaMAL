/****************************************************************************
 * jaMAL
 * Copyright (c) Emmanuel Rojas Fredini, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
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

namespace jaMAL
{
    /// <summary>
    /// Represents an user
    /// </summary>
    public class Account
    {
        #region Properties

        /// <summary>
        /// The default user Account
        /// </summary>
        public static Account DefaultUser = new Account("jaMALTestAccount", "jaMALTestAccount");

        protected readonly string _userName;

        /// <summary>
        /// Username
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }
        }

        protected readonly string _password;

        /// <summary>
        /// User password
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
        }

        private Nullable<uint> _userId;

        /// <summary>
        /// User id. Could be null if the account is not verified or the anime/manga list is not retrived yet
        /// </summary>
        public Nullable<uint> UserID
        {
            get
            {
                return _userId;
            }
            protected set
            {
                _userId = value;
            }
        }

        protected bool _verified;
        public bool Verified
        {
            get
            {
                return _verified;
            }
        }

        /// <summary>
        /// The user account anime list
        /// </summary>
        public AnimeList UserAnimeList;

        /// <summary>
        /// The user account manga list
        /// </summary>
        public MangaList UserMangaList;

        #endregion

        #region Methods

        /// <summary>
        /// Account constructor
        /// </summary>
        public Account(string userName, string password)
        {
            _userName = userName;
            _password = password;
            _userId = null;
            UserAnimeList = new AnimeList(this);
            UserMangaList = new MangaList(this);
            _verified = false;
        }

        public IAsyncResult BeginVerifyAccount(AsyncCallback verifiedCallback)
        {
            IAsyncResult res = Service.BeginVerifyCredentials(this, result =>
            {
                Service.VerifyCredentialsAsyncResult verifyResult = (Service.VerifyCredentialsAsyncResult)result;
                _verified = verifyResult.CouldVerify;
                if( _verified )
                    _userId = verifyResult.AccountId;
                if (verifiedCallback != null)
                    verifiedCallback(verifyResult);
            });
            return res;
        }

        public bool VerifyAccount()
        {
            _verified = Service.VerifyCredentials(this, out _userId);
            return _verified;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UserName: " + _userName);
            sb.AppendLine("Password: " + _password);
            sb.AppendLine("User Id: " + (_verified? _userId.ToString(): "?"));
            sb.AppendLine("Verified Account: " + _verified.ToString());
            sb.AppendLine("");
            sb.AppendLine("Anime List:");
            sb.AppendLine(UserAnimeList.ToString());
            sb.AppendLine("");
            sb.AppendLine("Manga List:");
            sb.AppendLine(UserMangaList.ToString());
            return sb.ToString();
        }

        #endregion

        #region Internal Properties

        

        #endregion

        
    }
}
