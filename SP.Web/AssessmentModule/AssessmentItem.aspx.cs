using BSW.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BSW.DataAccess;
using System.Text;
using BSW.BusinessRules;

namespace SP.Web.AssessmentModule
{
    public partial class AssessmentItem : BasePage
    {
        private BusinessRules.OP_Assessment gAssessment;
        private BusinessRules.OP_LookUp gLookUp;
        private int gAssessmentID;
        private Constants.enPageMode gPageMode;
        private bool gIsMultipleChoice;
        private bool gIsGridChoice;


        protected void Page_Load(object sender, EventArgs e)
        {
            this.Operation_ID = OP_Operation.OP_ASSESSMENT_ITEM_LIST;
            Initialize();
            if (Page.IsPostBack == false)
                BeforePostback();
            else
                AfterPostback();
        }

        public int AssessmentItemID
        {
            get
            {
                if (ViewState["AssessmentItemID"] == null)
                    ViewState["AssessmentItemID"] = -1;

                return (int)ViewState["AssessmentItemID"];
            }
            set
            {
                ViewState["AssessmentItemID"] = value;
            }
        }

        public List<string> ListSelectedAssessmentItems
        {
            get
            {
                if (ViewState["ListSelectedAssessmentItems"] == null)
                    ViewState["ListSelectedAssessmentItems"] = new List<string>();

                return (List<string>)ViewState["ListSelectedAssessmentItems"];
            }
            set
            {
                ViewState["ListSelectedAssessmentItems"] = value;
            }
        }


        private List<DataAccess.AssessmentItemXChoice> ListChoice
        {
            get
            {
                if (Session["ListChoice" + AssessmentItemID.ToString()] == null)
                    Session["ListChoice" + AssessmentItemID.ToString()] = new List<DataAccess.AssessmentItemXChoice>();
                return (List<DataAccess.AssessmentItemXChoice>)Session["ListChoice" + AssessmentItemID.ToString()];
            }
            set
            {
                Session["ListChoice" + AssessmentItemID.ToString()] = value;
            }
        }

