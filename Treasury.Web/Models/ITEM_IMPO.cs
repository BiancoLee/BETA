//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Treasury.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ITEM_IMPO
    {
        public string ITEM_ID { get; set; }
        public string INVENTORY_STATUS { get; set; }
        public string ITEM_NAME { get; set; }
        public int QUANTITY { get; set; }
        public int REMAINING { get; set; }
        public Nullable<decimal> AMOUNT { get; set; }
        public Nullable<System.DateTime> EXPECTED_ACCESS_DATE { get; set; }
        public string DESCRIPTION { get; set; }
        public string MEMO { get; set; }
        public string APLY_DEPT { get; set; }
        public string APLY_SECT { get; set; }
        public string APLY_UID { get; set; }
        public string CHARGE_DEPT { get; set; }
        public string CHARGE_SECT { get; set; }
        public Nullable<System.DateTime> PUT_DATE { get; set; }
        public Nullable<System.DateTime> GET_DATE { get; set; }
        public Nullable<System.DateTime> LAST_UPDATE_DT { get; set; }
        public string ITEM_NAME_AFT { get; set; }
        public Nullable<int> REMAINING_AFT { get; set; }
        public Nullable<decimal> AMOUNT_AFT { get; set; }
        public Nullable<System.DateTime> EXPECTED_ACCESS_DATE_AFT { get; set; }
        public string DESCRIPTION_AFT { get; set; }
        public string MEMO_AFT { get; set; }
        public string CHARGE_DEPT_AFT { get; set; }
        public string CHARGE_SECT_AFT { get; set; }
        public string ITEM_ID_FROM { get; set; }
        public string ITEM_NAME_ACCESS { get; set; }
        public Nullable<int> QUANTITY_ACCESS { get; set; }
        public Nullable<decimal> AMOUNT_ACCESS { get; set; }
        public Nullable<System.DateTime> EXPECTED_ACCESS_DATE_ACCESS { get; set; }
        public string DESCRIPTION_ACCESS { get; set; }
        public string MEMO_ACCESS { get; set; }
    }
}
