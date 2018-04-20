using BSW.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BSW.Web.UserControls;
using System.Text;
using BSW.DataAccess;
using System.Web.UI.HtmlControls;
using BSW.BusinessRules;

namespace SP.Web.AssessmentModule
{
    public partial class AssessmentList : BasePage
    {
        private BusinessRules.OP_Assessment gAssessment;
        private BusinessRules.OP_LookUp gLookUp;
        private bool gIsAdministrator = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            Operation_ID = OP_Operation.OP_ASSESSMENT_LIST;
            Initialize();
            if (Page.IsPostBack == false)
                BeforePostback();
            else
                AfterPostback();
        }

        protected override void Initialize()
        {
            base.Initialize();
            gAssessment = new BusinessRules.OP_Assessment(BSWSession.SessionID, BSWSession.UserID);
            gLookUp = new BusinessRules.OP_LookUp(BSWSession.SessionID, BSWSession.UserID);

            arrangeDisplayOptionText();
            gIsAdministrator = !isUser();

            if (string.IsNullOrEmpty(this.SortBy))
                this.SortBy = nameof(DataAccess.Assessment.AssessmentID);

            if (gIsPopup)
                grd.AddHyperLinkColumn("", "", FormatSelectRow);
            else
                grd.AddDataColumn("", nameof(DataAccess.Assessment.AssessmentID), PrepareMenu);

            grd.AddHiddenDataColumn("AssessmentID", nameof(DataAccess.Assessment.AssessmentID));
            grd.AddHyperLinkColumn("Name", nameof(DataAccess.Assessment.Name));
            grd.AddHyperLinkColumn("Type", nameof(DataAccess.Assessment.TypeID), FormatTypeColumn);
            grd.AddDataColumn("Version", nameof(DataAccess.Assessment.Version));
            grd.AddHyperLinkColumn("Author", nameof(DataAccess.Assessment.Author));

            if (IsUserAllowed(OP_Operation.OP_ASSESSMENT_CREATE))
                btnAddNewAssessmentItem.Visible = true;
            else
                btnAddNewAssessmentItem.Visible = false;
        }

        private string FormatSelectRow(string FieldName, DataRow row, clsExtraStyle pObjES)
        {
            string select = "<a href='#' onclick='selectRow(this)' data-text='" + row[nameof(DataAccess.Assessment.Name)] + "' data-id='" + row[nameof(DataAccess.Assessment.AssessmentID)] + "' >";
            select += "<i class='fa fa-arrow-left' aria-hidden='true'></i>&nbsp;Select";
            select += "</a>";

            return select;
        }

        private string PrepareMenu(string FieldName, DataRow row, clsExtraStyle pObjES)
        {
            string assessment_id = row[nameof(DataAccess.Assessment.AssessmentID)].ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='dropdown'>");
            sb.Append("<a href='javascript: void(0);' class='dropdown-toggle' data-toggle='dropdown' aria-expanded='false'></a>");
            sb.Append("<ul class='dropdown-menu' aria-labelledby='' role='menu'>");
            sb.AppendFormat("<a class='dropdown-item' href='{0}'><i class='dropdown-icon {1}'></i> {2}</a>", "Assessment.aspx" + EncryptQueryString("ID=" + assessment_id), "fa fa-search", "View");
            if (IsUserAllowed(OP_Operation.OP_ASSESSMENT_UPDATE))
                sb.AppendFormat("<a class='dropdown-item' href='{0}'><i class='dropdown-icon {1}'></i> {2}</a>", "Assessment.aspx" + EncryptQueryString("ID=" + assessment_id + "&PageMode=" + Convert.ToInt32((Constants.enPageMode.UPDATE_DATA)).ToString()), "fa fa-edit", "Update");

            if (IsUserAllowed(OP_Operation.OP_ASSESSMENT_ITEM_LIST))
                sb.AppendFormat("<a class='dropdown-item' href='{0}'><i class='dropdown-icon {1}'></i> {2}</a>", "AssessmentItemList.aspx" + EncryptQueryString("AssessmentID=" + assessment_id), "fa fa-check-square", "View Items");

            sb.AppendFormat("<a class='dropdown-item' href='{0}'><i class='dropdown-icon {1}'></i> {2}</a>", "AssessmentViewer.aspx" + EncryptQueryString("AssessmentID=" + assessment_id), "fa fa-pencil", "Take Assessment");
            return sb.ToString();
        }