        private List<DataAccess.AssessmentItem> ListSubAI
        {
            get
            {
                if (Session["ListSubAI" + AssessmentItemID.ToString()] == null)
                    Session["ListSubAI" + AssessmentItemID.ToString()] = new List<DataAccess.AssessmentItem>();
                return (List<DataAccess.AssessmentItem>)Session["ListSubAI" + AssessmentItemID.ToString()];
            }
            set
            {
                Session["ListSubAI" + AssessmentItemID.ToString()] = value;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            gAssessment = new BusinessRules.OP_Assessment(BSWSession.SessionID, BSWSession.UserID);
            gLookUp = new BusinessRules.OP_LookUp(BSWSession.SessionID, BSWSession.UserID);

            Set_QS_Values();
            if (gAssessmentID < 1)
            {
                Set_Message_On_Another_Page("You can not access that page directly", "AssessmentList.aspx", true);
                return;
            }

            if (AssessmentItemID > 0)
            {
                gPageMode = Constants.enPageMode.UPDATE_DATA;
                pnlChoice.Visible = true;
            }
            else
            {
                gPageMode = Constants.enPageMode.NEW_DATA;
                pnlChoice.Visible = false;
                if (!IsUserAllowed(OP_Operation.OP_ASSESSMENT_ITEM_CREATE))
                    ThrowNotAuthorized();
            }

            if (gPageMode == Constants.enPageMode.UPDATE_DATA && !IsUserAllowed(OP_Operation.OP_ASSESSMENT_ITEM_UPDATE))
            {
                btnSaveAll.Visible = false;
                btnSaveAssessmentItem.Visible = false;
                btnSaveChoice.Visible = false;

                DisableAllPage(pnlAI.ClientID);
            }

            btnBack.HRef = "AssessmentItemList.aspx" + EncryptQueryString("AssessmentID=" + gAssessmentID);
        }

        private void CheckIfTypeIsMultipleChoice()
        {
            if (string.IsNullOrEmpty(cmbType.SelectedValue))
            {
                gIsGridChoice = false;
                gIsMultipleChoice = false;
            }

            Constants.enAssessmentItemType _val = (Constants.enAssessmentItemType)Convert.ToInt32(cmbType.SelectedValue);
            gIsMultipleChoice = (_val == Constants.enAssessmentItemType.MULTIPLE_CHOICE_CHK || _val == Constants.enAssessmentItemType.MULTIPLE_CHOICE_RADIO);

            gIsGridChoice = (_val == Constants.enAssessmentItemType.GRID_CHK || _val == Constants.enAssessmentItemType.GRID_RADIO);

        }

        private void Set_QS_Values()
        {
            try
            {
                if (AssessmentItemID < 1)
                    AssessmentItemID = Convert.ToInt32(this.DecryptQueryString(Request.QueryString["value"], "ID"));
                gPageMode = Constants.enPageMode.UPDATE_DATA;
            }
            catch (Exception)
            {
                AssessmentItemID = -1;
            }

            try
            {
                gAssessmentID = Convert.ToInt32(this.DecryptQueryString(Request.QueryString["value"], "AssessmentID"));
            }
            catch (Exception)
            {
                gAssessmentID = -1;
                return;
            }
        }

        protected override void BeforePostback()
        {
            base.BeforePostback();
            lblAssessmentName.Text = gAssessment.GetAssessmentName(gAssessmentID);
            if (cmbAssessmentItems.DataSource == null)
            {
                cmbAssessmentItems.DataTextField = nameof(ClsListAssessmentItem.Text);
                cmbAssessmentItems.DataValueField = nameof(ClsListAssessmentItem.ItemID);
                List<ClsListAssessmentItem> listAI = gAssessment.GetAssessmentItemListFromCache(gAssessmentID, true);
                if (listAI != null && listAI.Count > 0)
                    listAI = listAI.FindAll(o => o.TypeID != Convert.ToInt32(Constants.enAssessmentItemType.PAGE_BREAK));
                cmbAssessmentItems.DataSource = listAI;
                cmbAssessmentItems.DataBind();
                if (cmbAssessmentItems.Items.FindByValue("-1") == null)
                    cmbAssessmentItems.Items.Insert(0, new ListItem("Please select...", "-1"));

                cmbAssessmentItems.Enabled = true;
            }
            BindControlsToDataSources();
            if (gPageMode == Constants.enPageMode.UPDATE_DATA)
            {
                //fill the question information
                DataAccess.AssessmentItem objAssessment = gAssessment.GetAssessmentItemByItemIDFromCache(AssessmentItemID);
                FillControls(objAssessment);
                CheckIfTypeIsMultipleChoice();
                //fill choices
                ListChoice = gAssessment.GetAssessmentItemChoicesByItemIDFromCache(AssessmentItemID, true);
                ListSubAI = gAssessment.GetSubAssessmentItemsByParentID(AssessmentItemID);
                Prepare_Choice_Section();
            }
        }



        private void FillControls(DataAccess.AssessmentItem objAssessment)
        {
            if (objAssessment.Order > 0)
                txtOrder.Text = objAssessment.Order.ToString();
            if (string.IsNullOrEmpty(objAssessment.Text) == false)
                txtText.Text = objAssessment.Text;
            if (string.IsNullOrEmpty(objAssessment.HelpText) == false)
                txtHelpText.Text = objAssessment.HelpText;
            if (objAssessment.IsMandatory)
                chkIsRequired.Checked = true;
            if (objAssessment.Version > 0)
                txtVersion.Text = objAssessment.Version.ToString();
            if (objAssessment.TypeID > 0)
                cmbType.SelectedValue = objAssessment.TypeID.ToString();
            if (!string.IsNullOrEmpty(objAssessment.DisplayCondition))
                txtFormula.Text = objAssessment.DisplayCondition;
        }

        protected override void AfterPostback()
        {
            base.AfterPostback();
            CheckIfTypeIsMultipleChoice();
        }

        protected override void BindControlsToDataSources()
        {
            base.BindControlsToDataSources();
            cmbType.DataTextField = "Description";
            cmbType.DataValueField = "ID";
            cmbType.DataSource = gLookUp.GetAssessmentItemTypesFromCache();
            cmbType.DataBind();


            ArrageUIEnableDisable();

            cmbOperator.Items.Insert(0, new ListItem("Please select...", "-1"));
            cmbOperator.Items.Add(new ListItem("Equal", "=="));
            cmbOperator.Items.Add(new ListItem("Not Equal", "!="));
        }

        private void ArrageUIEnableDisable()
        {
            if (string.IsNullOrEmpty(cmbAssessmentItems.SelectedValue) || cmbAssessmentItems.SelectedValue.Equals("-1"))
            {
                cmbControl.Enabled = false;
                divButtons.Enabled = false;
                cmbOperator.Enabled = false;
                txtControl.Enabled = false;
                return;
            }
            else
            {
                cmbOperator.Enabled = true;
                cmbControl.Enabled = false;
                divButtons.Enabled = false;
                txtControl.Enabled = false;
            }
            if (string.IsNullOrEmpty(cmbOperator.SelectedValue) || cmbOperator.SelectedValue.Equals("-1"))
            {
                cmbControl.Enabled = false;
                divButtons.Enabled = false;
                txtControl.Enabled = false;
                return;
            }
            else
            {
                cmbControl.Enabled = true;
                txtControl.Enabled = true;
                divButtons.Enabled = false;
            }
            if (cmbControl.Visible)
            {
                if (cmbControl.SelectedValue.Equals("-1"))
                    divButtons.Enabled = false;
                else
                    divButtons.Enabled = true;
            }
            else
            {
                divButtons.Enabled = true;
            }
        }

        private void Prepare_Choice_Section()
        {
            if (ListChoice.Count > 0)
            {
                divNewChoice.Visible = false;
                Create_Choice_Grid();
                grdChoices.Visible = true;
            }
            else
            {
                divNewChoice.Visible = true;
                grdChoices.Visible = false;
            }
            if (gIsMultipleChoice)
                divNewChoice.Visible = true;

            if (gIsGridChoice)
            {
                btnSaveChoice.Visible = false;
                btnAddGridChoice.Visible = true;
                divGridSubItem.Visible = true;
                divGridSubItem_Grid.Visible = true;
                divChoice.Visible = false;
                Create_SubAI_Grid();
                if (ListSubAI.Count > 0)
                {
                    divChoice.Visible = true;
                    divNewChoice.Visible = true;
                    Create_Choice_Grid();
                    grdChoices.Visible = true;
                }
            }
            else
            {
                btnSaveChoice.Visible = true;
                btnAddGridChoice.Visible = false;
                divGridSubItem.Visible = false;
                divGridSubItem_Grid.Visible = false;
                divChoice.Visible = true;
                divSaveAll.Visible = false;
            }

        }

        protected void btnSaveAssessmentItem_Click(object sender, EventArgs e)
        {
            string pErrMsg = string.Empty;
            if (!Validate_Data(ref pErrMsg))
            {
                Set_Error_Message(pErrMsg);
                return;
            }
            if(gPageMode == Constants.enPageMode.NEW_DATA)
            {
                List<BusinessRules.ClsListAssessmentItem> listAIs = gAssessment.GetAssessmentItemListFromCache(gAssessmentID);
                if (listAIs != null && listAIs.Count > 0)
                {
                    List<BusinessRules.ClsListAssessmentItem> listT = listAIs.FindAll(o => o.Order == Convert.ToInt32(txtOrder.Text));
                    if (listT != null && listT.Count > 0)
                    {
                        Set_Error_Message("Another item with the same order exists! Please swap the records");
                        return;
                    }
                }
            }

            DataAccess.AssessmentItem _objAssessment;
            if (gPageMode == Constants.enPageMode.NEW_DATA)
            {
                _objAssessment = new DataAccess.AssessmentItem();
                _objAssessment.Version = 1;
                _objAssessment.AssessmentItemTreeID = gAssessment.GetNextAssessmentItemTreeID();
            }
            else
            {
                _objAssessment = gAssessment.GetAssessmentItemByItemIDFromCache(AssessmentItemID);
                _objAssessment.Version = gAssessment.GetNextVersionNumber(_objAssessment.AssessmentItemTreeID);

            }
            _objAssessment.AssessmentID = gAssessmentID;
            _objAssessment.DisplayCondition = "";
            _objAssessment.HelpText = txtHelpText.Text;
            _objAssessment.Text = txtText.Text;
            _objAssessment.Order = Convert.ToInt32(txtOrder.Text);
            _objAssessment.TypeID = Convert.ToInt32(cmbType.SelectedValue);
            _objAssessment.IsMandatory = Convert.ToBoolean(chkIsRequired.Checked);
            _objAssessment.IsPageBreak = false;
            _objAssessment.IsLatest = true;
            _objAssessment.AssessmentItemID = AssessmentItemID;
            _objAssessment.DisplayCondition = txtFormula.Text;
            bool res = gAssessment.Save_Assessment_Item(_objAssessment);
            if (res == false)
            {
                Set_Error_Message("An error oocured during assessment item save process");
                return;
            }
            else
            {
                foreach (string item in ListSelectedAssessmentItems)
                {
                    int _id = Convert.ToInt32(item);
                    res = gAssessment.Save_Assessment_Item_RelatedDCIds(_id, _objAssessment.AssessmentItemID.ToString() + ",");
                    if (res == false)
                    {
                        Set_Error_Message("Assessment Item saved successfully but display condition has thrown error! Please update the display condition and try again later.");
                        return;
                    }
                }
            }

            string scr = string.Empty;
            scr = @"$('#choice_header').goTo();";
            //+ string.Format("$('#{0}').addClass('disabledbutton');"
            if (_objAssessment.TypeID != Convert.ToInt32(Constants.enAssessmentItemType.PAGE_BREAK))
            {
                if (Page.IsPostBack == false)
                {
                    this.Page.ClientScript.RegisterClientScriptBlock(pnlAI.GetType(), "InitDTgrid" + Guid.NewGuid().ToString(), scr, true);
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(pnlAI, pnlAI.GetType(), "PBInitDTGrid" + Guid.NewGuid().ToString(), scr, true);
                }
                pnlChoice.Visible = true;
                AssessmentItemID = _objAssessment.AssessmentItemID;
                Prepare_Choice_Section();

            }
            Set_Success_Message("Question is saved successfully. You can add the choices.");
        }

        private bool Validate_Data(ref string pErrMsg)
        {
            int _temp = -1;
            if (string.IsNullOrEmpty(txtText.Text))
            {
                pErrMsg = "Text can not be empty!";
                txtText.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtOrder.Text))
            {
                pErrMsg = "Order can not be empty!";
                txtOrder.Focus();
                return false;
            }
            else if (!int.TryParse(txtOrder.Text, out _temp))
            {
                pErrMsg = "Input value is not recognized as a valid number!";
                txtOrder.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(cmbType.SelectedValue))
            {
                pErrMsg = "Type can not be empty!";
                cmbType.Focus();
                return false;
            }

            return true;
        }

