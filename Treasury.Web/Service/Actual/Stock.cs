﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;
using System.ComponentModel;
using Treasury.Web.Controllers;

namespace Treasury.Web.Service.Actual
{
    public class Stock : Common, IStock
    {
        public Stock()
        {

        }
        #region GetData
        /// <summary>
        /// 最大股票編號(新增股票)
        /// </summary>
        /// <returns></returns>
        public int GetMaxStockNo()
        {
            int result = 0;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _ITEM_BOOK = db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == Ref.TreaItemType.D1015.ToString()).ToList();
                    //.Max(x => x.GROUP_NO);

            if(_ITEM_BOOK.Count > 0)
                    result = _ITEM_BOOK.Max(x => x.GROUP_NO);
            }
            return result;
        }

        /// <summary>
        /// 股票資料
        /// </summary>
        /// <returns></returns>
        public List<ItemBookStock> GetStockDate(int GroupNo, string vAplyNo = null)
        {
            var result = new List<ItemBookStock>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var vObj = from A in db.ITEM_BOOK.Where(x => x.ITEM_ID == Ref.TreaItemType.D1015.ToString() & x.COL == "AREA")
                           join M in db.ITEM_BOOK.Where(x => x.ITEM_ID == Ref.TreaItemType.D1015.ToString() & x.COL == "MEMO") on A.GROUP_NO equals M.GROUP_NO
                           join NBN in db.ITEM_BOOK.Where(x => x.ITEM_ID == Ref.TreaItemType.D1015.ToString() & x.COL == "NEXT_BATCH_NO") on A.GROUP_NO equals NBN.GROUP_NO
                           where A.GROUP_NO == GroupNo
                           select new ItemBookStock
                           {
                               GroupNo = A.GROUP_NO,
                               Area = A.COL_VALUE,
                               Memo = M.COL_VALUE,
                               Next_Batch_No = NBN.COL_VALUE
                           };

                result = vObj.ToList();

                //有申請編號
                if (!vAplyNo.IsNullOrWhiteSpace())
                {
                    var _itemId = db.OTHER_ITEM_APLY.AsNoTracking()
                                 .FirstOrDefault(x => x.APLY_NO == vAplyNo)?.ITEM_ID;
                    if (!_itemId.IsNullOrWhiteSpace())
                    {
                        result[0].Next_Batch_No = db.ITEM_STOCK.AsNoTracking().FirstOrDefault(x => x.ITEM_ID == _itemId).TREA_BATCH_NO.ToString();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 股票名稱
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <param name="vAplyNo"></param>
        /// <returns></returns>
        public List<SelectOption> GetStockName(string vAplyUnit = null, string vAplyNo = null)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Value = " ", Text = " " } };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var groupNos = new List<int>();
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    groupNos = db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    //.Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .Select(x => x.GROUP_NO).ToList();
                }
                if (!vAplyNo.IsNullOrWhiteSpace())
                {
                    var _itemId = db.OTHER_ITEM_APLY.AsNoTracking()
                                 .FirstOrDefault(x => x.APLY_NO == vAplyNo)?.ITEM_ID;
                    if (!_itemId.IsNullOrWhiteSpace())
                    {
                        groupNos.Add(db.ITEM_STOCK.AsNoTracking().FirstOrDefault(x => x.ITEM_ID == _itemId).GROUP_NO);
                    }
                }

                result.AddRange(db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == Ref.TreaItemType.D1015.ToString() && x.COL == "NAME")
                    .Where(x => groupNos.Contains(x.GROUP_NO), !vAplyUnit.IsNullOrWhiteSpace())
                    .OrderBy(x => x.GROUP_NO)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                        Value = x.GROUP_NO.ToString(),
                        Text = $@"{x.COL_VALUE}(編號:{x.GROUP_NO})"
                    }));
            }
            return result;
        }

        /// <summary>
        /// 區域
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetAreaType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "STOCK_AREA")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 類型
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetStockType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "STOCK_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 使用 群組編號 抓取在庫股票資料
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="vAplyUnit">申請部門</param>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        public List<StockDetailViewModel> GetDataByGroupNo(int groupNo, string vAplyUnit, string aplyNo = null)
        {
            var result = new List<StockDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var _emply = intra.getEmply();   //抓取員工資料
                var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                List<string> itemIds = new List<string>();

                if (!aplyNo.IsNullOrWhiteSpace()) //有單號須加單號資料
                {
                    itemIds = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => x.APLY_NO == aplyNo).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(
                        getMainModel(db.ITEM_STOCK.AsNoTracking()
                        .Where(x => x.GROUP_NO == groupNo)
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .Where(x => x.INVENTORY_STATUS == "4") //預約取出
                        .AsEnumerable(), _emply, _Inventory_types));
                }

                //批號篩選
                var _Trea_Batch_No_List = db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.GROUP_NO == groupNo)
                    .Where(x => x.INVENTORY_STATUS != "1")
                    .Select(x => x.TREA_BATCH_NO).ToList();

                var _data = db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.GROUP_NO == groupNo)
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .Where(x => !_Trea_Batch_No_List.Contains(x.TREA_BATCH_NO));

                //----------------------------------------
                //201905220094-00  190524 Edited by Bianco
                //Group By 主檔 取最大最後修改時間
                var groupByData = _data.AsEnumerable().GroupBy(x => new { x.GROUP_NO, x.TREA_BATCH_NO })
                                 .SelectMany(x => x.Where(y => y.LAST_UPDATE_DT == x.Max(z => z.LAST_UPDATE_DT)))
                                 .Select(x => new { x.TREA_BATCH_NO, x.LAST_UPDATE_DT }).ToList();

                //註解原程式
                //result.AddRange(
                //    getMainModel(db.ITEM_STOCK.AsNoTracking()
                //    .Where(x => x.GROUP_NO == groupNo)
                //    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                //    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                //    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                //    .Where(x => !_Trea_Batch_No_List.Contains(x.TREA_BATCH_NO))
                //    .AsEnumerable(), _emply, _Inventory_types));

                //原流程Distinct主檔
                result.AddRange(getMainModel(_data.AsEnumerable(), _emply, _Inventory_types));
                //將原流程Distinct主檔的 最後修改時間 重新押上 Griup By 後的最大最後修改時間
                groupByData.ForEach(x => {
                    result.FirstOrDefault(y => y.vTreaBatchNo == x.TREA_BATCH_NO).vLast_Update_Time = x.LAST_UPDATE_DT; 
                });
                //------------------------------------------
            }

            return result;
        }

        /// <summary>
        /// 使用 群組編號及入庫批號 抓取在庫股票明細資料
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="treaBatchNo">入庫批號</param>
        /// <returns></returns>
        public List<StockDetailViewModel> GetDetailData(int groupNo, int treaBatchNo)
        {
            var result = new List<StockDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                result =
                    getdetailModel(db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.GROUP_NO == groupNo)
                    .Where(x => x.TREA_BATCH_NO == treaBatchNo)
                    .AsEnumerable(), _Inventory_types).ToList();
            }

            return result;
        }

        /// <summary>
        /// 使用 群組編號及入庫批號 抓取異動在庫股票明細資料
        /// </summary>
        /// <param name="searchModel">CDC 查詢畫面條件</param>
        /// <param name="groupNo">群組編號</param>
        /// <param name="treaBatchNo">入庫批號</param>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        public List<CDCStockViewModel> GetCDCDetailData(CDCSearchViewModel searchModel, int groupNo, int treaBatchNo, string aplyNo)
        {
            var result = new List<CDCStockViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var depts = GetDepts();
                if (aplyNo.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => x.GROUP_NO == groupNo)
                        .Where(x => x.TREA_BATCH_NO == treaBatchNo)
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .AsEnumerable()
                        .Select((x) => new CDCStockViewModel()
                        {
                            vItemId = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vStock_Type = x.STOCK_TYPE,
                            vStock_Type_Aft = x.STOCK_TYPE_AFT,
                            vStock_No_Preamble = x.STOCK_NO_PREAMBLE,
                            vStock_No_Preamble_Aft = x.STOCK_NO_PREAMBLE_AFT,
                            vStock_No_B = x.STOCK_NO_B,
                            vStock_No_B_Aft = x.STOCK_NO_B_AFT,
                            vStock_No_E = x.STOCK_NO_E,
                            vStock_No_E_Aft = x.STOCK_NO_E_AFT,
                            vStock_Cnt = x.STOCK_CNT,
                            vStock_Cnt_Aft = x.STOCK_CNT_AFT,
                            vAmount_Per_Share = x.AMOUNT_PER_SHARE,
                            vAmount_Per_Share_Aft = x.AMOUNT_PER_SHARE_AFT,
                            vSingle_Number_Of_Shares = x.SINGLE_NUMBER_OF_SHARES,
                            vSingle_Number_Of_Shares_Aft = x.SINGLE_NUMBER_OF_SHARES_AFT,
                            vDenomination = x.DENOMINATION,
                            vDenomination_Aft = x.DENOMINATION_AFT,
                            vDenominationTotal = x.STOCK_CNT * x.DENOMINATION,
                            vDenominationTotal_Aft = GetDenominationTotal_Aft(x.STOCK_CNT, x.STOCK_CNT_AFT, x.DENOMINATION, x.DENOMINATION_AFT),
                            vNumberOfShares = x.NUMBER_OF_SHARES,
                            vNumberOfShares_Aft = x.NUMBER_OF_SHARES_AFT,
                            vMemo = x.MEMO,
                            vMemo_Aft = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList();
                    if (searchModel.vTreasuryIO == "N") //取出
                    {
                        if (result.Any())
                        {
                            var itemIds = result.Select(x => x.vItemId).ToList();
                            var uids = GetAplyUidName(itemIds);
                            result.ForEach(x =>
                            {
                                x.vGet_Uid_Name = uids.FirstOrDefault(y => y.itemId == x.vItemId)?.getAplyUidName;
                            });
                        }
                    }
                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aplyNo).Select(x => x.ITEM_ID).ToList();

                    result = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select((x) => new CDCStockViewModel()
                        {
                            vItemId = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vStock_Type = x.STOCK_TYPE,
                            vStock_Type_Aft = x.STOCK_TYPE_AFT,
                            vStock_No_Preamble = x.STOCK_NO_PREAMBLE,
                            vStock_No_Preamble_Aft = x.STOCK_NO_PREAMBLE_AFT,
                            vStock_No_B = x.STOCK_NO_B,
                            vStock_No_B_Aft = x.STOCK_NO_B_AFT,
                            vStock_No_E = x.STOCK_NO_E,
                            vStock_No_E_Aft = x.STOCK_NO_E_AFT,
                            vStock_Cnt = x.STOCK_CNT,
                            vStock_Cnt_Aft = x.STOCK_CNT_AFT,
                            vAmount_Per_Share = x.AMOUNT_PER_SHARE,
                            vAmount_Per_Share_Aft = x.AMOUNT_PER_SHARE_AFT,
                            vSingle_Number_Of_Shares = x.SINGLE_NUMBER_OF_SHARES,
                            vSingle_Number_Of_Shares_Aft = x.SINGLE_NUMBER_OF_SHARES_AFT,
                            vDenomination = x.DENOMINATION,
                            vDenomination_Aft = x.DENOMINATION_AFT,
                            vDenominationTotal = x.STOCK_CNT * x.DENOMINATION,
                            vDenominationTotal_Aft = GetDenominationTotal_Aft(x.STOCK_CNT, x.STOCK_CNT_AFT, x.DENOMINATION, x.DENOMINATION_AFT),
                            vNumberOfShares = x.NUMBER_OF_SHARES,
                            vNumberOfShares_Aft = x.NUMBER_OF_SHARES_AFT,
                            vMemo = x.MEMO,
                            vMemo_Aft = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList();
                }
                result.ForEach(x =>
                {
                    x.vCharge_Name = !x.vCharge_Sect_Name.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name : x.vCharge_Dept_Name;
                    x.vCharge_Name_AFT = !x.vCharge_Sect_Name_AFT.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name_AFT : (!x.vCharge_Dept_Name_AFT.IsNullOrWhiteSpace() ? x.vCharge_Dept_Name_AFT : null);
                });
            }

            return result;
        }

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <param name="EditFlag">可否修改,可以也需抓取庫存資料</param>
        /// <returns></returns>
        public StockViewModel GetDataByAplyNo(string aplyNo, bool EditFlag = false)
        {
            var result = new StockViewModel();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);

                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去股票庫存資料檔抓取資料
                    var details = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    if (details.Any())
                    {
                        var _groupNo = details.First().GROUP_NO;
                        //存取項目冊號檔讀取股票資料
                        var _ItemBooks = db.ITEM_BOOK.AsNoTracking()
                            .Where(x => x.GROUP_NO == _groupNo).ToList();
                        if (_ItemBooks.Any())
                        {
                            result.vStockDate = GetItemBookStock(_ItemBooks);
                        }
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        if (_TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.P.ToString())//存入
                        {
                            bool _accessStatus = (_TAR.APLY_STATUS == Ref.AccessProjectFormStatus.E01.ToString());
                            result.vDetail = getdetailModel(details, _Inventory_types, _accessStatus).ToList();
                        }
                        else if (_TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.G.ToString()) //取出
                        {
                            var _emply = intra.getEmply();   //抓取員工資料
                            var _vDetail = getMainModel(details, _emply, _Inventory_types).ToList();
                            if (EditFlag) //可以修改時需加入庫存資料
                            {
                                var dept = intra.getDept(_TAR.APLY_UNIT); //抓取單位
                                _vDetail.AddRange(
                                    getMainModel(db.ITEM_STOCK.AsNoTracking()
                                    .Where(x => x.GROUP_NO == _groupNo)
                                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                                    .AsEnumerable(), _emply, _Inventory_types));
                            }
                            result.vDetail = _vDetail.OrderBy(x => x.vTreaBatchNo).ToList();
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 股票名稱模糊比對
        /// </summary>
        /// <param name="StockName">股票名稱</param>
        /// <returns></returns>
        public List<ItemBookStock> GetStockCheck(string StockName)
        {
            var result = new List<ItemBookStock>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == Ref.TreaItemType.D1015.ToString())
                    .Where(x => x.COL == "NAME")
                    .Where(x => x.COL_VALUE.Contains(StockName))
                    .Select(x => new ItemBookStock()
                    {
                        GroupNo = x.GROUP_NO,
                        Name = x.COL_VALUE
                    })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// 查詢CDC資料
        /// </summary>
        /// <param name="searchModel">CDC 查詢畫面條件</param>
        /// <param name="aply_No">資料庫異動申請單紀錄檔  INVENTORY_CHG_APLY 單號</param>
        /// <returns></returns>
        public IEnumerable<ICDCItem> GetCDCSearchData(CDCSearchViewModel searchModel, string aply_No = null, string charge_Dept = null, string charge_Sect = null)
        {
            List<CDCStockViewModel> result = new List<CDCStockViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var depts = GetDepts();
                var _item_Id = Ref.TreaItemType.D1015.ToString();
                var _Item_Book = db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == _item_Id).ToList();
                var _Area = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "STOCK_AREA").ToList();
                if (aply_No.IsNullOrWhiteSpace())
                {
                    var PUT_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_From);
                    var PUT_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_To).DateToLatestTime();
                    var GET_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_From);
                    var GET_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_To).DateToLatestTime();

                    var Group_No = string.IsNullOrEmpty(searchModel.vName) ? 0 : int.Parse(searchModel.vName);

                    result.AddRange(db.ITEM_STOCK.AsNoTracking()
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value >= PUT_DATE_From.Value, PUT_DATE_From != null)
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value <= PUT_DATE_To.Value, PUT_DATE_To != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value >= GET_DATE_From.Value, GET_DATE_From != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value <= GET_DATE_To.Value, GET_DATE_To != null)
                        .Where(x => x.GROUP_NO == Group_No, searchModel.vName != null)
                        .Where(x => x.CHARGE_DEPT == charge_Dept, !charge_Dept.IsNullOrWhiteSpace())
                        .Where(x => x.CHARGE_SECT == charge_Sect, !charge_Sect.IsNullOrWhiteSpace())
                        .AsEnumerable().Distinct(new ItemStockComparer())
                        .Select((x) => new CDCStockViewModel()
                        {
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vIB_Group_No = x.GROUP_NO.ToString(),
                            vIB_Name = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "NAME")?.COL_VALUE,
                            vIB_Area = _Area.FirstOrDefault(z => z.CODE == _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "AREA")?.COL_VALUE)?.CODE_VALUE,
                            vIB_Memo = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "MEMO")?.COL_VALUE,
                            vTrea_Batch_No = x.TREA_BATCH_NO,
                            vNumber_Of_Shares_Total = GetCDCNumberOfSharesTotal(searchModel, x.GROUP_NO, x.TREA_BATCH_NO, aply_No),
                            vNumber_Of_Shares_Total_Aft = GetCDCNumberOfSharesTotal_Aft(searchModel, x.GROUP_NO, x.TREA_BATCH_NO, aply_No),
                            //CDC 權責異動查詢
                            vItemId = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                            vStock_Type = x.STOCK_TYPE,
                            vStock_Type_Aft = x.STOCK_TYPE_AFT,
                            vStock_No_Preamble = x.STOCK_NO_PREAMBLE,
                            vStock_No_Preamble_Aft = x.STOCK_NO_PREAMBLE_AFT,
                            vStock_No_B = x.STOCK_NO_B,
                            vStock_No_B_Aft = x.STOCK_NO_B_AFT,
                            vStock_No_E = x.STOCK_NO_E,
                            vStock_No_E_Aft = x.STOCK_NO_E_AFT,
                            vStock_Cnt = x.STOCK_CNT,
                            vStock_Cnt_Aft = x.STOCK_CNT_AFT,
                            vAmount_Per_Share = x.AMOUNT_PER_SHARE,
                            vAmount_Per_Share_Aft = x.AMOUNT_PER_SHARE_AFT,
                            vSingle_Number_Of_Shares = x.SINGLE_NUMBER_OF_SHARES,
                            vSingle_Number_Of_Shares_Aft = x.SINGLE_NUMBER_OF_SHARES_AFT,
                            vDenomination = x.DENOMINATION,
                            vDenomination_Aft = x.DENOMINATION_AFT,
                            vDenominationTotal = x.STOCK_CNT * x.DENOMINATION,
                            vDenominationTotal_Aft = GetDenominationTotal_Aft(x.STOCK_CNT, x.STOCK_CNT_AFT, x.DENOMINATION, x.DENOMINATION_AFT),
                            vNumberOfShares = x.NUMBER_OF_SHARES,
                            vNumberOfShares_Aft = x.NUMBER_OF_SHARES_AFT,
                            vMemo = x.MEMO,
                            vMemo_Aft = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(db.ITEM_STOCK.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable().Distinct(new ItemStockComparer())
                        .Select((x) => new CDCStockViewModel()
                        {
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vAply_No = aply_No,
                            vIB_Group_No = x.GROUP_NO.ToString(),
                            vIB_Name = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "NAME")?.COL_VALUE,
                            vIB_Area = _Area.FirstOrDefault(z => z.CODE == _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "AREA")?.COL_VALUE)?.CODE_VALUE,
                            vIB_Memo = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "MEMO")?.COL_VALUE,
                            vTrea_Batch_No = x.TREA_BATCH_NO,
                            vNumber_Of_Shares_Total = GetCDCNumberOfSharesTotal(searchModel, x.GROUP_NO, x.TREA_BATCH_NO, aply_No),
                            vNumber_Of_Shares_Total_Aft = GetCDCNumberOfSharesTotal_Aft(searchModel, x.GROUP_NO, x.TREA_BATCH_NO, aply_No)
                        }).ToList());
                }
                result.ForEach(x =>
                {
                    x.vCharge_Name = !x.vCharge_Sect_Name.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name : x.vCharge_Dept_Name;
                    x.vCharge_Name_AFT = !x.vCharge_Sect_Name_AFT.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name_AFT : (!x.vCharge_Dept_Name_AFT.IsNullOrWhiteSpace() ? x.vCharge_Dept_Name_AFT : null);
                });
            }

            return result;
        }
        #endregion

        #region SaveData
        /// <summary>
        /// 申請覆核動作
        /// </summary>
        /// <param name="insertDatas"></param>
        /// <param name="taData"></param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITreaItem>> ApplyAudit(IEnumerable<ITreaItem> insertDatas, TreasuryAccessViewModel taData)
        {
            var result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;
            try
            {
                if (insertDatas != null)
                {
                    var datas = (List<StockViewModel>)insertDatas;

                    if (datas.Any() && datas[0].vDetail.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();

                            string logStr = string.Empty; //log
                            var _TAR = new TREA_APLY_REC(); //申請單號
                            bool insertGroupFlag = false;
                            string stockName = "";  //股票名稱
                            var _APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(); //表單申請

                            if (taData.vAplyNo.IsNullOrWhiteSpace()) //新增申請單
                            {
                                #region 申請單紀錄檔 & 申請單歷程檔

                                var data = SaveTREA_APLY_REC(db, taData, logStr, dt);
                                _TAR.APLY_NO = data.Item1;
                                logStr = data.Item2;

                                #endregion

                                #region 存取項目冊號資料檔
                                var _first = datas.First();
                                var _StockModel = _first.vStockModel;
                                //判斷申請作業
                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                {
                                    //判斷存入資料
                                    switch (_first.vStockDate.StockFeaturesType)
                                    {
                                        case "StockInsert"://新增股票
                                            _first.vStockDate.GroupNo = GetMaxStockNo() + 1;
                                            stockName = _first.vStockDate.Name;
                                            foreach (var pro in _StockModel.GetType().GetProperties())
                                            {
                                                db.ITEM_BOOK.Add(new ITEM_BOOK()
                                                {
                                                    ITEM_ID = taData.vItem,
                                                    GROUP_NO = _first.vStockDate.GroupNo,
                                                    COL = pro.Name,
                                                    COL_NAME = (pro.GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute).Description,
                                                    COL_VALUE = pro.GetValue(_StockModel)?.ToString()?.Trim()
                                                });
                                            }
                                            insertGroupFlag = true;
                                            break;
                                        case "StockFromDB"://從資料庫選取股票
                                            var _ItemBooks = db.ITEM_BOOK.Where(x => x.GROUP_NO == _first.vStockDate.GroupNo && x.ITEM_ID == taData.vItem).ToList();
                                            foreach (var pro in _StockModel.GetType().GetProperties())
                                            {
                                                if (!(pro.Name == "NAME"))
                                                {
                                                    var _chang = _ItemBooks.FirstOrDefault(x => x.COL == pro.Name);
                                                    if (_chang != null)
                                                        _chang.COL_VALUE = pro.GetValue(_StockModel)?.ToString()?.Trim();
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                #endregion

                                #region 儲存資料
                                var details = _first.vDetail;
                                foreach (var item in details)
                                {
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var item_id = sysSeqDao.qrySeqNo("E7", string.Empty).ToString().PadLeft(8, '0');

                                            #region 其它存取項目申請資料檔
                                            var _OIA = new OTHER_ITEM_APLY()
                                            {
                                                APLY_NO = _TAR.APLY_NO,
                                                ITEM_ID = $@"E7{item_id}"
                                            };
                                            db.OTHER_ITEM_APLY.Add(_OIA);
                                            logStr += "|";
                                            logStr += _OIA.modelToString();
                                            #endregion

                                            #region 股票申請資料檔
                                            var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                            var _IS = new ITEM_STOCK()
                                            {
                                                ITEM_ID = $@"E7{item_id}", //物品編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                GROUP_NO = _first.vStockDate.GroupNo, //群組編號
                                                TREA_BATCH_NO = int.Parse(_first.vStockDate.Next_Batch_No),  //入庫批號
                                                STOCK_TYPE = item.vStockType,    //股票類型
                                                STOCK_NO_PREAMBLE = item.vStockNoPreamble,   //序號前置碼
                                                STOCK_NO_B = item.vStockNoB,  //股票序號(起)
                                                STOCK_NO_E = item.vStockNoE,  //股票序號(迄)
                                                STOCK_CNT = item.vStockTotal,    //股票張數
                                                AMOUNT_PER_SHARE = item.vAmount_Per_Share,    //每股金額
                                                SINGLE_NUMBER_OF_SHARES = item.vSingle_Number_Of_Shares,  //單張股數
                                                DENOMINATION = item.vDenomination,   //單張面額
                                                NUMBER_OF_SHARES = item.vNumberOfShares,   //股數小計
                                                MEMO = item.vMemo,//備註
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                LAST_UPDATE_DT = dt, //最後修改時間
                                            };
                                            db.ITEM_STOCK.Add(_IS);
                                            logStr += "|";
                                            logStr += _IS.modelToString();
                                            #endregion
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString()) //判斷申請作業-取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                        {
                                            //取得股票明細資料
                                            var StockDetail = db.ITEM_STOCK.AsNoTracking()
                                                                .Where(x => x.GROUP_NO == _first.vStockDate.GroupNo)
                                                                .Where(x => x.TREA_BATCH_NO == item.vTreaBatchNo)
                                                                .AsEnumerable().ToList();

                                            foreach (var detail in StockDetail)
                                            {
                                                #region 股票申請資料檔
                                                var _IS = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == detail.ITEM_ID);
                                                if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IS.INVENTORY_STATUS = "4"; //預約取出
                                                _IS.LAST_UPDATE_DT = dt;  //最後修改時間
                                                #endregion

                                                #region 其它存取項目申請資料檔
                                                var _OIA = new OTHER_ITEM_APLY()
                                                {
                                                    APLY_NO = _TAR.APLY_NO,
                                                    ITEM_ID = detail.ITEM_ID
                                                };
                                                db.OTHER_ITEM_APLY.Add(_OIA);
                                                logStr += "|";
                                                logStr += _OIA.modelToString();
                                                #endregion
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            else //修改申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                if (CustodyAppr.Contains(_TAR.APLY_STATUS))
                                {
                                    _APLY_STATUS = CustodyConfirmStatus;
                                    _TAR.CUSTODY_UID = AccountController.CurrentUserId; //保管單位直接帶使用者
                                    _TAR.CUSTODY_DT = dt;
                                }
                                else
                                {
                                    if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                        _APLY_STATUS = Ref.AccessProjectFormStatus.A05.ToString(); //為重新申請案例
                                }
                                _TAR.APLY_STATUS = _APLY_STATUS;
                                _TAR.LAST_UPDATE_DT = dt;

                                logStr += _TAR.modelToString();
                                #endregion

                                #region 申請單歷程檔
                                db.APLY_REC_HIS.Add(
                                new APLY_REC_HIS()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    APLY_STATUS = _TAR.APLY_STATUS,
                                    PROC_UID = _TAR.CREATE_UID,
                                    PROC_DT = dt
                                });
                                #endregion

                                #region 儲存資料
                                var _first = datas.First();
                                var details = _first.vDetail;
                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<ITEM_STOCK> inserts = new List<ITEM_STOCK>(); //新增資料
                                var _IS = new ITEM_STOCK();

                                foreach (var item in details)
                                {
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            if (item.vItemId.StartsWith("E7"))  //明細修改
                                            {
                                                _IS = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IS.GROUP_NO = _first.vStockDate.GroupNo;
                                                _IS.STOCK_TYPE = item.vStockType;    //股票類型
                                                _IS.STOCK_NO_PREAMBLE = item.vStockNoPreamble;   //序號前置碼
                                                _IS.STOCK_NO_B = item.vStockNoB;  //股票序號(起)
                                                _IS.STOCK_NO_E = item.vStockNoE;  //股票序號(迄)
                                                _IS.STOCK_CNT = item.vStockTotal;    //股票張數
                                                _IS.AMOUNT_PER_SHARE = item.vAmount_Per_Share;  //每股金額
                                                _IS.SINGLE_NUMBER_OF_SHARES = item.vSingle_Number_Of_Shares;    //單張股數
                                                _IS.DENOMINATION = item.vDenomination;   //單張面額
                                                _IS.NUMBER_OF_SHARES = item.vNumberOfShares;   //股數小計
                                                _IS.MEMO = item.vMemo;  //備註
                                                _IS.LAST_UPDATE_DT = dt; //最後修改時間

                                                updateItemIds.Add(item.vItemId);

                                                logStr += "|";
                                                logStr += _IS.modelToString();
                                            }
                                            else //明細新增
                                            {
                                                var item_id = sysSeqDao.qrySeqNo("E7", string.Empty).ToString().PadLeft(8, '0');

                                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                                _IS = new ITEM_STOCK()
                                                {
                                                    ITEM_ID = $@"E7{item_id}", //物品編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    GROUP_NO = _first.vStockDate.GroupNo, //群組編號
                                                    TREA_BATCH_NO = int.Parse(_first.vStockDate.Next_Batch_No),  //入庫批號
                                                    STOCK_TYPE = item.vStockType,    //股票類型
                                                    STOCK_NO_PREAMBLE = item.vStockNoPreamble,   //序號前置碼
                                                    STOCK_NO_B = item.vStockNoB,  //股票序號(起)
                                                    STOCK_NO_E = item.vStockNoE,  //股票序號(迄)
                                                    STOCK_CNT = item.vStockTotal,    //股票張數
                                                    SINGLE_NUMBER_OF_SHARES = item.vSingle_Number_Of_Shares, //單張股數
                                                    DENOMINATION = item.vDenomination,   //單張面額
                                                    NUMBER_OF_SHARES = item.vNumberOfShares,   //股數小計
                                                    MEMO = item.vMemo,//備註
                                                    APLY_DEPT = _dept.Item1, //申請人部門
                                                    APLY_SECT = _dept.Item2, //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1, //權責部門
                                                    CHARGE_SECT = _dept.Item2, //權責科別
                                                    LAST_UPDATE_DT = dt, //最後修改時間
                                                };

                                                inserts.Add(_IS);
                                            }
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus)) //判斷申請作業-取出
                                    {
                                        //取得股票明細資料
                                        var StockDetail = db.ITEM_STOCK.AsNoTracking()
                                                            .Where(x => x.GROUP_NO == _first.vStockDate.GroupNo)
                                                            .Where(x => x.TREA_BATCH_NO == item.vTreaBatchNo)
                                                            .AsEnumerable().ToList();

                                        foreach (var detail in StockDetail)
                                        {
                                            #region 股票申請資料檔
                                            _IS = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == detail.ITEM_ID);
                                            if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }

                                            //預約取出
                                            if (item.vTakeoutFlag)
                                            {
                                                if (_IS.INVENTORY_STATUS == "1") //原先為在庫
                                                {
                                                    _IS.INVENTORY_STATUS = "4"; //改為預約取出
                                                    _IS.LAST_UPDATE_DT = dt;  //最後修改時間
                                                    updateItemIds.Add(_IS.ITEM_ID);
                                                }
                                                else if (_IS.INVENTORY_STATUS == "4") //原先為預約取出
                                                {
                                                    updateItemIds.Add(_IS.ITEM_ID);
                                                }
                                            }
                                            else
                                            {
                                                if (_IS.INVENTORY_STATUS == "4") //原先為預約取出
                                                {
                                                    _IS.INVENTORY_STATUS = "1"; //改為在庫
                                                    _IS.LAST_UPDATE_DT = dt;  //最後修改時間
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_STOCK.RemoveRange(db.ITEM_STOCK.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_STOCK.AddRange(inserts);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus))//取出
                                {
                                    foreach (var backItemId in db.OTHER_ITEM_APLY.Where(x =>
                                     x.APLY_NO == taData.vAplyNo &&
                                     !updateItemIds.Contains(x.ITEM_ID)
                                    ).Select(x => x.ITEM_ID))
                                    {
                                        var back = db.ITEM_STOCK.First(x => x.ITEM_ID == backItemId);
                                        back.INVENTORY_STATUS = "1";
                                    }

                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(updateItemIds.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x
                                    }));
                                }
                                #endregion
                            }

                            #region Save Db

                            var validateMessage = db.GetValidationErrors().getValidateString();
                            if (validateMessage.Any())
                            {
                                result.DESCRIPTION = validateMessage;
                            }
                            else
                            {
                                try
                                {
                                    db.SaveChanges();

                                    #region LOG
                                    //新增LOG
                                    Log log = new Log();
                                    log.CFUNCTION = "申請覆核-新增股票";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, taData.vCreateUid);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    var addstr = insertGroupFlag ? (",新增股票:" + stockName) : string.Empty;
                                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_TAR.APLY_NO}{addstr}");
                                }
                                catch (DbUpdateException ex)
                                {
                                    result.DESCRIPTION = ex.exceptionMessage();
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }
                else
                {
                    result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }

            return result;
        }

        /// <summary>
        /// 刪除申請
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="aply_No">作廢單號</param>
        /// <param name="access_Type">取出或存入</param>
        /// <param name="logStr">log 字串</param>
        /// <param name="dt">更新時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CancelApply(TreasuryDBEntities db, string aply_No, string access_Type, string logStr, DateTime dt)
        {
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態資料要復原
            {
                return Recover(db, aply_No, logStr, dt, true);
            }
            else if (access_Type == Ref.AccessProjectTradeType.P.ToString())    //存入處理作業
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                .FirstOrDefault(x => x.APLY_NO == aply_No);

                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去股票庫存資料檔抓取資料
                    var details = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    if (details.Any())
                    {
                        db.ITEM_STOCK.RemoveRange(db.ITEM_STOCK.Where(x => OIAs.Contains(x.ITEM_ID)));
                        db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => OIAs.Contains(x.ITEM_ID)));
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, logStr);
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 作廢
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="aply_No">作廢單號</param>
        /// <param name="access_Type">取出或存入</param>
        /// <param name="logStr">log 字串</param>
        /// <param name="dt">更新時間</param>
        /// <returns></returns>
        public Tuple<bool, string> ObSolete(TreasuryDBEntities db, string aply_No, string access_Type, string logStr, DateTime dt)
        {
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態資料要復原
            {
                return Recover(db, aply_No, logStr, dt, false);
            }
            else if (access_Type == Ref.AccessProjectTradeType.P.ToString())    //存入處理作業
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                .FirstOrDefault(x => x.APLY_NO == aply_No);

                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去股票庫存資料檔抓取資料
                    var details = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    if (details.Any())
                    {
                        foreach (var item in details)
                        {
                            var _IS = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == item.ITEM_ID);
                            _IS.INVENTORY_STATUS = "7"; //已取消
                            _IS.LAST_UPDATE_DT = dt;
                            logStr += _IS.modelToString(logStr);
                        }
                    }
                    else
                    {
                        return new Tuple<bool, string>(false, logStr);
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存異動資料-申請覆核
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ICDCItem>> CDCApplyAudit(IEnumerable<ICDCItem> saveData, CDCSearchViewModel searchModel)
        {
            MSGReturnModel<IEnumerable<ICDCItem>> result = new MSGReturnModel<IEnumerable<ICDCItem>>();
            result.RETURN_FLAG = false;
            string logStr = string.Empty;
            DateTime dt = DateTime.Now;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                bool changFlag = false;
                var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt);
                logStr = _data.Item2;
                foreach (CDCStockViewModel model in saveData)
                {
                    var _Stock = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                    if (_Stock != null && !changFlag)
                    {
                        if (_Stock.LAST_UPDATE_DT > model.vLast_Update_Time || _Stock.INVENTORY_STATUS != "1")
                        {
                            changFlag = true;
                        }

                        if (!changFlag)
                        {
                            _Stock.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                            _Stock.STOCK_TYPE_AFT = model.vStock_Type_Aft;
                            _Stock.STOCK_NO_PREAMBLE_AFT = model.vStock_No_Preamble_Aft;
                            _Stock.STOCK_NO_B_AFT = model.vStock_No_B_Aft;
                            _Stock.STOCK_NO_E_AFT = model.vStock_No_E_Aft;
                            _Stock.STOCK_CNT_AFT = model.vStock_Cnt_Aft;
                            _Stock.AMOUNT_PER_SHARE_AFT = model.vAmount_Per_Share_Aft;
                            _Stock.SINGLE_NUMBER_OF_SHARES_AFT = model.vSingle_Number_Of_Shares_Aft;
                            _Stock.DENOMINATION_AFT = model.vDenomination_Aft;
                            _Stock.NUMBER_OF_SHARES_AFT = model.vNumberOfShares_Aft;
                            _Stock.MEMO_AFT = model.vMemo_Aft;
                            _Stock.LAST_UPDATE_DT = dt;

                            logStr = _Stock.modelToString(logStr);

                            var _OIA = new OTHER_ITEM_APLY()
                            {
                                APLY_NO = _data.Item1,
                                ITEM_ID = _Stock.ITEM_ID
                            };

                            db.OTHER_ITEM_APLY.Add(_OIA);

                            logStr = _OIA.modelToString(logStr);
                        }
                    }
                    else
                    {
                        changFlag = true;
                    }
                }

                if (changFlag)
                {
                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                }
                else
                {
                    db.SaveChanges();
                    #region LOG
                    //新增LOG
                    Log log = new Log();
                    log.CFUNCTION = "申請覆核-資料庫異動:股票";
                    log.CACTION = "A";
                    log.CCONTENT = logStr;
                    LogDao.Insert(log, searchModel.vCreate_Uid);
                    #endregion
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"申請單號:{_data.Item1}");
                    result.Datas = GetCDCSearchData(searchModel);
                }
            }

            return result;
        }

        /// <summary>
        /// 庫存異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _Stock = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Stock != null)
                {
                    _Stock.INVENTORY_STATUS = "1"; //在庫
                    _Stock.STOCK_TYPE_AFT = null;
                    _Stock.STOCK_NO_PREAMBLE_AFT = null;
                    _Stock.STOCK_NO_B_AFT = null;
                    _Stock.STOCK_NO_E_AFT = null;
                    _Stock.STOCK_CNT_AFT = null;
                    _Stock.AMOUNT_PER_SHARE_AFT = null;
                    _Stock.SINGLE_NUMBER_OF_SHARES_AFT = null;
                    _Stock.DENOMINATION_AFT = null;
                    _Stock.NUMBER_OF_SHARES_AFT = null;
                    _Stock.MEMO_AFT = null;
                    _Stock.LAST_UPDATE_DT = dt;
                    logStr = _Stock.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _Stock = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Stock != null)
                {
                    _Stock.INVENTORY_STATUS = "1"; //在庫
                    _Stock.STOCK_TYPE = GetNewValue(_Stock.STOCK_TYPE, _Stock.STOCK_TYPE_AFT);
                    _Stock.STOCK_TYPE_AFT = null;
                    _Stock.STOCK_NO_PREAMBLE = GetNewValue(_Stock.STOCK_NO_PREAMBLE, _Stock.STOCK_NO_PREAMBLE_AFT);
                    _Stock.STOCK_NO_PREAMBLE_AFT = null;
                    _Stock.STOCK_NO_B = GetNewValue(_Stock.STOCK_NO_B, _Stock.STOCK_NO_B_AFT);
                    _Stock.STOCK_NO_B_AFT = null;
                    _Stock.STOCK_NO_E = GetNewValue(_Stock.STOCK_NO_E, _Stock.STOCK_NO_E_AFT);
                    _Stock.STOCK_NO_E_AFT = null;
                    _Stock.STOCK_CNT = TypeTransfer.stringToIntN(GetNewValue(TypeTransfer.intNToString(_Stock.STOCK_CNT), TypeTransfer.intNToString(_Stock.STOCK_CNT_AFT)));
                    _Stock.STOCK_CNT_AFT = null;
                    _Stock.AMOUNT_PER_SHARE = TypeTransfer.stringToDecimal(GetNewValue(TypeTransfer.decimalNToString(_Stock.AMOUNT_PER_SHARE), TypeTransfer.decimalNToString(_Stock.AMOUNT_PER_SHARE_AFT)));
                    _Stock.AMOUNT_PER_SHARE_AFT = null;
                    _Stock.SINGLE_NUMBER_OF_SHARES = TypeTransfer.stringToDecimal(GetNewValue(TypeTransfer.decimalNToString(_Stock.SINGLE_NUMBER_OF_SHARES), TypeTransfer.decimalNToString(_Stock.SINGLE_NUMBER_OF_SHARES_AFT)));
                    _Stock.SINGLE_NUMBER_OF_SHARES_AFT = null;
                    _Stock.DENOMINATION = TypeTransfer.stringToDecimal(GetNewValue(TypeTransfer.decimalNToString(_Stock.DENOMINATION), TypeTransfer.decimalNToString(_Stock.DENOMINATION_AFT)));
                    _Stock.DENOMINATION_AFT = null;
                    _Stock.NUMBER_OF_SHARES = TypeTransfer.stringToDecimal(GetNewValue(TypeTransfer.decimalNToString(_Stock.NUMBER_OF_SHARES), TypeTransfer.decimalNToString(_Stock.NUMBER_OF_SHARES_AFT)));
                    _Stock.NUMBER_OF_SHARES_AFT = null;
                    _Stock.MEMO = GetNewValue(_Stock.MEMO, _Stock.MEMO_AFT);
                    _Stock.MEMO_AFT = null;
                    _Stock.LAST_UPDATE_DT = dt;
                    logStr = _Stock.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存權責異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCChargeReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _Stock = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Stock != null)
                {
                    _Stock.INVENTORY_STATUS = "1"; //在庫
                    _Stock.CHARGE_DEPT_AFT = null;
                    _Stock.CHARGE_SECT_AFT = null;
                    _Stock.LAST_UPDATE_DT = dt;
                    logStr = _Stock.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存權責異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCChargeApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _Stock = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == itemID); 
                if (_Stock != null)
                {
                    _Stock.INVENTORY_STATUS = "1"; //在庫
                    _Stock.CHARGE_DEPT = _Stock.CHARGE_DEPT_AFT;
                    _Stock.CHARGE_DEPT_AFT = null;
                    _Stock.CHARGE_SECT = _Stock.CHARGE_SECT_AFT;
                    _Stock.CHARGE_SECT_AFT = null;
                    _Stock.LAST_UPDATE_DT = dt;
                    logStr = _Stock.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        #endregion

        #region privation function
        /// <summary>
        /// 在庫股票主檔資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_emply"></param>
        /// <returns></returns>
        private IEnumerable<StockDetailViewModel> getMainModel(IEnumerable<ITEM_STOCK> data, List<V_EMPLY2> _emply, List<SYS_CODE> _Inventory_types)
        {
            data = data.Distinct(new ItemStockComparer());
            return data.Select(x => new StockDetailViewModel()
            {
                vTakeoutFlag = (x.INVENTORY_STATUS == "4"), //取出註記
                vTreaBatchNo = x.TREA_BATCH_NO,   //入庫批號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vAplyDate = string.IsNullOrEmpty(x.LAST_UPDATE_DT.ToString()) ? "" : DateTime.Parse(x.LAST_UPDATE_DT.ToString()).ToString("yyyy/MM/dd"),    //申請日期
                vAplyName = _emply.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,   //申請人
                vNumberOfSharesTotal = GetNumberOfSharesTotal(x.GROUP_NO, x.TREA_BATCH_NO), //總股數
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        /// <summary>
        /// 在庫股票明細資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<StockDetailViewModel> getdetailModel(IEnumerable<ITEM_STOCK> data, List<SYS_CODE> _Inventory_types, bool accessStatus = false)
        {
            return data.Select(x => new StockDetailViewModel()
            {
                vItemId = x.ITEM_ID,  //物品單號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vTreaBatchNo = x.TREA_BATCH_NO,   //入庫批號
                vStockType = accessStatus ? x.STOCK_TYPE_ACCESS : x.STOCK_TYPE,    //類型
                vStockNoPreamble = accessStatus ? x.STOCK_NO_PREAMBLE_ACCESS : x.STOCK_NO_PREAMBLE,   //序號前置碼
                vStockNoB = accessStatus ? x.STOCK_NO_B_ACCESS : x.STOCK_NO_B,  //序號(起)
                vStockNoE = accessStatus ? x.STOCK_NO_E_ACCESS : x.STOCK_NO_E,  //序號(迄)
                vStockTotal = accessStatus ? x.STOCK_CNT_ACCESS : x.STOCK_CNT,    //張數
                vAmount_Per_Share = accessStatus ? x.AMOUNT_PER_SHARE_ACCESS : x.AMOUNT_PER_SHARE,   //每股金額
                vSingle_Number_Of_Shares = accessStatus ? x.SINGLE_NUMBER_OF_SHARES_ACCESS : x.SINGLE_NUMBER_OF_SHARES, //單張股數
                vDenomination = accessStatus ? x.DENOMINATION_ACCESS : x.DENOMINATION,   //單張面額
                vDenominationTotal = accessStatus ? (x.STOCK_CNT_ACCESS * x.DENOMINATION_ACCESS ) : (x.STOCK_CNT * x.DENOMINATION),  //面額小計
                vNumberOfShares = accessStatus ? x.NUMBER_OF_SHARES_ACCESS : x.NUMBER_OF_SHARES,   //股數小計
                vMemo = accessStatus ? x.MEMO_ACCESS : x.MEMO,  //備註說明
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        /// <summary>
        /// 存取項目冊號資料檔-股票資料
        /// </summary>
        /// <param name="_ItemBooks"></param>
        /// <returns></returns>
        private ItemBookStock GetItemBookStock(List<ITEM_BOOK> _ItemBooks)
        {
            ItemBookStock result = new ItemBookStock();
            if (_ItemBooks.Any())
            {
                result = new ItemBookStock()
                {
                    GroupNo = (int)_ItemBooks.FirstOrDefault()?.GROUP_NO,//群組編號
                    Name = _ItemBooks.FirstOrDefault(x => x.COL == "NAME")?.COL_VALUE, //股票名稱
                    Area = _ItemBooks.FirstOrDefault(x => x.COL == "AREA")?.COL_VALUE, //區域
                    Memo = _ItemBooks.FirstOrDefault(x => x.COL == "MEMO")?.COL_VALUE, //備註
                    Next_Batch_No = _ItemBooks.FirstOrDefault(x => x.COL == "NEXT_BATCH_NO")?.COL_VALUE //下一次入庫批號
                };
            }
            return result;
        }

        //在庫股票Distinct
        class ItemStockComparer : IEqualityComparer<ITEM_STOCK>
        {
            public bool Equals(ITEM_STOCK x, ITEM_STOCK y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.GROUP_NO == y.GROUP_NO && x.TREA_BATCH_NO == y.TREA_BATCH_NO;
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(ITEM_STOCK ItemStock)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(ItemStock, null)) return 0;

                //Get hash code for the PutDate field if it is not null.
                int hashItemStockGroupNo = ItemStock.GROUP_NO.GetHashCode();

                //Get hash code for the TreaBatchNo field.
                int hashItemStockTreaBatchNo = ItemStock.TREA_BATCH_NO.GetHashCode();

                //Calculate the hash code for the product.
                return hashItemStockGroupNo ^ hashItemStockTreaBatchNo;
            }
        }

        //計算面額小計_異動後
        private decimal? GetDenominationTotal_Aft(int? Stock_Cnt, int? Stock_Cnt_Aft, decimal? Denomination, decimal? Denomination_Aft)
        {
            var NewStock_Cnt = Stock_Cnt_Aft == null ? Stock_Cnt : Stock_Cnt_Aft;
            var NewDenomination = Denomination_Aft == null ? Denomination : Denomination_Aft;

            //張數和單張面額皆未異動
            if (Stock_Cnt_Aft == null && Denomination_Aft == null)
                return null;

            return NewStock_Cnt * NewDenomination;
        }

        //計算總股數
        private int GetNumberOfSharesTotal(int GroupNo, int TreaBatchNo)
        {
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var NumberOfSharesTotal = db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.GROUP_NO == GroupNo)
                    .Where(x => x.TREA_BATCH_NO == TreaBatchNo)
                    .Sum(x => x.NUMBER_OF_SHARES);

                return (int)NumberOfSharesTotal;
            }
        }

        //計算異動總股數
        private int GetCDCNumberOfSharesTotal(CDCSearchViewModel searchModel, int GroupNo, int TreaBatchNo, string aply_No = null)
        {
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                if (aply_No.IsNullOrWhiteSpace())
                {
                    var NumberOfSharesTotal = db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.GROUP_NO == GroupNo)
                    .Where(x => x.TREA_BATCH_NO == TreaBatchNo)
                    .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                    .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                    .Where(x => x.NUMBER_OF_SHARES != null)
                    .Sum(x => x.NUMBER_OF_SHARES);

                    return NumberOfSharesTotal == null ? 0 : (int)NumberOfSharesTotal;
                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    var StockList = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable().ToList();

                    int result = 0;

                    if (StockList.Count==0)
                    {
                        result = 0;
                    }
                    else
                    {
                        StockList.ForEach(x =>
                        {
                            //總股數
                            //有股數才加總
                            if (x.NUMBER_OF_SHARES != null)
                                result += (int)x.NUMBER_OF_SHARES;
                        });
                    }

                    return result;
                }
            }
        }
        //計算異動總股數_異動後
        private int GetCDCNumberOfSharesTotal_Aft(CDCSearchViewModel searchModel, int GroupNo, int TreaBatchNo, string aply_No = null)
        {
            int result = 0, Number_Of_Shares_Total = 0, Number_Of_Shares_Total_Aft = 0;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                List<ITEM_STOCK> tempData = new List<ITEM_STOCK>();

                if (aply_No.IsNullOrWhiteSpace())
                {
                    tempData = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => x.GROUP_NO == GroupNo)
                        .Where(x => x.TREA_BATCH_NO == TreaBatchNo)
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .ToList();

                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    tempData = db.ITEM_STOCK.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable().ToList();
                }

                if (tempData.Count == 0)
                {
                    result = 0;
                }
                else
                {
                    tempData.ForEach(x =>
                    {
                        //總股數
                        //有股數才加總
                        if (x.NUMBER_OF_SHARES != null)
                            Number_Of_Shares_Total += (int)x.NUMBER_OF_SHARES;

                        #region 總股數_異動後
                        //判斷是否有股數_異動後
                        if (x.NUMBER_OF_SHARES_AFT != null)
                            Number_Of_Shares_Total_Aft += (int)x.NUMBER_OF_SHARES_AFT;
                        //有股數才加總
                        else if (x.NUMBER_OF_SHARES != null)
                            Number_Of_Shares_Total_Aft += (int)x.NUMBER_OF_SHARES;
                        #endregion
                    });
                }

                //總股數_異動後相同總股數
                if (Number_Of_Shares_Total == Number_Of_Shares_Total_Aft)
                    result = -1;
                else
                    result = Number_Of_Shares_Total_Aft;

            }

            return result;
        }

        /// <summary>
        /// 申請刪除 & 作廢 股票資料庫要復原的事件
        /// </summary>
        /// <param name="db"></param>
        /// <param name="aply_No"></param>
        /// <param name="logStr"></param>
        /// <param name="dt"></param>
        /// <param name="deleFlag"></param>
        /// <returns></returns>
        public Tuple<bool, string> Recover(TreasuryDBEntities db, string aply_No, string logStr, DateTime dt, bool deleFlag)
        {
            var _changeFlag = false;

            var _TAR = db.TREA_APLY_REC.AsNoTracking()
            .FirstOrDefault(x => x.APLY_NO == aply_No);

            if (_TAR != null)
            {
                //使用單號去其他存取項目檔抓取物品編號
                var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                    .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                //使用物品編號去股票庫存資料檔抓取資料
                var details = db.ITEM_STOCK.AsNoTracking()
                    .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                if (details.Any())
                {
                    foreach (var item in details)
                    {
                        var _IS = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == item.ITEM_ID);
                        _IS.INVENTORY_STATUS = "1"; //返回在庫
                        _IS.LAST_UPDATE_DT = dt;
                        logStr += _IS.modelToString(logStr);
                    }

                    //刪除其他存取項目檔
                    if (deleFlag)
                        db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => OIAs.Contains(x.ITEM_ID)));
                }
                else
                {
                    _changeFlag = true;
                }
            }
            else
            {
                _changeFlag = true;
            }

            if (_changeFlag)
            {
                return new Tuple<bool, string>(false, logStr);
            }
            else
            {
                return new Tuple<bool, string>(true, logStr);
            }
        }
        #endregion
    }
}