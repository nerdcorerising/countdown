using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace countdown
{
    class Program
    {
        private static List<string> _words = new List<string>();

        static void Main(string[] args)
        {
            if (args.Length != 1 || args[0].Length != 9)
            {
                Console.WriteLine("Provide the 9 letter scramble!");
                return;
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();
            LoadWordList();

            List<char> chars = args[0].ToLower().ToList();
            List<string> potentialWords = GeneratePermuation(chars);
            HashSet<string> foundWords = new HashSet<string>();
            foreach (string word in potentialWords)
            {
                List<string> subWords = GenerateSubWords(word);
                foreach (string subWord in subWords)
                {
                    if (IsValidWord(subWord))
                    {
                        foundWords.Add(subWord);
                    }
                }
            }

            foreach (string word in foundWords.OrderByDescending(x => x.Length))
            {
                Console.WriteLine($"{word}");
            }

            timer.Stop();
            Console.WriteLine($"Elapsed time:{timer.Elapsed.ToString()}");
        }

        private static bool IsValidWord(ReadOnlySpan<char> subWord)
        {
            int start = 0;
            int end = _words.Count;

            while (start < end)
            {
                int mid = (start + end) / 2;

                int result = MemoryExtensions.CompareTo(subWord, _words[mid], StringComparison.Ordinal);
                if (result == 0)
                {
                    return true;
                }
                else if (result > 0)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid - 1;
                }
            }

            return false;
        }

        private static List<string> GenerateSubWords(string word)
        {
            List<string> subWords = new List<string>();
            for (int i = 3; i < word.Length; ++i)
            {
                for (int pos = 0; pos + i < word.Length; ++pos)
                {
                    string subWord = word.Substring(pos, i);
                    subWords.Add(subWord);
                }
            }

            return subWords;
        }

        private static List<string> GeneratePermuation(List<char> chars)
        {
            List<string> permutations = new List<string>();
            if (chars.Count == 1)
            {
                permutations.Add(new String(chars.ToArray()));
            }
            else if (chars.Count > 1)
            {
                char ch;
                for (int i = 0; i < chars.Count; ++i)
                {
                    ch = chars.ElementAt(0);
                    chars.RemoveAt(0);
                    List<string> subPermutations = GeneratePermuation(chars);
                    foreach (string sub in subPermutations)
                    {
                        string str = ch + sub;
                        permutations.Add(str);
                    }
                    chars.Add(ch);
                }
            }

            return permutations;
        }

        static void LoadWordList()
        {
            using (StreamReader input = new StreamReader(new FileStream("english_words.txt", FileMode.Open)))
            {
                string line;
                while ((line = input.ReadLine()) != null)
                {
                    _words.Add(line.ToLower());
                }

                _words.OrderBy(x => x);
            }
        }
    }
}
