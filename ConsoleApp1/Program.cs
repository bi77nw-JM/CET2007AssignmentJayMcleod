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
            //Checking the json file is there then starting the program.
            string filePath = "players.json";
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                File.WriteAllText(filePath, "[]");
            }
            UserInterface UI = new UserInterface();
            UI.DisplayMainMenu();
        }
    }
    /// <summary>
    /// IUpdate is used as an observer interface to write logs.
    /// </summary>
    public interface IUpdate
    {
        void Update(string message);
    }
    /// <summary>
    /// Logs observer messages to the log file.
    /// </summary>
    public class FileLogger : IUpdate
    {
        private readonly string filePath;
        public FileLogger(string filePath) => this.filePath = filePath;

        public void Update(string message)
        {
            File.AppendAllText(filePath, message + Environment.NewLine);
        }
    }
    /// <summary>
    /// Used to check how the user wishes to sort the database.
    /// </summary>
    public enum SortBy
    {
        ID,
        HighScore,
        HoursPlayed
    }
    /// <summary>
    /// Player Class for creating Player objects that are added to the database.
    /// </summary>
    public class Player
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
    /// Using Template Method pattern to define the base class for adding to the json file
    /// </summary>
    public abstract class DatabaseHandling
    {
        protected SortBy sortBy = SortBy.ID;
        private readonly List<IUpdate> observers = new List<IUpdate>();
        public void AddObserver(IUpdate observer) => observers.Add(observer);
        /// <summary>
        /// give message to observers to add to log file
        /// </summary>
        /// <param name="message"></param>
        protected void NotifyObservers(string message)
        {
            foreach (var observer in observers) observer.Update(message);
        }
        /// <summary>
        /// Defines route of execution
        /// </summary>
        public void Execute()
        {
            var players = RetrieveFromDatabase();
            Modify(players);
            Sort(players);
            SaveToDatabase(players);
            LogUpdate();
        }
        /// <summary>
        /// Allows for the list to be changed when method overridden
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        protected abstract bool Modify(List<Player> players);
        /// <summary>
        /// Allows user to set the type by which they want to sort the list by
        /// </summary>
        /// <param name="type"></param>
        public void SetSortType(SortBy type) => sortBy = type;
        /// <summary>
        /// resorts the list by sortby type.
        /// </summary>
        /// <param name="players"></param>

        protected virtual void Sort(List<Player> players)
        {
            switch (sortBy)
            {
                case SortBy.HighScore:
                    players.Sort((a, b) => b.MainScore.CompareTo(a.MainScore));
                    break;
                case SortBy.HoursPlayed:
                    players.Sort((a, b) => b.HoursPlayed.CompareTo(a.HoursPlayed));
                    break;
                case SortBy.ID:
                default:
                    players.Sort((a, b) => a.ID.CompareTo(b.ID));
                    break;
            }

        }
        /// <summary>
        /// Adds the list back to the json file
        /// </summary>
        /// <param name="players"></param>
        protected void SaveToDatabase(List<Player> players)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("players.json", JsonSerializer.Serialize(players, options));
        }
        /// <summary>
        /// Allows the user to be shown their actions and add actions to the log.
        /// </summary>
        protected abstract void LogUpdate();
        /// <summary>
        /// Returns a list of players from the json file.
        /// </summary>
        /// <returns></returns>
        protected List<Player> RetrieveFromDatabase()
        {
            return JsonSerializer.Deserialize<List<Player>>(File.ReadAllText("players.json"));
        }
    }
    /// <summary>
    /// Player search class inherriting Database Handling. 
    /// It has one constructor that allows for an input of either username, id or both.
    /// </summary>
    public class PlayerSearch : DatabaseHandling
    {
        private string Username;
        private int ID;

        public PlayerSearch(string sUsername = null, int iID = 0)
        {
            this.Username = sUsername;
            this.ID = iID;
        }
        /// <summary>
        /// No modifications made during playersearch.
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        protected override bool Modify(List<Player> players) => true;
        /// <summary>
        /// updating the log and notifying user.
        /// </summary>
        protected override void LogUpdate()
        {
            string msg = $"Searched for: {Username} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            Console.WriteLine(msg);
            NotifyObservers(msg);
        }
        public bool Matches(Player p)
        {
            return (Username != null && Username == p.Username && (ID == 0 || ID == p.ID))
                || (ID != 0 && ID == p.ID && (Username == null || Username == p.Username));

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
                if (Matches(p))
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
    public class UpdateDatabase : DatabaseHandling
    {
        private readonly string Username;
        private readonly int KillCount;
        private readonly int DeathCount;
        private readonly int MainScore;
        private readonly int HoursPlayed;
        private bool updateSucceeded;
        /// <summary>
        /// sets adding values.
        /// </summary>
        public UpdateDatabase(string sUsername, int iAddKills, int iAddDeaths, int iAddHours)
        {
            this.Username = sUsername;
            this.KillCount = iAddKills;
            this.DeathCount = iAddDeaths;
            this.HoursPlayed = iAddHours;
            
        }
        public static int CalculateMainScore(int kills, int deaths)
        {
            return deaths > 0
                ? (int)((double)kills / deaths * 100)
                : kills * 100;

        }

        /// <summary>
        /// Edits the player found's scores.
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        protected override bool Modify(List<Player> players)
        {
            Player player = players.FirstOrDefault(p => p.Username == Username);
            if (player == null)
            {
                Console.WriteLine("Player not found.");
                updateSucceeded = false;
                return false;
            }
            player.KillCount += KillCount;
            player.DeathCount += DeathCount;
            player.HoursPlayed += HoursPlayed;
            player.MainScore = CalculateMainScore(player.KillCount, player.DeathCount);

            updateSucceeded = true;
            return true;
        }
        protected override void LogUpdate()
        {
            string msg = "";
            if (updateSucceeded)
            {
                msg = $"{Username} player record updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
            else
            {
                msg = $"Unsuccessful attempt to update database at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }

            Console.WriteLine(msg);
            NotifyObservers(msg);
        }
    }
    /// <summary>
    /// Shows the top 3 players by main score or hours played.
    /// </summary>
    public class DisplayRecords : DatabaseHandling
    {
        protected override bool Modify(List<Player> players) => true;
        /// <inheritdoc/>
        protected override void LogUpdate()
        {
            string msg = $"Displayed record sorted by {sortBy}";
            Console.WriteLine(msg);
            NotifyObservers(msg);
        }
        /// <summary>
        /// Allows the user to chose what type they wish to sort by
        /// </summary>
        public void ChooseSortType()
        {
            Console.WriteLine("Sort by 1: ID, 2: HighScore, 3: HoursPlayed?");
            string input = Console.ReadLine();
            switch (input)
            {
                case "1": SetSortType(SortBy.ID); break;
                case "2": SetSortType(SortBy.HighScore); break;
                case "3": SetSortType(SortBy.HoursPlayed); break;
                default:
                    Console.WriteLine("Invalid input, defaulting to ID.");
                    SetSortType(SortBy.ID); break;
            }
        }
        /// <summary>
        /// Executes, reads the json, displays the first three players.
        /// </summary>
        /// <param name="count"></param>
        public void DisplayTop3(int count = 3)
        {
            Execute();
            var players = JsonSerializer.Deserialize<List<Player>>(File.ReadAllText("players.json"));
            for (int i = 0; i < Math.Min(count, players.Count); i++) Console.WriteLine(players[i]);
        }
    }
    /// <summary>
    /// Add Player to database using the next available ID. 
    /// </summary>
    public class AddPlayer: DatabaseHandling
    {
        private string Username;
        public AddPlayer(string sUsername) => this.Username = sUsername;
        public static int GetNextID(List<Player> players)
        {
            return players.Any() ? players.Max(p => p.ID) + 1 : 1;
        }
        /// <summary>
        /// Finds the next available ID then adds it to the new player object.
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        protected override bool Modify(List<Player> players)
        {
            int iNextID = GetNextID(players);
            if (string.IsNullOrWhiteSpace(Username))
            {
                Console.WriteLine("Username cannot be empty.");
                return false;
            }
            if (players.Any(p => p.Username == Username)|| players.Any(p => p.ID == iNextID))
            {
                Console.WriteLine("Username or ID already exists.");
                return false;
            }
            players.Add(new Player(Username,iNextID));
            return true;
        }
        /// <summary>
        /// logging that a new player has been created.
        /// </summary>
        protected override void LogUpdate()
        {
            string msg = $"{Username} added to database at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            Console.WriteLine(msg);
            NotifyObservers(msg);
        }
        }
    }
/// <summary>
/// Using the factory pattern for the creation of class objects.
/// </summary>
public static class DatabaseFactory
{
    public static AddPlayer CreateAddPlayer(string username)
    {
        return new AddPlayer(username);
    }

    public static UpdateDatabase CreateUpdateDatabase(string username, int addKills, int addDeaths, int addHours)
    {
        return new UpdateDatabase(username, addKills, addDeaths, addHours);
    }

    public static DisplayRecords CreateDisplayRecords(SortBy sortBy = SortBy.ID)
    {
        var display = new DisplayRecords();
        display.SetSortType(sortBy);
        return display;
    }

    public static PlayerSearch CreatePlayerSearch(string username = null, int id = 0)
    {
        return new PlayerSearch(username, id);
    }
}
/// <summary>
/// The user interface for the program. Displays a main menu and the user selects 
/// the option they wish to use. The following functions are then executed using the 
/// template pattern we made earlier.
/// </summary>
class UserInterface
{
    int Pointer;
    int MenuStart;
    int MenuEnd;
    /// <summary>
    /// displays the main menu
    /// </summary>
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
    /// <summary>
    /// asks for the user input and checks the input using try-catch statements.
    /// If successful, it carries out the actions taken.
    /// It uses iteration to loop if unsuccessful.
    /// </summary>
    /// <param name="MenuType"></param>
    /// <param name="iMenuStart"></param>
    /// <param name="iMenuEnd"></param>
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
    /// <summary>
    /// Validates all user inputs that are expected to be integers
    /// </summary>
    /// <param name="input"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Carries out the sequence of actions depending on the users option selection.
    /// </summary>
    public void MainMenuController()
    {
        FileLogger logger = new FileLogger("update_log.txt");

        switch (Pointer)
        {
            case 1:
                Console.WriteLine("Enter username:");
                string username = Console.ReadLine();
                var add = DatabaseFactory.CreateAddPlayer(username);
                add.AddObserver(logger);
                add.Execute();
                break;
            case 2:
                Console.WriteLine("Input username or press enter to skip");
                string sUsername = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(sUsername)) sUsername = null;

                Console.WriteLine("Input ID or press enter to skip");
                string sID = Console.ReadLine();
                int iID = string.IsNullOrWhiteSpace(sID) ? 0 : GetIntInput(sID);

                var search = DatabaseFactory.CreatePlayerSearch(sUsername, iID);
                search.AddObserver(logger);
                search.SearchForPlayer();
                break;
            case 3:
                Console.WriteLine("Enter username to update:");
                string uname = Console.ReadLine();
                Console.WriteLine("Enter kills to add:");
                int kills = GetIntInput(Console.ReadLine());
                Console.WriteLine("Enter deaths to add:");
                int deaths = GetIntInput(Console.ReadLine());
                Console.WriteLine("Enter hours to add:");
                int hours = GetIntInput(Console.ReadLine());

                var update = DatabaseFactory.CreateUpdateDatabase(uname, kills, deaths, hours);
                update.AddObserver(logger);
                update.Execute();
                break;
            case 4:
                var display = DatabaseFactory.CreateDisplayRecords();
                display.AddObserver(logger);
                display.ChooseSortType();
                display.DisplayTop3();
                break;
            case 5:
                Environment.Exit(0);
                break;

        }
        DisplayMainMenu();
    }
}
