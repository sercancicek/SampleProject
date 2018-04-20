using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace SP.Common
{
    public class BSWSession
    {
        private HttpSessionState gSession;

        public int UserID
        {
            get
            {
                if (gSession["UserID"] != null)
                    return Convert.ToInt32(gSession["UserID"]);
                return -1;
            }
            set
            {
                gSession["UserID"] = value;
            }
        }

        public int UserRole
        {
            get
            {
                if (gSession["UserRole"] != null)
                    return Convert.ToInt32(gSession["UserRole"]);
                return -1;
            }
            set
            {
                gSession["UserRole"] = value;
            }
        }
        public int ActivationCodeId
        {
            get
            {
                if (gSession["ActivationCodeId"] != null)
                    return Convert.ToInt32(gSession["ActivationCodeId"]);
                return -1;
            }
            set
            {
                gSession["ActivationCodeId"] = value;
            }
        }
        public string SessionID
        {
            get
            {
                if (gSession["SessionID"] != null)
                    return gSession["SessionID"].ToString();
                return "";
            }
            set
            {
                gSession["SessionID"] = value;
            }
        }
        public string CoverPhotoPath
        {
            get
            {
                if (gSession["CoverPhotoPath"] != null)
                    return gSession["CoverPhotoPath"].ToString();
                return "";
            }
            set
            {
                gSession["CoverPhotoPath"] = value;
            }
        }

        public string SessionKey
        {
            get
            {
                if (gSession["SessionID"] != null)
                    return gSession["SessionID"].ToString().Substring(0, 8);

                return "";
            }
        }

        public string UserFullName
        {
            get
            {
                if (gSession["UserFullName"] != null)
                    return gSession["UserFullName"].ToString();
                return "";
            }
            set
            {
                gSession["UserFullName"] = value;
            }
        }
        public string UserFirstName
        {
            get
            {
                if (gSession["UserFirstName"] != null)
                    return gSession["UserFirstName"].ToString();
                return "";
            }
            set
            {
                gSession["UserName"] = value;
            }
        }
        public string UserMiddleName
        {
            get
            {
                if (gSession["UserMiddleName"] != null)
                    return gSession["UserMiddleName"].ToString();
                return "";
            }
            set
            {
                gSession["UserName"] = value;
            }
        }
        public string UserAvatarPath
        {
            get
            {
                if (gSession["UserAvatarPath"] != null)
                    return gSession["UserAvatarPath"].ToString();
                return "";
            }
            set
            {
                gSession["UserAvatarPath"] = value;
            }
        }

        public bool IsNewUser
        {
            get
            {
                if (gSession["IsNewUser"] != null)
                    return Convert.ToBoolean(gSession["IsNewUser"].ToString());
                return false;
            }
            set
            {
                gSession["IsNewUser"] = value;
            }
        }

        public string UserEmail
        {
            get
            {
                if (gSession["userEmail"] != null)
                    return gSession["userEmail"].ToString();
                return "";
            }
            set
            {
                gSession["userEmail"] = value;
            }
        }

        public string IPAddress
        {
            get
            {
                if (gSession["IPAddress"] != null)
                    return gSession["IPAddress"].ToString();
                return "";
            }
            set
            {
                gSession["IPAddress"] = value;
            }
        }

     
        public BSWSession(HttpSessionState pSession)
        {
            this.gSession = pSession;
        }

        public clsSessionBaseData Get_SessionBaseData()
        {
            return new clsSessionBaseData(this);
        }
      
        public void ClearSession()
        {
            gSession.Clear();
        }

        public HttpSessionState GetSession()
        {
            return gSession;
        }

    }



    public class clsSessionBaseData
    {
        //*********  only basic type (int, string, bool vs.) and static variables after LOGIN          ***************
       
        public string SessionID;
        public int UserID;
        public string SessionKey;

        public clsSessionBaseData(string pSessionID, int pUserID)
        {
            SessionID = pSessionID;
            UserID = pUserID;
            SessionKey = pSessionID.Substring(0, 8);
        }

        public clsSessionBaseData(BSWSession pBSWSession)
        {
            SessionID = pBSWSession.SessionID;
            UserID = pBSWSession.UserID;
            SessionKey = pBSWSession.SessionID.Substring(0, 8);
        }
    }
}