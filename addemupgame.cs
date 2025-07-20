
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class AddEmUpCardGame
{
    static void Main(string[] args)
    {
        string inputFile = "";
        string outputFile = "";

        try
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Please pass input and outfile file command line arguments.");
                return;
            }

            foreach (string arg in args)
            {
                if (arg == "--in")
                {
                    inputFile = args[Array.IndexOf(args, arg) + 1];
                }
                if (arg == "--out")
                {
                    outputFile = args[Array.IndexOf(args, arg) + 1];

                }
            }

            var fileContents = File.ReadAllLines(inputFile);

            GameProcessor process = new GameProcessor();
            process.process(fileContents, outputFile);



        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
            File.WriteAllText(outputFile, "Exception: " + e.Message);
        }

    }

    class GameProcessor
    {
        public void process(string[] fileContents, string outputFile)
        {
            try
            {
                var playersHands = CreatePlayerList(fileContents);

                Dictionary<string, Player> tiedPlayers;
                Player winnerPlayer = GetHighestScorePlayer(playersHands, out tiedPlayers);

                if (winnerPlayer == null) //tie
                {
                    Dictionary<string, Tuple<int, int>> tiedPlayersBySuitValue;
                    var finalWinner = GetFinalWinner(tiedPlayers, out tiedPlayersBySuitValue);

                    if (string.IsNullOrEmpty(finalWinner) && tiedPlayersBySuitValue.Count > 0)
                    {
                        var finalTiedPlayers = GetFinalTiedWinners(tiedPlayersBySuitValue);
                        Console.WriteLine("Winner= " + finalTiedPlayers);
                        File.WriteAllText(outputFile, finalTiedPlayers);
                    }
                    else
                    {
                        Console.WriteLine("Winner= " + finalWinner);
                        File.WriteAllText(outputFile, finalWinner);
                    }
                }
                else
                {
                    Console.WriteLine("Winner= " + winnerPlayer.playerName + ':' + winnerPlayer.score);
                    File.WriteAllText(outputFile, winnerPlayer.playerName + ':' + winnerPlayer.score);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                File.WriteAllText(outputFile, "Exception: " + e.Message);
            }
        }



        string GetFinalTiedWinners(Dictionary<string, Tuple<int, int>> tiedPlayersBySuitValue)
        {
            var names = tiedPlayersBySuitValue.Keys.ToArray();
            var outputNames = string.Join(",", names);

            var score = tiedPlayersBySuitValue[names[0]].Item1 + tiedPlayersBySuitValue[names[0]].Item2;
            var finalTiedPlayers = outputNames + ":" + score;
            return finalTiedPlayers;
        }
        string GetFinalWinner(Dictionary<string, Player> tiedPlayers, out Dictionary<string, Tuple<int, int>> tiedPlayersBySuitValue)
        {
            Player prevPlayer = null;
            string winner = "";
            var maxSuitScore = 0;
            tiedPlayersBySuitValue = new Dictionary<string, Tuple<int, int>>();

            foreach (var player in tiedPlayers.Values)
            {
                var highestHand = player.highestHand;

                var playerSuitScore = GetPlayersSuitScore(player.hands, highestHand);

                if (playerSuitScore > maxSuitScore)
                {
                    tiedPlayersBySuitValue.Clear();
                    maxSuitScore = playerSuitScore;
                    winner = player.playerName + ':' + (player.score + playerSuitScore);
                    prevPlayer = player;
                }
                else if (playerSuitScore == maxSuitScore)//suit tie
                {
                    winner = "";
                    if (!tiedPlayersBySuitValue.ContainsKey(prevPlayer.playerName))
                        tiedPlayersBySuitValue.Add(prevPlayer.playerName, new Tuple<int, int>(maxSuitScore, prevPlayer.score));
                    tiedPlayersBySuitValue.Add(player.playerName, new Tuple<int, int>(playerSuitScore, player.score));
                }


            }
            return winner;
        }

        List<Player> CreatePlayerList(string[] fileContents)
        {
            var playersHands = new List<Player>();
            foreach (string playerItem in fileContents)
            {
                if (!string.IsNullOrWhiteSpace(playerItem))
                {
                    Console.WriteLine(playerItem.Trim());
                    var eachPlayer = playerItem.Split(':');

                    //remove White spaces from eachPlayer[1] using regex.
                    var playerHands = Regex.Replace(eachPlayer[1], @"\s+", "");
                    var eachPlayerHands = playerHands.Split(',');

                    var cardValues = GetNumbersOfHands(eachPlayerHands);

                    var player = new Player()
                    {
                        playerName = eachPlayer[0].Trim(),
                        hands = eachPlayerHands,
                        handsNumValues = cardValues

                    };
                    playersHands.Add(player);
                }

            }
            return playersHands;
        }

        int GetPlayersSuitScore(string[] hands, int highestHandValue)
        {
            var suits = new Dictionary<char, int>()
                {
                    { 'D', 1 },
                    { 'H', 2 },
                    { 'S', 3 },
                    { 'C', 4 }
                };

            // int suitScore;
            foreach (var hand in hands)
            {
                //Console.WriteLine(hand);

                var faceValue = char.ToUpper(hand[0]);
                var suitKey = char.ToUpper(hand[1]);
                //Console.WriteLine(highestHandValue);
                if (faceValue == 'K' && highestHandValue == 13)
                    return suits[suitKey];
                else if (faceValue == 'Q' && highestHandValue == 12)
                    return suits[suitKey];
                else if ((faceValue == 'J' && highestHandValue == 11) || (faceValue == 'A' && highestHandValue == 11))
                    return suits[suitKey];
                else if (hand[0] == '1' && highestHandValue == 10) //10
                    return suits[char.ToUpper(hand[2])];
                else
                {
                    if (highestHandValue < 10)
                    {
                        var handValue = int.Parse(hand[0].ToString());
                        if (handValue == highestHandValue)
                        {
                            return suits[suitKey];
                        }
                    }
                }
            }
            return 0;
        }
        int[] GetNumbersOfHands(string[] hands)
        {
            int[] handNumbers = new int[hands.Length];

            for (int i = 0; i < handNumbers.Length; i++)
            {
                char faceValue = char.ToUpper(hands[i][0]);
                switch (faceValue)
                {
                    case 'J':
                    case 'A':
                        handNumbers[i] = 11;
                        break;
                    case 'Q':
                        handNumbers[i] = 12;
                        break;
                    case 'K':
                        handNumbers[i] = 13;
                        break;
                    case '1':
                        handNumbers[i] = 10;
                        break;
                    default:
                        handNumbers[i] = int.Parse(hands[i][0].ToString());
                        break;
                }

                Console.WriteLine(handNumbers[i]);
            }

            return handNumbers;
        }

        Player GetHighestScorePlayer(List<Player> players, out Dictionary<string, Player> tiedPlayers)
        {
            var previousPlayerName = "";
            int previousHighestHand = 0;
            var maxScore = 0;
            string[] previousHands = { };

            Player? winnerPlayer = null;
            tiedPlayers = new Dictionary<string, Player>();

            foreach (var player in players)
            {
                var cardNumValues = player.handsNumValues;
                Array.Sort(cardNumValues);


                var lastIndex = cardNumValues.Length - 1;
                var tempMaxScore = cardNumValues[cardNumValues.Length - 3] + cardNumValues[cardNumValues.Length - 2] + cardNumValues[lastIndex]; //array is sorted from low to high

                if (tempMaxScore > maxScore)
                {
                    tiedPlayers.Clear();
                    maxScore = tempMaxScore;
                    player.score = maxScore;
                    winnerPlayer = player;

                    Console.WriteLine("max score: " + maxScore);
                    previousPlayerName = player.playerName;
                    previousHighestHand = cardNumValues[lastIndex];
                    previousHands = player.hands;
                }
                else if (tempMaxScore == maxScore)
                {
                    winnerPlayer = null;
                    var player1 = new Player
                    {
                        playerName = previousPlayerName,
                        highestHand = previousHighestHand,
                        score = maxScore,
                        hands = previousHands

                    };

                    var player2 = new Player
                    {
                        playerName = player.playerName,
                        highestHand = cardNumValues[lastIndex],
                        score = tempMaxScore,
                        hands = player.hands

                    };

                    if (!(tiedPlayers.ContainsKey(previousPlayerName)))
                        tiedPlayers.Add(previousPlayerName, player1);

                    tiedPlayers.Add(player.playerName, player2);

                    Console.WriteLine("max score: " + maxScore);
                    Console.WriteLine("highest hand: " + cardNumValues[lastIndex]);

                }

                Console.WriteLine("previous player: " + previousPlayerName + ", prev highest hand: " + cardNumValues[lastIndex]);

            }

            return winnerPlayer;
        }

    }
    class Player
    {
        public string playerName { get; set; }
        public int highestHand { get; set; }
        public int score { get; set; }
        public string[] hands { get; set; }

        public int[] handsNumValues { get; set; }


    }


}