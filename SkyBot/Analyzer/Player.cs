using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Analyzer
{
    public class Player : IEquatable<Player>
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public HistoryJson.Score[] Scores { get; set; }

        public HistoryJson.Score HighestScore { get; set; }

        public float MVPScore { get; set; }

        public float AverageAccuracy { get; set; }
        public float AverageAccuracyRounded
        {
            get { return (float)Math.Round(AverageAccuracy, 2, MidpointRounding.AwayFromZero); }
        }

        public bool Equals(int userid)
            => UserId == userid;
        
        /// <summary>
        /// Empty player
        /// </summary>
        public Player()
        {

        }

        /// <summary>
        /// Creates the player and invokes <see cref="CalculateAverageAccuracy"/> and <see cref="GetHighestScore"/>
        /// </summary>
        /// <param name="scores"></param>
        public Player(params HistoryJson.Score[] scores)
        {
            Scores = scores;
            CalculateAverageAccuracy();
            GetHighestScore();
        }

        public void GetHighestScore()
        {
            foreach (HistoryJson.Score score in Scores)
                if (HighestScore == null || score.score.Value > HighestScore.score.Value)
                    HighestScore = score;
        }

        public void CalculateAverageAccuracy()
        {
            float AvgAcc = 0;

            Scores.ToList().ForEach(score => AvgAcc += score.accuracy.Value);
            AvgAcc /= Scores.Count();
            
            AvgAcc *= 100.0f;

            AverageAccuracy = AvgAcc;
        }

        public bool Equals(Player other)
        {
            return other != null &&
                   UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId);
        }

        public override bool Equals(object obj)
        {
            if (obj is Player pl)
                return Equals(pl);
            return false;
        }

        public static bool operator ==(Player player1, Player player2)
        {
            return EqualityComparer<Player>.Default.Equals(player1, player2);
        }

        public static bool operator !=(Player player1, Player player2)
        {
            return !(player1 == player2);
        }
    }
}
