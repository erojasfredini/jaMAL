/****************************************************************************
 * jaMAL
 * Copyright (c) Emmanuel Rojas Fredini, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
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
    /// Anime brief information. Anime class contains all this brief information and more, but this is necesary because when an anime or manga list is downloaded not all the anime/manga is given for the entries
    /// </summary>
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
