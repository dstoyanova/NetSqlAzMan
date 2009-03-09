using System;
using System.IO;
using System.Xml;
using System.Security.Principal;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.CodeDom;
using NetSqlAzMan;
using NetSqlAzMan.Interfaces;

namespace NetSqlAzManWebConsole
{
    public partial class dlgGenerateCheckAccessHelper : dlgPage
    {
        protected internal IAzManStorage storage = null;
        internal IAzManApplication application;

        protected void Page_Init(object sender, EventArgs e)
        {
            this.setImage("NetSqlAzMan_32x32.gif");
            this.setOkHandler(new EventHandler(this.btnOk_Click));
            this.showCloseOnly();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.storage = this.Session["storage"] as IAzManStorage;
            this.application = this.Session["selectedObject"] as IAzManApplication;
            this.Text = "Generate CheckAccessHelper class";
            this.Description = this.Text;
            this.Title = this.Text;
            if (!Page.IsPostBack)
            {
                this.btnCopy.Attributes["onclick"] = String.Format("javascript: copyToClipBoard('{0}');", this.txtSourceCode.UniqueID);
                this.txtNamespace.Text = this.TransformToVariable("", this.application.Name, false) + ".Security";
                this.btnGenerate_Click(this, EventArgs.Empty);
                this.txtClassName.Focus();
            }
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                this.closeWindow(false);
            }
            catch (Exception ex)
            {
                this.ShowError(ex.Message);
            }
        }

        private string TransformToVariable(string prefix, string name, bool toupper)
        {
            string ris = "";
            if (String.IsNullOrEmpty(name)) return String.Empty;
            char[] nc = name.ToCharArray();
            for (int i = 0; i < nc.Length; i++)
            {
                if (!char.IsLetterOrDigit(nc[i]) && (nc[i] != '_'))
                {
                    if (nc[i] != '[')
                    {
                        ris += "_";
                    }
                }
                else
                {
                    ris += nc[i].ToString();
                }
            }
            if (toupper)
            {
                ris = ris.ToUpper();
            }
            if (!char.IsLetter(ris.ToCharArray()[0]))
            {
                ris = "_" + ris;

            }
            ris = ris.Trim('_');
            ris = prefix + ris;
            return ris;
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                this.txtSourceCode.Text = String.Empty;
                CodeCompileUnit ccu = NetSqlAzMan.CodeDom.CodeDomGenerator.GenerateCheckAccessHelperClass(this.txtNamespace.Text, this.txtClassName.Text, this.chkAllowRoles.Checked, this.chkAllowTasks.Checked, this.application, this.rbCSharp.Checked ? NetSqlAzMan.CodeDom.Language.CSharp : NetSqlAzMan.CodeDom.Language.VB);
                this.txtSourceCode.Text = NetSqlAzMan.CodeDom.CodeDomGenerator.GenerateSourceCode(ccu, this.rbCSharp.Checked ? NetSqlAzMan.CodeDom.Language.CSharp : NetSqlAzMan.CodeDom.Language.VB);
            }
            catch (Exception ex)
            {
                this.ShowError(ex.Message);
            }
        }
    }
}