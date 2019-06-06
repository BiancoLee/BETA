using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web.Models;
using Treasury.WebUtility;

namespace Treasury.Web.Daos
{
    public class TreaEquipDao
    {

        /// <summary>
        /// 取得金庫設備下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public SelectList loadSelectList()
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            //var result = context.TypeDefine.Where(x => x.CTYPE == cType).OrderBy(x => x.ISORTBY);

            var result1 = (from equip in context.TREA_EQUIP
                           join syscode in context.SYS_CODE
                           on equip.CONTROL_MODE equals syscode.CODE
                           where equip.IS_DISABLED == "N"
                           && syscode.CODE_TYPE == "CONTROL_MODE"
                           orderby equip.TREA_EQUIP_ID
                           select new
                           {
                               CCODE = equip.TREA_EQUIP_ID.Trim(),
                               CVALUE = equip.EQUIP_NAME.Trim() + "(" + syscode.CODE_VALUE + ")"
                           }
                           );

            var items = new SelectList
                (
                items: result1,
                dataValueField: "CCODE",
                dataTextField: "CVALUE",
                selectedValue: (object)null
                );

            return items;
        }

        /// <summary>
        /// 以鍵項查詢金庫設備
        /// </summary>
        /// <param name="equip"></param>
        /// <returns></returns>
        public TREA_EQUIP qryByKey(String equipId)
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            var result = context.TREA_EQUIP.Where(x => x.TREA_EQUIP_ID == equipId).FirstOrDefault();


            return result;
        }



        public string jqgridSelect()
        {
            string equipStr = "";
            var equipList = loadSelectList();
            foreach (var item in equipList)
            {
                equipStr += item.Value.Trim() + ":" + item.Text.Trim() + ";";
            }
            equipStr = equipStr.Substring(0, equipStr.Length - 1) + "";
            return equipStr;
        }

        public List<SelectOption> getEquipFun(string contrlMod)
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            var _equip = context.TREA_EQUIP.AsNoTracking()
                .Where(x => x.IS_DISABLED == "N")
                .Where(x => x.CONTROL_MODE == contrlMod, !contrlMod.IsNullOrWhiteSpace())
                .AsEnumerable()
                .Join(context.SYS_CODE.AsNoTracking().Where(x=>x.CODE_TYPE == "CONTROL_MODE"),
                  x => x.CONTROL_MODE,
                  y => y.CODE,
                  (x,y) => new {x,y}
                )
                .Select(x => new SelectOption()
                {
                    Value = x.x.TREA_EQUIP_ID,
                    Text = $@"{x.x.EQUIP_NAME}({x.y.CODE_VALUE})" 
                }).ToList(); 
            return _equip;
        }
    }
}