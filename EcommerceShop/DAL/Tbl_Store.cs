
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace EcommerceShop.DAL
{

using System;
    using System.Collections.Generic;
    
public partial class Tbl_Store
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Tbl_Store()
    {

        this.Tbl_MemberInfo = new HashSet<Tbl_MemberInfo>();

        this.Tbl_Product = new HashSet<Tbl_Product>();

        this.Tbl_Transaction = new HashSet<Tbl_Transaction>();

        this.Tbl_Cart = new HashSet<Tbl_Cart>();

        this.Tbl_Members = new HashSet<Tbl_Members>();

    }


    public int StoreId { get; set; }

    public string storeName { get; set; }

    public string phone { get; set; }

    public string email { get; set; }

    public string street { get; set; }

    public string city { get; set; }

    public string states { get; set; }

    public string zip_code { get; set; }

    public Nullable<bool> IsActive { get; set; }

    public Nullable<bool> IsDelete { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tbl_MemberInfo> Tbl_MemberInfo { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tbl_Product> Tbl_Product { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tbl_Transaction> Tbl_Transaction { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tbl_Cart> Tbl_Cart { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tbl_Members> Tbl_Members { get; set; }

}

}
