using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;


namespace BL.Models.Services
{
    public class CustomCookieEvent : CookieAuthenticationEvents
    {
        private readonly IConfiguration _config;

        public CustomCookieEvent(IConfiguration config)
        {
            _config = config;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            if (context != null)
            {
                var accessToken = context.Properties.GetTokenValue("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var tokenS = handler.ReadToken(accessToken) as JwtSecurityToken;
                    var tokenExpiryDate = tokenS.ValidTo;
                    //// If there is no valid `exp` claim then `ValidTo` returns DateTime.MinValue
                    if (tokenExpiryDate == DateTime.MinValue) throw new Exception("Could not get exp claim from token");
                    if (tokenExpiryDate < DateTime.UtcNow)
                    {
       
                        var refreshToken = context.Properties.GetTokenValue("refresh_token");

                        //check if users refresh token is still valid?
                        var tokenRefresh = handler.ReadToken(refreshToken) as JwtSecurityToken;
                        var refreshTokenExpiryDate = tokenRefresh.ValidTo;
                        if (refreshTokenExpiryDate < DateTime.UtcNow)
                        {
                         
                            //probably need to log user out
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        }
                        else
                        {
                            try
                            {
                                var tokenResponse = await new HttpClient().RequestRefreshTokenAsync(new RefreshTokenRequest
                                {
                                    Address = _config["KeyCloakSettings:Authority"] + "/protocol/openid-connect/token",
                                    ClientId = _config["KeyCloakSettings:ClientId"],
                                    ClientSecret = _config["KeyCloakSettings:ClientSecret"],
                                    RefreshToken = refreshToken
                                });
                                if (tokenResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    context.Properties.UpdateTokenValue("access_token", tokenResponse.AccessToken);
                                    context.Properties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);
                                    await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal, context.Properties);
                                }
                                else
                                {
                                    //await context.HttpContext.SignOutAsync("Cookies", context.Principal, context.Properties);
                                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //, context.Principal, context.Properties);
                                }
                            }
                            catch (Exception ex)
                            {
                                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            }
                        }
                    }
                }
            }
        }
    }
    //public class CustomCookieEvent : CookieAuthenticationEvents
    //{
    //    private readonly IConfiguration _config;      

    //    public CustomCookieEvent(IConfiguration config)
    //    {
    //        _config = config;            
    //    }

    //    public void CheckSameSite(HttpContext httpContext, CookieOptions options)
    //    {
    //        if (options.SameSite == SameSiteMode.None)
    //        {
    //            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
    //            //configure cookie policy to omit samesite=none when request is not https
    //            if (!httpContext.Request.IsHttps || DisallowsSameSiteNone(userAgent))
    //            {
    //                options.SameSite = SameSiteMode.Unspecified;
    //            }
    //        }
    //    }

    //    public bool DisallowsSameSiteNone(string userAgent)
    //    {
    //        // Check if a null or empty string has been passed in, since this
    //        // will cause further interrogation of the useragent to fail.
    //        if (String.IsNullOrWhiteSpace(userAgent))
    //            return false;

    //        // Cover all iOS based browsers here. This includes:
    //        // - Safari on iOS 12 for iPhone, iPod Touch, iPad
    //        // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
    //        // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
    //        // All of which are broken by SameSite=None, because they use the iOS networking
    //        // stack.
    //        if (userAgent.Contains("CPU iPhone OS 12") ||
    //            userAgent.Contains("iPad; CPU OS 12"))
    //        {
    //            return true;
    //        }

    //        // Cover Mac OS X based browsers that use the Mac OS networking stack. 
    //        // This includes:
    //        // - Safari on Mac OS X.
    //        // This does not include:
    //        // - Chrome on Mac OS X
    //        // Because they do not use the Mac OS networking stack.
    //        if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
    //            userAgent.Contains("Version/") && userAgent.Contains("Safari"))
    //        {
    //            return true;
    //        }

    //        // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
    //        // and none in this range require it.
    //        // Note: this covers some pre-Chromium Edge versions, 
    //        // but pre-Chromium Edge does not require SameSite=None.
    //        if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
    //        {
    //            return true;
    //        }

    //        return false;
    //    }



    //    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    //    {
    //        if (context != null)
    //        {
    //            var accessToken = context.Properties.GetTokenValue("access_token");
    //            if (!string.IsNullOrEmpty(accessToken))
    //            {
    //                var handler = new JwtSecurityTokenHandler();
    //                var tokenS = handler.ReadToken(accessToken) as JwtSecurityToken;
    //                var tokenExpiryDate = tokenS.ValidTo;
    //                //// If there is no valid `exp` claim then `ValidTo` returns DateTime.MinValue
    //                //if (tokenExpiryDate == DateTime.MinValue) throw new Exception("Could not get exp claim from token");
    //                if (tokenExpiryDate < DateTime.UtcNow)
    //                {
    //                    Log.Information("Users access Token has expired");
    //                    var refreshToken = context.Properties.GetTokenValue("refresh_token");

    //                    //check if users refresh token is still valid?
    //                    var tokenRefresh = handler.ReadToken(refreshToken) as JwtSecurityToken;
    //                    var refreshTokenExpiryDate = tokenRefresh.ValidTo;
    //                    if (refreshTokenExpiryDate < DateTime.UtcNow)
    //                    {
    //                        Log.Information("Users refresh Token has expired");
    //                        //probably need to log user out
    //                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    //                    }
    //                    else
    //                    {
    //                        try
    //                        {
    //                            var tokenResponse = await new HttpClient().RequestRefreshTokenAsync(new RefreshTokenRequest
    //                            {
    //                                Address = _config["KeyCloakSettings:Authority"] + "/protocol/openid-connect/token",
    //                                ClientId = _config["KeyCloakSettings:ClientId"],
    //                                ClientSecret = _config["KeyCloakSettings:ClientSecret"],
    //                                RefreshToken = refreshToken
    //                            });
    //                            if (tokenResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
    //                            {
    //                                context.Properties.UpdateTokenValue("access_token", tokenResponse.AccessToken);
    //                                context.Properties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);

    //                                //if (context.Principal.Claims.Where(x => x.Type == "groups" && x.Value.Contains("govadmin")).Count() > 0)
    //                                //{
    //                                //    //var User = _userService.GetUserByEmail();
    //                                //    //_userService.AddAdmin(user, context.Principal.Claims);
    //                                //}

    //                                await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal, context.Properties);
    //                            }
    //                            else
    //                            {
    //                                //await context.HttpContext.SignOutAsync("Cookies", context.Principal, context.Properties);
    //                                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //, context.Principal, context.Properties);
    //                            }
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                            Log.Error("Unable to obtain new token from oauth Server: Message = {0}", ex.Message);
    //                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}