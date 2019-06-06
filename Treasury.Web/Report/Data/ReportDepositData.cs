using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Report.Interface;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Report.Data
{
    public abstract class ReportDepositData : IReportData
    {
        protected static string defaultConnection { get; private set; }
        public List<reportParm> extensionParms { get; set; }

        protected List<string> INVENTORY_STATUSs = new List<string>()
        {
           ((int)AccessInventoryType._1).ToString() , //在庫
           ((int)AccessInventoryType._4).ToString() , //預約取出
           ((int)AccessInventoryType._8).ToString() , //資料庫異動中
        };

        protected string INVENTORY_STATUSg = "2";

        public ReportDepositData()
        {
            extensionParms = new List<reportParm>();
            defaultConnection = System.Configuration.ConfigurationManager.
                         ConnectionStrings["dbTreasury"].ConnectionString;
        }

        public abstract DataSet GetData(List<reportParm> parms);

        protected REC _REC { get; private set; }

        protected void SetDetail(string aply_No, string isNTD, string vDep_Type)
        {
            _REC = new REC();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {  
                _REC.APLY_NO = aply_No;
                _REC.CURRENCY = isNTD == "Y" ? "台幣" : "外幣";

                _REC.DEP_TYPE = vDep_Type;

                //_REC.SYS_TYPE = DateTime.Now.DateToTaiwanDate(9);
                _REC.SYS_TYPE = TypeTransfer.dateTimeToString(DateTime.Now,false);

                //取得承作日期
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                    .FirstOrDefault(x => x.APLY_NO == aply_No);

                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去定期存單庫存資料檔抓取資料
                    var _IDOM_DataList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    _REC.COMMIT_DATE = TypeTransfer.dateTimeToString(_IDOM_DataList
                        .Where(x => x.CURRENCY == "NTD", isNTD == "Y")
                        .Where(x => x.CURRENCY != "NTD", isNTD == "N")
                        .Where(x => x.DEP_TYPE == vDep_Type, vDep_Type != "0")
                        .Select(x => x.COMMIT_DATE).FirstOrDefault(),false);
                    _REC.APLY_STATUS = getStatue(_TAR.APLY_STATUS); //申請狀態
                }
            }
        }

        protected void SetExtensionParm()
        {
            foreach (var item in _REC.GetType().GetProperties())
            {
                extensionParms.Add(new reportParm()
                {
                    key = item.Name,
                    value = item.GetValue(_REC)?.ToString(),
                });
            }
        }

        protected void SetDetail(string aply_No)
        {
            var depts = new List<VW_OA_DEPT>();
            var emps = new List<V_EMPLY2>();
            _REC = new REC();
            //var sys
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                depts = dbINTRA.VW_OA_DEPT.AsNoTracking().Where(x => x.DPT_CD != null).ToList();
                emps = dbINTRA.V_EMPLY2.AsNoTracking().ToList();
            }
        }

        protected class REC
        {
            public REC() {
                DEP_SET_QUALITY = "N";
            }

            [Description("申請單號")]
            public string APLY_NO { get; set; }

            [Description("台幣/外幣")]
            public string CURRENCY { get; set; }

            [Description("存單類型")]
            public string DEP_TYPE { get; set; }

            [Description("交割日期")]
            public string SYS_TYPE { get; set; }

            [Description("承作日期")]
            public string COMMIT_DATE { get; set; }

            [Description("設質否")]
            public string DEP_SET_QUALITY { get; set; }

            [Description("申請狀態")]
            public string APLY_STATUS { get; set; }

        }

        //修正 欄位型態 by mark 20180903
        protected class DepositReportData
        {
            [Description("類型")]
            public string TYPE { get; set; }

            [Description("物品編號")]
            public string ITEM_ID { get; set; }

            [Description("幣別")]
            public string CURRENCY { get; set; }

            [Description("到期日")]
            public string EXPIRY_DATE { get; set; }

            [Description("交易對象")]
            public string TRAD_PARTNERS { get; set; }

            [Description("存單類型")]
            public string DEP_TYPE { get; set; }

            [Description("存單類型(中文)")]
            public string DEP_TYPE_D { get; set; }

            [Description("存單號碼(起)")]
            public string DEP_NO_B { get; set; }

            [Description("存單號碼(迄)")]
            public string DEP_NO_E { get; set; }

            [Description("張數")]
            public int DEP_CNT { get; set; }

            [Description("單張面額")]
            public decimal? DENOMINATION { get; set; }

            [Description("總面額")]
            public decimal? TOTAL_DENOMINATION { get; set; }

            [Description("項次")]
            public string ISORTBY { get; set; }

            [Description("檢核項目")]
            public string DEP_CHK_ITEM_DESC { get; set; }

            [Description("取出原因")]
            public string MSG { get; set; }

        }


        protected class DepositReportMargingData
        {
            [Description("類別")]
            public string MARGIN_DEP_TYPE { get; set; }

            [Description("歸檔編號")]
            public string ITEM_ID { get; set; }

            [Description("冊號")]
            public string BOOK_NO { get; set; }

            [Description("入庫日期")]
            public string PUT_DATE { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }

            [Description("交易對象")]
            public string TRAD_PARTNERS { get; set; }

            [Description("金額")]
            public decimal? AMOUNT { get; set; }

            [Description("職場代號 ")]
            public string WORKPLACE_CODE { get; set; }

            [Description("說明")]
            public string DESCRIPTION { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            public string CHARGE_DEPT_ID { get; set; }

            public string CHARGE_SECT_ID { get; set; }
        }

        protected class DepositReportMarginpData
        {
            [Description("類別")]
            public string MARGIN_TAKE_OF_TYPE { get; set; }

            [Description("入庫日期")]
            public string PUT_DATE { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }

            [Description("歸檔編號")]
            public string ITEM_ID { get; set; }

            [Description("冊號")]
            public string BOOK_NO { get; set; }

            [Description("交易對象")]
            public string TRAD_PARTNERS { get; set; }

            [Description("金額")]
            public decimal? AMOUNT { get; set; }

            [Description("物品名稱")]
            public string MARGIN_ITEM { get; set; }

            [Description("物品發行人")]
            public string MARGIN_ITEM_ISSUER { get; set; }

            [Description("質押標的號碼")]
            public string PLEDGE_ITEM_NO { get; set; }

            [Description("有效期間(起)")]
            public string EFFECTIVE_DATE_B { get; set; }

            [Description("有效期間(迄)")]
            public string EFFECTIVE_DATE_E { get; set; }

            [Description("說明")]
            public string DESCRIPTION { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            public string CHARGE_DEPT_ID { get; set; }

            public string CHARGE_SECT_ID { get; set; }
        }

        protected class DepositReportSealData
        {
            [Description("項次")]
            public int ROW { get; set; }

            [Description("入庫日期")]
            public string PUT_DATE { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }

            [Description("印章內容")]
            public string SEAL_DESC { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            public string CHARGE_DEPT_ID { get; set; }

            public string CHARGE_SECT_ID { get; set; }

        }

        protected class DepositReportCAData
        {
            [Description("項次")]
            public int ROW { get; set; }

            [Description("入庫日期")]
            public string PUT_DATE { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }

            [Description("用途")]
            public string CA_USE { get; set; }

            [Description("類型")]
            public string CA_DESC { get; set; }

            [Description("銀行")]
            public string BANK { get; set; }

            [Description("號碼")]
            public string CA_NUMBER { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            public string CHARGE_DEPT_ID { get; set; }

            public string CHARGE_SECT_ID { get; set; }
        }

        protected class DepositReportESTATEData
        {
            [Description("項次")]
            public int ROW { get; set; }

            [Description("入庫日期")]
            public string PUT_DATE { get; set; }

            [Description("狀別")]
            public string ESTATE_FORM_NO { get; set; }

            [Description("發狀日")]
            public string ESTATE_DATE { get; set; }

            [Description("字號")]
            public string OWNERSHIP_CERT_NO { get; set; }

            [Description("地/建號")]
            public string LAND_BUILDING_NO { get; set; }

            [Description("門牌號")]
            public string HOUSE_NO { get; set; }
            
            [Description("流水號/編號")]
            public string ESTATE_SEQ { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            [Description("冊號")]
            public string BOOK_NO_DETAIL { get; set; }

            [Description("大樓名稱")]
            public string BUILDING_NAME { get; set; }

            [Description("坐落")]
            public string LOCATED { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }

            public string CHARGE_DEPT_ID { get; set; }

            public string CHARGE_SECT_ID { get; set; }
        }

        protected class DepositReportSTOCKData
        {
            [Description("入庫日期")]
            public string PUT_DATE { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }

            [Description("區域")]
            public string AREA { get; set; }

            [Description("類型")]
            public string STOCK_TYPE { get; set; }

            [Description("批次")]
            public string BATCH_NO { get; set; }

            [Description("序號前置碼")]
            public string STOCK_NO_PREAMBLE { get; set; }

            [Description("序號(起)")]
            public string STOCK_NO_B { get; set; }

            [Description("序號(迄)")]
            public string STOCK_NO_E { get; set; }

            [Description("每股金額")]
            public decimal? AMOUNT_PER_SHARE { get; set; }

            [Description("單張股數")]
            public decimal? SINGLE_NUMBER_OF_SHARES { get; set; } 

            [Description("張數")]
            public int? STOCK_CNT { get; set; }

            [Description("單張面額")]
            public decimal? DENOMINATION { get; set; }

            [Description("股數")]
            public decimal? NUMBER_OF_SHARES { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            [Description("編號")]
            public string BOOK_NO { get; set; }

            [Description("股票名稱")]
            public string NAME { get; set; }

            public string CHARGE_DEPT_ID { get; set; }

            public string CHARGE_SECT_ID { get; set; }
        }

        protected class DepositReportDEPOSIT_DEP_ORDER_M_Data
        {
            public string TYPE { get; set; }

            [Description("編號")]
            public string PK_ID { get; set; }

            [Description("存入日期")]
            public string PUT_DATE { get; set; }

            [Description("承作日期")]
            public string COMMIT_DATE { get; set; }

            [Description("到期日")]
            public string EXPIRY_DATE { get; set; }

            [Description("交易對象")]
            public string TRAD_PARTNERS { get; set; }

            [Description("幣別")]
            public string CURRENCY { get; set; }

            [Description("是否為台幣")]
            public string CURRENCY_Flag { get; set; }

            [Description("票面利率")]
            public Decimal? INTEREST_RATE { get; set; }

            [Description("存單號碼(起)")]
            public string DEP_NO_B { get; set; }

            [Description("存單號碼(迄)")]
            public string DEP_NO_E { get; set; }

            [Description("張數")]
            public int DEP_CNT { get; set; }

            [Description("單張面額")]
            public decimal DENOMINATION { get; set; }

            [Description("面額小計")]
            public decimal SUBTOTAL_DENOMINATION { get; set; }

            [Description("總張數")]
            public int SUMTOTAL_DEP_CNT { get; set; }

            [Description("總面額")]
            public decimal SUMTOTAL_DENOMINATION { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }

            [Description("交易類型")]
            public string DEP_TYPE { get; set; }

            [Description("設質否")]
            public string DEP_SET_QUALITY { get; set; }

            [Description("存單ID")]
            public string ITEMID { get; set; }

            public string CHARGE_DEPT_ID { get; set; }

            public string CHARGE_SECT_ID { get; set; }

        }

        protected class DepositReportITEMIMPData
        {
            [Description("歸檔編號")]
            public string ITEM_ID { get; set; }

            [Description("入庫日期")]
            public string PUT_DATE { get; set; }

            [Description("權責部門")]
            public string CHARGE_DEPT { get; set; }

            [Description("權責科別")]
            public string CHARGE_SECT { get; set; }

            [Description("物品名稱")]
            public string ITEM_NAME { get; set; }

            [Description("數量")]
            public int QUANTITY { get; set; }

            [Description("金額")]
            public decimal? AMOUNT { get; set; }

            [Description("預計存取日期")]
            public string EXPECTED_ACCESS_DATE { get; set; }

            [Description("說明")]
            public string DESCRIPTION { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            [Description("權責部門ID")]
            public string CHARGE_DEPT_ID { get; set; }

            [Description("權責科別ID")]
            public string CHARGE_SECT_ID { get; set; }
        }

        protected class TreasuryKeyCheckReport
        {
            [Description("方式")]
            public string CUSTODY_MODE { get; set; }

            [Description("設備名稱")]
            public string EQUIP_NAME { get; set; }

            [Description("保管人(部門/科別)")]
            public string EMP_NAME { get; set; }

            [Description("代理人(部門/科別)")]
            public string AGENT_NAME { get; set; }

            [Description("備註")]
            public string MEMO { get; set; }

            [Description("編號")]
            public int? ROW { get; set; }
        }

        /// <summary>
        /// 使用 部門ID 獲得 部門名稱
        /// </summary>
        /// <param name="depts"></param>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        protected string getEmpName(List<VW_OA_DEPT> depts, string DPT_CD)
        {
            if (!DPT_CD.IsNullOrWhiteSpace() && depts.Any())
                return depts.FirstOrDefault(x => x.DPT_CD?.Trim() == DPT_CD?.Trim())?.DPT_NAME?.Trim() ?? string.Empty;
            return string.Empty;
        }

        private string getStatue(string status)
        {
            string _status = string.Empty;
            List<string> Aply_Appr_Type = new List<string>()
            {
                Ref.AccessProjectFormStatus.A01.ToString(),
                Ref.AccessProjectFormStatus.A02.ToString(),
                Ref.AccessProjectFormStatus.A03.ToString(),
                Ref.AccessProjectFormStatus.A04.ToString(),
                Ref.AccessProjectFormStatus.A05.ToString(),
                Ref.AccessProjectFormStatus.A06.ToString()
            };
            List<string> End_Type = new List<string>()
            {
                Ref.AccessProjectFormStatus.E01.ToString(),
                Ref.AccessProjectFormStatus.E02.ToString(),
                Ref.AccessProjectFormStatus.E03.ToString(),
                Ref.AccessProjectFormStatus.E04.ToString()
            };

            var otherType = Aply_Appr_Type;
            otherType.AddRange(End_Type);

            if (End_Type.Contains(status))
            {
                _status = "已結申請單/已完成出入庫";
            }
            else if (!otherType.Contains(status))
            {
                _status = "在途申請單";
            }

            return _status;
        }
    }
}