// 
// Copyright (c) 2005-2013 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
// 

using System;
using System.Collections.Generic;
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

    private void TryAutoLogin()
    {
        if (IsPostBack)
            return;

        if (Globals.IsLogOut || Requester.IsLoggedAsAnonymous)
        {
            Globals.IsLogOut = false;
            return;
        }

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
                    Requester user = Requester.FindByEmail(ldapUser.Email);

                    if (user == null)
                    {
                        // temporarily in try..catch, 'cause once saved Requester is not valid, but able to be logged
                        try
                        {
                            // if not exists, create new requester
                            user = Requester.RetrieveOrCreate(user == null ? null : user.UserID);
                            user.Email = ldapUser.Email;
                            user.Login = ldapUser.UserName;
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
            if (provider.GetType() == typeof(ActiveDirectoryMembershipProvider))
            {                
                if (username.Contains("\\"))
                {
                    string[] loginParts = username.Split('\\');
                    if (loginParts.Length > 2)
                    {
                        continue;
                    }

                    if (loginParts[0].ToUpper() == provider.Name.ToUpper())
                    {
                        username = loginParts[1];
                    }
                    else
                    {
                        continue;
                    }
                }

                if (provider.ValidateUser(username, password))
                {
                    user = provider.GetUser(username, false);                    
                    return (user != null);
                }
            }
        }

        return false;
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