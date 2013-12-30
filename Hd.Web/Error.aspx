<%@ Page Language="C#" AutoEventWireup="true" Inherits="Error" CodeBehind="Error.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body style="background: #BDD9FF">
    <form id="form1" runat="server">
        <div style="padding: 20px">
            <h2>Whoops!</h2>
            <div style="font: 16px Arial">
                There was something incorrect with system behavior.
                <br />
                <br />
            </div>
            <div>
                Try to <a href="Login.aspx">Login</a> again.
            </div>
        </div>
        <br />
        <div style="color: #444; padding-left: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
        <br />
    </form>
</body>
</html>