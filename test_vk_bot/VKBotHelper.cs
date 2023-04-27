using VkNet.Model.RequestParams;
using VkNet.Model;
using VkNet;
using VkNet.Model.GroupUpdate;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;

namespace HCS_vk_bot
{
    internal class VKBotHelper
    {
        public string _token { get; set; }


        private const string TEXT_CONSULT = "Получить консультацию";
        private const string TEXT_REQ = "Заявка";

        private const string TEXT_MY_REQ = "Мои заявки";
        private const string TEXT_LEAVE_REQ = "Сделать заявку";

        private const string TEXT_BACK = "Назад";

        UserRequest userRequets;

        public static VkApi api = new VkApi();
        private Dictionary<long?, UserState> _clientState = new Dictionary<long?, UserState>();

        private List<long?> admins_id = new List<long?> { 617811876 };


        public VKBotHelper(string token)
        {
            _token = token;
        }

        internal void GetUpdate()
        {

            api.Authorize(new ApiAuthParams() { AccessToken = "vk1.a.yGb3R5HDRj8DEyIcggKoGOXq3lAnNNSB-YNDp440LyeRBrSp8oCsoWCkHnW1pvVl-aokWuR1Zkjvek3kl2zj485Aoifs07N8guYVtDPbGZxu0UCZHwr0ZRppBhWZPlU_o7TDQGgCtOwwhL0K4NxZ-j3cHxpQp6EKRqGpI7-u3P_e_J5TvkoMfwyti7NpGFpBZa-Ddf9Rsy-518Nzl3o7YA" });
            while (true) // Бесконечный цикл, получение обновлений
            {
                try
                {
                    var s = api.Groups.GetLongPollServerAsync(220144707).Result;
                    var poll = api.Groups.GetBotsLongPollHistoryAsync(
                       new BotsLongPollHistoryParams()
                       { Server = s.Server, Ts = s.Ts, Key = s.Key, Wait = 25 });
                    if (poll.Result?.Updates == null) continue; // Проверка на новые события
                    foreach (var update in poll.Result.Updates)
                    {
                        processUpdate(update);
                        //offset = update.Id + 1;
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }
                Thread.Sleep(100);
            }
        }

        private void processUpdate(GroupUpdate update)
        {
            long? userId = update.Message.UserId;
            if (update.Type == GroupUpdateType.MessageNew)
            {
                string userMessage = update.Message.Body;
                

                var state = _clientState.ContainsKey(userId) ? _clientState[userId] : null;
                if (update.Message.Payload != null) userMessage = update.Message.Payload.ToString();
                if (state != null)
                {
                    switch (state.State)
                    {
                        //Registering user
                        case State.RegisterUser:
                            _clientState[userId] = new UserState { State = State.RegisterUserRepeat };
                            SendMessage(userId, $"Ваш номер телефона:\n{userMessage}");
                            SendMessage(userId, $"Все верно?", replyMarkup: GetYesNoButtons());
                            break;
                        case State.RegisterUserRepeat:
                            if (userMessage == "Да")
                            {
                                RegisterUser(userMessage);
                                SendMessage(userId, "Регистрация прошла успешно!", replyMarkup: GetButtons());
                                _clientState[userId] = null;
                            }
                            else if (userMessage == "Нет")
                            {
                                SendMessage(userId, "Повторите попытку.");
                                SendMessage(userId, "Введите номер телефона в формате 79*********");
                                _clientState[userId] = new UserState { State = State.RegisterUser };
                            }
                            else
                            {
                                SendMessage(userId, "Повторите команду");
                            }
                            break;
                        //Choose Consult
                        case State.ChooseConsult:
                            if (userMessage == TEXT_BACK)
                            {
                                SendMessage(userId, "Выберите:", replyMarkup: GetButtons());
                                _clientState[userId] = null;
                            }
                            else
                            {
                                switch (userMessage)
                                {
                                    case "Адрес и телефон":
                                        SendMessage(userId, "Г.Стерлитамак,ул.Ленина,12");
                                        SendMessage(userId, "📱+79999999999", replyMarkup: GetButtons());
                                        _clientState[userId] = null;
                                        break;
                                    case "Часы работы":
                                        SendMessage(userId, "Будние: с 9.00 - 19.00\nВыходные: с 10.00 - 15.00", replyMarkup: GetButtons());
                                        _clientState[userId] = null;
                                        break;
                                    case "Как зовут директора?":
                                        SendMessage(userId, "Петр Петров Петрович", replyMarkup: GetButtons());
                                        _clientState[userId] = null;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        //Users Request
                        case State.StartReq:
                            if (userMessage == TEXT_BACK)
                            {
                                SendMessage(userId, "Выберите:", replyMarkup: GetButtons());
                                _clientState[userId] = null;
                            }
                            else if (userMessage == TEXT_MY_REQ)
                            {
                                var requests = GetRequests(userId);

                                foreach (var request in requests)
                                {
                                    string answer1 = $"Фио - {request["ФИО"]}\nАдрес - {request["Адрес"]}\nНомер - {request["Номер"]}\nЧто произошло? - {request["Что произошло?"]}";
                                    SendMessage(userId, answer1, replyMarkup: GetButtons());
                                }
                                _clientState[userId] = null;
                            }
                            else if (userMessage == TEXT_LEAVE_REQ)
                            {
                                KeyboardBuilder key = new KeyboardBuilder();
                                key.Clear();
                                SendMessage(userId, "Давайте заполним заявку.\nВведите ФИО, как к Вам обращаться?", key.Build());
                                //SendMessage(userId, );
                                userRequets = new UserRequest();
                                _clientState[userId] = new UserState { State = State.ReqEnterFIO };
                            }
                            else
                            {
                                SendMessage(userId, "Повторите команду");
                            }
                            break;
                        case State.ReqEnterFIO:
                            userRequets.FIO = userMessage;
                            SendMessage(userId, "Введите адрес. Куда нужно будет подъехать?");

                            _clientState[userId] = new UserState { State = State.ReqEnterAdress };
                            break;
                        case State.ReqEnterAdress:
                            userRequets.Adress = userMessage;
                            SendMessage(userId, "Введите номер телефона, по которому можно будет созвониться.");

                            _clientState[userId] = new UserState { State = State.ReqEnterNumber };
                            break;
                        case State.ReqEnterNumber:
                            userRequets.Number = userMessage;
                            SendMessage(userId, "Напишите вкратце, что произошло. ");

                            _clientState[userId] = new UserState { State = State.ReqEnterIncident };
                            break;
                        case State.ReqEnterIncident:
                            userRequets.Icident = userMessage;
                            SendMessage(userId, "Проверьте, все ли верно введено:");

                            string answer = $"Фио - {userRequets.FIO}\nАдрес - {userRequets.Adress}\nНомер - {userRequets.Number}\nЧто произошло? - {userRequets.Icident}";
                            SendMessage(userId, answer, replyMarkup: GetYesNoButtons());
                            _clientState[userId] = new UserState { State = State.ReqEnterYesNo };
                            break;
                        //Admin panel
                        case State.StartAdmin:
                            switch (userMessage)
                            {
                                case "Рассылка":
                                    KeyboardBuilder key = new KeyboardBuilder();
                                    key.Clear();
                                    SendMessage(userId, "Введите текст, который Вы хотите разослать.", key.Build());
                                    _clientState[userId] = new UserState { State = State.StartSendList };
                                    break;
                                case "Колличество пользователей":
                                    int count_users = GetCountUsers();
                                    SendMessage(userId, $"Колличество пользователей:\n{count_users}", replyMarkup: GetAdminButtons());
                                    break;
                                case "Выход":
                                    SendMessage(userId, $"Выберите:", replyMarkup: GetButtons());
                                    _clientState[userId] = null;
                                    break;
                                //case "Нет":
                                //    key = new KeyboardBuilder();
                                //    key.Clear();
                                //    SendMessage(userId, "Введите текст, который Вы хотите разослать.");
                                //    _clientState[userId] = new UserState { State = State.StartSendList };
                                //    break;
                                default:
                                    break;
                            }
                            break;
                        case State.StartSendList:
                            SendMessage(userId, $"Ваш текст:\n{userMessage}");
                            SendMessage(userId, $"Все верно?", replyMarkup: GetYesNoButtons());
                            _clientState[userId] = new UserState { State = State.SendListYesNo };
                            break;
                        case State.SendListYesNo:
                            if (userMessage == "Да")
                            {
                                SendMessagesAllUsers();
                                SendMessage(userId, $"Сообщение успешно разослано.", replyMarkup: GetAdminButtons());
                                _clientState[userId] = new UserState { State = State.StartAdmin };
                            }
                            else
                            {
                                KeyboardBuilder key = new KeyboardBuilder();
                                key.Clear();
                                SendMessage(userId, "Введите текст, который Вы хотите разослать.", key.Build());
                                _clientState[userId] = new UserState { State = State.StartSendList };
                            }
                            break;
                        default:

                            break;
                    }
                }
                else
                {
                    switch (userMessage)
                    {
                        case "Начать":
                            if (!UserLogin(userId))
                            {
                                
                                _clientState[userId] = new UserState { State = State.RegisterUser };
                                SendMessage(userId, "Для того чтобы мной пользоваться, необходимо зарегестрироваться.");
                                SendMessage(userId, "Нужен только твой номер :)\n Введи его в формате 79*********");

                            }
                            else
                            {
                                Console.WriteLine(userId);
                                SendMessage(userId, "Добро пожаловать!\nЧем я могу помочь?", replyMarkup: GetButtons());
                                _clientState[userId] = null;

                            }
                            break;
                        case TEXT_CONSULT:
                            SendMessage(userId, "Выберите интересующий вопрос из списка:", replyMarkup: GetConsultButtons());
                            _clientState[userId] = new UserState { State = State.ChooseConsult };
                            break;
                        case TEXT_REQ:
                            SendMessage(userId, "Выберите то что Вам нужно:", replyMarkup: GetReqButtons());
                            _clientState[userId] = new UserState { State = State.StartReq };
                            break;
                        case "/admin":
                            if (admins_id.Contains(userId))
                            {
                                SendMessage(userId, "Добро пожалость в панель администратора.\nВыберите необходимое действие", replyMarkup: GetAdminButtons());
                                _clientState[userId] = new UserState { State = State.StartAdmin };
                            }
                            else
                            {
                                SendMessage(userId, "Я не знаю такой команды.\nПовтори попытку.", replyMarkup: GetButtons());
                            }
                            break;
                        default:
                            if (!UserLogin(userId))
                            {
                                //Set state RegisterUser
                                _clientState[userId] = new UserState { State = State.RegisterUser };
                                SendMessage(userId, "Для того чтобы мной пользоваться, необходимо зарегестрироваться.");
                                SendMessage(userId, "Нужен только Ваш номер :)\n Введите его в формате 79*********");
                            }
                            else
                            {
                                SendMessage(userId, "Я не знаю такой команды.\nПовтори попытку.", replyMarkup: GetButtons());
                            }

                            break;
                    }
                }


            }
            else
            {
                SendMessage(userId, "Я не знаю такой команды.\nПовтори попытку.", replyMarkup: GetButtons());
            }
        }


        private int GetCountUsers()
        {
            return -1;
        }

        private void SendMessagesAllUsers()
        {
            var a = GetUsers();
            foreach (var user in a)
            {
                Console.WriteLine(user);
            }
            Console.WriteLine("Acces!");
        }

        private List<long> GetUsers()
        {
            return new List<long> { 1610733398 };
        }

        private MessageKeyboard? GetAdminButtons()
        {
            KeyboardBuilder key = new KeyboardBuilder();
            key.AddButton("Рассылка", "sendlist", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton("Колличество пользователей", "count_users", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton("Выход", "exit", KeyboardButtonColor.Default);
            return key.Build();
        }

        private void PutRequest(UserRequest userRequets)
        {
            return;
        }

        private void RegisterUser(string userMessage)
        {
            Console.WriteLine("Регистрация прошла успешно!");
        }

        private List<Dictionary<string, string>> GetRequests(long? userId)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>{
                    {"ФИО","Петрович"},
                    {"Адрес","Стерлитамак, ул ленина, 123" },
                    {"Номер","123123" },
                    {"Что произошло?","трубы горят" }
                }
            };

            return list;
        }

        private MessageKeyboard? GetYesNoButtons()
        {
            KeyboardBuilder key = new KeyboardBuilder();
            key.AddButton("Да", "yess", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton("Нет", "no", KeyboardButtonColor.Primary);
            return key.Build();
        }

        private MessageKeyboard? GetButtons()
        {
            KeyboardBuilder key = new KeyboardBuilder();
            key.AddButton(TEXT_CONSULT, "consult", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton(TEXT_REQ, "requare", KeyboardButtonColor.Primary);
            return key.Build();
        }

        private MessageKeyboard? GetReqButtons()
        {
            KeyboardBuilder key = new KeyboardBuilder();
            key.AddButton(TEXT_MY_REQ, "my_req", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton(TEXT_LEAVE_REQ, "leave_req", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton(TEXT_BACK, "back", KeyboardButtonColor.Default);
            return key.Build();
        }

        private MessageKeyboard? GetConsultButtons()
        {
            KeyboardBuilder key = new KeyboardBuilder();
            key.AddButton("Адрес и телефон", "adress", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton("Часы работы", "o'clock", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton("Как зовут директора?", "name_boss", KeyboardButtonColor.Primary);
            key.AddLine();
            key.AddButton(TEXT_BACK, "back", KeyboardButtonColor.Default);
            return key.Build();
        }

        private bool UserLogin(long? userId)
        {
            return true;
        }

        public static void SendMessage(long? userID, string message, MessageKeyboard? replyMarkup = null)
        {
            Random rnd = new Random();
            api.Messages.SendAsync(new MessagesSendParams
            {
                RandomId = rnd.Next(),
                UserId = userID,
                Message = message,
                Keyboard = replyMarkup
            });

        }
    }
}