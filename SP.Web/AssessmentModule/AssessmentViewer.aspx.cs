using BSW.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BSW.BusinessRules;
using System.Text;
using BSW.DataAccess;
using System.Web.UI.HtmlControls;

namespace SP.Web.AssessmentModule
{
    public partial class AssessmentViewer : BasePage
    {
        private BusinessRules.OP_Assessment gAssessment_br;
        private BusinessRules.OP_LookUp gLookUp;
        public int gAssessmentID;
        public int gAssessmentRespID;
        private List<DataAccess.AssessmentItemResponse> listResponses;
        private bool gIsPagingEnabled;
        private bool gIsAssessmentClosed;


        protected void Page_Load(object sender, EventArgs e)
        {
            Initialize();
            if (gAssessmentID == -1)
            {
                Set_Message_On_Another_Page("Assessment ID could not be found!", "AssessmentList.aspx", true);
                return;
            }
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
                return;
            }

            try
            {
                gAssessmentRespID = Convert.ToInt32(this.DecryptQueryString(Request.QueryString["value"], "AssessmentRespID"));
            }
            catch (Exception)
            {
                gAssessmentRespID = -1;
            }


            gAssessment_br = new BusinessRules.OP_Assessment(BSWSession.SessionID, BSWSession.UserID);
            gLookUp = new BusinessRules.OP_LookUp(BSWSession.SessionID, BSWSession.UserID);
          

            listResponses = new List<AssessmentItemResponse>();
            if (gAssessmentRespID > 0)
            {
                //load old responses
                AssessmentResponse objResp = gAssessment_br.GetAssessmentResponse(BSWSession.UserID, gAssessmentRespID, null);

                listResponses = OP_Assessment.GetItemResponsesByAssessmentRespID(gAssessmentRespID);
                gIsAssessmentClosed = !objResp.IsCurent;

                if (gIsAssessmentClosed)
                    btnDeleteAssessmentResponse.Visible = true;
            }
            else
                gIsAssessmentClosed = false;
        }

        protected override void AfterPostback()
        {
            base.AfterPostback();
        }

        protected override void BeforePostback()
        {
            base.BeforePostback();
            BindAssessment();
            BindAssessmentItems();
        }

        private void BindAssessmentItems()
        {
            StringBuilder sb = new StringBuilder();
            List<BusinessRules.ClsListAssessmentItem> listItems = gAssessment_br.GetAssessmentItemListFromCache(gAssessmentID);
            if (listItems == null || listItems.Count == 0)
            {
                Set_Error_Message("This assessment has no items!");
                return;
            }
            listItems = listItems.OrderBy(o => o.Order).ToList();
            List<BusinessRules.ClsListAssessmentItem> listPageBreaks = listItems.FindAll(o => o.TypeID == Convert.ToInt32(Constants.enAssessmentItemType.PAGE_BREAK));
            if (listPageBreaks != null && listPageBreaks.Count > 0)
            {
                gIsPagingEnabled = true;
                CreateAssessmentWizard(listPageBreaks, ref sb);
                divWizard.Visible = true;
                divAssessmentItems.Visible = false;
            }
            else
            {
                divWizard.Visible = false;
                gIsPagingEnabled = false;
            }
            CreateAssessment(listItems);
        }

