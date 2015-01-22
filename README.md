# TwitterDataRetrieval
Retrieve user's data using Twitterizer library and Twitter API

Twitterizer has been stopped updating its library since early 2014. 
However it is working properly with this program.

This program has been usually run with Microsoft Visual Studio using C#.

In order to use this program properly, this program must be connected to the user's PC's database and create a new database for itself.

First of all, run this program after its edmx (database file) is connected to your PC's database.
This program will automatically created a database and some tables: TwitterStatus, TwitterUser, etc.

However this program will not create tables for storing followers' and following's lists, so the user has to create them himself.

NOTES to use this program:
---------------------------------------------------------------------------------------------------------------------------------------

1. You need to type in screenname for most of processes. 

2. Please do Add/Update User (0a) first of all. User's information will be saved in the TwitterUser table. (10~15 secs per one)

3. Create Followers ID (1a) List is for creating a list of followers, and Record user's followers (3x) will find and record more data of those followers in the list.
You can find the list in Twitter_Followers table, and the followers' information will be saved in TwitterUser table. 
Create Following ID (2a) is for creating a list of following, and Record User's following (3y) will find and record more data of those following in the list. (1 ~ 8 hours per one for doing both processes)

4. Get Timeline (4a) retrieves all timeline data including retweets. You can find the data in TwitterStatus table where isfavorited is null. (5 ~ 20 mins per one)

5. Favourites (5a) retrieves all posts which the user put in his favorite list. You can find the data in TwitterStatus table where isfavorited is not null. (1 ~ 180 mins per one)

6. Member Of (5b) and Subscribed To (5c) are optional commands because most Twitter users do not have these lists. You can find the data in TwitterLists table. (0.5 ~ 20 mins)

7. Search Results (6a) retrieves max 100 results (7 days limit) of a searched word. You can type in like @helloworld or #helloworld or any words for the search.
You can find the data in Twitter_SearchResults. (10 ~ 30 secs per one)
