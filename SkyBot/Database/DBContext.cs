﻿using Microsoft.EntityFrameworkCore;
using SkyBot.Database.Models;
using SkyBot.Osu.AutoRef;
using SkyBot.Database.Models.GlobalStatistics;
using SkyBot.Database.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Text;
using SkyBot.Database.Models.Customs;
using SkyBot.Database.Models.Web;
using SkyBot.Database.Models.AutoRef;

public class DBContext : DbContext
{
    public virtual DbSet<APIUser> APIUser { get; set; }
    public virtual DbSet<BannedUser> BannedUser { get; set; }
    public virtual DbSet<ByteTable> ByteTable { get; set; }
    public virtual DbSet<User> User { get; set; }
    public virtual DbSet<Permission> Permission { get; set; }
    public virtual DbSet<DiscordRoleBind> DiscordRoleBind { get; set; }
    public virtual DbSet<DiscordGuildConfig> DiscordGuildConfig { get; set; }
    public virtual DbSet<Verification> Verification { get; set; }

    public virtual DbSet<SeasonPlayer> SeasonPlayer { get; set; }
    public virtual DbSet<SeasonResult> SeasonResult { get; set; }
    public virtual DbSet<SeasonScore> SeasonScore { get; set; }
    public virtual DbSet<SeasonBeatmap> SeasonBeatmap { get; set; }
    public virtual DbSet<SeasonPlayerCardCache> SeasonPlayerCardCache { get; set; }
    public virtual DbSet<SeasonTeamCardCache> SeasonTeamCardCache { get; set; }
    public virtual DbSet<WarmupBeatmap> WarmupBeatmaps { get; set; }
    public virtual DbSet<Reminder> Reminder { get; set; }
    public virtual DbSet<Ticket> Ticket { get; set; }
    public virtual DbSet<Mute> Mute { get; set; }
    public virtual DbSet<CommandAccess> CommandAccess { get; set; }

    public virtual DbSet<GSTeam> GSTeam { get; set; }
    public virtual DbSet<GSTeamMember> GSTeamMember { get; set; }
    public virtual DbSet<GSTournament> GSTournament { get; set; }
    public virtual DbSet<PlayerProfile> PlayerProfile { get; set; }

    public virtual DbSet<CustomScript> CustomScript { get; set; }
    public virtual DbSet<CustomCommand> CustomCommand { get; set; }

    public virtual DbSet<AnalyzerMods> AnalyzerMods { get; set; }

    public virtual DbSet<WebUser> WebUser { get; set; }

    public virtual DbSet<AutoRefConfig> AutoRefConfig { get; set; }


    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql(SkyBot.SkyBotConfig.MySQLConnectionString, builder =>
            {
                builder.EnableRetryOnFailure(25, TimeSpan.FromSeconds(2), null);

                if (SkyBot.SkyBotConfig.UseMySQLMariaDB)
                    builder.ServerVersion(new Version(10, 1, 41), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MariaDb);
            }).EnableSensitiveDataLogging();

