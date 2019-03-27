using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodId.DemoSite.Models.Config
{
    public class GoodIdConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string DefaultClaims { get; set; }
        public string SigPrivKeyPem { get; set; }
        public string EncPrivKeyPem { get; set; }
        public string IdpUri { get; set; }
        public string IssuerUri { get; set; }

    }
}
