using SP.Common;
// using BSW.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.BusinessRules
{
    public class OP_Assessment
    {
        private string gUniqueID;
        private int gUserID;

        public OP_Assessment(string UniqueID, int pUserID)
        {
            gUniqueID = UniqueID;
            gUserID = pUserID;
        }

        public List<Program> Get_Program_List_From_Cache(bool pGetPassiveProgramsToo = false)
        {
            object cache_entry = Cache_Manager.Get_Entry_From_Cache(Cache_Manager.enCacheTypes.PROGRAM, 1);
            List<Program> listPrograms = null;

            if (cache_entry == Cache_Manager.ERR_CACHE)
            {
                //**** 3 ****  Cache failed for some reason; go directly to DB
                listPrograms = Get_All_Programs();
            }
            else if (cache_entry == Cache_Manager.NO_CACHE)
            {
                //**** 4 ****  cache miss; get from DB and add to cache; after that return the object directly
                List<Program> obj = Get_All_Programs();
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.PROGRAM, 1, obj as object);
                listPrograms = obj;
            }
            else
            {
                //**** 5 ****  return from cache
                if (cache_entry == null)
                    return null;

                listPrograms = (cache_entry as List<Program>);
            }
            if (pGetPassiveProgramsToo == false)
                listPrograms = listPrograms.FindAll(o => o.StatusID == 1);
            return listPrograms;
        }

        private List<Program> Get_All_Programs()
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.Programs;
                if (query != null && query.Count() > 0)
                    return query.ToList();
            }
            return null;
        }

        #region "Assessment"
        private static List<Assessment> GetAssessments(bool pOnlyLatest)
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.Assessments.AsQueryable();
                if(pOnlyLatest)
                    query = query.Where(o=> o.IsLatest == true);

                if (query != null && query.Count() > 0)
                    return query.ToList();
            }
            return null;
        }

        public string GetAssessmentName(int pAssessmentID)
        {
            string assessment_name = string.Empty;

            Assessment objArticle = GetLatestAssessmentByIDFromCache(pAssessmentID);
            if (objArticle != null)
                assessment_name = objArticle.Name;

            return assessment_name;
        }

        public Assessment GetLatestAssessmentByIDFromCache(int pAssessmentID)
        {
            Assessment objArticle = GetAssessmentByIDFromCache(pAssessmentID);

            if (objArticle != null && !objArticle.IsLatest && objArticle.AssessmentTreeID.HasValue)
                objArticle = GetLatestAssessmentByTreeIDFromCache(objArticle.AssessmentTreeID.Value);

            return objArticle;
        }

        public Assessment GetLatestAssessmentByTreeIDFromCache(int pTreeID)
        {
            List<Assessment> list = GetAssessmentsFromCache(false);
            if (list != null)
                return list.FirstOrDefault(o => o.IsLatest == true && o.AssessmentTreeID == pTreeID);

            return null;
        }

        public List<Assessment> GetAssessmentsByTreeIDFromCache(int pTreeID)
        {
            List<Assessment> list = GetAssessmentsFromCache(false);
            if (list != null)
                return list.Where(o => o.AssessmentTreeID == pTreeID).ToList();

            return null;
        }

        public Assessment GetAssessmentByIDFromCache(int pAssessmentID)
        {
            List<Assessment> list = GetAssessmentsFromCache(true);
            if (list != null)
                return list.FirstOrDefault(o => o.AssessmentID == pAssessmentID);

            return null;
        }


        public static List<Assessment> GetAssessmentsFromCache(bool pOnlyLatest)
        {
            int merged_id = 1;
            if (pOnlyLatest)
                merged_id = -1;
            //**** 2 ****  Get item from cache - if exists!
            object cache_entry = Cache_Manager.Get_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENTS, merged_id);
            List<Assessment> listAssessments = null;

            if (cache_entry == Cache_Manager.ERR_CACHE)
            {
                //**** 3 ****  Cache failed for some reason; go directly to DB
                listAssessments = GetAssessments(pOnlyLatest);
            }
            else if (cache_entry == Cache_Manager.NO_CACHE)
            {
                //**** 4 ****  cache miss; get from DB and add to cache; after that return the object directly
                List<Assessment> obj = GetAssessments(pOnlyLatest);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENTS, merged_id, obj as object);
                listAssessments = obj;
            }
            else
            {
                //**** 5 ****  return from cache
                if (cache_entry == null)
                    return null;

                listAssessments = (cache_entry as List<Assessment>);
            }
            return listAssessments;
        }

        public int GetNextAssessmentTreeID()
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    context.DeferredLoadingEnabled = false;
                    var query = context.VNextAvailableTreeIDs;
                    List<VNextAvailableTreeID> list_temp = query.ToList();
                    return list_temp[0].next_available_id;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return -1;
            }
        }

        public bool Save_Assessment(Assessment pObj, bool pIsVersionUpdate)
        {
            try
            {
                int _AssessmentID = pObj.AssessmentID;
                using (var context = ContextHandler.GetInstance())
                {
                    if (_AssessmentID > 0 && pIsVersionUpdate)
                    {
                        context.DeferredLoadingEnabled = false;
                        var assessment = context.Assessments.FirstOrDefault(o => o.AssessmentID == _AssessmentID);
                        if (assessment != null)
                        {
                            assessment.IsLatest = false;
                        }
                        context.SubmitChanges();
                    }
                    context.Assessments.InsertOnSubmit(pObj);
                    context.SubmitChanges();

                    Invalidate_All_Assessments_From_Cache();

                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }           
        }

        public static void Invalidate_All_Assessments_From_Cache()
        {
            Cache_Manager.Drop_All_Entries(Cache_Manager.enCacheTypes.ASS_ASSESSMENTS, true);
        }
        public static void Invalidate_Assessment_From_Cache(int pAssessmentID)
        {
            Cache_Manager.Drop_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENTS, pAssessmentID.ToString());
        }
        #endregion

        #region "AssessmentItem"
        public static void FillClsAICache()
        {
            List<Assessment> listAsse = GetAssessmentsFromCache(false);
            foreach (var item in listAsse)
            {
                List<ClsListAssessmentItem> obj = GetAssessmentItems(item.AssessmentID, false);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_LIST, item.AssessmentID, obj as object);
                obj = GetAssessmentItems(item.AssessmentID, true);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_LIST, item.AssessmentID * -1, obj as object);


            }
        }

        public List<ClsListAssessmentItem> GetAssessmentItemListFromCache(int pAssessmentID, bool pAddSubItemsAsMain = false)
        {
            int merged_id = pAssessmentID;
            if (pAddSubItemsAsMain)
                merged_id = -1 * pAssessmentID;

            //**** 2 ****  Get item from cache - if exists!
            object cache_entry = Cache_Manager.Get_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_LIST, merged_id);
            List<ClsListAssessmentItem> listAssessments = null;

            if (cache_entry == Cache_Manager.ERR_CACHE)
            {
                //**** 3 ****  Cache failed for some reason; go directly to DB
                listAssessments = GetAssessmentItems(pAssessmentID, pAddSubItemsAsMain);
            }
            else if (cache_entry == Cache_Manager.NO_CACHE)
            {
                //**** 4 ****  cache miss; get from DB and add to cache; after that return the object directly
                List<ClsListAssessmentItem> obj = GetAssessmentItems(pAssessmentID, pAddSubItemsAsMain);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_LIST, merged_id, obj as object);
                listAssessments = obj;
            }
            else
            {
                //**** 5 ****  return from cache
                if (cache_entry == null)
                    return null;

                listAssessments = (cache_entry as List<ClsListAssessmentItem>);
            }
            return listAssessments;
        }

        private static List<ClsListAssessmentItem> GetAssessmentItems(int pAssessmentID, bool pAddSubItemsAsMain)
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.AssessmentItems.Where(o => o.IsLatest == true && o.AssessmentID == pAssessmentID);
                if (query != null && query.Count() > 0)
                {
                    List<AssessmentItem> list_temp = query.ToList();
                    return ClsListAssessmentItem.ConvertAssessmentItemsToClass(list_temp, pAddSubItemsAsMain);
                }
            }
            return null;
        }

        public List<int> GetAssessmentItemIDsByTreeID(int pAssessmentTreeID)
        {
            List<int> resList = new List<int>();
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.AssessmentItems.Where(o => o.AssessmentItemTreeID == pAssessmentTreeID);
                if (query != null && query.Count() > 0)
                {
                    List<AssessmentItem> list_temp = query.ToList();
                    foreach (var item in list_temp)
                    {
                        resList.Add(item.AssessmentItemID);
                    }
                    return resList;
                }
            }
            return null;
        }

        private AssessmentItem GetAssessmentItem(int pAssessmentItemID)
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.AssessmentItems.Where(o => o.AssessmentItemID == pAssessmentItemID);
                if (query != null && query.Count() > 0)
                {
                    List<AssessmentItem> list_temp = query.ToList();
                    if (list_temp != null && list_temp.Count > 0)
                        return list_temp[0];
                    else
                        return new AssessmentItem();
                }
            }
            return null;
        }

        private static List<AssessmentItem> GetAssessmentItems()
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.AssessmentItems.Where(o=> o.IsLatest == true);
                if (query != null && query.Count() > 0)
                {
                    List<AssessmentItem> list_temp = query.ToList();
                    if (list_temp != null && list_temp.Count > 0)
                        return list_temp;
                    else
                        return new List<AssessmentItem>();
                }
            }
            return null;
        }

        public static void FillAICache()
        {
            List<AssessmentItem> listAI = GetAssessmentItems();
            foreach (var item in listAI)
            {
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM, item.AssessmentItemID.ToString(), item as object);
            }

        }

        public AssessmentItem GetAssessmentItemByItemIDFromCache(int pAssessmentItemID)
        {
            //**** 2 ****  Get item from cache - if exists!
            object cache_entry = Cache_Manager.Get_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM, pAssessmentItemID.ToString());
            AssessmentItem objAssessment = null;

            if (cache_entry == Cache_Manager.ERR_CACHE)
            {
                //**** 3 ****  Cache failed for some reason; go directly to DB
                objAssessment = GetAssessmentItem(pAssessmentItemID);
            }
            else if (cache_entry == Cache_Manager.NO_CACHE)
            {
                //**** 4 ****  cache miss; get from DB and add to cache; after that return the object directly
                AssessmentItem obj = GetAssessmentItem(pAssessmentItemID);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM, pAssessmentItemID.ToString(), obj as object);
                objAssessment = obj;
            }
            else
            {
                //**** 5 ****  return from cache
                if (cache_entry == null)
                    return null;

                objAssessment = (cache_entry as AssessmentItem);
            }
            return objAssessment;
        }

        public static void Invalidate_All_AssessmentItemList_From_Cache()
        {
            Cache_Manager.Drop_All_Entries(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_LIST, true);
        }

        public static void Invalidate_Assessment_Item_From_Cache(int pAssessmentItemID)
        {
            Cache_Manager.Drop_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM, pAssessmentItemID.ToString());
        }

        public static void Invalidate_Assessment_Items_From_Cache(int pAssessmentID)
        {
            Cache_Manager.Drop_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_LIST, pAssessmentID);
        }

        public bool Save_Assessment_Item(AssessmentItem pObj)
        {
            try
            {
                int _AssessmentItemID = pObj.AssessmentItemID;
                using (var context = ContextHandler.GetInstance())
                {
                    List<DataAccess.AssessmentItemXChoice> list_xchoice = null;
                    if (_AssessmentItemID > 0)
                    {
                        var assessment_item = context.AssessmentItems.FirstOrDefault(o => o.AssessmentItemID == _AssessmentItemID);
                        if (assessment_item != null)
                        {
                            assessment_item.IsLatest = false;
                        }
                        list_xchoice = GetAssessmentItemChoicesByItemIDFromCache(_AssessmentItemID, true);
                        context.SubmitChanges();
                    }
                    pObj.Version += 1;
                    context.AssessmentItems.InsertOnSubmit(pObj);
                    context.SubmitChanges();
                    if (list_xchoice != null && list_xchoice.Count > 0)
                    {
                        for (int i = 0; i < list_xchoice.Count; i++)
                        {
                            DataAccess.AssessmentItemXChoice obj_temp = list_xchoice[i];
                            obj_temp.AssessmentItemID = pObj.AssessmentItemID;
                            context.AssessmentItemXChoices.InsertOnSubmit(obj_temp);
                            context.SubmitChanges();
                            Invalidate_Assessment_Item_X_Choice_From_Cache(obj_temp.ID);
                        }
                    }
                    Invalidate_Assessment_Item_From_Cache(_AssessmentItemID);
                    Invalidate_Assessment_Items_From_Cache(pObj.AssessmentID);

                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }
        }

        public bool Save_Assessment_Item_RelatedDCIds(int pAssessmentItemID, string pRelatedDCIds)
        {
            try
            {
                if (pAssessmentItemID < 0)
                    return false;

                using (var context = ContextHandler.GetInstance())
                {
                    var assessment_item = context.AssessmentItems.FirstOrDefault(o => o.AssessmentItemID == pAssessmentItemID);
                    if (assessment_item != null)
                    {
                        assessment_item.RelatedDCIds += pRelatedDCIds;
                    }
                    context.SubmitChanges();
                    Invalidate_Assessment_Item_From_Cache(pAssessmentItemID);
                    Invalidate_Assessment_Items_From_Cache(pAssessmentItemID);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }
        }

        public bool Save_Assessment_Item_RelatedFBIds(int pAssessmentItemID, string pRelatedFBIds)
        {
            try
            {
                if (pAssessmentItemID < 0)
                    return false;

                using (var context = ContextHandler.GetInstance())
                {
                    var assessment_item = context.AssessmentItems.FirstOrDefault(o => o.AssessmentItemID == pAssessmentItemID);
                    if (assessment_item != null)
                    {
                        assessment_item.RelatedFBIds += pRelatedFBIds;
                    }
                    context.SubmitChanges();
                    Invalidate_Assessment_Item_From_Cache(pAssessmentItemID);
                    Invalidate_Assessment_Items_From_Cache(pAssessmentItemID);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }
        }

        public int GetNextAssessmentItemTreeID()
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    context.DeferredLoadingEnabled = false;
                    var query = context.VNextAvailableItemTreeIDs;
                    List<VNextAvailableItemTreeID> list_temp = query.ToList();
                    return list_temp[0].next_available_id;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return -1;
            }
        }

        public AssessmentItem GetAssessmentItemByItemTreeIDFromCache(int pAssessmentItemTreeID)
        {
            //**** 2 ****  Get item from cache - if exists!
            object cache_entry = Cache_Manager.Get_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM, pAssessmentItemTreeID);
            AssessmentItem objAssessment = null;

            if (cache_entry == Cache_Manager.ERR_CACHE)
            {
                //**** 3 ****  Cache failed for some reason; go directly to DB
                objAssessment = GetLatestAssessmentItemByTreeID(pAssessmentItemTreeID);
            }
            else if (cache_entry == Cache_Manager.NO_CACHE)
            {
                //**** 4 ****  cache miss; get from DB and add to cache; after that return the object directly
                AssessmentItem obj = GetLatestAssessmentItemByTreeID(pAssessmentItemTreeID);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM, pAssessmentItemTreeID, obj as object);
                objAssessment = obj;
            }
            else
            {
                //**** 5 ****  return from cache
                if (cache_entry == null)
                    return null;

                objAssessment = (cache_entry as AssessmentItem);
            }
            return objAssessment;
        }

        private AssessmentItem GetLatestAssessmentItemByTreeID(int pAssessmentItemTreeID)
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.AssessmentItems.Where(o => o.AssessmentItemTreeID == pAssessmentItemTreeID);
                if (query != null && query.Count() > 0)
                {
                    List<AssessmentItem> list_temp = query.ToList();
                    if (list_temp != null && list_temp.Count > 0)
                        return list_temp[0];
                    else
                        return new AssessmentItem();
                }
            }
            return null;
        }

        public List<AssessmentItem> GetSubAssessmentItemsByParentID(int pParentAIID)
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.AssessmentItems.Where(o => o.ParentAssessmentID == pParentAIID);
                if (query != null && query.Count() > 0)
                {
                    List<AssessmentItem> list_temp = query.ToList();
                    if (list_temp != null && list_temp.Count > 0)
                        return list_temp;

                }
            }
            return null;
        }

        public int GetNextVersionNumber(int pAssessmentItemTreeID)
        {
            AssessmentItem obj = GetAssessmentItemByItemTreeIDFromCache(pAssessmentItemTreeID);
            return obj.Version + 1;
        }
        #endregion

        #region "AssessmentItem X Choice"
        private static List<DataAccess.AssessmentItemXChoice> GetAssessmentItemChoices(int pAssessmentItemID, bool pAddSubChoices)
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                List<int> listIDs = new List<int>();
                listIDs.Add(pAssessmentItemID);
                if (pAddSubChoices)
                {
                    var query_assessment_items = context.AssessmentItems.Where(x => x.ParentAssessmentID == pAssessmentItemID).Select(x => x.AssessmentItemID);
                    if (query_assessment_items != null && query_assessment_items.Count() > 0)
                    {
                        foreach (var item in query_assessment_items)
                        {
                            listIDs.Add(item);
                        }
                    }
                }
                var query = context.AssessmentItemXChoices.Where(o => listIDs.Contains(o.AssessmentItemID));

                if (query != null && query.Count() > 0)
                {
                    List<DataAccess.AssessmentItemXChoice> list_temp = query.ToList();
                    return list_temp;
                }
            }
            return null;
        }

        public static void FillAIChoices()
        {
            List<AssessmentItem> listAI = GetAssessmentItems();

            foreach (var item in listAI)
            {
                List<DataAccess.AssessmentItemXChoice> listChoices = GetAssessmentItemChoices(item.AssessmentItemID, false);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_CHOICES, item.AssessmentItemID, listChoices as object);
            }
        }

        public List<DataAccess.AssessmentItemXChoice> GetAssessmentItemChoicesByItemIDFromCache(int pAssessmentItemID, bool pAddSubChoices)
        {
            //**** 2 ****  Get item from cache - if exists!
            object cache_entry = Cache_Manager.Get_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_CHOICES, pAssessmentItemID);
            List<DataAccess.AssessmentItemXChoice> listChoices = null;

            if (cache_entry == Cache_Manager.ERR_CACHE)
            {
                //**** 3 ****  Cache failed for some reason; go directly to DB
                listChoices = GetAssessmentItemChoices(pAssessmentItemID, pAddSubChoices);
            }
            else if (cache_entry == Cache_Manager.NO_CACHE)
            {
                //**** 4 ****  cache miss; get from DB and add to cache; after that return the object directly
                List<DataAccess.AssessmentItemXChoice> obj = GetAssessmentItemChoices(pAssessmentItemID, pAddSubChoices);
                if (!pAddSubChoices)
                    Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_CHOICES, pAssessmentItemID, obj as object);
                listChoices = obj;
            }
            else
            {
                //**** 5 ****  return from cache
                if (cache_entry == null)
                    return null;

                listChoices = (cache_entry as List<DataAccess.AssessmentItemXChoice>);
            }
            return listChoices;
        }

        public DataAccess.AssessmentItemXChoice GetAssessmentItemChoice(int pAssessmentItemChoiceID)
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.AssessmentItemXChoices.Where(o => o.ID == pAssessmentItemChoiceID);
                if (query != null && query.Count() > 0)
                {
                    List<DataAccess.AssessmentItemXChoice> list_temp = query.ToList();
                    if (list_temp != null && list_temp.Count > 0)
                        return list_temp[0];
                }
            }
            return null;
        }

        public DataAccess.AssessmentItemXChoice GetAssessmentItemChoiceFromCache(int pAssessmentItemChoiceID)
        {
            //**** 2 ****  Get item from cache - if exists!
            object cache_entry = Cache_Manager.Get_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_CHOICE, pAssessmentItemChoiceID);
            DataAccess.AssessmentItemXChoice objChoice = null;

            if (cache_entry == Cache_Manager.ERR_CACHE)
            {
                //**** 3 ****  Cache failed for some reason; go directly to DB
                objChoice = GetAssessmentItemChoice(pAssessmentItemChoiceID);
            }
            else if (cache_entry == Cache_Manager.NO_CACHE)
            {
                //**** 4 ****  cache miss; get from DB and add to cache; after that return the object directly
                DataAccess.AssessmentItemXChoice obj = GetAssessmentItemChoice(pAssessmentItemChoiceID);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_CHOICE, pAssessmentItemChoiceID, obj as object);
                objChoice = obj;
            }
            else
            {
                //**** 5 ****  return from cache
                if (cache_entry == null)
                    return null;

                objChoice = (cache_entry as DataAccess.AssessmentItemXChoice);
            }
            return objChoice;
        }

        public bool Save_List_Assessment_Item_X_Choice(List<DataAccess.AssessmentItemXChoice> pListObj)
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    foreach (var item in pListObj)
                    {
                        DataAccess.AssessmentItemChoice objChoice = item.AssessmentItemChoice;
                        context.DeferredLoadingEnabled = true;
                        if (objChoice != null)
                            context.AssessmentItemChoices.InsertOnSubmit(objChoice);

                        item.AssessmentItemChoice = objChoice;
                        context.AssessmentItemXChoices.InsertOnSubmit(item);
                    }
                    context.SubmitChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }

        }

        public bool Save_Assessment_Item_X_Choice(DataAccess.AssessmentItemXChoice pObj, DataAccess.AssessmentItemChoice pObjChoice)
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    context.DeferredLoadingEnabled = true;
                    if (pObjChoice != null)
                        context.AssessmentItemChoices.InsertOnSubmit(pObjChoice);

                    pObj.AssessmentItemChoice = pObjChoice;
                    context.AssessmentItemXChoices.InsertOnSubmit(pObj);
                    context.SubmitChanges();
                    Invalidate_AssessmentItemChoices(pObj.AssessmentItemID);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }
        }

        public bool DeleteAssessmentItemXChoice(int pAssessmentItemXChoiceID)
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    DataAccess.AssessmentItemXChoice obj = GetAssessmentItemChoiceFromCache(pAssessmentItemXChoiceID);
                    if (obj == null)
                    {
                        CommonUtilities.Log("No AssessmentItemXChoice item found! No delete process! AssessmentItemXChoice ID : " + pAssessmentItemXChoiceID.ToString());
                        return false;
                    }
                    context.DeferredLoadingEnabled = true;
                    context.AssessmentItemXChoices.Attach(obj);
                    context.AssessmentItemXChoices.DeleteOnSubmit(obj);
                    context.SubmitChanges();
                    Invalidate_Assessment_Item_X_Choice_From_Cache(pAssessmentItemXChoiceID);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }
        }

        public static void Invalidate_Assessment_Item_X_Choice_From_Cache(int pAssessmentItemXChoiceID)
        {
            Cache_Manager.Drop_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_CHOICE, pAssessmentItemXChoiceID.ToString());
        }

        public static void Invalidate_AssessmentItemChoices(int pAIId)
        {
            Cache_Manager.Drop_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_CHOICES, pAIId);
        }
        #endregion

        #region "Assessment and Item Response"
        public bool Close_Assessment_Response(int pAssessmentRespID, int pUserID)
        {
            try
            {
                var context = ContextHandler.GetInstance();
                AssessmentResponse objResponse = GetAssessmentResponse(pUserID, pAssessmentRespID, context);
                if (objResponse == null)
                    return false;

                objResponse.IsCurent = false;
                objResponse.AdministerDate = DateTime.Now;
                using (context)
                {
                    context.SubmitChanges();
                    //Invalidate_Assessment_Item_X_Choice_From_Cache(_AssessmentItemXChoiceID);

                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }
        }

        public bool Save_Assessment_Item_Response(AssessmentItemResponse pObj, int pAssessmentID, ref int pAssessmentResponseID, int pUserID, Constants.enAssessmentItemType pType)
        {
            try
            {
                var context = ContextHandler.GetInstance();
                bool is_update_main = false;
                bool is_update = false;

                AssessmentResponse objResponse = GetAssessmentResponse(pUserID, pAssessmentResponseID, context);
                if (objResponse == null)
                {
                    objResponse = new AssessmentResponse()
                    {
                        AdministerDate = DateTime.Now,
                        IsCurent = true,
                        UserID = pUserID,
                        AssessmentID = pAssessmentID
                    };
                    objResponse.TotalScore += pObj.ScoreValue;
                }
                else
                {
                    is_update_main = true;
                    string _val = pObj.Value;
                    int _score = pObj.ScoreValue;
                    int? _choice_id = -1;
                    if (pType == Constants.enAssessmentItemType.MULTIPLE_CHOICE_CHK || pType == Constants.enAssessmentItemType.GRID_CHK)
                        _choice_id = pObj.ChoiceID;
                    AssessmentItemResponse _obj = GetAssessmentItemResponse(objResponse.ID, pObj.AssessmentItemID, _choice_id, context);
                    _choice_id = pObj.ChoiceID;
                    if (_obj != null && (pType != Constants.enAssessmentItemType.GRID_CHK) && (pType != Constants.enAssessmentItemType.MULTIPLE_CHOICE_CHK))
                    {
                        int _ex_score = pObj.ScoreValue;
                        pObj = _obj;
                        pObj.ScoreValue = _score;
                        pObj.Value = _val;
                        pObj.ChoiceID = _choice_id;
                        objResponse.TotalScore += (_score - _ex_score);

                        is_update = true;
                    }
                    else if (_obj != null && pObj.Value.Equals("0") && ((pType == Constants.enAssessmentItemType.GRID_CHK) || (pType == Constants.enAssessmentItemType.MULTIPLE_CHOICE_CHK)))
                    {
                        //if val equals 0, delete record
                        using (context)
                        {
                            objResponse.TotalScore -= _obj.ScoreValue;
                            context.AssessmentItemResponses.DeleteOnSubmit(_obj);
                            context.SubmitChanges();
                            return true;
                        }
                    }
                    else
                        objResponse.TotalScore += pObj.ScoreValue;
                }


                using (context)
                {
                    if (!is_update_main)
                        context.AssessmentResponses.InsertOnSubmit(objResponse);
                    if (!is_update)
                    {
                        pObj.AssessmentResponse = objResponse;
                        context.AssessmentItemResponses.InsertOnSubmit(pObj);
                    }
                    context.SubmitChanges();
                    pAssessmentResponseID = objResponse.ID;
                    //Invalidate_Assessment_Item_X_Choice_From_Cache(_AssessmentItemXChoiceID);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }
        }

        public List<DataAccess.AssessmentResponse> GetAssessmentResponsesByUserAndAssessmentID(int pUserID, int pAssessmentD)
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    var query = context.AssessmentResponses.Where(o => o.AssessmentID == pAssessmentD && o.UserID == pUserID).OrderByDescending(o => o.AdministerDate);
                    if (query != null && query.Count() > 0)
                    {
                        List<DataAccess.AssessmentResponse> list_temp = query.ToList();
                        if (list_temp != null && list_temp.Count > 0)
                            return list_temp;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return null;
            }
        }

        public static List<DataAccess.AssessmentResponse> GetAssessmentResponses()
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    var query = context.AssessmentResponses.ToList();
                    if (query != null && query.Count() > 0)
                    {
                            return query;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return null;
            }
        }

        public static int GetTotalCompletedAssessmentCount()
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    var query = context.AssessmentResponses.Where(o=> o.IsCurent == false).Count();
                    return query;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return 0;
            }
        }

        public DataAccess.AssessmentResponse GetAssessmentResponse(int pUserID, int pAssessmentRespID, DataAccessLinqDataContext pContext)
        {
            bool is_commit = false;
            var context = pContext;
            if (pContext == null)
            {
                context = ContextHandler.GetInstance();
                context.DeferredLoadingEnabled = true;
                is_commit = true;
            }
            try
            {
                var query = context.AssessmentResponses.Where(o => o.ID == pAssessmentRespID && o.UserID == pUserID).OrderByDescending(o => o.AdministerDate);
                if (query != null && query.Count() > 0)
                {
                    List<DataAccess.AssessmentResponse> list_temp = query.ToList();
                    if (list_temp != null && list_temp.Count > 0)
                        return list_temp[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return null;
            }
            finally
            {
                if (is_commit)
                    context.Dispose();
            }

        }

        public static List<DataAccess.AssessmentItemResponse> GetItemResponsesByAssessmentRespID(int pAssessmentRespID)
        {
            try
            {
                using (var context = ContextHandler.GetInstance())
                {
                    var query = context.AssessmentItemResponses.Where(o => o.AssessmentResponseID == pAssessmentRespID);
                    if (query != null && query.Count() > 0)
                    {
                        List<DataAccess.AssessmentItemResponse> list_temp = query.ToList();
                        if (list_temp != null && list_temp.Count > 0)
                            return list_temp;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return null;
            }
        }

        public static void FillAIRCache()
        {
            List<DataAccess.AssessmentResponse> listAir = GetAssessmentResponses();
            foreach (var item in listAir)
            {
                List<DataAccess.AssessmentItemResponse> _listAIR = GetItemResponsesByAssessmentRespID(item.ID);
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_RESPONSE, item.ID, _listAIR as object);
            }
        }

        public List<DataAccess.AssessmentItemResponse> GetItemResponsesByAssessmentRespID_FromCache(int pAssessmentRespID)
        {
            //**** 2 ****  Get item from cache - if exists!
            object cache_entry = Cache_Manager.Get_Entry_From_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_RESPONSE, pAssessmentRespID);
            List<DataAccess.AssessmentItemResponse> listAIR = null;

            if (cache_entry == Cache_Manager.ERR_CACHE)
            {
                //**** 3 ****  Cache failed for some reason; go directly to DB
                listAIR = GetItemResponsesByAssessmentRespID(pAssessmentRespID);
            }
            else if (cache_entry == Cache_Manager.NO_CACHE)
            {
                //**** 4 ****  cache miss; get from DB and add to cache; after that return the object directly
                List<DataAccess.AssessmentItemResponse> _listAIR = GetItemResponsesByAssessmentRespID(pAssessmentRespID); 
                Cache_Manager.Add_Entry_To_Cache(Cache_Manager.enCacheTypes.ASS_ASSESSMENT_ITEM_RESPONSE, pAssessmentRespID, _listAIR as object);
                listAIR = _listAIR;
            }
            else
            {
                //**** 5 ****  return from cache
                if (cache_entry == null)
                    return null;

                listAIR = (cache_entry as List<DataAccess.AssessmentItemResponse>);
            }
            return listAIR;
        }

        public DataAccess.AssessmentItemResponse GetAssessmentItemResponse(int pAssessmentResponseID, int pAssessmentItemID, int? pChoiceID, DataAccessLinqDataContext pContext)
        {
            //check from cache
            List<DataAccess.AssessmentItemResponse> listAIR = GetItemResponsesByAssessmentRespID_FromCache(pAssessmentResponseID);
            if(listAIR != null && listAIR.Count > 0)
            {
                listAIR = listAIR.Where(o => o.AssessmentResponseID == pAssessmentResponseID && o.AssessmentItemID == pAssessmentItemID && (pChoiceID > 0 ? o.ChoiceID == pChoiceID : true)).ToList();
                if (listAIR != null && listAIR.Count() > 0)
                    return listAIR[0];
            }

            bool is_commit = false;
            var context = pContext;
            if (pContext == null)
            {
                context = ContextHandler.GetInstance();
                context.DeferredLoadingEnabled = true;
                context.DeferredLoadingEnabled = false;
                is_commit = true;
            }
            try
            {
                var query = context.AssessmentItemResponses.Where(o => o.AssessmentResponseID == pAssessmentResponseID && o.AssessmentItemID == pAssessmentItemID && (pChoiceID > 0 ? o.ChoiceID == pChoiceID : true));

                if (query != null && query.Count() > 0)
                {
                    List<DataAccess.AssessmentItemResponse> list_temp = query.ToList();
                    if (list_temp != null && list_temp.Count > 0)
                        return list_temp[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return null;
            }
            finally
            {
                if (is_commit)
                    context.Dispose();
            }
        }



        #endregion

        private TempAIDisplayFormula GetTempAIDisplayRuleFormulas(int pAID)
        {
            using (var context = ContextHandler.GetInstance())
            {
                context.DeferredLoadingEnabled = false;
                var query = context.TempAIDisplayFormulas.Where(o => o.AIDisplayFormulaID == pAID && o.UserID == gUserID);
                if (query != null && query.Count() > 0)
                    return query.ToList()[0];
            }
            return null;
        }

        public bool UpdateAIDisplayFormula(int pRelatedAIID, int pAssessmentItemID, string pValue)
        {
            try
            {
                string _asse_id = "AIID_" + pAssessmentItemID.ToString();
                string _asse_id2 = "AIID_" + pAssessmentItemID.ToString() + "_v_";

                AssessmentItem objAI_F = GetAssessmentItemByItemIDFromCache(pRelatedAIID);
                string _formula = "";
                TempAIDisplayFormula _tempFormula = GetTempAIDisplayRuleFormulas(pRelatedAIID);
                if (_tempFormula == null)
                    _formula = objAI_F.DisplayCondition;
                else
                    _formula = _tempFormula.AIDisplayFormulaText;

                if (_formula.Contains(_asse_id))
                {
                    if (_formula.Contains(_asse_id2))
                    {
                        string _val = string.Empty;
                        int start_index = _formula.IndexOf(_asse_id2);
                        _val = _formula.Substring(start_index);
                        int _len = _val.IndexOf("#") + 3;
                        _val = _val.Substring(0, _len);
                        _formula = _formula.Replace(_val, _asse_id2 + pValue + "#_#");
                    }
                    else
                        _formula = _formula.Replace(_asse_id, _asse_id + "_v_" + pValue + "#_#");

                    using (var context = ContextHandler.GetInstance())
                    {
                        var objFRF = context.TempAIDisplayFormulas.FirstOrDefault(o => o.AIDisplayFormulaID == pRelatedAIID && o.UserID == gUserID);
                        if (objFRF == null)
                        {
                            objFRF = new TempAIDisplayFormula();
                            objFRF.AIDisplayFormulaID = pRelatedAIID;
                            objFRF.AIDisplayFormulaText = _formula;
                            objFRF.UserID = gUserID;
                            context.TempAIDisplayFormulas.InsertOnSubmit(objFRF);
                        }
                        else
                        {
                            objFRF.AIDisplayFormulaText = _formula;
                        }
                        context.SubmitChanges();
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                CommonUtilities.Log(ex);
                return false;
            }
        }

        public void ExecuteFormula(int pAssessmentItemID, string pValue, ref bool pIsAIDisplayExists)
        {
            pIsAIDisplayExists = false;
            AssessmentItem objAI = GetAssessmentItemByItemIDFromCache(pAssessmentItemID);
            if (objAI != null)
            {
                TempAIDisplayFormula objAID = GetTempAIDisplayRuleFormulas(pAssessmentItemID);
                if (objAID == null)
                    return;

                string _formula = objAID.AIDisplayFormulaText;
                string _val = "";
                while (_formula.Contains("AIID_"))
                {
                    int start_index = _formula.IndexOf("AIID_");
                    _val = _formula.Substring(start_index);
                    if (_val.IndexOf("_v_") < 0)
                    {
                        _formula = _formula.Insert(start_index + 2, "xx");
                        continue;
                    }
                    int _len = _val.IndexOf("_v_") + 3;
                    int _checkpoint_len = _val.IndexOf("#_#") + 3;
                    string _check_val = _val.Substring(_len, _checkpoint_len - _len - 3);
                    _val = _val.Substring(0, _len);
                    int _temp_val = -1;
                    if (int.TryParse(_check_val, out _temp_val))
                        _formula = _formula.Replace(_val, "");
                    else
                    {
                        //convert the first _aiid to xx
                        _formula = _formula.Replace(_formula.Substring(start_index, 5), "xx");
                    }
                }
                _formula = _formula.Replace("#_#", "");
                try
                {
                    string _res = ScriptEngine.Eval("Jscript", _formula).ToString();
                    pIsAIDisplayExists = Convert.ToBoolean(_res);
                }
                catch
                {
                }

            }
        }
    }

  
}
