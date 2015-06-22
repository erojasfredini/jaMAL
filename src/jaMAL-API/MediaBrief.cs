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
using System.Threading.Tasks;

namespace jaMAL
{
    public class MediaBrief : INotifyPropertyChanged
    {
        #region Properties

        protected uint _id;
        public uint Id
        {
            get
            {
                return _id;
            }
            internal set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        protected string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            internal set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        protected string _synonyms;
        public string Synonyms
        {
            get
            {
                return _synonyms;
            }
            internal set
            {
                _synonyms = value;
                RaisePropertyChanged("Synonyms");
            }
        }
        
        protected string _logoImageURI;
        public string LogoImageURI
        {
            get
            {
                return _logoImageURI;
            }
            internal set
            {
                _logoImageURI = value;
                RaisePropertyChanged("LogoImageURI");
            }
        }

        protected byte[] _logoImage;
        public byte[] LogoImage
        {
            get
            {
                return _logoImage;
            }
            internal set
            {
                _logoImage = value;
                RaisePropertyChanged("LogoImage");
            }
        }

        protected Media.MediaStatus _status;
        public Media.MediaStatus Status
        {
            get
            {
                return _status;
            }
            internal set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        protected Media.MediaType _type;
        public Media.MediaType Type
        {
            get
            {
                return _type;
            }
            internal set
            {
                _type = value;
                RaisePropertyChanged("Type");
            }
        }

        protected DateTime _startDate;
        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            internal set
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
            internal set
            {
                _endDate = value;
                RaisePropertyChanged("EndDate");
            }
        }

        #endregion

        #region Methods

        internal MediaBrief()
        { }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " + _name);
            sb.AppendLine("Id: " + _id);
            if( _synonyms != null )
                sb.AppendLine("Synonyms: " + _synonyms.ToString());
            sb.AppendLine("Type: " + _type.ToString());
            sb.AppendLine("Status: " + _status.ToString());
            sb.AppendLine("StartDate: " + _startDate.ToString());
            sb.AppendLine("EndDate: " + _endDate.ToString());
            sb.AppendLine("LogoImageURI: " + _logoImageURI);
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
    }
}
