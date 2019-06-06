using Treasury.WebActionFilter;
using System;
using Treasury.Web.Properties;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.Controllers;
using Treasury.Web.ViewModels;
using System.Linq;
using Treasury.Web.Enum;
/// <summary>
/// 功能說明：保管物品庫存表列印作業
/// 初版作者：20180828 卓建毅
/// 修改歷程：2018 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class TreasuryReportController : CommonController
    {
        
        private ITreasuryReport TreasuryReport;
        private IDeposit Deposit;
        public TreasuryReportController()
        {
            Deposit = new Deposit();
            TreasuryReport = new TreasuryReport();
        }

        /// <summary>
        /// 資料查詢異動作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var All = new SelectOption() { Text = "All", Value = "All" };
            //var _CustodyFlag = Convert.ToBoolean(Session["CustodyFlag"]);
            var _CustodyFlag = AccountController.CustodianFlag;
            string Det_cd = new Service.Actual.Common().GetEmps()?.FirstOrDefault(x => x.USR_ID != null && x.USR_ID == AccountController.CurrentUserId)?.DPT_CD?.Trim();
            var _Depts = new Service.Actual.Common().GetDepts()?.FirstOrDefault(x => x.DPT_CD?.Trim() == Det_cd);
            string branch = "";
            string dept = "";

            //_Depts.UP_DPT_CD = "XK000";
            //_Depts.Dpt_type = "03";
            //_Depts.DPT_CD = "VK100";
            //Det_cd = "VK100";
            //_CustodyFlag = false;

            if (_Depts != null && _Depts.Dpt_type == "04")
            {
                branch = Det_cd;
                dept = _Depts?.UP_DPT_CD?.Trim();
            }
            else if (_Depts != null)
            {
                branch = "";
                dept = _Depts?.DPT_CD?.Trim();
            }
            else
            {
                branch = "";
                dept = "";
            }


            ViewBag.CustodyFlag = _CustodyFlag;
            ViewBag.opScope = GetopScope("~/TreasuryReport/");

            var viewModel = TreasuryReport.GetItemId(_CustodyFlag, dept, branch);

            return View(viewModel);
        }

        /// <summary>
        /// 抓取權責部門,科別名稱
        /// </summary>
        /// <param name="type"></param>
        /// <param name="DEPT_ITEM">部門名稱</param>
        /// <returns></returns>
        public JsonResult GetCharge(Ref.TreaItemType  type , string DEPT_ITEM)
        {
            var result = new List<SelectOption>();
            var All = new SelectOption() { Text = "All", Value = "All" };

            // 190603 Edited by Biacno 非保管科只能顯示本科資料
            //var _CustodyFlag = Convert.ToBoolean(Session["CustodyFlag"]);
            var _CustodyFlag = AccountController.CustodianFlag;
            string Det_cd = new Service.Actual.Common().GetEmps()?.FirstOrDefault(x => x.USR_ID != null && x.USR_ID == AccountController.CurrentUserId)?.DPT_CD?.Trim();
            var _Depts = new Service.Actual.Common().GetDepts()?.FirstOrDefault(x => x.DPT_CD.Trim() == Det_cd);
            string branch = "";
            string dept = "";
            if (_Depts != null && _Depts.Dpt_type == "04")
            {
                branch = Det_cd;
                dept = _Depts?.UP_DPT_CD?.Trim();
            }
            else if (_Depts != null)
            {
                branch = "";
                dept = _Depts?.DPT_CD?.Trim();
            }
            else
            {
                branch = "";
                dept = "";
            }

            //_Depts.UP_DPT_CD = "X0000";
            //Det_cd = "XQ000";
            ///////////////////////////////

            if (DEPT_ITEM.IsNullOrWhiteSpace())
            {
                result = TreasuryReport.getDEPT(type, _CustodyFlag, dept);
            }
            else
            {
               result =  TreasuryReport.getSECT(DEPT_ITEM,type, _CustodyFlag, dept);
            }
            if (_CustodyFlag)
                result.Insert(0, All);
            return Json(result);
        }
        public JsonResult GetNAME( Ref.TreaItemType type,string DEPT_ITEM = null)
        {
             var result = new List<SelectOption>();
             var All = new SelectOption() { Text = "All", Value = "All" };         
             var deps = new Treasury.Web.Service.Actual.Common().GetDepts();

            // 190603 Edited by Biacno 非保管科只能顯示本科資料
            //var _CustodyFlag = Convert.ToBoolean(Session["CustodyFlag"]);
            var _CustodyFlag = AccountController.CustodianFlag;
            string Det_cd = new Service.Actual.Common().GetEmps()?.FirstOrDefault(x => x.USR_ID != null && x.USR_ID == AccountController.CurrentUserId)?.DPT_CD?.Trim();
            var _Depts = new Service.Actual.Common().GetDepts()?.FirstOrDefault(x => x.DPT_CD.Trim() == Det_cd);
            
            string branch = "";
            string dept = "";

            //_Depts.UP_DPT_CD = "XK000";
            //_Depts.Dpt_type = "03";
            //_Depts.DPT_CD = "VK100";
            //Det_cd = "VK100";
            //_CustodyFlag = false;

            if (_Depts != null && _Depts.Dpt_type == "04")
            {
                branch = Det_cd;
                dept = _Depts?.UP_DPT_CD;
            }
            else if (_Depts != null)
            {
                branch = "";
                dept = _Depts?.DPT_CD;
            }
            else
            {
                branch = "";
                dept = "";
            }
            //////////////////

            if (!DEPT_ITEM.IsNullOrWhiteSpace())
             {
                 result = TreasuryReport.getSECT(DEPT_ITEM, type, _CustodyFlag, branch);
             }
             else
             {
                 result = TreasuryReport.getDEPT(type, _CustodyFlag, dept);
             }

            if(_CustodyFlag)
               result.Insert(0, All);   
            else
                if(DEPT_ITEM != null && _Depts.Dpt_type != "04" && result.Count() < 1)
                result.Insert(0, All);

            return Json(result);
        }

    }
}