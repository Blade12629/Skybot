using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.API.V1
{
    public enum WinType
    {
        Red = 1,
        Blue = 2,
        Draw = 3,
    }

    public enum TeamColor
    {
        None,
        Red,
        Blue
    }

    public enum JsonFormat
    {
        MultiMatch,
        GetPlayer,
        Get_BeatMaps
    }

    public enum ApprovedEnum
    {
        WIP = -1,
        pending = 0,
        ranked = 1,
        approved = 2,
        qualified = 3,
        loved = 4
    }

    public enum GenreEnum
    {
        any,
        unspecified,
        video_game,
        anime,
        rock,
        pop,
        other,
        novelty,
        hip_hop = 9,
        electronic = 10
    }

    public enum LanguageIDEnum
    {
        any,
        other,
        english,
        japanese,
        chinese,
        instrumental,
        korean,
        french,
        german,
        swedish,
        spanish,
        italian
    }

    public enum GameModeEnum
    {
        standard,
        taiko,
        catchthebeat,
        osumania
    }

    public enum CountryCode
    {

    }
}
