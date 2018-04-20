using BSW.BusinessRules;
using BSW.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SP.Web.AssessmentModule
{
    public partial class Assessment : BasePage
    {
        private int gAssessmentID;
        private BusinessRules.OP_Assessment gAssessment_br;
        private BusinessRules.OP_LookUp gLookup_br;
        private Constants.enPageMode gPageMode;

        protected void Page_Load(object sender, EventArgs e)
        {
            Initialize();
            if (Page.IsPostBack == false)
                BeforePostback();
            else
                AfterPostback();
        }

        protected override void AfterPostback()
        {
            base.AfterPostback();
        }

        protected override void BeforePostback()
        {
            base.BeforePostback();
            BindControlsToDataSources();
            ArrangeUI();
        }

        private void ArrangeUI()
        {
            if (gPageMode != Constants.enPageMode.NEW_DATA)
            {
                DataAccess.Assessment objAssessment = gAssessment_br.GetAssessmentByIDFromCache(gAssessmentID);
                txtName.Text = objAssessment.Name;
                txtIntroduction.Text = objAssessment.Introduction;
                string[] arr_version = objAssessment.Version.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (arr_version.Length == 3)
                {
                    txtMajorVersion.Text = arr_version[0];
                    txtMinorVersion.Text = arr_version[1];
                    txtBuildNumber.Text = arr_version[2];
                }
                txtAuthor.Text = objAssessment.Author;
                cmbType.Value = objAssessment.TypeID.ToString();
                cmbProgram.Value = objAssessment.ProgramID.ToString();
                if (gPageMode == Constants.enPageMode.VIEW_DATA)
                {
                    DisableAllPage("divControls");
                    btnSaveAssessment.Visible = false;
                }

            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            gAssessment_br = new BusinessRules.OP_Assessment(BSWSession.SessionID, BSWSession.UserID);
            gLookup_br = new BusinessRules.OP_LookUp(BSWSession.SessionID, BSWSession.UserID);
            try
            {
                gAssessmentID = Convert.ToInt32(this.DecryptQueryString(Request.QueryString["value"], "ID"));
            }
            catch (Exception)
            {
                gAssessmentID = -1;
            }

            try
            {
                gPageMode = (Constants.enPageMode)Convert.ToInt32(this.DecryptQueryString(Request.QueryString["value"], "PageMode"));
            }
            catch (Exception)
            {
                if (gAssessmentID > 0)
                    gPageMode = Constants.enPageMode.VIEW_DATA;
                else
                    gPageMode = Constants.enPageMode.NEW_DATA;
            }
            if (gPageMode == Constants.enPageMode.NEW_DATA && !IsUserAllowed(OP_Operation.OP_ASSESSMENT_CREATE))
                ThrowNotAuthorized();

            if (gPageMode == Constants.enPageMode.UPDATE_DATA && !IsUserAllowed(OP_Operation.OP_ASSESSMENT_UPDATE))
            {
                DisableAllPage("divControls");
                btnSaveAssessment.Visible = false;
            }

        }

        protected override void BindControlsToDataSources()
        {
            base.BindControlsToDataSources();
            cmbType.DataTextField = "Description";
            cmbType.DataValueField = "ID";
            cmbType.DataSource = gLookup_br.GetAssessmentTypesFromCache();
            cmbType.DataBind();

            cmbProgram.DataTextField = "Name";
            cmbProgram.DataValueField = "ID";
            cmbProgram.DataSource = gAssessment_br.Get_Program_List_From_Cache();
            cmbProgram.DataBind();
        }

        protected void btnSaveAssessment_Click(object sender, EventArgs e)
        {
            bool is_version_update = false;
            if (!Validate_Data())
                return;
            DataAccess.Assessment objAssessment;
            if (gPageMode == Constants.enPageMode.NEW_DATA)
                objAssessment = new DataAccess.Assessment();
            else
            {
                objAssessment = gAssessment_br.GetAssessmentByIDFromCache(gAssessmentID);
                string[] arr_version = objAssessment.Version.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (arr_version.Length == 3)
                {
                    if (!txtMajorVersion.Text.Equals(arr_version[0]) || txtMinorVersion.Text.Equals(arr_version[1]))
                        is_version_update = true;

                }
            }

            objAssessment.Name = txtName.Text;
            objAssessment.Introduction = txtIntroduction.Text;
            objAssessment.Version = txtMajorVersion.Text + "." + txtMinorVersion.Text + "." + txtBuildNumber.Text;
            objAssessment.Author = txtAuthor.Text;
            objAssessment.TypeID = Convert.ToInt32(cmbType.Value);
            objAssessment.ProgramID = Convert.ToInt32(cmbProgram.Value);
            if (gPageMode == Constants.enPageMode.NEW_DATA)
                objAssessment.AssessmentTreeID = gAssessment_br.GetNextAssessmentTreeID();

            bool res = gAssessment_br.Save_Assessment(objAssessment, is_version_update);
            if (res)
                Set_Message_On_Another_Page("Assessment saved successfully", "AssessmentList.aspx", false);
            else
                Set_Error_Message("An error occured during save process");
        }

        private bool Validate_Data()
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                Set_Error_Message("Name can not be empty");
                txtName.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtIntroduction.Text))
            {
                Set_Error_Message("Introduction can not be empty");
                txtIntroduction.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(cmbType.Value))
            {
                Set_Error_Message("Type can not be empty");
                cmbType.Focus();
                return false;
            }
            else
            {
                int _temp = -1;
                if (int.TryParse(cmbType.Value, out _temp) == false)
                {
                    Set_Error_Message("Type input is not valid");
                    cmbType.Focus();
                    return false;
                }
            }
            if (string.IsNullOrEmpty(cmbProgram.Value))
            {
                Set_Error_Message("Program can not be empty");
                cmbProgram.Focus();
                return false;
            }
            else
            {
                int _temp = -1;
                if (int.TryParse(cmbProgram.Value, out _temp) == false)
                {
                    Set_Error_Message("Program input is not valid");
                    cmbProgram.Focus();
                    return false;
                }
            }
            return true;
        }
    }
}