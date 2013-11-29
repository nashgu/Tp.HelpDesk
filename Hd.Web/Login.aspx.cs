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

public partial class TpLogin : PersisterBasePage
{
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
		if(Settings.IsPublicMode)
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

	private void PerformLogin(string username, string password)
	{
		Response.Cookies.Remove(Globals.LOGIN_COOKIE);
		Response.Cookies.Remove(Globals.PASSWORD_COOKIE);

		//DataPortal.Instance.ResetCachedValue(typeof(Requester), requester.ID);

        bool authenticated = false;
        string fullDomainName = null;
        foreach (MembershipProvider provider in this.GetADMembershipProviders())
        {
            if (provider.GetType() == typeof(ActiveDirectoryMembershipProvider))
            {
                string login = username;
                if (username.Contains("\\"))
                {
                    string[] loginParts = username.Split('\\');
                    if (loginParts.Length > 2)
                    {
                        continue;
                    }

                    if (loginParts[0].ToUpper() == provider.Name.ToUpper())
                    {
                        login = loginParts[1];
                    }
                    else
                    {
                        continue;
                    }
                }

                if (provider.ValidateUser(login, password))
                {
                    fullDomainName = string.Format(@"{0}\{1}", provider.Name, login);
                    System.Web.HttpCookie authCookie = FormsAuthentication.GetAuthCookie(fullDomainName, false);
                    if (this.StoreUserInfoIntoCookie(username, authCookie) != null)
                    {
                        authCookie.Expires = authCookie.Expires.AddMinutes(20130);
                        Response.Cookies.Add(authCookie);
                        authenticated = true;
                        break;
                    }
                }
            }
        }
		FormsAuthentication.RedirectFromLoginPage(username, RememberMe.Checked);
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

                PerformLogin(UserName.Text, Password.Text);

                //if(Requester.Validate(UserName.Text, Password.Text))
                //{
                //    Requester user = Requester.FindByEmail(UserName.Text);
                //    PerformLogin(user);
                //}
                //else
                //    FailureText.Text = "Login failed. Most likely you have entered incorrect login or password.";
                break;
		}
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
