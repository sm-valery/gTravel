using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using gTravel.Models;
using SelectPdf;
using System.Data.Entity;

namespace gTravel.Servises
{
    public class ContractService
    {
        public Contract create_contract(goDbEntities db, Guid seriaid, string userid)
        {

            var seria = db.serias.FirstOrDefault(x => x.SeriaId == seriaid);


            if (seria == null)
            {
                return null;
            }

            Contract c = new Contract(db)
            {
                seriaid = seriaid,
                currencyid = (Guid)seria.DefaultCurrencyId,
                date_out = DateTime.Now
            };


            return c.add_contract(userid) ? c : null;
        }

        public bool contract_access(goDbEntities db, Guid contractid, string userid)
        {
            return spContract(db, userid, contractid);

        }

        public bool spContract(goDbEntities db, string userid, Guid contractid)
        {

            return db.spContract(userid, null, null, contractid, null).Any();
        }


        public Contract GetContractForEdit(goDbEntities db, Guid contractid, string userid)
        {
            var c = db.Contracts.Include("Contract_territory")
              .Include("ContractConditions")
              .Include("Subjects")
              .Include("ContractStatu")
              .SingleOrDefault(x => x.ContractId == contractid);

            if (c != null)
            {
                c.db = db;

                c.CheckFactors(userid);

                c.Subjects = c.Subjects.OrderBy(x => x.num).ToList();

                c.ContractConditions = c.ContractConditions.OrderBy(o => o.num).ToList();

                return c;
            }

            return null;
        }

        public void AddNewBonusToContract(goDbEntities db, Guid factorid, Guid contractid)
        {
            var newfactor = db.Factors.SingleOrDefault(x => x.IdFactor == factorid);

            var needToAdd = true;


            if (newfactor.SingleItemInGroup)
            {
                var oldfact =
                    db.ContractFactors.FirstOrDefault(
                        x => x.Factor.FactorType == newfactor.FactorType && x.ContractId == contractid);
                if (oldfact != null)
                {
                    oldfact.IdFactor = newfactor.IdFactor;
                    oldfact.Val_n = newfactor.Factor1;

                    db.Entry(oldfact).State = EntityState.Modified;

                    db.SaveChanges();
                    needToAdd = false;
                }
            }

            if (needToAdd)
            {
                var num = db.ContractFactors.Count(x => x.ContractId == contractid);

                var cfnew = new ContractFactor();
                cfnew.ContractFactorId = Guid.NewGuid();
                cfnew.IdFactor = newfactor.IdFactor;
                cfnew.ContractId = contractid;
                cfnew.Val_n = newfactor.Factor1;
                cfnew.Position = num++;

                db.ContractFactors.Add(cfnew);

                db.SaveChanges();
            }

        }

        public List<Factor> BaseFactors(goDbEntities db, string userid, Guid seriaid)
        {
            return (from au in db.AgentUsers
                join ags in db.AgentSerias on au.AgentId equals ags.AgentId
                join f in db.Factors on ags.AgentSeriaId equals f.AgentSeriaId
                where au.UserId == userid 
                && ags.SeriaId == seriaid
                && !f.auto
                select f).OrderBy(o=>o.Position).ToList();
        }

        public virtual void OutputPdf(string htmlContent,string filename)
        {
            var converter = new HtmlToPdf();

            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.MarginBottom = 10;
            converter.Options.MarginLeft = 10;
            converter.Options.MarginRight = 10;
            converter.Options.MarginTop = 10;

            var doc = converter.ConvertHtmlString(htmlContent);

            // save pdf document
            var pdfBuf = doc.Save();


            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/pdf";
            HttpContext.Current.Response.AddHeader("content-disposition", string.Format("attachment;filename=\"{0}\"",filename));

            HttpContext.Current.Response.OutputStream.Write(pdfBuf, 0, pdfBuf.Count());

            HttpContext.Current.Response.End();

            // close pdf document
            doc.Close();
        }


        public void GetImportExampleFile(goDbEntities db)
        {
            var w = new XLWorkbook();
            var s = w.Worksheets.Add("Импорт");

            var impset = db.import_settings.OrderBy(x => x.numcol);
            var colcount = 0;


            foreach (var i in impset)
            {
                colcount++;
                s.Cell(1, i.numcol.Value).SetValue(i.colname);

                switch (i.colcode.Trim())
                {
                    case "contract_number":
                        s.Cell(2, i.numcol.Value).SetValue("1");
                        s.Cell(5, i.numcol.Value).SetValue("2");
                        break;
                    case "date_begin":
                        s.Cell(2, i.numcol.Value).SetValue(DateTime.Now);
                        s.Cell(5, i.numcol.Value).SetValue(DateTime.Now);
                        break;
                    case "date_end":
                        s.Cell(2, i.numcol.Value).SetValue(DateTime.Now.AddDays(10));
                        s.Cell(5, i.numcol.Value).SetValue(DateTime.Now.AddDays(20));
                        break;
                    case "date_out":
                        s.Cell(2, i.numcol.Value).SetValue(DateTime.Now);
                        s.Cell(5, i.numcol.Value).SetValue(DateTime.Now);
                        break;
                    case "subjname":
                        s.Cell(2, i.numcol.Value).SetValue("Застрахованный 1 дог1");
                        s.Cell(3, i.numcol.Value).SetValue("Застрахованный 2 дог1");
                        s.Cell(4, i.numcol.Value).SetValue("Застрахованный 3 дог1");
                        s.Cell(5, i.numcol.Value).SetValue("Застрахованный 1 дог2");
                        break;
                }
            }

            s.Columns().AdjustToContents();
            s.Range(1, 1, 1, colcount).Style.Font.SetBold();

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=\"import.xlsx\"");

