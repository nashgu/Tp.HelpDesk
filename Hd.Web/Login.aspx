<%@ Page Language="C#" AutoEventWireup="true" Inherits="TpLogin" Codebehind="Login.aspx.cs" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Login</title>
</head>
<body>
	<form id="frm" runat="server">
	<asp:ScriptManager ID="sm" runat="server" ScriptMode="Release" />
	<table width="100%" style="height: 100%; border: 1px solid #000">
		<tr>
			<td align="center" valign="middle">
				<table width="420" cellpadding="0" cellspacing="0">
					<tr>
						<td colspan="5" align="center" class="loginForm">
							<asp:UpdatePanel runat="server" ID="mailPanel">
								<ContentTemplate>
									<asp:Panel runat="server" ID="loginPanel">
										<table id="loginPanel" cellpadding="0" cellspacing="0" style="margin: 20px">
											<tr>
												<td class="login_header">
												<img src="img/symfonie_logo.png" alt="Symfonie" />Moravia Help Desk
												</td>
											</tr>
											<tr>
												<td style="color: #507cb6">
													<asp:Literal ID="FailureText" runat="server"></asp:Literal>
												</td>
											</tr>
											<tr>
												<td>
												
                                                    <label for="USerName">Username</label>
													<asp:TextBox ID="UserName" CssClass="form-control" runat="server"></asp:TextBox>
													<asp:RequiredFieldValidator ValidationGroup="login" ID="UserNameRequired" runat="server"
														ControlToValidate="UserName" Text="*"></asp:RequiredFieldValidator>
												</td>
											</tr>
											<tr>
												
												<td>
                                                    <label for="Password">Password</label>
													<asp:TextBox ID="Password" CssClass="form-control" runat="server" TextMode="Password"></asp:TextBox>
													<asp:RequiredFieldValidator ValidationGroup="login" ID="PasswordRequired" runat="server"
														ControlToValidate="Password" Text="*"></asp:RequiredFieldValidator>
												</td>
											</tr>
											<tr>
												<td>
													<asp:CheckBox ID="RememberMe" runat="server" Text="&nbsp;Remember me next time"></asp:CheckBox>
												</td>
											</tr>
											<tr>
												<td>
													<asp:Button ID="btnLogin" ValidationGroup="login" CssClass="btn btn-default" CommandName="Login"
														runat="server" Text="Enter" OnCommand="OnLogin"></asp:Button>
													<asp:Button ID="btnLoginAsGuest" CssClass="inputLarge"  CommandName="LoginAsGuest" runat="server" Text="Guest" OnCommand="OnLogin" />
												</td>
											</tr>
											<tr>
												<td align="left">
													Please login to see your requests.
												</td>
											</tr>
										</table>
									</asp:Panel>
								</ContentTemplate>
							</asp:UpdatePanel>
						</td>
					</tr>
					<tr>
						<td colspan="5" class="copy">
							Copyright &copy; 2004-2012 <a href="http://www.targetprocess.com" target="_blank">TargetProcess.</a>
							All rights reserved.<br />
						</td>
					</tr>
				</table>
			</td>
		</tr>
	</table>
	</form>
</body>
</html>
