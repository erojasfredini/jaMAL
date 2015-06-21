using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaMAL
{
    public class MangaBrief: MediaBrief
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

        public override string ToString()
        {
            return base.ToString();
        }

        internal MangaBrief(uint id, string name)
        {
            _id = id;
            _name = name;
        }

        internal MangaBrief(Manga manga)
        {
            _id = manga.Id;
            _name = manga.Name;
            _synonyms = manga.Synonyms;
            _logoImageURI = manga.LogoImageURI;
            _status = manga.Status;
            _type = manga.Type;
            _startDate = manga.StartDate;
            _endDate = manga.EndDate;
            _chapter = manga.Chapter;
            _volume = manga.Volume;
        }
        #endregion
    }
}
