using Jint;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;
using System.Text;
using SkyBot;
using DSharpPlus;
using DSharpPlus.Entities;
using LogLevel = SkyBot.LogLevel;
using System.Globalization;
using DiscordCommands.Scripting.Wrappers;
using SkyBot.Database.Models;

namespace DiscordCommands.Scripting
{
    public class ScriptingEngine
    {
        public ScriptingEngine()
        {
        }

        void SetupEngine(Engine engine, DiscordGuild guild, DiscordChannel channel, DiscordMember member, DiscordHandler handler, DiscordGuildConfig cfg)
        {
            #region references
            //Reference the types
            engine.SetValue("ScriptWrapper", TypeReference.CreateTypeReference(engine, typeof(ScriptWrapper)));
            engine.SetValue("ConvertWrapper", TypeReference.CreateTypeReference(engine, typeof(ConvertWrapper)));
            engine.SetValue("ConfigWrapper", TypeReference.CreateTypeReference(engine, typeof(ConfigWrapper)));

            engine.SetValue("DiscordChannel", TypeReference.CreateTypeReference(engine, typeof(DiscordChannelWrapper)));
            engine.SetValue("DiscordGuild", TypeReference.CreateTypeReference(engine, typeof(DiscordGuildWrapper)));
            engine.SetValue("DateTime", TypeReference.CreateTypeReference(engine, typeof(DateTime)));
            engine.SetValue("DateTimeOffset", TypeReference.CreateTypeReference(engine, typeof(DateTimeOffset)));
            engine.SetValue("Uri", TypeReference.CreateTypeReference(engine, typeof(Uri)));

            #region embeds
            //Embed builder references
            engine.SetValue("DiscordEmbedBuilder", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedBuilder)));
            engine.SetValue("EmbedAuthor", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedBuilder.EmbedAuthor)));
            engine.SetValue("EmbedFooter", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedBuilder.EmbedFooter)));
            engine.SetValue("DiscordEmbedField", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedField)));
            engine.SetValue("DiscordColor", TypeReference.CreateTypeReference(engine, typeof(DiscordColor)));

            //Embed references
            engine.SetValue("DiscordEmbed", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbed)));
            engine.SetValue("DiscordEmbedFooter", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedFooter)));
            engine.SetValue("DiscordEmbedImage", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedImage)));
            engine.SetValue("DiscordEmbedThumbnail", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedThumbnail)));
            engine.SetValue("DiscordEmbedVideo", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedVideo)));
            engine.SetValue("DiscordEmbedProvider", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedProvider)));
            engine.SetValue("DiscordEmbedAuthor", TypeReference.CreateTypeReference(engine, typeof(DiscordEmbedAuthor)));
            #endregion
            #endregion

            #region static objects
            //Add default objects
            engine.SetValue("Guild", new DiscordGuildWrapper(guild, handler));
            engine.SetValue("Channel", new DiscordChannelWrapper(channel, handler));
            engine.SetValue("Member", new DiscordMemberWrapper(member, handler));
            engine.SetValue("Script", new ScriptWrapper());
            engine.SetValue("Convert", new ConvertWrapper());
            engine.SetValue("Config", new ConfigWrapper(cfg));
            #endregion


            #region constructors, actions and functions
            //Add Constructors

            //Add Actions


            //Add Functions
            engine.SetValue("ToString", new Func<object, string>(o => o.ToString()));
            #endregion


        }

        public void RunScript(string script, DiscordGuild guild, DiscordChannel channel, DiscordMember member, DiscordHandler handler, DiscordGuildConfig cfg)
        {
            try
            {
                Engine engine = new Engine();
                SetupEngine(engine, guild, channel, member, handler, cfg);

                engine.Execute(script);
            }
            catch (Jint.Runtime.JavaScriptException jse)
            {
                if (jse.GetBaseException() is ScriptExitException)
                    return;

                throw jse;
            }
            catch (ScriptExitException)
            {

            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to run script: {ex}", LogLevel.Error);
            }
        }
    }


}
