using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaMAL;

namespace HelloWorldAsynchronous
{
    class Program
    {
        static void Main(string[] args)
        {
            Account user = new Account("jaMALTestAccount", "jaMALTestAccount");
            Service.UserAccount = user;
            if (!user.VerifyAccount())
                throw new Exception("Verification failed :'(");

            IAsyncResult res = null;
            // gets the anime and manga list from myanimelist
            res = user.UserAnimeList.BeginRefreshList(r =>
            {
                // after we refresh we show the anime and manga lists of the user
                Console.WriteLine(user.ToString());
            });

            // now we wait to finish refreshing
            res.AsyncWaitHandle.WaitOne();

            // get Fullmetal Alchemist anime information
            bool finishGettingAnime = false;
            Anime FullmetalAlchemist = null;
            res = MediaDataBase.BeginGetAnime("Fullmetal Alchemist", r =>
            {
                // after we show the result of the query
                Service.SearchAnimeAsyncResult ra = r as Service.SearchAnimeAsyncResult;
                FullmetalAlchemist = ra.Animes[0];
                Console.WriteLine(FullmetalAlchemist.ToString());
                finishGettingAnime = true;
            });

            // now we wait to finish getting the anime and executing the callback
            res.AsyncWaitHandle.WaitOne();
            while (!finishGettingAnime)
            { }

            // add Fullmetal Alchemist to the user anime list
            AnimeEntry fmaEntry = new AnimeEntry(FullmetalAlchemist, 22/*episode*/, MediaEntry.EntryStatus.Currently, 9/*score*/);
            user.UserAnimeList.AnimeEntries.Add(fmaEntry.Id, fmaEntry);

            // add, remove or update the entries that you want from the lists :)

            // now sync so the changes are send to myanimelist
            user.UserAnimeList.BeginSyncAnimeList(r =>
            {
                // do some stuff after we sync
            });

            Console.WriteLine("Press a key to finish");
            Console.ReadKey();
        }
    }
}
