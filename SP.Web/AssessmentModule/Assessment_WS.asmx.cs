using System;
using System.Web;
using System.Web.Services;
using SP.Common;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace SP.Web.AssessmentModule
{
    /// <summary>
    /// Summary description for Assessment1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Assessment_WS : System.Web.Services.WebService
    {

        [WebMethod(EnableSession = true)]
        public string SaveValueX(string pItemID, string pAsseID, string pScore, string pVal, string pType, string pRequired, string pCID, string pKeyRel, string pAsseRespID, string pKeyRelFB)
        {
            bool _temp_res = false;
            bool is_req = false;
            bool.TryParse(pRequired, out is_req);

            if (string.IsNullOrEmpty(pVal) && is_req)
                throw new Exception("Can not be empty!");

            if (string.IsNullOrEmpty(pVal))
                return "false";

            int _cid = -1;
            int type_assessment_val;
            Constants.enAssessmentItemType _type = Constants.enAssessmentItemType.TEXT;
            if (Int32.TryParse(pType, out type_assessment_val))
            {
                _type = (Constants.enAssessmentItemType)type_assessment_val;
                if (_type == Constants.enAssessmentItemType.DATETIME)
                {
                    DateTime _dt;
                    if (DateTime.TryParse(pVal, out _dt) == false)
                    {
                        CommonUtilities.Log("Can not convert value to date! Value : " + pVal + Environment.NewLine + "Assessment_WS.asmx");
                        throw new Exception("Can not convert value to date!");
                    }
                }
                else if (_type == Constants.enAssessmentItemType.INTEGER)
                {
                    int _val;
                    if (int.TryParse(pVal, out _val) == false)
                    {
                        CommonUtilities.Log("Can not convert value to number! Value : " + pVal + Environment.NewLine + "Assessment_WS.asmx");
                        throw new Exception("Can not convert value to number!");
                    }
                }
                else if (_type == Constants.enAssessmentItemType.MULTIPLE_CHOICE_CHK || _type == Constants.enAssessmentItemType.MULTIPLE_CHOICE_RADIO || _type == Constants.enAssessmentItemType.GRID_RADIO || _type == Constants.enAssessmentItemType.GRID_CHK)
                {
                    if (int.TryParse(pCID, out _cid) == false)
                    {
                        CommonUtilities.Log("Can not convert value to number! Value : " + pVal + Environment.NewLine + "Assessment_WS.asmx");
                        throw new Exception("Can not convert value to number!");
                    }
                }
            }
            else
                throw new Exception("An error occured!");

            //if(Constants.enAssessmentItemType.TEXT )

            BSWSession cses = new BSWSession(HttpContext.Current.Session);
            if (cses == null && cses.UserID < 0)
            {
                CommonUtilities.Log("Session is empty! Assessment.asmx SaveValueX. ");
                throw new Exception("Please signin again!");
            }
            try
            {
                string _valTemp = string.Empty;
                BusinessRules.OP_Assessment assessment_br = new BusinessRules.OP_Assessment(cses.SessionID, cses.UserID);
                DataAccess.AssessmentItemResponse objResp = new DataAccess.AssessmentItemResponse();
                int _assessment_item_id = Convert.ToInt32(CommonUtilities.GlobalDecrypt(pItemID, cses.SessionKey));
                objResp.AssessmentItemID = _assessment_item_id;
                objResp.ScoreValue = Convert.ToInt32(CommonUtilities.GlobalDecrypt(pScore, cses.SessionKey));
                objResp.Value = pVal;
                if (_cid > 0)
                {
                    objResp.ChoiceID = _cid;
                    _valTemp = _cid.ToString();
                }
                else
                        _valTemp = pVal;

                int _resp_id = Convert.ToInt32(pAsseRespID);
                bool res = assessment_br.Save_Assessment_Item_Response(objResp, Convert.ToInt32(pAsseID),ref _resp_id, cses.UserID, _type);
                if (!res)
                    return res.ToString();
                if (!string.IsNullOrEmpty(pKeyRelFB))
                {
                    BusinessRules.OP_Feedback feedback_br = new BusinessRules.OP_Feedback(cses.SessionID, cses.UserID);
                    res = feedback_br.UpdateFeedbackRuleFormula(_assessment_item_id, Convert.ToInt32(pAsseID), _valTemp);
                    if (!res)
                        return res.ToString();
                }

                string result_json = string.Empty;
                if (!string.IsNullOrEmpty(pKeyRel))
                {
                    List<AIDDisplayResult> listRes = new List<AIDDisplayResult>();
                    string[] _arr = pKeyRel.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item in _arr)
                    {
                        int _aid = -1;
                        if (int.TryParse(item, out _aid))
                        {
                            assessment_br.UpdateAIDisplayFormula(_aid, _assessment_item_id, _valTemp);
                            assessment_br.ExecuteFormula(_aid, _valTemp, ref _temp_res);
                            AIDDisplayResult obj = new AIDDisplayResult();
                            obj.AID = _aid;
                            obj.Result = _temp_res;
                            obj.RespID = _resp_id;
                            listRes.Add(obj);
                            JavaScriptSerializer serializer = new JavaScriptSerializer();
                            result_json = serializer.Serialize(listRes);
                        }

                        //ExecuteFormula
                    }
                }
                else
                    return res.ToString() + "#" + _resp_id.ToString();

                return result_json;
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                throw new Exception("An error occured during save process!");
            }
        }

    }

    public class AIDDisplayResult
    {
        public int AID { get; set; }
        public bool Result { get; set; }
        public int RespID { get; set; }
    }
}
