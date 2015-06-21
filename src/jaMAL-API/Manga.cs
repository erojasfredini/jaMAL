using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jaMAL
{
    public class Manga : Media
    {
        #region Properties

        private uint _chapter;
        public uint Chapter
        {
            get
            {
                return _chapter;
            }
            internal set
            {
                _chapter = value;
                RaisePropertyChanged("Chapter");
            }
        }

        private uint _volume;
        public uint Volume
        {
            get
            {
                return _volume;
            }
            internal set
            {
                _volume = value;
                RaisePropertyChanged("Volume");
            }
        }

        #endregion

        #region Methods

        internal Manga(uint id, string name)
        {
            _id = id;
            _name = name;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Manga return false.
            Manga m = obj as Manga;
            if ((System.Object)m == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(m) && (_chapter == m._chapter) && (_volume == m._volume);
        }

        public bool Equals(Manga m)
        {
            // If parameter is null return false:
            if ((object)m == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(m) && (_chapter == m._chapter) && (_volume == m._volume);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion
    }
}
