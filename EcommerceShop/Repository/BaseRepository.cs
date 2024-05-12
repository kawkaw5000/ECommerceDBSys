using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EcommerceShop.Contracts;
using System.Data.Entity;
using EcommerceShop.DAL;

namespace EcommerceShop.Repository
{
    public class BaseRepository<T> : IBaseRepository<T>
        where T : class
    {
        private DbContext _db;
        private DbSet<T> _table;
        public IQueryable<T> Table => _table;
        public BaseRepository()
        {
            _db = new dbMyOnlineShoppingEntities();
            _table = _db.Set<T>();
        }

        public BaseRepository(dbMyOnlineShoppingEntities _db)
        {
            this._db = _db;
        }

        public T Get(object id)
        {
            return _table.Find(id);
        }

        public List<T> GetAll()
        {
            return _table.ToList();
        }
        public Contracts.ErrorCode Create(T t, out string errorMsg)
        {
            try
            {
                _table.Add(t);
                _db.SaveChanges();
                errorMsg = "Success";

                return Contracts.ErrorCode.Success;
            }
            catch (Exception ex)
            {
                errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Contracts.ErrorCode.Error;
            }
        }

        public Contracts.ErrorCode Delete(object id)
        {
            try
            {
                var obj = Get(id);
                _table.Remove(obj);
                _db.SaveChanges();
                return Contracts.ErrorCode.Success;
            }
            catch (Exception ex)
            {

                return Contracts.ErrorCode.Error;
            }
        }

        

        public Contracts.ErrorCode Update(object id, T t)
        {
            try
            {
                var oldOjb = Get(id);
                _db.Entry(oldOjb).CurrentValues.SetValues(t);
                _db.SaveChanges();
                return Contracts.ErrorCode.Success;
            }
            catch (Exception ex)
            {

                return Contracts.ErrorCode.Error;
            }
        }
    }
}