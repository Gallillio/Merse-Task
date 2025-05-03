using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dialogue
{
    /// <summary>
    /// Utility class for splitting text into sentences
    /// </summary>
    public class SentenceSplitter
    {
        /// <summary>
        /// Split a block of text into individual sentences
        /// </summary>
        /// <param name="text">The text to split</param>
        /// <returns>A list of sentences</returns>
        public List<string> SplitIntoSentences(string text)
        {
            var sentences = new List<string>();
            if (string.IsNullOrEmpty(text))
                return sentences;

            // Use regex to split on sentence-ending punctuation followed by whitespace
            Regex regex = new Regex(@"(?<=[.?!])\s+");
            string[] split = regex.Split(text);

            // Filter out any empty sentences
            foreach (string sentence in split)
            {
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    sentences.Add(sentence);
                }
            }

            return sentences;
        }

        /// <summary>
        /// Clean response text by removing newlines and trimming
        /// </summary>
        /// <param name="text">The text to clean</param>
        /// <returns>Cleaned text</returns>
        public string CleanResponseText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("\n", " ").Replace("\r", "").Trim();
        }
    }
}