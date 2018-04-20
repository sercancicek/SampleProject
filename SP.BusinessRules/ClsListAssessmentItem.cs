using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BusinessRules
{
    public class ClsListAssessmentItem
    {
        public int ItemID { get; set; }
        public int TypeID { get; set; }
        public int Version { get; set; }
        public string Text { get; set; }
        public string HelpText { get; set; }
        public int Order { get; set; }
        public int AITreeID { get; set; }
        public int? ParentID { get; set; }
        public bool IsMandatory { get; set; }
        public string DisplayCondition { get; set; }
        public string RelatedDCIds { get; set; }
        public string RelatedFBIds { get; set; }
        public List<ClsListAssessmentItem> SubItems { get; set; }

        public static List<ClsListAssessmentItem> ConvertAssessmentItemsToClass(List<AssessmentItem> pSourceList, bool pAddSubItemsAsMain)
        {
            List<ClsListAssessmentItem> listRes = new List<ClsListAssessmentItem>();
            List<AssessmentItem> listParents = pSourceList;

            if (!pAddSubItemsAsMain)
                listParents = pSourceList.FindAll(o => o.ParentAssessmentID == null || o.ParentAssessmentID == 0);

            foreach (var AssessmentItem in listParents)
            {
                ClsListAssessmentItem obj = new ClsListAssessmentItem()
                {
                    ItemID = AssessmentItem.AssessmentItemID,
                    Order = AssessmentItem.Order,
                    Text = AssessmentItem.Text,
                    TypeID = AssessmentItem.TypeID,
                    Version = AssessmentItem.Version,
                    ParentID = AssessmentItem.ParentAssessmentID,
                    HelpText = AssessmentItem.HelpText,
                    IsMandatory = AssessmentItem.IsMandatory,
                    DisplayCondition = AssessmentItem.DisplayCondition,
                    RelatedDCIds = AssessmentItem.RelatedDCIds,
                    RelatedFBIds = AssessmentItem.RelatedFBIds,
                    AITreeID = AssessmentItem.AssessmentItemTreeID,
                    SubItems = new List<ClsListAssessmentItem>()
                };
                List<AssessmentItem> listChild = null;

                if (!pAddSubItemsAsMain)
                    listChild = pSourceList.FindAll(o => o.ParentAssessmentID == AssessmentItem.AssessmentItemID);

                if (listChild != null && listChild.Count > 0)
                {
                    obj.SubItems = ConvertAssessmentItemsToClass(listChild, pAddSubItemsAsMain);
                }
                listRes.Add(obj);
            }
            if (listParents == null || listParents.Count == 0)
            {
                foreach (var AssessmentItem in pSourceList)
                {
                    ClsListAssessmentItem obj = new ClsListAssessmentItem()
                    {
                        ItemID = AssessmentItem.AssessmentItemID,
                        Order = AssessmentItem.Order,
                        Text = AssessmentItem.Text,
                        TypeID = AssessmentItem.TypeID,
                        Version = AssessmentItem.Version,
                        ParentID = AssessmentItem.ParentAssessmentID,
                        SubItems = new List<ClsListAssessmentItem>()
                    };
                    listRes.Add(obj);
                }
            }

            return listRes;
        }

        public static ClsListAssessmentItem ConvertAssessmentItemToClass(AssessmentItem pSourceObj)
        {
            ClsListAssessmentItem obj = new ClsListAssessmentItem()
            {
                ItemID = pSourceObj.AssessmentItemID,
                Order = pSourceObj.Order,
                Text = pSourceObj.Text,
                TypeID = pSourceObj.TypeID,
                Version = pSourceObj.Version,
                ParentID = pSourceObj.ParentAssessmentID,
                HelpText = pSourceObj.HelpText,
                IsMandatory = pSourceObj.IsMandatory,
                DisplayCondition = pSourceObj.DisplayCondition,
                RelatedDCIds = pSourceObj.RelatedDCIds,
                RelatedFBIds = pSourceObj.RelatedFBIds,
                AITreeID = pSourceObj.AssessmentItemTreeID,
                SubItems = new List<ClsListAssessmentItem>()
            };

            return obj;
        }
    }
}
