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
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace jaMAL
{
    /// <summary>
    /// Base class for anime and manga lists
    /// </summary>
    public  class MediaList : INotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        /// The account owner of this list
        /// </summary>
        public Account AccountOwner
        {
            get
            {
                return _accountOwner;
            }
        }

        /// <summary>
        /// Amount of days consuming the media
        /// </summary>
        public float DaysSpentConsuming
        {
            get
            {
                return _daysSpentConsuming;
            }
        }

        /// <summary>
        /// Tells if the list is currently synchronizing with the service
        /// </summary>
        public bool Synchronizing
        {
            get
            {
                return (_synchronizing || _refreshing);
            }
        }

        #endregion

        #region Methods

        internal MediaList(Account owner)
        {
            _accountOwner = owner;
            _synchronizing = false;
            _refreshing = false;
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UserID: " + _accountOwner.UserID);
            sb.AppendLine("UserName: " + _accountOwner.UserName);
            return sb.ToString();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Ower account
        /// </summary>
        protected readonly Account _accountOwner;

        /// <summary>
        /// Days consuming the media
        /// </summary>
        protected float _daysSpentConsuming;

        /// <summary>
        /// The watching entries at last list refresh (only at refresh, not maintain)
        /// </summary>
        protected uint _refreshWatching;

        /// <summary>
        /// The completed entries at last list refresh (only at refresh, not maintain)
        /// </summary>
        protected uint _refreshCompleted;

        /// <summary>
        /// The completed entries at last list refresh (only at refresh, not maintain)
        /// </summary>
        protected uint _refreshOnHold;

        /// <summary>
        /// The dropped entries at last list refresh (only at refresh, not maintain)
        /// </summary>
        protected uint _refreshDropped;

        /// <summary>
        /// The planed to consume entries at last list refresh (only at refresh, not maintain)
        /// </summary>
        protected uint _refreshPlanToConsume;

        /// <summary>
        /// True if actually synchronizing
        /// </summary>
        protected bool _synchronizing;

        /// <summary>
        /// True if actually refreshing
        /// </summary>
        protected bool _refreshing;

        #endregion
    }
}
