using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;

namespace Bot
{
    public class EasyManager
    {

        private readonly VkApi _api = new VkApi();

        public Action<ConversationAndLastMessage> GetMessage;

        public bool IsAuthorized => _api.IsAuthorized;

        private Thread _mThread;

        private bool _isWorking = false;

        private readonly Random _random = new Random();

        private string _accessToken;

        private int Rnd => _random.Next(int.MinValue, int.MaxValue);


        public EasyManager(string token)
        {
            _accessToken = token;
            Authorize(token);
        }

        public void Listen()
        {
            _isWorking = true;

            _mThread = new Thread(ThreadStart =>
            {
                while (_isWorking)
                {
                    try
                    {
                        var conversations = _api.Messages.GetConversations(new VkNet.Model.RequestParams.GetConversationsParams
                        {
                            Filter = GetConversationFilter.Unanswered
                        }).Items;
                        foreach (ConversationAndLastMessage conversation in conversations)
                        {
                            GetMessage(conversation);
                        }

                    }
                    catch
                    {
                        Authorize(_accessToken);
                    }


                }
            });
            _mThread.Start();
        }

        private void Authorize(string accessToken)
        {
            try
            {
                _api.Authorize(new ApiAuthParams { AccessToken = accessToken });
            }
            catch
            {
                throw new ArgumentException("Ошибка авторизации");
            }
        }

        public void SendMessageToUser(Message message)
        {
            _api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams
            {
                UserId = message.UserId,
                Message = message.Text,
                RandomId = Rnd
            });
        }

        public void Stop()
        {
            _isWorking = false;
        }
    }

}