        private string FormatTypeColumn(string FieldName, DataRow row, clsExtraStyle pObjES)
        {
            int type_id = -1;
            int.TryParse(row[FieldName].ToString(), out type_id);
            if (type_id > 0)
                return gLookUp.GetAssesmentTypeFromCache(type_id).Description;

            return "";
        }

        protected override void AfterPostback()
        {
            base.AfterPostback();
        }

        protected override void BeforePostback()
        {
            base.BeforePostback();
            BindGrid();
        }

        private void BindGrid()
        {
            int total_count = 0;
            List<DataAccess.Assessment> listAssessments = BusinessRules.OP_Assessment.GetAssessmentsFromCache(true);

            if (listAssessments == null)
                return;


            if (gIsAdministrator && !(Request.QueryString["user"] == "1"))
            {
                btnAddNewAssessmentItem.Visible = true;
                grd.Visible = true;
                btnOptDisplay.Visible = true;
                divCardViewer.Visible = false;
                grd.TotalCount = total_count;
                grd.DataSource = listAssessments.ToDataTable();
            }
            else
            {
                btnAddNewAssessmentItem.Visible = false;
                grd.Visible = false;
                if (!(Request.QueryString["user"] ==  "1"))
                    btnOptDisplay.Visible = false;
                else
                    btnOptDisplay.Visible = true;
                divCardViewer.Visible = true;
                Create_Card_Viewer(listAssessments);
            }
        }

