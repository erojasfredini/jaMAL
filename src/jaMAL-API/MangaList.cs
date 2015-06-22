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

namespace jaMAL
{
    /// <summary>
    /// MangaList is responsible to represent and manipulate the manga list of an user
    /// </summary>
    public class MangaList : MediaList
    {
        #region Properties

        public List<MangaEntry> MangaEntries;

        #endregion

        #region Methods

        internal MangaList(Account owner): base(owner)
        {
            MangaEntries = new List<MangaEntry>();
        }

        override public string ToString()
        {
            string mangaList = base.ToString();

            foreach (MangaEntry e in MangaEntries)
            {
                mangaList += "\n";
                mangaList += e.ToString();
            }

            return mangaList;
        }

        #endregion
    }
}