        protected void btnSaveChoice_Click(object sender, EventArgs e)
        {
            DataAccess.AssessmentItemXChoice objXChoice = new DataAccess.AssessmentItemXChoice();
            objXChoice.AssessmentItemID = AssessmentItemID;
            objXChoice.IsDefault = chk_ChoiceIsDefault.Checked;
            objXChoice.Order = Convert.ToInt32(txtChoiceOrder.Text);
            objXChoice.ScoreValue = Convert.ToInt32(txtScoreValue.Text);
            objXChoice.Value = txtChoicevalue.Text;

            DataAccess.AssessmentItemChoice objChoice = null;
            if (gIsMultipleChoice)
            {
                objChoice = new AssessmentItemChoice()
                {
                    Text = txtChoicevalue.Text
                };
            }
            bool res = gAssessment.Save_Assessment_Item_X_Choice(objXChoice, objChoice);
            if (res == false)
                Set_Error_Message("An error occured during assessment item X choice save process");
            else
            {
                ListChoice.Add(objXChoice);
                Prepare_Choice_Section();

            }
        }

        private void Create_Choice_Grid()
        {
            StringBuilder sb = new StringBuilder();
            List<DataAccess.AssessmentItemXChoice> listAdded = new List<DataAccess.AssessmentItemXChoice>();
            foreach (DataAccess.AssessmentItemXChoice item in ListChoice)
            {
                DataAccess.AssessmentItemXChoice added_obj = listAdded.Find(o => o.Value == item.Value && o.ScoreValue == item.ScoreValue);
                List<DataAccess.AssessmentItemXChoice> listFound = ListChoice.FindAll(o => o.Value == item.Value && o.ScoreValue == item.ScoreValue);

                if (added_obj != null && listFound != null && listFound.Count > 1)
                    continue;

                sb.Append("<tr>");
                sb.Append("<th scope='row'>");
                sb.Append(item.Order.ToString());
                sb.Append("</th>");
                sb.Append("<td>");
                sb.Append(CommonUtilities.EditGridText(item.Value));
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(item.ScoreValue.ToString());
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(item.IsDefault.ToString());
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(@"<a href=javascript:DeleteItem('" + CommonUtilities.GlobalEncrypt(item.ID.ToString(), BSWSession.SessionKey) + "'); class='fa fa-remove' style='color:red;' ></a>");
                sb.Append("</td>");
                sb.Append("</tr>");
                listAdded.Add(item);
            }
            grdChoiceBody.InnerHtml = sb.ToString();
        }

