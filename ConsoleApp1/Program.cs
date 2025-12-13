using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {

        }
    }
    /// <summary>
    /// Player Class for creating Player objects that are added to the database.
    /// ***Need to add system where if ID and Username are already registered then it cannot be added.***
    /// </summary>
    class Player
    {
        public string Username { get; set; }
        public int ID { get; set; }
        public int DeathCount;
        public int KillCount;
        public int MainScore;
        public int HoursPlayed;
        /// <summary>
        /// Constructor method for building a player object. 
        /// It MUST be given a username AND an ID for it to work and so there is only one constructor.
        /// </summary>
        /// <param name="sUsername"></param>
        /// <param name="iID"></param>
        public Player (string sUsername, int iID)
        {
            Username = sUsername;

            DeathCount = 0;
            KillCount = 0;
            MainScore = 0;
            HoursPlayed = 0;
        }
        /// <summary>
        /// Allows the player record to be easily displayed.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string message = "Username: " + Username + " ID: " + ID + " DeathCount: " + DeathCount + " Kill Count: " + KillCount + " Main Score: " + MainScore + " Hours Played" + HoursPlayed;
            return message;
            
        }
    }
    /// <summary>
    /// Class that handles the json file by using Lists.
    /// </summary>
    abstract class DatabaseHandling
    {
        private List<Player> playerList1 = new List<Player>();
        /// <summary>
        /// Takes the string input of a list and writes it to the json file.
        /// </summary>
        /// <param name="json"></param>
        public void AddToDatabase(string json)
        {
            File.WriteAllText("players.json", json);
            Update();
        }
        /// <summary>
        /// Returns a list of all players stored in the json file as objects.
        /// </summary>
        /// <returns></returns>
        public List<Player> RetreiveFromDatabase()
        {
            string jsonData = File.ReadAllText("players.json");
            List<Player> playerList2 = JsonSerializer.Deserialize<List<Player>>(jsonData);
            return playerList2;
        }
        /// <summary>
        /// makes sure the file is ordered by player ID.
        /// </summary>
        public void SortDatabase()
        {
            //If I didn't have automatic sorting algorithms that were faster,
            //I would use my slower bubble sort algorithm as shown in comments.
            //bool sorted = false;
            //Player temp1 = null;
            List<Player> playerListSort = RetreiveFromDatabase();
            //while (!sorted)
            //{
            //    sorted = true;
            //    for (int i = 0; i < playerListSort.Count -1; i++)
            //    {
            //        if (playerListSort[i].ID > playerListSort[i + 1].ID)
            //       {
            //            temp1 = playerListSort[i];
            //            playerListSort[i] = playerListSort[i+1];
            //            playerListSort[i + 1] = temp1;
            //        }
            //        else
            //        {
            //            sorted = false;
            //        }
            //    }
            //}
            playerListSort.Sort((p1, p2) => p1.ID.CompareTo(p2.ID));
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(playerListSort, options);
            AddToDatabase(json);
            Update();
        }
        /// <summary>
        /// Tells the user when the database has been updated.
        /// </summary>
        public void Update()
        {
            Console.WriteLine("Database updated.");
        }
    }
    /// <summary>
    /// Player search class inherriting Database Handling. 
    /// It has many constructors to allow for many ways to search.
    /// </summary>
    class PlayerSearch : DatabaseHandling
    {
        private string Username { get; set; }
        private int ID { get; set; }

        public PlayerSearch(string sUsername, int iID) 
        {
            Username = sUsername;
            ID = iID;
        }
        public PlayerSearch(string sUsername)
        {
            Username = sUsername;
        }
        public PlayerSearch(int iID)
        {
            ID = iID;
        }
        /// <summary>
        /// SearchForPlayer retreives all players in the database and compares the username and or ID to the database.
        /// It returns the first match found.
        /// </summary>
        /// <returns></returns>
        public Player SearchForPlayer()
        {
            List<Player> playerList2 = RetreiveFromDatabase();
            foreach (Player p in playerList2)
            {
                if (Username == p.Username || ID == p.ID)
                {
                    Console.WriteLine("Player found:");
                    p.ToString();
                    return p;
                }
            }
            Console.WriteLine("Player not found.");
            return null;
        }
    }
    /// <summary>
    /// UpdateDatabase allows the user to update the scores of players that are in the database.
    /// </summary>
    class UpdateDatabase : DatabaseHandling
    {
        string Username;
        int KillCount;
        int DeathCount;
        int MainScore;
        int HoursPlayed;
        Player UpdatePlayer = null;
        /// <summary>
        /// Finds the player the user wishes to update then sets the values temporarily.
        /// </summary>
        public UpdateDatabase()
        {
            Console.WriteLine("Enter Username of the player who's statistics you wish to update.");
            Username = Console.ReadLine();
            PlayerSearch playerSearch = new PlayerSearch(Username);
            UpdatePlayer = playerSearch.SearchForPlayer();
            KillCount = UpdatePlayer.KillCount;
            DeathCount = UpdatePlayer.DeathCount;
            MainScore = UpdatePlayer.MainScore;
            HoursPlayed = UpdatePlayer.HoursPlayed;
        }
        /// <summary>
        /// The user inputs the added increase to the scores and the database is updated.
        /// </summary>
        /// <param name="iAddKillCount"></param>
        /// <param name="iAddDeathCount"></param>
        /// <param name="iAddHours"></param>
        public void CalculateUpdate(int iAddKillCount, int iAddDeathCount, int iAddHours)
        {
            KillCount += iAddKillCount;
            DeathCount += iAddDeathCount;
            HoursPlayed += iAddHours;
            if(DeathCount > 0)
            {
                MainScore = (KillCount / DeathCount) * 100;
            }
            else
            {
                MainScore = (KillCount) * 100;
            }


            UpdatePlayer.KillCount = KillCount;
            UpdatePlayer.DeathCount = DeathCount;
            UpdatePlayer.MainScore = MainScore;
            UpdatePlayer.HoursPlayed = HoursPlayed;
            List<Player> CurrentList = RetreiveFromDatabase();
            foreach (Player player in CurrentList)
            {
                if(Username.Equals(player.Username))
                {
                    int i = CurrentList.IndexOf(player);
                    CurrentList[i] = UpdatePlayer;
                }
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(CurrentList, options);
            AddToDatabase(json);
            SortDatabase();
        }
    }
    /// <summary>
    /// ***Not Completed***
    /// </summary>
    class DisplayRecords
    {
        int HighScore1;
        int HighScore2;
        int HighScore3;
        int HoursPlayed1;
        int HoursPlayed2;
        int HoursPlayed3;

    }
    /// <summary>
    /// Add Player to database using the next available ID. 
    /// </summary>
    class AddPlayer: DatabaseHandling
    {
        public void AddPlayerToDatabase()
        {
            SortDatabase();
            int iNextID;
            Console.WriteLine("Please enter the username you wish to use.");
            string sUsername = Console.ReadLine();
            Player lastPlayer = RetreiveFromDatabase().LastOrDefault();
            if (lastPlayer != null)
            {
                iNextID = lastPlayer.ID + 1;
            }
            else
            {
                iNextID = 1;
            }

            Player player = new Player(sUsername, iNextID);
            List<Player> CurrentPlayers = RetreiveFromDatabase();
            CurrentPlayers.Add(player);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(CurrentPlayers, options);
            AddToDatabase(json);
        }
    }
}
