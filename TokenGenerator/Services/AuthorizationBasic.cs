﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace TokenGenerator.Services
{
    public class AuthorizationBasic : IAuthorizationBasic
    {
        private readonly Settings settings;

        public AuthorizationBasic(IOptions<Settings> settings)
        {
            this.settings = settings.Value;
        }

        public async Task<ActionResult> IsAuthorized(string authorizationString)
        {
            if (!ParseUserNamePassword(authorizationString, out string userName, out string password))
            {
                return new BadRequestResult();
            }

            if (!IsUserAuthorized(userName, password)) {
                return new BasicAuthenticationRequestResult();
            }

            return await Task.FromResult<ActionResult>(null);
        }

        private bool ParseUserNamePassword(string rawInput, out string userName, out string password)
        {
            userName = null;
            password = null;
            try
            {
                string[] parts = Encoding.UTF8.GetString(
                    // Add padding if missing
                    Convert.FromBase64String(rawInput + new string('=', (4 - rawInput.Length % 4) % 4))).Split(':', 2);
                userName = parts[0];
                password = parts[1];

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsUserAuthorized(string userName, string password)
        {
            return settings.BasicAuthorizationUsersDict.ContainsKey(userName) && string.Equals(settings.BasicAuthorizationUsersDict[userName], password);
        }
    }
}