        private void CreateAssessment(List<ClsListAssessmentItem> listItems)
        {
            StringBuilder sb = new StringBuilder();
            // StringBuilder sb_toc = new StringBuilder();
            //TOC
            List<ClsListAssessmentItem> list_addedsubItems = new List<ClsListAssessmentItem>();

            List<string> listDivWizards = new List<string>();
            List<AssessmentItemResponse> listAIR = null;
            if(gIsAssessmentClosed)
            {
                listAIR = OP_Assessment.GetItemResponsesByAssessmentRespID(gAssessmentRespID);
            }

            foreach (ClsListAssessmentItem item in listItems)
            {
                if (list_addedsubItems.Contains(item))
                    continue;
                ClsListAssessmentItem _item = null;
                if (gIsAssessmentClosed && listAIR !=null)
                {
                    List<int> listAllAIds = gAssessment_br.GetAssessmentItemIDsByTreeID(item.AITreeID);
                    if(listAllAIds != null & listAllAIds.Count>0)
                    {
                        AssessmentItemResponse objAIR = listAIR.Find(o => listAllAIds.Contains(o.AssessmentItemID));
                        if(objAIR != null)
                        {
                            _item = ClsListAssessmentItem.ConvertAssessmentItemToClass(gAssessment_br.GetAssessmentItemByItemIDFromCache(objAIR.AssessmentItemID));
                        }
                    }
                }
                if (_item == null)
                    _item = item;
                //gAssessment_br.GetAssessment_itemBy_itemTreeIDFromCache()

                //DataAccess.Assessment_item obj = gAssessment_br.GetAssessment_itemBy_itemIDFromCache(_item._itemID);
                //HtmlGenericControl divTemp = new HtmlGenericControl("div");
                if (_item.TypeID == Convert.ToInt32(Constants.enAssessmentItemType.PAGE_BREAK))
                {
                    string _div = string.Empty;
                    int _id_count = listDivWizards.Count + 1;
                    _div = @"<h3><span class='cat__wizard__steps__title'>" + _id_count.ToString() + "</span></h3><section class=''>" + sb.ToString() + "</section>";
                    //divTemp.Attributes.Add("class", "tab-pane");
                    //divTemp.ID = "tab" + _id_count.ToString();
                    //divTemp.InnerHtml = sb.ToString();
                    //divTemp.ClientIDMode = ClientIDMode.Static;
                    listDivWizards.Add(_div);
                    sb = new StringBuilder();
                    continue;
                }

                sb.Append(CreateAssessmentItem(_item, false));

                if (_item.SubItems.Count > 0)
                {
                    foreach (ClsListAssessmentItem sub_item in _item.SubItems)
                    {
                        if (list_addedsubItems.Contains(sub_item))
                            continue;

                        //obj = gAssessment_br.GetAssessmentItemByItemIDFromCache(sub_item.ItemID);

                        bool is_sub_grid = ((sub_item.TypeID == Convert.ToInt32(Constants.enAssessmentItemType.GRID_CHK) || sub_item.TypeID == Convert.ToInt32(Constants.enAssessmentItemType.GRID_RADIO)) && sub_item.ParentID > 0);

                        // sb_toc.Append(CreateaTOCSub(_item.Order, sub_item.Order));
                        sb.Append(CreateAssessmentItem(sub_item, is_sub_grid, _item.Order, sub_item.Order));

                        if (is_sub_grid)
                            list_addedsubItems.AddRange(_item.SubItems.FindAll(o => o.ParentID == sub_item.ParentID));
                    }
                }
                if (gIsPagingEnabled && _item.Equals(listItems.Last()))
                {
                    string _div = string.Empty;
                    int _id_count = listDivWizards.Count + 1;
                    _div = @"<h3><span class='cat__wizard__steps__title'>" + _id_count.ToString() + "</span></h3><section class=''>" + sb.ToString() + "</section>";
                    listDivWizards.Add(_div);
                }
            }
            //divTOC_Contents.InnerHtml = sb_toc.ToString();
            if (listDivWizards.Count > 0)
            {
                foreach (var item in listDivWizards)
                {
                    divWizardContent.InnerHtml += item;
                }
                string scr = @"$(function () {  $('[id*=divWizardContent]').steps({
                                                    headerTag: 'h3',
                                                    bodyTag: 'section',
                                                    transitionEffect: 'slideLeft',
                                                    autoFocus: true,
                                                    onFinishing: StepsFinishing,
                                                    onFinished: StepsFinished
                                                }); 
$(""[aria-label='Pagination']"").prepend(""<li aria-hidden='false' aria-disabled='false'><a href='AssessmentList.aspx'><i class='fa fa-arrow-left' aria-hidden='true'></i>Back To List</a></li>"");";
                if (gIsAssessmentClosed)
                    scr += @"$(""[aria-label='Pagination']"").children().last().remove();";