        private void Create_Card_Viewer(List<DataAccess.Assessment> listAssessments)
        {
            foreach (var item in listAssessments)
            {
                int _AssessmentID = item.AssessmentID;

                int _total_score = -1;

                HtmlGenericControl divContainer = new HtmlGenericControl("div");
                divContainer.Attributes.Add("class", "col-xl-3 col-lg-4 col-md-6 col-sm-12 pull-left");
                HtmlGenericControl divContainerChild = new HtmlGenericControl("div");
                divContainerChild.Attributes.Add("class", "cat__ecommerce__catalog__item");

                //cat__ecommerce__catalog__item__status
                HtmlGenericControl divARImage = new HtmlGenericControl("div");
                divARImage.Attributes.Add("class", "cat__ecommerce__catalog__item__img");
                HtmlGenericControl divARStatus = new HtmlGenericControl("div");
                HtmlGenericControl spanARStatus = new HtmlGenericControl("span");
                spanARStatus.Attributes.Add("class", "cat__ecommerce__catalog__item__status__title");
                List<HtmlGenericControl> listBtns = new List<HtmlGenericControl>();
                //resp exists  <div class="cat__ecommerce__catalog__item__status">
                List<DataAccess.Assessment> listOtherVersios = gAssessment.GetAssessmentsByTreeIDFromCache(item.AssessmentTreeID.Value);
                List<AssessmentResponse> listAssessmentResponse = new List<AssessmentResponse>();
                if (listOtherVersios != null && listOtherVersios.Count > 1)
                {
                    foreach (var item_older in listOtherVersios)
                    {
                        List<AssessmentResponse> listAssessmentResponse_temp = gAssessment.GetAssessmentResponsesByUserAndAssessmentID(BSWSession.UserID, item_older.AssessmentID);
                        if (listAssessmentResponse_temp != null && listAssessmentResponse_temp.Count > 0)
                            listAssessmentResponse.AddRange(listAssessmentResponse_temp);
                    }
                }
                else
                    listAssessmentResponse = gAssessment.GetAssessmentResponsesByUserAndAssessmentID(BSWSession.UserID, _AssessmentID);

                if (listAssessmentResponse != null && listAssessmentResponse.Count > 0)
                {
                    bool is_crrt = false;
                    bool is_have_history = false;
                    HtmlGenericControl objDDLMenu = CreateDDLMenuAI();
                    HtmlGenericControl divbtngrp = new HtmlGenericControl("div");
                    divbtngrp.Attributes.Add("class", "btn-group pull-right ");
                    divbtngrp.Attributes.Add("style", "padding-right:4px; ");
                    divbtngrp.Attributes.Add("role", "group");
                    HtmlGenericControl objBtn_t = new HtmlGenericControl("a");
                    objBtn_t.Attributes.Add("class", "btn btn-icon btn-outline-success dropdown-toggle");
                    objBtn_t.Attributes.Add("data-toggle", "dropdown");
                    objBtn_t.Attributes.Add("aria-expanded", "false");
                    objBtn_t.InnerHtml = "<i class='fa fa-search' style='color: #46be8a;' ></i>";
                    HtmlGenericControl uldivbtngrp = new HtmlGenericControl("ul");
                    uldivbtngrp.Attributes.Add("class", "dropdown-menu");

                    foreach (var itemAR in listAssessmentResponse)
                    {
                        _AssessmentID = itemAR.AssessmentID;
                        if (listAssessmentResponse.Count == 1)
                        {
                            HtmlGenericControl objBtn_t1 = new HtmlGenericControl("a");
                            HtmlGenericControl ctrl_i_t = new HtmlGenericControl("i");
                            ctrl_i_t.Attributes.Add("class", itemAR.IsCurent ? "fa fa-pencil" : "fa fa-search");
                            objBtn_t1.Attributes.Add("class", "btn btn-icon btn-outline-success mr-2 mb-2 pull-right");
                            objBtn_t1.Attributes.Add("href", "AssessmentViewer.aspx" + EncryptQueryString("AssessmentID=" + _AssessmentID.ToString() + "&AssessmentRespID=" + itemAR.ID.ToString()));
                            objBtn_t1.Controls.Add(ctrl_i_t);
                            listBtns.Add(objBtn_t1);
                            if (!itemAR.IsCurent)
                            {
                                objBtn_t1 = new HtmlGenericControl("a");
                                ctrl_i_t = new HtmlGenericControl("i");
                                ctrl_i_t.Attributes.Add("class", "fa fa-pencil");
                                objBtn_t1.Attributes.Add("class", "btn btn-icon btn-outline-success mr-2 mb-2 pull-right");
                                objBtn_t1.Attributes.Add("href", "AssessmentViewer.aspx" + EncryptQueryString("AssessmentID=" + _AssessmentID.ToString()));
                                objBtn_t1.Controls.Add(ctrl_i_t);
                                listBtns.Add(objBtn_t1);
                            }
                            else
                            {
                                is_crrt = true;
                                divARStatus.Attributes.Add("class", "cat__ecommerce__catalog__item__status continue-color");
                                spanARStatus.InnerHtml = "CNT";
                            }
                        }
                        else
                        {
                            if (itemAR.Equals(listAssessmentResponse.First()))
                            {
                                HtmlGenericControl objBtn_t1 = new HtmlGenericControl("a");
                                HtmlGenericControl ctrl_i_t1 = new HtmlGenericControl("i");


                                if (!itemAR.IsCurent)
                                {
                                    objBtn_t1 = new HtmlGenericControl("a");
                                    ctrl_i_t1 = new HtmlGenericControl("i");
                                    ctrl_i_t1.Attributes.Add("class", "fa fa-refresh");
                                    objBtn_t1.Attributes.Add("class", "btn btn-icon btn-outline-success mr-2 mb-2 pull-right");
                                    objBtn_t1.Attributes.Add("href", "AssessmentViewer.aspx" + EncryptQueryString("AssessmentID=" + _AssessmentID.ToString()));
                                    objBtn_t1.Controls.Add(ctrl_i_t1);
                                    listBtns.Add(objBtn_t1);
                                }
                                else
                                {
                                    ctrl_i_t1.Attributes.Add("class", itemAR.IsCurent ? "fa fa-pencil" : "fa fa-search");
                                    objBtn_t1.Attributes.Add("class", "btn btn-icon btn-outline-success mr-2 mb-2 pull-right");
                                    objBtn_t1.Attributes.Add("href", "AssessmentViewer.aspx" + EncryptQueryString("AssessmentID=" + _AssessmentID.ToString() + "&AssessmentRespID=" + itemAR.ID.ToString()));
                                    objBtn_t1.Controls.Add(ctrl_i_t1);
                                    listBtns.Add(objBtn_t1);
                                    is_crrt = true;
                                    divARStatus.Attributes.Add("class", "cat__ecommerce__catalog__item__status continue-color");
                                    spanARStatus.InnerHtml = "CNT";
                                    continue;
                                }
                            }

                            HtmlGenericControl objBtn_t_a = new HtmlGenericControl("a");
                            objBtn_t_a.Attributes.Add("class", "dropdown-item");
                            objBtn_t_a.Attributes.Add("href", "AssessmentViewer.aspx" + EncryptQueryString("AssessmentID=" + _AssessmentID.ToString() + "&AssessmentRespID=" + itemAR.ID.ToString()));
                            objBtn_t_a.InnerText = itemAR.AdministerDate.ToShortDateString();
                            HtmlGenericControl ctrl_i_t = new HtmlGenericControl("i");
                            ctrl_i_t.InnerHtml = "<span class='badge badge-pill badge-info mr-2 mb-2 pull-right'>" + itemAR.TotalScore.ToString() + "</span>";
                            objBtn_t_a.Controls.Add(ctrl_i_t);
                            uldivbtngrp.Controls.Add(objBtn_t_a);
                            is_have_history = true;
                        }

                        if (itemAR.IsCurent)
                        {

                        }
                    }
                    if (is_have_history)
                    {
                        divbtngrp.Controls.Add(objBtn_t);
                        divbtngrp.Controls.Add(uldivbtngrp);
                        listBtns.Add(divbtngrp);
                    }

                    if (!is_crrt)
                    {
                        divARStatus.Attributes.Add("class", "cat__ecommerce__catalog__item__status completed-color");
                        spanARStatus.InnerHtml = "DONE";
                        _total_score = listAssessmentResponse[0].TotalScore;
                    }
                }
                else
                {
                    divARStatus.Attributes.Add("class", "cat__ecommerce__catalog__item__status");
                    spanARStatus.InnerHtml = "NEW";
                    HtmlGenericControl objBtn_t = new HtmlGenericControl("a");
                    HtmlGenericControl ctrl_i_t = new HtmlGenericControl("i");
                    ctrl_i_t.Attributes.Add("class", "fa fa-pencil");
                    objBtn_t.Attributes.Add("class", "btn btn-icon btn-outline-success mr-2 mb-2 pull-right");
                    objBtn_t.Attributes.Add("href", "AssessmentViewer.aspx" + EncryptQueryString("AssessmentID=" + _AssessmentID.ToString()));
                    objBtn_t.Controls.Add(ctrl_i_t);
                    listBtns.Add(objBtn_t);
                }
                divARStatus.Controls.Add(spanARStatus);
                divARImage.Controls.Add(divARStatus);

                //TODO - Favouirete Assessment
                HtmlGenericControl divARFavourite = new HtmlGenericControl("div");
                divARFavourite.Attributes.Add("class", "cat__ecommerce__catalog__item__like cat__ecommerce__catalog__item__like");
                if (_total_score > 0)
                    divARFavourite.InnerHtml = "<span class='badge badge-pill badge-success mr-2 mb-2'> " + _total_score.ToString() + " </span>";
                //<span class='badge badge-pill badge-success mr-2 mb-2'>90</span>
                //if(liked) add class --selected
                divARImage.Controls.Add(divARFavourite);

                HtmlGenericControl divARContentItem = new HtmlGenericControl("div");
                divARContentItem.Attributes.Add("class", "cat__ecommerce__catalog__item__price");
                divARContentItem.InnerHtml = item.Name;
                divARImage.Controls.Add(divARContentItem);

                divContainerChild.Controls.Add(divARImage);
                //divARIImage ended

                //content
                HtmlGenericControl divARContent = new HtmlGenericControl("div");
                divARContent.Attributes.Add("class", "cat__ecommerce__catalog__item__title col-sm-12 row");

                HtmlGenericControl divARContent_t = new HtmlGenericControl("div");
                divARContent_t.Attributes.Add("class", "col-sm-6");

                HtmlGenericControl html_a_ARTitle = new HtmlGenericControl("a");
                html_a_ARTitle.Attributes.Add("href", "javascript:void(0);");
                string _a_type = string.Empty;
                if (item.TypeID > 0)
                    _a_type = gLookUp.GetAssesmentTypeFromCache(item.TypeID).Description;
                if (listAssessmentResponse != null && listAssessmentResponse.Count > 0)
                    html_a_ARTitle.InnerHtml = "<i class='fa fa-calendar' ></i> " + listAssessmentResponse[0].AdministerDate.ToShortDateString();
                else
                    html_a_ARTitle.InnerText = _a_type;

                divARContent_t.Controls.Add(html_a_ARTitle);
                divARContent.Controls.Add(divARContent_t);

                HtmlGenericControl divARContent_t2 = new HtmlGenericControl("div");
                divARContent_t2.Attributes.Add("class", "col-sm-6 ");
                divARContent_t2.Attributes.Add("style", "padding-right: 0 !important; margin-top: -8px !important;");

                foreach (var itemBtn in listBtns)
                {
                    divARContent_t2.Controls.Add(itemBtn);
                }
                divARContent.Controls.Add(divARContent_t2);

                divContainerChild.Controls.Add(divARContent);

                divContainer.Controls.Add(divContainerChild);
                divCardViewer.Controls.Add(divContainer);
            }
        }

