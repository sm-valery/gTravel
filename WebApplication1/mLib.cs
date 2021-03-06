﻿using gTravel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
//using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;



namespace gTravel.Models
{

    //public static class ContractManager
    //{
    //    public static int checkaccess(goDbEntities db, Guid ContractId, string userid)
    //    {

    //        if (!db.spContract(userid, null, null, ContractId, null).Any())
    //            return 0;

    //        return 1;
    //    }

    //    public static decimal getnextnumber(goDbEntities db, Contract c, string userid)
    //    {

    //        return getnextnumber(db,c.seriaid, userid);
    //    }
    //    public static decimal getnextnumber(goDbEntities db, Guid seriaid, string userid)
    //    {
    //        decimal? newnum;

    //        var agentid = db.AgentUsers.FirstOrDefault(x => x.UserId == userid).AgentId;

    //        newnum = (from c in db.Contracts
    //                  join au in db.AgentUsers on c.UserId equals au.UserId
    //                  where au.AgentId == agentid
    //                  && c.seriaid == seriaid
    //                  select c).Max(x => x.contractnumber);


    //        if (newnum == null)
    //            newnum = 0;

    //        return newnum.Value + 1;
    //    }


    //    public static Guid change_status(goDbEntities db, Guid ContractId, string userid, string new_status_code = "project")
    //    {

    //        ContractStatu stat = new ContractStatu();
    //        stat.ContractStatusId = Guid.NewGuid();
    //        stat.ContractId = ContractId;

    //        stat.StatusId = db.Status.SingleOrDefault(x => x.Code.Trim() == new_status_code).StatusId;
    //        stat.DateInsert = DateTime.Now;
    //        stat.UserId = userid;


    //        db.ContractStatus.Add(stat);

    //        return stat.ContractStatusId;
    //    }

    //    public void SubjectClearDeleted()
    //    {
    //        var deleted = this.Subjects.Where(x => x.num == -1).ToList();

    //        foreach (var s in deleted)
    //        {
    //            this.Subjects.Remove(s);
    //        }

    //    }
    //}

    public partial class Contract
    {
        public goDbEntities db;

        public bool IsBaseFactorInRisk { get; set; }


        public Contract(goDbEntities dbEntities)
        {

            this.db = dbEntities;
        }

        ~Contract()
        {
            if (db != null)
                this.db.Dispose();
        }

        public void CheckFactors(string userid)
        {
            IsBaseFactorInRisk = (from agu in db.AgentUsers
                                  join ags in db.AgentSerias on agu.AgentId equals ags.AgentId
                                  join f in db.Factors on ags.AgentSeriaId equals f.AgentSeriaId
                                  where agu.UserId == userid
                                  && f.RiskId != null
                                  select agu).Any();

        }


        //public int checkaccess( string userid)
        //{

        //    if (!db.spContract(userid, null, null, this.ContractId,null).Any())
        //        return 0;

        //    return 1;
        //}
        public decimal getnextnumber(string userid)
        {

            return getnextnumber(this.seriaid, userid);
        }
        public decimal getnextnumber(Guid seriaid, string userid)
        {
            decimal? newnum;

            // if (db.Contracts.Any(x=>x.seriaid==seriaid))
            //{
            var agentid = db.AgentUsers.FirstOrDefault(x => x.UserId == userid).AgentId;



            newnum = (from c in db.Contracts
                      join au in db.AgentUsers on c.UserId equals au.UserId
                      where au.AgentId == agentid
                      && c.seriaid == seriaid
                      select c).Max(x => x.contractnumber);

            //newnum = db.Contracts.Where(x => x.seriaid == seriaid && x.UserId != "86cf7814-6257-4057-b143-0f50d07dce7f").Max(s => s.contractnumber).Value;
            //}

            if (newnum == null)
                newnum = 0;

            return newnum.Value + 1;
        }


        public void add_agent()
        {
                var caguser = new ContractAgent();

                caguser.ContractAgentId = Guid.NewGuid();
                caguser.ContractId = this.ContractId;
                caguser.num = 1;
 
                caguser.AgentId = mLib.GetCurrentUserAgent(this.UserId).AgentId;

                var ags = db.AgentSerias.SingleOrDefault(x => x.AgentId == caguser.AgentId && x.SeriaId == this.seriaid);
            if(ags!=null)
                caguser.Percent = ags.AgentFee;

            decimal ContractPrem = (decimal)this.ContractRisks.Sum(x=>x.InsPrem);
            decimal ContractPremRur = (decimal)this.ContractRisks.Sum(x=>x.InsPremRur);


            caguser.InsPrem = mLib.calcAgentCommision(ContractPrem, caguser.Percent);
            caguser.InsPremRur = mLib.calcAgentCommision(ContractPremRur, caguser.Percent);
         

                db.ContractAgents.Add(caguser);

        }

