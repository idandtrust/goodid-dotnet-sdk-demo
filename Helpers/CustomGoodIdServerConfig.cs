using GoodId.Core.Helpers;
using GoodId.DemoSite.Models.Config;
using Microsoft.Extensions.Options;

namespace GoodId.DemoSite.Helpers
{
    /// <summary>
    /// This is a custom GoodId configuration class. It is only a container for configuration values.
    /// You can ovverride any config value if it necessary.
    ///
    /// See <see cref="GoodIdSdk.ServiceLocator"/> class
    /// </summary>
    public class CustomGoodIdServerConfig : GoodIdServerConfig
    {
        private readonly IOptions<GoodIdConfig> goodIdConfig;

        public CustomGoodIdServerConfig( IOptions<GoodIdConfig> config)
        {
            goodIdConfig = config;
        }
        public override string IdpUri 
        {
            get { return goodIdConfig.Value.IdpUri ?? base.IdpUri; }
        }
        public override string IssuerUri
        {
            get { return goodIdConfig.Value.IssuerUri ?? base.IssuerUri; }
        }
    }
}
