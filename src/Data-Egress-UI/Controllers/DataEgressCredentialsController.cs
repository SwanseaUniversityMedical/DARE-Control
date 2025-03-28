﻿using BL.Models;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BL.Models.APISimpleTypeReturns;

namespace Data_Egress_UI.Controllers
{
    [Authorize(Roles = "data-egress-admin")]
    public class DataEgressCredentialsController : Controller
    {
        private readonly IDataEgressClientHelper _clientHelper;
        public DataEgressCredentialsController(IDataEgressClientHelper client)
        {
            _clientHelper = client;
        }
        [HttpGet]
        public async Task<IActionResult> UpdateCredentialsAsync()
        {
            var valid = await _clientHelper.CallAPIWithoutModel<BoolReturn>("/api/TreCredentials/EgressCheckCredentialsAreValid");
            return View(new KeycloakCredentials()
                { Valid = valid.Result });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCredentials(KeycloakCredentials credentials)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _clientHelper.CallAPI<KeycloakCredentials, KeycloakCredentials>(
                        "/api/TreCredentials/EgressUpdateCredentials", credentials);
                if (result.Valid)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(credentials);
                }
            }
            else
            {
                return View(credentials);
            }
        }
    }
}