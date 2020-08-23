using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Discord.Data
{
    public class EmbedJson
    {
        public string Content { get; set; }
        public Embed Embed { get; set; }

        public static EmbedJson ReverseEmbed(DiscordMessage message)
        {
            if (message == null)
                return null;

            DiscordEmbed embed = message.Embeds.ElementAt(0);

            Thumbnail thumbnail = null;
            Image image = null;

            Author author = null;
            Footer footer = null;

            if (embed.Thumbnail != null)
                thumbnail = new Thumbnail()
                {
                    Url = embed.Thumbnail.Url.ToString()
                };

            if (embed.Image != null)
                image = new Image()
                {
                    Url = embed.Image.Url.ToString()
                };

            if (embed.Author != null)
                author = new Author()
                {
                    Url = embed.Author.Url.ToString(),
                    IconUrl = embed.Author.IconUrl.ToString(),
                    Name = embed.Author.Name
                };

            if (embed.Footer != null)
                footer = new Footer()
                {
                    IconUrl = embed.Footer.IconUrl?.ToString() ?? null,
                    Text = embed.Footer.Text
                };

            List<Field> fields = null;

            if (embed.Fields != null)
            {
                fields = new List<Field>();

                foreach (DiscordEmbedField f in embed.Fields)
                    fields.Add(new Field()
                    {
                        Inline = f.Inline,
                        Value = f.Value,
                        Name = f.Name
                    });
            }

            EmbedJson result = new EmbedJson()
            {
                Content = message.Content,
                Embed = new Embed()
                {
                    Author = author,
                    Thumbnail = thumbnail,
                    Image = image,
                    Footer = footer,
                    Color = embed.Color.Value,
                    Description = embed.Description,
                    Fields = fields?.ToArray() ?? null,
                    Timestamp = embed.Timestamp?.DateTime ?? DateTime.MinValue,
                    Title = embed.Title,
                    Url = embed.Url?.ToString() ?? null
                }
            };

            return result;
        }

        public DiscordEmbed BuildEmbed()
        {
            Thumbnail thumbnail = Embed.Thumbnail;
            Image image = Embed.Image;

            Author author = Embed.Author;
            Footer footer = Embed.Footer;

            DateTimeOffset? offset = null;

            if (!Embed.Timestamp.Equals(DateTime.MinValue) &&
                !Embed.Timestamp.Equals(DateTime.MaxValue))
                offset = Embed.Timestamp;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = Embed.Title,
                Description = Embed.Description,
                Url = Embed.Url,
                Color = new DiscordColor(Embed.Color),
                Timestamp = offset,
                ThumbnailUrl = thumbnail?.Url ?? null,
                ImageUrl = image?.Url ?? null,
            };

            if (footer != null)
            {
                builder.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    IconUrl = footer?.IconUrl ?? null,
                    Text = footer?.Text ?? null,
                };
            }

            if (author != null)
            {
                builder.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    IconUrl = author?.IconUrl ?? null,
                    Name = author?.Name ?? null,
                    Url = author?.Url ?? null
                };
            }

            if (Embed.Fields != null)
            {
                foreach (Field f in Embed.Fields)
                    builder.AddField(f.Name, f.Value, f.Inline);
            }

            return builder.Build();
        }
    }
}
