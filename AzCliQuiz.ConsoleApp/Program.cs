using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzCliQuiz.ConsoleApp
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            client.BaseAddress = new Uri("https://docs.microsoft.com/en-us/cli/azure/");
            var result = await client.GetAsync("reference-index?view=azure-cli-latest");
            var html = await result.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode
                .SelectNodes("//table")
                .Descendants("tr")
                .Where(tr => tr.Elements("td").Count() > 1)
                .Select(tr => tr.Elements("td")
                    .Select(td => td.InnerText.Trim())
                    .ToList())
                .ToList();

            var azCliCommands = new Dictionary<string, string>();
            foreach (var command in nodes)
            {
                if (!string.IsNullOrEmpty(command.ElementAt(0)) && !string.IsNullOrEmpty(command.ElementAt(1)))
                {
                    azCliCommands.Add(command.ElementAt(0), command.ElementAt(1));
                }
            }

            Console.WriteLine("    #############################################");
            Console.WriteLine("    #                                           #");
            Console.WriteLine("    #       AZ CLI QUIZ made by Melissa C.      #");
            Console.WriteLine("    #                                           #");
            Console.WriteLine("    #############################################");

            var questionNumber = 1;
            var correctAnswers = 0;
            Console.WriteLine("\nHow many questions do you want to have?");
            var maxNumberOfQuestions = Convert.ToInt32(Console.ReadLine());
            while (questionNumber <= maxNumberOfQuestions)
            {
                Console.WriteLine($"\nQuestion {questionNumber}/{maxNumberOfQuestions}:");
                var isAnswerCorrect = NextQuestion(azCliCommands);
                if (isAnswerCorrect)
                {
                    correctAnswers++;
                }
                questionNumber++;
                Console.WriteLine("-----------------------------------------------------");
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Score: {Math.Round((decimal)correctAnswers/maxNumberOfQuestions*100, 2)}% ({correctAnswers} out of {maxNumberOfQuestions} correct)");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("\nThanks for playing this quiz!");
            Console.WriteLine("Press enter to quit...");
            Console.ReadLine();
        }

        private static int GetRandomIndex(int indexLength)
        {
            var random = new Random();
            return random.Next(0, indexLength - 1);
        }

        private static bool NextQuestion(Dictionary<string, string> azCliCommands)
        {
            var question = azCliCommands.ElementAt(GetRandomIndex(azCliCommands.Count));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("What is the az cli command to: " + question.Value);
            Console.ForegroundColor = ConsoleColor.White;

            var retryCount = 0;
            while (!ValidateAnswer(question))
            {
                if (retryCount < 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Wrong, please try again...");
                    Console.ForegroundColor = ConsoleColor.White;
                    retryCount++;
                }
                else if (retryCount == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Wrong, please try again...");
                    Console.WriteLine("Hint: the command start with '" + question.Key.Substring(0, 4));
                    retryCount++;
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Wrong again");
                    Console.WriteLine("The correct answer is: " + question.Key);
                    Console.ForegroundColor = ConsoleColor.White;
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateAnswer(KeyValuePair<string, string> question)
        {
            var answer = Console.ReadLine();
            if (answer.ToLower() == question.Key.ToLower())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("CORRECT");
                Console.ForegroundColor = ConsoleColor.White;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