        private HtmlGenericControl CreateDDLMenuAI()
        {
            HtmlGenericControl divbtngrp = new HtmlGenericControl("div");
            divbtngrp.Attributes.Add("class", "btn-group");
            divbtngrp.Attributes.Add("role", "group");
            HtmlGenericControl objBtn_t = new HtmlGenericControl("a");
            objBtn_t.Attributes.Add("class", "btn btn-icon btn-outline-success mr-2 mb-2 pull-right dropdown-toggle");
            objBtn_t.Attributes.Add("data-toggle", "dropdown");
            objBtn_t.Attributes.Add("aria-expanded", "false");
            HtmlGenericControl uldivbtngrp = new HtmlGenericControl("ul");
            uldivbtngrp.Attributes.Add("class", "dropdown-menu");
            objBtn_t.Controls.Add(uldivbtngrp);
            divbtngrp.Controls.Add(objBtn_t);
            return divbtngrp;
        }

        protected void btnAddNewAssessmentItem_Click(object sender, EventArgs e)
        {
            Response.Redirect("Assessment.aspx");
        }

        protected void btnOptDisplay_Click(object sender, EventArgs e)
        {
            if (isUser())
            { // if current status is user, redirect admin
                Response.Redirect("AssessmentList.aspx");
            }
            else
            {
                Response.Redirect("AssessmentList.aspx?user=1");

            }
        }

        private void arrangeDisplayOptionText()
        {
            if (isUser())
            {
                btnOptDisplay.Text = "Display as Admin";
            }
            else
            {
                btnOptDisplay.Text = "Display as User";
            }


        }

        private bool isUser()
        {
            if ((IsUserAllowed(OP_Operation.OP_ASSESSMENT_CREATE) || IsUserAllowed(OP_Operation.OP_ASSESSMENT_DELETE) || IsUserAllowed(OP_Operation.OP_ASSESSMENT_UPDATE)) && Request.QueryString["user"] != "1")
                return false;
            else
                return true;
        }
    }
}