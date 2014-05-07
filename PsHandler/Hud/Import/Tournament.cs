﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PsHandler.Hud.Import
{
    public class Tournament
    {
        public long TournamentNumber;
        public FileInfo FileInfo;
        public long LastLength;
        public List<Hand> Hands = new List<Hand>();
        private readonly object _lock = new object();

        public DateTime GetFirstHandTimestamp()
        {
            lock (_lock)
            {
                return Hands[0].Timestamp;
            }
        }

        public DateTime GetLastHandTimestamp()
        {
            lock (_lock)
            {
                return Hands[Hands.Count - 1].Timestamp;
            }
        }

        public decimal GetLatestStack(string name)
        {
            if (string.IsNullOrEmpty(name)) return decimal.MinValue;

            lock (_lock)
            {
                Player firstOrDefault = Hands[Hands.Count - 1].Players.FirstOrDefault(o => o.Name.Equals(name));
                if (firstOrDefault != null)
                {
                    return firstOrDefault.Stack;
                }
                return decimal.MinValue;
            }
        }

        //

        private void AddHands(IEnumerable<Hand> hands)
        {
            lock (_lock)
            {
                Hands.AddRange(hands);
            }
        }

        public void UpdateHands()
        {
            FileInfo = new FileInfo(FileInfo.FullName);
            if (FileInfo.Length > LastLength)
            {
                string text = Methods.ReadSeek(FileInfo.FullName, LastLength);
                LastLength = FileInfo.Length;
                AddHands(Hand.Parse(text));
            }
        }

        public override string ToString()
        {
            return string.Format("Tournament: {0}, Hands: {1}", TournamentNumber, Hands.Count);
        }
    }
}