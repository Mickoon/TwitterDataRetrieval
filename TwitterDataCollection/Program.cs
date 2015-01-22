using System;
using System.Linq;
using System.Data.Entity;
using Twitterizer;
using System.IO;
using System.Diagnostics;
using TwitterDataCollection.Properties;
using System.Data;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Runtime.CompilerServices;
using System.Data.Objects;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace TwitterDataCollection
{
    class Program
    {
        // Since Tweet does not really matter because the user only can retrieve data from approximately an year ago.
        // (About 3,200 posts in maximum)
        private static decimal _sinceTweetId = 222222222222222222; 
        private static string _sinceTweetIdAsString = _sinceTweetId.ToString();

        private static decimal _maxTweetId = 999999999999999999;        // This is just an estimate

        private const int _MaxNumberOfTweets = 200;
        private const int _NumberOfUsersPerLookup = 100;

        private static int ThisInstanceNumber;
        private static int NumInstances;

        private static OAuthTokens _tokens;

        private static TimeSpan _oneMin = new TimeSpan(0, 1, 0);
        private static TimeSpan _fiveSec = new TimeSpan(0, 0, 5);

        static Program()
        {
            _tokens = new OAuthTokens
            {
                ConsumerKey = Settings.Default.ConsumerKey,
                ConsumerSecret = Settings.Default.ConsumerSecret
            };
        }
        private static OAuthTokenResponse GetUserToken()
        {
            var token = OAuthUtility.GetRequestToken(Settings.Default.ConsumerKey, Settings.Default.ConsumerSecret, "oob");
            Process.Start("https://twitter.com/oauth/authorize?oauth_token=" + token.Token);


            Console.Error.Write("Please authorize our application and enter the pin code: ");
            var pin = ReadPassword();
            Console.WriteLine();

            return OAuthUtility.GetAccessToken(Settings.Default.ConsumerKey, Settings.Default.ConsumerSecret, token.Token, pin);
        }
        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        password = password.Substring(0, password.Length - 1);
                        int pos = Console.CursorLeft;
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }

            return password;
        }

        static void Main(string[] args)
        {
            #region get_tokens
            if (Settings.Default.UseAccessPair)
            {
                _tokens.AccessToken = Settings.Default.AccessToken;
                _tokens.AccessTokenSecret = Settings.Default.AccessSecret;
            }
            else
            {
                if (string.IsNullOrEmpty(Settings.Default.UserAccessToken) || string.IsNullOrEmpty(Settings.Default.UserAccessSecret))
                {
                    var tokenResponse = GetUserToken();
                    Settings.Default.UserAccessToken = tokenResponse.Token;
                    Settings.Default.UserAccessSecret = tokenResponse.TokenSecret;
                    Settings.Default.Save();
                }
                _tokens.AccessToken = Settings.Default.UserAccessToken;
                _tokens.AccessTokenSecret = Settings.Default.UserAccessSecret;
            }
            #endregion

            while (true)
            {
                #region print options
                Console.WriteLine(
                "\n" +
                "************************************************************************\n" +
                "* " + "0a. Update existing user / Save new user in database\n" +
                "* " + "1a. create followers (user) id list for screenname\n" +
                "* " + "2a. create friends (user) id list for list for screenname\n" +
                "* " + "3x. record users screenname's followers id list\n" +
                "* " + "3y. record users screenname's following id list\n" +
                "* " + "4a. record timeline of screenname\n" +
                "* " + "5a. record list of favorites of screenname\n" +
                "* " + "5b. record 'member of' list of screenname\n" +
                "* " + "5c. record 'subscribed to' list of screenname\n" +
                "* " + "6a. record search results of query\n" +
                "* " + ". \n" +
                "* " + "9. show ratelimit details\n" +
                "* " + "t. db connection test\n" +
                "* " + "0. exit\n" +
                "************************************************************************\n" +
                "Select a function: "
                );
                #endregion

                string selection = Console.ReadLine();
                Console.WriteLine();

                int[] instanceDetails = null;
                //Should use int[] GetInstanceDetails() in all methods
                // - this would require all methods to be refactored though :(
                switch (selection.Trim().ToLower())
                {
                    #region case "0a": Update existing user / Save new user in database
                    case "0a":
                        Console.WriteLine("\nEnter a screenname: ");
                        SaveUpdateUserInfo(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "t": db connection test
                    case "t":
                        Console.WriteLine("attempting to connect using friendship context...");
                        var f = new TwitterEntitiesForFriendship();
                        Console.WriteLine("number of users using friendship context = " + f.FriendshipUsers.Count());
                        Console.WriteLine();
                        Console.WriteLine("attempting to connect using twittercontext...");
                        var db = new TwitterContext();
                        Console.WriteLine("number of users using twittercontext = " + db.Users.Count());

                        f.Connection.Close();
                        f.Dispose();
                        f = null;
                        db.Database.Connection.Close();
                        db.Dispose();
                        db = null;

                        break;
                    #endregion

                    #region case "1a": create followers (user) id list for screenname
                    case "1a":
                        Console.WriteLine("\n\nEnter a screenname: ");
                        CreateFollowerListForUser(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "2a": create friends (user) id list for list for screenname
                    case "2a":
                        Console.WriteLine("\n\nEnter a screenname: ");
                        CreateFriendListForUser(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "3x": record users screenname's followers id list
                    case "3x":
                        Console.WriteLine("\n\nEnter a screenname: ");
                        SaveUsersFromFollowerList(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "3y": record users screenname's followeings id list
                    case "3y":
                        Console.WriteLine("\n\nEnter a screenname: ");
                        SaveUsersFromFollowingList(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "4a": record timeline of screenname
                    case "4a":
                        Console.WriteLine("\n\nEnter a screenname: ");
                        SaveUserTimeline(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "4aa": record timeline of screenname for a second time
                    case "4aa":
                        Console.WriteLine("\n\nEnter a screenname: ");
                        SaveUserTimelineSecond(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "5a": record list of favorites of screenname
                    case "5a":
                        Console.WriteLine("\nEnter a screenname: ");
                        SaveFavorites(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "5b": record "member of" list of screenname
                    case "5b":
                        Console.WriteLine("\nEnter a screenname: ");
                        SaveMemberOf(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "5c": record "subscribed to" list of screenname
                    case "5c":
                        Console.WriteLine("\nEnter a screenname: ");
                        SaveSubscribedTo(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "6a": record search results of query
                    case "6a":
                        Console.WriteLine("\nEnter a screenname: ");
                        SaveSearchResults(Console.ReadLine());
                        PrintCompleted();
                        break;
                    #endregion

                    #region case "9": show ratelimit details
                    case "9":
                        ShowRateLimitDetails();
                        break;
                    #endregion

                    #region case "0": exit
                    case "0":
                        Environment.Exit(0);
                        break;
                    #endregion

                    default:
                        Console.WriteLine("Invalid Option.");
                        break;
                }
            }

        }

        private static void SaveUpdateUserInfo(string screenName)
        {
            //Create a new Token for authetication
            OAuthTokens tokens = new OAuthTokens();
            tokens.ConsumerKey = Settings.Default.ConsumerKey;
            tokens.ConsumerSecret = Settings.Default.ConsumerSecret;
            tokens.AccessToken = Settings.Default.AccessToken;
            tokens.AccessTokenSecret = Settings.Default.AccessSecret;

            //string screenName = Console.ReadLine();
            TwitterResponse<TwitterUser> showUserResponse = TwitterUser.Show(tokens, screenName);
            //Console.WriteLine("Response Result " + showUserResponse.Result);
            //Console.WriteLine("Success? " + RequestResult.Success);
            Console.WriteLine("");

            //Print out basic information of the inserted screenname
            if (showUserResponse.Result == RequestResult.Success)
            {
                screenName = showUserResponse.ResponseObject.ScreenName;
                decimal id = showUserResponse.ResponseObject.Id;
                //Console.WriteLine("ID := " + id);
                //Console.WriteLine("Screen Name := " + screenName);
                //Console.WriteLine("Number of Followers := " + showUserResponse.ResponseObject.NumberOfFollowers);
                //Console.WriteLine("Number of Status = " + showUserResponse.ResponseObject.NumberOfStatuses);
                //Console.WriteLine("RateLimiter := " + showUserResponse.RateLimiting.Total);
                //Console.WriteLine("Access Level := " + showUserResponse.AccessLevel);

                var data_base = new TwitterContext();
                var _friendshipContext = new TwitterEntitiesForFriendship();
                var user = (from u in data_base.Users where u.ScreenName == screenName select u).FirstOrDefault();
                var followerFriendship = (from u in _friendshipContext.FollowerFriendships where u.UserId == id select u).FirstOrDefault();

                if (user == null)
                {
                    //Insert User into dbo.TwitterUser
                    showUserResponse.ResponseObject.Status = null;
                    data_base.Users.Add(showUserResponse.ResponseObject);
                    data_base.SaveChanges();
                    Console.WriteLine("New User '" + showUserResponse.ResponseObject.Name + "' is added in the database 'dbo.TwitterUser'");
                }
                else
                {
                    try
                    {
                        //********** Below commended codes are not working ******************//
                        //if (user != showUserResponse.ResponseObject)
                        //{
                        //    user = showUserResponse.ResponseObject;
                        //}
                        //******************************************************************//
                        if (user.Description != showUserResponse.ResponseObject.Description)
                            user.Description = showUserResponse.ResponseObject.Description;
                        if (user.NumberOfFollowers != showUserResponse.ResponseObject.NumberOfFollowers)
                            user.NumberOfFollowers = showUserResponse.ResponseObject.NumberOfFollowers;
                        if (user.NumberOfFavorites != showUserResponse.ResponseObject.NumberOfFavorites)
                            user.NumberOfFavorites = showUserResponse.ResponseObject.NumberOfFavorites;
                        if (user.NumberOfFriends != showUserResponse.ResponseObject.NumberOfFriends)
                            user.NumberOfFriends = showUserResponse.ResponseObject.NumberOfFriends;
                        if (user.NumberOfStatuses != showUserResponse.ResponseObject.NumberOfStatuses)
                            user.NumberOfStatuses = showUserResponse.ResponseObject.NumberOfStatuses;
                        if (user.ListedCount != showUserResponse.ResponseObject.ListedCount)
                            user.ListedCount = showUserResponse.ResponseObject.ListedCount;

                        data_base.SaveChanges();
                        Console.WriteLine("User's Information has been updated!");
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException)
                    {
                        Console.WriteLine("DB Update Exception: Your database is not updated");
                    }
                }

                //If no user is saved in dbo.FollowerFriendship
                if (followerFriendship == null)
                {
                    FollowerFriendship response = FollowerFriendship.CreateFollowerFriendship(id, false, false, false, false);
                    _friendshipContext.FollowerFriendships.AddObject(response);
                    _friendshipContext.SaveChanges();
                    Console.WriteLine("New User '" + showUserResponse.ResponseObject.Name + "' is added in the database 'dbo.FollowerFriendship'");
                }
            }
            else
            {
                Console.WriteLine("User ScreenName " + screenName + " does not exist.");
            }
        }

        private static FriendshipUser GetFriendshipUserFromScreenname(string screenname)
        {
            using (var db = new TwitterEntitiesForFriendship())
            {
                var user = db.FriendshipUsers.FirstOrDefault(u => u.ScreenName.Equals(screenname));

                if (user == null)
                {
                    Console.Write("User with screenname " + screenname + " not found in database.");
                }
                return user;
            }
        }

        private static void CreateFollowerListForUser(string screename)
        {
            var user = GetFriendshipUserFromScreenname(screename);
            if (user != null)
            {
                CreateFollowerListForUser(user);
            }
        }

        private static void CreateFollowerListForUser(FriendshipUser user)
        {

            #region Set up logging to file (for debugging)
            ////////////////////////////////////
            /*
            string originalFileName = screenname + "_followers";
            string fileName = originalFileName;

            int suffix = 1;
            while (File.Exists(fileName))
            {
                Console.Error.WriteLine(fileName + " already exists. Creating new file named " + (fileName = originalFileName + ++suffix));
            }

            TextWriter file = File.CreateText(fileName);
            Console.WriteLine("Logging ids to " + fileName + " for debugging.");

            ///////////////////////////////////
             */
            #endregion

            TimeSpan _fifteenMin = new TimeSpan(0, 15, 0);
            string screenname = user.ScreenName;
            Console.WriteLine("\n[" + DateTime.Now + "] Attempting to save followers ids list for " + screenname);

            try
            {
                DateTime recordedDate = DateTime.Now;

                #region Get followers' ids and record friendship in database
                int followersSaved = 0;
                int duplicatesFound = 0;
                long nextCursor = -1;

                //Each call returns up to 5000 ids, so need to keep looking while next_cursor > 0
                do
                {
                    var _friendshipContext = new TwitterEntitiesForFriendship();
                    //Friendship_1.

                    TwitterResponse<UserIdCollection> response = TwitterFriendship.FollowersIds(_tokens, new UsersIdsOptions { ScreenName = screenname, Cursor = nextCursor });

                    if (response.Result == RequestResult.Success && response.ResponseObject != null)
                    {
                        try
                        {
                            var followersID = response.ResponseObject;
                            nextCursor = followersID.NextCursor;

                            foreach (var id in followersID.ToList())
                            {
                                if (!_friendshipContext.Friendships.Any(f => f.UserId == user.Id && f.FollowerUserId == id))
                                {
                                    _friendshipContext.Friendships.AddObject(new Friendship { UserId = user.Id, FollowerUserId = id, RecordedAt = recordedDate, IsProcessed = false });
                                    followersSaved++;
                                }
                                else
                                {
                                    duplicatesFound++;
                                }
                            }
                            _friendshipContext.SaveChanges();

                            Console.WriteLine("Saved {0} followers so far; {1} duplicates not saved", followersSaved, duplicatesFound);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                            Console.Error.WriteLine("Continuing (but not retrying) for user " + screenname);
                        }
                    }
                    /*** this isn't as good an idea as originally thought. The extra 150 calls /hr must be shared across all instances :( ***/
                    //else if (response.Result == RequestResult.RateLimited)
                    //{
                    //    //switch modes, wait 2 mins, then resume.
                    //    Settings.Default.UseAuthentication = !Settings.Default.UseAuthentication;

                    //    Console.WriteLine("Rate limit reached. Switching authentication modes (to " + (Settings.Default.UseAuthentication ? "authenticated" : "non-authenicated") + ") and retrying in 1 min...");
                    //    System.Threading.Thread.Sleep(_oneMin);
                    //    Console.WriteLine("Resuming...");
                    //    continue;
                    //}
                    else if (response.Result == RequestResult.Unauthorized || response.Result == RequestResult.FileNotFound)
                    {
                        Console.WriteLine("User " + user.ScreenName + " is now protected or no longer exists.");

                        var u = _friendshipContext.FriendshipUsers.FirstOrDefault(s => s.Id == user.Id);
                        u.IsProtected = true;
                        _friendshipContext.SaveChanges();
                    }
                    else if (response.Result == RequestResult.RateLimited || response.Result == RequestResult.Unknown)
                    {
                        Console.WriteLine("RATE LIMIT REACHED (when 75000 followers found)");

                        Console.WriteLine("Rate limit reached. Switching authentication modes and retrying in 15 mins...");
                        Console.WriteLine("Next page to be processed is :     " + nextCursor);
                        StreamWriter w1 = new StreamWriter("SaveFollowingsList_PageSoFar.txt");
                        w1.WriteLine("Record datetime: " + DateTime.Now);
                        w1.WriteLine("Page number next to be: " + nextCursor);
                        w1.Close();

                        System.Threading.Thread.Sleep(_fifteenMin);

                        Console.WriteLine("Resuming...");
                        continue;
                    }
                    else //Just in case if the program is unexpectedly failed and closed, record last processed page number
                    {
                        Console.WriteLine("Error: " + response.Result);
                        //StreamWriter w1 = new StreamWriter("SaveFollowingsList_PageSoFar.txt");
                        //w1.WriteLine("Record datetime: " + DateTime.Now);
                        //w1.WriteLine("Page number next to be: " + nextCursor);
                        //w1.Close();
                        Console.WriteLine("End of Process"); 
                        break; 
                    }
                } while (nextCursor > 0);

                //Set IsFollowersAdded = 1
                try
                {
                    var _friendshipContext = new TwitterEntitiesForFriendship();
                    var followerFriendship = _friendshipContext.FollowerFriendships.FirstOrDefault(f => f.UserId == user.Id);

                    followerFriendship.IsFollowersAdded = true;
                    _friendshipContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Couldn't update IsFollowersAdded = 1 for user " + screenname);
                    Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                }
            }
            catch (Exception)
            {
                throw;
                //TODO: handle this
                //from TwitterCommand.cs
                // Occasionally, Twitter responds with XML error data even though we asked for json.
                // This is that scenario. We will deal with it by doing nothing. It's up to the developer to deal with it.
            }

            //file.Close();
                #endregion
        }

        private static void CreateFollowerListFromIdsList(int numInstances, int instanceNumber)
        {
            ThisInstanceNumber = instanceNumber;

            var db = new TwitterEntitiesForFriendship();

            var users = (from f in db.FollowerFriendships
                         join u in db.FriendshipUsers on f.UserId equals u.Id
                         where f.UserId % numInstances == (ThisInstanceNumber - 1)
                             && !f.IsFollowersAdded
                             && !u.IsProtected
                             && u.NumberOfFollowers > 0
                         select u).ToList();
            db = null;

            foreach (var u in users)
            {
                try
                {
                    CreateFollowerListForUser(u);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                    Console.Error.WriteLine("Aborting for user " + u.ScreenName + "(" + u.Id + ")");
                }
            }

        }

        private static void CreateFriendListForUser(string screenname)
        {
            var user = GetFriendshipUserFromScreenname(screenname);
            if (user != null)
            {
                CreateFriendListForUser(user);
            }
        }

        private static void CreateFriendListForUser(FriendshipUser user)
        {
            string screenname = user.ScreenName;
            Console.WriteLine("\n[" + DateTime.Now + "] Attempting to save friends ids list for " + screenname);

            try
            {
                DateTime recordedDate = DateTime.Now;

                #region Get friends' ids and record friendship in database

                int friendsSaved = 0;
                int duplicatesFound = 0;
                long nextCursor = -1;

                //Each call returns up to 5000 ids, so need to keep looking while next_cursor > 0
                do
                {
                    using (var _friendshipContext = new TwitterEntitiesForFriendship())
                    {
                        //TimeSpan s1 = new TimeSpan(DateTime.Now.Ticks);
                        TwitterResponse<UserIdCollection> response = TwitterFriendship.FriendsIds(_tokens, new UsersIdsOptions { ScreenName = screenname, Cursor = nextCursor });
                        //TimeSpan f1 = new TimeSpan(DateTime.Now.Ticks);
                        //TimeSpan timeTaken1 = f1 - s1;
                        //Console.WriteLine("API call took " + timeTaken1.TotalSeconds + " secs)");

                        if (response.Result == RequestResult.Success && response.ResponseObject != null)
                        {
                            try
                            {
                                var friendsIDs = response.ResponseObject;
                                nextCursor = friendsIDs.NextCursor;

                                TimeSpan s2 = new TimeSpan(DateTime.Now.Ticks);

                                //if (ThisInstanceNumber == 1)
                                //{
                                //    friendsSaved += insertFriendships_1(_friendshipContext.Friendship_1, friendsIDs, user.Id, recordedDate);
                                //}
                                //else if (ThisInstanceNumber == 2)
                                //{
                                //    friendsSaved += insertFriendships_2(_friendshipContext.Friendship_2, friendsIDs, user.Id, recordedDate);
                                //}
                                //else if (ThisInstanceNumber == 3)
                                //{
                                //    friendsSaved += insertFriendships_3(_friendshipContext.Friendship_3, friendsIDs, user.Id, recordedDate);
                                //}
                                //else if (ThisInstanceNumber == 4)
                                //{
                                //    friendsSaved += insertFriendships_4(_friendshipContext.Friendship_4, friendsIDs, user.Id, recordedDate);
                                //}
                                //else if (ThisInstanceNumber == 5)
                                //{
                                //    friendsSaved += insertFriendships_5(_friendshipContext.Friendship_5, friendsIDs, user.Id, recordedDate);
                                //}
                                //else if (ThisInstanceNumber == 6)
                                //{
                                //    friendsSaved += insertFriendships_6(_friendshipContext.Friendship_6, friendsIDs, user.Id, recordedDate);
                                //}
                                //else //insert into dbo.Friendship (original code just did this for all instances)
                                //{
                                    foreach (var id in friendsIDs.ToList())
                                    {
                                        _friendshipContext.Friendship_Following.MergeOption = MergeOption.NoTracking;

                                        if (!_friendshipContext.Friendship_Following.Any(f => f.UserId == user.Id && f.FollowingUserId == id))
                                        {
                                            _friendshipContext.Friendship_Following.AddObject(new Friendship_Following { UserId = user.Id, FollowingUserId = id, RecordedAt = recordedDate, IsProcessed = false });
                                            friendsSaved++;
                                        }
                                        else
                                        {
                                            duplicatesFound++;
                                        }
                                    }
                                //}

                                _friendshipContext.SaveChanges();
                                TimeSpan f2 = new TimeSpan(DateTime.Now.Ticks);
                                TimeSpan timeTaken2 = f2 - s2;

                                Console.WriteLine("Saved {0} friends so far; {1} duplicates not saved (inserts took " + timeTaken2.TotalSeconds + " secs)", friendsSaved, duplicatesFound);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                                Console.Error.WriteLine("Continuing (but not retrying) for user " + screenname);
                            }
                        }
                        else if (response.Result == RequestResult.Unauthorized || response.Result == RequestResult.FileNotFound)
                        {
                            Console.WriteLine("User " + user.ScreenName + " is now protected or no longer exists.");

                            var u = _friendshipContext.FriendshipUsers.FirstOrDefault(s => s.Id == user.Id);
                            u.IsProtected = true;
                            _friendshipContext.SaveChanges();
                        }
                        //todo : what to do here?
                        //else if ()
                        //{
                        //}
                        else
                        {
                            HandleTwitterizerError<UserIdCollection>(response);
                        }
                    }
                } while (nextCursor > 0);

                //Set IsFriendsAdded = 1
                try
                {
                    var _friendshipContext = new TwitterEntitiesForFriendship();
                    var followerFriendship = _friendshipContext.FollowerFriendships.FirstOrDefault(f => f.UserId == user.Id);

                    followerFriendship.IsFriendsAdded = true;
                    _friendshipContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Couldn't update IsFriendsAdded = 1 for user " + screenname);
                    Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                }
            }
            catch (Exception)
            {
                throw;
                //TODO: handle this
                //from TwitterCommand.cs
                // Occasionally, Twitter responds with XML error data even though we asked for json.
                // This is that scenario. We will deal with it by doing nothing. It's up to the developer to deal with it.
            }
                #endregion
        }

        private static void CreateFriendListFromIdsList(int numInstances, int instanceNumber)
        {
            ThisInstanceNumber = instanceNumber;

            var db = new TwitterEntitiesForFriendship();
            var users = (from f in db.FollowerFriendships
                         join u in db.FriendshipUsers on f.UserId equals u.Id
                         where f.UserId % numInstances == (ThisInstanceNumber - 1)
                             && !f.IsFriendsAdded
                             && !u.IsProtected
                             && u.NumberOfFriends > 0
                         select u).ToList();
            db = null;

            foreach (var u in users)
            {
                try
                {
                    CreateFriendListForUser(u);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                    Console.Error.WriteLine("Aborting for user " + u.ScreenName + "(" + u.Id + ")");
                }
            }
        }

        private static void SaveUsersFromFollowerList(string screenname)
        {
            var db = new TwitterContext();

            var user = (from u in db.Users where u.ScreenName == screenname select u).FirstOrDefault();
            if (user != null)
            {
                var _friendshipContext = new TwitterEntitiesForFriendship();
                var followersIdsQuery = from fship in _friendshipContext.Friendships
                                        where fship.UserId == user.Id
                                            && fship.IsProcessed == false
                                        select fship.FollowerUserId;

                IList<decimal> followersIds = followersIdsQuery.ToList();

                Console.WriteLine("Starting user lookup for " + followersIds.Count + " followers");
                printWorking();

                LookupUsersOptions lookupOptions = new LookupUsersOptions();

                for (int i = 0; i < followersIds.Count; i++)
                {
                    // We can only look up _NumberOfUsersPerLookup at most each time
                    if (i > 0 && i % _NumberOfUsersPerLookup == 0)
                    {
                        PerformUserLookup(user, _tokens, lookupOptions);
                        lookupOptions.UserIds.Clear();
                    }

                    lookupOptions.UserIds.Add(followersIds[i]);
                }
                if (lookupOptions.UserIds.Count > 0)
                {
                    PerformUserLookup(user, _tokens, lookupOptions);
                }
            }
            else
            {
                Console.WriteLine("User with screenname " + screenname + " not in the database.");
            }
        }

        private static void SaveUsersFromFollowingList(string screenname)
        {
            var db = new TwitterContext();

            var user = (from u in db.Users where u.ScreenName == screenname select u).FirstOrDefault();
            if (user != null)
            {
                var _friendshipContext = new TwitterEntitiesForFriendship();
                var followingsIdsQuery = from fship in _friendshipContext.Friendship_Following
                                        where fship.UserId == user.Id
                                            && fship.IsProcessed == false
                                        select fship.FollowingUserId;

                IList<decimal> followingsIds = followingsIdsQuery.ToList();

                Console.WriteLine("Starting user lookup for " + followingsIds.Count + " followers");
                printWorking();

                LookupUsersOptions lookupOptions = new LookupUsersOptions();

                for (int i = 0; i < followingsIds.Count; i++)
                {
                    // We can only look up _NumberOfUsersPerLookup at most each time
                    if (i > 0 && i % _NumberOfUsersPerLookup == 0)
                    {
                        PerformUserLookup(user, _tokens, lookupOptions);
                        lookupOptions.UserIds.Clear();
                    }

                    lookupOptions.UserIds.Add(followingsIds[i]);
                }
                if (lookupOptions.UserIds.Count > 0)
                {
                    PerformUserLookup(user, _tokens, lookupOptions);
                }
            }
            else
            {
                Console.WriteLine("User with screenname " + screenname + " not in the database.");
            }
        }

        private static void PerformUserLookup(TwitterUser user, OAuthTokens tokens, LookupUsersOptions lookupOptions)
        {
            TwitterResponse<TwitterUserCollection> userLookupResponse;
            try
            {
                userLookupResponse = TwitterUser.Lookup(tokens, lookupOptions);
            }
            catch (TwitterizerException)
            {
                throw; //something is wrong with Twitterizer
            }
            catch (Exception)
            {
                //do nothing (and manually try again later)
                return;
            }

            if (userLookupResponse.Result == RequestResult.Success && userLookupResponse.ResponseObject != null)
            {
                SaveUsers(user, userLookupResponse.ResponseObject);
                Console.WriteLine(userLookupResponse.ResponseObject.Count + " users added.");
            }
            else
            {
                HandleTwitterizerError<TwitterUserCollection>(userLookupResponse);
            }
        }

        private static void SaveUsers(TwitterUser user, TwitterUserCollection followers)
        {
            var db = new TwitterContext();
            var _friendshipContext = new TwitterEntitiesForFriendship();

            foreach (var follower in followers.ToList())
            {
                try
                {
                    follower.Status = null;

                    db.Users.Add(follower);
                    db.SaveChanges();

                    var fship = _friendshipContext.Friendships.First(f => f.UserId == user.Id && f.FollowerUserId == follower.Id);
                    fship.IsProcessed = true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[" + DateTime.Now + "] Error occurred (" + ex.Message + ") saving follower " + follower.ScreenName + "(id:" + follower.Id + "). Continuing with other followers");
                    //do nothing, really
                    db = new TwitterContext(); //to clear the follower in the current context which caused the exception
                }
            }
            _friendshipContext.SaveChanges();
        }

        private static void RecordTimelines()
        {
            RecordTimelines(null, false);
        }

        private static void RecordTimelines(bool resumeStarted)
        {
            RecordTimelines(null, resumeStarted);
        }

        private static void RecordTimelines(string followsScreename, bool resumeStarted)
        {
            var db = new TwitterContext();
            bool resume = false;
            decimal userId = -1;

            if (!string.IsNullOrEmpty(followsScreename))
            {
                var userIdQuery = db.Users.Where(u => u.ScreenName.Equals(followsScreename)).Select(u => u.Id);

                if (userIdQuery.Any())
                {
                    userId = userIdQuery.First();
                }
                else
                {
                    Console.WriteLine("User with screenname " + followsScreename + " not in the database.");
                    return;
                }
            }


            do //while there are still users to be processed
            {
                try
                {
                    var _friendshipContext = new TwitterEntitiesForFriendship();

                    FriendshipUser follower = GetUserToProcess(userId, resumeStarted);

                    if (follower != null)
                    {
                        SaveUserTimeline(follower);
                        resume = true;
                    }
                    else
                    {
                        resume = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[" + DateTime.Now + "] Error in processing timelines for all users: " + ex.Message);
                    Console.Error.WriteLine("Continuing with loop...");
                }


            } while (resume);
        }

        private static FriendshipUser GetUserToProcess(decimal userId, bool resumeStarted)
        {
            var _friendshipContext = new TwitterEntitiesForFriendship();

            var fUser = (from f in _friendshipContext.FollowerFriendships //old code used dbo.Friendship and checked was follower of 4 airlines
                         join u in _friendshipContext.FriendshipUsers on f.UserId equals u.Id
                         where !f.IsTweetsAdded
                             && u.Id % NumInstances == (ThisInstanceNumber - 1)
                             && (resumeStarted ? f.IsTweetsInProgress : !f.IsTweetsInProgress)
                             && !u.IsProtected
                             && u.NumberOfStatuses > 0

                         select u).FirstOrDefault();

            return fUser;
        }

        private static void SaveFavorites(string screenname)
        {
            try
            {
                FriendshipUser fuser = (new TwitterEntitiesForFriendship()).FriendshipUsers.Where(u => u.ScreenName.Equals(screenname)).First();

                var db = new TwitterContext();

                int recordsAffected = 0;
                int numFavorites = 0;
                int duplicatesFound = 0;
                int pageNum = 1;

                TimeSpan _fifteenMin = new TimeSpan(0, 15, 0);

                while (true)
                {
                    TwitterResponse<TwitterStatusCollection> favoriteResponse = TwitterFavorite.List(_tokens, new ListFavoritesOptions
                    {
                        Page = pageNum,
                        UserNameOrId = fuser.Id.ToString()
                    });

                    if (favoriteResponse.Result == RequestResult.Success)
                    {
                        TwitterStatusCollection favorites = favoriteResponse.ResponseObject;

                        if (favorites != null && favorites.Count > 0)
                        {
                            foreach (TwitterStatus favorite in favorites)
                            {
                                try
                                {
                                    db = new TwitterContext();

                                    if (!db.Tweets.Any(t => t.Id == favorite.Id)) //check if favorite already exists
                                    {
                                        //don't re-add the favorite
                                        favorite.User.Status = null;
                                        db.Entry(favorite.User).State = EntityState.Unchanged;

                                        //don't re-add the place
                                        if (favorite.Place != null)
                                        {
                                            var existingPlace = getExistingPlaceIfExists(db, favorite.Place);
                                            if (existingPlace != null)
                                            {
                                                favorite.Place = existingPlace;
                                            }
                                        }

                                        //don't re-add the retweet's: status (tweet), user, or place
                                        if (favorite.RetweetedStatus != null)
                                        {
                                            //status - this should be abstracted to function like the checks for user and place (especially if reused)
                                            var existingTweet = db.Tweets.FirstOrDefault(t => t.Id == favorite.RetweetedStatus.Id);
                                            if (existingTweet != null)
                                            {
                                                //using dumb long-winded way because I can't figure out how to use .Include( users ) on db.Tweets above.
                                                //  and changing the state doesn't seem to work.
                                                var existingUser = getExistingUserIfExists(db, favorite.RetweetedStatus.User);
                                                if (existingUser != null)
                                                {
                                                    existingTweet.User = existingUser;
                                                }
                                                favorite.RetweetedStatus = existingTweet;
                                            }
                                            else
                                            {
                                                //place
                                                if (favorite.RetweetedStatus.Place != null)
                                                {
                                                    var existingPlace = getExistingPlaceIfExists(db, favorite.RetweetedStatus.Place);
                                                    if (existingPlace != null)
                                                    {
                                                        favorite.RetweetedStatus.Place = existingPlace;
                                                    }
                                                }

                                                //user
                                                if (favorite.RetweetedStatus.User != null) //I think this is always true if we have a retweet
                                                {
                                                    if (db.Users.Any(u => u.Id == favorite.RetweetedStatus.User.Id))
                                                    {
                                                        if (favorite.RetweetedStatus.User.Id.ToString() != favorite.Id)
                                                        {
                                                            db.Entry(favorite.RetweetedStatus.User).State = EntityState.Unchanged;
                                                        }
                                                        else //retweeted own tweet - don't save this retweet, it should be saved already or will be from a later timeline call
                                                        {
                                                            favorite.RetweetedStatus = null;
                                                        }
                                                    }
                                                    else //new user to add
                                                    {
                                                        favorite.RetweetedStatus.User.Status = null; //add the user but not their current status (this would add a new tweet with user set to null)
                                                    }
                                                }
                                                else
                                                {
                                                    //does this ever happen?
                                                }
                                            }
                                        }

                                        //For search purpose in database, IsFavorited = 1 can classify which data is for favorites
                                        favorite.IsFavorited = true;

                                        db.Tweets.Add(favorite);
                                        recordsAffected += db.SaveChanges();
                                        numFavorites++;
                                    }
                                    else
                                    {
                                        duplicatesFound++;
                                    }
                                }
                                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                                {
                                    Console.Error.WriteLine("[" + DateTime.Now + "] DbUpdateException : " + ex.Message);
                                }
                            }
                            Console.WriteLine("Saved {0} favorites so far to the database ({1} entities total); {2} duplicates not saved", numFavorites, recordsAffected, duplicatesFound);
                            pageNum++;
                        }
                        else 
                        {
                            Console.WriteLine("Reached the end, no data returned");
                            break; 
                        }
                    }
                    else if (favoriteResponse.Result == RequestResult.RateLimited 
                        || favoriteResponse.Result == RequestResult.Unknown)
                    {
                        Console.WriteLine("Rate limit reached. Switching authentication modes and retrying in 15 mins...");
                        Console.WriteLine("Next page to be processed is :     " + pageNum);
                        StreamWriter w1 = new StreamWriter("SaveFavorite_PageSoFar.txt");
                        w1.WriteLine("Record datetime: " + DateTime.Now);
                        w1.WriteLine("Page number next to be: " + pageNum);
                        w1.Close();

                        System.Threading.Thread.Sleep(_fifteenMin);

                        Console.WriteLine("Resuming...");
                        continue;
                    }
                    else //Just in case if the program is unexpectedly failed and closed, record last processed page number
                    {
                        Console.WriteLine("Error: " + favoriteResponse.Result);
                        Console.WriteLine("End of Process"); 
                        break; 
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Record for " + screenname + " not saved. (User probably doesn't exist in database.");
            }
        }

        private static void SaveMemberOf(string screenname)
        {
            try
            {
                FriendshipUser fbuser = (new TwitterEntitiesForFriendship()).FriendshipUsers.Where(u => u.ScreenName.Equals(screenname)).First();

                TimeSpan _fifteenMin = new TimeSpan(0, 15, 0);
                long nextCursor = -1;

                while (true)
                {
                    TwitterResponse<TwitterListCollection> membershipResponse = TwitterList.GetMemberships(_tokens, screenname, new ListMembershipsOptions { Cursor = nextCursor });
                    if (membershipResponse.Result == RequestResult.Success)
                    {
                        JObject j_ob = JObject.Parse(membershipResponse.Content);
                        nextCursor = long.Parse(j_ob["next_cursor"].ToString());

                        int recordsAffected = 0;
                        int numTweets = 0;
                        int duplicatesFound = 0;

                        TwitterListCollection members = membershipResponse.ResponseObject;
                        if (members != null && members.Count > 0)
                        {
                            foreach (TwitterList member in members)
                            {
                                try
                                {
                                    var database = new TwitterEntitiesForFriendship();

                                    if (!database.TwitterLists.Any(t => t.Id == member.Id)) //check if tweet already exists
                                    {
                                        member.User.Status = null;
                                        database.TwitterLists.AddObject(
                                            new TwitterLists
                                            {
                                                Id = member.Id,
                                                Name = member.Name,
                                                FullName = member.FullName,
                                                Description = member.User.Description,
                                                AbsolutePath = member.AbsolutePath,
                                                NumberOfMembers = member.NumberOfMembers,
                                                NumberOfSubscribers = member.NumberOfSubscribers,
                                                Slug = member.Slug,
                                                User_Id = member.User.Id.ToString(),
                                                Recorded_At = DateTime.Now,
                                                IsMemberOf = true,
                                                IsSubscribedTo = false,
                                                Mode = member.Mode,
                                                From_Who = fbuser.Name,
                                                From_Who_ScreenName = fbuser.ScreenName
                                            });
                                        recordsAffected += database.SaveChanges();
                                        numTweets++;
                                    }
                                    else
                                    {
                                        duplicatesFound++;
                                    }
                                }
                                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                                {
                                    Console.Error.WriteLine("[" + DateTime.Now + "] DbUpdateException : " + ex.Message);
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                                }
                            }
                            Console.WriteLine("Saved {0} tweets so far to the database ({1} entities total); {2} duplicates not saved", numTweets, recordsAffected, duplicatesFound);
                        }
                        else
                        {
                            Console.WriteLine("Reached the end, no data returned");
                            break;
                        }
                    }
                    else if (membershipResponse.Result == RequestResult.RateLimited
                        || membershipResponse.Result == RequestResult.Unknown)
                    {
                        Console.WriteLine("Rate limit reached. Switching authentication modes and retrying in 15 mins...");
                        Console.WriteLine("Next Cursor to be processed is :     " + nextCursor);
                        StreamWriter w1 = new StreamWriter("SaveMemberOf_PageSoFar.txt");
                        w1.WriteLine("Record datetime: " + DateTime.Now);
                        w1.WriteLine("Next Cursor number next to be: " + nextCursor);
                        w1.Close();

                        System.Threading.Thread.Sleep(_fifteenMin);

                        Console.WriteLine("Resuming...");
                        continue;
                    }
                    else //Just in case if the program is unexpectedly failed and closed, record last processed page number
                    {
                        Console.WriteLine("Error: " + membershipResponse.Result);
                        Console.WriteLine("End of Process");
                        break;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Record for " + screenname + " not saved. (User probably doesn't exist in database.");
            }
        }

        private static void SaveSubscribedTo(string screenname)
        {
            try
            {
                FriendshipUser fbuser = (new TwitterEntitiesForFriendship()).FriendshipUsers.Where(u => u.ScreenName.Equals(screenname)).First();

                TimeSpan _fifteenMin = new TimeSpan(0, 15, 0);

                TwitterResponse<TwitterListCollection> listResponse =
                        TwitterList.GetLists(_tokens, new GetListsOptions { ScreenName = screenname });
                if (listResponse.Result == RequestResult.Success)
                {
                    int recordsAffected = 0;
                    int numTweets = 0;
                    int duplicatesFound = 0;

                    TwitterListCollection members = listResponse.ResponseObject;
                    if (members != null && members.Count > 0)
                    {
                        foreach (TwitterList member in members)
                        {
                            try
                            {
                                var database = new TwitterEntitiesForFriendship();

                                if (!database.TwitterLists.Any(t => t.Id == member.Id)) //check if tweet already exists
                                {
                                    member.User.Status = null;
                                    database.TwitterLists.AddObject(
                                        new TwitterLists
                                        {
                                            Id = member.Id,
                                            Name = member.Name,
                                            FullName = member.FullName,
                                            Description = member.User.Description,
                                            AbsolutePath = member.AbsolutePath,
                                            NumberOfMembers = member.NumberOfMembers,
                                            NumberOfSubscribers = member.NumberOfSubscribers,
                                            Slug = member.Slug,
                                            User_Id = member.User.Id.ToString(),
                                            Recorded_At = DateTime.Now,
                                            IsMemberOf = false,
                                            IsSubscribedTo = true,
                                            Mode = member.Mode,
                                            From_Who = fbuser.Name,
                                            From_Who_ScreenName = fbuser.ScreenName
                                        });
                                    recordsAffected += database.SaveChanges();
                                    numTweets++;
                                }
                                else
                                {
                                    duplicatesFound++;
                                }
                            }
                            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                            {
                                Console.Error.WriteLine("[" + DateTime.Now + "] DbUpdateException : " + ex.Message);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                            }
                        }
                        Console.WriteLine("Saved {0} tweets so far to the database ({1} entities total); {2} duplicates not saved", numTweets, recordsAffected, duplicatesFound);
                    }
                }
                else if (listResponse.Result == RequestResult.RateLimited
                    || listResponse.Result == RequestResult.Unknown)
                {
                    Console.WriteLine("Rate limit reached. Switching authentication modes and retrying in 15 mins...");

                    System.Threading.Thread.Sleep(_fifteenMin);

                    Console.WriteLine("Resuming...");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Record for " + screenname + " not saved. (User probably doesn't exist in database.");
            }
        }

        private static void SaveSearchResults(string screenname)
        {
            try
            {
                string query = "#" + screenname;  //#screenname
                var database = new TwitterEntitiesForFriendship();
                TimeSpan _fifteenMin = new TimeSpan(0, 15, 0);

                //*** JUST IN CASE IF THERE ARE MORE THAN NumberPerPage ***//
                //long since_id = 111111111111111111;
                //long max_id = 999999999999999999;
                //var earliestTweetInDb = (from t in database.TwitterSearches1
                //                         where  
                //                                 t.IdStr.Length > 18 || 
                //                                (t.IdStr.Length == 18 && t.IdStr.CompareTo(_sinceTweetIdAsString) > 0)
                                               
                //                         orderby t.Id
                //                         select t.Id).FirstOrDefault();

                //if (earliestTweetInDb != 0)
                //{
                //    max_id = earliestTweetInDb;
                //}

                //!!!!!!!!!! Twitter Search only returns tweets in the past week ( < 7days) !!!!!!!!!!!!!!!//
                TwitterResponse<TwitterSearchResultCollection> searchResponse =
                        TwitterSearch.Search(_tokens, query, new SearchOptions { NumberPerPage = 100 });

                if (searchResponse.Result == RequestResult.Success)
                {
                    int recordsAffected = 0;
                    int numTweets = 0;
                    int duplicatesFound = 0;

                    TwitterSearchResultCollection results = searchResponse.ResponseObject;
                    if (results != null && results.Count > 0)
                    {
                        foreach (TwitterSearchResult result in results)
                        {
                            try
                            {
                                //Get User's ID and Screen_Name
                                JObject j_obj = result.User;
                                string userid = j_obj["id"].ToString();
                                string userscreenname = j_obj["screen_name"].ToString();

                                //Get media information
                                string mediaUrl = "";
                                JObject j_obj2 = result.Entities;
                                if (j_obj2 != null)
                                {
                                    JToken mediaJObject = j_obj2["media"];
                                    if (mediaJObject != null)
                                    {
                                        string mediaArray = mediaJObject[0].ToString();
                                        JObject mediaArrayObject = JObject.Parse(mediaArray);
                                        mediaUrl = mediaArrayObject["media_url"].ToString();
                                    }
                                }
                                
                                if (!database.TwitterSearches1.Any(t => t.Id == result.Id)) //check if tweet already exists
                                {
                                    var includePic = false;
                                    if (mediaUrl.Contains("jpg"))   //Picture contained in tweet?
                                    {
                                        includePic = true;
                                    }

                                    var includeVideo = false;
                                    if(result.Text.Contains("https://")){   //Video contained in tweet?
                                        includeVideo = true;
                                    }

                                    database.TwitterSearches1.AddObject(
                                        new TwitterSearches
                                        {
                                            Id = result.Id,
                                            IdStr = result.Id.ToString(),
                                            Text = result.Text,
                                            Source = result.Source,
                                            Created_Date = result.CreatedDate,
                                            Retweet_Count = result.RetweetCount,
                                            Favorite_Count = result.FavoriteCount,
                                            Location = result.Location,
                                            InReplyTo_StatusId = result.InReplyTo_StatusId,
                                            InReplyTo_Screenname = result.InReplyToScreenname,
                                            InReplyTo_UserId = result.InReplyToScreenname,
                                            Is_Picture_Included = includePic,
                                            Is_Video_Included = includeVideo,
                                            Searched_Words = query,
                                            User_Id = userid,
                                            User_ScreenName = userscreenname
                                        });
                                    recordsAffected += database.SaveChanges();
                                    numTweets++;
                                }
                                else
                                {
                                    duplicatesFound++;
                                }
                            }
                            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                            {
                                Console.Error.WriteLine("[" + DateTime.Now + "] DbUpdateException : " + ex.Message);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected error: " + ex.Message);
                            }
                        }
                        Console.WriteLine("Saved {0} tweets so far to the database ({1} entities total); {2} duplicates not saved", numTweets, recordsAffected, duplicatesFound);
                    }
                    else
                    {
                        Console.WriteLine("No data available (Perhaps no result is shown by search)");
                    }
                }
                else if (searchResponse.Result == RequestResult.RateLimited
                    || searchResponse.Result == RequestResult.Unknown)
                {
                    Console.WriteLine("Rate limit reached. Switching authentication modes and retrying in 15 mins...");

                    System.Threading.Thread.Sleep(_fifteenMin);

                    Console.WriteLine("Resuming...");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Record for " + screenname + " not saved. (User probably doesn't exist in database.");
            }
        }

        private static void SaveUserTimeline(string screenname)
        {
            try
            {
                FriendshipUser fUser = (new TwitterEntitiesForFriendship()).FriendshipUsers.Where(u => u.ScreenName.Equals(screenname)).First();
                SaveUserTimeline(fUser);
            }
            catch (Exception)
            {
                Console.WriteLine("Timeline for " + screenname + " not saved. (User probably doesn't exist in database.");
            }
        }

        private static void SaveUserTimeline(FriendshipUser fUser)
        {
            try
            {
                setFriendshipIsTweetsInProgress(fUser.Id, true); //to stop other parallel runs from trying to process the same user

                var db = new TwitterContext();
                var _friendshipContext = new TwitterEntitiesForFriendship();

                TwitterResponse<TwitterStatusCollection> timelineResponse;
                int recordsAffected = 0;
                int numTweets = 0;
                int duplicatesFound = 0;
                //bool recheckEarliestTweet = true;

                decimal earliestTweetId = _maxTweetId;

                //TEMPORARY CODE FOR FINDING LAST TWEET THAT WAS PROCESSED [FOR UNFINISHED WORK]
                //var earliestTweetInDb = (from t in _friendshipContext.FriendshipTweets
                //                         where t.User_Id == fUser.Id && t.RetweetCountString.Equals("0")
                //                            && (
                //                                t.Id.Trim().Length > _sinceTweetIdAsString.Length || //previously hardcoded 18 as _Oct1stBaseTweet.Length = 18
                //                                (t.Id.Trim().Length == 18 && t.Id.Trim().CompareTo(_sinceTweetIdAsString) > 0)
                //                               )
                //                         orderby t.Id.Length, t.Id
                //                         select t.Id).FirstOrDefault();

                //if (!string.IsNullOrEmpty(earliestTweetInDb))
                //{
                //    earliestTweetId = Convert.ToDecimal(earliestTweetInDb);
                //}

                Console.WriteLine("\nAttempting to save timeline for " + fUser.ScreenName + " (" + fUser.NumberOfStatuses + " tweets) ...");

                do
                {
                    timelineResponse = TwitterTimeline.UserTimeline(_tokens, new UserTimelineOptions { UserId = fUser.Id, Count = _MaxNumberOfTweets, MaxStatusId = earliestTweetId - 1, SinceStatusId = _sinceTweetId, IncludeRetweets = true });

                    if (timelineResponse.Result == RequestResult.Success)
                    {
                        TwitterStatusCollection tweets = timelineResponse.ResponseObject;

                        if (tweets != null)
                        {
                            foreach (TwitterStatus tweet in tweets)
                            {
                                try
                                {
                                    _friendshipContext = new TwitterEntitiesForFriendship();
                                    db = new TwitterContext();
                                  
                                    //check if tweet belongs to wrong user - twitter has (presumably erroneously) returned tweets from another user on more than one occasion)
                                    if (tweet.User != null && fUser.Id != tweet.User.Id)
                                    {
                                        continue;
                                    }

                                    if (!db.Tweets.Any(t => t.Id == tweet.Id)) //check if tweet already exists
                                    {
                                        //don't re-add the user
                                        tweet.User.Status = null;
                                        db.Entry(tweet.User).State = EntityState.Unchanged;

                                        //don't re-add the place
                                        if (tweet.Place != null)
                                        {
                                            var existingPlace = getExistingPlaceIfExists(db, tweet.Place);
                                            if (existingPlace != null)
                                            {
                                                tweet.Place = existingPlace;
                                            }
                                        }

                                        //don't re-add the retweet's: status (tweet), user, or place
                                        if (tweet.RetweetedStatus != null)
                                        {
                                            //status - this should be abstracted to function like the checks for user and place (especially if reused)
                                            var existingTweet = db.Tweets.FirstOrDefault(t => t.Id == tweet.RetweetedStatus.Id);
                                            if (existingTweet != null)
                                            {
                                                //using dumb long-winded way because I can't figure out how to use .Include( users ) on db.Tweets above.
                                                //  and changing the state doesn't seem to work.
                                                var existingUser = getExistingUserIfExists(db, tweet.RetweetedStatus.User);
                                                if (existingUser != null)
                                                {
                                                    existingTweet.User = existingUser;
                                                }
                                                tweet.RetweetedStatus = existingTweet;
                                            }
                                            else
                                            {
                                                //place
                                                if (tweet.RetweetedStatus.Place != null)
                                                {
                                                    var existingPlace = getExistingPlaceIfExists(db, tweet.RetweetedStatus.Place);
                                                    if (existingPlace != null)
                                                    {
                                                        tweet.RetweetedStatus.Place = existingPlace;
                                                    }
                                                }

                                                //user
                                                if (tweet.RetweetedStatus.User != null) //I think this is always true if we have a retweet
                                                {
                                                    if (db.Users.Any(u => u.Id == tweet.RetweetedStatus.User.Id))
                                                    {
                                                        if (tweet.RetweetedStatus.User.Id != fUser.Id)
                                                        {
                                                            db.Entry(tweet.RetweetedStatus.User).State = EntityState.Unchanged;
                                                        }
                                                        else //retweeted own tweet - don't save this retweet, it should be saved already or will be from a later timeline call
                                                        {
                                                            tweet.RetweetedStatus = null;
                                                        }
                                                    }
                                                    else //new user to add
                                                    {
                                                        tweet.RetweetedStatus.User.Status = null; //add the user but not their current status (this would add a new tweet with user set to null)
                                                    }
                                                }
                                                else
                                                {
                                                    //does this ever happen?
                                                }
                                            }
                                        }
                                        
                                        //Get media information *** Could not perfectly analyse if it contains Photo... ***//
                                        bool IsMediaContained = false;
                                        Twitterizer.Entities.TwitterEntityCollection entityCollection = tweet.Entities;
                                        if (entityCollection != null)
                                        {
                                            if (entityCollection.Count > 0)
                                            {
                                                Twitterizer.Entities.TwitterEntity mediaEntity = entityCollection[entityCollection.Count - 1];
                                                string mediaJObjectStr = mediaEntity.ToString();
                                                if (mediaJObjectStr.Contains("TwitterMediaEntity"))
                                                {
                                                    IsMediaContained = true;
                                                }
                                            }
                                        }

                                        var istherePic = false;
                                        var isthereVideo = false;

                                        //When text includes a video file, it has https:// link
                                        if (tweet.Text.Contains("https://"))
                                        {
                                            isthereVideo = true;
                                        }
                                        //Quite not sure http:// is only for pictures. It also could be a link to other web pages
                                        /*** It needs to be considered further ***/
                                        if (tweet.Text.Contains("http") && IsMediaContained)
                                        {
                                            istherePic = true;
                                        }
                                        

                                        //Check if retweeted_Status is null
                                        var retweetedStatusId = tweet.RetweetedStatus;
                                        string retweetedStatusIdStr = null;
                                        if (retweetedStatusId != null)
                                        {
                                            retweetedStatusIdStr = tweet.Id.ToString();
                                        }

                                        //Check if place_id is null
                                        var placeId = tweet.Place;
                                        string placeIdStr = null;
                                        if (placeId != null)
                                        {
                                            placeIdStr = placeId.ToString();
                                        }

                                        //Check if geo_id is null
                                        //var geoId = tweet.Geo;
                                        int? geoIdInt = null;
                                        //if (geoId != null)
                                        //{
                                        //    geoIdInt = Convert.ToInt32(geoId);
                                        //}

                                        _friendshipContext.FriendshipTweets.AddObject(new FriendshipTweet { 
                                            Id = tweet.Id, 
                                            StringId = tweet.StringId,
                                            CreatedDate = tweet.CreatedDate,
                                            Source = tweet.Source,
                                            InReplyToScreenName = tweet.InReplyToScreenName,
                                            InReplyToStatusId = tweet.InReplyToStatusId,
                                            InReplyToUserId = tweet.InReplyToUserId,
                                            Text = tweet.Text,
                                            FavoriteCountString = tweet.FavoriteCountString,
                                            RetweetCountString = tweet.RetweetCountString,
                                            Retweeted = tweet.Retweeted,
                                            RetweetedStatus_Id = retweetedStatusIdStr,
                                            User_Id = tweet.User.Id,
                                            Place_Id = placeIdStr,
                                            Geo_id = geoIdInt,
                                            includePic = istherePic,
                                            includeVideo = isthereVideo
                                        });
                                        //db.Tweets.Add(tweet);
                                        //recordsAffected += db.SaveChanges();
                                        recordsAffected += _friendshipContext.SaveChanges();
                                        numTweets++;
                                    }
                                    else
                                    {
                                        duplicatesFound++;
                                    }
                                }
                                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                                {
                                    Console.Error.WriteLine("[" + DateTime.Now + "] DbUpdateException : " + ex.Message);
                                    Exception inner = ex.InnerException;
                                    string tab = "";

                                    while (inner != null)
                                    {
                                        tab += "\t";
                                        Console.Error.WriteLine(tab + "Inner exception: " + inner.Message);
                                        inner = inner.InnerException;
                                    }

                                    //assume PK violation and abort - let the other instance finish it off or eventually try again later.
                                    Console.Error.WriteLine("Aborting for user " + fUser.ScreenName);
                                    return;
                                }
                            }
                            Console.WriteLine("Saved {0} tweets so far to the database ({1} entities total); {2} duplicates not saved", numTweets, recordsAffected, duplicatesFound);

                            if (tweets.Count < _MaxNumberOfTweets)
                            {
                                goto EndLoop; //hack
                            }
                            else
                            {
                                //set next MaxStatusId to the earliest tweet returned from the last timeline call
                                var earliestTweetIdReturned = tweets.OrderBy(t => t.CreatedDate).Select(t => t.Id).FirstOrDefault();

                                if (!string.IsNullOrEmpty(earliestTweetIdReturned.ToString()))
                                {
                                    earliestTweetId = Convert.ToDecimal(earliestTweetIdReturned);
                                }
                            }
                            //TODO: a check if numtweets hasn't changed in the last 5 attempts (or so) then give up
                        }

                    }
                    else if (timelineResponse.Result == RequestResult.Unauthorized || timelineResponse.Result == RequestResult.FileNotFound)
                    {
                        /**
                         * Attempt to fix a bug discovered on 2012-06-21: user no longer exists so Twitter returns a 
                         * FileNotFound error ('sorry the page no longer exists'). Because of the hack above which 
                         * forces the loop to continue it keeps looping and getting the same error until all 350 calls
                         * are exhausted then repeats and repeats :(
                         * 
                         * Attempted fix/change is: added "|| timelineResponse.Result == RequestResult.FileNotFound"
                         * treat no-longer-existant users the same as protected users.
                         **/
                        Console.WriteLine("User " + fUser.ScreenName + " is now protected or no longer exists.");

                        TwitterUser u = db.Users.FirstOrDefault(s => s.Id == fUser.Id);
                        u.IsProtected = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        HandleTwitterizerError<TwitterStatusCollection>(timelineResponse);

                        if (timelineResponse.ResponseObject == null) //this is a hack to force the loop to continue (force timelineResponse.ResponseObject.Count to be > 0)
                        {
                            var t = new TwitterStatusCollection();
                            t.Add(new TwitterStatus());
                            timelineResponse.ResponseObject = t;
                        }
                    }
                } while (timelineResponse.ResponseObject != null && timelineResponse.ResponseObject.Count > 0);

            EndLoop:
                //and when finished, set isTweetsProcessed flag set to 1 !
                setFriendshipIsTweetsAdded(fUser.Id, true);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected exception. Message is: " + e.Message);
                return; //or throw?
            }
            finally
            {
                setFriendshipIsTweetsInProgress(fUser.Id, false);
            }
        }

        private static void SaveUserTimelineSecond(string screenname)
        {
            try
            {
                FriendshipUser fUser = (new TwitterEntitiesForFriendship()).FriendshipUsers.Where(u => u.ScreenName.Equals(screenname)).First();
                SaveUserTimelineSecond(fUser);
            }
            catch (Exception)
            {
                Console.WriteLine("Timeline for " + screenname + " not saved. (User probably doesn't exist in database.");
            }
        }

        private static void SaveUserTimelineSecond(FriendshipUser fUser)
        {
            try
            {
                setFriendshipIsTweetsInProgress(fUser.Id, true); //to stop other parallel runs from trying to process the same user

                var db = new TwitterContext();
                var _friendshipContext = new TwitterEntitiesForFriendship();

                TwitterResponse<TwitterStatusCollection> timelineResponse;
                int recordsAffected = 0;
                int numTweets = 0;
                int duplicatesFound = 0;
                //bool recheckEarliestTweet = true;

                decimal earliestTweetId = _maxTweetId;

                //TEMPORARY CODE FOR FINDING LAST TWEET THAT WAS PROCESSED [FOR UNFINISHED WORK]
                var earliestTweetInDb = (from t in _friendshipContext.FriendshipTweets
                                         where t.User_Id == fUser.Id && t.RetweetCountString.Equals("0")
                                            && (
                                                t.Id.Trim().Length > _sinceTweetIdAsString.Length || //previously hardcoded 18 as _Oct1stBaseTweet.Length = 18
                                                (t.Id.Trim().Length == 18 && t.Id.Trim().CompareTo(_sinceTweetIdAsString) > 0)
                                               )
                                         orderby t.Id.Length, t.Id
                                         select t.Id).FirstOrDefault();

                if (!string.IsNullOrEmpty(earliestTweetInDb))
                {
                    earliestTweetId = Convert.ToDecimal(earliestTweetInDb);
                }

                Console.WriteLine("\nAttempting to save timeline for " + fUser.ScreenName + " (" + fUser.NumberOfStatuses + " tweets) ...");

                do
                {
                    timelineResponse = TwitterTimeline.UserTimeline(_tokens, new UserTimelineOptions { UserId = fUser.Id, Count = _MaxNumberOfTweets, MaxStatusId = earliestTweetId - 1, SinceStatusId = _sinceTweetId, IncludeRetweets = true });

                    if (timelineResponse.Result == RequestResult.Success)
                    {
                        TwitterStatusCollection tweets = timelineResponse.ResponseObject;

                        if (tweets != null)
                        {
                            foreach (TwitterStatus tweet in tweets)
                            {
                                try
                                {
                                    _friendshipContext = new TwitterEntitiesForFriendship();
                                    db = new TwitterContext();

                                    //check if tweet belongs to wrong user - twitter has (presumably erroneously) returned tweets from another user on more than one occasion)
                                    if (tweet.User != null && fUser.Id != tweet.User.Id)
                                    {
                                        continue;
                                    }

                                    if (!db.Tweets.Any(t => t.Id == tweet.Id)) //check if tweet already exists
                                    {
                                        //don't re-add the user
                                        tweet.User.Status = null;
                                        db.Entry(tweet.User).State = EntityState.Unchanged;

                                        //don't re-add the place
                                        if (tweet.Place != null)
                                        {
                                            var existingPlace = getExistingPlaceIfExists(db, tweet.Place);
                                            if (existingPlace != null)
                                            {
                                                tweet.Place = existingPlace;
                                            }
                                        }

                                        //don't re-add the retweet's: status (tweet), user, or place
                                        if (tweet.RetweetedStatus != null)
                                        {
                                            //status - this should be abstracted to function like the checks for user and place (especially if reused)
                                            var existingTweet = db.Tweets.FirstOrDefault(t => t.Id == tweet.RetweetedStatus.Id);
                                            if (existingTweet != null)
                                            {
                                                //using dumb long-winded way because I can't figure out how to use .Include( users ) on db.Tweets above.
                                                //  and changing the state doesn't seem to work.
                                                var existingUser = getExistingUserIfExists(db, tweet.RetweetedStatus.User);
                                                if (existingUser != null)
                                                {
                                                    existingTweet.User = existingUser;
                                                }
                                                tweet.RetweetedStatus = existingTweet;
                                            }
                                            else
                                            {
                                                //place
                                                if (tweet.RetweetedStatus.Place != null)
                                                {
                                                    var existingPlace = getExistingPlaceIfExists(db, tweet.RetweetedStatus.Place);
                                                    if (existingPlace != null)
                                                    {
                                                        tweet.RetweetedStatus.Place = existingPlace;
                                                    }
                                                }

                                                //user
                                                if (tweet.RetweetedStatus.User != null) //I think this is always true if we have a retweet
                                                {
                                                    if (db.Users.Any(u => u.Id == tweet.RetweetedStatus.User.Id))
                                                    {
                                                        if (tweet.RetweetedStatus.User.Id != fUser.Id)
                                                        {
                                                            db.Entry(tweet.RetweetedStatus.User).State = EntityState.Unchanged;
                                                        }
                                                        else //retweeted own tweet - don't save this retweet, it should be saved already or will be from a later timeline call
                                                        {
                                                            tweet.RetweetedStatus = null;
                                                        }
                                                    }
                                                    else //new user to add
                                                    {
                                                        tweet.RetweetedStatus.User.Status = null; //add the user but not their current status (this would add a new tweet with user set to null)
                                                    }
                                                }
                                                else
                                                {
                                                    //does this ever happen?
                                                }
                                            }
                                        }

                                        //Get media information *** Could not perfectly analyse if it contains Photo... ***//
                                        bool IsMediaContained = false;
                                        Twitterizer.Entities.TwitterEntityCollection entityCollection = tweet.Entities;
                                        if (entityCollection != null)
                                        {
                                            if (entityCollection.Count > 0)
                                            {
                                                Twitterizer.Entities.TwitterEntity mediaEntity = entityCollection[entityCollection.Count - 1];
                                                string mediaJObjectStr = mediaEntity.ToString();
                                                if (mediaJObjectStr.Contains("TwitterMediaEntity"))
                                                {
                                                    IsMediaContained = true;
                                                }
                                            }
                                        }

                                        var istherePic = false;
                                        var isthereVideo = false;

                                        //When text includes a video file, it has https:// link
                                        if (tweet.Text.Contains("https://"))
                                        {
                                            isthereVideo = true;
                                        }
                                        //Quite not sure http:// is only for pictures. It also could be a link to other web pages
                                        /*** It needs to be considered further ***/
                                        if (tweet.Text.Contains("http") && IsMediaContained)
                                        {
                                            istherePic = true;
                                        }


                                        //Check if retweeted_Status is null
                                        var retweetedStatusId = tweet.RetweetedStatus;
                                        string retweetedStatusIdStr = null;
                                        if (retweetedStatusId != null)
                                        {
                                            retweetedStatusIdStr = tweet.Id.ToString();
                                        }

                                        //Check if place_id is null
                                        var placeId = tweet.Place;
                                        string placeIdStr = null;
                                        if (placeId != null)
                                        {
                                            placeIdStr = placeId.ToString();
                                        }

                                        //Check if geo_id is null
                                        //var geoId = tweet.Geo;
                                        int? geoIdInt = null;
                                        //if (geoId != null)
                                        //{
                                        //    geoIdInt = Convert.ToInt32(geoId);
                                        //}

                                        _friendshipContext.FriendshipTweets.AddObject(new FriendshipTweet
                                        {
                                            Id = tweet.Id,
                                            StringId = tweet.StringId,
                                            CreatedDate = tweet.CreatedDate,
                                            Source = tweet.Source,
                                            InReplyToScreenName = tweet.InReplyToScreenName,
                                            InReplyToStatusId = tweet.InReplyToStatusId,
                                            InReplyToUserId = tweet.InReplyToUserId,
                                            Text = tweet.Text,
                                            FavoriteCountString = tweet.FavoriteCountString,
                                            RetweetCountString = tweet.RetweetCountString,
                                            Retweeted = tweet.Retweeted,
                                            RetweetedStatus_Id = retweetedStatusIdStr,
                                            User_Id = tweet.User.Id,
                                            Place_Id = placeIdStr,
                                            Geo_id = geoIdInt,
                                            includePic = istherePic,
                                            includeVideo = isthereVideo
                                        });
                                        //db.Tweets.Add(tweet);
                                        //recordsAffected += db.SaveChanges();
                                        recordsAffected += _friendshipContext.SaveChanges();
                                        numTweets++;
                                    }
                                    else
                                    {
                                        duplicatesFound++;
                                    }
                                }
                                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                                {
                                    Console.Error.WriteLine("[" + DateTime.Now + "] DbUpdateException : " + ex.Message);
                                    Exception inner = ex.InnerException;
                                    string tab = "";

                                    while (inner != null)
                                    {
                                        tab += "\t";
                                        Console.Error.WriteLine(tab + "Inner exception: " + inner.Message);
                                        inner = inner.InnerException;
                                    }

                                    //assume PK violation and abort - let the other instance finish it off or eventually try again later.
                                    Console.Error.WriteLine("Aborting for user " + fUser.ScreenName);
                                    return;
                                }
                            }
                            Console.WriteLine("Saved {0} tweets so far to the database ({1} entities total); {2} duplicates not saved", numTweets, recordsAffected, duplicatesFound);

                            if (tweets.Count < _MaxNumberOfTweets)
                            {
                                goto EndLoop; //hack
                            }
                            else
                            {
                                //set next MaxStatusId to the earliest tweet returned from the last timeline call
                                var earliestTweetIdReturned = tweets.OrderBy(t => t.CreatedDate).Select(t => t.Id).FirstOrDefault();

                                if (!string.IsNullOrEmpty(earliestTweetIdReturned.ToString()))
                                {
                                    earliestTweetId = Convert.ToDecimal(earliestTweetIdReturned);
                                }
                            }
                            //TODO: a check if numtweets hasn't changed in the last 5 attempts (or so) then give up
                        }

                    }
                    else if (timelineResponse.Result == RequestResult.Unauthorized || timelineResponse.Result == RequestResult.FileNotFound)
                    {
                        /**
                         * Attempt to fix a bug discovered on 2012-06-21: user no longer exists so Twitter returns a 
                         * FileNotFound error ('sorry the page no longer exists'). Because of the hack above which 
                         * forces the loop to continue it keeps looping and getting the same error until all 350 calls
                         * are exhausted then repeats and repeats :(
                         * 
                         * Attempted fix/change is: added "|| timelineResponse.Result == RequestResult.FileNotFound"
                         * treat no-longer-existant users the same as protected users.
                         **/
                        Console.WriteLine("User " + fUser.ScreenName + " is now protected or no longer exists.");

                        TwitterUser u = db.Users.FirstOrDefault(s => s.Id == fUser.Id);
                        u.IsProtected = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        HandleTwitterizerError<TwitterStatusCollection>(timelineResponse);

                        if (timelineResponse.ResponseObject == null) //this is a hack to force the loop to continue (force timelineResponse.ResponseObject.Count to be > 0)
                        {
                            var t = new TwitterStatusCollection();
                            t.Add(new TwitterStatus());
                            timelineResponse.ResponseObject = t;
                        }
                    }

                    //if (timelineResponse.ResponseObject == null || timelineResponse.ResponseObject.Count <= 0)
                    //{
                    //    break;
                    //}
                } while (timelineResponse.ResponseObject != null && timelineResponse.ResponseObject.Count > 0);

            EndLoop:
                //and when finished, set isTweetsProcessed flag set to 1 !
                setFriendshipIsTweetsAdded(fUser.Id, true);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("[" + DateTime.Now + "] Unexpected exception. Message is: " + e.Message);
                return; //or throw?
            }
            finally
            {
                setFriendshipIsTweetsInProgress(fUser.Id, false);
            }
        }

        private static TwitterUser getExistingUserIfExists(TwitterContext db, TwitterUser user)
        {
            return db.Users.FirstOrDefault(u => u.Id == user.Id);
        }

        private static TwitterPlace getExistingPlaceIfExists(TwitterContext db, TwitterPlace place)
        {
            return db.Places.FirstOrDefault(p => p.Id.Equals(place.Id));
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        private static void setFriendshipIsTweetsInProgress(decimal userId, bool status)
        {
            var _friendshipContext = new TwitterEntitiesForFriendship();

            //old code using dbo.Friendship
            //var fship = _friendshipContext.Friendships.Where(f => f.FollowerUserId == userId);
            //foreach (var f in fship.ToList())
            //{
            //    f.IsTweetsInProgress = status;
            //}

            //new code using dbo.FollowerFriendship
            var fship = _friendshipContext.FollowerFriendships.Where(f => f.UserId == userId).First();

            fship.IsTweetsInProgress = status;

            _friendshipContext.SaveChanges();
        }

        private static void setFriendshipIsTweetsAdded(decimal userId, bool status)
        {
            var _friendshipContext = new TwitterEntitiesForFriendship();

            //old code using dbo.Friendship
            //var fship = _friendshipContext.Friendships.Where(f => f.FollowerUserId == userId);
            //foreach (var f in fship.ToList())
            //{
            //    f.IsTweetsAdded = status;
            //}

            //new code using dbo.FollowerFriendship
            var fship = _friendshipContext.FollowerFriendships.Where(f => f.UserId == userId).First();

            fship.IsTweetsAdded = status;

            _friendshipContext.SaveChanges();
        }

        private static void ShowRateLimitDetails()
        {
            var v = new VerifyCredentialsOptions();
            v.UseSSL = true;

            var AccountResponse = TwitterAccount.VerifyCredentials(_tokens, v);

            if (AccountResponse.Result == RequestResult.Success)
            {
                TwitterUser acc = AccountResponse.ResponseObject;
                RateLimiting status = AccountResponse.RateLimiting;

                Console.WriteLine("\n");
                Console.WriteLine("Screenname     : " + acc.ScreenName);
                Console.WriteLine("Hourly limit   : " + status.Total);
                Console.WriteLine("Remaining hits : " + status.Remaining);
                Console.WriteLine("Reset time     : " + status.ResetDate + " (" + DateTime.Now.ToUniversalTime().Subtract(status.ResetDate).Duration().TotalMinutes + " mins left)");
            }
            else
            {
                HandleTwitterizerError<TwitterUser>(AccountResponse);
            }
        }

        private static void printWorking() { Console.WriteLine("\nplease wait..."); }

        private static void PrintCompleted() { Console.WriteLine("\n[" + DateTime.Now + "] Task Ended."); }

        private static void HandleTwitterizerError<T>(TwitterResponse<T> response, bool wait) where T : Twitterizer.Core.ITwitterObject
        {
            // Something bad happened, time to figure it out.
            string rawDataReturnedByTwitter = response.Content;
            string errorMessageReturnedByTwitter = response.ErrorMessage;
            if (string.IsNullOrEmpty(errorMessageReturnedByTwitter))
            {
                errorMessageReturnedByTwitter = "No error given";
            }
            Console.Error.Write("[" + DateTime.Now + "] Error from twitter: " + errorMessageReturnedByTwitter + " | ");
            // The possible reasons something went wrong
            switch (response.Result)
            {
                case RequestResult.FileNotFound:
                    Console.Error.WriteLine("This usually means the user doesn't exist.");
                    break;
                case RequestResult.BadRequest:
                    Console.Error.WriteLine("An unknown error occurred (RequestResult = BadRequest).");
                    break;
                case RequestResult.Unauthorized:
                    Console.Error.WriteLine("An unknown error occurred (RequestResult = Unauthorized).");
                    break;
                case RequestResult.NotAcceptable:
                    Console.Error.WriteLine("An unknown error occurred (RequestResult = NotAcceptable).");
                    break;
                case RequestResult.RateLimited:
                    TimeSpan ttr = DateTime.Now.ToUniversalTime().Subtract(response.RateLimiting.ResetDate).Duration();
                    Console.WriteLine("Rate limit of " + response.RateLimiting.Total + " reached; ");

                    if (wait)
                    {
                        Console.WriteLine("" + Math.Round(ttr.TotalMinutes, 2) + " mins until it resets so I'm going to wait it out before resuming (last failed api call will not be retried).");

                        while (ttr.TotalSeconds > 0)
                        {
                            Console.WriteLine("Waiting for " + Math.Round(ttr.TotalMinutes, 0) + " more mins ...");
                            System.Threading.Thread.Sleep(_oneMin);
                            ttr = ttr.Subtract(_oneMin);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Changing authentication mode and retrying...");
                    }
                    break;
                case RequestResult.TwitterIsDown:
                    //
                    break;
                case RequestResult.TwitterIsOverloaded:
                    Console.Error.WriteLine("Twitter is overloaded (or down)");
                    System.Threading.Thread.Sleep(_fiveSec);
                    break;
                case RequestResult.ConnectionFailure:
                    Console.Error.WriteLine("An unknown error occurred (RequestResult = ConnectionFailure).");
                    break;
                case RequestResult.Unknown:
                    Console.Error.WriteLine("An unknown error occurred (RequestResult = Unknown).");
                    break;
                default:
                    Console.Error.WriteLine("An unknown error occurred.");
                    break;

            }
        }

        private static void HandleTwitterizerError<T>(TwitterResponse<T> response) where T : Twitterizer.Core.ITwitterObject
        {
            HandleTwitterizerError<T>(response, true);
        }

        private static int[] GetInstanceDetails()
        {
            int numInstances = 0;
            int thisInstanceNumber = 1;

            try
            {
                Console.WriteLine("\n\nEnter the number of instances: ");
                numInstances = int.Parse(Console.ReadLine());

                if (numInstances < 1)
                {
                    throw new Exception("number of instances must be at least 1");
                }

                Console.WriteLine("Enter the number of this instance: ");
                thisInstanceNumber = int.Parse(Console.ReadLine());

                if (thisInstanceNumber < 1 || thisInstanceNumber > numInstances)
                {
                    throw new Exception("number of this instance must be between 1 and the number of instances");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + ". Try again.");
                return null;
            }

            return new int[] { numInstances, thisInstanceNumber };
        }
    }

    public class TwitterContext : DbContext
    {
        public DbSet<TwitterUser> Users { get; set; }
        public DbSet<TwitterStatus> Tweets { get; set; }
        public DbSet<TwitterPlace> Places { get; set; }
        public DbSet<TwitterList> Lists { get; set; }

        public TwitterContext()
            : base("TwitterDb")
        {
            ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.CommandTimeout = 300;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TwitterUser>().HasOptional(u => u.Status)
                .WithOptionalPrincipal(s => s.User);
        }
    }
}