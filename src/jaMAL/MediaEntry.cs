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
using System.ComponentModel;

namespace jaMAL
{
    /// <summary>
    /// MediaEntry is the generalization of an anime entry or manga entry
    /// </summary>
    public abstract class MediaEntry : INotifyPropertyChanged
    {
        #region Properties


        public abstract uint Id
        {
            get;
            set;
        }

        public enum EntryStatus { Currently, Completed, OnHold, Dropped, PlanToWatch}
        protected EntryStatus _status;
        public EntryStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        protected uint _score;
        public uint Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
                RaisePropertyChanged("Score");
            }
        }

        protected DateTime _startDate;
        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                _startDate = value;
                RaisePropertyChanged("StartDate");
            }
        }

        protected DateTime _endDate;
        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
                RaisePropertyChanged("EndDate");
            }
        }

        protected uint _lastUpdated;
        public uint LastUpdated
        {
            get
            {
                return _lastUpdated;
            }
            set
            {
                _lastUpdated = value;
                RaisePropertyChanged("LastUpdated");
            }
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

        #region Methods

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to MediaEntry return false.
            MediaEntry me = obj as MediaEntry;
            if ((System.Object)me == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Id == me.Id) && (_status == me._status) && (_score == me._score) && _startDate.Equals(me._startDate) && _endDate.Equals(me._endDate);// do not compare _lastUpdated, it changes duhh!
        }

        public bool Equals(MediaEntry me)
        {
            // If parameter is null return false:
            if ((object)me == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Id == me.Id) && (_status == me._status) && (_score == me._score) && (_startDate == me._startDate) && (_endDate == me._endDate);// do not compare _lastUpdated, it changes duhh!
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Id: " + Id);
            sb.AppendLine("Status: " + _status.ToString());
            sb.AppendLine("Score: " + _score);
            sb.AppendLine("Start Date: " + _startDate.ToString());
            sb.AppendLine("End Date: " + _endDate.ToString());
            sb.AppendLine("LastUpdated: " + _lastUpdated.ToString());
            return sb.ToString();
        }

        public abstract string ExportXML();

        #endregion

    }
}
