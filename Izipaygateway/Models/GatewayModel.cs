using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Izipaygateway.Models
{
    public class GatewayModel
    {
        private const string _KEY_USER = "########";
        private const string _KEY_PASSWORD = "####################";
        private const string _KEY_JS = "###############################";
        private const string _KEY_SHA256 = "##########################";
        private const string _URL_BASE = "https://api.micuentaweb.pe/";

        public string KEY_USER => _KEY_USER;
        public string KEY_PASSWORD => _KEY_PASSWORD;
        public string KEY_JS => _KEY_JS;
        public string KEY_SHA256 => _KEY_SHA256;
        public string URL_BASE => _URL_BASE;

    }
}
