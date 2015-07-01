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
using System.Threading.Tasks;
using PCLCrypto;
using PCLStorage;

namespace jaMAL
{
    /// <summary>
    /// Account represents an user. Maintains the anime and manga list of the given user
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

        private static byte[] _deriveKey(string encryptionKey)
        {
            byte[] salt; // best initialized to a unique value for each user, and stored with the user record
            salt = new byte[8];
            for (int i = 0; i < 8; ++i)
                salt[i] = 0;
            int iterations = 5000; // higher makes brute force attacks more expensive
            int keyLengthInBytes = 16;
            byte[] key = NetFxCrypto.DeriveBytes.GetBytes(encryptionKey, salt, iterations, keyLengthInBytes);
            return key;
        }

        private static void _initCrypto(string encryptionKey, out ICryptographicKey key, out byte[] iv)
        {
            byte[] keyMaterial = _deriveKey(encryptionKey);
            var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            key = provider.CreateSymmetricKey(keyMaterial);
            iv = null; // this is optional, but must be the same for both encrypting and decrypting
        }

        public static async Task<IList<Account>> LoadCredentials(string encryptionKey)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            Task<IFolder> tFolder = rootFolder.CreateFolderAsync("userCredentials", CreationCollisionOption.OpenIfExists);
            tFolder.Wait();
            IFolder folder = tFolder.Result;

            IList<IFile> files = await folder.GetFilesAsync();

            List<Account> accounts = new List<Account>();
            foreach(IFile f in files)
            {
                System.IO.Stream stream = await f.OpenAsync(FileAccess.ReadAndWrite);
                using (stream)
                {
                    byte[] aux = new byte[256];
                    int read = stream.Read(aux, 0, 256);
                    byte[] cipherText = new byte[read];
                    for (int i = 0; i < read; ++i)
                        cipherText[i] = aux[i];

                    byte[] iv;
                    ICryptographicKey key;
                    _initCrypto(encryptionKey, out key, out iv);
                    byte[] plainText = WinRTCrypto.CryptographicEngine.Decrypt(key, cipherText, iv);
                    string readText = Encoding.UTF8.GetString(plainText, 0, plainText.Length);
                    string[] accountData = readText.Split(' ');
                    accounts.Add(new Account(accountData[0], accountData[1]));
                }
            }

            return accounts;
        }

        public async void SaveCredentials(string encryptionKey)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            Task<IFolder> tFolder = rootFolder.CreateFolderAsync("userCredentials", CreationCollisionOption.OpenIfExists);
            tFolder.Wait();
            IFolder folder = tFolder.Result;

            string name = _userName+".acc";

            IFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

            byte[] iv;
            ICryptographicKey key;
            _initCrypto(encryptionKey, out key, out iv);
            string contentText = _userName + " " + _password;
            byte[] data = Encoding.UTF8.GetBytes(contentText);
            
            byte[] cipherText = WinRTCrypto.CryptographicEngine.Encrypt(key, data, iv);

            byte[] plainText = WinRTCrypto.CryptographicEngine.Decrypt(key, cipherText, iv);
            string readText = Encoding.UTF8.GetString(plainText, 0, plainText.Length);

            System.IO.Stream stream = await file.OpenAsync(FileAccess.ReadAndWrite);
            using (stream)
            {
                stream.Write(cipherText, 0, cipherText.Length);
            }
        }

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
