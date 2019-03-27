using System;
using System.Threading.Tasks;
using GoodId.DemoSite.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using GoodId.DemoSite.Models.Config;
using GoodId.Core.Helpers;
using GoodId.Core;
using GoodId.DemoSite.Models;
using GoodId.Core.Authentication;
using GoodIdSdk.Mvc.Models;
using GoodId.Core.Helpers.Key;
using GoodId.Core.Authentication.Response;
using GoodId.Core.Exceptions;
using GoodId.Core.Helpers.OpenIdRequestSources;
using GoodId.Core.Helpers.HttpResponses;

namespace GoodId.DemoSite.Controllers
{
    public class GoodIdController : Controller
    {
        static JToken sSelectedclaims;
        static Acr sSelectedAcr = Acr.LEVEL_DEFAULT;
        private ILogger mLogger;

        private readonly IOptions<GoodIdConfig> goodIdConfig;

        public GoodIdController(ILogger<GoodIdController> logger, IOptions<GoodIdConfig> config)
        {
            this.goodIdConfig = config;
            mLogger = logger;

            if(sSelectedclaims == null){
                sSelectedclaims = JToken.Parse(config.Value.DefaultClaims ?? "{\"id_token\": {\"acr\": { \"value\": \"1\" }},\"userinfo\": {\"name\": {\"essential\": true},\"email\": {\"essential\": true}}}");

            }

        }

        public IActionResult SaveData()
        {
            try
            {
                sSelectedAcr = (Acr)int.Parse(Request.Form["acr"]);
            }
            catch (Exception e)
            {
                mLogger.LogWarning($"Error type:{e.GetType()}\nError message: {e.Message}\nTrace:{e.StackTrace}");
                HttpContext.Session.SetString("error", "Error setting the ACR: " + e.Message);
            }

            try
            {
                string  claims = sSelectedclaims.ToString();
                if(string.IsNullOrEmpty(Request.Form["claims"]) == false)
                {
                    claims = Request.Form["claims"];
                }
                sSelectedclaims = JToken.Parse(claims);
            }
            catch (Exception e)
            {
                mLogger.LogWarning($"Error type:{e.GetType()}\nError message: {e.Message}\nTrace:{e.StackTrace}\nValues:{Request.Form["claims"]}");
                HttpContext.Session.SetString("error", "Error setting the Claims: " + e.Message);
            }


            var fullUrl = this.Url.Action("Index", "GoodID", null, Request.Scheme);
            return new RedirectResult(fullUrl);
        }


        public async Task<IActionResult> Index()
        {
            const string rootUri = "/";

            // If it is a GoodID login:
            if (HttpContext.Request.Query.ContainsKey("code") || HttpContext.Request.Query.ContainsKey("error"))
            {
                try
                {
                    var serviceLocator = new ServiceLocator(
                        new MvcSessionDataHandler(HttpContext.Session),
                        new MvcLogger(mLogger))
                    {
                        ServerConfig = new CustomGoodIdServerConfig(goodIdConfig)
                    };

                    var response = await new GoodIdResponseCollector(
                        serviceLocator,
                        new MvcIncomingRequest(Request),
                        goodIdConfig.Value.ClientId,
                        goodIdConfig.Value.ClientSecret,
                        RsaPrivateKey.FromPem(goodIdConfig.Value.SigPrivKeyPem),
                        new RsaPrivateKey[] { RsaPrivateKey.FromPem(goodIdConfig.Value.EncPrivKeyPem) }
                    ).CollectAsync();

                    if (response is GoodIdResponseError errorResponse)
                    {
                        ViewData["Error"] = errorResponse.Error + ": " + errorResponse.ErrorDescription;
                    }
                    else if (response is GoodIdResponseSuccess successResponse)
                    {
                        mLogger.LogDebug("There was a success response");
                        ViewData["Response"] = successResponse.DataJObject.ToString();
                    }
                }
                catch (GoodIdException e)
                {
                    ViewData["Error"] = e.ToString();
                    // Please don't display the actual error string in production.
                }
            }

            ViewData["RootUri"] = rootUri;
            ViewData["ClientId"] = goodIdConfig.Value.ClientId;
            ViewData["SelectedClaims"] = sSelectedclaims;
            ViewData["SelectedAcr"] = sSelectedAcr;
            ViewData["Claims"] = "";

            return View();
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult LoginInit()
        {
            try
            {             

                var serviceLocator = new ServiceLocator(
                    new MvcSessionDataHandler(HttpContext.Session),
                    new MvcLogger(mLogger))
                {
                    ServerConfig = new CustomGoodIdServerConfig(goodIdConfig)
                };

                var goodIdHttpResponse = GoodIdEndpointFactory.CreateGoodIDEndpoint(
                    serviceLocator,
                    new MvcIncomingRequest(Request),
                    goodIdConfig.Value.ClientId,
                    RsaPrivateKey.FromPem(goodIdConfig.Value.SigPrivKeyPem),
                    RsaPrivateKey.FromPem(goodIdConfig.Value.EncPrivKeyPem),
                    new OpenIdRequestObject(sSelectedclaims.ToString()),
                    goodIdConfig.Value.RedirectUri,
                    sSelectedAcr).Run();

                return ToActionResult(goodIdHttpResponse);
            }
            catch (GoodIdException e)
            {
                mLogger.LogError(e, "GoodIdException happened on login initiation endpoint.");
                return new StatusCodeResult(500);
            }
        }

        private IActionResult ToActionResult(GoodIdHttpResponse response)
        {
            if (response is GoodIdHttpResponseRedirect redirectResponse)
            {
                return new RedirectResult(redirectResponse.Url, false);
            }
            if (response is GoodIdHttpResponseContent contentResponse)
            {
                return new ContentResult()
                {
                    Content = contentResponse.Content,
                    ContentType = contentResponse.ContentType
                };
            }
            return new StatusCodeResult(500); // Shouldn't happen
        }
    }

}
