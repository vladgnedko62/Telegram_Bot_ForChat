using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Requests;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using ts= System.Threading;
using System.Collections.Generic;
using Classes;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using Telegram.Bot.Types.Enums;

namespace Telegram_Bot_ChatClear
{
    internal class Program
    {

        static ITelegramBotClient bot = new TelegramBotClient("5349291014:AAHXpccldQk9txmYRhri2d4Bcxe0xpuiUOc");
       public static SqlConnection connection=new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TGChats;Integrated Security=true;");
        static List<Chat> creator = new List<Chat>();
        static Chat ch = new Chat() { Id = -1001616005050 };
        static List<string> commands = new List<string>();
        static bool st=false;
        static string wApi = "db868b6ad58cac2c774ca64a6592c667";
       static List<InputOnlineFile> cats=new List<InputOnlineFile>();
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
              
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    var message = update.Message;
      //              Message msg = await botClient.SendPollAsync(
      //    chatId: message.Chat,
      //    question: "Привет в мд5 это",
      //    options: new[] { "6103fff172f43f4c09ae92042367ec63", "14a17bdea7c48736f4f54a5a19cedc4a", "d3d1ae6c16afb13e5e2e421cd0ee9b9d" },
      //    isAnonymous: false,
      //    type: PollType.Regular,
      //    allowsMultipleAnswers: false
      //);
                    
                    //if (!st)
                    //{
                    //     ts.Tasks.Task.Run(() => { UserTime(botClient, ch); });
                    //}
                    
                    Console.WriteLine("Name = {0} |\tTag = {1} |\tText = {2} |\tID = {3}", message.From.FirstName, message.From.Username, message.Text, message.From.Id);
                   
                        if ((await connection.QueryFirstOrDefaultAsync<DateTime>("select Mute from Users where UserId=@p", new { p = message.From.Id })) >= DateTime.Now)
                        {
                            await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                            return;
                        }
                        if (! CheckUser(botClient, message).Result) {
                           
                            return;
                        }
                        
                        if (message.Type == Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded)
                        {
                            await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                        return;
                        }
                        else
                        {
                            if (message.Type == Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft)
                            {
                                await DeleteUserFromDB(botClient, message.From, message.Chat);
                                await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                            return;
                            }
                            else
                            {
                                if (message.Type == Telegram.Bot.Types.Enums.MessageType.MessagePinned)
                                {
                                    await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                                return;
                            }
                            }
                        }
                    
