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
            if (args.Length != 1 || args[0].Length < 3)
            {
                Console.WriteLine("Provide the letter scramble at least 3 characters long!");
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
            int[] p = new int[buffer.Length + 1];
            for (int count = 0; count < p.Length; ++count)
            {
                p[count] = count;
            }

            FindSubWords(buffer, words);

            int i = 1;
            while (i < buffer.Length)
            {
                int j;
                p[i]--;
                if (i % 2 == 0)
                {
                    j = 0;
                }
                else
                {
                    j = p[i];
                }

                char ch = buffer[i];
                buffer[i] = buffer[j];
                buffer[j] = ch;

                FindSubWords(buffer, words);

                i = 1;
                while (p[i] == 0)
                {
                    p[i] = i;
                    ++i;
                }
            }
        }

        private static void FindSubWords(Span<char> buffer, StringHash words)
        {
            for (int i = 3; i <= buffer.Length; ++i)
            {
                for (int pos = 0; pos + i <= buffer.Length; ++pos)
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
