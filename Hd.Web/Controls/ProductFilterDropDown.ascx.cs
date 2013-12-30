//
// Copyright (c) 2005-2013 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
//

using System.Web.UI;
using System.Web.UI.WebControls;
using Hd.Web.Extensions;

namespace Hd.Web.Controls
{
    public partial class ProductFilterDropDown : UserControl
    {
        protected void lstProducts_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            var project = this.lstProducts.SelectedItem.Value;
            var url = this.Page.Request.Url.AbsolutePath + (string.IsNullOrEmpty(project) ? "" : "?Project.Name=" + project);
            this.Page.Response.Redirect(url);
        }

        protected void lstProducts_DataBound(object sender, System.EventArgs e)
        {
            if (Page.IsPostBack) return;
            this.lstProducts.Items.Insert(0, new ListItem("- All projects -", ""));
            if (string.IsNullOrEmpty(this.Request.QueryString["Project.Name"]))
            {
                this.lstProducts.SelectedValue = "";
            }
            else
            {
                this.lstProducts.SelectedValue = this.Request.QueryString["Project.Name"];
            }
        }
    }
}