                    if (message.Type==Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        if (message.Text.ToLower().IndexOf("влад ") != -1)
                        {
                            await CheckCommandPublic(botClient, message);
                        }
                        else
                        {
                            if (message.Text != null && message.Text[0] == '/')
                            {
                                foreach (var item in creator)
                                {
                                    if (message.From.Id == item.Id)
                                    {
                                        await CheckCommand(botClient, message);
                                        break;
                                    }

                                }

                            }
                        }
                    }
                    
                    
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        public static async Task CheckCommandPublic(ITelegramBotClient client,Message message)
        {
          
            if (message.Text.ToLower().IndexOf(commands[0])!=-1)
            {
                string city = message.Text.Substring(commands[0].Length,message.Text.Length-commands[0].Length);
                WebClient web = new WebClient();
                string url = "";
                url = @"https://api.openweathermap.org/data/2.5/weather?q="+city+"&appid="+wApi+"&units=metric";
                byte[] vs = web.DownloadData(url);
                string json = Encoding.UTF8.GetString(vs);
                var WeatherNormal = JsonConvert.DeserializeObject<Rootobject>(json);
                string msg=
                "___Погода от Владоса___\n" +
                    "Город: "+WeatherNormal.name+"\n" +
                    "Температура: "+WeatherNormal.main.temp+"\n" +
                    "Минимальная: " + WeatherNormal.main.temp_min + "\n" +
                    "Максимальная: " + WeatherNormal.main.temp_max + "\n"+
                    "Небо: " + WeatherNormal.weather[0].description + "\n";

               await client.SendTextMessageAsync(message.Chat, msg);
            }
            else
            {
                if (message.Text.ToLower().IndexOf(commands[1]) != -1)
                {
                    var Dates = connection.QueryAsync<_User>("select * from Users").Result.AsList();
                    Random a = new Random();
                    int index = a.Next(0, Dates.Count);
                    bool ok = false;
                    while (true)
                    {
                        if(Dates[index].Username!=null)
                        {
                            ok = true;
                            break;
                        }
                        else
                        {
                            break;
                        }
                        index= a.Next(0, Dates.Count);
                    }
                   
                   
                    string msg = "";
                    string word= message.Text.Substring(commands[1].Length, message.Text.Length - commands[1].Length);
                    if (ok)
                    {
                        msg = "@" + Dates[index].Username + " сегодня " + word;
                    }
                    else
                    {
                        msg =Dates[index].FirstName + " сегодня " + word;
                    }
                    connection.Execute("insert into DescXD (TelegramId,Description) values(@ti,@d)", new { ti = Dates[index].UserId, d = word });
                    await client.SendTextMessageAsync(message.Chat, msg);
                }
                else
                {
                    if (message.Text.ToLower().IndexOf(commands[2]) != -1)
                    {
                        string word = message.Text.Substring(commands[2].Length, message.Text.Length - commands[2].Length);
                        Random a = new Random();
                        switch (word)
                        {
                            case "кота":await client.SendStickerAsync(message.Chat,cats[a.Next(0,15)]);break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (message.Text.ToLower().IndexOf(commands[3]) != -1)
                        {
                           string text= message.Text.Substring(commands[3].Length, message.Text.Length - commands[3].Length);
                          
                            await client.SendTextMessageAsync(message.Chat, "Зашифрован\nТип: md5\nСообщение\n" + MD5Encryptor.MD5Hash(text));
                               
                         
                        }
                        if (message.Text.ToLower().IndexOf(commands[4]) != -1)
                        {
                            string text = message.Text.Substring(commands[4].Length, message.Text.Length - commands[4].Length);
                            int i = 0;
                            int index = 0;
                            string num = "";
                            while (text[i]!=' ')
                            {
                               
                                num += text[i];
                                i++;
                            }
                            if (!int.TryParse(num,out index))
                            {
                                return;
                            }
                            text = text.Remove(0, i+1);
                            await client.SendTextMessageAsync(message.Chat, "Зашифрован\nТип: цезарь\nСообщение\n" + CesarEncryptor.CesarEncrypt(text,index));


                        }
                        else
                        {
                            if (message.Text.ToLower().IndexOf(commands[5]) != -1)
                            {
                                string text = message.Text.Substring(commands[5].Length, message.Text.Length - commands[5].Length);
                                int i = 0;
                                int index = 0;
                                string num = "";
                                while (text[i] != ' ')
                                {

                                    num += text[i];
                                    i++;
                                }
                                if (!int.TryParse(num, out index))
                                {
                                    return;
                                }
                                text = text.Remove(0, i + 1);
                                await client.SendTextMessageAsync(message.Chat, "Дешифрован\nТип: цезарь\nСообщение\n" + CesarEncryptor.CesarDecrypt(text, index));


                            }
                            else
                            {
                                if (message.Text.ToLower().IndexOf(commands[6]) != -1)
                                {
                                    string text = message.Text.Substring(commands[6].Length, message.Text.Length - commands[6].Length);
                                   

                                    Message msg = await client.SendPollAsync(
                                     chatId: message.Chat,
                                    question: "Дорогие и не дорогие друзья кто гуляет "+text,
                                     options: new[] { "Я", "Я не буду бо огурец", "Возможно невозможное" },
                                     isAnonymous: false,
                                     type: PollType.Regular,
                                     allowsMultipleAnswers: false
       );
                                }
                                else
                                {
                                    if (message.Text.ToLower().IndexOf(commands[7]) != -1)
                                    {
                                        _User user = connection.QueryFirstOrDefaultAsync<_User>("select * from Users where UserId=@ui", new { ui = message.From.Id }).Result;
                                        List<DescXD> descript = connection.QueryAsync<DescXD>("select * from DescXD where TelegramId=@ui", new { ui = message.From.Id }).Result.AsList();
                                        string msg = user.FirstName + " ты: ";
                                        foreach (var item in descript)
                                        {
                                            msg += item.Description + ", ";
                                        }
                                        await client.SendTextMessageAsync(message.Chat, msg);
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
        public static async Task CheckCommand(ITelegramBotClient client,Message message)
        {
            string Command = "";
            string WhoSet = "";
            long Id = 0;
            if (message.Text.IndexOf(@"/ban")!=-1&&message.Text!="")
            {
                await DeleteUser(client, new User() { Id = message.ReplyToMessage.From.Id }, message.Chat);
            }
            else
            {
                if (message.Text== @"/dauny@Vip_Vlad_bot")
                {
                    
                    var Dates = connection.QueryAsync<_User>("select FirstName,Username from Users").Result.AsList();
                    string mess = "";
                    int MessId = message.ReplyToMessage.MessageId;
                    foreach (var item in Dates)
                    {
                        await client.SendTextMessageAsync(message.Chat, "@"+item.Username+" - голосуй или иди нахуй <3",replyToMessageId:MessId);
                        Thread.Sleep(1000);
                    }
                    
                }
                else
                {
                    if (message.Text == @"/inf@Vip_Vlad_bot")
                    {
                        var Dates = connection.QueryAsync<_User>("select FirstName,CountMsg from Users").Result.AsList();
                        Dates.Sort();
                        string mess = "";
                        foreach (var item in Dates)
                        {
                            mess +=item.FirstName + " - " + item.CountMsg+ " сообщений \n";
                        }
                        await client.SendTextMessageAsync(message.Chat, mess);
                    }
                    else
                    {
                        if (message.Text == @"/pin@Vip_Vlad_bot")
                        {
                           await client.PinChatMessageAsync(message.Chat, message.ReplyToMessage.MessageId);
                        }
                        else
                        {
                            if (message.Text== @"/mute15m@Vip_Vlad_bot")
                            {
                                await connection.ExecuteAsync("update Users set Mute=@p where UserId=@p1", new { p = DateTime.Now.AddMinutes(15),
                                    p1=message.ReplyToMessage.From.Id });
                                await client.SendTextMessageAsync(message.Chat, message.ReplyToMessage.From.FirstName + " попал в мут на 15 мин");
                            }
                            else
                            {
                                if (message.Text == @"/mute1h@Vip_Vlad_bot")
                                {
                                    await connection.ExecuteAsync("update Users set Mute=@p where UserId=@p1", new
                                    {
                                        p = DateTime.Now.AddHours(1),
                                        p1 = message.ReplyToMessage.From.Id
                                    });
                                    await client.SendTextMessageAsync(message.Chat, message.ReplyToMessage.From.FirstName + " попал в мут на 1 час");
                                }
                                else
                                {
                                    if (message.Text == @"/mute1d@Vip_Vlad_bot")
                                    {
                                        await connection.ExecuteAsync("update Users set Mute=@p where UserId=@p1", new
                                        {
                                            p = DateTime.Now.AddDays(1),
                                            p1 = message.ReplyToMessage.From.Id
                                        });
                                        await client.SendTextMessageAsync(message.Chat, message.ReplyToMessage.From.FirstName + " попал в мут на 1 день допизделся");
                                    }
                                    else
                                    {
                                        if (message.Text == @"/unmute@Vip_Vlad_bot")
                                        {
                                            await connection.ExecuteAsync("update Users set Mute=@p where UserId=@p1", new
                                            {
                                                p = DateTime.Now,
                                                p1 = message.ReplyToMessage.From.Id
                                            });
                                            await client.SendTextMessageAsync(message.Chat, message.ReplyToMessage.From.FirstName + " розмучен");
                                        }
                                        else
                                        {
                                            if (message.Text == @"/clear")
                                            {
                                                await client.SendTextMessageAsync(message.Chat, "Чистка бази от даунов которие не пишут неделю");
                                                List<_User> _Users = connection.QueryAsync<_User>("select * from Users").Result.AsList();
                                                int i = 0;
                                                foreach (var item in _Users)
                                                {
                                                    if ((DateTime.Now-item.LastSendMessage).Days>5)
                                                    {
                                                        connection.Execute("delete from Users where UserId=@ui", new { ui = item.UserId });
                                                       await DeleteUser(client, new User() { Id=item.UserId}, message.Chat);
                                                        await client.SendTextMessageAsync(message.Chat, item.FirstName + " удален(а) с бази");
                                                        i++;
                                                    }
                                                }
                                                if (i==0)
                                                {
                                                    await client.SendTextMessageAsync(message.Chat, "Даунов не обнаружено, хороший день чтобы выпить)");
                                                }
                                            }
                                            else
                                            {
                                                if (message.Text == @"/spam")
                                                {
                                                    await client.SendTextMessageAsync(message.Chat, "Спам начат");
                                                    List<_User> _Users = connection.QueryAsync<_User>("select * from Users").Result.AsList();
                                                    for (int i = 0; i < 1000; i++)
                                                    {
                                                        foreach (var item in _Users)
                                                        {
                                                            await client.SendTextMessageAsync(new ChatId(item.UserId), item.FirstName + " - Привет от разраба + ты лошок\nС любовью @exeption_error)");
                                                        }
                                                    }
                                                   await client.SendTextMessageAsync(message.Chat, "Спам окончен");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
               
            }

            switch (message.Text)
            {

                default:
                    break;
            }
        }

        public static async Task<bool> CheckUser(ITelegramBotClient client,Message message)
        {
            bool Add=false;
           
            var Already = await connection.ExecuteScalarAsync("select count(Id) from Users where UserId=@p", new { p = message.From.Id });
            int alr;
            if (Already != null)
            {
                alr = (int)Already;
                if (alr <= 0)
                {
                    Add = true;
                }
                
            }
           
            
            if (Add)
            {
                //if (message.From.Username == null)
                //{
                //    await client.SendTextMessageAsync(message.Chat, "Пожалуйста настройте свой username(тег), и напишите снова\nБез него не будут приходить уведомления о гуляках");
                //    return false;
                //}

                await AddInDB(client,message,message.From);
                return true;
            }
            else
            {
                if (UpdateInformationOfUser(client, message, connection).Result)
                {
                    int Count = await connection.QueryFirstAsync<int>("select CountMsg from Users where UserId=@p", new { p = message.From.Id });
                    await connection.ExecuteAsync("update Users set LastSendMessage=@p,CountMsg=@p2 where UserId=@p1", new { p = DateTime.Now.Date, p1 = message.From.Id, p2 = Count + 1 });
                    return true;
                }
                else
                {
                    return false;
                }
                
                
            }
            return false;
            
        }
        public async static Task<bool> UpdateInformationOfUser(ITelegramBotClient client, Message message, SqlConnection sql)
        {
            if (message.From.Username == null)
            {
                return true;
                if (message.From.Id == creator[0].Id) {}
                await client.DeleteMessageAsync(message.Chat, message.MessageId);
                await client.SendTextMessageAsync(message.Chat, message.From.FirstName+"\nПожалуйста настройте свой username(тег), и напишите снова\nБез него не будут приходить уведомления о гуляках");
                return false;
            }
            
            if (sql.QueryFirstOrDefault<_User>("select * " +
             "from Users " +
             "where UserId=@p", new { p = message.From.Id }) != null)
            {
                _User user = sql.QueryFirstOrDefault<_User>("select * " +
                "from Users " +
                "where UserId=@p", new { p = message.From.Id });
                if (user.FirstName != message.From.FirstName)
                {
                    sql.Execute("update Users " +
              "Set FirstName=@fn " +
              "where UserId=@ti", new { fn = message.From.FirstName, ti = message.From.Id });
                }
                if (user.Username != message.From.Username)
                {
                    sql.Execute("update Users " +
              "Set Username=@us " +
              "where UserId=@ti ", new { us = (string)message.From.Username, ti = message.From.Id });
                }

            }
            return true;
        }
        public static async Task AddInDB(ITelegramBotClient client,Message message,User user)
        {
            await connection.ExecuteAsync("insert into Users (FirstName,Username,UserId,LastSendMessage,CountMsg,Mute)" +
                   " values(@fn,@un,@ui,@lsm,@cmsg,@mt)",
                   new {  fn = user.FirstName, un = user.Username, ui = user.Id, lsm = DateTime.Now.Date,cmsg=0,mt=DateTime.Now });
            string Name = "@";
            await client.SendTextMessageAsync(message.Chat,"@"+ message.From.Username+" - " +message.From.FirstName+ " ты добавлен(на) в базу!\nПравила:\n1.Адекват\n" +
                "2.Не нагонять на других\n" +
                "Спасибо что ты с нами :)\n\n" +
                "Главние админи\n" +
                "->@exeption_error");
        }
            public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        //public static async Task UserTime(ITelegramBotClient client, Chat chat)
        //{
        //    st = true;
        //    while (true)
        //    {
        //        await connection.OpenAsync();
        //        var Dates = connection.QueryAsync<_User>("select LastSendMessage,UserId from Users").Result.AsList();
        //        ts.Thread.Sleep(2000);
        //       connection.Close();
        //        foreach (var item in Dates)
        //        {
        //            Console.WriteLine("CHECK USERS_______ADMIN_______"+item.UserId);
        //            break;
        //        }

        //        foreach (var item in Dates)
        //        {
        //            if ((DateTime.Now - item.LastSendMessage).Days >= 3)
        //            {
        //                await DeleteUser(client, new User() { Id=item.UserId}, chat);
        //            }
        //        }
        //        ts.Thread.Sleep(60000);
        //    }
        //}
        public static async Task DeleteUser(ITelegramBotClient client,User user,Chat chat)
        {
            await client.BanChatMemberAsync(chat, user.Id);
           await connection.ExecuteAsync("DELETE FROM Users WHERE UserId=@p", new { p = user.Id });
        }
        public static async Task DeleteUserFromDB(ITelegramBotClient client, User user, Chat chat)
        {
            await client.BanChatMemberAsync(chat, user.Id);
            await connection.ExecuteAsync("DELETE FROM Users WHERE UserId=@p", new { p = user.Id });
        }

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var enc1251 = Encoding.GetEncoding(1251);

            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            System.Console.InputEncoding = enc1251;
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            creator.Add(new Chat() { Id = 1339927699 });
            commands.Add("влад погода ");
            commands.Add("влад кто сегодня ");
            commands.Add("влад покажи ");
            commands.Add("влад шифр мд5 ");
            commands.Add("влад шифр цезарь ");
            commands.Add("влад дешифр цезарь ");
            commands.Add("влад кто гуляет ");
            commands.Add("влад статистика");
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILUmJ4AwbUyNyBrQABlJVrlzZhppGfNQACgwADxxQtMUrA0HMYAk4fJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILVWJ4BFMUGjeivBPTwpPI-Clga_47AAK-EwACAtrZSMX10iMT7WkVJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILWGJ4BQUa6EaeXz9tdNUle3zQY6oIAAKGDwACE4upS0dUpoxRhuGOJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILYWJ4BgMrxPnSH2bFulKertIZq2VrAAIzAAPHFC0xpMICcibkGm4kBA"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBCAACAgIAAxkBAAILkGJ4By5LuQOxF1Ml57AdFUrX7baWAAImFwAChPHpS9Qh2ozZcAeNJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILj2J4By7Oriznimv-D1cv2yT0xuUpAAIqFQAC57zpS-7oCV2jDL4aJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILjmJ4By43EqDMgaoAAQxUnZRI_CdpXgACqBgAAgpE6EvDWinKdFqpyCQE"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILjWJ4By6b8av3yyFEAZhAHL9CHc6_AAIfEwACvgPwS2W_vlkuPs7kJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILi2J4By557LKJkRTw6t7N1tpIf2OgAAIPGAACN5joS79CT57DcATZJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILimJ4By7nYUEri6qSrHCQoIVLViKPAAKcCwACOFNJScl2zEQ1X3rfJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILiWJ4By03IE9x3fRb_PGoVnyphoA3AAL7CwAC6hhISSmJAAEf32AphSQE"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILiGJ4By0tMAAB2nSM8RUWEWhRQONW5wAC1gsAAgG3SEkOE2-aXSEqwiQE"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILh2J4By1QrySrSWF9RnnvNmeVlTjEAAKNDwACnZpJSXqBEFvB525rJAQ"));
            cats.Add(new InputOnlineFile("CAACAgIAAxkBAAILhmJ4By04QnEeRJHZopPmL3AmKXnvAAIrCwACIclJSdEjcfR3ddoDJAQ"));

            bot.StartReceiving(
        HandleUpdateAsync,
        HandleErrorAsync,
        receiverOptions,
        cancellationToken
    );
            Console.ReadKey();
        }
    }
}
