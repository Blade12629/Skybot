using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Discord.Data
{
    public class EmbedJson
    {
        public string content { get; set; }
        public Embed embed { get; set; }

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
                    url = embed.Thumbnail.Url.ToString()
                };

            if (embed.Image != null)
                image = new Image()
                {
                    url = embed.Image.Url.ToString()
                };

            if (embed.Author != null)
                author = new Author()
                {
                    url = embed.Author.Url.ToString(),
                    icon_url = embed.Author.IconUrl.ToString(),
                    name = embed.Author.Name
                };

            if (embed.Footer != null)
                footer = new Footer()
                {
                    icon_url = embed.Footer.IconUrl?.ToString() ?? null,
                    text = embed.Footer.Text
                };

            List<Field> fields = null;

            if (embed.Fields != null)
            {
                fields = new List<Field>();

                foreach (DiscordEmbedField f in embed.Fields)
                    fields.Add(new Field()
                    {
                        inline = f.Inline,
                        value = f.Value,
                        name = f.Name
                    });
            }

            EmbedJson result = new EmbedJson()
            {
                content = message.Content,
                embed = new Embed()
                {
                    author = author,
                    thumbnail = thumbnail,
                    image = image,
                    footer = footer,
                    color = embed.Color.Value,
                    description = embed.Description,
                    fields = fields?.ToArray() ?? null,
                    timestamp = embed.Timestamp?.DateTime ?? DateTime.MinValue,
                    title = embed.Title,
                    url = embed.Url?.ToString() ?? null
                }
            };

            return result;
        }

        public DiscordEmbed BuildEmbed()
        {
            Thumbnail thumbnail = embed.thumbnail;
            Image image = embed.image;

            Author author = embed.author;
            Footer footer = embed.footer;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = embed.title,
                Description = embed.description,
                Url = embed.url,
                Color = new DiscordColor(embed.color),
                Timestamp = embed.timestamp,
                ThumbnailUrl = thumbnail?.url ?? null,
                ImageUrl = image?.url ?? null,
            };

            if (footer != null)
            {
                builder.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    IconUrl = footer?.icon_url ?? null,
                    Text = footer?.text ?? null,
                };
            }

            if (author != null)
            {
                builder.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    IconUrl = author?.icon_url ?? null,
                    Name = author?.name ?? null,
                    Url = author?.url ?? null
                };
            }

            if (embed.fields != null)
            {
                foreach (Field f in embed.fields)
                    builder.AddField(f.name, f.value, f?.inline ?? false);
            }

            return builder.Build();
        }
    }

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA1056 // Uri properties should not be strings
    public class Embed
    {
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public int color { get; set; }
        public DateTime timestamp { get; set; }
        public Footer footer { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Image image { get; set; }
        public Author author { get; set; }
        public Field[] fields { get; set; }
    }

    public class Footer
    {
        public string icon_url { get; set; }
        public string text { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
    }

    public class Image
    {
        public string url { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public string url { get; set; }
        public string icon_url { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public string value { get; set; }
        public bool inline { get; set; }
    }
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA1056 // Uri properties should not be strings
}
