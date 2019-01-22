using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace countdown
{
    class Program
    {
        private static StringHash _words = new StringHash();

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

            char[] buffer = args[0].ToLower().ToArray();
            StringHash words = new StringHash();
            FindWords(buffer, words);

            foreach (string word in words.EnumerateItems().OrderBy(x => x.Length))
            {
                Console.WriteLine(word);
            }

            timer.Stop();
            Console.WriteLine($"Elapsed time:{timer.Elapsed.ToString()}");
        }

        private static void FindWords(Span<char> buffer, StringHash words)
        {
            int[] counts = new int[buffer.Length + 1];
            for (int i = 0; i < counts.Length; ++i)
            {
                counts[i] = i;
            }

            int pos = 1;
            while (pos < buffer.Length)
            {
                FindSubWords(buffer, words);

                int pos2;
                counts[pos]--;
                if (pos % 2 == 0)
                {
                    pos2 = 0;
                }
                else
                {
                    pos2 = counts[pos];
                }

                char ch = buffer[pos];
                buffer[pos] = buffer[pos2];
                buffer[pos2] = ch;

                pos = 1;
                while (counts[pos] == 0)
                {
                    counts[pos] = pos;
                    ++pos;
                }
            }
        }

        private static void FindSubWords(Span<char> buffer, StringHash words)
        {
            for (int i = 3; i < buffer.Length; ++i)
            {
                for (int pos = 0; pos + i < buffer.Length; ++pos)
                {
                    ReadOnlySpan<char> slice = buffer.Slice(pos, i);
                    if (IsValidWord(slice))
                    {
                        words.Add(slice);
                    }
                }
            }
        }

        private static bool IsValidWord(ReadOnlySpan<char> subWord)
        {
            return _words.Contains(subWord);
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
            }
        }
    }
}