        public Guid change_status(string userid, string new_status_code = "project")
        {

            ContractStatu stat = new ContractStatu();
            stat.ContractStatusId = Guid.NewGuid();
            stat.ContractId = this.ContractId;

            stat.StatusId = db.Status.SingleOrDefault(x => x.Code.Trim() == new_status_code).StatusId;

            stat.DateInsert = DateTime.Now;
            stat.UserId = userid;


            db.ContractStatus.Add(stat);

        
            return stat.ContractStatusId;
        }

        public void save()
        {
            db.Entry(this).State = EntityState.Modified;
            db.SaveChanges();
        }

        //очистить застрахованных от удаленных
        public void SubjectClearDeleted()
        {
            var deleted = this.Subjects.Where(x => x.num == -1).ToList();

            foreach (var s in deleted)
            {
                this.Subjects.Remove(s);
            }

        }



        public bool add_contract(string userid)
        {
            var seria = db.serias.FirstOrDefault(x => x.SeriaId == seriaid);

            if (seria == null)
            {
                return false;
            }

            #region Страхователь

            Subject s = new Subject();

            s.SubjectId = Guid.NewGuid();
            s.Type = "fiz";

            this.Holder_SubjectId = s.SubjectId;

            db.Subjects.Add(s);

            #endregion

            #region contract

            this.ContractId = Guid.NewGuid();
            this.currencyid = (this.currencyid == Guid.Parse("{00000000-0000-0000-0000-000000000000}")) ? seria.DefaultCurrencyId.Value : this.currencyid;


            this.date_out = (this.date_out == null) ? DateTime.Now : this.date_out;
            this.date_diff = mLib.get_period_diff(date_begin, date_end);


            if (seria.AutoNumber == 1)
            {
                contractnumber = getnextnumber(seriaid, userid);
            }


            ContractStatusId = change_status(userid);

            UserId = userid;
            db.Contracts.Add(this);



            #endregion

            #region риски

            var cr = db.RiskSerias.Where(x => x.SeriaId == seriaid).OrderBy(o => o.sort).ToList();
            
            //RiskProgram firstprog = new RiskProgram();

            foreach (var item in cr)
            {
                //
                var rp = db.RiskPrograms.OrderBy(o=>o.Num).FirstOrDefault(x => x.RiskSeriaId == item.RiskSeriaId);

                ContractRisk item_rs = new ContractRisk();

                item_rs.ContractRiskId = Guid.NewGuid();
                item_rs.ContractId = ContractId;
                item_rs.RiskId = item.RiskId;

                item_rs.InsSum = item.InsSumDefauld;

                //TODO 27042015 добавить поле sort в  ContractRisk, изменить view: v_contract_risk
                item_rs.sort = item.sort;
                item_rs.isMandatory = item.isMandatory;
                item_rs.ischecked = item.isMandatory;


                if (rp != null)
                {
                    item_rs.RiskProgramId = rp.RiskProgramId;
                    item_rs.InsSum = rp.DefaultInsSum;
                }

                rp = null;
              

                db.ContractRisks.Add(item_rs);
            }

            #endregion

            #region доп параметры
            var cs = db.ConditionSerias.Include("Condition").Where(x => x.SeriaId == seriaid).OrderBy(o => o.num);

            foreach (var item in cs)
            {
                ContractCondition cc = new ContractCondition();
                cc.ContractCondId = Guid.NewGuid();
                cc.ConditionId = item.ConditionId;
                cc.Contractid = ContractId;
                //cc.Condition = item.Condition;
                cc.num = item.num;

                switch (item.Condition.Type)
                {
                    case "L":
                        cc.Val_l = false;
                        break;
                }


                db.ContractConditions.Add(cc);

            }
            #endregion
            ////TODO территория по умолчанию
            #region территория по умолчанию
            if (seria.DefaultTerritoryId != null)
            {
                Contract_territory crt = new Contract_territory();
                crt.ContractTerritoryId = Guid.NewGuid();
                crt.ContractId = ContractId;
                crt.TerritoryId = seria.DefaultTerritoryId;
                //crt.TerritoryId = Guid.Parse("cf6d6985-0bd1-4f71-9b78-91ae11505988");

                db.Contract_territory.Add(crt);

            }
            #endregion

            #region СкидкиНадбавки
            #endregion

            db.SaveChanges();

            return true;
        }


        public Subject add_insured(goDbEntities DbEntities = null)
        {
            if (db == null)
                db = DbEntities;

            Subject s = new Subject();

            s.ContractId = this.ContractId;


            return add_insured(s);

        }

        public Subject add_insured(Subject s)
        {
            s.SubjectId = Guid.NewGuid();

            if (s.ContractId == null)
                s.ContractId = this.ContractId;

            s.num = db.Subjects.Where(x => x.ContractId == s.ContractId).Count() + 1;

            db.Subjects.Add(s);
            db.SaveChanges();

            return s;
        }

    }


