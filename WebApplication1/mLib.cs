using gTravel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace gTravel.Models
{
    public partial class Contract
    {
        public int checkaccess(goDbEntities db, string userid)
        {

            if (!db.spContract(userid, null, null, this.ContractId).Any())
                return 0;
            
            return 1;
        }
        public decimal getnextnumber(goDbEntities db)
        {

            return getnextnumber(db, this.seriaid);
        }
        public decimal getnextnumber(goDbEntities db, Guid seriaid)
        {
            decimal newnum = 0;

            newnum = db.Contracts.Where(x => x.seriaid == seriaid).Max(s=>s.contractnumber).Value;

            return newnum+1;
        }

        public bool add_contract(goDbEntities db, string userid)
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

            db.Subjects.Add(s);
            #endregion

            Guid contr_stat_id = Guid.NewGuid();

            #region contract
            //Contract c = new Contract();

            this.ContractId = Guid.NewGuid();
            this.currencyid = (this.currencyid == Guid.Parse("{00000000-0000-0000-0000-000000000000}")) ? seria.DefaultCurrencyId.Value : this.currencyid;

           // c.date_begin = c.date_begin;
            //c.date_end = c.date_end;
            this.date_out = (this.date_out == null) ? DateTime.Now : this.date_out;
            this.date_diff = mLib.get_period_diff(date_begin, date_end);
            Holder_SubjectId = s.SubjectId;

            if (seria.AutoNumber == 1)
                contractnumber = getnextnumber(db, seriaid);

            ContractStatusId = contr_stat_id;

            UserId = userid;
            db.Contracts.Add(this);

            ContractStatu stat = new ContractStatu();
            stat.ContractStatusId = contr_stat_id;
            stat.ContractId = ContractId;
            stat.StatusId = db.Status.SingleOrDefault(x => x.Code.Trim() == "project").StatusId;
            stat.DateInsert = DateTime.Now;
            stat.UserId = userid;

            db.ContractStatus.Add(stat);

            #endregion

            #region риски

            var cr = db.RiskSerias.Where(x => x.SeriaId == seriaid).OrderBy(o => o.sort);

            foreach (var item in cr)
            {
                ContractRisk item_rs = new ContractRisk();

                item_rs.ContractRiskId = Guid.NewGuid();
                item_rs.ContractId = ContractId;
                item_rs.RiskId = item.RiskId;
                //item_rs.Risk = item.Risk;

                ContractRisks.Add(item_rs);
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

            db.SaveChanges();

            return true;
        }

        public Subject add_insured(goDbEntities db)
        {
            Subject s = new Subject();

            
            s.ContractId = this.ContractId;

           return add_insured(db, s);

        }

        public Subject add_insured(goDbEntities db,Subject s)
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

        public ImportLog(goDbEntities db,  string UserId)
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
            if(!db.ImportLogContracts.Any(x=>x.ImportLogId==this.ImportLogId && x.ContractId==contract_id))
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
        public static SelectList GenderList(string gender = "N")
        {
            return new SelectList(new[]{
                                                new SelectListItem(){Text="Мужской", Value="M"},
                                                new SelectListItem(){Text="Женский",Value="F"},
                                                new SelectListItem(){Text="Неизвестно",Value="N"}
                                            }, "Value", "Text",gender);
        }


        public static int get_period_diff(DateTime? d1, DateTime? d2)
        {
            if (d1.HasValue && d2.HasValue)
                return (d2.Value - d1.Value).Days + 1;

            return 0;
        }
        public static string gender_parse(string gender_code)
        {
            string ret = "N";

            

            switch(gender_code.Trim())
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