﻿// PsHandler - poker software helping tool.
// Copyright (C) 2014  kampiuceris

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Shapes;

namespace PsHandler.Import
{
    // SeatMax

    public enum TableSize
    {
        Default,
        Max1,
        Max2,
        Max3,
        Max4,
        Max5,
        Max6,
        Max7,
        Max8,
        Max9,
        Max10,
    }

    // Level

    public class Level
    {
        public decimal Ante;
        public decimal SmallBlind;
        public decimal BigBlind;
        public bool IsAnteDefined;
    }

    // Player

    public class Player
    {
        public string Name;
        public decimal Stack;
        public decimal Bet;

        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", Name, Stack, Bet);
        }
    }

    // Hand

    public class Hand
    {
        public long HandNumber;
        public long TournamentNumber;
        public DateTime Timestamp;
        public TimeZone TimeZone;
        public DateTime TimestampUtc;
        public Player[] Players;
        public Player[] PlayersAfterHand;
        public TableSize TableSize;
        public short ButtonSeat;
        public Level Level;

        private readonly List<Player> _players = new List<Player>();
        private bool _smallBlindCollectPots;

        public static List<Hand> Parse(string text, out int importErrors)
        {
            List<Hand> hands = new List<Hand>();

            string[] handsText = text.Split(new[] { "PokerStars Ha" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> handHistories = new List<string>();
            for (int i = 0; i < handsText.Length; i++)
            {
                if (handsText[i].StartsWith("nd #"))
                {
                    handHistories.Add("PokerStars Ha" + handsText[i]);
                }
            }

            importErrors = 0;

            foreach (string ht in handHistories)
            {
                try
                {
                    string[] lines = ht.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    Hand hand = new Hand();

                    foreach (string line in lines)
                    {
                        AnalyzeLine(line, ref hand);
                    }

                    // finalize hand
                    hand.Players = hand._players.ToArray();
                    hand.PlayersAfterHand = hand.Players.Where(o => o.Stack > 0).ToArray();

                    hands.Add(hand);
                }
                catch
                {
                    importErrors++;
                }
            }

            return hands;
        }

        public override string ToString()
        {
            return string.Format("Hand: {0}, [{1}], Players: {2}", HandNumber, Timestamp, Players.Length);
        }

        //

        public static Regex RegexHeader = new Regex(@"\APokerStars Hand #(?<hand_id>\d+): Tournament #(?<tournament_id>\d+),.+ Level (?<level_number>(I|V|X|L|C|D|M))+ \((?<level_sb>\d+)\/(?<level_bb>\d+)\) - (?<year>\d\d\d\d).(?<month>\d\d).(?<day>\d\d) (?<hour>\d{1,2}):(?<minute>\d{1,2}):(?<second>\d{1,2}) (?<timezone>.+) \[(?<year_et>\d\d\d\d).(?<month_et>\d\d).(?<day_et>\d\d) (?<hour_et>\d{1,2}):(?<minute_et>\d{1,2}):(?<second_et>\d{1,2}) (?<timezone_et>.+)\]\z");
        public static Regex RegexSeatMaxButton = new Regex(@"\ATable '\d+ \d+' (?<table_size>\d+)-max Seat #(?<button_seat>\d+) is the button");
        public static Regex RegexSeat = new Regex(@"Seat \d{1,2}: (?<name>.+) \((?<stack>\d+) in chips\)");
        public static Regex RegexAnte = new Regex(@"(?<name>.+): posts the ante (?<amount>\d+)");
        public static Regex RegexSmallBlind = new Regex(@"(?<name>.+): posts small blind (?<amount>\d+)");
        public static Regex RegexBigBlind = new Regex(@"(?<name>.+): posts big blind (?<amount>\d+)");
        public static Regex RegexFlop = new Regex(@"\*\*\* FLOP \*\*\*");
        public static Regex RegexTurn = new Regex(@"\*\*\* TURN \*\*\*");
        public static Regex RegexRiver = new Regex(@"\*\*\* RIVER \*\*\*");
        public static Regex RegexBets = new Regex(@"(?<name>.+): bets (?<amount>\d+)");
        public static Regex RegexCalls = new Regex(@"(?<name>.+): calls (?<amount>\d+)");
        public static Regex RegexRaises = new Regex(@"(?<name>.+): raises (?<amount>\d+) to (?<amount_total>\d+)");
        public static Regex RegexUncalledAmount = new Regex(@"Uncalled bet \((?<amount>\d+)\) returned to (?<name>.+)");
        public static Regex RegexCollectedFromPot = new Regex(@"(?<name>.+) collected (?<amount>\d+) from.+pot");

        private static void AnalyzeLine(string line, ref Hand hand)
        {
            Match match;

            match = RegexHeader.Match(line);
            if (match.Success)
            {
                hand.HandNumber = long.Parse(match.Groups["hand_id"].Value);
                hand.TournamentNumber = long.Parse(match.Groups["tournament_id"].Value);
                int year = int.Parse(match.Groups["year_et"].Value);
                int month = int.Parse(match.Groups["month_et"].Value);
                int day = int.Parse(match.Groups["day_et"].Value);
                int hour = int.Parse(match.Groups["hour_et"].Value);
                int minute = int.Parse(match.Groups["minute_et"].Value);
                int second = int.Parse(match.Groups["second_et"].Value);
                hand.TimeZone = TimeZones.AllTimeZones.First(o => o.Code.Equals(match.Groups["timezone_et"].Value));
                hand.Timestamp = new DateTime(year, month, day, hour, minute, second);
                hand.TimestampUtc = hand.Timestamp - hand.TimeZone.TimeDifference;
                hand.Level = new Level
                {
                    SmallBlind = int.Parse(match.Groups["level_sb"].Value),
                    BigBlind = int.Parse(match.Groups["level_bb"].Value),
                    IsAnteDefined = false,
                };
                return;
            }

            match = RegexSeatMaxButton.Match(line);
            if (match.Success)
            {
                hand.TableSize = (TableSize)int.Parse(match.Groups["table_size"].Value);
                hand.ButtonSeat = short.Parse(match.Groups["button_seat"].Value);
                return;
            }

            match = RegexSeat.Match(line);
            if (match.Success)
            {
                hand._players.Add(new Player { Name = match.Groups["name"].Value, Stack = decimal.Parse(match.Groups["stack"].Value) });
                return;
            }

            match = RegexAnte.Match(line);
            if (match.Success)
            {
                Player player = hand._players.First(p => p.Name.Equals(match.Groups["name"].Value));
                decimal amount = decimal.Parse(match.Groups["amount"].Value);
                player.Stack -= amount;
                player.Bet += amount;
                return;
            }

            match = RegexSmallBlind.Match(line);
            if (match.Success)
            {
                hand.CollectBets();
                hand._smallBlindCollectPots = true;
                Player player = hand._players.First(p => p.Name.Equals(match.Groups["name"].Value));
                decimal amount = decimal.Parse(match.Groups["amount"].Value);
                player.Stack -= amount;
                player.Bet += amount;
                return;
            }

            match = RegexBigBlind.Match(line);
            if (match.Success)
            {
                if (!hand._smallBlindCollectPots) hand.CollectBets();
                Player player = hand._players.First(p => p.Name.Equals(match.Groups["name"].Value));
                decimal amount = decimal.Parse(match.Groups["amount"].Value);
                player.Stack -= amount;
                player.Bet += amount;
                return;
            }

            match = RegexFlop.Match(line);
            if (match.Success)
            {
                hand.CollectBets();
                return;
            }

            match = RegexTurn.Match(line);
            if (match.Success)
            {
                hand.CollectBets();
                return;
            }

            match = RegexRiver.Match(line);
            if (match.Success)
            {
                hand.CollectBets();
                return;
            }

            match = RegexBets.Match(line);
            if (match.Success)
            {
                Player player = hand._players.First(p => p.Name.Equals(match.Groups["name"].Value));
                decimal amount = decimal.Parse(match.Groups["amount"].Value);
                player.Stack -= amount;
                player.Bet += amount;
                return;
            }

            match = RegexCalls.Match(line);
            if (match.Success)
            {
                Player player = hand._players.First(p => p.Name.Equals(match.Groups["name"].Value));
                decimal amount = decimal.Parse(match.Groups["amount"].Value);
                player.Stack -= amount;
                player.Bet += amount;
                return;
            }

            match = RegexRaises.Match(line);
            if (match.Success)
            {
                Player player = hand._players.First(p => p.Name.Equals(match.Groups["name"].Value));
                //decimal amount = decimal.Parse(match.Groups["amount"].Value);
                decimal amountTotal = decimal.Parse(match.Groups["amount_total"].Value);

                decimal amountReal = amountTotal - player.Bet;
                player.Bet += amountReal;
                player.Stack -= amountReal;
                return;
            }


            match = RegexUncalledAmount.Match(line);
            if (match.Success)
            {
                Player player = hand._players.First(p => p.Name.Equals(match.Groups["name"].Value));
                decimal amount = decimal.Parse(match.Groups["amount"].Value);
                player.Stack += amount;
                player.Bet -= amount;
                return;
            }

            match = RegexCollectedFromPot.Match(line);
            if (match.Success)
            {
                Player player = hand._players.First(p => p.Name.Equals(match.Groups["name"].Value));
                decimal amount = decimal.Parse(match.Groups["amount"].Value);
                player.Stack += amount;
                return;
            }
        }

        public void CollectBets()
        {
            foreach (var player in _players)
                player.Bet = 0;
        }
    }
}
