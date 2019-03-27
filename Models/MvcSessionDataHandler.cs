using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using GoodId.Core.AbstractClasses;

namespace GoodId.DemoSite.Models
{
    public class MvcSessionDataHandler : SessionDataHandler
    {
        const string GOODID_SESSION_PREFIX = "_GoodID_";

        readonly ISession mSession;

        public MvcSessionDataHandler(ISession session)
        {
            mSession = session;
        }

        protected override string GetVariableImpl(string key)
        {
            return mSession.GetString(GOODID_SESSION_PREFIX + key);
        }

        protected override void RemoveAllGoodIdVariablesImpl()
        {
            var keysToRemove = mSession
                .Keys
                .Where(s => s.StartsWith(GOODID_SESSION_PREFIX, StringComparison.Ordinal))
                .ToArray();

            foreach (string key in keysToRemove)
            {
                mSession.Remove(key);
            }
        }

        protected override void RemoveVariableImpl(string key)
        {
            mSession.Remove(GOODID_SESSION_PREFIX + key);
        }

        protected override void SetVariableImpl(string key, string value)
        {
            mSession.SetString(GOODID_SESSION_PREFIX + key, value);
        }
    }
}
