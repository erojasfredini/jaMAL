using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace jaMAL
{
    public class AnimeEntry : MediaEntry
    {
        #region Properties

        public readonly AnimeBrief Anime;

        public override uint Id
        {
            get
            {
                return Anime.Id;
            }
            set
            {
                Anime.Id = value;
                RaisePropertyChanged("Id");
            }
        }

        private uint _episode;
        public uint Episode
        {
            get
            {
                return _episode;
            }
            set
            {
                _episode = value;
                RaisePropertyChanged("Episode");
            }
        }



        private uint _rewatching;
        public uint Rewatching
        {
            get
            {
                return _rewatching;
            }
            set
            {
                _rewatching = value;
                RaisePropertyChanged("Rewatching");
            }
        }

        private uint _rewatchingEpisode;
        public uint RewatchingEpisode
        {
            get
            {
                return _rewatchingEpisode;
            }
            set
            {
                _rewatchingEpisode = value;
                RaisePropertyChanged("RewatchingEpisode");
            }
        }



        #endregion

        #region Methods

        public AnimeEntry(Anime anime, uint episode, EntryStatus status, uint score = 0, DateTime startDate = default(DateTime), DateTime endDate = default(DateTime), uint rewatching = 0, uint rewatchingEpisode = 0) 
        {
            Anime     = new AnimeBrief(anime);
            _episode  = episode;
            Debug.Assert( !(Anime.Status == Media.MediaStatus.Airing && status == EntryStatus.Completed), "Could't have seen completly an anime if it haven't finish airing yet!");
            // dont be a jerk, you could't have  seen it all! - also myanimelist corrects that and makes our cache be un-sync!
            if( Anime.Status == Media.MediaStatus.Airing && status == EntryStatus.Completed )
                _status = EntryStatus.Currently;
            else
                _status   = status;
            _score    = score;
            _rewatching = rewatching;
            _rewatchingEpisode = rewatchingEpisode;
            _startDate = startDate;
            _endDate = endDate;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to AnimeEntry return false.
            AnimeEntry ae = obj as AnimeEntry;
            if ((System.Object)ae == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(obj) && (_episode == ae._episode) && (_rewatching == ae._rewatching) && (_rewatchingEpisode == ae._rewatchingEpisode);
        }

        public bool Equals(AnimeEntry ae)
        {
            // If parameter is null return false:
            if ((object)ae == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(ae) && (_episode == ae._episode) && (_rewatching == ae._rewatching) && (_rewatchingEpisode == ae._rewatchingEpisode);
        }

        public override string ToString()
        {
            string mangaEntry = base.ToString();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Episode: " + _episode.ToString());
            sb.AppendLine("Rewatching: " + _rewatching.ToString());
            sb.AppendLine("Rewatching Episode: " + _rewatchingEpisode.ToString());
            sb.AppendLine("Anime:");
            sb.AppendLine(Anime.ToString());
            return mangaEntry + sb.ToString();
        }

        public override string ExportXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<entry>");
            sb.AppendFormat("<episode>{0}</episode>", Episode); sb.AppendLine();
            sb.AppendFormat("<status>{0}</status>", Status); sb.AppendLine();
            sb.AppendFormat("<score>{0}</score>", Score); sb.AppendLine();
            sb.AppendLine("<downloaded_episodes></downloaded_episodes>");
            sb.AppendLine("<storage_type></storage_type>");
            sb.AppendLine("<storage_value></storage_value>");
            sb.AppendLine("<times_rewatched></times_rewatched>");
            sb.AppendLine("<rewatch_value></rewatch_value>");
            sb.AppendLine("<date_start></date_start>");
            sb.AppendLine("<date_finish></date_finish>");
            sb.AppendLine("<priority></priority>");
            sb.AppendLine("<enable_discussion></enable_discussion>");
            sb.AppendLine("<enable_rewatching></enable_rewatching>");
            sb.AppendLine("<comments></comments>");
            sb.AppendLine("<fansub_group></fansub_group>");
            sb.AppendLine("<tags></tags>");
            sb.AppendLine("</entry>");

            return sb.ToString();
        }

        #endregion

    }
}
