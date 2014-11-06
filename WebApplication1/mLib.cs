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

      
    }
}