            base.OnConfiguring(optionsBuilder);
        }
    }

    void CreateConfigModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommandAccess>(entity =>
        {
            entity.ToTable("command_access");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.TypeName)
                .HasColumnName("type_name")
                .HasColumnType("longtext");

            entity.Property(e => e.AccessLevel)
                .HasColumnName("access_level")
                .HasColumnType("int(11)");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permission");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.AccessLevel)
                .HasColumnName("access_level")
                .HasColumnType("smallint(1)");
        });

        modelBuilder.Entity<DiscordRoleBind>(entity =>
        {
            entity.ToTable("discord_role_bind");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.GuildId)
                .HasColumnName("guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.AccessLevel)
                .HasColumnName("access_level")
                .HasColumnType("smallint(1)");
        });

        modelBuilder.Entity<DiscordGuildConfig>(entity =>
        {
            entity.ToTable("discord_guild_config");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.GuildId)
                .HasColumnName("guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.AnalyzeChannelId)
                .HasColumnName("analyze_channel_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.TicketDiscordChannelId)
                .HasColumnName("ticket_discord_channel_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.CommandChannelId)
                .HasColumnName("command_channel_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.VerifiedNameAutoSet)
                .HasColumnName("verified_name_auto_set")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.VerifiedRoleId)
                .HasColumnName("verified_role_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.WelcomeMessage)
                .HasColumnName("welcome_message")
                .HasColumnType("longtext");

            entity.Property(e => e.WelcomeChannel)
                .HasColumnName("welcome_channel")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.AnalyzeWarmupMatches)
                .HasColumnName("analyze_warmup_matches")
                .HasColumnType("smallint(1)");

            entity.Property(e => e.MutedRoleId)
                .HasColumnName("muted_role_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Prefix)
                .HasColumnName("prefix")
                .HasColumnType("char(1)");

            entity.Property(e => e.Debug)
                .HasColumnName("debug")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.DebugChannel)
                .HasColumnName("debug_channel")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.BlacklistRoleId)
                .HasColumnName("blacklist_role_id")
                .HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<WarmupBeatmap>(entity =>
        {
            entity.ToTable("warmup_beatmap");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.BeatmapId)
                .HasColumnName("beatmap_id")
                .HasColumnType("bigint(20)");

        });
    }

    void CreateUserRelatedModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("ticket");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Tag)
                .HasColumnName("tag")
                .HasColumnType("smallint(6)");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("smallint(6)");

            entity.Property(e => e.Priority)
                .HasColumnName("priority")
                .HasColumnType("smallint(6)");

            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .HasColumnType("datetime");

            entity.Property(e => e.Message)
                .HasColumnName("message")
                .HasColumnType("longtext");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.ToTable("reminder");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordChannelId)
                .HasColumnName("discord_channel_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Message)
                .HasColumnName("message")
                .HasColumnType("longtext");

            entity.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.OsuUserId)
                .HasColumnName("osu_user_id")
                .HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<APIUser>(entity =>
        {
            entity.ToTable("api_user");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.APIKeyMD5)
                .HasColumnName("api_key_md5")
                .HasColumnType("longtext");

            entity.Property(e => e.IsValid)
                .HasColumnName("is_valid")
                .HasColumnType("tinyint(1)");

        });

        modelBuilder.Entity<Verification>(entity =>
        {
            entity.ToTable("verification");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.VerificationCode)
                .HasColumnName("verification_code")
                .HasColumnType("longtext");

            entity.Property(e => e.InvalidatesAt)
                .HasColumnName("invalidates_at")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<BannedUser>(entity =>
        {
            entity.ToTable("banned_user");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.OsuUserId)
                .HasColumnName("osu_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Reason)
                .HasColumnName("reason")
                .HasColumnType("longtext");
        });

        modelBuilder.Entity<Mute>(entity =>
        {
            entity.ToTable("mute");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.StartTime)
                .HasColumnName("start_time")
                .HasColumnType("datetime");

            entity.Property(e => e.DurationM)
                .HasColumnName("duration_m")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Reason)
                .HasColumnName("reason")
                .HasColumnType("longtext");
        });
    }

    void CreateSeasonModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SeasonPlayer>(entity =>
        {
            entity.ToTable("season_player");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.OsuUserId)
                .HasColumnName("osu_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.LastOsuUsername)
                .HasColumnName("last_osu_username")
                .HasColumnType("text");

            entity.Property(e => e.TeamName)
                .HasColumnName("team_name")
                .HasColumnType("text");
        });

        modelBuilder.Entity<SeasonResult>(entity =>
        {
            entity.ToTable("season_result");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.MatchId)
                .HasColumnName("match_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Stage)
                .HasColumnName("stage")
                .HasColumnType("text");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.MatchName)
                .HasColumnName("match_name")
                .HasColumnType("text");

            entity.Property(e => e.WinningTeam)
                .HasColumnName("winning_team")
                .HasColumnType("text");

            entity.Property(e => e.WinningTeamWins)
                .HasColumnName("winning_team_wins")
                .HasColumnType("int(11)");

            entity.Property(e => e.WinningTeamColor)
                .HasColumnName("winning_team_color")
                .HasColumnType("int(11)");

            entity.Property(e => e.LosingTeam)
                .HasColumnName("losing_team")
                .HasColumnType("text");

            entity.Property(e => e.LosingTeamWins)
                .HasColumnName("losing_team_wins")
                .HasColumnType("int(11)");

            entity.Property(e => e.TimeStamp)
                .HasColumnName("time_stamp")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SeasonScore>(entity =>
        {
            entity.ToTable("season_score");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(11)");

            entity.Property(e => e.BeatmapId)
                .HasColumnName("beatmap_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.SeasonPlayerId)
                .HasColumnName("season_player_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.SeasonResultId)
                .HasColumnName("season_result_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.TeamName)
                .HasColumnName("team_name")
                .HasColumnType("text");

            entity.Property(e => e.TeamVs)
                .HasColumnName("team_vs")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.PlayOrder)
                .HasColumnName("play_order")
                .HasColumnType("int(11)");

            entity.Property(e => e.GeneralPerformanceScore)
                .HasColumnName("general_performance_score")
                .HasColumnType("double");

            entity.Property(e => e.HighestGeneralPerformanceScore)
                .HasColumnName("highest_general_performance_score")
                .HasColumnType("tinyint(1)");

            entity.Property(e => e.Accuracy)
                .HasColumnName("accuracy")
                .HasColumnType("float");

            entity.Property(e => e.Score)
                .HasColumnName("score")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.MaxCombo)
                .HasColumnName("max_combo")
                .HasColumnType("int(11)");

            entity.Property(e => e.Perfect)
                .HasColumnName("perfect")
                .HasColumnType("int(11)");

            entity.Property(e => e.PlayedAt)
                .HasColumnName("played_at")
                .HasColumnType("datetime");

            entity.Property(e => e.Pass)
                .HasColumnName("pass")
                .HasColumnType("int(11)");

            entity.Property(e => e.Count50)
                .HasColumnName("count_50")
                .HasColumnType("int(11)");

            entity.Property(e => e.Count100)
                .HasColumnName("count_100")
                .HasColumnType("int(11)");

            entity.Property(e => e.Count300)
                .HasColumnName("count_300")
                .HasColumnType("int(11)");

            entity.Property(e => e.CountGeki)
                .HasColumnName("count_geki")
                .HasColumnType("int(11)");

            entity.Property(e => e.CountKatu)
                .HasColumnName("count_katu")
                .HasColumnType("int(11)");

            entity.Property(e => e.CountMiss)
                .HasColumnName("count_miss")
                .HasColumnType("int(11)");
        });

        modelBuilder.Entity<SeasonBeatmap>(entity =>
        {
            entity.ToTable("season_beatmap");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.BeatmapId)
                .HasColumnName("beatmap_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Author)
                .HasColumnName("author")
                .HasColumnType("text");

            entity.Property(e => e.Difficulty)
                .HasColumnName("difficulty")
                .HasColumnType("text");

            entity.Property(e => e.DifficultyRating)
                .HasColumnName("difficulty_rating")
                .HasColumnType("double");

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasColumnType("text");
        });

        modelBuilder.Entity<SeasonPlayerCardCache>(entity =>
        {
            entity.ToTable("season_player_card_cache");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.OsuUserId)
                .HasColumnName("osu_user_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Username)
                .HasColumnName("username")
                .HasColumnType("text");

            entity.Property(e => e.TeamName)
                .HasColumnName("team_name")
                .HasColumnType("text");

            entity.Property(e => e.AverageAccuracy)
                .HasColumnName("average_accuracy")
                .HasColumnType("double");

            entity.Property(e => e.AverageCombo)
                .HasColumnName("average_combo")
                .HasColumnType("double");

            entity.Property(e => e.AverageMisses)
                .HasColumnName("average_misses")
                .HasColumnType("double");

            entity.Property(e => e.AverageScore)
                .HasColumnName("average_score")
                .HasColumnType("double");

            entity.Property(e => e.AveragePerformance)
                .HasColumnName("average_performance")
                .HasColumnType("double");

            entity.Property(e => e.OverallRating)
                .HasColumnName("overall_rating")
                .HasColumnType("double");

            entity.Property(e => e.MatchMvps)
                .HasColumnName("match_mvps")
                .HasColumnType("int(11)");

            entity.Property(e => e.LastUpdated)
                .HasColumnName("last_updated")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SeasonTeamCardCache>(entity =>
        {
            entity.ToTable("season_team_card_cache");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.TeamName)
                .HasColumnName("team_name")
                .HasColumnType("longtext");

            entity.Property(e => e.MVPName)
                .HasColumnName("mvp_name")
                .HasColumnType("longtext");

            entity.Property(e => e.TotalMatchMVPs)
                .HasColumnName("total_match_mvps")
                .HasColumnType("int(11)");

            entity.Property(e => e.AverageOverallRating)
                .HasColumnName("average_overall_rating")
                .HasColumnType("double");

            entity.Property(e => e.AverageGeneralPerformanceScore)
                .HasColumnName("average_general_performance_score")
                .HasColumnType("double");

            entity.Property(e => e.AverageAccuracy)
                .HasColumnName("average_accuracy")
                .HasColumnType("double");

            entity.Property(e => e.AverageScore)
                .HasColumnName("average_score")
                .HasColumnType("double");

            entity.Property(e => e.AverageMisses)
                .HasColumnName("average_misses")
                .HasColumnType("double");

            entity.Property(e => e.AverageCombo)
                .HasColumnName("average_combo")
                .HasColumnType("double");

            entity.Property(e => e.TeamRating)
                .HasColumnName("team_rating")
                .HasColumnType("double");

            entity.Property(e => e.LastUpdated)
                .HasColumnName("last_updated")
                .HasColumnType("datetime");
        });

    }

    void CreatePlayerProfileModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GSTeam>(entity =>
        {
            entity.ToTable("gs_team");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.GSTournamentId)
                .HasColumnName("gs_tournament_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Placement)
                .HasColumnName("placement")
                .HasColumnType("text");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("longtext");
        });

        modelBuilder.Entity<GSTeamMember>(entity =>
        {
            entity.ToTable("gs_team_member");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.GSTeamId)
                .HasColumnName("gs_team_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.OsuUserId)
                .HasColumnName("osu_user_id")
                .HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<GSTournament>(entity =>
        {
            entity.ToTable("gs_tournament");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.HostOsuId)
                .HasColumnName("host_osu_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("longtext");

            entity.Property(e => e.Acronym)
                .HasColumnName("acronym")
                .HasColumnType("text");

            entity.Property(e => e.Thread)
                .HasColumnName("thread")
                .HasColumnType("longtext");

            entity.Property(e => e.CountryCode)
                .HasColumnName("country_code")
                .HasColumnType("longtext");

            entity.Property(e => e.Start)
                .HasColumnName("start")
                .HasColumnType("datetime");

            entity.Property(e => e.End)
                .HasColumnName("end")
                .HasColumnType("datetime");

            entity.Property(e => e.RankMin)
                .HasColumnName("rank_min")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.RankMax)
                .HasColumnName("rank_max")
                .HasColumnType("bigint(20)");
        });

        modelBuilder.Entity<PlayerProfile>(entity =>
        {
            entity.ToTable("player_profile");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Username)
                .HasColumnName("username")
                .HasColumnType("longtext");

            entity.Property(e => e.OsuId)
                .HasColumnName("osu_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.BWSRank)
                .HasColumnName("bws_rank")
                .HasColumnType("double");

            entity.Property(e => e.TournamentWins)
                .HasColumnName("tournament_wins")
                .HasColumnType("int(11)");

            entity.Property(e => e.LastPlacement)
                .HasColumnName("last_placement")
                .HasColumnType("text");

            entity.Property(e => e.TournamentsPlayed)
                .HasColumnName("tournaments_played")
                .HasColumnType("int(11)");

            entity.Property(e => e.BadgeCount)
                .HasColumnName("badge_count")
                .HasColumnType("int(11)");

            entity.Property(e => e.LastUpdated)
                .HasColumnName("last_updated")
                .HasColumnType("datetime");
        });
    }

    void CreateCustomModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomCommand>(entity =>
        {
            entity.ToTable("custom_command");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.ScriptId)
                .HasColumnName("script_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.IsEnabled)
                .HasColumnName("is_enabled")
                .HasColumnType("tinyint(1)");


            entity.Property(e => e.AccessLevel)
                .HasColumnName("access_level")
                .HasColumnType("int(11)");

            entity.Property(e => e.Command)
                .HasColumnName("command")
                .HasColumnType("longtext");

            entity.Property(e => e.MinParameter)
                .HasColumnName("min_parameter")
                .HasColumnType("int(11)");

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasColumnType("longtext");

            entity.Property(e => e.Usage)
                .HasColumnName("usage")
                .HasColumnType("longtext");
        });

        modelBuilder.Entity<CustomScript>(entity =>
        {
            entity.ToTable("custom_script");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Script)
                .HasColumnName("script")
                .HasColumnType("longtext");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.LastEditedBy)
                .HasColumnName("last_edited_by")
                .HasColumnType("bigint(20)");
        });
    }

    void CreateWebModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebUser>(entity =>
        {
            entity.ToTable("web_user");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");
            
            entity.Property(e => e.DiscordUserId)
                .HasColumnName("discord_user_id")
                .HasColumnType("bigint(20)");
            
            entity.Property(e => e.AllowGlobalStats)
                .HasColumnName("allow_global_stats")
                .HasColumnType("tinyint(1)");
        });
    }

    void CreateAutoRefModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AutoRefConfig>(entity =>
        {
            entity.ToTable("auto_ref_config");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordNotifyChannelId)
                .HasColumnName("discord_notify_channel_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Key)
                .HasColumnName("key")
                .HasColumnType("longtext");

            entity.Property(e => e.TeamMode)
                .HasColumnName("team_mode")
                .HasColumnType("int(11)");

            entity.Property(e => e.WinCondition)
                .HasColumnName("win_condition")
                .HasColumnType("int(11)");

            entity.Property(e => e.BestOf)
                .HasColumnName("best_of")
                .HasColumnType("int(11)");

            entity.Property(e => e.TotalWarmups)
                .HasColumnName("total_warmups")
                .HasColumnType("int(11)");

            entity.Property(e => e.PlayersPerTeam)
                .HasColumnName("players_per_team")
                .HasColumnType("int(11)");

            entity.Property(e => e.Script0)
                .HasColumnName("script_0")
                .HasColumnType("longtext");
            
            entity.Property(e => e.Script1)
                .HasColumnName("script_1")
                .HasColumnType("longtext");
            
            entity.Property(e => e.Script2)
                .HasColumnName("script_2")
                .HasColumnType("longtext");
            
            entity.Property(e => e.Script3)
                .HasColumnName("script_3")
                .HasColumnType("longtext");

            entity.Property(e => e.CurrentScript)
                .HasColumnName("current_script")
                .HasColumnType("int(11)");
        });
    }

    void CreateAnalyzerModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalyzerMods>(entity =>
        {
            entity.ToTable("analyzer_mods");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.DiscordGuildId)
                .HasColumnName("discord_guild_id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.NF)
                .HasColumnName("nf")
                .HasColumnType("double");

            entity.Property(e => e.EZ)
                .HasColumnName("ez")
                .HasColumnType("double");

            entity.Property(e => e.HT)
                .HasColumnName("ht")
                .HasColumnType("double");

            entity.Property(e => e.HR)
                .HasColumnName("hr")
                .HasColumnType("double");

            entity.Property(e => e.DTNC)
                .HasColumnName("dt_nc")
                .HasColumnType("double");

            entity.Property(e => e.HD)
                .HasColumnName("hd")
                .HasColumnType("double");

            entity.Property(e => e.FL)
                .HasColumnName("fl")
                .HasColumnType("double");

            entity.Property(e => e.RLX)
                .HasColumnName("rlx")
                .HasColumnType("double");

            entity.Property(e => e.AP)
                .HasColumnName("ap")
                .HasColumnType("double");
        });
    }

    void CreateMiscModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ByteTable>(entity =>
        {
            entity.ToTable("byte_table");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("bigint(20)");

            entity.Property(e => e.Identifier)
                .HasColumnName("identifier")
                .HasColumnType("longtext");

            entity.Property(e => e.Data)
                .HasColumnName("data")
                .HasColumnType("longblob");
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        CreateConfigModels(modelBuilder);
        CreateUserRelatedModels(modelBuilder);
        CreateSeasonModels(modelBuilder);
        CreatePlayerProfileModels(modelBuilder);
        CreateCustomModels(modelBuilder);
        CreateWebModels(modelBuilder);
        CreateAnalyzerModels(modelBuilder);
        CreateAutoRefModels(modelBuilder);
        CreateMiscModels(modelBuilder);
    }

    public void CreateDefaultTables()
    {
        string createScript = Database.GenerateCreateScript();
        Database.ExecuteSqlRaw(createScript);
        SaveChanges();
    }
}
