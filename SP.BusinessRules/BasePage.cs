using SP.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace SP.BusinessRules
{
    public class BasePage : Page
    {

        #region "Members"
        private static string mDefaultPage = "Landing.aspx";
        public string mDashboardPage = "Dashboard.aspx";
        private object mSystemLogObject = null;
        private static PageRenderFunctionType mAfterPageRenderFunction = null;
        private static PageLoadFunctionType mBeforePageLoadFunction = null;
        private static PageLoadFunctionType mAfterPageLoadFunction = null;
        #endregion

        #region "Properties"
        public int Operation_ID { get; set; }
        public bool gIsPopup { get; set; }
        public string gPopupDataType { get; set; }
        #endregion

        public delegate void PageLoadFunctionType(string DateTime, BasePage Page, string PageName, bool IsPostBack, BSWSession pBSWSession, BSWApplication pBSWApplication);

        public delegate void PageRenderFunctionType(BasePage Page, string PageName);

        private BSWSession gBSWSession;

        public BSWSession BSWSession
        {
            get
            {
                if (gBSWSession == null)
                    gBSWSession = new BSWSession(this.Session);
                return gBSWSession;
            }
            set
            {
                gBSWSession = value;
                //if (IsUserAllowed(OP_Operation.OP_ADMIN_DASHBOARD))
                //    mDashboardPage = "AdminDashboard.aspx";
            }
        }

        protected virtual void BindControlsToDataSources()
        {
        }

        protected virtual void Initialize()
        {

        }

        protected bool IsUserAllowed(int pOperationID)
        {
            bool res = false;
           // res = OP_Operation.IsUserAllowed(pOperationID, BSWSession.UserID);
            return res;
        }

        protected virtual void BeforePostback()
        {
        }

        protected virtual void AfterPostback()
        {
        }

        public static void SetBeforePageLoadFunction(PageLoadFunctionType pBeforePageLoadFunction)
        {
            mBeforePageLoadFunction = pBeforePageLoadFunction;
        }

        public static void SetAfterPageLoadFunction(PageLoadFunctionType pAfterPageLoadFunction)
        {
            mAfterPageLoadFunction = pAfterPageLoadFunction;
        }

        public static void SetAfterPageRenderFunction(PageRenderFunctionType pAfterPageRenderFunction)
        {
            mAfterPageRenderFunction = pAfterPageRenderFunction;
        }


        public object SystemLogObject
        {
            get
            {
                return mSystemLogObject;
            }
            set
            {
                mSystemLogObject = value;
            }
        }

        public void Redirect(string url)
        {
            if (string.IsNullOrEmpty(url))
                url = "~/" + mDashboardPage;

            Response.Redirect(url);
        }

        protected virtual string GetCurrentPageQuery()
        {
            return Request.Url.Query;
        }

        protected virtual string AppendAdditionalUrlQueryParams(string query)
        {
            return query;
        }

        protected void DisableAllPage(string pMostTopControlClientID)
        {
            string scr = "$('#" + pMostTopControlClientID + "').addClass('disabledbutton');";
            if (IsPostBack == false)
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "InitDTgrid" + Guid.NewGuid().ToString(), scr, true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "InitPage" + Guid.NewGuid().ToString(), scr, true);
            }
        }

        protected string GetReturnUrlForCurrentPage()
        {
            var query = GetCurrentPageQuery();
            query = AppendAdditionalUrlQueryParams(query);

            if (string.IsNullOrEmpty(query))
                return Request.Url.AbsolutePath;
            else
                return Request.Url.AbsolutePath + "?" + query;
        }


        public string SortBy
        {
            get { return CommonUtilities.SpaceIfNull((this.ViewState["SortBy"])); }
            set { this.ViewState["SortBy"] = value; }
        }

        public int CurrentGridPage
        {
            get
            {
                int _current_page = 1;
                int.TryParse(CommonUtilities.SpaceIfNull((this.ViewState["CurrentGridPage"])), out _current_page);
                if (_current_page == 0)
                    _current_page = 1;

                return _current_page;
            }
            set { this.ViewState["CurrentGridPage"] = value; }
        }

        public bool IsSortDescending
        {
            get { return Convert.ToBoolean((this.ViewState["IsSortDescending"])); }
            set { this.ViewState["IsSortDescending"] = value; }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            try
            {
                gIsPopup = false;
                if (Convert.ToBoolean(this.DecryptQueryString(Request.QueryString["value"], "Popup")))
                {
                    gIsPopup = true;
                    MasterPageFile = CommonUtilities.BaseURL(Request) + "General_Plain.Master";
                }
            }
            catch { }

            try
            {
                gPopupDataType = this.DecryptQueryString(Request.QueryString["value"], "POPUPDATATYPE");
            }
            catch { }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            MaintainScrollPositionOnPostBack = true;

            string pageName = System.IO.Path.GetFileNameWithoutExtension(Request.Path).ToLower();

            var session = this.BSWSession;
            if (pageName != "login" && pageName != "landing" && pageName != "preview" && pageName != "signup" && (session == null || session.UserID < 1))
            {
                RedirectToLogin("Not authorized");
                return;
            }

            if (Session["page_msg_suc"] != null && !(string.IsNullOrEmpty(Session["page_msg_suc"].ToString())))
            {
                Set_Success_Message(Session["page_msg_suc"].ToString());
                Session["page_msg_suc"] = null;
            }
            else if (Session["page_msg_err"] != null && !(string.IsNullOrEmpty(Session["page_msg_err"].ToString())))
            {
                Set_Error_Message(Session["page_msg_err"].ToString());
                Session["page_msg_err"] = null;
            }

            if (pageName != "login" && pageName != "landing" && Constants.gBSWApplication.Check_If_User_Active(session.UserID, session.SessionID, session.IPAddress) == false)
            {

                //BusinessRules_Common.User.clsUserInfoCache objUser = BusinessRules_Common.User.Get_User_Information_From_Cache(cses.UserID, null);
                //string url = BaseURL(page) + "Login.aspx?err=1002";
                //if ((objUser == null) == false)
                //{
                //    if (objUser.IsDeleted)
                //    {
                //        url = BaseURL(page) + "Login.aspx?err=1006";
                //    }
                //    else if (objUser.IsPassive)
                //    {
                //        url = BaseURL(page) + "Login.aspx?err=1007";
                //    }
                //}

                //page.Response.Redirect(url);

                RedirectToLogin("Not authorized");
                return;
            }

            if ((mBeforePageLoadFunction != null))
            {
                mBeforePageLoadFunction.Invoke(DateTime.Now.ToString(), this, Get_Decoded_Query_String(), this.IsPostBack, this.BSWSession, Constants.gBSWApplication);
            }
        }
        public string Get_Decoded_Query_String()
        {
            string decodedQuerystring = null;
            int i = this.Request.Url.Query.ToLower().IndexOf("?value=");
            if (i >= 0)
            {
                string querystring = this.Request.Url.Query.Substring(i + 7);
                //querystring = querystring.Replace("%3d", "=")
                querystring = Server.UrlDecode(querystring);
                string remaining = "";
                int j = 0;

                if (i + 1 < querystring.Length)
                {
                    j = querystring.IndexOf("&", i + 1);
                }
                else
                {
                    j = -1;
                }

                if (j > -1)
                {
                    remaining = querystring.Substring(j);
                    querystring = querystring.Substring(0, j);
                }

                try
                {
                    decodedQuerystring = "+++" + DecryptQueryString(querystring) + remaining;
                }
                catch
                {
                    decodedQuerystring = "+++" + querystring + remaining;
                }

                decodedQuerystring = decodedQuerystring.Replace("+++value=", "");
                decodedQuerystring = decodedQuerystring.Replace("+++", "");
            }
            else
            {
                decodedQuerystring = this.Request.Url.Query;
            }

            return decodedQuerystring;
        }


        private void RedirectToLogin(string pMsg = "")
        {
            Redirect("~/Login.aspx?returnURL=" + HttpUtility.UrlEncode(Request.RawUrl));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var scr = "";

            if (scr != "")
                ScriptManager.RegisterStartupScript(this, GetType(), "scrGnrl", scr, true);
        }

        public virtual void Set_Success_Message(string message, params object[] args)
        {
            message = message.Replace(System.Environment.NewLine, " ");
            if (args != null && args.Length > 0)
                message = string.Format(message, args);

            ScriptManager.RegisterStartupScript(this,
                            this.GetType(),
                            Guid.NewGuid().ToString(),
                            string.Format("showSuccessMessage('{0}');", message.Replace("'", @"\'")),
                            true
                        );
        }

        public virtual void Set_Success_Notification(string header, string message, params object[] args)
        {
            message = message.Replace(System.Environment.NewLine, " ");
            if (args != null && args.Length > 0)
                message = string.Format(message, args);

            string jsCode = @"$(function (){
               $.notify({
                    icon: 'font-icon font-icon-warning',
                    title: ""<strong>" + header + @":</strong>"",
                    message:""" + message + @"""},{
                    placement: {
                        align: 'center'
                    },
                    type:'success'});
                return false;});";

            ScriptManager.RegisterStartupScript(this,
                            GetType(),
                            Guid.NewGuid().ToString(),
                            jsCode,
                            true
                        );
        }

        public virtual void Set_Error_Notification(string header, string message, params object[] args)
        {
            message = message.Replace(System.Environment.NewLine, " ");
            if (args != null && args.Length > 0)
                message = string.Format(message, args);

            string jsCode = @"$(function (){
               $.notify({
                    icon: 'font-icon font-icon-warning',
                    title: ""<strong>" + header + @":</strong>"",
                    message:""" + message + @"""},{
                    placement: {
                        align: 'center'
                    },
                    type:'danger'});
                return false;});";

            ScriptManager.RegisterStartupScript(this,
                            GetType(),
                            Guid.NewGuid().ToString(),
                            jsCode,
                            true
                          );
        }

        public virtual void Set_Warning_Message(string message, params object[] args)
        {
            message = message.Replace(System.Environment.NewLine, " ");
            if (args != null && args.Length > 0)
                message = string.Format(message, args);

            ScriptManager.RegisterStartupScript(this,
                            this.GetType(),
                            Guid.NewGuid().ToString(),
                            string.Format("showWarningMessage('{0}');", message.Replace("'", @"\'")),
                            true
                        );
        }

        protected virtual void Set_Message_On_Another_Page(string message, string pURL, bool pIsErrorMesage)
        {
            if (pIsErrorMesage)
                Session["page_msg_err"] = message;
            else
                Session["page_msg_suc"] = message;

            Redirect(pURL);
        }

        public virtual void Set_Error_Message(string message, params object[] args)
        {
            message = message.Replace(System.Environment.NewLine, string.Empty);
            if (args != null && args.Length > 0)
                message = string.Format(message, args);

            ScriptManager.RegisterStartupScript(this,
                            this.GetType(),
                            Guid.NewGuid().ToString(),
                            string.Format("showErrorMessage('{0}');", message.Replace("'", @"\'")),
                            true
                        );
        }

        public virtual void DisablePanelAndContents(Control pCtrl)
        {
            ScriptManager.RegisterStartupScript(this,
                          this.GetType(),
                          Guid.NewGuid().ToString(),
                          string.Format("$('{0}').addClass('disabledbutton');", pCtrl.ClientID),
                          true
                      );
        }

        protected virtual Control FindControlRecursive(string id)
        {
            return FindControlRecursive(id, this);
        }
        protected virtual Control FindControlRecursive(string id, Control parent)
        {
            // If parent is the control we're looking for, return it
            if (string.Compare(parent.ID, id, true) == 0)
                return parent;
            // Search through children
            foreach (Control child in parent.Controls)
            {
                Control match = FindControlRecursive(id, child);
                if (match != null)
                    return match;
            }
            // If we reach here then no control with id was found
            return null;
        }

        protected void Page_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (!(ex is ThreadAbortException))
            {
                Session["LastError"] = ex.Message + "    " + ex.Data + Environment.NewLine + ex.StackTrace;
                // Do something with the exception e.g. log it
                CommonUtilities.Log("-------GENERIC ERROR-------" + Environment.NewLine);
                CommonUtilities.Log(ex);
                CommonUtilities.Log(Environment.NewLine + "-------END-------" + Environment.NewLine);
                Redirect("~/GenericError.aspx");
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //string tmp_scr = "if (Override_DoPostBack != null) { Override_DoPostBack()}";
            //    this.ClientScript.RegisterStartupScript(this.GetType(), "dopostback", "<script language=javascript>" + tmp_scr + "</script>");

            OP_User user_br = new OP_User(this.BSWSession.SessionID, this.BSWSession.UserID);

            //check the operation id
            if (Operation_ID > 0)
            {
                if (!IsUserAllowed(Operation_ID))
                    Set_Message_On_Another_Page("NOT AUTHORIZED", CommonUtilities.BaseURL(this) + mDashboardPage, true);
            }

        }

        public void ThrowNotAuthorized()
        {
            Set_Message_On_Another_Page("NOT AUTHORIZED", CommonUtilities.BaseURL(this) + mDashboardPage, true);
        }

        public void GotoDefaultPage(string iMessage)
        {
            string gotoPage = null;
            if (string.IsNullOrEmpty(mDefaultPage))
            {
                gotoPage = CommonUtilities.BaseURL(this) + mDashboardPage;
            }
            else
            {
                gotoPage = mDefaultPage;
            }

            if (!string.IsNullOrEmpty(iMessage))
            {
                gotoPage += EncryptQueryString("Message=" + iMessage);
            }

            gotoPage = CommonUtilities.BaseURL(this.Request) + gotoPage;
            Response.Redirect(AppendCacheBusterToURL(gotoPage));
        }

        public static string AppendCacheBusterToURL(string pURL)
        {
            string t = "?";
            if (pURL.Contains("?"))
            {
                t = "&";
            }

            return pURL + t + "TS=" + DateTime.Now.Ticks.ToString();
        }

        public string EncryptQueryString(string pQueryString)
        {
            clsEncryptDecryptQueryString objEDQueryString = new clsEncryptDecryptQueryString();
            return "?value=" + HttpUtility.UrlEncode(objEDQueryString.Encrypt(pQueryString, CommonUtilities.privateKey.Substring(0,8)));
        }

        public string DecryptQueryString(string pQueryString)
        {
            if (pQueryString.StartsWith("?value="))
                pQueryString = pQueryString.Replace("?value=", "");
            clsEncryptDecryptQueryString objEDQueryString = new clsEncryptDecryptQueryString();
            return HttpUtility.UrlDecode(objEDQueryString.Decrypt(pQueryString, CommonUtilities.privateKey.Substring(0, 8)));
        }

        /// <summary>
        /// Get value from encrypted query string
        /// </summary>
        /// <param name="pQueryString">Query String to decrypted</param>
        /// <param name="pKey">The key of the value to get </param>
        /// <returns></returns>
        public string DecryptQueryString(string pQueryString, string pKey)
        {
            NameValueCollection parsedQS = HttpUtility.ParseQueryString(DecryptQueryString(pQueryString));
            if (parsedQS[pKey] != null)
                return parsedQS[pKey];
            else
                return "";
        }
    }
}
