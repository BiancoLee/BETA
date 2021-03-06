﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;

namespace Treasury.Web.Service.Actual
{

    public class TreasuryReport : ITreasuryReport
    {
        #region Get Date
        public TreasuryReportViewModel GetItemId(bool IsCustody, string DEPT_ITEM, string UserBranch)
        {
            var result = new TreasuryReportViewModel();
            List<SelectOption> jobProject = new List<SelectOption>(); //庫存表名稱
            List<SelectOption> vName = new List<SelectOption>(); //股票編號
            List<SelectOption> TRAD = new List<SelectOption>(); //交易對象
            List<SelectOption> dept = new List<SelectOption>(); //權責部門
            List<SelectOption> branch = new List<SelectOption>(); //權責科別
            var strs = new List<string>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {

                var deps = new Treasury.Web.Service.Actual.Common().GetDepts();
                var items = new List<string>()
                {
                     Ref.TreaItemType.D1012.ToString(),// 空白票據項目 用於條件判斷
                     Ref.TreaItemType.D1019.ToString(),// 其他物品項目 用於條件判斷
                };
                var bill = Ref.TreaItemType.D1012.ToString();

                //if (IsCustody)
                //{
                    //jobProject = db.TREA_ITEM.AsNoTracking() // 抓資料表的所有資料
                    //             .Where(x => x.ITEM_OP_TYPE == "3" &&
                    //             x.IS_DISABLED == "N" &&
                    //             !items.Contains(x.ITEM_ID)) //條件
                    //             .AsEnumerable().Select(x => new SelectOption()
                    //             {
                    //                   Value = x.ITEM_ID,
                    //                   Text = x.ITEM_DESC
                    //             }).ToList();

                    //印章
                  //  if(db.ITEM_SEAL.AsNoTracking()//抓取在庫的所有資料
                  //.Where(x => x.INVENTORY_STATUS == "1")
                  //.Select(x => x.TREA_ITEM_NAME)
                  //.Distinct()
                  //.OrderBy(x => x).ToList().Count > 0 )
                  //      strs.Add(Ref.TreaItemType.D1013.ToString());

                //}
                //else
                //{
                    //印章
                    strs = db.ITEM_SEAL.AsNoTracking()//抓取在庫的所有資料
                   .Where(x => x.INVENTORY_STATUS == "1")
                   .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                   .Select(x => x.TREA_ITEM_NAME)
                   .Distinct()
                   .OrderBy(x => x).ToList();


                    if (db.ITEM_DEP_ORDER_M.AsNoTracking()//抓取在庫的所有資料
                              .Where(x => x.INVENTORY_STATUS == "1")
                              .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                              .Select(x => x.CHARGE_SECT)
                              .Distinct()
                              .OrderBy(x => x).ToList().Count > 0)
                        strs.Add(Ref.TreaItemType.D1013.ToString());

                    if(db.ITEM_REAL_ESTATE.AsNoTracking()//抓取在庫的所有資料
                             .Where(x => x.INVENTORY_STATUS == "1")
                             .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                             .Select(x => x.CHARGE_SECT)
                             .Distinct()
                             .OrderBy(x => x).ToList().Count > 0)
                        strs.Add(Ref.TreaItemType.D1014.ToString());

                    if(db.ITEM_STOCK.AsNoTracking()//抓取在庫的所有資料
                             .Where(x => x.INVENTORY_STATUS == "1")
                             .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                             .Select(x => x.CHARGE_SECT)
                             .Distinct()
                             .OrderBy(x => x).ToList().Count > 0)
                        strs.Add(Ref.TreaItemType.D1015.ToString());

                    if(db.ITEM_REFUNDABLE_DEP.AsNoTracking()//抓取在庫的所有資料
                             .Where(x => x.INVENTORY_STATUS == "1")
                             .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                             .Select(x => x.CHARGE_SECT)
                             .Distinct()
                             .OrderBy(x => x).ToList().Count > 0)
                        strs.Add(Ref.TreaItemType.D1016.ToString());

                    if(db.ITEM_DEP_RECEIVED.AsNoTracking()//抓取在庫的所有資料
                             .Where(x => x.INVENTORY_STATUS == "1")
                             .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                             .Select(x => x.CHARGE_SECT)
                             .Distinct()
                             .OrderBy(x => x).ToList().Count > 0)
                        strs.Add(Ref.TreaItemType.D1017.ToString());

                    if(db.ITEM_IMPO.AsNoTracking()//抓取在庫的所有資料
                             .Where(x => x.INVENTORY_STATUS == "1")
                             .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                             .Select(x => x.CHARGE_SECT)
                             .Distinct()
                             .OrderBy(x => x).ToList().Count > 0)
                        strs.Add(Ref.TreaItemType.D1018.ToString());

                    if( db.ITEM_OTHER.AsNoTracking()//抓取在庫的所有資料
                             .Where(x => x.INVENTORY_STATUS == "1")
                             .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                             .Select(x => x.CHARGE_SECT)
                             .Distinct()
                             .OrderBy(x => x).ToList().Count > 0)
                        strs.Add(Ref.TreaItemType.D1019.ToString());

                    if(db.ITEM_CA.AsNoTracking()//抓取在庫的所有資料
                             .Where(x => x.INVENTORY_STATUS == "1")
                             .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch, !IsCustody)
                             .Select(x => x.CHARGE_SECT)
                             .Distinct()
                             .OrderBy(x => x).ToList().Count > 0)
                        strs.Add(Ref.TreaItemType.D1024.ToString());
               // }

                jobProject = db.TREA_ITEM.AsNoTracking() // 抓資料表的所有資料
                                 .Where(x => x.ITEM_OP_TYPE == "3" &&
                                 x.IS_DISABLED == "N" && 
                                 !items.Contains(x.ITEM_ID) &&
                                 strs.Contains(x.ITEM_ID)) //條件
                                 .AsEnumerable().Select(x => new SelectOption()
                                 {
                                     Value = x.ITEM_ID,
                                     Text = x.ITEM_DESC
                                 }).ToList();

                if (strs.Count > 0)
                {
                    string firstItem = strs.FirstOrDefault();
                    Ref.TreaItemType _firstItem;
                    if (System.Enum.TryParse(firstItem, false, out _firstItem)){
                        switch (_firstItem)
                        {
                            case Ref.TreaItemType.D1008://印章
                            case Ref.TreaItemType.D1009:
                            case Ref.TreaItemType.D1010:
                            case Ref.TreaItemType.D1011:
                                var item = firstItem;

                                strs = db.ITEM_SEAL.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1" &&
                               x.TREA_ITEM_NAME == item)
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;

                            case Ref.TreaItemType.D1013://定期存單
                                strs = db.ITEM_DEP_ORDER_M.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1")
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;
                            case Ref.TreaItemType.D1014://不動產
                                strs = db.ITEM_REAL_ESTATE.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1")
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;
                            case Ref.TreaItemType.D1015://股票
                                strs = db.ITEM_STOCK.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1")
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;
                            case Ref.TreaItemType.D1016://存出保證金
                                strs = db.ITEM_REFUNDABLE_DEP.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1")
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;
                            case Ref.TreaItemType.D1017://存入保證金
                                strs = db.ITEM_DEP_RECEIVED.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1")
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;
                            case Ref.TreaItemType.D1018://重要物品
                                strs = db.ITEM_IMPO.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1")
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;
                            case Ref.TreaItemType.D1019://其他物品
                                strs = db.ITEM_OTHER.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1")
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;
                            case Ref.TreaItemType.D1024://電子憑證
                                strs = db.ITEM_CA.AsNoTracking()//抓取在庫的所有資料
                               .Where(x => x.INVENTORY_STATUS == "1")
                               .Select(x => x.CHARGE_DEPT)
                               .Distinct()
                               .OrderBy(x => x).ToList();
                                break;
                        }
                    } 
                }

