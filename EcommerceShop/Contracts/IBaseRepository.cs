using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceShop.Contracts
{   
    public enum ErrorCode
    {
        Success,
        Error
    }
    public interface IBaseRepository<T>
    {
        T Get(object id);
        List<T> GetAll();
        ErrorCode Create(T t, out String errorMsg);
        ErrorCode Update(object id, T t);
        ErrorCode Delete(object id);
    }
}