        protected void btnDeleteChoice_Click(object sender, EventArgs e)
        {
            string _val = hidChoice2DeleteID.Value;
            _val = CommonUtilities.GlobalDecrypt(_val, BSWSession.SessionKey);
            int _id = -1;
            if (int.TryParse(_val, out _id))
            {
                bool res = gAssessment.DeleteAssessmentItemXChoice(_id);
                if (res)
                {
                    Prepare_Choice_Section();
                    Set_Success_Message("Deleted Successfully");
                }
                else
                    Set_Error_Message("The item could not be deleted!");
            }
            else
                Set_Error_Message("Item can not be found!");
        }

        protected void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIfTypeIsMultipleChoice();
        }

        protected void btnAddGridChoice_Click(object sender, EventArgs e)
        {
            foreach (var item in ListSubAI)
            {
                DataAccess.AssessmentItemXChoice objXChoice = new DataAccess.AssessmentItemXChoice()
                {
                    AssessmentItemID = item.AssessmentItemID,
                    IsDefault = chk_ChoiceIsDefault.Checked,
                    Order = Convert.ToInt32(txtChoiceOrder.Text),
                    ScoreValue = Convert.ToInt32(txtScoreValue.Text),
                    Value = txtChoicevalue.Text
                };
                DataAccess.AssessmentItemChoice objChoice = new AssessmentItemChoice()
                {
                    Text = txtChoicevalue.Text
                };
                objXChoice.AssessmentItemChoice = objChoice;
                ListChoice.Add(objXChoice);
            }
            divSaveAll.Visible = true;
            Prepare_Choice_Section();
        }

