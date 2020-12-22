using System;
using System.Collections.Generic;
using System.Text;

namespace Bot
{
    public class Day
    {
        public string Name { get; set; }

        public string Message
        {
            get => _message;

            set => _message = value;

        }
        public string _message;
    }
}
