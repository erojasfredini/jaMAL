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
    /// Entry of a manga list
    /// </summary>
    public class MangaEntry : MediaEntry
    {
        #region Properties

        public readonly MangaBrief Manga;

        public override uint Id
        {
            get
            {
                return Manga.Id;
            }
            set
            {
                Manga.Id = value;
                RaisePropertyChanged("Id");
            }
        }

        private uint _chapter;
        public uint Chapter
        {
            get
            {
                return _chapter;
            }
            set
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
            set
            {
                _volume = value;
                RaisePropertyChanged("Volume");
            }
        }

        private uint _rereading;
        public uint Rereading
        {
            get
            {
                return _rereading;
            }
            set
            {
                _rereading = value;
                RaisePropertyChanged("Rereading");
            }
        }

        private uint _rereadingChapter;
        public uint RereadingChapter
        {
            get
            {
                return _rereadingChapter;
            }
            set
            {
                _rereadingChapter = value;
                RaisePropertyChanged("RereadingChapter");
            }
        }

        #endregion

        #region Methods

        public MangaEntry(Manga manga, uint chapter, uint volume, EntryStatus status, uint score)
        {
            Manga = new MangaBrief(manga);
            _chapter = chapter;
            _volume = volume;
            _status = status;
            _score = score;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to AnimeEntry return false.
            MangaEntry me = obj as MangaEntry;
            if ((System.Object)me == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(obj) && (_chapter == me._chapter) && (_volume == me._volume) && (_rereading == me._rereading) && (_rereadingChapter == me._rereadingChapter);
        }

        public bool Equals(MangaEntry me)
        {
            // If parameter is null return false:
            if ((object)me == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(me) && (_chapter == me._chapter) && (_volume == me._volume) && (_rereading == me._rereading) && (_rereadingChapter == me._rereadingChapter);
        }

        public override string ToString()
        {
            string mangaEntry = base.ToString();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Chapter: " + _chapter.ToString());
            sb.AppendLine("Volume: " + _volume.ToString());
            sb.AppendLine("Rereading: " + _rereading.ToString());
            sb.AppendLine("Rereading Chapter: " + _rereadingChapter.ToString());
            sb.AppendLine("Manga:");
            sb.AppendLine(Manga.ToString());
            return mangaEntry+sb.ToString();
        }

        public override string ExportXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<entry>");
            sb.AppendFormat("<chapter>{0}</chapter>", Chapter); sb.AppendLine();
            sb.AppendFormat("<volume>{0}</volume>", Volume); sb.AppendLine();
            sb.AppendFormat("<status>{0}</status>", Status); sb.AppendLine();
            sb.AppendFormat("<score>{0}</score>", Score); sb.AppendLine();
            sb.AppendLine("<downloaded_chapters></downloaded_chapters>");
            sb.AppendLine("<times_reread></times_reread>");
            sb.AppendLine("<reread_value></reread_value>");
            sb.AppendLine("<date_start></date_start>");
            sb.AppendLine("<date_finish></date_finish>");
            sb.AppendLine("<priority></priority>");
            sb.AppendLine("<enable_discussion></enable_discussion>");
            sb.AppendLine("<enable_rereading></enable_rereading>");
            sb.AppendLine("<comments></comments>");
            sb.AppendLine("<scan_group></scan_group>");
            sb.AppendLine("<tags></tags>");
            sb.AppendLine("<retail_volumes></retail_volumes>");
            sb.AppendLine("</entry>");

            return sb.ToString();
        }

        #endregion

    }
}