        protected void btnSaveSubAssessmentItem_Click(object sender, EventArgs e)
        {
            string pErrMsg = string.Empty;
            if (!Validate_Data(ref pErrMsg))
            {
                Set_Error_Message(pErrMsg);
                return;
            }

            if (ListSubAI.Find(o => o.Order == Convert.ToInt32(txtSubOrder.Text)) != null)
            {
                Set_Error_Message("Another item with the same order exists! Please swap the records");
                return;
            }

            DataAccess.AssessmentItem _objAssessment = new DataAccess.AssessmentItem();
            _objAssessment.Version = 1;
            _objAssessment.AssessmentItemTreeID = gAssessment.GetNextAssessmentItemTreeID();

            _objAssessment.AssessmentID = gAssessmentID;
            _objAssessment.DisplayCondition = "";
            _objAssessment.HelpText = txtSubHelpText.Text;
            _objAssessment.Text = txtSubText.Text;
            _objAssessment.Order = Convert.ToInt32(txtSubOrder.Text);
            _objAssessment.TypeID = Convert.ToInt32(cmbType.SelectedValue);
            _objAssessment.IsMandatory = false;
            _objAssessment.IsPageBreak = false;
            _objAssessment.IsLatest = true;
            _objAssessment.ParentAssessmentID = AssessmentItemID;
            bool res = gAssessment.Save_Assessment_Item(_objAssessment);
            if (res == false)
            {
                Set_Error_Message("An error oocured during assessment item save process");
                return;
            }
            ListSubAI.Add(_objAssessment);
            divGridSubItem_Grid.Visible = true;
            Create_SubAI_Grid();
            divChoice.Visible = true;
        }

        private bool Validate_Sub_Data(ref string pErrMsg)
        {
            int _temp = -1;
            if (string.IsNullOrEmpty(txtSubText.Text))
            {
                pErrMsg = "Text can not be empty!";
                txtSubText.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtSubOrder.Text))
            {
                pErrMsg = "Order can not be empty!";
                txtSubOrder.Focus();
                return false;
            }
            else if (!int.TryParse(txtSubOrder.Text, out _temp))
            {
                pErrMsg = "Input value is not recognized as a valid number!";
                txtSubOrder.Focus();
                return false;
            }

