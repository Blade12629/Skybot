using SkyBot.API.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SkyBot.API.Data.GlobalStatistics
{
    public class GlobalStatsTournament : IBinaryAPISerializable
    {
        public long HostOsuId { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public string Thread { get; set; }
        public string CountryCode { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public long RankMin { get; set; }
        public long RankMax { get; set; }
        public List<GlobalStatsTeam> Teams { get; }

        public GlobalStatsTournament(long hostOsuId, string name, string acronym, 
                                     string thread, string countryCode, DateTime start, 
                                     DateTime end) 
                                     : this(hostOsuId, name, acronym, thread, 
                                            countryCode, start, end, new List<GlobalStatsTeam>())
        {
            HostOsuId = hostOsuId;
            Name = name;
            Acronym = acronym;
            Thread = thread;
            CountryCode = countryCode;
            Start = start;
            End = end;
        }

        public GlobalStatsTournament(long hostOsuId, string name, string acronym, string thread, 
                                     string countryCode, DateTime start, DateTime end, 
                                     List<GlobalStatsTeam> teams)
        {
            HostOsuId = hostOsuId;
            Name = name;
            Acronym = acronym;
            Thread = thread;
            CountryCode = countryCode;
            Start = start;
            End = end;
            Teams = teams;
        }

        public GlobalStatsTournament()
        {
            Teams = new List<GlobalStatsTeam>();
        }

        //HostOsuUserId(number) name(text) acronym(text) thread(text) country_code(text) start(date) end(date)
        //name(text) placement(number) userId1(number) userid2(number) userid3(number) etc.

        public void ToTSVFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);

            using (StreamWriter swriter = File.CreateText(file))
            {
                swriter.WriteLine($"{HostOsuId}\t{Name}\t{Acronym}\t{Thread}\t{CountryCode}\t{Start}\t{End}\t{RankMin}\t{RankMax}");

                foreach(var team in Teams)
                {
                    string line = $"{team.Name}\t{team.Placement}";

                    if (team.OsuUserIds.Count > 0)
                    {
                        StringBuilder useridsBuilder = new StringBuilder();

                        for (int i = 0; i < team.OsuUserIds.Count; i++)
                            useridsBuilder.Append($"\t{team.OsuUserIds[i]}");

                        line += useridsBuilder.ToString();
                    }

                    swriter.WriteLine(line);
                }

                swriter.Flush();
            }
        }

        public static GlobalStatsTournament FromTSVFile(string file)
        {
            if (!File.Exists(file))
                return null;

            GlobalStatsTournament tourney = new GlobalStatsTournament();

            bool teamSearch = false;
            using (StreamReader reader = new StreamReader(file))
            {
                while(!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] split = line.Split('\t');

                    if (split.Length < 4)
                    {
                        Console.WriteLine("Line too short: " + line);
                        return null;
                    }

                    if (!teamSearch)
                    {
                        tourney.HostOsuId = GetInt(split[0]);

                        tourney.Name = split[1];
                        tourney.Acronym = split[2];
                        tourney.Thread = split[3];
                        tourney.CountryCode = split[4];
                        
                        tourney.Start = GetDate(split[5]);
                        tourney.End = GetDate(split[6]);

                        tourney.RankMin = GetInt(split[7]);
                        tourney.RankMax = GetInt(split[8]);

                        if (tourney.HostOsuId == -1)
                        {
                            Console.WriteLine("Failed to read host id");
                            return null;
                        }

                        teamSearch = true;
                        continue;
                    }

                    GlobalStatsTeam team = new GlobalStatsTeam();
                    team.Placement = GetInt(split[1]);
                    team.Name = split[0];

                    for (int i = 2; i < split.Length; i++)
                        team.OsuUserIds.Add(GetLong(split[i]));

                    tourney.Teams.Add(team);
                }
            }

            return tourney;

            int GetInt(string value)
            {
                if (int.TryParse(value, out int result))
                    return result;

                return -1;
            }

            long GetLong(string value)
            {
                if (long.TryParse(value, out long result))
                    return result;

                return -1;
            }

            DateTime GetDate(string value)
            {
                string[] dateAndHourSplit = value.Split(' ');
                string[] dateSplit = dateAndHourSplit[0].Split('.');
                List<string> hourSplit = dateAndHourSplit[1].Split(':').ToList();

                if (hourSplit[hourSplit.Count - 1].Contains('.', StringComparison.CurrentCultureIgnoreCase))
                {
                    string[] msSplit = hourSplit[hourSplit.Count - 1].Split('.');
                    hourSplit.RemoveAt(hourSplit.Count - 1);

                    hourSplit.AddRange(msSplit);
                }
                else
                    hourSplit.Add("0");

                DateTime date = new DateTime(GetInt(dateSplit[0]), GetInt(dateSplit[1]), GetInt(dateSplit[2]), GetInt(hourSplit[0]), GetInt(hourSplit[1]), GetInt(hourSplit[2]), GetInt(hourSplit[3]));

                return date;
            }
        }

        public void Serialize(BinaryAPIWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(HostOsuId);
            writer.Write(Name);
            writer.Write(Acronym);
            writer.Write(Thread);
            writer.Write(CountryCode);
            writer.Write(Start);
            writer.Write(End);
            writer.Write(RankMin);
            writer.Write(RankMax);

            writer.Write(Teams.Count);
            for (int i = 0; i < Teams.Count; i++)
                Teams[i].Serialize(writer);
        }

        public void Deserialize(BinaryAPIReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            HostOsuId = reader.ReadLong();
            Name = reader.ReadString();
            Acronym = reader.ReadString();
            Thread = reader.ReadString();
            CountryCode = reader.ReadString();
            Start = reader.ReadDate();
            End = reader.ReadDate();
            RankMin = reader.ReadLong();
            RankMax = reader.ReadLong();

            int teams = reader.ReadInt();
            for (int i = 0; i < teams; i++)
            {
                GlobalStatsTeam team = new GlobalStatsTeam();
                team.Deserialize(reader);

                Teams.Add(team);
            }
        }
    }
}
