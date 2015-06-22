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
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaMAL;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace jaMAL_APITestBench
{
    class Program
    {
        static void AsynchronousHelloWorld()
        {
            Account user = new Account("jaMALTestAccount", "jaMALTestAccount");
            Service.UserAccount = user;
            if( !user.VerifyAccount() )
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
            while( !finishGettingAnime )
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
        }

        static void SynchronousHelloWorld()
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
        }

        static bool TestAnimeList()
        {
            Account user = null;
            try
            {
                user = new Account("jaMALTestAccount", "jaMALTestAccount");
                Service.UserAccount = user;

                Console.WriteLine("Verifying account...");
                if (user.VerifyAccount())
                    Console.WriteLine("Verification accomplish!!! :D");
                else
                    Console.WriteLine("Verification failed :(");

                user.UserAnimeList.RefreshList();
                Debug.Assert(user.UserAnimeList.SeemsSynchronized && user.UserAnimeList.IsSynchronized, "AnimeList: List not sync after first RefreshList");

                Console.WriteLine(user.ToString());

                Console.ReadKey();

                Console.WriteLine("Original Anime List:");
                Console.WriteLine("-------------------");
                Console.WriteLine(user.UserAnimeList.ToString());
                Console.WriteLine("----------------------");

                Anime FullmetalAlchemist = MediaDataBase.GetAnime("Fullmetal Alchemist");
                AnimeEntry fmaEntry = new AnimeEntry(FullmetalAlchemist, 22, MediaEntry.EntryStatus.Currently, 9);

                user.UserAnimeList.AnimeEntries.Add(fmaEntry.Id, fmaEntry);

                Anime Monster = MediaDataBase.GetAnime("Monster");
                AnimeEntry monsterEntry = new AnimeEntry(Monster, 18, MediaEntry.EntryStatus.Currently, 10);
                
                user.UserAnimeList.AnimeEntries.Add(monsterEntry.Id, monsterEntry);

                
                //user.UserAnimeList.AnimeEntries.Remove(onePiece.Id);
                //user.UserAnimeList.AnimeEntries.Remove(FullmetalAlchemist.Id);

                Console.WriteLine("Antes de la parte critica");

                user.UserAnimeList.SyncAnimeList();

                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                //Debug.Assert(user.UserAnimeList.SeemsSynchronized, "AnimeList: List not sync after SyncAnimeList");
                //Debug.Assert(user.UserAnimeList.IsSynchronized, "AnimeList: List not sync after SyncAnimeList");
                Debug.Assert(user.UserAnimeList.SeemsSynchronized && user.UserAnimeList.IsSynchronized, "AnimeList: List not sync after SyncAnimeList");
                //Thread.Sleep(5000);
                //user.UserAnimeList.RefreshList();

                Anime onePiece = MediaDataBase.GetAnime("One Piece");
                AnimeEntry onePieceEntry = new AnimeEntry(onePiece, 11, MediaEntry.EntryStatus.Currently, 10);

                user.UserAnimeList.AnimeEntries.Add(onePieceEntry.Id, onePieceEntry);
                user.UserAnimeList.SyncAnimeList();
                Debug.Assert(user.UserAnimeList.SeemsSynchronized && user.UserAnimeList.IsSynchronized, "AnimeList: List not sync after SyncAnimeList");

                Console.WriteLine("Despues de la parte critica :D");
                Console.ReadKey();

                Console.WriteLine("");
                Console.WriteLine("Final Anime List:");
                Console.WriteLine("-----------------");
                Console.WriteLine(user.UserAnimeList.ToString());
                Console.WriteLine("----------------------");

                Console.WriteLine("Start Downloading Images");
                foreach (AnimeEntry ae in user.UserAnimeList.AnimeEntries.Values)
                {
                    ImageDataBase.GetImage(MediaDataBase.GetAnime(ae.Anime.Name));
                }
                Console.WriteLine("Finish Downloading");

                Console.WriteLine("Start Saving Images Cache");
                ImageDataBase.SaveCache();
                Console.WriteLine("Finish Saving");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            
            return true;
        }

        static void Main(string[] args)
        {
            //SynchronousHelloWorld();
            AsynchronousHelloWorld();
            Console.ReadKey();

            bool success = false;
            success = TestAnimeList();
            Console.ReadKey();

            ObservableConcurrentDictionary<uint,string> od = new ObservableConcurrentDictionary<uint,string>();
            od.CollectionChanged += (sender, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        Console.WriteLine("Add operation");
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        Console.WriteLine("Move operation");
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        Console.WriteLine("Remove operation");
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        Console.WriteLine("Replace operation");
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        Console.WriteLine("Reset operation");
                        break;
                }
                if (e.NewItems != null)
                    foreach (var x in e.NewItems)
                    {
                        Console.WriteLine("New value: {0}", x);
                    }

                if (e.OldItems != null)
                    foreach (var y in e.OldItems)
                    {
                        Console.WriteLine("Old value: {0}", y);
                    }
            };
            // test add
            od.Add(1, "Hola");
            od.Add(2, "_");
            od.Add(3, "Mundo");
            od.Add(4, "!");
            foreach(string s in od.Values)
            {
                Console.Write(s);
            }
            Console.WriteLine("");

            // test observable
            
            
            od.Add(6, "1");
            od.Add(1, "Halo");
            od.Remove(2);
            Console.ReadKey();

            ObservableCollection<uint> od2 = new ObservableCollection<uint>();
            od2.CollectionChanged += (sender, e) =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        Console.WriteLine("Add operation");
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        Console.WriteLine("Move operation");
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        Console.WriteLine("Remove operation");
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        Console.WriteLine("Replace operation");
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        Console.WriteLine("Reset operation");
                        break;
                }
                if (e.NewItems != null)
                    foreach (var x in e.NewItems)
                    {
                        Console.WriteLine("New value: {0}", x);
                    }

                if (e.OldItems != null)
                    foreach (var y in e.OldItems)
                    {
                        Console.WriteLine("Old value: {0}", y);
                    }
            };
            od2.Add(1);
            od2.Add(2);
            od2.Add(3);
            Console.ReadKey();

            //Service.UserAccount = new Account("bobxiv", "em230588");
            Anime FullmetalAlchemist = MediaDataBase.GetAnime("Fullmetal Alchemist");
            AnimeEntry FMAentry = new AnimeEntry(FullmetalAlchemist, 13, MediaEntry.EntryStatus.Currently, 10);
            List<Anime> search = MediaDataBase.GetAnimes("Fullmetal Alchemist");

            /*
            // test Service.BeginAdd
            {
                Console.WriteLine("Service.BeginAdd | Anime");
                bool finishAddCallback = false;
                IAsyncResult res = Service.BeginAdd(FMAentry, MediaType.Anime,
                    ar =>
                    {
                        finishAddCallback = true;
                    });

                while (!(res.IsCompleted && finishAddCallback))
                { }

                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Console.WriteLine("---------------------------");
            }
            
            // test Service.Search Anime & download image
            {
                Console.WriteLine("Service.Search | Anime");
                List<Media> media = Service.Search("One Piece", MediaType.Anime);

                foreach (Media m in media)
                {
                    Console.WriteLine((m as Anime).ToString());

                    bool finishDownloadingImage = false;
                    m.BeginDownloadImage(
                        ar =>
                        {
                            finishDownloadingImage = true;
                        });

                    while (!finishDownloadingImage)
                    { }

                    using (System.IO.FileStream file = new System.IO.FileStream("C:\\Users\\Bob\\Documents\\Proyectos\\jaMAL\\jaMAL-APITestBench\\fma.jpg", System.IO.FileMode.Create))
                    {
                        file.Write(m.LogoImage, 0, m.LogoImage.Length);
                    }
                }

                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Console.WriteLine("---------------------------");
            }
            */
            // test Service.BeginGetUserList
            {
                Console.WriteLine("Service.BeginGetUserList | AnimeList");
                bool finishGetUserAnimeListCallback = false;
                IAsyncResult res = Service.BeginGetUserList(MediaType.Anime,
                    ar =>
                    {
                        Console.WriteLine((ar as Service.SearchUserAnimeListAsyncResult).AnimeList.ToString());
                        finishGetUserAnimeListCallback = true;
                    });

                while (!(res.IsCompleted && finishGetUserAnimeListCallback))
                { }

                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Console.WriteLine("---------------------------");
            }

            // test Service.BeginDelete
            {
                Console.WriteLine("Service.BeginDelete | Anime");
                bool finishDeleteCallback = false;
                IAsyncResult res = Service.BeginDelete(FMAentry.Anime.Id, MediaType.Anime,
                    ar =>
                    {
                        finishDeleteCallback = true;
                    });

                while (!(res.IsCompleted && finishDeleteCallback))
                { }

                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Console.WriteLine("---------------------------");
            }


            // test Service.Search Anime
            {
                Console.WriteLine("Service.Search | Anime");
                List<Media> media = Service.Search("Full Metal Alchemist", MediaType.Anime);

                foreach (Media m in media)
                {
                    Console.WriteLine((m as Anime).ToString());

                    bool finishDownloadingImage = false;
                    byte[] image = ImageDataBase.GetImage(m);

                    using (System.IO.FileStream file = new System.IO.FileStream("C:\\Users\\Bob\\Documents\\Proyectos\\jaMAL\\jaMAL-APITestBench\\fma.jpg", System.IO.FileMode.Create))
                    {
                        file.Write(image, 0, image.Length);
                    }
                }

                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Console.WriteLine("---------------------------");
            }



            // test Service.Search Manga
            {
                Console.WriteLine("Service.Search | Manga");
                List<Media> media = Service.Search("Full Metal Alchemist", MediaType.Manga);

                foreach (Media m in media)
                    Console.WriteLine(m.Name);

                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Console.WriteLine("---------------------------");
            }

            // test Service.BeginSearch
            {
                Console.WriteLine("Service.BeginSearch | Anime");
                Service.SearchAnimeAsyncResult u = null;
                bool finishSearchCallback = false;
                IAsyncResult res = Service.BeginSearch("Full Metal Alchemist", MediaType.Anime,
                    ar =>
                    {
                        u = (Service.SearchAnimeAsyncResult)ar;
                        Service.SearchAnimeAsyncResult animQuery = (Service.SearchAnimeAsyncResult)ar;
                        foreach (Anime a in animQuery.Animes)
                            Console.WriteLine(a.Name);
                        finishSearchCallback = true;
                    });

                while (!(res.IsCompleted && finishSearchCallback))
                { }

                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Console.WriteLine("---------------------------");
            }
        }
    }
}
