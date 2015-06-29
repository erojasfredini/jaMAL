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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace jaMAL
{
    /// <summary>
    /// AnimeList is responsible to represent and manipulate the anime list of an user
    /// </summary>
    public class AnimeList : MediaList
    {
        /// <summary>
        /// Amount of watching animes 
        /// </summary>
        public uint Watching
        {
            get
            {
                return (uint)AnimeEntries.Values.Count(ae => ae.Status == MediaEntry.EntryStatus.Currently);
            }
        }

        /// <summary>
        /// Amount of completed animes 
        /// </summary>
        public uint Completed
        {
            get
            {
                return (uint)AnimeEntries.Values.Count(ae => ae.Status == MediaEntry.EntryStatus.Completed);
            }
        }

        /// <summary>
        /// Amount of animes on hold
        /// </summary>
        public uint OnHold
        {
            get
            {
                return (uint)AnimeEntries.Values.Count(ae => ae.Status == MediaEntry.EntryStatus.OnHold);
            }
        }

        /// <summary>
        /// Amount of dropped animes
        /// </summary>
        public uint Dropped
        {
            get
            {
                return (uint)AnimeEntries.Values.Count(ae => ae.Status == MediaEntry.EntryStatus.Dropped);
            }
        }

        /// <summary>
        /// Amount of animes planned to watch
        /// </summary>
        public uint PlanToConsume
        {
            get
            {
                return (uint)AnimeEntries.Values.Count(ae => ae.Status == MediaEntry.EntryStatus.PlanToWatch);
            }
        }

        #region Properties

        /// <summary>
        /// Anime list local state dictionary. The Keys are the media id and the value the entry. After modifying the local state need to call BeginSyncAnimeList or SyncAnimeList to synchronize the local state with the server
        /// </summary>
        public ObservableConcurrentDictionary<uint, AnimeEntry> AnimeEntries;

        #endregion

        #region Methods

        internal AnimeList(Account owner): base(owner)
        {
            AnimeEntries = new ObservableConcurrentDictionary<uint, AnimeEntry>();
            _lastServiceAnimeEntries = new ConcurrentDictionary<uint, AnimeEntry>();
            AnimeEntries.CollectionChanged += _onAnimeEntryCollectionChange;
        }

        public override string ToString()
        {
            string animeList = base.ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append(animeList);
            sb.AppendLine("Watching: " + Watching);
            sb.AppendLine("Completed: " + Completed);
            sb.AppendLine("OnHold: " + OnHold);
            sb.AppendLine("Dropped: " + Dropped);
            sb.AppendLine("Plan To Consume: " + PlanToConsume);
            sb.AppendLine("Days Spent Consuming: " + DaysSpentConsuming);
            sb.AppendLine();
            foreach (AnimeEntry e in AnimeEntries.Values)
            {
                sb.AppendLine("Entry: " + e.Anime.Name);
                sb.Append(e.ToString());
            }

            return sb.ToString();
        }

        #endregion

        #region Observing AnimeEntries

        /// <summary>
        /// The callback when AnimeEntries collection is modified
        /// </summary>
        /// <param name="sender">The collection</param>
        /// <param name="e">The mofication information</param>
        private void _onAnimeEntryCollectionChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            // we dont want to stop modifications to the dictionary just because we are synchronizing (also it gives serius problems!)
            //if (Synchronizing)
            //    throw new jaMALException("Tring to change AnimeEntry while synchronizing (synchronize or refresh) with the service");

            switch( e.Action )
            {
                case NotifyCollectionChangedAction.Add:
                {
                    // One or more items were added to the collection
                    if (e.NewItems != null)
                    {
                        foreach (AnimeEntry newItem in e.NewItems)
                            _addItem(newItem);
                        _refreshListProperties();
                    }
                }
                break;

                case NotifyCollectionChangedAction.Remove:
                {
                    // One or more items were removed from the collection
                    if (e.OldItems != null)
                    {
                        foreach (AnimeEntry oldItem in e.OldItems)
                            _removeItem(oldItem);
                        _refreshListProperties();
                    }
                }
                break;

                case NotifyCollectionChangedAction.Move:
                    // One or more items were moved within the collection
                    // that dont change anything for us!
                break;
                case NotifyCollectionChangedAction.Replace:
                    // One or more items were replaced in the collection
                    if (e.OldItems != null)
                    {
                        foreach (AnimeEntry oldItem in e.OldItems)
                            _removeItem(oldItem);

                        foreach (AnimeEntry newItem in e.NewItems)
                            _addItem(newItem);
                        _refreshListProperties();
                    }
                break;
                case NotifyCollectionChangedAction.Reset:
                {
                    // The content of the collection changed dramatically
                    // oh my gosh!!!
                    _refreshListProperties();
                }
                break;
            }
        }

        /// <summary>
        /// Refresh the properties of the list
        /// </summary>
        private void _refreshListProperties()
        {
            // refresh list properties
            RaisePropertyChanged("Watching");
            RaisePropertyChanged("Completed");
            RaisePropertyChanged("OnHold");
            RaisePropertyChanged("Dropped");
            RaisePropertyChanged("PlanToConsume");
        }

        /// <summary>
        /// React to the add of an item to the anime list
        /// </summary>
        /// <param name="newItem">The added anime entry</param>
        private void _addItem(AnimeEntry newItem)
        {
            // Add listener for each item on PropertyChanged event
            newItem.PropertyChanged += this._onAnimeEntryChange;
        }

        /// <summary>
        /// React to the remove of an item to the anime list
        /// </summary>
        /// <param name="oldItem">The removed anime entry</param>
        private void _removeItem(AnimeEntry oldItem)
        {
            // Remove listener for each item on PropertyChanged event
            oldItem.PropertyChanged -= this._onAnimeEntryChange;
        }

        /// <summary>
        /// The callback when any item of AnimeEntries collection is modified
        /// </summary>
        /// <param name="sender">The item</param>
        /// <param name="e">The mofication information</param>
        private void _onAnimeEntryChange(object sender, PropertyChangedEventArgs e)
        {
            switch( e.PropertyName )
            {
                // if the status was change the values of the list changed and need to be refreshed
                case "Status":
                {
                    _refreshListProperties();
                }
                break;
            }
            
        }

        #endregion

        #region Is Sync

        /// <summary>
        /// If the anime list is synchronized with the last snapshot of the anime list from the server
        /// </summary>
        public bool SeemsSynchronized
        {
            get
            {
                // check if last service anime list and current state anime list are identical
                IEnumerable<uint> keysToAdd = AnimeEntries.Keys.Except(_lastServiceAnimeEntries.Keys);
                IEnumerable<uint> keysToDelete = _lastServiceAnimeEntries.Keys.Except(AnimeEntries.Keys);
                IEnumerable<uint> keysToUpdate = AnimeEntries.Keys.Intersect(_lastServiceAnimeEntries.Keys).Where(key => !AnimeEntries[key].Equals(_lastServiceAnimeEntries[key]));

                long totalCounter = keysToAdd.Count() + keysToDelete.Count() + keysToUpdate.Count();
                return totalCounter == 0;
            }
        }

        /// <summary>
        /// If the anime list is synchronized with the the server. Be aware, is a slow check! best try with SeemsSynchronized
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                if (Synchronizing)
                    throw new jaMALException("Cannot test IsSynchronized while synchronizing or refreshing!");

                // get the actual animelist and check if really really is synchronized
                List<MediaEntry> list = Service.GetUserList(AccountOwner.UserName, AccountOwner.Password, MediaType.Anime, -1);
                Dictionary<uint, AnimeEntry> aux = new Dictionary<uint,AnimeEntry>();
                foreach (MediaEntry ae in list)
                    aux.Add(ae.Id, ae as AnimeEntry);

                IEnumerable<uint> service_keysToAdd = AnimeEntries.Keys.Except(aux.Keys);
                IEnumerable<uint> service_keysToDelete = aux.Keys.Except(AnimeEntries.Keys);
                IEnumerable<uint> service_keysToUpdate = AnimeEntries.Keys.Intersect(aux.Keys).Where(key => !AnimeEntries[key].Equals(aux[key]));

                long service_totalCounter = service_keysToAdd.Count() + service_keysToDelete.Count() + service_keysToUpdate.Count();
                return service_totalCounter == 0;
            }
        }

        #endregion

        #region Sync

        /// <summary>
        /// Sync the local anime list with myanimelist asynchronously
        /// </summary>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public List<IAsyncResult> BeginSyncAnimeList(AsyncCallback resultCallback)
        {
            if (Synchronizing)
                throw new jaMALException("Cannot synchronize while synchronizing or refreshing!");

            IEnumerable<uint> keysToAdd = AnimeEntries.Keys.Except(_lastServiceAnimeEntries.Keys);
            IEnumerable<uint> keysToDelete = _lastServiceAnimeEntries.Keys.Except(AnimeEntries.Keys);
            IEnumerable<uint> keysToUpdate = AnimeEntries.Keys.Intersect(_lastServiceAnimeEntries.Keys).Where(key => !AnimeEntries[key].Equals(_lastServiceAnimeEntries[key]));

            List<IAsyncResult> results = new List<IAsyncResult>();
            long totalCounter = keysToAdd.Count() + keysToDelete.Count() + keysToUpdate.Count();

            if( totalCounter == 0 )
            {
                if (resultCallback != null)
                    resultCallback(null);
                return results;
            }

            long currentCounter = 0;
            try 
            {
                _synchronizing = true;

                foreach (uint key in keysToAdd)
                {
                    IAsyncResult aux = _beginAddAnimeEntry(AnimeEntries[key],
                        u =>
                        {
                            Interlocked.Increment(ref currentCounter);
                            if (currentCounter == totalCounter)
                            {
                                _synchronizing = false;
                                if(resultCallback != null)
                                    resultCallback(null);
                            }
                        });
                    results.Add(aux);
                }
                foreach (uint key in keysToDelete)
                {
                    IAsyncResult aux = _beginDeleteAnimeEntry(key,
                        u =>
                        {
                            Interlocked.Increment(ref currentCounter);
                            if (currentCounter == totalCounter)
                            {
                                _synchronizing = false;
                                if (resultCallback != null)
                                    resultCallback(null);
                            }
                        });
                    results.Add(aux);
                }
                foreach (uint key in keysToUpdate)
                {
                    IAsyncResult aux = _beginUpdateAnimeEntry(AnimeEntries[key],
                        u =>
                        {
                            Interlocked.Increment(ref currentCounter);
                            if (currentCounter == totalCounter)
                            {
                                _synchronizing = false;
                                if (resultCallback != null)
                                    resultCallback(null);
                            }
                        });
                    results.Add(aux);
                }
            }catch( Exception e )
            {
                // tell that sync stopped and throw exception again
                _synchronizing = false;
                throw e;
            }
            return results;
        }

        /// <summary>
        /// Sync the local anime list with myanimelist synchronously
        /// </summary>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public void SyncAnimeList(int operationTimeout = 5000)
        {
            bool finishSyncCallback = false;
            List<IAsyncResult> results = BeginSyncAnimeList(
                r =>
                {
                    finishSyncCallback = true;
                });

            foreach (IAsyncResult ar in results)
            {
                bool finish = false;
                finish = ar.AsyncWaitHandle.WaitOne(operationTimeout);
                if (!finish)
                    throw new jaMALException("Timeout waiting for connection");
            }

            while (!finishSyncCallback)
            { }
            return;
        }

        #endregion

        #region Refresh Entries

        /// <summary>
        /// Refresh the anime list asynchronously
        /// </summary>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public IAsyncResult BeginRefreshList(AsyncCallback resultCallback)
        {
            if( Synchronizing )
                throw new jaMALException("Cannot refresh while synchronizing or refreshing!");

            IAsyncResult res = null;
            try
            {
                _refreshing = true;
                res = Service.BeginGetUserList(AccountOwner.UserName, AccountOwner.Password, MediaType.Anime,
                    result =>
                    {
                        Service.SearchUserAnimeListAsyncResult getlistResult = (Service.SearchUserAnimeListAsyncResult)result;
                        Debug.Assert(getlistResult.UserName == AccountOwner.UserName && (AccountOwner.UserID != null && AccountOwner.UserID == getlistResult.UserID));
                        _refreshWatching = getlistResult.Watching;
                        _refreshCompleted = getlistResult.Completed;
                        _refreshOnHold = getlistResult.OnHold;
                        _refreshDropped = getlistResult.Dropped;
                        _refreshPlanToConsume = getlistResult.PlanToConsume;
                        _daysSpentConsuming = getlistResult.DaysSpentConsuming;
                        _lastServiceAnimeEntries.Clear();
                        _refreshing = false;
                        AnimeEntries.Clear();
                        foreach (AnimeEntry ae in getlistResult.AnimeList)
                        {
                            if (!_lastServiceAnimeEntries.TryAdd(ae.Id, ae))
                                _lastServiceAnimeEntries[ae.Id] = ae;
                            AnimeEntries.Add(ae.Id, ae);
                        }

                        if (resultCallback != null)
                            resultCallback(getlistResult);
                    }
                );
            }catch(Exception e )
            {
                // tell that refresh stopped and throw exception again
                _refreshing = false;
                throw e;
            }
            
            return res;
        }

        /// <summary>
        /// Refresh the anime list synchronously
        /// </summary>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public void RefreshList(int operationTimeout = 5000)
        {
            if (Synchronizing)
                throw new jaMALException("Cannot refresh while synchronizing or refreshing!");

            try
            {
                _refreshing = true;
                string userName;
                uint userID, watching, completed, onHold, dropped, planToConsume;
                float daysSpentConsuming;
                List<MediaEntry> list = Service.GetUserList(AccountOwner.UserName, AccountOwner.Password, MediaType.Anime, out userID, out userName, out watching, out completed, out onHold, out dropped, out planToConsume, out daysSpentConsuming, operationTimeout);
                Debug.Assert(userName == _accountOwner.UserName && (_accountOwner.UserID != null && _accountOwner.UserID == userID), "Obtained anime list user info is different from caller user");
                _refreshWatching = watching;
                _refreshCompleted = completed;
                _refreshOnHold = onHold;
                _refreshDropped = dropped;
                _refreshPlanToConsume = planToConsume;
                _daysSpentConsuming = daysSpentConsuming;
                _lastServiceAnimeEntries.Clear();
                _refreshing = false;
                AnimeEntries.Clear();
                foreach (AnimeEntry ae in list)
                {
                    if (!_lastServiceAnimeEntries.TryAdd(ae.Id, ae))
                        _lastServiceAnimeEntries[ae.Id] = ae;
                    AnimeEntries.Add(ae.Id, ae);
                }
            }catch(Exception e )
            {
                // tell that refresh stopped and throw exception again
                _refreshing = false;
                throw e;
            }

            return;
        }

        #endregion

        #region Add Entry

        /// <summary>
        /// Add an anime to the anime list asynchronously
        /// </summary>
        /// <param name="entry">The anime entry to add</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private IAsyncResult _beginAddAnimeEntry(AnimeEntry entry, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;

            res = Service.BeginAdd(AccountOwner.UserName, AccountOwner.Password, entry, MediaType.Anime,
                result =>
                {
                    Service.AddUpdateAnimeAsyncResult animAdd = (Service.AddUpdateAnimeAsyncResult)result;
                    if (animAdd.CouldDo)
                    {
                        //AnimeEntries.Add(animAdd.AnimeEntry.Id, animAdd.AnimeEntry);;
                        _lastServiceAnimeEntries[animAdd.AnimeEntry.Id] = animAdd.AnimeEntry;
                    }
                    resultCallback(animAdd);
                }
            );

            return res;
        }

        /// <summary>
        /// Add an anime to the anime list synchronously
        /// </summary>
        /// <param name="entry">The anime entry to add</param>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <returns>True if could add or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private bool _addAnimeEntry(AnimeEntry entry, int operationTimeout = 5000)
        {
            bool couldAddUpdate = false;

            couldAddUpdate = Service.Add(AccountOwner.UserName, AccountOwner.Password, entry, MediaType.Anime, operationTimeout);

            if (couldAddUpdate)
            {
                //AnimeEntries.Add(entry.Id, entry);
                _lastServiceAnimeEntries[entry.Id] = entry;
            }

            return couldAddUpdate;
        }

        #endregion

        #region Update Entry

        /// <summary>
        /// Add an anime to the anime list asynchronously
        /// </summary>
        /// <param name="entry">The anime entry to add</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private IAsyncResult _beginUpdateAnimeEntry(AnimeEntry entry, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;

            res = Service.BeginUpdate(AccountOwner.UserName, AccountOwner.Password, entry, MediaType.Anime,
                    result =>
                    {
                        Service.AddUpdateAnimeAsyncResult animUpdate = (Service.AddUpdateAnimeAsyncResult)result;
                        if (animUpdate.CouldDo)
                        {
                            //AnimeEntries.Add(animUpdate.AnimeEntry.Id, animUpdate.AnimeEntry);;
                            _lastServiceAnimeEntries[animUpdate.AnimeEntry.Id] = animUpdate.AnimeEntry;
                        }
                        resultCallback(animUpdate);
                    }
                );

            return res;
        }

        /// <summary>
        /// Add an anime to the anime list synchronously
        /// </summary>
        /// <param name="entry">The anime entry to add</param>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <returns>True if could add or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private bool _updateAnimeEntry(AnimeEntry entry, int operationTimeout = 5000)
        {
            bool couldAddUpdate = false;

            couldAddUpdate = Service.Update(AccountOwner.UserName, AccountOwner.Password, entry, MediaType.Anime, operationTimeout);

            if (couldAddUpdate)
            {
                //AnimeEntries.Add(entry.Id, entry);
                _lastServiceAnimeEntries[entry.Id] = entry;
            }

            return couldAddUpdate;
        }

        #endregion

        #region Remove Entry

        /// <summary>
        /// Delete an anime from the anime list asynchronously
        /// </summary>
        /// <param name="animeId">The id of the anime to delete</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private IAsyncResult _beginDeleteAnimeEntry(uint animeId, AsyncCallback resultCallback)
        {
            IAsyncResult res = Service.BeginDelete(AccountOwner.UserName, AccountOwner.Password, animeId, MediaType.Anime,
                result =>
                {
                    Service.DeleteAnimeAsyncResult animDelete = (Service.DeleteAnimeAsyncResult)result;
                    if (animDelete.CouldDelete)
                    {
                        //AnimeEntries.Remove(animDelete.AnimeId);
                        AnimeEntry dummy;
                        _lastServiceAnimeEntries.TryRemove(animDelete.AnimeId, out dummy);
                    }
                    resultCallback(animDelete);
                }
            );
            return res;
        }

        /// <summary>
        /// Delete an anime from the anime list synchronously
        /// </summary>
        /// <param name="animeId">The id of the anime to delete</param>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <returns>True if could delete or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private bool _deleteAnimeEntry(uint animeId, int operationTimeout = 5000)
        {
            bool couldDelete = Service.Delete(AccountOwner.UserName, AccountOwner.Password, animeId, MediaType.Anime, operationTimeout);
            if (couldDelete)
            {
                //AnimeEntries.Remove(animeId);
                AnimeEntry dummy;
                _lastServiceAnimeEntries.TryRemove(animeId, out dummy);
            }

            return couldDelete;
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// The last snapshot of the anime list from the server that we know about
        /// </summary>
        private ConcurrentDictionary<uint, AnimeEntry> _lastServiceAnimeEntries;

        #endregion
    }
}
