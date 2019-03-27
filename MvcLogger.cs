using System;
using Microsoft.Extensions.Logging;
using GoodId.Core.AbstractClasses;
using GoodId.Core.Exceptions;

namespace GoodId.DemoSite
{
    public class MvcLogger : Logger
    {
        readonly ILogger mLogger;

        public MvcLogger(ILogger logger)
        {
            mLogger = logger;
        }

        protected override void LogImpl(Level level, string str)
        {
            switch (level)
            {
                case Level.INFO:
                    mLogger.LogInformation(str);
                    break;
                case Level.WARNING:
                    mLogger.LogWarning(str);
                    break;
                case Level.ERROR:
                    mLogger.LogError(str);
                    break;
                default:
                    throw new GoodIdException("Unsupported log level " + level);
            }
        }
    }
}
