using System;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace HCS_vk_bot
{
    class Program
    {

        public static VkApi api = new VkApi();
        static void Main(string[] args)
        {

            VKBotHelper helper = new VKBotHelper(token: "6081288192:AAFJABHQMJ4bDrYRCkCZ-xR0ZoG3SUGX-5M");
            helper.GetUpdate();





            //api.Authorize(new ApiAuthParams() { AccessToken = "vk1.a.yGb3R5HDRj8DEyIcggKoGOXq3lAnNNSB-YNDp440LyeRBrSp8oCsoWCkHnW1pvVl-aokWuR1Zkjvek3kl2zj485Aoifs07N8guYVtDPbGZxu0UCZHwr0ZRppBhWZPlU_o7TDQGgCtOwwhL0K4NxZ-j3cHxpQp6EKRqGpI7-u3P_e_J5TvkoMfwyti7NpGFpBZa-Ddf9Rsy-518Nzl3o7YA" });

            //while (true) // Бесконечный цикл, получение обновлений
            //{
            //    var s = api.Groups.GetLongPollServer(220144707);
            //    var poll = api.Groups.GetBotsLongPollHistory(
            //       new BotsLongPollHistoryParams()
            //       { Server = s.Server, Ts = s.Ts, Key = s.Key, Wait = 25 });
            //    if (poll?.Updates == null) continue; // Проверка на новые события
            //    foreach (var a in poll.Updates)
            //    {
            //        if (a.Type == GroupUpdateType.MessageNew)
            //        {
            //            KeyboardBuilder key = new KeyboardBuilder();
                        
            //            string userMessage = a.Message.Body.ToLower();
            //            long? userId = a.Message.UserId;
                         
            //            //if(a.Message.Payload != null) userMessage = a.Message.Payload.ToString();
            //            switch (userMessage)
            //            {
            //                case "hello":
                               
            //                    key.AddButton("Menu", "menu", VkNet.Enums.SafetyEnums.KeyboardButtonColor.Positive);
            //                    key.AddLine();
            //                    key.AddButton("My name is Max", "myname", VkNet.Enums.SafetyEnums.KeyboardButtonColor.Default);
            //                    SendMessage("Choose button", userId, key.Build());

            //                    break;
            //                case "menu":
            //                    SendMessage("It builded", userId, null);
            //                    break;
            //                case "myname":
            //                    SendMessage("HI!\nMy name is bot", userId, null);
            //                    break;
            //                default:
            //                    SendMessage("Sry, i dont know this command", userId, null);
            //                    break;
            //            }
                        
            //        }
            //    }
            //}
        }

        //public static void SendMessage(string message, long? userID, MessageKeyboard? keyboard)
        //{
        //    Random rnd = new Random();
        //    api.Messages.Send(new MessagesSendParams
        //    {
        //        RandomId = rnd.Next(),
        //        UserId = userID,
        //        Message = message,
        //        Keyboard = keyboard
        //    });

        //}
    }
}