                //var ITEM_SEALs= db.ITEM_SEAL.AsNoTracking()//抓取在庫的所有資料
                //.Where(x => x.INVENTORY_STATUS == "1").ToList();

                List<SelectOption> _dept = new List<SelectOption>();
                List<SelectOption> _branch = new List<SelectOption>();

                    _dept = strs.Select(x => x).Distinct()
                    .OrderBy(x => x)
                    .Select(x => new SelectOption()
                    {
                        Value = x,
                        Text = deps.FirstOrDefault(y => y.DPT_CD == x)?.DPT_NAME
                    }).ToList();

                var vNames =  new Stock().GetStockName();
                vNames.RemoveAt(0);
                vNames.Insert(0,new SelectOption(){Value="All",Text="All"});
                result.vName =vNames;
                var vBook_Nos = new Estate().GetBuildName();
                vBook_Nos.RemoveAt(0);
                vBook_Nos.Insert(0,new SelectOption(){Value="All",Text="All"});
                result.vBook_No = vBook_Nos;
                TRAD = new Deposit().GetTRAD_Partners();
                TRAD.Insert(0, new SelectOption() { Value = "All", Text = "All" });
                result.vTRAD_Partners = TRAD;
                result.vdept = _dept;
                result.vjobProject= jobProject;
                return result;
            }
        }

            public  List<SelectOption> getDEPT(Ref.TreaItemType type, bool IsCustody, string UserDept)
            {
             var result = new List<SelectOption>();
             var strs = new  List<string>();
             var deps = new Treasury.Web.Service.Actual.Common().GetDepts();
               using (TreasuryDBEntities db = new TreasuryDBEntities())
               {
                if (IsCustody)
                {
                    switch (type)
                    {
                        case Ref.TreaItemType.D1008://印章
                        case Ref.TreaItemType.D1009:
                        case Ref.TreaItemType.D1010:
                        case Ref.TreaItemType.D1011:
                            var item = type.ToString();

                            strs = db.ITEM_SEAL.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" &&
                           x.TREA_ITEM_NAME == item)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;

                        case Ref.TreaItemType.D1013://定期存單
                            strs = db.ITEM_DEP_ORDER_M.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1014://不動產
                            strs = db.ITEM_REAL_ESTATE.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1015://股票
                            strs = db.ITEM_STOCK.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1016://存出保證金
                            strs = db.ITEM_REFUNDABLE_DEP.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1017://存入保證金
                            strs = db.ITEM_DEP_RECEIVED.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1018://重要物品
                            strs = db.ITEM_IMPO.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1019://其他物品
                            strs = db.ITEM_OTHER.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1024://電子憑證
                            strs = db.ITEM_CA.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case Ref.TreaItemType.D1008://印章
                        case Ref.TreaItemType.D1009:
                        case Ref.TreaItemType.D1010:
                        case Ref.TreaItemType.D1011:
                            var item = type.ToString();

                            strs = db.ITEM_SEAL.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" &&
                           x.TREA_ITEM_NAME == item && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;

                        case Ref.TreaItemType.D1013://定期存單
                            strs = db.ITEM_DEP_ORDER_M.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1014://不動產
                            strs = db.ITEM_REAL_ESTATE.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1015://股票
                            strs = db.ITEM_STOCK.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1016://存出保證金
                            strs = db.ITEM_REFUNDABLE_DEP.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1017://存入保證金
                            strs = db.ITEM_DEP_RECEIVED.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1018://重要物品
                            strs = db.ITEM_IMPO.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1019://其他物品
                            strs = db.ITEM_OTHER.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1024://電子憑證
                            strs = db.ITEM_CA.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.CHARGE_DEPT == UserDept)
                           .Select(x => x.CHARGE_DEPT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                    }
                }
                   

                    result = strs
                    .Select(x=>new SelectOption()
                    {
                        Value = x,
                        Text = deps.FirstOrDefault(y => 
                            y.DPT_CD != null &&
                            y.DPT_CD.Trim() == x)?.DPT_NAME
                    }).ToList();
               }
             return result ;
            }


         public  List<SelectOption> getSECT(string DEPT_ITEM, Ref.TreaItemType type, bool IsCustody, string UserBranch)
         {
             var result = new List<SelectOption>();
             var strs = new  List<string>();
             var deps = new Treasury.Web.Service.Actual.Common().GetDepts();
               using (TreasuryDBEntities db = new TreasuryDBEntities())
               {
                if (IsCustody)
                {
                    switch (type)
                    {
                        case Ref.TreaItemType.D1008://印章
                        case Ref.TreaItemType.D1009:
                        case Ref.TreaItemType.D1010:
                        case Ref.TreaItemType.D1011:
                            var item = type.ToString();
                            strs = db.ITEM_SEAL.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.TREA_ITEM_NAME == item)
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;

                        case Ref.TreaItemType.D1013://定期存單
                            strs = db.ITEM_DEP_ORDER_M.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1014://不動產
                            strs = db.ITEM_REAL_ESTATE.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;

                        case Ref.TreaItemType.D1015://股票
                            strs = db.ITEM_STOCK.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1016://存出保證金
                            strs = db.ITEM_REFUNDABLE_DEP.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1017://存入保證金
                            strs = db.ITEM_DEP_RECEIVED.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1018://重要物品
                            strs = db.ITEM_IMPO.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1019://其他物品
                            strs = db.ITEM_OTHER.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1024://電子憑證
                            strs = db.ITEM_CA.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case Ref.TreaItemType.D1008://印章
                        case Ref.TreaItemType.D1009:
                        case Ref.TreaItemType.D1010:
                        case Ref.TreaItemType.D1011:
                            var item = type.ToString();
                            strs = db.ITEM_SEAL.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1" && x.TREA_ITEM_NAME == item)
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;

                        case Ref.TreaItemType.D1013://定期存單
                            strs = db.ITEM_DEP_ORDER_M.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1014://不動產
                            strs = db.ITEM_REAL_ESTATE.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;

                        case Ref.TreaItemType.D1015://股票
                            strs = db.ITEM_STOCK.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1016://存出保證金
                            strs = db.ITEM_REFUNDABLE_DEP.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1017://存入保證金
                            strs = db.ITEM_DEP_RECEIVED.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1018://重要物品
                            strs = db.ITEM_IMPO.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1019://其他物品
                            strs = db.ITEM_OTHER.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                        case Ref.TreaItemType.D1024://電子憑證
                            strs = db.ITEM_CA.AsNoTracking()//抓取在庫的所有資料
                           .Where(x => x.INVENTORY_STATUS == "1")
                           .Where(x => x.CHARGE_DEPT == DEPT_ITEM && x.CHARGE_SECT == UserBranch)
                           .Select(x => x.CHARGE_SECT)
                           .Distinct()
                           .OrderBy(x => x).ToList();
                            break;
                    }
                }

                if (strs.Count <= 1 && strs.FirstOrDefault()?.Trim() == "")
                {
                   
                }
                else
                {
                    result = strs
                   .Select(x => new SelectOption()
                   {
                       Value = x,
                       Text = deps.FirstOrDefault(y =>
                           y.DPT_CD != null &&
                           y.DPT_CD.Trim() == x)?.DPT_NAME
                   }).ToList();
                }
                    
               }
             return result ;
         }
      
        #endregion

        #region Save Data

        #endregion

        #region private function
          
        #endregion
         }
    }
