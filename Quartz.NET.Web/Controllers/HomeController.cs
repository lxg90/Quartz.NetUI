﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Quartz.NET.Web.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration { get; set; }
        private IMemoryCache _memoryCache { get; set; }
        public HomeController(IConfiguration configuration, IMemoryCache memoryCache)
        {
            this._configuration = configuration;
            this._memoryCache = memoryCache;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            string msg = _memoryCache.Get("msg")?.ToString();
            if (msg != null)
            {
                ViewBag.msg = msg;
                _memoryCache.Remove("msg");
            }
            return View();
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> ValidateAuthor(string token)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _memoryCache.Remove("msg");
            if (!string.IsNullOrEmpty(token) && _configuration["token"]?.ToString() == token)
            {
                ClaimsIdentity claimIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                claimIdentity.AddClaim(new Claim(ClaimTypes.AuthorizationDecision, token));
                await HttpContext.SignInAsync(
               new ClaimsPrincipal(claimIdentity),
               new AuthenticationProperties()
               {
                   ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
               });
            }
            else
            {
                _memoryCache.Set("msg", string.IsNullOrEmpty(token) ? "请填写token" : "token不正确");
            }
            return new RedirectResult("/");
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return new RedirectResult("/");
        }

        public IActionResult Guide()
        {
            return View("~/Views/Home/Guide.cshtml");
        }
        public IActionResult Help()
        {
            return View("~/Views/Home/Help.cshtml");
        }
    }
}