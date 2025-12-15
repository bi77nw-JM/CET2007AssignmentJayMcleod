using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ConsoleApp1;
using System.Collections.Generic;

namespace CET2007AssignmentUnitTests
{
    [TestClass]
    public class TextFixture_ConsoleApp1Tests
    {
        [TestMethod]
        public void CalculateMainScore_AKC10ADC5_MainScoreEquals200()
        {
            int score = UpdateDatabase.CalculateMainScore(10, 5);
            Assert.AreEqual(200, score);
        }
        [TestMethod]
        public void CalculateMainScore_KC10DC0_MainScoreEquals1000()
        {
            int score = UpdateDatabase.CalculateMainScore(10, 0);
            Assert.AreEqual(1000, score);
        }

        [TestMethod]
        public void PlayerSearch_Username_MatchesByUsername()
        {
            var search = new PlayerSearch("Harry");
            var player = new Player("Harry", 1);

            Assert.IsTrue(search.Matches(player));
        }

        [TestMethod]
        public void GetNextID_EmptyList_Returns1()
        {
            Assert.AreEqual(1, AddPlayer.GetNextID(new List<Player>()));
        }

        [TestMethod]
        public void GetNextID_NonEmptyList_ReturnsMaxPlusOne()
        {
            var players = new List<Player>
            {
                new Player("PlayerA",1),
                new Player("PlayerB",5)
            };
            Assert.AreEqual(6, AddPlayer.GetNextID(players));
        }

    }
}