    public partial class ImportLog
    {

        public ImportLog()
        {

        }

        public ImportLog(goDbEntities db, string UserId)
        {
            this.ImportLogId = Guid.NewGuid();
            this.userid = UserId;
            this.dateinsert = DateTime.Now;
            this.docnum = db.ImportLogs.Count(x => x.userid == UserId);

            db.ImportLogs.Add(this);
            db.SaveChanges();
        }


        public void add_log(goDbEntities db, Guid contract_id)
        {
            if (!db.ImportLogContracts.Any(x => x.ImportLogId == this.ImportLogId && x.ContractId == contract_id))
            {
                ImportLogContract l = new ImportLogContract();

                l.ImportLogContract1 = Guid.NewGuid();
                l.ImportLogId = this.ImportLogId;
                l.ContractId = contract_id;

                db.ImportLogContracts.Add(l);
                db.SaveChanges();
            }
        }
    }
}

namespace gTravel
{


    static class mLib
    {
        public static decimal calcAgentCommision(decimal moneyvalue, decimal? percent)
        {
            return (percent.HasValue)? Math.Round(percent.Value * moneyvalue/100,2,MidpointRounding.AwayFromZero):0;
        }

        public static void NoAjaxCache()
        {
            HttpContext.Current.Response.CacheControl = "no-cache";
            //HttpContext.Current.Response.Cache.SetETag((Guid.NewGuid()).ToString());
            // HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");
            HttpContext.Current.Response.AddHeader("Pragma", "no-cache");
            HttpContext.Current.Response.Expires = -1;
          
        }

        public static int GetAge(DateTime birthDate, DateTime date)
        {
            var years = date.Year - birthDate.Year;
            if (date.Month < birthDate.Month || (date.Month == birthDate.Month && date.Day < birthDate.Day))
                years--;
            return years;
        }

        public static bool IsSetCurrentUserAgent(string userid)
        {
            return HttpContext.Current.Session["useragentid"] != null;
        }

        public static void SetCurrentUserAgent(string userid)
        {
            if (IsSetCurrentUserAgent(userid)) return;

            goDbEntities db = new goDbEntities();

            var singleOrDefault = db.AgentUsers.SingleOrDefault(x => x.UserId == userid);

            if (singleOrDefault != null)
            {
                HttpContext.Current.Session["useragentid"] = singleOrDefault.AgentId;
                HttpContext.Current.Session["useragentname"] = singleOrDefault.Agent.Name;
            }

            HttpContext.Current.Session["AgentRoles"] = db.AgentRoles.Where(x => x.AgentId == singleOrDefault.AgentId).ToList();


        }


        public static Agent GetCurrentUserAgent(string userid)
        {
            if (IsSetCurrentUserAgent(userid))
                return new Agent() { AgentId = Guid.Parse(HttpContext.Current.Session["useragentid"].ToString()), 
                    Name = HttpContext.Current.Session["useragentname"].ToString() }; 
            //(Agent)HttpContext.Current.Session["useragent"];

            SetCurrentUserAgent(userid);

            return GetCurrentUserAgent(userid);
        }

        [UserIdFilter]
        public static bool AgentInRole(string userid, string role)
        {
            var ag = GetCurrentUserAgent(userid);

            var aroles = (List<AgentRole>)HttpContext.Current.Session["AgentRoles"];

            if (aroles.SingleOrDefault(x => x.RoleCode.Trim() == role) != null)
                return true;

            return false;
        }

        public static string gethash(string key)
        {
            var h = MD5.Create();

            byte[] data = h.ComputeHash(Encoding.Default.GetBytes(key));

            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();

        }
        public static SelectList GenderList(string gender = "N")
        {
            return new SelectList(new[]{
                                                new SelectListItem(){Text="Мужской", Value="M"},
                                                new SelectListItem(){Text="Женский",Value="F"},
                                                new SelectListItem(){Text="Неизвестно",Value="N"}
                                            }, "Value", "Text", gender);
        }


        public static int get_period_diff(DateTime? d1, DateTime? d2)
        {
            int diff = 0;

            if (d1.HasValue && d2.HasValue)
                diff = (d2.Value - d1.Value).Days + 1;

            return (diff > 0) ? diff : 0;
        }

        public static int get_age(DateTime dateBirthDay, DateTime dateNow)
        {

            int year = dateNow.Year - dateBirthDay.Year;
            if (dateNow.Month < dateBirthDay.Month ||
                (dateNow.Month == dateBirthDay.Month && dateNow.Day < dateBirthDay.Day)) year--;

            return year;
        }

        
        public static string gender_parse(string gender_code)
        {
            string ret = "N";



            switch (gender_code.Trim())
            {
                case "1":
                    ret = "M";
                    break;
                case "2":
                    ret = "F";
                    break;

            }
            return ret;

        }



    }
}