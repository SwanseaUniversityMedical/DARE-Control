using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Rabbit
{
    public class RabbitMQSetting
    {
        public string HostAddress { get; set; } = "localhost";
        public string PortNumber { get; set; } = "5672";
        public string VirtualHost { get; set; } = "";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}
