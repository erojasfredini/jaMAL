using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaMAL;

namespace HelloWorldSynchronous
{
    class Program
    {
        static void Main(string[] args)
        {
            Account user = new Account("jaMALTestAccount", "jaMALTestAccount");
            Service.UserAccount = user;
            if (!user.VerifyAccount())
                throw new Exception("Verification failed :'(");

            // gets the anime and manga list from myanimelist
            user.UserAnimeList.RefreshList();

            // show the anime and manga lists of the user
            Console.WriteLine(user.ToString());

            // get Fullmetal Alchemist anime information
            Anime FullmetalAlchemist = MediaDataBase.GetAnime("Fullmetal Alchemist");
            Console.WriteLine(FullmetalAlchemist.ToString());

            // add Fullmetal Alchemist to the user anime list
            AnimeEntry fmaEntry = new AnimeEntry(FullmetalAlchemist, 22/*episode*/, MediaEntry.EntryStatus.Currently, 9/*score*/);
            user.UserAnimeList.AnimeEntries.Add(fmaEntry.Id, fmaEntry);

            // add, remove or update the entries that you want from the lists :)

            // now sync so the changes are send to myanimelist
            user.UserAnimeList.SyncAnimeList();

            Console.WriteLine("Press a key to finish");
            Console.ReadKey();
        }
    }
}
