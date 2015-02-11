using ClosedXML.Excel;
using gTravel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace gTravel
{
    public class import_xls
    {
        private goDbEntities db = new goDbEntities();

        private string _error;
        private decimal _lognum;

        public string error_message { 
            get{ return _error;}
            private set { _error = value; } 
        }
        public decimal lognum
        {
            get { return _lognum; }
            private set { _lognum = value; }
        }
 
        private string UserId;
        private Guid SeriaId;

        public import_xls(string userid, Guid seriaid)
        {
            UserId = userid;
            SeriaId = seriaid;
        }
        public bool import(Stream file)
        {
            bool ret = true;

            try
            {
                var workbook = new XLWorkbook(file);

                var ws = workbook.Worksheet(1);

                import_data(ws.RowsUsed());
            }
            catch(Exception e)
            {
                error_message= "Ошибка при загрузки. Возможно файл некорректный. "+ e.Message;

                ret = false;
            }

            return ret; 
        }

        private void import_data(IXLRows rows)
        {
            Contract newcontract = new Contract(db);
            ImportLog l = new ImportLog(db, UserId);

            if(l.docnum.HasValue)
                this.lognum = l.docnum.Value;

            int iusedrow = 0;

            foreach (var row in rows)
            {
                iusedrow++;

                //пропустим шапку
                if (iusedrow == 1)
                    continue;

                var crow = readimportrow(row);
                
                //новый договор
                if (!newcontract.contractnumber.HasValue || !string.IsNullOrEmpty(crow.contract_number_str))
                {
                    newcontract = create_contract(crow);
                    l.add_log(db, newcontract.ContractId);

                    continue;
                }

                //застрахованный
                newcontract.add_insured( create_insured(crow));

            }
        }

        private Contract create_contract(cl_import_contract crow)
        {
            //создаем новый 
            Contract contract_new = new Contract(db);

            contract_new.seriaid = SeriaId;
            contract_new.contractnumber = contract_new.getnextnumber(UserId);
            contract_new.date_out = crow.date_out;
            contract_new.date_begin = crow.date_begin;
            contract_new.date_end = crow.date_end;

            contract_new.add_contract( UserId);
          
            //застрахованный
            contract_new.add_insured(create_insured(crow));

            return contract_new;
        }

        private Subject create_insured(cl_import_contract crow)
        {
            var s = new Subject();
            s.Name1 = crow.SubjName;
            s.Gender = mLib.gender_parse(crow.gender);
            s.DateOfBirth = crow.dateofbirth;
            s.Pasport = crow.pasport;
            s.PasportValidDate = crow.passportvaliddate;
            s.PlaceOfBirth = crow.placeofbirth;

            return s;
        }

        private int get_import_pos(string code)
        {
            code = code.Trim();

            return db.import_settings.SingleOrDefault(x => x.colcode.Trim() == code).numcol.Value;

        }

        private cl_import_contract readimportrow(IXLRow row)
        {
            cl_import_contract ret = new cl_import_contract();
            //1
            int inum = 0;

            ret.contract_number_str = row.Cell(get_import_pos("contract_number")).Value.ToString();
            if (!int.TryParse(ret.contract_number_str, out inum))
            {
                ret.contract_number = 0;
            }
            else
                ret.contract_number = inum;

            //2
            DateTime dt_out;
            if (DateTime.TryParse(row.Cell(get_import_pos("date_out")).Value.ToString(), out dt_out))
                ret.date_out = dt_out;

            //3
            if (DateTime.TryParse(row.Cell(get_import_pos("date_begin")).Value.ToString(), out dt_out))
                ret.date_begin = dt_out;

            //4
            if (DateTime.TryParse(row.Cell(get_import_pos("date_end")).Value.ToString(), out dt_out))
                ret.date_end = dt_out;

            if (DateTime.TryParse(row.Cell(get_import_pos("date_flyout")).Value.ToString(), out dt_out))
                ret.date_flyout = dt_out;

            ret.entry_in = row.Cell(get_import_pos("entry_in")).Value.ToString();
            ret.entry_out = row.Cell(get_import_pos("entry_out")).Value.ToString();


            ret.SubjName = row.Cell(get_import_pos("subjname")).Value.ToString();
            //6
            ret.gender = row.Cell(get_import_pos("gender")).Value.ToString();

            if (DateTime.TryParse(row.Cell(get_import_pos("dateofbirth")).Value.ToString(), out dt_out))
                ret.dateofbirth = dt_out;

            ret.placeofbirth = row.Cell(get_import_pos("placeofbirth")).Value.ToString();

            ret.pasport = row.Cell(get_import_pos("pasport")).Value.ToString();

            if (DateTime.TryParse(row.Cell(get_import_pos("passportvaliddate")).Value.ToString(), out dt_out))
                ret.passportvaliddate = dt_out;



            return ret;
        }
    }
}