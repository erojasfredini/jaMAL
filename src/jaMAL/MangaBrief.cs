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
using System.Threading.Tasks;

namespace jaMAL
{
    /// <summary>
    /// Manga brief information. Manga class contains all this brief information and more, but this is necesary because when an anime or manga list is downloaded not all the anime/manga is given for the entries
    /// </summary>
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
