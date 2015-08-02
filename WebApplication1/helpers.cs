using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            StringBuilder sbody = new StringBuilder();

            sbody.AppendFormat("<input type=\"hidden\" name=\"ContractConditions[{0}].ContractCondId\" value=\"{1}\">", idx, cond.ContractCondId);
            sbody.AppendFormat("<input type=\"hidden\" name=\"ContractConditions[{0}].ConditionId\" value=\"{1}\">", idx, cond.ConditionId.Value);
            sbody.AppendFormat("<input type=\"hidden\" name=\"ContractConditions[{0}].Contractid\" value=\"{1}\">", idx, cond.Contractid);
            sbody.AppendFormat("<input type=\"hidden\" name=\"ContractConditions[{0}].num\" value=\"{1}\">", idx, cond.num);


            switch (cond.Condition.Type)
            {
                case "L":
                    sbody.AppendFormat("<div class=\"checkbox\"><label><input {1} type=\"checkbox\" name=\"ContractConditions[{0}].Val_c\"> {2}</label></div>",
                        idx,
                        (cond.Val_c=="on") ? "checked" : "",
                        cond.Condition.Name);
                    break;

                case "C":
                    
                    sbody.AppendFormat("<div class=\"row\">  <div class=\"col-md-2\">  <label>{2}:</label> </div> <div class=\"col-md-10\">" +
                        "<input type=\"text\" name=\"ContractConditions[{0}].Val_c\" value=\"{1}\" {3} class=\"form-control input-sm input-value\" >",
                        idx,
                        cond.Val_c,
                        (string.IsNullOrEmpty(cond.link_text)) ? cond.Condition.Name : string.Format("<a href='{0}'>{1}</a>", cond.link_text, cond.Condition.Name)
                        ,(cond.Val_id!=null)?"readonly":""
                        );

                    if(!string.IsNullOrEmpty( cond.link_text))
                    {
                        sbody.AppendFormat("&nbsp;<a href='{0}'  title='Перейти'><span class='glyphicon glyphicon-share'></span></a>", cond.link_text);
                    }

                    sbody.Append("</div> </div>");

                    break;

                case "S":
                    sbody.AppendFormat("<div class=\"row\"><div class=\"col-md-2\"><label>{2}:</label></div><div class=\"col-md-10\">" +
                        "<textarea name =\"ContractConditions[{0}].Val_c\" class=\"form-control input-sm input-value\" style = \"width:300px;\">{1}</textarea> </div>  </div>",
                        idx,
                        cond.Val_c,
                        cond.Condition.Name);


                    break;
            }

            return MvcHtmlString.Create(sbody.ToString());
        }
    }
}