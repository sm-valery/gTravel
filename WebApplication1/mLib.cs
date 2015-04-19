using gTravel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace gTravel.Models
{

    public partial class Contract
    {
       public goDbEntities db;

       public Contract(goDbEntities DbEntities)
       {
           

           this.db = DbEntities;
       }

        public int checkaccess( string userid)
        {

            if (!db.spContract(userid, null, null, this.ContractId,null).Any())
                return 0;
            
            return 1;
        }
        public decimal getnextnumber(string userid)
        {

            return getnextnumber(this.seriaid, userid);
        }
        public decimal getnextnumber( Guid seriaid,string userid)
        {
            decimal? newnum;

           // if (db.Contracts.Any(x=>x.seriaid==seriaid))
            //{
                var agentid = db.AgentUsers.FirstOrDefault(x=>x.UserId==userid).AgentId;

          

                newnum = (from c in db.Contracts
                          join au in db.AgentUsers on c.UserId equals au.UserId
                          where au.AgentId == agentid
                          && c.seriaid ==seriaid
                          select c).Max(x => x.contractnumber);

                //newnum = db.Contracts.Where(x => x.seriaid == seriaid && x.UserId != "86cf7814-6257-4057-b143-0f50d07dce7f").Max(s => s.contractnumber).Value;
            //}

                if (newnum == null)
                    newnum = 0;

            return newnum.Value + 1;
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

            db.Subjects.Add(s);
            #endregion

            #region contract
         
            this.ContractId = Guid.NewGuid();
            this.currencyid = (this.currencyid == Guid.Parse("{00000000-0000-0000-0000-000000000000}")) ? seria.DefaultCurrencyId.Value : this.currencyid;

   
            this.date_out = (this.date_out == null) ? DateTime.Now : this.date_out;
            this.date_diff = mLib.get_period_diff(date_begin, date_end);
            Holder_SubjectId = s.SubjectId;

            if (seria.AutoNumber == 1)
            {
                contractnumber = getnextnumber(seriaid,userid);
            }
               

            ContractStatusId = change_status(userid);

            UserId = userid;
            db.Contracts.Add(this);

        

            #endregion

            #region риски

            var cr = db.RiskSerias.Where(x => x.SeriaId == seriaid).OrderBy(o => o.sort);

            foreach (var item in cr)
            {
                ContractRisk item_rs = new ContractRisk();

                item_rs.ContractRiskId = Guid.NewGuid();
                item_rs.ContractId = ContractId;
                item_rs.RiskId = item.RiskId;

                item_rs.InsSum = item.InsSumDefauld;
                
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
        public static string gethash(string key)
        {
           var h= MD5.Create();

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