using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace gTravel
{
    public class helpers
    {

        public static MvcHtmlString ContractStatus(gTravel.Models.Status s)
        {
            string sbody = "";

            switch (s.Code.Trim())
            {
                case "project":
                    sbody += string.Format("<span class=\"label label-default\">Статус: {0} </span>", s.Name);
                    break;
                case "confirmed":
                    sbody += string.Format("<span class=\"label label-primary\">Статус: {0} </span>", s.Name);

                    break;
                case "bordero":
                    sbody += string.Format("<span class=\"label label-success\">Статус: {0} </span>", s.Name);

                    break;

            }
            return MvcHtmlString.Create(sbody);
        }

        public static MvcHtmlString BuildCond(int idx, gTravel.Models.ContractCondition cond)
        {
             string sbody = "";

            sbody += string.Format("<input type=\"hidden\" name=\"ContractConditions[{0}].ContractCondId\" value=\"{1}\">",idx,cond.ContractCondId);
            sbody += string.Format("<input type=\"hidden\" name=\"ContractConditions[{0}].ConditionId\" value=\"{1}\">",idx,cond.ConditionId.Value);
            sbody += string.Format("<input type=\"hidden\" name=\"ContractConditions[{0}].Contractid\" value=\"{1}\">",idx,cond.Contractid);
            sbody += string.Format("<input type=\"hidden\" name=\"ContractConditions[{0}].num\" value=\"{1}\">",idx,cond.num);


             switch (cond.Condition.Type)
    {
        case "L":
                      sbody += string.Format("<div class=\"checkbox\"><label><input {1} type=\"checkbox\" name=\"ContractConditions[{0}].Val_l\"> {2}</label></div>",
                          idx,
                          (cond.Val_l.Value)?"checked":"",
                          cond.Condition.Name);
            break;
    }

            return MvcHtmlString.Create(sbody);
        }
    }
}