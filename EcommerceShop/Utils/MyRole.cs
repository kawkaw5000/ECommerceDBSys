using EcommerceShop.DAL;
using EcommerceShop.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace EcommerceShop.Utils
{
    public class MyRole : RoleProvider
    {
        public BaseRepository<Tbl_Roles> _role = new BaseRepository<Tbl_Roles>();
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            return _role.GetAll().Select(m => m.RoleName).ToArray();
        }

        public override string[] GetRolesForUser(string Username)
        {
            dbMyOnlineShoppingEntities db = new dbMyOnlineShoppingEntities();         
            return db.vw_UserRole.Where(m => m.Username == Username).Select(m => m.RoleName).ToArray();
            
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}