            using (var memoryStream = new MemoryStream())
            {
                w.SaveAs(memoryStream);
                memoryStream.WriteTo(HttpContext.Current.Response.OutputStream);
                memoryStream.Close();
            }

            HttpContext.Current.Response.End();
        }


        private bool ContractRecalc(goDbEntities db, Contract c, List<string> ErrMess)
        {
            var ret = true;

            var date_diff = (decimal)c.date_diff;


            try
            {
                var cter = c.Contract_territory.FirstOrDefault();

                //TODO пока скидки надбавки по всему полису, далее переделать на порисковые скидки
                //скидки надбавки

                //идем по людям
                //валидация
                foreach (var s in c.Subjects)
                {
                    if (!s.DateOfBirth.HasValue)
                    {
                        ErrMess.Add(string.Format("У застрахованного {0} не заполнено поле дата рождения", s.Name1));
                        ret = false;
                    }
                }

                foreach (var crisk in c.ContractRisks)
                {
                    crisk.BaseTarif = 0;
                    crisk.InsPrem = 0;
                    crisk.InsFee = 0;
                    crisk.InsPremRur = 0;
                    crisk.FactorsDescr = "";

                    //найдем тариф
                    var t = (from ags in db.AgentUsers
                            join au in db.AgentSerias on ags.AgentId equals au.AgentId
                            join tf in db.Tarifs on au.AgentSeriaId equals tf.AgentSeriaId
                            where ags.UserId == c.UserId
                            && au.SeriaId == c.seriaid
                            && tf.RiskId ==crisk.RiskId
                            && tf.PeriodFrom >= date_diff
                            && tf.PeriodTo <= date_diff
                            select tf).FirstOrDefault();

                    //var t = (from tr in db.Tarifs
                    //         where tr.SeriaId == c.seriaid &&
                    //               tr.TerritoryId == cter.TerritoryId &&
                    //               tr.RiskId == crisk.RiskId
                    //         select tr).FirstOrDefault();


                    if (t != null && ret)
                    {
                        crisk.BaseTarif = (decimal)t.PremSum;
                        var riskprem = (decimal)crisk.BaseTarif * (decimal)c.date_diff;

                        var factor_descr = new List<factorgrp>();

                        riskprem = riskprem * getXtrimFactor(c.ContractConditions, c.seriaid, factor_descr);

                        crisk.InsPrem = CalcSubjectsPremium(c.Subjects, c.seriaid, riskprem, c.date_out.Value,
                            factor_descr);
                        crisk.InsFee = CalcSubjectsPremium(c.Subjects, c.seriaid,
                            (decimal)t.InsFee * (decimal)c.date_diff, c.date_out.Value);
                        crisk.InsPremRur = crisk.InsPrem * CurrManage.getCurRate(db, c.currencyid, c.date_out);

                        //TODO 27042015 добавить поле FactorsDescr в таблицу [ContractRisk]
                        var fgrp = from n in factor_descr
                                   group n by new { t = n.ftype, v = n.fvalue }
                                       into g
                                       select new factorgrp { ftype = g.Key.t, fvalue = g.Key.v, qnt = g.Count() };

                        var fdescr = new StringBuilder();

                        foreach (var f in fgrp)
                        {
                            fdescr.AppendFormat("{0}: {1}({2}); ", f.ftype, f.fvalue, f.qnt);
                            //crisk.FactorsDescr += string.Format("{0}: {1}({2}); ",f.ftype,f.fvalue,f.qnt);
                        }
                        crisk.FactorsDescr = fdescr.ToString();

                        //crisk.BaseTarif = (decimal)t.PremSum;
                        //dcount = (decimal)(c.date_diff * c.Subjects.Count());
                        //crisk.InsPrem = crisk.BaseTarif * dcount;
                        //crisk.InsFee = (decimal)t.InsFee * dcount;
                        //crisk.InsPremRur = crisk.InsPrem * CurrManage.getCurRate(db, c.currencyid, c.date_out);
                    }
                    else
                    {
                        ErrMess.Add("Тариф не найден! Обратитесь к администратору");

                        ret = false;
                    }
                }
            }
            catch
            {
                ErrMess.Add("Ошибка при расчете тарифа! Обратитесь к администратору");

                ret = false;
            }

            return ret;
        }
    }
}