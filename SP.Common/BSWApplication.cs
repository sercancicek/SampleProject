using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SP.Common
{
    public class BSWApplication
    {
        private HttpApplicationState Application;

        public void SetApplication(HttpApplicationState iApplication)
        {
            Application = iApplication;
        }

        public HttpApplicationState GetApplication()
        {
            return Application;
        }

        public bool Is_This_Session_Timed_Out(string pSessionId, int pTimeOut)
        {
            int pRemainingMinutes = -1;
            System.DateTime activity_date = DateTime.MinValue;
            try
            {
                activity_date = SessionActivityInfo[pSessionId];
            }
            catch (Exception)
            {
            }

            if ((activity_date == DateTime.MinValue))
            {
                return true;
            }

            return TimedOut(activity_date, pTimeOut,ref pRemainingMinutes);
        }

        public bool Will_This_Session_Time_Out(string pSessionId, int pTimeOut, int pWarnBeforeMinutes, ref int pRemainingMinutes)
        {
            pRemainingMinutes = -1;

            int pMinutesPassedSinceLastActivity = -1;
            System.DateTime activity_date = DateTime.MinValue;
            try
            {
                activity_date = SessionActivityInfo[pSessionId];
            }
            catch (Exception)
            {
            }

            if ((activity_date == DateTime.MinValue))
            {
                return true;
            }

            bool will_time_out = TimedOut(activity_date, pTimeOut - pWarnBeforeMinutes, ref pMinutesPassedSinceLastActivity);
            if (will_time_out)
            {
                pRemainingMinutes = pTimeOut - pMinutesPassedSinceLastActivity;
                return true;
            }
            else
            {
                return false;
            }
        }

        private Dictionary<string, System.DateTime> SessionActivityInfo
        {
            get
            {
                if (Application == null || Application["SessionActivityInfo"] == null)
                {
                    Application.Lock();
                    Application["SessionActivityInfo"] = new Dictionary<string, System.DateTime>();
                    Application.UnLock();
                }
                return (Dictionary<string, System.DateTime>)Application["SessionActivityInfo"];
            }
        }
        
        public Dictionary<int, clsUserSessionData> ActiveUsersDict
        {
            get
            {
                if (Application == null || Application["ActiveUsersList"] == null)
                {
                    Application.Lock();
                    Application["ActiveUsersList"] = new Dictionary<int, clsUserSessionData>();
                    Application.UnLock();
                }
                return (Dictionary<int, clsUserSessionData>)Application["ActiveUsersList"];
            }
        }


        public  Dictionary<string, List<string>> ActiveUsersList(int pUserID)
        {
           
                Dictionary<int, clsUserSessionData> obj = ActiveUsersDict;
                if ((obj == null))
                {
                    return null;
                }

                try
                {
                    if (obj.ContainsKey(pUserID))
                    {
                        return obj[pUserID].Dict;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
        }

        public class clsUserSessionData
        {
            public long Login_Time_Ticks;

            public Dictionary<string, List<string>> Dict;
            public clsUserSessionData()
            {
                Dict = new Dictionary<string, List<string>>();
                Login_Time_Ticks = DateTime.Now.Ticks;
            }
        }


        private bool TimedOut(System.DateTime pDate, int pTimeOut, ref int pMinutesPassedSinceLastActivity)
        {
            try
            {
                
                pMinutesPassedSinceLastActivity = Convert.ToInt32((DateTime.Now - pDate).TotalMinutes);
                if (pMinutesPassedSinceLastActivity > pTimeOut)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                pMinutesPassedSinceLastActivity = -1;
                return false;
            }

            pMinutesPassedSinceLastActivity = -1;
            return false;
        }

        public void Add_Active_User(int pUserId, string pSessionId, string pIPAddress, bool pAllowMultipleSessionsFromSameIP)
        {
            List<string> ips_to_kick = new List<string>();
            bool is_new = false;

            Application.Lock();

            if (ActiveUsersDict.ContainsKey(pUserId) == false)
            {
                is_new = true;
                ActiveUsersDict.Add(pUserId, new clsUserSessionData());
            }

            Dictionary<string, List<string>> dictUserStatus = ActiveUsersList(pUserId);

            if (is_new == false)
            {
                //request gFrom_br dif. IP addresses
                foreach (string ip in dictUserStatus.Keys)
                {
                    //kicks out all existing entries for this user gFrom_br other IP addresses
                    if (ip != pIPAddress)
                    {
                        ips_to_kick.Add(ip);
                    }
                }

                foreach (string ip in ips_to_kick)
                {
                    dictUserStatus.Remove(ip);
                }

                //same IP different sessions (optional)
                if (pAllowMultipleSessionsFromSameIP == false)
                {
                    ips_to_kick = new List<string>();
                    foreach (string ip in dictUserStatus.Keys)
                    {
                        List<string> sess_ids_to_kick = new List<string>();
                        foreach (string sess_id in dictUserStatus[ip])
                        {
                            if (pSessionId != sess_id)
                            {
                                sess_ids_to_kick.Add(sess_id);
                            }
                        }

                        if (sess_ids_to_kick.Count > 0)
                        {
                            foreach (string sess_id_to_kick in sess_ids_to_kick)
                            {
                                dictUserStatus[ip].Remove(sess_id_to_kick);
                            }
                            if (dictUserStatus[ip].Count == 0)
                            {
                                ips_to_kick.Add(ip);
                            }
                        }
                    }

                    foreach (string ip in ips_to_kick)
                    {
                        dictUserStatus.Remove(ip);
                    }
                }
            }

            //if this IP address does not exist, add it
            if (dictUserStatus.ContainsKey(pIPAddress) == false)
            {
                dictUserStatus.Add(pIPAddress, new List<string>());
            }

            dictUserStatus[pIPAddress].Add(pSessionId);

            if (SessionActivityInfo.ContainsKey(pSessionId) == false)
            {
                SessionActivityInfo.Add(pSessionId, DateTime.Now);
            }
            else
            {
                SessionActivityInfo[pSessionId] = DateTime.Now;
            }

            Application.UnLock();
        }

        //used when session end called gFrom_br global.asa and refreshes the list if the user did not went through logout page (i.e. timed out etc)
        public void Remove_Active_User_From_Session_End(string pSessionId, string pIPAddress)
        {
            Application.Lock();

            int user_id_to_remove = int.MinValue;

            try
            {

                foreach (int user_id in ActiveUsersDict.Keys)
                {
                    Dictionary<string, List<string>> user_id_dict = ActiveUsersList(user_id);

                    if (user_id_dict.ContainsKey(pIPAddress))
                    {
                        if (user_id_dict[pIPAddress].Contains(pSessionId))
                        {
                            user_id_dict[pIPAddress].Remove(pSessionId);

                            if (user_id_dict[pIPAddress].Count == 0)
                            {
                                //if we removed last session id for this ip, remove the ip as well
                                user_id_dict.Remove(pIPAddress);
                            }

                            if (user_id_dict.Keys.Count == 0)
                            {
                                //if we removed last ip for this user, remove the user 
                                user_id_to_remove = user_id;
                            }

                            break; // TODO: might not be correct. Was : Exit For
                        }
                    }
                }

                if (user_id_to_remove > 0)
                {
                    ActiveUsersDict.Remove(user_id_to_remove);
                }

                if (SessionActivityInfo.ContainsKey(pSessionId))
                {
                    SessionActivityInfo.Remove(pSessionId);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Application.UnLock();
            }
        }

        //used gTo_br kick a user out i.e. when user is deleted
        public void Remove_Active_User(int pUserId)
        {
            try
            {
                Dictionary<int, clsUserSessionData> dictUser = ActiveUsersDict;

                if ((dictUser == null) == false)
                {
                    Application.Lock();
                    if (dictUser.ContainsKey(pUserId))
                    {
                        dictUser.Remove(pUserId);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Application.UnLock();
            }
        }

        //used when a user logs out
        public void Remove_Active_User(int pUserId, string pSessionId, string pIPAddress)
        {
            Application.Lock();

            try
            {
                if ((ActiveUsersList(pUserId) == null) == false)
                {
                    if (ActiveUsersList(pUserId).ContainsKey(pIPAddress))
                    {
                        if (ActiveUsersList(pUserId)[pIPAddress].Contains(pSessionId))
                        {
                            ActiveUsersList(pUserId)[pIPAddress].Remove(pSessionId);
                        }

                        if (ActiveUsersList(pUserId)[pIPAddress].Count == 0)
                        {
                            //if we removed last session id for this ip, remove the ip as well
                            ActiveUsersList(pUserId).Remove(pIPAddress);
                        }

                        if (ActiveUsersList(pUserId).Keys.Count == 0)
                        {
                            //if we removed last ip for this user, remove the user 
                            ActiveUsersDict.Remove(pUserId);
                        }
                    }
                }

                if (SessionActivityInfo.ContainsKey(pSessionId))
                {
                    SessionActivityInfo.Remove(pSessionId);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Application.UnLock();
            }
        }

        private void Kick_Inactive_Users(int pTimeOut)
        {
            List<int> user_list = new List<int>();
            List<string> ip_list = new List<string>();
            List<string> session_id_list = new List<string>();
            int dummy = -1;

            try
            {
                //kick inactive sessions first
                foreach (int u in ActiveUsersDict.Keys)
                {
                    foreach (string ip in ActiveUsersList(u).Keys)
                    {
                        foreach (string s in ActiveUsersList(u)[ip])
                        {
                            if (SessionActivityInfo.ContainsKey(s) && TimedOut(SessionActivityInfo[s], pTimeOut,ref dummy))
                            {
                                user_list.Add(u);
                                ip_list.Add(ip);
                                session_id_list.Add(s);
                            }
                        }
                    }
                }

                for (int i = 0; i <= user_list.Count - 1; i++)
                {
                    Remove_Active_User(user_list[i], session_id_list[i], ip_list[i]);
                }
            }
            catch (Exception)
            {
            }
        }

        public int Get_No_Of_Active_Users(int pTimeOut)
        {
            Kick_Inactive_Users(pTimeOut);
            return ActiveUsersDict.Keys.Count;
        }

        public bool Check_If_User_Active(int pUserId, string pSessionId, string pIPAddress, bool No_Activity_Update = false)
        {
            if ((ActiveUsersList(pUserId) == null) == false)
            {
                if (ActiveUsersList(pUserId).ContainsKey(pIPAddress))
                {
                    if (ActiveUsersList(pUserId)[pIPAddress].Contains(pSessionId))
                    {
                        if (SessionActivityInfo.ContainsKey(pSessionId))
                        {
                            if (No_Activity_Update == false)
                            {
                                SessionActivityInfo[pSessionId] = DateTime.Now;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }


    }
}
