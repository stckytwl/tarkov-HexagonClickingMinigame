﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.INI
{
    /// <summary>
    ///     Provides the best-effort line parsing strategy -
    ///     inherit from this to guide behavior, eg to set a custom comment
    ///     delimiter or custom escaping of values, bearing in mind that doing
    ///     so will probably make your ini file unreadable by other tooling
    /// </summary>
    public class BestEffortLineParser : ILineParser
    {
        /// <summary>
        ///     The default marker for the start of a comment
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public const string DEFAULT_COMMENT_DELIMITER = ";";

        private static readonly Tuple<string, string>[] EscapeMap =
        {
            Tuple.Create("\\\"", "\""), Tuple.Create("\\\\", "\\")
        };

        private static readonly HashSet<string> KnownEscapeSequences = new(
            EscapeMap.Select(o => o.Item1)
        );

        /// <summary>
        ///     The marker used by this parser for the start of a comment
        /// </summary>
        public virtual string CommentDelimiter { get; set; } =
            DEFAULT_COMMENT_DELIMITER;

        /// <summary>
        ///     Attempts to parse a string to a structure which
        ///     defines the parts of the line (key, value, comment, etc)
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public IParsedLine Parse(string line)
        {
            if (line is null) return new ParsedLine("", "", "", false);

            if (line.Trim().StartsWith(CommentDelimiter)) return new ParsedLine("", null, TrimComment(line), false);

            var parts = line.Split('=');
            var key = parts[0];
            if (parts.Length == 1)
            {
                // this is not a key-value line, probably a section heading or other
                // line; we still respect quoting & commenting, but the "data"
                // portion becomes the key
                var header = SplitCommentAndData(key);
                return new ParsedLine(
                    header.Item1,
                    null,
                    header.Item2,
                    false
                );
            }

            var data = string.Join("=", parts.Skip(1));
            var containsEscapedEntities =
                ContainsKnownEscapeSequences(data) &&
                !ContainsBackslashesWhichAreNotRecognisedEscapeSequences_(data);

            var valueAndComment = SplitCommentAndData(data, containsEscapedEntities);
            var value = Unescape(
                valueAndComment.Item1,
                containsEscapedEntities
            );

            return new ParsedLine(
                key.Trim(),
                value,
                valueAndComment.Item2,
                containsEscapedEntities
            );
        }

        private bool ContainsKnownEscapeSequences(string data)
        {
            return EscapeMap.Aggregate(
                false,
                (acc, cur) => acc || data.Contains(cur.Item1)
            );
        }

        /// <summary>
        ///     Un-escape a line
        /// </summary>
        /// <param name="data"></param>
        /// <param name="containsEscapedEntities"></param>
        /// <returns></returns>
        protected virtual string Unescape(
            string data,
            bool containsEscapedEntities)
        {
            if (data is null) return data;

            if (data.IndexOf('\\') == -1) return data;

            if (!containsEscapedEntities) return data;

            return ApplyEscapeSequences(data);
        }

        /// <summary>
        ///     Applies the escape sequences to an incoming data string
        ///     and produces a new string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected string ApplyEscapeSequences(string data)
        {
            return EscapeMap.Aggregate(
                data,
                (acc, cur) => acc.Replace(cur.Item1, cur.Item2));
        }

        private bool ContainsBackslashesWhichAreNotRecognisedEscapeSequences_(
            string data
        )
        {
            var slashPos = data.IndexOf('\\');
            while (slashPos > -1 && slashPos != data.Length - 1)
            {
                var nextChar = data[slashPos + 1];
                var sequence = $"\\{nextChar}";
                if (!KnownEscapeSequences.Contains(sequence)) return true;

                slashPos = data.IndexOf('\\', slashPos + 1);
            }

            return false;
        }

        private Tuple<string, string> SplitCommentAndData(
            string data
        )
        {
            return SplitCommentAndData(
                data,
                !ContainsBackslashesWhichAreNotRecognisedEscapeSequences_(data)
            );
        }

        // item1 is the value
        // item2 is the inline comment
        private Tuple<string, string> SplitCommentAndData(
            string data,
            bool containsEscapedEntities)
        {
            if (data.IndexOf('\\') > -1 &&
                data.StartsWith("\"") &&
                containsEscapedEntities)
                return StrictSplitCommentAndData(data);

            return LegacySplitCommentAndData(data);
        }

        private Tuple<string, string> StrictSplitCommentAndData(string data)
        {
            var wasQuoted = false;
            if (data.StartsWith("\""))
            {
                wasQuoted = true;
                data = data.Substr(1);
            }

            var parts = new Queue<string>(data.Split('"'));
            var left = new List<string>();
            while (parts.Count > 0)
            {
                left.Add(parts.Dequeue());
                if (!left.Last().EndsWith("\\")) break;
            }

            var value = string.Join("\"", left);
            if (!wasQuoted && string.IsNullOrEmpty(value))
                value = string.IsNullOrEmpty(value)
                    ? null
                    : value.Trim();

            return Tuple.Create(
                value,
                TrimComment(string.Join("\"", parts))
            );
        }

        private string TrimComment(string str)
        {
            str = str.Trim();
            return str.StartsWith(CommentDelimiter)
                ? str.Substring(1)
                : str;
        }

        private Tuple<string, string> LegacySplitCommentAndData(string data)
        {
            var semiPos = data.IndexOf(CommentDelimiter, 0, StringComparison.Ordinal);
            if (semiPos == -1)
                return Tuple.Create(
                    StripOuterMostPairedCommentsFrom(data.Trim()),
                    ""
                );

            var quoteAfterSemi = data.IndexOf('"', semiPos + 1);
            while (quoteAfterSemi > -1 && semiPos > -1)
            {
                semiPos = data.IndexOf(CommentDelimiter, semiPos + 1, StringComparison.Ordinal);
                quoteAfterSemi = data.IndexOf('"', semiPos + 1);
            }

            if (semiPos < 0) semiPos = data.Length;

            var value = data.Substr(0, semiPos);
            var comment = data.Substr(semiPos + 1);
            return Tuple.Create(
                string.IsNullOrEmpty(value)
                    ? null
                    : StripOuterMostPairedCommentsFrom(value.Trim()),
                comment
            );
        }

        private static string StripOuterMostPairedCommentsFrom(string data)
        {
            if (data.Length < 2 || data.First() != '"' || data.Last() != '"') return data;

            return data.Substring(0, data.Length - 1).Substring(1);
        }
    }

    static internal class StringExtensions
    {
        static internal string Substr(
            this string str,
            int start
        )
        {
            return str.Substr(start, str?.Length ?? 0);
        }

        static internal string Substr(
            this string str,
            int start,
            int length
        )
        {
            if (str is null ||
                start > str.Length)
                return "";

            if (start < 0) start = 0;

            if (length < 0) length = str.Length - start + length;

            if (start + length > str.Length) length = str.Length - start;

            return str.Substring(start, length);
        }
    }

    internal class ParsedLine : IParsedLine
    {
        public ParsedLine(
            string key,
            string value,
            string comment,
            bool containedEscapedEntities
        )
        {
            Key = key;
            Value = value;
            Comment = comment;
            ContainedEscapedEntities = containedEscapedEntities;
        }

        public string Key { get; }
        public string Value { get; }
        public string Comment { get; }
        public bool ContainedEscapedEntities { get; }
    }
}