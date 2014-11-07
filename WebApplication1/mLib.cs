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