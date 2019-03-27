using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using GoodId.Core.AbstractClasses;

namespace GoodIdSdk.Mvc.Models
{
    public class MvcIncomingRequest : IncomingRequest
    {
        readonly Dictionary<String, String> mParameters;
        readonly Method mMethod;

        public MvcIncomingRequest(HttpRequest request)
        {
            if (request.Method.ToUpper() == "GET")
            {
                mMethod = Method.GET;
                mParameters = new Dictionary<string, string>();
                foreach (var kv in request.Query)
                {
                    mParameters[kv.Key] = kv.Value.ToString();
                }
            }
            else if (request.Method.ToUpper() == "POST")
            {
                mMethod = Method.POST;
                var stream = request.Body;
                var jsonString = new StreamReader(stream).ReadToEnd();
                mParameters = JObject.Parse(jsonString).ToObject<Dictionary<String, String>>();
            }
            else
            {
                mMethod = Method.OTHER;
                mParameters = new Dictionary<string, string>();
            }
        }

        protected override Method GetMethodImpl()
        {
            return mMethod;
        }

        protected override string GetStringParameterImpl(string name)
        {
            if (mParameters.TryGetValue(name, out string value))
            {
                return value;
            }

            return null;
        }
    }
}
