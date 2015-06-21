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
