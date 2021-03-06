﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebDaos;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Actual
{
    public class TREAReviewWork : Common, ITREAReviewWork
    {
        /// <summary>
        /// 取得初始資料
        /// </summary>
        /// <returns></returns>
        public List<TREAReviewWorkDetailViewModel> GetSearchDatas(string cUserId)
        {
            List<TREAReviewWorkDetailViewModel> result = new List<TREAReviewWorkDetailViewModel>();
            string status = Ref.AccessProjectFormStatus.D02.ToString(); //金庫登記簿覆核中

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking();
                var _OPEN_TREA_TYPE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE").ToList();
                var _FORM_STATUS = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "FORM_STATUS").ToList();
                result = _TREA_OPEN_REC
                    .Where(x => x.REGI_STATUS == status)
                    //.Where(x => x.CREATE_UNIT == GetUserInfo(cUserId).DPT_ID)
                    .AsEnumerable()
                    .Select(x => new TREAReviewWorkDetailViewModel
                    {
                        vOPEN_TREA_TYPE = _OPEN_TREA_TYPE.FirstOrDefault(y => y.CODE == x.OPEN_TREA_TYPE)?.CODE_VALUE,
                        vTREA_REGISTER_ID = x.TREA_REGISTER_ID,
                        hvTREA_REGISTER_ID = x.TREA_REGISTER_ID,
                        vOPEN_TREA_TIME = x.OPEN_TREA_TIME,
                        vACTUAL_PUT_TIME = x.ACTUAL_PUT_TIME?.ToString("HH:mm"),
                        vACTUAL_GET_TIME = x.ACTUAL_GET_TIME?.ToString("HH:mm"),
                        vREGI_STATUS = _FORM_STATUS.FirstOrDefault(y => y.CODE == x.REGI_STATUS)?.CODE_VALUE,
                        vLAST_UPDATE_DT = x.LAST_UPDATE_DT,
                        vOPEN_TREA_DATE = x.OPEN_TREA_DATE.ToString("yyyy/MM/dd"),
                        vLAST_UPDATE_UID = x.LAST_UPDATE_UID,
                        Ischecked = false
                    }).ToList();
            }
            return result;
        }

        public List<TREAReviewWorkSearchDetailViewModel> GetDetailsDatas(string RegisterId)
        {
            List<TREAReviewWorkSearchDetailViewModel> result = new List<TREAReviewWorkSearchDetailViewModel>();

            if (!RegisterId.IsNullOrWhiteSpace())
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _TREA_APLY_REC = db.TREA_APLY_REC.AsNoTracking();
                    var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                    var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                    var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.AsNoTracking();
                    var _SYS_CODE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "ACCESS_TYPE").ToList();

                    result = _TREA_APLY_REC.Where(x => x.TREA_REGISTER_ID == RegisterId)
                        .AsEnumerable()
                        .Select(x => new TREAReviewWorkSearchDetailViewModel()
                        {
                            vITeM_OP_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE,
                            vITEM_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC,
                            vSEAL_ITEM = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(z => z.APLY_NO == x.APLY_NO).ITEM_ID)?.SEAL_DESC : null,
                            vACCESS_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _SYS_CODE.FirstOrDefault(y => y.CODE == x.ACCESS_TYPE)?.CODE_VALUE : null,
                            vAPLY_NO = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "3" ? x.APLY_NO : null,
                            vACCESS_REASON = x.ACCESS_REASON,
                            vCONFIRM_NAME = x.CONFIRM_UID != null ? GetUserInfo(x.CONFIRM_UID)?.EMP_Name : null,
                            vACTUAL_ACCESS_TYPE = x.ACTUAL_ACCESS_TYPE != x.ACCESS_TYPE ? _SYS_CODE.FirstOrDefault(y => y.CODE == x.ACTUAL_ACCESS_TYPE)?.CODE_VALUE : null,
                            vACTUAL_ACCESS_NAME = x.ACTUAL_ACCESS_UID != x.CONFIRM_UID ? GetUserInfo(x.ACTUAL_ACCESS_UID)?.EMP_Name : null,
                            vTREA_REGISTER_ID = x.TREA_REGISTER_ID,
                            vITEM_ID = x.ITEM_ID
                        }).ToList();
                }
            }
            return result;
        }

        /// <summary>
        /// 覆核
        /// </summary>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<List<TREAReviewWorkDetailViewModel>> InsertApplyData(List<TREAReviewWorkDetailViewModel> ViewModel, string cUserId)
        {
            var result = new MSGReturnModel<List<TREAReviewWorkDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!ViewModel.Any())
            {
                return result;
            }
            DateTime dt = DateTime.Now;
            string logStr = string.Empty; //Log
            string aplyNoUid = "";
            string status = Ref.AccessProjectFormStatus.E01.ToString(); // 已完成出入庫，通知申請人員
            SysSeqDao sysSeqDao = new SysSeqDao();
            List<string> approvedList = new List<string>();
            List<string> applyUidList = new List<string>();
            List<TREA_OPEN_REC> TORs = new List<TREA_OPEN_REC>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                foreach (var item in ViewModel.Where(x => x.Ischecked))
                {
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == item.hvTREA_REGISTER_ID);
                    var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                    if (_TREA_OPEN_REC == null) //找不到該單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.hvTREA_REGISTER_ID}");
                        return result;
                    }
                    if (_TREA_OPEN_REC.LAST_UPDATE_DT > item.vLAST_UPDATE_DT) //資料已備更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.hvTREA_REGISTER_ID}");
                        return result;
                    }
                    _TREA_OPEN_REC.REGI_STATUS = status;
                    _TREA_OPEN_REC.REGI_APPR_UID = cUserId;
                    _TREA_OPEN_REC.REGI_APPR_DT = dt;
                    _TREA_OPEN_REC.LAST_UPDATE_UID = cUserId;
                    _TREA_OPEN_REC.LAST_UPDATE_DT = dt;

                    TORs.Add(_TREA_OPEN_REC);
                    logStr += _TREA_OPEN_REC.modelToString(logStr);
                    //approvedList.Add(item.hvTREA_REGISTER_ID);

                    var _TREA_APLY_REC = db.TREA_APLY_REC.Where(y => y.TREA_REGISTER_ID == item.hvTREA_REGISTER_ID && y.APLY_STATUS == "D02")
                        .Where(y => y.APLY_STATUS == "D02")
                        .ToList();
                    _TREA_APLY_REC.ForEach(y =>
                    {
                        y.APLY_STATUS = status;
                        y.LAST_UPDATE_UID = cUserId;
                        y.LAST_UPDATE_DT = dt;

                        logStr += y.modelToString(logStr);

                        var vITEM_OP_TYPE = _TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == y.ITEM_ID)?.ITEM_OP_TYPE;
                        //var _OTHER_ITEM_APLY_ITEM_ID = db.OTHER_ITEM_APLY.FirstOrDefault(x => x.APLY_NO == y.APLY_NO)?.ITEM_ID;
                        var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == y.APLY_NO);
                        _OTHER_ITEM_APLY.ToList().ForEach(z =>
                        {
                            var _ITEM_SEAL = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);

                            if (vITEM_OP_TYPE == "2")
                            {
                                if (_TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == y.ITEM_ID).TREA_ITEM_TYPE == "SEAL")
                                {
                                    switch (y.ACTUAL_ACCESS_TYPE)
                                    {
                                        //存入
                                        case "P":
                                        case "S":
                                        case "A":
                                        case "B":
                                            _ITEM_SEAL.INVENTORY_STATUS = "1";
                                            _ITEM_SEAL.PUT_DATE = dt;
                                            break;
                                        //取出
                                        case "G":
                                            _ITEM_SEAL.INVENTORY_STATUS = "6";
                                            _ITEM_SEAL.GET_DATE = dt;
                                            break;
                                    }
                                    _ITEM_SEAL.LAST_UPDATE_DT = dt;
                                    logStr += _ITEM_SEAL.modelToString(logStr);
                                }
                            }
                            else if (vITEM_OP_TYPE == "3")
                            {
                                if (_TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == y.ITEM_ID).TREA_ITEM_TYPE == "SEAL")
                                {
                                    switch (y.ACTUAL_ACCESS_TYPE)
                                    {
                                        //存入
                                        case "P":
                                        case "S":
                                        case "A":
                                        case "B":
                                            if (_ITEM_SEAL.INVENTORY_STATUS == "3" && _ITEM_SEAL.SEAL_DESC_ACCESS == null)
                                            {
                                                _ITEM_SEAL.SEAL_DESC_ACCESS = _ITEM_SEAL.SEAL_DESC;
                                                _ITEM_SEAL.MEMO_ACCESS = _ITEM_SEAL.MEMO;
                                                _ITEM_SEAL.PUT_DATE_ACCESS = dt;
                                            }

                                            _ITEM_SEAL.INVENTORY_STATUS = "1";
                                            _ITEM_SEAL.PUT_DATE = dt;
                                            break;
                                        //取出
                                        case "G":
                                            _ITEM_SEAL.INVENTORY_STATUS = "2";
                                            _ITEM_SEAL.GET_DATE = dt;
                                            break;
                                    }
                                    _ITEM_SEAL.LAST_UPDATE_DT = dt;

                                    logStr += _ITEM_SEAL.modelToString(logStr);
                                }
                            }
                        });

                        switch (_TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == y.ITEM_ID)?.TREA_ITEM_TYPE)
                        {
                            case "BILL": // 空白票據
                                var _BLANK_NOTE_APLY = db.BLANK_NOTE_APLY.Where(x => x.APLY_NO == y.APLY_NO);

                                var item_Seq = "E2"; //空白票卷流水號開頭編碼

                                if (y.ACTUAL_ACCESS_TYPE == "P")
                                {
                                    _BLANK_NOTE_APLY.ToList().ForEach(x =>
                                    {
                                        var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');

                                        x.ITEM_BLANK_NOTE_FINAL_ITEM_ID = $@"{item_Seq}{item_id}"; //物品編號;
                                        logStr += x.modelToString(logStr);

                                        var ADD_ITEM_BLANK_NOTE = db.ITEM_BLANK_NOTE;
                                        ADD_ITEM_BLANK_NOTE.Add(new ITEM_BLANK_NOTE()
                                        {
                                            ITEM_ID = $@"{item_Seq}{item_id}", //物品編號;
                                            INVENTORY_STATUS = "1",
                                            ISSUING_BANK = x.ISSUING_BANK,
                                            CHECK_TYPE = x.CHECK_TYPE,
                                            CHECK_NO_TRACK = x.CHECK_NO_TRACK,
                                            CHECK_NO_B = x.CHECK_NO_B,
                                            CHECK_NO_E = x.CHECK_NO_E,
                                            APLY_DEPT = GetUserDPT(y.APLY_UID),
                                            APLY_SECT = y.APLY_UNIT,
                                            APLY_UID = y.APLY_UID,
                                            CHARGE_DEPT = GetUserDPT(y.APLY_UID),
                                            CHARGE_SECT = y.APLY_UNIT,
                                            CREATE_DT = dt,
                                            PUT_DATE = dt,
                                            CHECK_NO_B_ACCESS = x.CHECK_NO_B,
                                            CHECK_NO_E_ACCESS = x.CHECK_NO_E,
                                            CHECK_NO_TRACK_ACCESS = x.CHECK_NO_TRACK,
                                            CHECK_TYPE_ACCESS = x.CHECK_TYPE,
                                            ISSUING_BANK_ACCESS = x.ISSUING_BANK
                                        });
                                        logStr += ADD_ITEM_BLANK_NOTE.modelToString(logStr);
                                    });

                                }
                                else if (y.ACTUAL_ACCESS_TYPE == "G")
                                {
                                    _BLANK_NOTE_APLY.ToList().ForEach(x =>
                                    {
                                        var _ITEM_BLANK_NOTE = db.ITEM_BLANK_NOTE.FirstOrDefault(z => z.ITEM_ID == x.ITEM_BLANK_NOTE_ITEM_ID);

                                        if (_ITEM_BLANK_NOTE.INVENTORY_STATUS == "4")
                                        {
                                            _ITEM_BLANK_NOTE.INVENTORY_STATUS = "2";
                                            _ITEM_BLANK_NOTE.GET_DATE = dt;
                                            _ITEM_BLANK_NOTE.LAST_UPDATE_DT = dt;
                                            logStr += _ITEM_BLANK_NOTE.modelToString(logStr);

                                            x.ITEM_BLANK_NOTE_FINAL_ITEM_ID = x.ITEM_BLANK_NOTE_ITEM_ID;
                                            logStr += x.modelToString(logStr);
                                        }
                                        else
                                        {
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            x.ITEM_BLANK_NOTE_FINAL_ITEM_ID = $@"{item_Seq}{item_id}"; //物品編號;
                                            logStr += x.modelToString(logStr);

                                            var ADD_ITEM_BLANK_NOTE = db.ITEM_BLANK_NOTE;
                                            ADD_ITEM_BLANK_NOTE.Add(new ITEM_BLANK_NOTE()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}", //物品編號;
                                                INVENTORY_STATUS = "2",
                                                ISSUING_BANK = x.ISSUING_BANK,
                                                CHECK_TYPE = x.CHECK_TYPE,
                                                CHECK_NO_TRACK = x.CHECK_NO_TRACK,
                                                CHECK_NO_B = x.CHECK_NO_B,
                                                CHECK_NO_E = x.CHECK_NO_E,
                                                APLY_DEPT = GetUserDPT(y.APLY_UID),
                                                APLY_SECT = y.APLY_UNIT,
                                                APLY_UID = y.APLY_UID,
                                                CHARGE_DEPT = GetUserDPT(y.APLY_UID),
                                                CHARGE_SECT = y.APLY_UNIT,
                                                CREATE_DT = dt,
                                                GET_DATE = dt,
                                                PUT_DATE = _ITEM_BLANK_NOTE.PUT_DATE
                                            });
                                            logStr += x.modelToString(logStr);
                                        }
                                    });
                                }
                                break;
                            case "ESTATE": // 不動產
                                _OTHER_ITEM_APLY.ToList().ForEach(z =>
                                {
                                    var _ITEM_REAL_ESTATE = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);
                                    if (y.ACTUAL_ACCESS_TYPE == "P")
                                    {
                                        if (_ITEM_REAL_ESTATE.INVENTORY_STATUS == "3")
                                        {
                                            _ITEM_REAL_ESTATE.ESTATE_DATE_ACCESS = _ITEM_REAL_ESTATE.ESTATE_DATE;
                                            _ITEM_REAL_ESTATE.ESTATE_FORM_NO_ACCESS = _ITEM_REAL_ESTATE.ESTATE_FORM_NO;
                                            _ITEM_REAL_ESTATE.ESTATE_SEQ_ACCESS = _ITEM_REAL_ESTATE.ESTATE_SEQ;
                                            _ITEM_REAL_ESTATE.HOUSE_NO_ACCESS = _ITEM_REAL_ESTATE.HOUSE_NO;
                                            _ITEM_REAL_ESTATE.LAND_BUILDING_NO_ACCESS = _ITEM_REAL_ESTATE.LAND_BUILDING_NO;
                                            _ITEM_REAL_ESTATE.MEMO_ACCESS = _ITEM_REAL_ESTATE.MEMO;
                                            _ITEM_REAL_ESTATE.OWNERSHIP_CERT_NO_ACCESS = _ITEM_REAL_ESTATE.OWNERSHIP_CERT_NO;
                                        }

                                        _ITEM_REAL_ESTATE.INVENTORY_STATUS = "1";
                                        _ITEM_REAL_ESTATE.PUT_DATE = dt;
                                    }
                                    else if (y.ACTUAL_ACCESS_TYPE == "G")
                                    {
                                        _ITEM_REAL_ESTATE.INVENTORY_STATUS = "2";
                                        _ITEM_REAL_ESTATE.GET_DATE = dt;
                                    }
                                    _ITEM_REAL_ESTATE.LAST_UPDATE_DT = dt;

                                    logStr += _ITEM_REAL_ESTATE.modelToString(logStr);

                                });
                                break;
                            case "CA": // 電子憑證
                                _OTHER_ITEM_APLY.ToList().ForEach(z =>
                                {
                                    var _ITEM_CA = db.ITEM_CA.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);

                                    if (y.ACTUAL_ACCESS_TYPE == "P")
                                    {
                                        if (_ITEM_CA.INVENTORY_STATUS == "3")
                                        {
                                            _ITEM_CA.BANK_ACCESS = _ITEM_CA.BANK;
                                            _ITEM_CA.CA_DESC_ACCESS = _ITEM_CA.CA_DESC;
                                            _ITEM_CA.CA_NUMBER_ACCESS = _ITEM_CA.CA_NUMBER;
                                            _ITEM_CA.CA_USE_ACCESS = _ITEM_CA.CA_USE;
                                            _ITEM_CA.MEMO_ACCESS = _ITEM_CA.MEMO;
                                        }
                                        _ITEM_CA.INVENTORY_STATUS = "1";
                                        _ITEM_CA.PUT_DATE = dt;

                                    }
                                    else if (y.ACTUAL_ACCESS_TYPE == "G")
                                    {
                                        _ITEM_CA.INVENTORY_STATUS = "2";
                                        _ITEM_CA.GET_DATE = dt;
                                    }

                                    _ITEM_CA.LAST_UPDATE_DT = dt;

                                    logStr += _ITEM_CA.modelToString(logStr);
                                });
                                break;
                            case "DEPOSIT": //定期存單
                                _OTHER_ITEM_APLY.ToList().ForEach(z =>
                                {
                                    var _ITEM_DEP_ORDER_M = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);

                                    if (y.ACTUAL_ACCESS_TYPE == "P")
                                    {
                                        if (_ITEM_DEP_ORDER_M.INVENTORY_STATUS == "3")
                                        {
                                            var _ITEM_DEP_ORDER_D = db.ITEM_DEP_ORDER_D.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);
                                            _ITEM_DEP_ORDER_D.DENOMINATION_ACCESS = _ITEM_DEP_ORDER_D.DENOMINATION;
                                            _ITEM_DEP_ORDER_D.DEP_CNT_ACCESS = _ITEM_DEP_ORDER_D.DEP_CNT;
                                            _ITEM_DEP_ORDER_D.DEP_NO_B_ACCESS = _ITEM_DEP_ORDER_D.DEP_NO_B;
                                            _ITEM_DEP_ORDER_D.DEP_NO_E_ACCESS = _ITEM_DEP_ORDER_D.DEP_NO_E;
                                            _ITEM_DEP_ORDER_D.DEP_NO_PREAMBLE_ACCESS = _ITEM_DEP_ORDER_D.DEP_NO_PREAMBLE;
                                            _ITEM_DEP_ORDER_D.DEP_NO_TAIL_ACCESS = _ITEM_DEP_ORDER_D.DEP_NO_TAIL;
                                            _ITEM_DEP_ORDER_D.SUBTOTAL_DENOMINATION_ACCESS = _ITEM_DEP_ORDER_D.SUBTOTAL_DENOMINATION;

                                            logStr += _ITEM_DEP_ORDER_M.modelToString(logStr);

                                            _ITEM_DEP_ORDER_M.AUTO_TRANS_ACCESS = _ITEM_DEP_ORDER_M.AUTO_TRANS_ACCESS;
                                            _ITEM_DEP_ORDER_M.COMMIT_DATE_ACCESS = _ITEM_DEP_ORDER_M.COMMIT_DATE;
                                            _ITEM_DEP_ORDER_M.CURRENCY_ACCESS = _ITEM_DEP_ORDER_M.CURRENCY;
                                            _ITEM_DEP_ORDER_M.DEP_SET_QUALITY_ACCESS = _ITEM_DEP_ORDER_M.DEP_SET_QUALITY;
                                            _ITEM_DEP_ORDER_M.DEP_TYPE_ACCESS = _ITEM_DEP_ORDER_M.DEP_TYPE;
                                            _ITEM_DEP_ORDER_M.EXPIRY_DATE_ACCESS = _ITEM_DEP_ORDER_M.EXPIRY_DATE;
                                            _ITEM_DEP_ORDER_M.INTEREST_RATE_ACCESS = _ITEM_DEP_ORDER_M.INTEREST_RATE;
                                            _ITEM_DEP_ORDER_M.INTEREST_RATE_TYPE_ACCESS = _ITEM_DEP_ORDER_M.INTEREST_RATE_TYPE;
                                            _ITEM_DEP_ORDER_M.MEMO_ACCESS = _ITEM_DEP_ORDER_M.MEMO;
                                            _ITEM_DEP_ORDER_M.TOTAL_DENOMINATION_ACCESS = _ITEM_DEP_ORDER_M.TOTAL_DENOMINATION;
                                            _ITEM_DEP_ORDER_M.TRAD_PARTNERS_ACCESS = _ITEM_DEP_ORDER_M.TRAD_PARTNERS;
                                            _ITEM_DEP_ORDER_M.TRANS_EXPIRY_DATE_ACCESS = _ITEM_DEP_ORDER_M.TRANS_EXPIRY_DATE;
                                            _ITEM_DEP_ORDER_M.TRANS_TMS_ACCESS = _ITEM_DEP_ORDER_M.TRANS_TMS;
                                        }

                                        _ITEM_DEP_ORDER_M.INVENTORY_STATUS = "1";
                                        _ITEM_DEP_ORDER_M.PUT_DATE = dt;
                                    }
                                    else if (y.ACTUAL_ACCESS_TYPE == "G")
                                    {
                                        if (_ITEM_DEP_ORDER_M.DEP_SET_QUALITY == "N")
                                            _ITEM_DEP_ORDER_M.INVENTORY_STATUS = "2";
                                        else
                                            _ITEM_DEP_ORDER_M.INVENTORY_STATUS = "6";

                                        _ITEM_DEP_ORDER_M.GET_DATE = dt;
                                    }
                                    _ITEM_DEP_ORDER_M.LAST_UPDATE_DT = dt;

                                    logStr += _ITEM_DEP_ORDER_M.modelToString(logStr);
                                });
                                break;
                            case "STOCK": //股票
                                _OTHER_ITEM_APLY.ToList().ForEach(z =>
                                {
                                    var _ITEM_STOCK = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);

                                    if (y.ACTUAL_ACCESS_TYPE == "P")
                                    {
                                        if (_ITEM_STOCK.INVENTORY_STATUS == "3")
                                        {
                                            _ITEM_STOCK.DENOMINATION_ACCESS = _ITEM_STOCK.DENOMINATION;
                                            _ITEM_STOCK.MEMO_ACCESS = _ITEM_STOCK.MEMO;
                                            _ITEM_STOCK.NUMBER_OF_SHARES_ACCESS = _ITEM_STOCK.NUMBER_OF_SHARES;
                                            _ITEM_STOCK.STOCK_CNT_ACCESS = _ITEM_STOCK.STOCK_CNT;
                                            _ITEM_STOCK.STOCK_NO_B_ACCESS = _ITEM_STOCK.STOCK_NO_B;
                                            _ITEM_STOCK.STOCK_NO_E_ACCESS = _ITEM_STOCK.STOCK_NO_E;
                                            _ITEM_STOCK.STOCK_NO_PREAMBLE_ACCESS = _ITEM_STOCK.STOCK_NO_PREAMBLE;
                                            _ITEM_STOCK.STOCK_TYPE_ACCESS = _ITEM_STOCK.STOCK_TYPE;
                                            _ITEM_STOCK.AMOUNT_PER_SHARE_ACCESS = _ITEM_STOCK.AMOUNT_PER_SHARE;
                                            _ITEM_STOCK.SINGLE_NUMBER_OF_SHARES_ACCESS = _ITEM_STOCK.SINGLE_NUMBER_OF_SHARES;
                                        }

                                        _ITEM_STOCK.INVENTORY_STATUS = "1";
                                        _ITEM_STOCK.PUT_DATE = dt;
                                    }
                                    else if (y.ACTUAL_ACCESS_TYPE == "G")
                                    {
                                        _ITEM_STOCK.INVENTORY_STATUS = "2";
                                        _ITEM_STOCK.GET_DATE = dt;
                                    }
                                    _ITEM_STOCK.LAST_UPDATE_DT = dt;

                                    logStr += _ITEM_STOCK.modelToString(logStr);
                                });
                                break;
                            case "MARGING": //存出保證金
                                _OTHER_ITEM_APLY.ToList().ForEach(z =>
                                {
                                    var _ITEM_REFUNDABLE_DEP = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);

                                    if (y.ACTUAL_ACCESS_TYPE == "P")
                                    {
                                        if (_ITEM_REFUNDABLE_DEP.INVENTORY_STATUS == "3")
                                        {
                                            _ITEM_REFUNDABLE_DEP.AMOUNT_ACCESS = _ITEM_REFUNDABLE_DEP.AMOUNT;
                                            _ITEM_REFUNDABLE_DEP.BOOK_NO_ACCESS = _ITEM_REFUNDABLE_DEP.BOOK_NO;
                                            _ITEM_REFUNDABLE_DEP.DESCRIPTION_ACCESS = _ITEM_REFUNDABLE_DEP.DESCRIPTION;
                                            _ITEM_REFUNDABLE_DEP.MARGIN_DEP_TYPE_ACCESS = _ITEM_REFUNDABLE_DEP.MARGIN_DEP_TYPE;
                                            _ITEM_REFUNDABLE_DEP.MEMO_ACCESS = _ITEM_REFUNDABLE_DEP.MEMO;
                                            _ITEM_REFUNDABLE_DEP.TRAD_PARTNERS_ACCESS = _ITEM_REFUNDABLE_DEP.TRAD_PARTNERS;
                                            _ITEM_REFUNDABLE_DEP.WORKPLACE_CODE_ACCESS = _ITEM_REFUNDABLE_DEP.WORKPLACE_CODE;
                                        }

                                        _ITEM_REFUNDABLE_DEP.INVENTORY_STATUS = "1";
                                        _ITEM_REFUNDABLE_DEP.PUT_DATE = dt;
                                    }
                                    else if (y.ACTUAL_ACCESS_TYPE == "G")
                                    {
                                        _ITEM_REFUNDABLE_DEP.INVENTORY_STATUS = "2";
                                        _ITEM_REFUNDABLE_DEP.GET_DATE = dt;
                                    }
                                    _ITEM_REFUNDABLE_DEP.LAST_UPDATE_DT = dt;

                                    logStr += _ITEM_REFUNDABLE_DEP.modelToString(logStr);
                                });
                                break;
                            case "MARGINP": //存入保證金
                                _OTHER_ITEM_APLY.ToList().ForEach(z =>
                                {
                                    var _ITEM_DEP_RECEIVED = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);

                                    if (y.ACTUAL_ACCESS_TYPE == "P")
                                    {
                                        if (_ITEM_DEP_RECEIVED.INVENTORY_STATUS == "3")
                                        {
                                            _ITEM_DEP_RECEIVED.AMOUNT_ACCESS = _ITEM_DEP_RECEIVED.AMOUNT;
                                            _ITEM_DEP_RECEIVED.BOOK_NO_ACCESS = _ITEM_DEP_RECEIVED.BOOK_NO;
                                            _ITEM_DEP_RECEIVED.DESCRIPTION_ACCESS = _ITEM_DEP_RECEIVED.DESCRIPTION;
                                            _ITEM_DEP_RECEIVED.EFFECTIVE_DATE_B_ACCESS = _ITEM_DEP_RECEIVED.EFFECTIVE_DATE_B;
                                            _ITEM_DEP_RECEIVED.EFFECTIVE_DATE_E_ACCESS = _ITEM_DEP_RECEIVED.EFFECTIVE_DATE_E;
                                            _ITEM_DEP_RECEIVED.MARGIN_ITEM_ACCESS = _ITEM_DEP_RECEIVED.MARGIN_ITEM;
                                            _ITEM_DEP_RECEIVED.MARGIN_ITEM_ISSUER_ACCESS = _ITEM_DEP_RECEIVED.MARGIN_ITEM_ISSUER;
                                            _ITEM_DEP_RECEIVED.MARGIN_TAKE_OF_TYPE_ACCESS = _ITEM_DEP_RECEIVED.MARGIN_TAKE_OF_TYPE;
                                            _ITEM_DEP_RECEIVED.MEMO_ACCESS = _ITEM_DEP_RECEIVED.MEMO;
                                            _ITEM_DEP_RECEIVED.PLEDGE_ITEM_NO_ACCESS = _ITEM_DEP_RECEIVED.PLEDGE_ITEM_NO;
                                            _ITEM_DEP_RECEIVED.TRAD_PARTNERS_ACCESS = _ITEM_DEP_RECEIVED.TRAD_PARTNERS;
                                        }

                                        _ITEM_DEP_RECEIVED.INVENTORY_STATUS = "1";
                                        _ITEM_DEP_RECEIVED.PUT_DATE = dt;
                                    }
                                    else if (y.ACTUAL_ACCESS_TYPE == "G")
                                    {
                                        _ITEM_DEP_RECEIVED.INVENTORY_STATUS = "2";
                                        _ITEM_DEP_RECEIVED.GET_DATE = dt;
                                    }
                                    _ITEM_DEP_RECEIVED.LAST_UPDATE_DT = dt;

                                    logStr += _ITEM_DEP_RECEIVED.modelToString(logStr);
                                });
                                break;
                            case "ITEMIMP": //重要物品
                                var _OTHER_ITEM_APLY_FORITEMIMP = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => x.APLY_NO == y.APLY_NO);
                                var item_SeqIMP = "E8"; //重要物品流水號開頭編碼
                                _OTHER_ITEM_APLY_FORITEMIMP.ToList().ForEach(x =>
                                {
                                    var _ITEM_IMPO = db.ITEM_IMPO.FirstOrDefault(z => z.ITEM_ID == x.ITEM_ID);

                                    if (y.ACTUAL_ACCESS_TYPE == "P")
                                    {
                                        if (_ITEM_IMPO.INVENTORY_STATUS == "3")
                                        {
                                            _ITEM_IMPO.AMOUNT_ACCESS = _ITEM_IMPO.AMOUNT;
                                            _ITEM_IMPO.DESCRIPTION_ACCESS = _ITEM_IMPO.DESCRIPTION;
                                            _ITEM_IMPO.EXPECTED_ACCESS_DATE_ACCESS = _ITEM_IMPO.EXPECTED_ACCESS_DATE;
                                            _ITEM_IMPO.ITEM_NAME_ACCESS = _ITEM_IMPO.ITEM_NAME;
                                            _ITEM_IMPO.MEMO_ACCESS = _ITEM_IMPO.MEMO;
                                            _ITEM_IMPO.QUANTITY_ACCESS = _ITEM_IMPO.QUANTITY;
                                        }

                                        _ITEM_IMPO.INVENTORY_STATUS = "1";
                                        _ITEM_IMPO.PUT_DATE = dt;
                                        _ITEM_IMPO.LAST_UPDATE_DT = dt;
                                        logStr += _ITEM_IMPO.modelToString(logStr);
                                    }

                                    else if (y.ACTUAL_ACCESS_TYPE == "G")
                                    {
                                        if (_ITEM_IMPO.REMAINING == 0)
                                        {
                                            _ITEM_IMPO.INVENTORY_STATUS = "10"; //完全取出
                                            _ITEM_IMPO.LAST_UPDATE_DT = dt;
                                            logStr += _ITEM_IMPO.modelToString(logStr);
                                        }
                                        //取得流水號
                                        var item_id = sysSeqDao.qrySeqNo(item_SeqIMP, string.Empty).ToString().PadLeft(8, '0');
                                        var ADD_ITEM_IMPO = new ITEM_IMPO()
                                        {
                                            ITEM_ID = $@"{item_SeqIMP}{item_id}", //物品編號
                                            INVENTORY_STATUS = "2",
                                            ITEM_NAME = _ITEM_IMPO.ITEM_NAME,
                                            QUANTITY = x.Memo_I != null ? Convert.ToInt32(x.Memo_I) : 0,
                                            AMOUNT = _ITEM_IMPO.AMOUNT,
                                            EXPECTED_ACCESS_DATE = _ITEM_IMPO.EXPECTED_ACCESS_DATE,
                                            DESCRIPTION = _ITEM_IMPO.DESCRIPTION,
                                            MEMO = _ITEM_IMPO.MEMO,
                                            APLY_DEPT = _ITEM_IMPO.APLY_DEPT,
                                            APLY_SECT = _ITEM_IMPO.APLY_SECT,
                                            APLY_UID = _ITEM_IMPO.APLY_UID,
                                            CHARGE_DEPT = _ITEM_IMPO.CHARGE_DEPT,
                                            CHARGE_SECT = _ITEM_IMPO.CHARGE_SECT,
                                            GET_DATE = dt,
                                            PUT_DATE = _ITEM_IMPO.PUT_DATE,
                                            LAST_UPDATE_DT = dt,
                                            ITEM_NAME_AFT = _ITEM_IMPO.ITEM_NAME_AFT,
                                            REMAINING_AFT = _ITEM_IMPO.REMAINING_AFT,
                                            AMOUNT_AFT = _ITEM_IMPO.AMOUNT_AFT,
                                            DESCRIPTION_AFT = _ITEM_IMPO.DESCRIPTION_AFT,
                                            EXPECTED_ACCESS_DATE_AFT = _ITEM_IMPO.EXPECTED_ACCESS_DATE_AFT,
                                            MEMO_AFT = _ITEM_IMPO.MEMO_AFT,
                                            CHARGE_DEPT_AFT = _ITEM_IMPO.CHARGE_DEPT_AFT,
                                            CHARGE_SECT_AFT = _ITEM_IMPO.CHARGE_SECT_AFT,
                                            REMAINING = _ITEM_IMPO.REMAINING,
                                            ITEM_ID_FROM = _ITEM_IMPO.ITEM_ID
                                        };
                                        db.ITEM_IMPO.Add(ADD_ITEM_IMPO);
                                        logStr += ADD_ITEM_IMPO.modelToString(logStr);
                                        x.Memo_S = ADD_ITEM_IMPO.ITEM_ID;
                                    }

                                });
                                break;
                            case "ITEMOTH": //其他物品
                                _OTHER_ITEM_APLY.ToList().ForEach(z =>
                                {
                                    var _ITEM_OTHER = db.ITEM_OTHER.FirstOrDefault(x => x.ITEM_ID == z.ITEM_ID);

                                    if (y.ACTUAL_ACCESS_TYPE == "P")
                                    {
                                        _ITEM_OTHER.INVENTORY_STATUS = "1";
                                        _ITEM_OTHER.PUT_DATE = dt;
                                    }
                                    else if (y.ACTUAL_ACCESS_TYPE == "G")
                                    {
                                        _ITEM_OTHER.INVENTORY_STATUS = "2";
                                        _ITEM_OTHER.GET_DATE = dt;
                                    }
                                    _ITEM_OTHER.LAST_UPDATE_DT = dt;

                                    logStr += _ITEM_OTHER.modelToString(logStr);
                                });
                                break;
                        }
                        #region 申請單歷程檔
                        var ARH = new APLY_REC_HIS()
                        {
                            APLY_NO = y.APLY_NO,
                            APLY_STATUS = status,
                            PROC_UID = cUserId,
                            PROC_DT = dt
                        };
                        logStr += ARH.modelToString(logStr);
                        db.APLY_REC_HIS.Add(ARH);
                        #endregion
                        aplyNoUid = string.Format("{0};{1};{2}", y.APLY_NO, y.APLY_UID, vITEM_OP_TYPE);
                        approvedList.Add(aplyNoUid);

                    });
                }
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

                        MAIL_TIME MT = new MAIL_TIME();
                        MAIL_CONTENT MC = new MAIL_CONTENT();
                        //通知表單申請人及保管科人員已完成物品存入,取出 抓4

                        List<Tuple<string, string>> _ccTo = new List<Tuple<string, string>>();
                        MT = db.MAIL_TIME.AsNoTracking().FirstOrDefault(x => x.MAIL_TIME_ID == "4" && x.IS_DISABLED != "Y");
                        var _MAIL_CONTENT_ID = MT?.MAIL_CONTENT_ID;
                        MC = db.MAIL_CONTENT.AsNoTracking().FirstOrDefault(x => x.MAIL_CONTENT_ID == _MAIL_CONTENT_ID && x.IS_DISABLED != "Y");
                        var _MAIL_RECEIVE = db.MAIL_RECEIVE.AsNoTracking();
                        var _CODE_ROLE_FUNC = db.CODE_ROLE_FUNC.AsNoTracking();
                        var _CODE_USER_ROLE = db.CODE_USER_ROLE.AsEnumerable();
                        var _CODE_USER = db.CODE_USER.AsNoTracking();
                        var emps = GetEmps();
                        #region 寄信
                        if (MC != null)
                        {
                            var _FuncId = _MAIL_RECEIVE.Where(x => x.MAIL_CONTENT_ID == MC.MAIL_CONTENT_ID).Select(x => x.FUNC_ID);
                            var _RoleId = _CODE_ROLE_FUNC.Where(x => _FuncId.Contains(x.FUNC_ID)).Select(x => x.ROLE_ID);
                            var _UserId = _CODE_USER_ROLE.Where(x => _RoleId.Contains(x.ROLE_ID)).Select(x => x.USER_ID).Distinct();
                            List<string> _userIdList = new List<string>();

                            _userIdList.AddRange(_CODE_USER.Where(x => _UserId.Contains(x.USER_ID) && x.IS_MAIL == "Y").Select(x => x.USER_ID).ToList());
                            if (_userIdList.Any())
                            {
                                //人名 EMAIl
                                var _EMP = emps.Where(x => _userIdList.Contains(x.USR_ID)).ToList();
                                if (_EMP.Any())
                                {
                                    _EMP.ForEach(x =>
                                    {
                                        _ccTo.Add(new Tuple<string, string>(x.EMAIL, x.EMP_NAME));
                                    });
                                }
                            }


                            foreach (var NoUidType in approvedList)
                            {
                                //List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys") };
                                List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>();
                                var aplyNo = NoUidType.Split(';')[0];
                                var aplyUid = NoUidType.Split(';')[1];
                                var itemOpType = NoUidType.Split(';')[2];
                                if (itemOpType == "3")
                                {
                                    string str = MC?.MAIL_CONTENT1 ?? "通知您申請金庫存取之申請單號:@_APLYNO_已完成相關作業! ";
                                    StringBuilder sb = new StringBuilder();
                                    str = str.Replace("@_APLYNO_", aplyNo);
                                    sb.AppendLine(str);

                                    var _aplyInfo = emps.FirstOrDefault(x => x.USR_ID == aplyUid);
                                    if (_aplyInfo != null)
                                    {
                                        _mailTo.Add(new Tuple<string, string>(_aplyInfo.EMAIL, _aplyInfo.EMP_NAME));
                                    }

                                    try
                                    {
                                        var sms = new SendMail.SendMailSelf();
                                        sms.smtpPort = 25;
                                        sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                                        sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                                        sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                                        sms.Mail_Send(
                                           new Tuple<string, string>(sms.mailAccount, "金庫管理系統"),
                                           _mailTo,
                                           _ccTo,
                                           MT?.FUNC_ID ?? "存取作業確認通知",
                                           sb.ToString()
                                           );
                                    }
                                    catch (Exception ex)
                                    {
                                        result.DESCRIPTION = $"Email 發送失敗請人工通知。";
                                    }
                                }
                            }
                        }
                        //                        foreach (var TOR in TORs)
                        //                        {
                        //                            StringBuilder sb = new StringBuilder();
                        //                            sb.AppendLine(

                        //        $@"您好,
                        //通知今日金庫開關庫時間為:{TOR.OPEN_TREA_TIME}，請準時至金庫門口集合。
                        //為配合金庫大門之啟閉，請有權人在:{TOR.EXEC_TIME_E} 前進入「金庫進出管理系統」完成入庫確認作業，謝謝。
                        //");
                        //                            try
                        //                            {
                        //                                var sms = new SendMail.SendMailSelf();
                        //                                sms.smtpPort = 25;
                        //                                sms.smtpServer = Properties.Settings.Default["smtpServer"]?.ToString();
                        //                                sms.mailAccount = Properties.Settings.Default["mailAccount"]?.ToString();
                        //                                sms.mailPwd = Properties.Settings.Default["mailPwd"]?.ToString();
                        //                                sms.Mail_Send(
                        //                                    new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys"),
                        //                                    new List<Tuple<string, string>>() { new Tuple<string, string>("glsisys.life@fbt.com", "測試帳號-glsisys") },
                        //                                    null,
                        //                                    "存取作業確認通知",
                        //                                    sb.ToString()
                        //                                    );
                        //                            }
                        //                            catch(Exception ex)
                        //                            {
                        //                                result.DESCRIPTION = $"Email 發送失敗請人工通知。";
                        //                            }
                        //                        }
                        #endregion

                        #region LOG
                        //新增LOG
                        Log log = new Log();
                        log.CFUNCTION = "覆核-金庫登記簿覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", approvedList.Select(x => x.Split(';')[0]))} 覆核成功";
                        result.Datas = GetSearchDatas(cUserId);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="RejectReason"></param>
        /// <param name="ViewModel"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<List<TREAReviewWorkDetailViewModel>> RejectData(string RejectReason, List<TREAReviewWorkDetailViewModel> ViewModel, string cUserId)
        {
            var result = new MSGReturnModel<List<TREAReviewWorkDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!ViewModel.Any())
            {
                return result;
            }

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            string status = Ref.AccessProjectFormStatus.D04.ToString(); // 金庫登記簿覆核退回
            List<string> rejectList = new List<string>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                foreach (var item in ViewModel.Where(x => x.Ischecked))
                {
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == item.hvTREA_REGISTER_ID);
                    if (_TREA_OPEN_REC == null) //找不到該單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.hvTREA_REGISTER_ID}");
                        return result;
                    }
                    if (_TREA_OPEN_REC.LAST_UPDATE_DT > item.vLAST_UPDATE_DT) //資料已備更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.hvTREA_REGISTER_ID}");
                        return result;
                    }
                    _TREA_OPEN_REC.REGI_STATUS = status;
                    _TREA_OPEN_REC.REGI_APPR_UID = cUserId;
                    _TREA_OPEN_REC.REGI_APPR_DT = dt;
                    _TREA_OPEN_REC.REGI_APPR_DESC = RejectReason.Trim();
                    _TREA_OPEN_REC.LAST_UPDATE_UID = cUserId;
                    _TREA_OPEN_REC.LAST_UPDATE_DT = dt;

                    logStr += _TREA_OPEN_REC.modelToString(logStr);
                    rejectList.Add(item.hvTREA_REGISTER_ID);

                    var _TREA_APLY_REC = db.TREA_APLY_REC.Where(y => y.TREA_REGISTER_ID == item.hvTREA_REGISTER_ID)
                        .Where(y => y.APLY_STATUS == "D02")
                        .ToList();
                    _TREA_APLY_REC.ForEach(y =>
                    {
                        y.APLY_STATUS = status;
                        y.LAST_UPDATE_UID = cUserId;
                        y.LAST_UPDATE_DT = dt;
                        logStr += y.modelToString(logStr);
                    });
                }

                //檢核欄位
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
                        log.CFUNCTION = "駁回-金庫登記簿覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", rejectList)} 已駁回";
                        result.Datas = GetSearchDatas(cUserId);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 取得 人員基本資料
        /// </summary>
        /// <param name="cUserID"></param>
        /// <returns></returns>
        public BaseUserInfoModel GetUserInfo(string cUserID)
        {
            if (cUserID.IsNullOrWhiteSpace())
            {
                return null;
            }
            BaseUserInfoModel user = new BaseUserInfoModel();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                var _emply = dbINTRA.V_EMPLY2.AsNoTracking().FirstOrDefault(x => x.USR_ID == cUserID);
                if (_emply != null)
                {
                    user.EMP_ID = cUserID;
                    user.EMP_Name = _emply.EMP_NAME?.Trim();
                    user.DPT_ID = _emply.DPT_CD?.Trim();
                    user.DPT_Name = _emply.DPT_NAME?.Trim();
                }
            }
            return user;
        }

        /// <summary>
        /// 查部門
        /// </summary>
        public string GetUserDPT(string cUserID)
        {
            string DPT_CD = "";
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                var _emply_DPT_CD = dbINTRA.V_EMPLY2.AsNoTracking().FirstOrDefault(x => x.USR_ID == cUserID)?.DPT_CD;
                if (_emply_DPT_CD != null)
                {
                    var _VW_OA_DEPT = dbINTRA.VW_OA_DEPT.AsNoTracking().FirstOrDefault(x => x.DPT_CD == _emply_DPT_CD);
                    if (_VW_OA_DEPT != null)
                    {
                        if (_VW_OA_DEPT.Dpt_type == "04")
                        {
                            DPT_CD = _VW_OA_DEPT.UP_DPT_CD?.Trim();
                        }
                        else if (_VW_OA_DEPT.Dpt_type == "03" || _VW_OA_DEPT.Dpt_type == "02")
                        {
                            DPT_CD = _VW_OA_DEPT.DPT_CD?.Trim();
                        }
                    }
                }
            }
            return DPT_CD;
        }
    }
}