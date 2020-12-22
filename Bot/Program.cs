using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{

    static class Program
    {
        private static EasyManager _manager;

        private static readonly List<Day> _list = new List<Day>();

        private static string TimeTable => "timetable.json";

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            if (args.Length == 7)
            {
                for (var i = 0; i < args.Length; i++)
                    _list.Add(new Day
                    {
                        Name = i switch
                        {
                            0 => "понедельник",
                            1 => "вторник",
                            2 => "среда",
                            3 => "четверг",
                            4 => "пятница",
                            5 => "суббота",
                            6 => "воскресенье"
                        }
                    ,
                        Message = args[i]
                    });
                var sb = new StringBuilder();
                foreach (var day in _list)
                {
                    sb.Append(JsonConvert.SerializeObject(day) + "\n");
                }
                using (var sw = new StreamWriter(new FileStream(TimeTable, FileMode.Create, FileAccess.Write)))
                    sw.Write(sb.ToString());
            }
            else if (args.Length == 1)
            {
                if (File.Exists(TimeTable))
                {
                    var timetables = File.ReadAllText(TimeTable).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string timetable in timetables)
                        _list.Add(JsonConvert.DeserializeObject<Day>(timetable));
                    _manager = new EasyManager(args[0]);
                    if (_manager.IsAuthorized)
                    {
                        _manager.Listen();
                        _manager.GetMessage += Answer;
                        var cancellationTokenSource = new CancellationTokenSource();
                        AppDomain.CurrentDomain.ProcessExit += (s, e) => cancellationTokenSource.Cancel();
                        Console.CancelKeyPress += (s, e) => cancellationTokenSource.Cancel();
                        await Task.Delay(-1, cancellationTokenSource.Token).ContinueWith(t => { });
                    }
                    else
                    {
                        Console.WriteLine("Неправильный токен");
                    }
                }
                else Console.WriteLine("Нет файла расписания");
            }
            else
                Console.WriteLine("Неправильное количество аргументов");

        }

        private static string Analyze(string line)
        {
            // Логика.

            var result = line.ToLower() switch
            {
                "понедельник" => _list[0].Message,
                "вторник" => _list[1].Message,
                "среда" => _list[2].Message,
                "четверг" => _list[3].Message,
                "пятница" => _list[4].Message,
                "суббота" => _list[5].Message,
                "воскресенье" => _list[6].Message,
                "сегодня" => _list[GetDayNum(DateTime.Now)].Message,
                "завтра" => _list[GetDayNum(DateTime.Now.AddDays(1))].Message,
                _ => "Нет такого дня"
            };
            return result.Replace("\\n", "\n");
        }

        static void Answer(ConversationAndLastMessage conversation)
        {
            string message = conversation.LastMessage.Text;
            if (conversation.Conversation.Peer.Type == ConversationPeerType.User)
            {
                string answer = Analyze(message);

                _manager.SendMessageToUser(new Message
                {
                    Text = answer,
                    UserId = conversation.LastMessage.PeerId
                });

            }
        }

        static int GetDayNum(DateTime time) => time.DayOfWeek switch
        {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            DayOfWeek.Sunday => 6,
        };


    }
}
