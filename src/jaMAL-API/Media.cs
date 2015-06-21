using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Threading;

namespace jaMAL
{
    public class Media : INotifyPropertyChanged
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

        protected string _englishName;
        public string EnglishName
        {
            get
            {
                return _englishName;
            }
            internal set
            {
                _englishName = value;
                RaisePropertyChanged("EnglishName");
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

        protected float _score;
        public float Score
        {
            get
            {
                return _score;
            }
            internal set
            {
                _score = value;
                RaisePropertyChanged("Score");
            }
        }

        protected string _synopsis;
        public string Synopsis
        {
            get
            {
                return _synopsis;
            }
            internal set
            {
                _synopsis = value;
                RaisePropertyChanged("Synopsis");
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

        public enum MediaStatus { Airing, Finish }
        protected MediaStatus _status;
        public MediaStatus Status
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

        public enum MediaType { TV, Movie, OVA, Special, Manga, OneShot, Novel }
        protected MediaType _type;
        public MediaType Type
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

        internal Media()
        { }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Anime return false.
            Media m = obj as Media;
            if ((System.Object)m == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (_id == m._id) && _name.Equals(m._name) && _synonyms.Equals(m._synonyms) && (_score == m._score) && _synopsis.Equals(m._synopsis) && _logoImageURI.Equals(m._logoImageURI) && (_status == m._status) && (_type == m._type) && _startDate.Equals(m._startDate) && _endDate.Equals(m._endDate);
        }

        public bool Equals(Media m)
        {
            // If parameter is null return false:
            if ((object)m == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (_id == m._id) && _name.Equals(m._name) && _synonyms.Equals(m._synonyms) && (_score == m._score) && _synopsis.Equals(m._synopsis) && _logoImageURI.Equals(m._logoImageURI) && (_status == m._status) && (_type == m._type) && _startDate.Equals(m._startDate) && _endDate.Equals(m._endDate);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " + _name);
            sb.AppendLine("English: " + _englishName);
            sb.AppendLine("Id: " + _id);
            sb.AppendLine("Score: " + _score);
            if( _synonyms != null )
                sb.AppendLine("Synonyms: " + _synonyms.ToString());
            sb.AppendLine("Type: " + _type.ToString());
            sb.AppendLine("Status: " + _status.ToString());
            sb.AppendLine("StartDate: " + _startDate.ToString());
            sb.AppendLine("EndDate: " + _endDate.ToString());
            sb.AppendLine("Synopsis: " + _synopsis);
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
