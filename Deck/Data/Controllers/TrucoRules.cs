using System;
using System.Collections.Generic;
using System.Linq;
using truco_teca.Deck.Data.Models;

namespace truco_teca.Deck.Data.Controllers
{
    public static class TrucoRules
    {
        private static Dictionary<(int, CardSuit), int> hierarchy = new();
        private static CardSuit? dominantSuit = null;
        private static int? viraValue = null;

        public static void SetVira(Card vira)
        {
            dominantSuit = vira.Suit;
            viraValue = vira.Value;
            BuildHierarchy();
        }
        public static Card? GetVira()
        {
            if (dominantSuit == null || viraValue == null)
                return null;

            return new Card(viraValue.Value, dominantSuit.Value);
        }
        public static void Reset()
        {
            hierarchy.Clear();
            dominantSuit = null;
            viraValue = null;
        }

        private static void BuildHierarchy()
        {
            hierarchy.Clear();
            hierarchy[(1, CardSuit.Stick)] = 14; 
            hierarchy[(1, CardSuit.Sword)] = 13; 
            hierarchy[(7, CardSuit.Sword)] = 12;
            hierarchy[(7, CardSuit.Gold)] = 11; 
            var rankTable = new Dictionary<int, int>
            {
                { 3, 10 },
                { 2, 9 },
                { 1, 8 },   
                { 12, 7 },
                { 11, 6 },
                { 10, 5 },
                { 7, 4 },  
                { 6, 3 },
                { 5, 2 },
                { 4, 1 }
            };
            foreach (var (value, rank) in rankTable)
            {
                foreach (CardSuit suit in Enum.GetValues<CardSuit>())
                {
                    if (!hierarchy.ContainsKey((value, suit)))
                        hierarchy[(value, suit)] = rank;
                }
            }
            if (dominantSuit != null && viraValue != null)
            {
                int vira = viraValue.Value;
                int card15 = (vira == 10) ? 12 : (vira == 11 ? 10 : 10);
                int card16 = (vira == 10) ? 11 : (vira == 11 ? 12 : 11);
                hierarchy[(card15, dominantSuit.Value)] = 15;
                hierarchy[(card16, dominantSuit.Value)] = 16;
            }

        }
        public static int GetCardRank(Card card)
        {
            return hierarchy.TryGetValue((card.Value, card.Suit), out int rank) ? rank : 0;
        }
        public static int DetermineRoundWinner(Dictionary<Player, Card> playedCards)
        {
            if (playedCards.Count == 0)
                return 0;
            var teamGroups = playedCards.GroupBy(p => p.Key.Team)
                                        .Select(g => new
                                        {
                                            Team = g.Key,
                                            BestCard = g.OrderByDescending(c => GetCardRank(c.Value)).First()
                                        })
                                        .ToList();
            if (teamGroups.Count < 2)
                return teamGroups[0].Team;
            int team1Rank = GetCardRank(teamGroups[0].BestCard.Value);
            int team2Rank = GetCardRank(teamGroups[1].BestCard.Value);

            if (team1Rank == team2Rank)
                return 0; 
            return team1Rank > team2Rank ? teamGroups[0].Team : teamGroups[1].Team;
        }
    }
}
