using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jaMAL
{
    public class AnimeBrief : MediaBrief
    {
        #region Properties

        protected uint _episodes;
        public uint Episodes
        {
            get
            {
                return _episodes;
            }
            internal set
            {
                _episodes = value;
                RaisePropertyChanged("Episodes");
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return base.ToString();
        }

        internal AnimeBrief(uint id, string name)
        {
            _id = id;
            _name = name;
        }

        internal AnimeBrief(Anime anime)
        {
            _id = anime.Id;
            _name = anime.Name;
            _synonyms = anime.Synonyms;
            _logoImageURI = anime.LogoImageURI;
            _status = anime.Status;
            _type = anime.Type;
            _startDate = anime.StartDate;
            _endDate = anime.EndDate;
            _episodes = anime.Episodes;
        }
        #endregion
    }
}
