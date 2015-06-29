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
    /// MangaList is responsible to represent and manipulate the manga list of an user
    /// </summary>
    public class MangaList : MediaList
    {
        /// <summary>
        /// Amount of reading mangas
        /// </summary>
        public uint Reading
        {
            get
            {
                return (uint)MangaEntries.Values.Count(me => me.Status == MediaEntry.EntryStatus.Currently);
            }
        }

        /// <summary>
        /// Amount of completed mangas 
        /// </summary>
        public uint Completed
        {
            get
            {
                return (uint)MangaEntries.Values.Count(me => me.Status == MediaEntry.EntryStatus.Completed);
            }
        }

        /// <summary>
        /// Amount of mangas on hold
        /// </summary>
        public uint OnHold
        {
            get
            {
                return (uint)MangaEntries.Values.Count(me => me.Status == MediaEntry.EntryStatus.OnHold);
            }
        }

        /// <summary>
        /// Amount of dropped mangas
        /// </summary>
        public uint Dropped
        {
            get
            {
                return (uint)MangaEntries.Values.Count(me => me.Status == MediaEntry.EntryStatus.Dropped);
            }
        }

        /// <summary>
        /// Amount of mangas planned to watch
        /// </summary>
        public uint PlanToConsume
        {
            get
            {
                return (uint)MangaEntries.Values.Count(me => me.Status == MediaEntry.EntryStatus.PlanToWatch);
            }
        }

        #region Properties

        /// <summary>
        /// Manga list local state dictionary. The Keys are the media id and the value the entry. After modifying the local state need to call BeginSyncMangaList or SyncMangaList to synchronize the local state with the server
        /// </summary>
        public ObservableConcurrentDictionary<uint, MangaEntry> MangaEntries;

        #endregion

        #region Methods

        internal MangaList(Account owner): base(owner)
        {
            MangaEntries = new ObservableConcurrentDictionary<uint, MangaEntry>();
            _lastServiceMangaEntries = new ConcurrentDictionary<uint, MangaEntry>();
            MangaEntries.CollectionChanged += _onMangaEntryCollectionChange;
        }

        public override string ToString()
        {
            string mangaList = base.ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append(mangaList);
            sb.AppendLine("Reading: " + Reading);
            sb.AppendLine("Completed: " + Completed);
            sb.AppendLine("OnHold: " + OnHold);
            sb.AppendLine("Dropped: " + Dropped);
            sb.AppendLine("Plan To Consume: " + PlanToConsume);
            sb.AppendLine("Days Spent Consuming: " + DaysSpentConsuming);
            sb.AppendLine();
            foreach (MangaEntry e in MangaEntries.Values)
            {
                sb.AppendLine("Entry: " + e.Manga.Name);
                sb.Append(e.ToString());
            }

            return sb.ToString();
        }

        #endregion

        #region Observing MangaEntries

        /// <summary>
        /// The callback when MangaEntries collection is modified
        /// </summary>
        /// <param name="sender">The collection</param>
        /// <param name="e">The mofication information</param>
        private void _onMangaEntryCollectionChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            // we dont want to stop modifications to the dictionary just because we are synchronizing (also it gives serius problems!)
            //if (Synchronizing)
            //    throw new jaMALException("Tring to change MangaEntry while synchronizing (synchronize or refresh) with the service");

            switch( e.Action )
            {
                case NotifyCollectionChangedAction.Add:
                {
                    // One or more items were added to the collection
                    if (e.NewItems != null)
                    {
                        foreach (MangaEntry newItem in e.NewItems)
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
                        foreach (MangaEntry oldItem in e.OldItems)
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
                        foreach (MangaEntry oldItem in e.OldItems)
                            _removeItem(oldItem);

                        foreach (MangaEntry newItem in e.NewItems)
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
            RaisePropertyChanged("Reading");
            RaisePropertyChanged("Completed");
            RaisePropertyChanged("OnHold");
            RaisePropertyChanged("Dropped");
            RaisePropertyChanged("PlanToConsume");
        }

        /// <summary>
        /// React to the add of an item to the manga list
        /// </summary>
        /// <param name="newItem">The added manga entry</param>
        private void _addItem(MangaEntry newItem)
        {
            // Add listener for each item on PropertyChanged event
            newItem.PropertyChanged += this._onMangaEntryChange;
        }

        /// <summary>
        /// React to the remove of an item to the manga list
        /// </summary>
        /// <param name="oldItem">The removed manga entry</param>
        private void _removeItem(MangaEntry oldItem)
        {
            // Remove listener for each item on PropertyChanged event
            oldItem.PropertyChanged -= this._onMangaEntryChange;
        }

        /// <summary>
        /// The callback when any item of MangaEntries collection is modified
        /// </summary>
        /// <param name="sender">The item</param>
        /// <param name="e">The mofication information</param>
        private void _onMangaEntryChange(object sender, PropertyChangedEventArgs e)
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
        /// If the manga list is synchronized with the last snapshot of the manga list from the server
        /// </summary>
        public bool SeemsSynchronized
        {
            get
            {
                // check if last service manga list and current state manga list are identical
                IEnumerable<uint> keysToAdd = MangaEntries.Keys.Except(_lastServiceMangaEntries.Keys);
                IEnumerable<uint> keysToDelete = _lastServiceMangaEntries.Keys.Except(MangaEntries.Keys);
                IEnumerable<uint> keysToUpdate = MangaEntries.Keys.Intersect(_lastServiceMangaEntries.Keys).Where(key => !MangaEntries[key].Equals(_lastServiceMangaEntries[key]));

                long totalCounter = keysToAdd.Count() + keysToDelete.Count() + keysToUpdate.Count();
                return totalCounter == 0;
            }
        }

        /// <summary>
        /// If the manga list is synchronized with the the server. Be aware, is a slow check! best try with SeemsSynchronized
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                if (Synchronizing)
                    throw new jaMALException("Cannot test IsSynchronized while synchronizing or refreshing!");

                // get the actual mangalist and check if really really is synchronized
                List<MediaEntry> list = Service.GetUserList(AccountOwner.UserName, AccountOwner.Password, MediaType.Manga, -1);
                Dictionary<uint, MangaEntry> aux = new Dictionary<uint, MangaEntry>();
                foreach (MediaEntry me in list)
                    aux.Add(me.Id, me as MangaEntry);

                IEnumerable<uint> service_keysToAdd = MangaEntries.Keys.Except(aux.Keys);
                IEnumerable<uint> service_keysToDelete = aux.Keys.Except(MangaEntries.Keys);
                IEnumerable<uint> service_keysToUpdate = MangaEntries.Keys.Intersect(aux.Keys).Where(key => !MangaEntries[key].Equals(aux[key]));

                long service_totalCounter = service_keysToAdd.Count() + service_keysToDelete.Count() + service_keysToUpdate.Count();
                return service_totalCounter == 0;
            }
        }

        #endregion

        #region Sync

        /// <summary>
        /// Sync the local manga list with myanimelist asynchronously
        /// </summary>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public List<IAsyncResult> BeginSyncMangaList(AsyncCallback resultCallback)
        {
            if (Synchronizing)
                throw new jaMALException("Cannot synchronize while synchronizing or refreshing!");

            IEnumerable<uint> keysToAdd = MangaEntries.Keys.Except(_lastServiceMangaEntries.Keys);
            IEnumerable<uint> keysToDelete = _lastServiceMangaEntries.Keys.Except(MangaEntries.Keys);
            IEnumerable<uint> keysToUpdate = MangaEntries.Keys.Intersect(_lastServiceMangaEntries.Keys).Where(key => !MangaEntries[key].Equals(_lastServiceMangaEntries[key]));

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
                    IAsyncResult aux = _beginAddMangaEntry(MangaEntries[key],
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
                    IAsyncResult aux = _beginDeleteMangaEntry(key,
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
                    IAsyncResult aux = _beginUpdateMangaEntry(MangaEntries[key],
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
        /// Sync the local manga list with myanimelist synchronously
        /// </summary>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        public void SyncMangaList(int operationTimeout = 5000)
        {
            bool finishSyncCallback = false;
            List<IAsyncResult> results = BeginSyncMangaList(
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
        /// Refresh the manga list asynchronously
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
                res = Service.BeginGetUserList(AccountOwner.UserName, AccountOwner.Password, MediaType.Manga,
                    result =>
                    {
                        Service.SearchUserMangaListAsyncResult getlistResult = (Service.SearchUserMangaListAsyncResult)result;
                        Debug.Assert(getlistResult.UserName == _accountOwner.UserName && (_accountOwner.UserID != null && _accountOwner.UserID == getlistResult.UserID));
                        _refreshWatching = getlistResult.Watching;
                        _refreshCompleted = getlistResult.Completed;
                        _refreshOnHold = getlistResult.OnHold;
                        _refreshDropped = getlistResult.Dropped;
                        _refreshPlanToConsume = getlistResult.PlanToConsume;
                        _daysSpentConsuming = getlistResult.DaysSpentConsuming;
                        _lastServiceMangaEntries.Clear();
                        _refreshing = false;
                        MangaEntries.Clear();
                        foreach (MangaEntry me in getlistResult.MangaList)
                        {
                            if (!_lastServiceMangaEntries.TryAdd(me.Id, me))
                                _lastServiceMangaEntries[me.Id] = me;
                            MangaEntries.Add(me.Id, me);
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
        /// Refresh the manga list synchronously
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
                List<MediaEntry> list = Service.GetUserList(AccountOwner.UserName, AccountOwner.Password, MediaType.Manga, out userID, out userName, out watching, out completed, out onHold, out dropped, out planToConsume, out daysSpentConsuming, operationTimeout);
                Debug.Assert(userName == _accountOwner.UserName && (_accountOwner.UserID != null && _accountOwner.UserID == userID), "Obtained manga list user info is different from caller user");
                _refreshWatching = watching;
                _refreshCompleted = completed;
                _refreshOnHold = onHold;
                _refreshDropped = dropped;
                _refreshPlanToConsume = planToConsume;
                _daysSpentConsuming = daysSpentConsuming;
                _lastServiceMangaEntries.Clear();
                _refreshing = false;
                MangaEntries.Clear();
                foreach (MangaEntry me in list)
                {
                    if (!_lastServiceMangaEntries.TryAdd(me.Id, me))
                        _lastServiceMangaEntries[me.Id] = me;
                    MangaEntries.Add(me.Id, me);
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
        /// Add a manga to the manga list asynchronously
        /// </summary>
        /// <param name="entry">The manga entry to add</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private IAsyncResult _beginAddMangaEntry(MangaEntry entry, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;

            res = Service.BeginAdd(AccountOwner.UserName, AccountOwner.Password, entry, MediaType.Manga,
                result =>
                {
                    Service.AddUpdateMangaAsyncResult mangaAdd = (Service.AddUpdateMangaAsyncResult)result;
                    if (mangaAdd.CouldDo)
                    {
                        //MangaEntries.Add(mangaAdd.MangaEntry.Id, mangaAdd.MangaEntry);;
                        _lastServiceMangaEntries[mangaAdd.MangaEntry.Id] = mangaAdd.MangaEntry;
                    }
                    resultCallback(mangaAdd);
                }
            );

            return res;
        }

        /// <summary>
        /// Add a manga to the manga list synchronously
        /// </summary>
        /// <param name="entry">The manga entry to add</param>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <returns>True if could add or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private bool _addMangaEntry(MangaEntry entry, int operationTimeout = 5000)
        {
            bool couldAddUpdate = false;

            couldAddUpdate = Service.Add(AccountOwner.UserName, AccountOwner.Password, entry, MediaType.Manga, operationTimeout);

            if (couldAddUpdate)
            {
                //MangaEntries.Add(entry.Id, entry);
                _lastServiceMangaEntries[entry.Id] = entry;
            }

            return couldAddUpdate;
        }

        #endregion

        #region Update Entry

        /// <summary>
        /// Update a manga to the manga list asynchronously
        /// </summary>
        /// <param name="entry">The manga entry to add</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private IAsyncResult _beginUpdateMangaEntry(MangaEntry entry, AsyncCallback resultCallback)
        {
            IAsyncResult res = null;

            res = Service.BeginUpdate(AccountOwner.UserName, AccountOwner.Password, entry, MediaType.Manga,
                    result =>
                    {
                        Service.AddUpdateMangaAsyncResult mangaUpdate = (Service.AddUpdateMangaAsyncResult)result;
                        if (mangaUpdate.CouldDo)
                        {
                            //MangaEntries.Add(mangaUpdate.MangaEntry.Id, mangaUpdate.MangaEntry);;
                            _lastServiceMangaEntries[mangaUpdate.MangaEntry.Id] = mangaUpdate.MangaEntry;
                        }
                        resultCallback(mangaUpdate);
                    }
                );

            return res;
        }

        /// <summary>
        /// Add a manga to the manga list synchronously
        /// </summary>
        /// <param name="entry">The manga entry to add</param>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <returns>True if could add or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private bool _updateMangaEntry(MangaEntry entry, int operationTimeout = 5000)
        {
            bool couldAddUpdate = false;

            couldAddUpdate = Service.Update(AccountOwner.UserName, AccountOwner.Password, entry, MediaType.Manga, operationTimeout);

            if (couldAddUpdate)
            {
                //MangaEntries.Add(entry.Id, entry);
                _lastServiceMangaEntries[entry.Id] = entry;
            }

            return couldAddUpdate;
        }

        #endregion

        #region Remove Entry

        /// <summary>
        /// Delete a manga from the manga list asynchronously
        /// </summary>
        /// <param name="mangaId">The id of the manga to delete</param>
        /// <param name="resultCallback">The callback to execute at the end of the process</param>
        /// <returns>The IAsyncResult of the call</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private IAsyncResult _beginDeleteMangaEntry(uint mangaId, AsyncCallback resultCallback)
        {
            IAsyncResult res = Service.BeginDelete(AccountOwner.UserName, AccountOwner.Password, mangaId, MediaType.Manga,
                result =>
                {
                    Service.DeleteMangaAsyncResult mangaDelete = (Service.DeleteMangaAsyncResult)result;
                    if (mangaDelete.CouldDelete)
                    {
                        //MangaEntries.Remove(mangaDelete.MangaId);
                        MangaEntry dummy;
                        _lastServiceMangaEntries.TryRemove(mangaDelete.MangaId, out dummy);
                    }
                    resultCallback(mangaDelete);
                }
            );
            return res;
        }

        /// <summary>
        /// Delete a manga from the manga list synchronously
        /// </summary>
        /// <param name="mangaId">The id of the manga to delete</param>
        /// <param name="timeout">The miliseconds to wait for an answer for the sync (-1 to wait forever). Default 5 seconds</param>
        /// <returns>True if could delete or false otherwise</returns>
        /// <exception cref="jaMal.jaMALException">Something went wrong with the web request</exception>
        private bool _deleteMangaEntry(uint mangaId, int operationTimeout = 5000)
        {
            bool couldDelete = Service.Delete(AccountOwner.UserName, AccountOwner.Password, mangaId, MediaType.Manga, operationTimeout);
            if (couldDelete)
            {
                //MangaEntries.Remove(mangaId);
                MangaEntry dummy;
                _lastServiceMangaEntries.TryRemove(mangaId, out dummy);
            }

            return couldDelete;
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// The last snapshot of the manga list from the server that we know about
        /// </summary>
        private ConcurrentDictionary<uint, MangaEntry> _lastServiceMangaEntries;

        #endregion
    }
}
