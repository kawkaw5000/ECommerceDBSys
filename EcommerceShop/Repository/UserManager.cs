using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using EcommerceShop.DAL;
using EcommerceShop.Utils;

namespace EcommerceShop.Repository
{
    public class UserManager
    {
        private BaseRepository<Tbl_Members> _userAcc;
        private BaseRepository<Tbl_MemberInfo> _userInf;

        public UserManager()
        {
            _userAcc = new BaseRepository<Tbl_Members>();
            _userInf = new BaseRepository<Tbl_MemberInfo>();
        }
        public Tbl_Members GetUserById(int Id)
        {
            return _userAcc.Get(Id);
        }
        public Tbl_Members GetUserByUserId(String userId)
        {
            return _userAcc.Table.Where(m => m.userId == userId).FirstOrDefault();
        }

        public Tbl_Members GetUserByUsername(String username)
        {
            return _userAcc.Table.Where(m => m.Username == username).FirstOrDefault();
        }
        public Tbl_Members GetUserByEmail(String email)
        {
            return _userAcc.Table.Where(m => m.EmailId == email).FirstOrDefault();
        }
        public ErrorCode SignIn(String username, String password, ref String errMsg)
        {
            var userSignIn = GetUserByUsername(username);
            if (userSignIn == null)
            {
                errMsg = "User not exist!";
                return ErrorCode.Error;
            }

            if (!userSignIn.Password.Equals(password))
            {
                errMsg = "Password is Incorrect";
                return ErrorCode.Error;
            }

            // user exist
            errMsg = "Login Successful";
            return ErrorCode.Success;
        }

        public Contracts.ErrorCode SignUp(Tbl_Members ua, ref String errMsg)
        {
            ua.userId = Utilities.gUid;        
            ua.CreatedOn = DateTime.Now;
            ua.IsDelete = true;     
            
            if (GetUserByUsername(ua.Username) != null)
            {
                errMsg = "Username Already Exist";
                return Contracts.ErrorCode.Error;
            }

            if (GetUserByEmail(ua.EmailId) != null)
            {
                errMsg = "Email Already Exist";
                return Contracts.ErrorCode.Error;
            }

            if (_userAcc.Create(ua, out errMsg) != Contracts.ErrorCode.Success)
            {
                return Contracts.ErrorCode.Error;
            }

            return Contracts.ErrorCode.Success;
        }

      
    }
}