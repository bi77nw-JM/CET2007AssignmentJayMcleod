using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    interface IObserver
    {
        void Update();
    }
    class Player
    {
        public string Username { get; set; }
        public int ID { get; set; }
        public int DeathCount;
        public int KillCount;
        public int MainScore;
        public int HoursPlayed;
        public Player (string sUsername, int iID)
        {
            Username = sUsername;

            DeathCount = 0;
            KillCount = 0;
            MainScore = 0;
            HoursPlayed = 0;
        }
        public override string ToString()
        {
            string message = "Username: " + Username + " ID: " + ID + " DeathCount: " + DeathCount + " Kill Count: " + KillCount + " Main Score: " + MainScore + " Hours Played" + HoursPlayed;
            return message;
            
        }
    }
    abstract class DatabaseHandling : IObserver
    {
        private List<Player> playerList1 = new List<Player>();
        bool PlayerAdded = false;

        public void AddToDatabase()
        {
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
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(player, options);
            File.WriteAllText("players.json", json);
            PlayerAdded = true;
        }
        public List<Player> RetreiveFromDatabase()
        {
            string jsonData = File.ReadAllText("players.json");
            List<Player> playerList2 = JsonSerializer.Deserialize<List<Player>>(jsonData);
            return playerList2;
        }
        public void Update()
        {
            Console.WriteLine("New player added to database.");
            PlayerAdded = false;
        }
    }
}
