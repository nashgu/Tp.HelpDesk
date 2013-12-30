//
// Copyright (c) 2005-2013 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using Hd.Portal;
using Hd.Portal.Components;
using Hd.Web.Extensions;
using Hd.Web.Extensions.Components;
using log4net;

public partial class TpLogin : PersisterBasePage
{
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public override bool IsLoginPage
    {
        get { return true; }
    }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        btnLoginAsGuest.Visible = Settings.IsPublicMode;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        UserName.Focus();
        Response.Expires = 0;
        TryAutoLogin();
    }

    private void TryLoginAnonymously()
    {
        if (Settings.IsPublicMode)
            FormsAuthentication.RedirectFromLoginPage(Requester.ANONYMOUS_USER_ID.ToString(CultureInfo.InvariantCulture), false);
    }

    private void TrySSOLogin()
    {
        // get authentication cookie from Symfonie
        var symfCookie = HttpContext.Current.Request.Cookies.Get(ConfigurationManager.AppSettings["SymfonieCookie"]);
        if (symfCookie == null || string.IsNullOrEmpty(symfCookie.Value)) return;

        try
        {
            // UserData in format
            // string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}"), user.id, user.email, user.name, user.company, user.position, user.gravatar_hash, user.user_type.Enum, user.company_code);
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(symfCookie.Value);
            if (ticket.Expired)
            {
                log.Error("Authentication ticket is expired, please sign in by login page");
                return;
            }

            // if exists user && email, use this to authenticate to HD
            // TODO: this avoids AD authentication, becase there is no password
            string[] userData = ticket.UserData.Split('|');
            var email = userData[1];
            var userName = userData[2];
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(email))
            {
                // check if user exists in LDAP
                MembershipUser ldapUser = null;
                if (ExistsInLDAP(email, out ldapUser))
                {
                    PerformLogin(ldapUser.Email, ldapUser.UserName);
                }
            }
        }
        catch (Exception ex)
        {
            log.Warn("Cannot single sign on to the application", ex);
        }
    }

    private void TryAutoLogin()
    {
        if (IsPostBack)
            return;

        if (Globals.IsLogOut || Requester.IsLoggedAsAnonymous)
        {
            Globals.IsLogOut = false;
            return;
        }

        TrySSOLogin();

        TryLoginAnonymously();
    }

    private void PerformLogin(Requester requester)
    {
        Response.Cookies.Remove(Globals.LOGIN_COOKIE);
        Response.Cookies.Remove(Globals.PASSWORD_COOKIE);

        DataPortal.Instance.ResetCachedValue(typeof(Requester), requester.ID);

        FormsAuthentication.RedirectFromLoginPage(requester.ID.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), RememberMe.Checked);

        if (RememberMe.Checked)
        {
            var authCookie = HttpContext.Current.Request.Cookies.Get(FormsAuthentication.FormsCookieName);
            if (authCookie != null)
            {
                authCookie.Expires = authCookie.Expires.AddMinutes(20130);
            }
        }

        Globals.IsLogOut = false;
    }

    private void PerformLogin(string email, string userName)
    {
        Requester user = Requester.FindByEmail(email);

        if (user == null)
        {
            // temporarily in try..catch, 'cause once saved Requester is not valid, but able to be logged
            try
            {
                // if not exists, create new requester
                user = Requester.RetrieveOrCreate(user == null ? null : user.UserID);
                user.Email = email;
                user.Login = userName;
                user.Password = "LB14q4TyL6grOou";

                // ... what else to save to create valid requester?
                Requester.Save(user);
            }
            catch (Exception ex)
            {
                log.DebugFormat("Error creating requester", ex);
            }
        }

        PerformLogin(user);
    }

    protected void OnLogin(object sender, CommandEventArgs e)
    {
        switch (e.CommandName)
        {
            case "LoginAsGuest":
                Response.Redirect("~/");
                break;

            case "Login":
                string username = UserName.Text;
                string password = Password.Text;
                // try to logon using LDAP
                MembershipUser ldapUser = null;
                bool isInLDAP = LogonUsingLDAP(username, Password.Text, out ldapUser);
                if (isInLDAP)
                {
                    PerformLogin(ldapUser.Email, ldapUser.UserName);
                }
                else
                {
                    FailureText.Text = "Login failed. Most likely you have entered incorrect login or password.";
                }
                break;
        }
    }

    private bool LogonUsingLDAP(string username, string password, out MembershipUser user)
    {
        user = null;
        foreach (MembershipProvider provider in this.GetADMembershipProviders())
        {
            if (provider.GetType() != typeof(ActiveDirectoryMembershipProvider)) continue;
            if (ResolveUserName(provider.Name, ref username))
            {
                if (provider.ValidateUser(username, password))
                {
                    user = provider.GetUser(username, false);
                    return (user != null);
                }
            }
        }
        return false;
    }

    private bool ExistsInLDAP(string email, out MembershipUser user)
    {
        user = null;
        foreach (MembershipProvider provider in this.GetADMembershipProviders())
        {
            if (provider.GetType() == typeof(ActiveDirectoryMembershipProvider))
            {
                var userName = provider.GetUserNameByEmail(email);
                user = provider.GetUser(userName, false);
                return (user != null);
            }
        }
        return false;
    }

    private bool ResolveUserName(string domainName, ref string username)
    {
        if (username.Contains("\\"))
        {
            string[] loginParts = username.Split('\\');
            if (loginParts.Length > 2)
            {
                return false;
            }

            if (loginParts[0].ToUpper() == domainName.ToUpper())
            {
                username = loginParts[1];
            }
            else
            {
                return false;
            }
        }
        return username.Length > 0;
    }

    private IEnumerable<MembershipProvider> GetADMembershipProviders()
    {
        MembershipProvider cz = Membership.Providers["CZ"];
        if (cz != null)
        {
            yield return cz;
        }

        foreach (MembershipProvider provider in Membership.Providers)
        {
            if (provider != cz && provider is ActiveDirectoryMembershipProvider)
            {
                yield return provider;
            }
        }
    }

    private string StoreUserInfoIntoCookie(string username, System.Web.HttpCookie authCookie)
    {
        FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
        FormsAuthenticationTicket newTicket =
           new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, username);
        authCookie.Value = FormsAuthentication.Encrypt(newTicket);
        return authCookie.Value;
    }

    private void ShowPasswordSentPanel()
    {
        loginPanel.Visible = false;
        forgotPasswordPanel.Visible = false;
        passwordSentPanel.Visible = true;
    }

    protected void OnSendPasswordButtonClick(object sender, EventArgs e)
    {
        HideLoginPanel();
        try
        {
            bool result = Requester.ForgotPassword(emailToSendPassword.Text);
            if (result)
                ShowPasswordSentPanel();
            ShowErrorMessage(string.Format("The User with email '{0}' does not exist.", emailToSendPassword.Text));
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    private void ShowErrorMessage(string message)
    {
        errorMessage.Text = message;
        errorMessage.Visible = true;
    }

    private void HideLoginPanel()
    {
        loginPanel.Visible = false;
        forgotPasswordPanel.Visible = true;
    }

    protected void forgotPassword_OnForgotPassword(object sender, EventArgs e)
    {
        HideLoginPanel();
    }
}