            return true;
        }

        private void Create_SubAI_Grid()
        {
            StringBuilder sb = new StringBuilder();
            foreach (DataAccess.AssessmentItem item in ListSubAI)
            {
                sb.Append("<tr>");
                sb.Append("<th scope='row'>");
                sb.Append(item.Order.ToString());
                sb.Append("</th>");
                sb.Append("<td>");
                sb.Append(CommonUtilities.EditGridText(item.Text));
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(CommonUtilities.EditGridText(item.HelpText));
                sb.Append("</td>");
                sb.Append("<td>");
                sb.Append(@"<a href=javascript:DeleteAIItem('" + CommonUtilities.GlobalEncrypt(item.AssessmentItemID.ToString(), BSWSession.SessionKey) + "'); class='fa fa-remove' style='color:red;' ></a>");
                sb.Append("</td>");
                sb.Append("</tr>");
            }
            grdSubAI.InnerHtml = sb.ToString();
        }

        protected void btnSaveAll_Click(object sender, EventArgs e)
        {
            bool _res = gAssessment.Save_List_Assessment_Item_X_Choice(ListChoice);
            if (_res == false)
            {
                Set_Error_Message("An error oocured during assessment item save process");
                return;
            }
            Set_Message_On_Another_Page("Assessment Item saved successfully.", "AssessmentItemList.aspx" + EncryptQueryString("AssessmentID=" + gAssessmentID), false);
        }

        protected void btnAddTheItem_Click(object sender, EventArgs e)
        {
            StringBuilder item_formula = new StringBuilder();
            item_formula.AppendFormat("AIID_{0}", cmbAssessmentItems.SelectedValue);
            item_formula.AppendFormat(" {0} ", cmbOperator.SelectedValue);
            ListSelectedAssessmentItems.Add(cmbAssessmentItems.SelectedValue);
            if (cmbControl.Visible)
            {
                if (!string.IsNullOrEmpty(cmbControl.SelectedValue))
                    item_formula.AppendFormat(" {0} ", cmbControl.SelectedValue);
                else
                    item_formula.AppendFormat(" \"{0}\" ", cmbControl.SelectedItem.Text);
            }
            else
            {
                item_formula.AppendFormat(" \"{0}\" ", txtControl.Text);
            }
            string scr = "Add2Box('" + item_formula.ToString() + "')";
            if (Page.IsPostBack == false)
            {
                this.Page.ClientScript.RegisterClientScriptBlock(pnlAI.GetType(), "GenerateScript" + Guid.NewGuid().ToString(), scr, true);
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(pnlAI, pnlAI.GetType(), "GenerateScript" + Guid.NewGuid().ToString(), scr, true);
            }
        }

        protected void cmbControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            int _cId = -1;
            if (int.TryParse(cmbControl.SelectedValue, out _cId))
            {
                ArrageUIEnableDisable();
                if (_cId == -1)
                    return;
            }
            else
                ArrageUIEnableDisable();
        }

        protected void cmbOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            int _cId = -1;
            if (int.TryParse(cmbOperator.SelectedValue, out _cId))
            {
                ArrageUIEnableDisable();
                if (_cId == -1)
                    return;
            }
            else
                ArrageUIEnableDisable();
        }

        protected void cmbAssessmentItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            int _aiId = -1;
            if (int.TryParse(cmbAssessmentItems.SelectedValue, out _aiId))
            {
                ArrageUIEnableDisable();
                if (_aiId == -1)
                    return;
                DataAccess.AssessmentItem obj = gAssessment.GetAssessmentItemByItemIDFromCache(_aiId);
                Constants.enAssessmentItemType _val_type = (Constants.enAssessmentItemType)Convert.ToInt32(obj.TypeID);
                if (_val_type == Constants.enAssessmentItemType.TEXT)
                {
                    divCombo.Visible = false;
                    divText.Visible = true;
                }
                else if (_val_type == Constants.enAssessmentItemType.DATETIME)
                {
                    divCombo.Visible = false;
                    divText.Visible = true;
                    txtControl.TextMode = TextBoxMode.DateTime;
                }
                else if (_val_type == Constants.enAssessmentItemType.INTEGER)
                {
                    divCombo.Visible = false;
                    divText.Visible = true;
                    txtControl.TextMode = TextBoxMode.Number;
                }
                else if (_val_type == Constants.enAssessmentItemType.TRUE_FALSE)
                {
                    divCombo.Visible = true;
                    divText.Visible = false;
                    cmbControl.Items.Add(new ListItem("false", "0"));
                    cmbControl.Items.Add(new ListItem("true", "1"));
                }
                else
                {
                    divCombo.Visible = true;
                    divText.Visible = false;
                    cmbControl.DataTextField = nameof(DataAccess.AssessmentItemXChoice.Value);
                    cmbControl.DataValueField = nameof(DataAccess.AssessmentItemXChoice.ChoiceID);
                    cmbControl.DataSource = gAssessment.GetAssessmentItemChoicesByItemIDFromCache(_aiId, false);
                    cmbControl.DataBind();
                }
                if (cmbControl.Items.FindByValue("-1") == null)
                    cmbControl.Items.Insert(0, new ListItem("Please select...", "-1"));
            }
            else
                ArrageUIEnableDisable();
        }
    }
}