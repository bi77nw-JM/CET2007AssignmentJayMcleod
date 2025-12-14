using ConsoleApp1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "players.json";
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                File.WriteAllText(filePath, "[]");
            }
            UserInterface UI = new UserInterface();
            UI.DisplayMainMenu();
        }
    }
    interface IUpdate
    {
        void Update(string message);
    }
    /// <summary>
    /// Player Class for creating Player objects that are added to the database.
    /// </summary>
    class Player
    {
        public string Username { get; set; }
        public int ID { get; set; }
        public int DeathCount { get; set; }
        public int KillCount { get; set; }
        public int MainScore { get; set; }
        public int HoursPlayed { get; set; }

        public Player() { }
        /// <summary>
        /// Constructor method for building a player object. 
        /// It MUST be given a username AND an ID for it to work and so there is only one constructor.
        /// </summary>
        /// <param name="sUsername"></param>
        /// <param name="iID"></param>

        public Player (string sUsername, int iID)
        {
            Username = sUsername;
            ID = iID;
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
            string message = "Username: " + Username + " ID: " + ID + " DeathCount: " + DeathCount + " Kill Count: " + KillCount + " Main Score: " + MainScore + " Hours Played: " + HoursPlayed;
            return message;
            
        }
    }
    /// <summary>
    /// Class that handles the json file by using Lists.
    /// </summary>
    abstract class DatabaseHandling: IUpdate
    {
        //private List<Player> playerList1 = new List<Player>();
        /// <summary>
        /// Takes the string input of a list and writes it to the json file.
        /// </summary>
        /// <param name="json"></param>
        public void AddToDatabase(string json)
        {
            File.WriteAllText("players.json", json);
        }
        /// <summary>
        /// Returns a list of all players stored in the json file as objects.
        /// </summary>
        /// <returns></returns>
        public List<Player> RetrieveFromDatabase()
        {
            string jsonData = File.ReadAllText("players.json");
            List<Player> playerList2 = JsonSerializer.Deserialize<List<Player>>(jsonData);
            return playerList2;
        }
        /// <summary>
        /// makes sure the file is ordered by player ID.
        /// </summary>
        public void SortDatabase(string SortBy)
        {
            SortBy = SortBy.ToLower();
            //If I didn't have automatic sorting algorithms that were faster,
            //I would use my slower bubble sort algorithm as shown in comments.
            //bool sorted = false;
            //Player temp1 = null;
            List<Player> playerListSort = RetrieveFromDatabase();
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
            if (SortBy == "id")
            { playerListSort.Sort((p1, p2) => p1.ID.CompareTo(p2.ID)); }
            else if(SortBy == "high score")
            { playerListSort.Sort((p1, p2) => p2.MainScore.CompareTo(p1.MainScore)); }
            else if(SortBy == "hours played")
            { playerListSort.Sort((p1, p2) => p2.HoursPlayed.CompareTo(p1.HoursPlayed)); }
            else
            {
                Console.WriteLine("Sorting type is invalid. Please re-enter the type you wish to sort by.");
                SortBy = Console.ReadLine();
                SortDatabase(SortBy);
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(playerListSort, options);
            AddToDatabase(json);
            Update($"Database resorted at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        /// <summary>
        /// Tells the user when the database has been updated.
        /// </summary>
        public void Update(string message)
        {

            File.AppendAllText("update_log.txt", message + Environment.NewLine);

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
        /// SearchForPlayer retrieves all players in the database and compares the username and or ID to the database.
        /// It returns the first match found.
        /// </summary>
        /// <returns></returns>
        public Player SearchForPlayer()
        {
            List<Player> playerList2 = RetrieveFromDatabase();
            foreach (Player p in playerList2)
            {
                bool usernameMatch = Username != null && Username == p.Username;
                bool idMatch = ID != 0 && ID == p.ID;
                if (usernameMatch || idMatch)
                {
                    Console.WriteLine("Player found:");
                    Console.WriteLine(p.ToString());
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
            if(UpdatePlayer == null)
            {
                Console.WriteLine("Update Cancelled");
                return;
            }
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
                MainScore = (int)((double)KillCount / DeathCount) * 100;
            }
            else
            {
                MainScore = (KillCount) * 100;
            }


            UpdatePlayer.KillCount = KillCount;
            UpdatePlayer.DeathCount = DeathCount;
            UpdatePlayer.MainScore = MainScore;
            UpdatePlayer.HoursPlayed = HoursPlayed;
            List<Player> CurrentList = RetrieveFromDatabase();
            for (int i = 0; i < CurrentList.Count; i++)
            {
                if (Username.Equals(CurrentList[i].Username))
                {
                    CurrentList[i] = UpdatePlayer;
                }
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(CurrentList, options);
            AddToDatabase(json);
            SortDatabase("ID");
            Update($"{Username} player record updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
    }
    /// <summary>
    /// Shows the top 3 players by main score or hours played.
    /// </summary>
    class DisplayRecords : DatabaseHandling
    {
        public void DisplayHighScores()
        {
            Console.WriteLine("Enter the data type by which you would like to see the records.");
            string SortBy = Console.ReadLine();
            SortDatabase(SortBy);
            List<Player> TopScorers = RetrieveFromDatabase();
            for(int i = 1; i < Math.Min(TopScorers.Count, 3); i++)
            {
                Console.WriteLine($"{i}: {TopScorers[i].ToString()}");
            }
        }
    }
    /// <summary>
    /// Add Player to database using the next available ID. 
    /// </summary>
    class AddPlayer: DatabaseHandling
    {
        public void AddPlayerToDatabase()
        {
            SortDatabase("ID");
            int iNextID;
            Console.WriteLine("Please enter the username you wish to use.");
            string sUsername = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(sUsername))
            {
                Console.WriteLine("Username cannot be empty.");
                return;
            }

            List<Player> CurrentPlayers = RetrieveFromDatabase();

            Player lastPlayer = RetrieveFromDatabase().LastOrDefault();
            if (lastPlayer != null)
            {
                iNextID = lastPlayer.ID + 1;
            }
            else
            {
                iNextID = 1;
            }

            foreach (Player p in CurrentPlayers)
            {
                if (sUsername == p.Username || iNextID == p.ID)
                {
                    Console.WriteLine("Username or ID already exists.");
                    return;
                }
            }

            Player player = new Player(sUsername, iNextID);
            CurrentPlayers.Add(player);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(CurrentPlayers, options);
            AddToDatabase(json);
            Update($"{player.Username} added to database at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
    }

    class UserInterface
    {
        int Pointer;
        int MenuStart;
        int MenuEnd;
        public void DisplayMainMenu()
        {
            Console.WriteLine("---------- Main Menu ----------");
            Console.WriteLine("1: Add player to database.");
            Console.WriteLine("2: Search for a player in the database");
            Console.WriteLine("3: Update a players record.");
            Console.WriteLine("4: Display high scores");
            Console.WriteLine("5: Exit Program");
            MenuStart = 1;
            MenuEnd = 5;
            MenuSelection("Main", MenuStart, MenuEnd);
        }
        public void MenuSelection(string MenuType, int iMenuStart, int iMenuEnd)
        {
            Console.WriteLine("Please select one of the options shown by entering the corresponding number.");
            try
            {
                Pointer = int.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("You have entered an invalid character.");
                MenuSelection(MenuType, iMenuStart, iMenuEnd);
            }
            catch (OverflowException)
            {
                Console.WriteLine("Your input number was too large.");
                MenuSelection(MenuType, iMenuStart, iMenuEnd);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Input attemt unsucclessful: {e.Message}");
                MenuSelection(MenuType, iMenuStart, iMenuEnd);
            }

            if(Pointer >= iMenuStart && Pointer <= iMenuEnd)
            {
                if(MenuType == "Main")
                {
                    MainMenuController();
                }
            }
            else
            {
                Console.WriteLine("Input out of range.");
                MenuSelection(MenuType,iMenuStart, iMenuEnd);
            }
        }
        private int GetIntInput(string input, int min = int.MinValue, int max = int.MaxValue)
        {
            int result;
            while (!int.TryParse(input, out result) || result < min || result > max)
            {
                Console.WriteLine("Invalid input. Please enter a whole number.");
                input = Console.ReadLine();
            }
            return result;
        }
        public void MainMenuController()
        {
            if(Pointer == 1)
            {
                AddPlayer newPlayer = new AddPlayer();
                newPlayer.AddPlayerToDatabase();
                DisplayMainMenu();
            }
            if(Pointer == 2)
            {
                int iID = 0;
                Console.WriteLine("Enter the Username of the player you wish to search. Press enter if you only wish to search by ID");
                string sUsername = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(sUsername)) sUsername = null;

                Console.WriteLine("Enter the ID of the player you wish to search. Press enter if you only with to search by Username");
                string sID = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(sID)) sID = null;
                else
                {
                    iID = GetIntInput(sID);
                }
                
                if(sUsername == null && sID == null)
                {
                    Console.WriteLine("Both fields cannot be null. Please enter at least one value.");
                    MainMenuController();
                }
                PlayerSearch search = (sUsername != null && sID != null) ? new PlayerSearch(sUsername, iID) : (sUsername != null) ? new PlayerSearch(sUsername) : new PlayerSearch(iID);
                search.SearchForPlayer();
                DisplayMainMenu();
            }
            if(Pointer == 3)
            {
                UpdateDatabase update = new UpdateDatabase();
                int iAddKillCount = 0;
                int iAddDeathCount = 0;
                int iAddHoursPlayed = 0;
                string sAKC;
                string sADC;
                string sAHP;
                Console.WriteLine("Enter the amount you wish to add to the kill count.");
                sAKC = Console.ReadLine();
                iAddKillCount = GetIntInput(sAKC);
                Console.WriteLine("Enter the amount you wish to add to the death count");
                sADC = Console.ReadLine();
                iAddDeathCount = GetIntInput(sADC);
                Console.WriteLine("Enter the amount you wish to add to the hours played");
                sAHP = Console.ReadLine();
                iAddHoursPlayed = GetIntInput(sAHP);

                update.CalculateUpdate(iAddKillCount, iAddDeathCount, iAddHoursPlayed);
                DisplayMainMenu();
            }
            if(Pointer == 4)
            {
                DisplayRecords displayRecords = new DisplayRecords();
                displayRecords.DisplayHighScores();
                DisplayMainMenu();
            }
            if(Pointer == 5)
            {
                Console.WriteLine("Exiting program, Goodbye");
                Environment.Exit(0);
            }
        }
    }
}
