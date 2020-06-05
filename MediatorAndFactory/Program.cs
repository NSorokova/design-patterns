namespace MediatorAndFactory
{
    using System;
    using System.Collections.Generic;

    class Program
    {

        static void Main(string[] args)
        {
            IMediator _mediator = new ChatMediator();

            _mediator.CreateChat();
                
            IFactory _userFactory = new UserFactory(_mediator);


            IChatMember userJJ = _userFactory.Create("JJ");
            IChatMember userPJ = _userFactory.Create("PJ");
            IChatMember userRJ = _userFactory.Create("RJ");

            _mediator.AddToChat(userJJ);
            _mediator.AddToChat(userPJ);
            _mediator.AddToChat(userRJ);


            userJJ.SendMessage("");
            userPJ.SendMessage("addBot");
            userJJ.SendMessage("cat");

            Console.ReadLine();
        }
    }
    public class Chat
    {
        public List<IChatMember> ChatMembers { get; set; }
        public List<Tuple<IChatMember, string>> ChatMessages { get; set; }

        public Chat()
        {
            ChatMembers = new List<IChatMember>();
            ChatMessages = new List<Tuple<IChatMember, string>>();
        }
    }

    public interface IChatMember
    {
        public string Name { get; set; }
        public void SendMessage(string Message);

        public abstract void ReceiveAddToChatNotification(object sender, ChatArgs chatArgs);
        public abstract void ReceiveRemoveFromChatNotification(object sender, ChatArgs chatArgs);
        public abstract void ReceiveAddedMessageNotification(object sender, ChatArgs chatArgs);
    }
    public class User : IChatMember
    {
        public string Name { get; set; }
        private readonly IMediator _mediator;
        public User(string name, IMediator mediator)
        {
            this.Name = name;
            _mediator = mediator;
        }

        public void SendMessage(string Message)
        {
            _mediator.SendMessage(this, Message);
        }

        //Notifications
        public void ReceiveAddedMessageNotification(object sender, ChatArgs chatArgs)
        {
            Console.WriteLine(this.Name + "- recieved that " + chatArgs.ChatMember.Name + " wrote message - " + chatArgs.Message);
        }

        public void ReceiveAddToChatNotification(object sender, ChatArgs chatArgs)
        {
            Console.WriteLine(this.Name + "- recieved that " + chatArgs.ChatMember.Name + " was added to chat");
        }

        public void ReceiveRemoveFromChatNotification(object sender, ChatArgs chatArgs)
        {
            Console.WriteLine(this.Name + "- recieved that " + chatArgs.ChatMember.Name + " was removed from chat");
        }
    }
    public class ChatBot : IChatMember
    {
        public string Name { get; set; }
        private readonly IMediator _mediator;
        public ChatBot(string name, IMediator mediator)
        {
            this.Name = name;
            _mediator = mediator;
        }

        public void SendMessage(string Message)
        {
            _mediator.SendMessage(this, Message);
        }

        public void ReceiveAddedMessageNotification(object sender, ChatArgs chatArgs)
        {
            if (chatArgs.Message == "cat")
            {
                _mediator.RemoveFromChat(chatArgs.ChatMember);
                SendMessage("Word cat is forbiden");
            }
        }

        public void ReceiveAddToChatNotification(object sender, ChatArgs chatArgs)
        {
        }

        public void ReceiveRemoveFromChatNotification(object sender, ChatArgs chatArgs)
        {
        }
    }

    ////// Factory
    public interface IFactory
    {
        public IChatMember Create(string name);
    }
    public class UserFactory : IFactory
    {
        private readonly IMediator _mediator;
        public UserFactory(IMediator mediator)
        {
            _mediator = mediator;
        }
        public IChatMember Create(string name)
        {
            return new User(name, _mediator);
        }
    }
    public class ChatBotFactory : IFactory
    {
        private readonly IMediator _mediator;
        public ChatBotFactory(IMediator mediator)
        {
            _mediator = mediator;
        }
        public IChatMember Create(string name)
        {
            return new ChatBot(name, _mediator);
        }
    }

    //////mediator
    public interface IMediator
    {
        void CreateChat();
        void AddToChat(IChatMember chatMember);
        void RemoveFromChat(IChatMember chatMember);
        void SendMessage(IChatMember from, string message);
    }

    public class ChatMediator : IMediator
    {
        private Chat chat;

        public event EventHandler<ChatArgs> AddToChatNotification = delegate { };
        public event EventHandler<ChatArgs> RemoveFromChatNotification = delegate { };
        public event EventHandler<ChatArgs> AddMessage = delegate { };

        public void CreateChat()
        {
            this.chat = new Chat();
            Console.WriteLine("Chat Created");
        }

        public void AddToChat(IChatMember chatMember)
        {
            AddToChatNotification(this, new ChatArgs(chatMember));

            chat.ChatMembers.Add(chatMember);
            AddToChatNotification += chatMember.ReceiveAddToChatNotification;
            RemoveFromChatNotification += chatMember.ReceiveRemoveFromChatNotification;
        }
        public void RemoveFromChat(IChatMember chatMember)
        {
            AddToChatNotification -= chatMember.ReceiveAddToChatNotification;
            RemoveFromChatNotification -= chatMember.ReceiveRemoveFromChatNotification;
            RemoveFromChatNotification(this, new ChatArgs(chatMember));
        }

        public void SendMessage(IChatMember from, string message)
        {

            chat.ChatMessages.Add(new Tuple<IChatMember, string>(from, message));
            if (message == "addBot")
            {
                IChatMember bot = new ChatBotFactory(this).Create("Angry Bot");
                AddMessage += bot.ReceiveAddedMessageNotification;
                this.AddToChat(bot);
                return;
            }

            AddMessage(this, new ChatArgs(from, message));

            foreach (var item in chat.ChatMembers)
                if (item != from)
                    AddMessage += item.ReceiveAddedMessageNotification;

        }
    }

    public class ChatArgs : EventArgs
    {
        public IChatMember ChatMember { get; }
        public string Message { get; set; }

        public ChatArgs(IChatMember chatMember)
        {
            ChatMember = chatMember;
        }
        public ChatArgs(IChatMember chatMember, string message)
        {
            ChatMember = chatMember;
            Message = message;
        }
    }

}

