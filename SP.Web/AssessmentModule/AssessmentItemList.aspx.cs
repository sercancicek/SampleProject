using BSW.BusinessRules;
using BSW.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SP.Web.AssessmentModule
{
    public partial class AssessmentItemList : BasePage
    {
        private BusinessRules.OP_Assessment gAssessment;
        private BusinessRules.OP_LookUp gLookUp;
        private int gAssessmentID;

        protected void Page_Load(object sender, EventArgs e)
        {
            Operation_ID = OP_Operation.OP_ASSESSMENT_ITEM_LIST;
            Initialize();
            if (Page.IsPostBack == false)
                BeforePostback();
            else
                AfterPostback();
        }

        protected override void Initialize()
        {
            base.Initialize();
            try
            {
                gAssessmentID = Convert.ToInt32(this.DecryptQueryString(Request.QueryString["value"], "AssessmentID"));
            }
            catch (Exception)
            {
                gAssessmentID = -1;
                Set_Message_On_Another_Page("You can not access that page directly", "AssessmentList.aspx", true);
                return;
            }

            gAssessment = new BusinessRules.OP_Assessment(BSWSession.SessionID, BSWSession.UserID);
            gLookUp = new BusinessRules.OP_LookUp(BSWSession.SessionID, BSWSession.UserID);
            if (string.IsNullOrEmpty(this.SortBy))
                this.SortBy = nameof(ClsListAssessmentItem.Order);

            grd.AddDataColumn("", nameof(ClsListAssessmentItem.ItemID), PrepareMenu);
            grd.AddHiddenDataColumn("ItemID", nameof(ClsListAssessmentItem.ItemID));
            grd.AddHyperLinkColumn("Order", nameof(ClsListAssessmentItem.Order));
            grd.AddHyperLinkColumn("Type", nameof(ClsListAssessmentItem.TypeID), FormatTypeColumn);
            grd.AddDataColumn("Version", nameof(ClsListAssessmentItem.Version));
            grd.AddHyperLinkColumn("Text", nameof(ClsListAssessmentItem.Text));
            if (!IsUserAllowed(OP_Operation.OP_ASSESSMENT_ITEM_CREATE))
                btnAddNewAssessmentItem.Visible = false;
        }

        private string PrepareMenu(string FieldName, DataRow row, clsExtraStyle pObjES)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='dropdown'>");
            sb.Append("<a href='javascript: void(0);' class='dropdown-toggle' data-toggle='dropdown' aria-expanded='false'></a>");
            sb.Append("<ul class='dropdown-menu' aria-labelledby='' role='menu'>");
            sb.AppendFormat("<a class='dropdown-item' href='{0}'><i class='dropdown-icon {1}'></i> {2}</a>", "AssessmentItem.aspx" + EncryptQueryString("ID=" + row[nameof(ClsListAssessmentItem.ItemID)] + "&AssessmentID=" + gAssessmentID.ToString()), "fa fa-search", "View");
            return sb.ToString();
        }

        private string FormatTypeColumn(string FieldName, DataRow row, clsExtraStyle pObjES)
        {
            int type_id = -1;
            int.TryParse(row[FieldName].ToString(), out type_id);
            if (type_id > 0)
                return gLookUp.GetAssesmentItemTypeFromCache(type_id).Description;

            return "";
        }

        protected override void AfterPostback()
        {
            base.AfterPostback();
        }

        protected override void BeforePostback()
        {
            base.BeforePostback();
            lblAssessmentName.Text = gAssessment.GetAssessmentName(gAssessmentID);
            BindGrid();
        }

        private void BindGrid()
        {
            int total_count = 0;
            List<BusinessRules.ClsListAssessmentItem> listAssessmentItemList = gAssessment.GetAssessmentItemListFromCache(gAssessmentID);

            if (listAssessmentItemList == null)
                return;
            grd.TotalCount = total_count;
            grd.DataSource = listAssessmentItemList.ToDataTable();
        }

        protected void btnAddNewAssessmentItem_Click(object sender, EventArgs e)
        {
            string _url = "AssessmentItem.aspx" + EncryptQueryString("AssessmentID=" + gAssessmentID.ToString());
            Response.Redirect(_url);
        }
    }
}