                scr += "});";
                ScriptManager.RegisterClientScriptBlock(pnl, pnl.GetType(), "CreateAssessmentItemXChoice" + Guid.NewGuid().ToString(), scr, true);
                divBtnSinglePage.Visible = false;
            }
            else
            {
                divBtnSinglePage.Visible = true;
                divAssessmentItems.InnerHtml = sb.ToString();
            }
        }

        private string CreateaTOC(int order)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<li class='mb-2'>");
            sb.Append("<a href='#item" + order.ToString() + "' class='cat__core__link--blue cat__core__link--underlined'>");
            sb.Append("Question " + order.ToString() + "</a>");
            sb.Append("</li>");

            return sb.ToString();
        }

        private string CreateaTOCSub(int order, int sub_order)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<li class='mb-2'>");
            sb.Append("<a href='#item" + order.ToString() + "_" + sub_order.ToString() + "' class='cat__core__link--blue cat__core__link--underlined'>");
            sb.Append("Question " + order.ToString() + "." + sub_order.ToString() + "</a>");
            sb.Append("</li>");

            return sb.ToString();
        }

        private void CreateAssessmentWizard(List<ClsListAssessmentItem> listPageBreaks, ref StringBuilder sb)
        {
            int counter = 2;
            sb.Append(@"<h3><span class='cat__wizard__steps__title'>1</span></ h3 > ");
            foreach (ClsListAssessmentItem item in listPageBreaks)
            {
                sb.AppendFormat("<li><a href='#tab{0}' data-toggle='tab'>{0}</a></li>", counter.ToString());
                //sb.Append("<h3>");
                //sb.Append("<span class='cat__wizard__steps__title'></span>");
                //sb.Append("</h3>");
                //sb.Append("<section>");
                //sb.Append(divAssessmentItems.InnerHtml);
                //sb.Append("</section>");
                counter++;
            }
            //divWizardHeaders.InnerHtml = sb.ToString();
            //divWizardItems.InnerHtml = sb.ToString();
            //divWizardItems.Visible = true;
        }

        private string CreateAssessmentItem(ClsListAssessmentItem pObj, bool pIs_Sub_Grid, int pParentOrder = -1, int pSubOrder = -1)
        {
            StringBuilder sb = new StringBuilder();
            //question
            string _display = string.Empty;
            string _tooltip = string.Empty;
            string _is_required_symbol = string.Empty;
            bool is_sub = (pObj.ParentID > 0 && pParentOrder > 0 && pSubOrder > 0);
            string header_tag = "<h6";
            string _id_img_postfix = "_";

            if (!string.IsNullOrEmpty(pObj.HelpText))
                _tooltip = " data-toggle='tooltip' data-placement='top' title='" + pObj.HelpText + "' ";

            if (pObj.IsMandatory)
                _is_required_symbol = "<text style='color:red;'>*</text>";

            if (is_sub && !pIs_Sub_Grid)
            {
                header_tag = "<h7";
                _id_img_postfix += pParentOrder.ToString() + "_" + pSubOrder.ToString();
            }
            else if (is_sub && pIs_Sub_Grid)
                _id_img_postfix += pParentOrder.ToString();
            else
                _id_img_postfix += pObj.Order.ToString();


            if (!pIs_Sub_Grid)
            {
                if (!string.IsNullOrEmpty(pObj.DisplayCondition))
                {
                    bool formula_result = false;
                    gAssessment_br.ExecuteFormula(pObj.ItemID, "", ref formula_result);
                    if (!formula_result)
                        _display = " display: none !important; ";

                }
                sb.Append("<hr />");
                sb.AppendFormat("{0} class='mb-3 text-black' id='item{1}' style='{2}' data-key='{3}' >", header_tag, _id_img_postfix, _display, pObj.ItemID.ToString());
                sb.AppendFormat("<strong {0}>", _tooltip);
                if (is_sub)
                    sb.AppendFormat("{0}.{1}) {2} {3}", pParentOrder.ToString(), pSubOrder.ToString(), pObj.Text, _is_required_symbol);
                else
                    sb.AppendFormat("{0}) {1} {2}", pObj.Order.ToString(), pObj.Text, _is_required_symbol);

                sb.Append("</strong>");
                
                sb.AppendFormat("<a id='divStatus_{0}' class='pull-right'> <i id='img{0}' data-required='{1}' class='' style='font-size:20px;' ></i> </a>", _id_img_postfix, pObj.IsMandatory.ToString());
                sb.Append("<hr />");
            }

            //answer & option
            ClsListAssessmentItem objAI = null;
            if (pIs_Sub_Grid)
                objAI = pObj;
            sb.Append(CreateAssessmentItemXChoice(pObj.ItemID, pObj.TypeID, _id_img_postfix, pObj.RelatedDCIds, pObj.RelatedFBIds,  objAI));
            //CommonUtilities.GlobalEncrypt(pObj.AssessmentItemID.ToString(), BSWSession.SessionKey)
            return sb.ToString();
        }

        private string CreateAssessmentItemXChoice(int pAssessmentItemID, int pTypeID, string pIDPostFix, string pRelAIDs, string pRelFBIDs,  ClsListAssessmentItem pObjAI = null)
        {

            //GetItemResponsesByAssessmentID
            StringBuilder sb = new StringBuilder();
            List<DataAccess.AssessmentItemXChoice> listXChoices = gAssessment_br.GetAssessmentItemChoicesByItemIDFromCache(pAssessmentItemID, false);
            Constants.enAssessmentItemType _type = (Constants.enAssessmentItemType)pTypeID;
            bool is_multiple_choice_category = (_type == Constants.enAssessmentItemType.MULTIPLE_CHOICE_CHK || _type == Constants.enAssessmentItemType.MULTIPLE_CHOICE_RADIO || _type == Constants.enAssessmentItemType.GRID_CHK || _type == Constants.enAssessmentItemType.GRID_RADIO);
           

            if (listXChoices == null || listXChoices.Count == 0)
                return string.Empty;

            string _value = string.Empty;
            if (listResponses != null && listResponses.Count > 0 && !is_multiple_choice_category)
            {
                AssessmentItemResponse objResp = listResponses.Find(o => o.AssessmentItemID == pAssessmentItemID);
                if (objResp != null)
                {
                    _value = objResp.Value;
                    string scr = string.Empty;
                    if (gIsAssessmentClosed == false)
                    {
                        scr = "$(function () { $('#img" + pIDPostFix + "').attr('class', 'fa fa-check text-success'); });";
                        ScriptManager.RegisterClientScriptBlock(pnl, pnl.GetType(), "CreateAssessmentItemXChoice" + Guid.NewGuid().ToString(), scr, true);
                    }
                }
            }
            string _rel_id_tag = string.Format("data-rel-con='{0}'", pRelAIDs);
            string _rel_fb_id_tag = string.Format("data-rel-con-fb='{0}'", pRelFBIDs);


            if (!is_multiple_choice_category)
            {
                DataAccess.AssessmentItemXChoice objXChoice = listXChoices[0];
                string _id_xchoice = objXChoice.ID.ToString();
                string _score_val = CommonUtilities.GlobalEncrypt(objXChoice.ScoreValue.ToString(), BSWSession.SessionKey);
                string _assessment_item_id = CommonUtilities.GlobalEncrypt(objXChoice.AssessmentItemID.ToString(), BSWSession.SessionKey);

                sb.Append("<div class='col-lg-12'>");

                if (gIsAssessmentClosed)
                {
                    //printer friendly
                    if (_type == Constants.enAssessmentItemType.TRUE_FALSE)
                        sb.AppendFormat("<i class='fa fa-arrow-circle-right {2}'></i> <span class='{2}' style='{1}'>{0}</span>", _value.Equals("1") ? "True" : "False", "font-weight: bold;", "text-primary");
                    else
                        sb.AppendFormat("<i class='fa fa-arrow-circle-right {2}'> </i> <span class='{2}' style='{1}'>{0}</span>", _value, "font-weight: bold;", "text-primary");
                }
                else
                {
                    sb.Append("<div class='form-group row'>");
                    sb.AppendFormat("<label class='form-control-label col-md-3' for='txt_choice_{1}'>{0} </label>", objXChoice.Value, _id_xchoice);

                    sb.Append("<div class='col-md-9'>");
                    if (_type == Constants.enAssessmentItemType.TEXT)
                        sb.AppendFormat("<textarea id='txt_choice_{0}' type='text' class='form-control' rows='4' data-attr1='{1}' onchange='SaveValue(this);' data-img='{2}' data-attr2='{4}' data-attr3='1' {5} {6} >{3} </textarea>", _id_xchoice, _score_val, pIDPostFix, _value, _assessment_item_id, _rel_id_tag, _rel_fb_id_tag);
                    else if (_type == Constants.enAssessmentItemType.INTEGER)
                        sb.AppendFormat("<input id='txt_choice_{0}' type='number' class='form-control' data-attr1='{1}' data-img='{2}' onchange='SaveValue(this);' data-attr2='{4}' value='{3}' data-attr3='2' {5} {6} > </input>", _id_xchoice, _score_val, pIDPostFix, _value, _assessment_item_id, _rel_id_tag, _rel_fb_id_tag);
                    else if (_type == Constants.enAssessmentItemType.DATETIME)
                    {
                        sb.AppendFormat("<div class='input-group date' id='datetimepicker{0}'>", _id_xchoice);
                        sb.AppendFormat("<input id='txt_choice_{0}' type='text' class='form-control' data-attr1='{1}' data-img='{2}' onchange='SaveValue(this);' data-attr2='{4}' value='{3}' data-attr3='3' {5} {6}></input>", _id_xchoice, _score_val, pIDPostFix, _value, _assessment_item_id, _rel_id_tag, _rel_fb_id_tag);
                        sb.Append("<span class='input-group-addon'><span class='fa fa-calendar'></span></span>");
                        sb.Append("</div>");
                    }
                    else if (_type == Constants.enAssessmentItemType.TRUE_FALSE)
                    {
                        sb.AppendFormat(@"<div class='form-check'>
                        <label class='form-check-label'>
                            <input class='form-check-input' type='radio' name='exampleRadios_{4}' id='chk_choice_{0}' data-attr1='{1}' data-img='{2}' value='1' onchange='SaveValue(this);' data-attr2='{4}' data-attr3='4' {3} {5} {6} >
                            True
                        </label>
                    </div>", _id_xchoice, _score_val, pIDPostFix, (_value.Equals("1") ? "checked=checked" : ""), _assessment_item_id, _rel_id_tag, _rel_fb_id_tag);
                        sb.AppendFormat(@"<div class='form-check'>
                        <label class='form-check-label'>
                            <input class='form-check-input' type='radio' name='exampleRadios_{4}' data-attr1='{1}' id='chk_choice_{0}_1' data-img='{2}' value='0' onchange='SaveValue(this);' data-attr2='{4}' {3} data-attr3='4' {5} {6} > 
                            False
                        </label>
                    </div>", _id_xchoice, _score_val, pIDPostFix, (_value.Equals("0") ? "checked=checked" : ""), _assessment_item_id, _rel_id_tag, _rel_fb_id_tag);
                    }
                    sb.Append("</div>");
                    sb.Append("</div>");
                }

                sb.Append("</div>");
            }
            else
            {
                List<AssessmentItemResponse> listResp = new List<AssessmentItemResponse>();
                if (listResponses != null)
                    listResp = listResponses.FindAll(o => o.AssessmentItemID == pAssessmentItemID);


                //grid, multiple choice 
                if (_type == Constants.enAssessmentItemType.MULTIPLE_CHOICE_CHK || _type == Constants.enAssessmentItemType.MULTIPLE_CHOICE_RADIO)
                {
                    string _type_id = "6";
                    string _input_type = "radio";
                    if (_type == Constants.enAssessmentItemType.MULTIPLE_CHOICE_CHK)
                    {
                        _input_type = "checkbox";
                        _type_id = "5";
                    }

                    foreach (DataAccess.AssessmentItemXChoice item in listXChoices)
                    {

                        int _choice_id = item.ChoiceID.Value;
                        string _value_m = string.Empty;
                        string _assessment_item_id_m = CommonUtilities.GlobalEncrypt(item.AssessmentItemID.ToString(), BSWSession.SessionKey);
                        bool is_answered = false;
                        if (listResp != null && listResp.Count > 0 && _choice_id > 0)
                        {
                            AssessmentItemResponse _objResp = listResp.Find(o => o.ChoiceID == _choice_id);
                            if (_objResp != null)
                            {
                                if (gIsAssessmentClosed == false)
                                {
                                    string scr = "$(function () { $('#img" + pIDPostFix + "').attr('class', 'fa fa-check text-success'); });";
                                    ScriptManager.RegisterClientScriptBlock(pnl, pnl.GetType(), "CreateAssessmentItemXChoice" + Guid.NewGuid().ToString(), scr, true);
                                }
                                _value_m = _objResp.Value;
                                is_answered = true;
                            }
                        }
                        string _score_val = CommonUtilities.GlobalEncrypt(item.ScoreValue.ToString(), BSWSession.SessionKey);
                        if (gIsAssessmentClosed)
                        {
                            if (is_answered)
                                sb.AppendFormat("<i class='fa fa-arrow-circle-right {2}'></i> <span class='{2}' style='{1}'>{0}</span>", item.Value, "font-weight: bold;", "text-primary");

                            continue;
                        }
                        sb.AppendFormat(@"<div class='form-check'>
                        <label class='form-check-label'>
                            <input class='form-check-input' type='{6}' name='exampleRadios_{4}' id='chk_choice_{0}' data-attr1='{1}' data-img='{2}' value='1' onchange='SaveValue(this);' data-attr2='{4}' data-attr3='{8}' data-attr4='{7}' {3}  {9}>
                            {5} 
                        </label>
                    </div>", item.ID.ToString(), _score_val, pIDPostFix, (_value_m.Equals("1") ? "checked=checked" : ""), _assessment_item_id_m, item.Value, _input_type, _choice_id.ToString(), _type_id, _rel_id_tag, _rel_fb_id_tag);

                    }
                }
                else
                {
                    string _input_type = "radio";
                    string _type_val = "8";
                    if (_type == Constants.enAssessmentItemType.GRID_CHK)
                    {
                        _type_val = "7";
                        _input_type = "checkbox";
                    }


                    List<DataAccess.AssessmentItem> listParentOfItem = gAssessment_br.GetSubAssessmentItemsByParentID(pObjAI.ParentID.Value);
                    listParentOfItem = listParentOfItem.OrderBy(x => x.Order).ToList();
                    sb.Append("<table class='table'>");
                    foreach (DataAccess.AssessmentItem item in listParentOfItem)
                    {
                        listXChoices = gAssessment_br.GetAssessmentItemChoicesByItemIDFromCache(item.AssessmentItemID, false);
                        if (item.Equals(listParentOfItem.First()))
                        {
                            sb.Append("<thead>");
                            sb.Append("<tr>");
                            sb.AppendFormat("<th style='border-top:none;'></th>");
                            foreach (DataAccess.AssessmentItemXChoice item_sub in listXChoices)
                            {

                                sb.AppendFormat("<th class='Item-Centered'>{0}</th>", item_sub.Value);
                            }
                            sb.Append("</tr>");
                        }
                        pIDPostFix += "_" + item.Order.ToString();
                            string _display = string.Empty;
                        if (!string.IsNullOrEmpty(item.DisplayCondition))
                        {
                            bool formula_result = false;
                            gAssessment_br.ExecuteFormula(item.AssessmentItemID, "", ref formula_result);
                            if (!formula_result)
                                _display = " display: none !important; ";

                        }
                        sb.AppendFormat("<tr data-key='{0}'>", item.AssessmentItemID.ToString());
                        sb.AppendFormat("<th id='thAI_{0}' scope='row' style='font-weight: normal; {2}'  data-rel-con='{1}' data-rel-con-fb='{3}' >", pIDPostFix, HttpUtility.JavaScriptStringEncode(item.RelatedDCIds), _display, HttpUtility.JavaScriptStringEncode(item.RelatedFBIds));
                        sb.Append(item.Text.ToString());
                        sb.AppendFormat("<a id='divStatus_{0}' class='pull-right'> <i id='img{0}' data-required='{1}' class='' style='font-size:20px;' ></i> </a>", pIDPostFix, "0");
                        sb.Append("</th>");
                        foreach (DataAccess.AssessmentItemXChoice item_sub in listXChoices)
                        {
                            if (listResponses != null)
                                listResp = listResponses.FindAll(o => o.AssessmentItemID == item.AssessmentItemID);

                            int _choice_id = item_sub.ChoiceID.Value;
                            string _assessment_item_id_m = CommonUtilities.GlobalEncrypt(item.AssessmentItemID.ToString(), BSWSession.SessionKey);
                            string _score_val = CommonUtilities.GlobalEncrypt(item_sub.ScoreValue.ToString(), BSWSession.SessionKey);
                            string _value_g = string.Empty;

                            if (listResp != null && listResp.Count > 0 && _choice_id > 0)
                            {
                                AssessmentItemResponse _objResp = listResp.Find(o => o.ChoiceID == _choice_id);
                                if (_objResp != null)
                                {
                                    if (gIsAssessmentClosed == false)
                                    {
                                        string scr = "$(function () { $('#img" + pIDPostFix + "').attr('class', 'fa fa-check text-success'); });";
                                        ScriptManager.RegisterClientScriptBlock(pnl, pnl.GetType(), "CreateAssessmentItemXChoice" + Guid.NewGuid().ToString(), scr, true);
                                       
                                    }
                                    _value_g = _objResp.Value;
                                }
                            }
                            sb.Append("<td class='Item-Centered'>");
                            if (gIsAssessmentClosed == false)
                                sb.AppendFormat(@"<input type='{6}' name='exampleRadios_{9}' id='chk_choice_{0}' data-attr1='{1}' data-img='{2}' value='1' onchange='SaveValue(this);' data-attr2='{4}' data-attr3='{8}' data-attr4='{7}' {3}>", item_sub.ID.ToString(), _score_val, pIDPostFix, (_value_g.Equals("1") ? "checked=checked" : ""), _assessment_item_id_m, item_sub.Value, _input_type, _choice_id.ToString(), _type_val, item.AssessmentItemID.ToString());
                            else
                            {
                                sb.AppendFormat("{0}", (_value_g.Equals("1") ? "<i class='fa fa-check text-primary' style='font-size:20px;' ></i>" : "-"));
                            }
                            sb.Append("</td>");
                        }
                        sb.Append("</tr>");
                    }
                    sb.Append("</table>");
                }
            }
            return sb.ToString();
        }

        private void BindAssessment()
        {
            DataAccess.Assessment obj = gAssessment_br.GetAssessmentByIDFromCache(gAssessmentID);
            if (obj == null)
            {
                Set_Error_Message("Assessment could not be found!");
                return;
            }
            lblAssessmentName.Text = obj.Name;
            lblAssessmentVersion.Text = obj.Version;
            lblIntroduction.Text = obj.Introduction;
            lblAssessmentType.Text = gLookUp.GetAssesmentTypeFromCache(obj.TypeID).Description;
        }

        protected void btnCloseAssessment_Click(object sender, EventArgs e)
        {
            if (gAssessmentRespID < 0)
                int.TryParse(hdnRespID.Value, out gAssessmentRespID);
            
            bool res = gAssessment_br.Close_Assessment_Response(gAssessmentRespID, BSWSession.UserID);
            if (res)
            {
                bool res_feedback = false;
                OP_Feedback feedback_br = new OP_Feedback(BSWSession.SessionID, BSWSession.UserID);
                feedback_br.ExecuteFormula(gAssessmentID, ref res_feedback);
                if (res_feedback)
                    Response.Redirect("../FeedbackModule/FeedbackViewer.aspx" + EncryptQueryString("AssessmentID=" + gAssessmentID));
                else
                    Set_Message_On_Another_Page("Assessment finished successfully", "AssessmentList.aspx", false);
            }
            else
                Set_Error_Message("An error occured during save process");
        }

        protected void btnDeleteAssessmentResponse_Click(object sender, EventArgs e)
        {
            using (var context = BusinessRules.ContextHandler.GetInstance())
            {
                try
                {
                    var assResp = context.AssessmentResponses.Where(x => x.ID == gAssessmentRespID).FirstOrDefault();

                    context.AssessmentItemResponses.DeleteAllOnSubmit(assResp.AssessmentItemResponses);
                    context.AssessmentResponses.DeleteOnSubmit(assResp);
                    context.FeedbackResponses.DeleteAllOnSubmit(context.FeedbackResponses.Where(x => x.AssessmentID == gAssessmentID && x.UserID == BSWSession.UserID));
                    context.SubmitChanges();
                    Set_Message_On_Another_Page("Assessment Response deleted successfully", "AssessmentList.aspx", false);
                }
                catch { }
            }
        }

        protected void btnUpdateRespID_Click(object sender, EventArgs e)
        {
            try
            {
                gAssessmentRespID = Convert.ToInt32(hdnRespID.Value);
            }
            catch
            {
            }
        }
    }
}