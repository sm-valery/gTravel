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
    public class ContractService : IDisposable
    {
        private goDbEntities db = new goDbEntities();


        public ContractService()
        {

        }

        public ContractService(goDbEntities db)
        {
            // TODO: Complete member initialization
            this.db = db;
        }



        //~ContractService()
        //{
        //    db.Dispose();
        //}

        public Contract create_contract(Guid seriaid, string userid)
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

        public bool contract_access(Guid contractid, string userid)
        {
            return spContract(userid, contractid);

        }

        public bool spContract(string userid, Guid contractid)
        {

            return db.spContract(userid, null, null, contractid, null).Any();
        }


        public Contract GetContractForEdit(Guid contractid, string userid)
        {
            var c = db.Contracts.Include("Contract_territory")
              .Include("ContractConditions")
                // .Include("Subjects")
              .Include("ContractStatu")
              .SingleOrDefault(x => x.ContractId == contractid);

            if (c != null)
            {
                c.db = db;

                c.CheckFactors(userid);

                c.Subjects = c.Subjects.OrderBy(x => x.num).ToList();

                c.ContractConditions = c.ContractConditions.OrderBy(o => o.num).ToList();

                foreach (var rr in c.ContractRisks)
                {
                    rr.Risk = db.Risks.SingleOrDefault(x => x.RiskId == rr.RiskId);
                }

                //  c.ContractStatu = db.ContractStatus.SingleOrDefault(x => x.ContractStatusId == c.ContractStatusId);

                return c;
            }

            return null;
        }

        public void AddNewBonusToContract(Guid factorid, Guid contractid)
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

        public List<Factor> BaseFactors(string userid, Guid seriaid)
        {
            return (from au in db.AgentUsers
                    join ags in db.AgentSerias on au.AgentId equals ags.AgentId
                    join f in db.Factors on ags.AgentSeriaId equals f.AgentSeriaId
                    where au.UserId == userid
                    && ags.SeriaId == seriaid
                    && !f.auto
                    select f).OrderBy(o => o.Position).ToList();
        }

        public RiskToPrint[] get_diver_risksum(int? day, string prog, string curcode)
        {
            RiskToPrint[] r = new RiskToPrint[3];

            r[0] = new RiskToPrint()
            {
                risk_name = "Утрата снаряжения в результате несчастного случая, произошедшего под водой",
                inssum = "-"
            };

            if (prog.Trim() == "G" && day >= 365)
            {
                r[0].inssum = "1000 " + curcode;
            }
            if (prog.Trim() == "G+" && day >= 365)
            {
                r[0].inssum = "1000 " + curcode;
            }
            if (prog.Trim() == "P" && day >= 365)
            {
                r[0].inssum = "1800 " + curcode;
            }
            if (prog.Trim() == "P+" && day >= 365)
            {
                r[0].inssum = "1800 " + curcode;
            }
            if (prog.Trim() == "I")
            {
                r[0].inssum = "1800 " + curcode;
            }

            r[1] = new RiskToPrint() { risk_name = "Инвалидность I группы/Смерть", inssum = "-" };

            if (prog.Trim() == "G" && day >= 365)
            {
                r[1].inssum = "6000 " + curcode;
            }
            if (prog.Trim() == "G+" && day >= 365)
            {
                r[1].inssum = "6000 " + curcode;
            }
            if (prog.Trim() == "S" && day >= 365)
            {
                r[1].inssum = "5000 " + curcode;
            }
            if (prog.Trim() == "P" && day >= 365)
            {
                r[1].inssum = "6500 " + curcode;
            }
            if (prog.Trim() == "P+" && day >= 365)
            {
                r[1].inssum = "6500 " + curcode;
            }

            if (prog.Trim() == "I")
            {
                r[1].inssum = "10000 " + curcode;
            }


            r[2] = new RiskToPrint() {risk_name = "Инвалидность II, III группы", inssum = "-"};

            if (prog.Trim() == "G" && day >= 365)
            {
                r[2].inssum = "2000 " + curcode;
            }
            if (prog.Trim() == "G+" && day >= 365)
            {
                r[2].inssum = "2000 " + curcode;
            }
            if (prog.Trim() == "S" && day >= 365)
            {
                r[2].inssum = "1500 " + curcode;
            }
            if (prog.Trim() == "P" && day >= 365)
            {
                r[2].inssum = "3000 " + curcode;
            }
            if (prog.Trim() == "P+" && day >= 365)
            {
                r[2].inssum = "3000 " + curcode;
            }

            if (prog.Trim() == "I")
            {
                r[2].inssum = "5000 " + curcode;
            }

            return r;
        }

        public virtual void OutputPdf(string htmlContent, string filename)
        {
            var converter = new HtmlToPdf();

            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            converter.Options.WebPageWidth = 1000;

            converter.Options.MarginBottom = 20;
            converter.Options.MarginLeft = 20;
            converter.Options.MarginRight = 20;
            converter.Options.MarginTop = 20;

            var doc = converter.ConvertHtmlString(htmlContent);

            // save pdf document
            var pdfBuf = doc.Save();


            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/pdf";
            HttpContext.Current.Response.AddHeader("content-disposition", string.Format("attachment;filename=\"{0}\"", filename));

            HttpContext.Current.Response.OutputStream.Write(pdfBuf, 0, pdfBuf.Count());

            HttpContext.Current.Response.End();

            // close pdf document
            doc.Close();
        }


        public void GetImportExampleFile()
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

        //private decimal CalcSubjecsPrem(Contract c, decimal sumprem)
        //{
        //    decimal subj_sum = 0;

        //    foreach (var s in c.Subjects)
        //    {
        //        if (s.num == -1)
        //            continue;

        //        decimal age = mLib.GetAge(s.DateOfBirth.Value, (DateTime)c.date_out);


        //    }

        //    return subj_sum;
        //}

        //    private decimal CalcSubjectsPremiumX(IEnumerable<Subject> subjects,
        //Guid seriaid,
        //decimal sum,
        //DateTime curdate,
        //List<factorgrp> factor_descr)
        //    {
        //        decimal subj_sum = 0;

        //        foreach (var s in subjects)
        //        {
        //            if (s.num == -1)
        //                continue;

        //            decimal age = GetAge(s.DateOfBirth.Value, curdate);

        //            var fs = db.Factors.FirstOrDefault(x => x.SeriaId == seriaid
        //                                                    && x.ValueFrom <= age && x.ValueTo >= age
        //                                                    && x.FactorType == "age");

        //            if (fs == null)
        //            {
        //                subj_sum += sum;
        //            }
        //            else
        //            {
        //                subj_sum += sum * fs.Factor1.Value;
        //                factor_descr.Add(new factorgrp { ftype = "возраст", fvalue = fs.Factor1.Value.ToString() });
        //            }
        //        }


        //        return subj_sum;
        //    }

        private void ContractFactorsFillAutoFactors(Contract c, Guid agentseriaid)
        {
            var fs = db.Factors.Where(x => x.AgentSeriaId == agentseriaid && x.auto);
            foreach (var x in fs)
            {
                switch (x.FactorType.Trim().ToLower())
                {
                    case "age":
                        ContractFactorsFillAge(c, x);
                        break;
                    case "territory":
                        ContractFactorFillTerrit(c, x);
                        break;
                }
            }

            db.SaveChanges();
        }

        private void ContractFactorsFillAge(Contract c, Factor f)
        {
            //bool issave = false;
            //var slist = db.Subjects.Where(x => x.ContractId == c.ContractId);
            foreach (var s in c.Subjects)
            {
                int cage = mLib.GetAge(s.DateOfBirth.Value, c.date_out.Value);
                if (cage >= f.ValueFrom && cage <= f.ValueTo)
                {
                    ContractFactorsAdd(c, f);
                }
            }

            //if (issave)

        }

        private void ContractFactorFillTerrit(Contract c, Factor f)
        {
            foreach (var t in c.Contract_territory)
            {
                if (t.TerritoryId == f.TerritoryId)
                {
                    ContractFactorsAdd(c, f);
                }
            }
        }

        private void ContractFactorsAdd(Contract c, Factor f)
        {
            db.ContractFactors.Add(new ContractFactor()
            {
                ContractFactorId = Guid.NewGuid(),
                IdFactor = f.IdFactor,
                ContractId = c.ContractId,
                Val_n = f.Factor1
            });
        }

        private void ContractFactorsDeleteAutoFactors(Contract c, Guid agentseriaid)
        {
            //var ff = from cf in db.ContractFactors
            //         join fct in db.Factors on cf.IdFactor equals fct.IdFactor
            //         where cf.ContractId == c.ContractId
            //         && fct.auto
            //         select cf;

            db.ContractFactors.RemoveRange(db.ContractFactors.Where(x => x.ContractId == c.ContractId && x.Factor.auto));
            db.SaveChanges();
        }


        private decimal calcprem(decimal basetarif, IEnumerable<v_contract_factors> vContractFactors, int subj_count, decimal daycount = 0, bool ismulty = false, int risk_seria_type_tarif = 0, decimal inssum = 0)
        {
            decimal riskprem = 0;
            decimal riskpremdatediff = 0;

            riskpremdatediff = basetarif;

            //премия это сумма за день умноженная на кол-во дней
            if (!ismulty && risk_seria_type_tarif == 0)
                riskpremdatediff = basetarif * daycount;

            //премия это процент от страх суммы
            if (risk_seria_type_tarif == 2)
            {
                riskpremdatediff = basetarif * inssum;
            }

            int agefactorscount = vContractFactors.Count(x => x.FactorType.Trim() == "age");

            decimal zfactor = vContractFactors.Where(x => x.FactorType.Trim() != "age").Aggregate((decimal)1.0, (sa, b) => sa * b.Val_n.Value);

            riskprem += zfactor * (subj_count - agefactorscount) * riskpremdatediff;

            foreach (var f in vContractFactors.Where(x => x.FactorType.Trim() == "age"))
            {
                riskprem += zfactor * (decimal)f.Val_n * riskpremdatediff;
            }



            return riskprem;
        }

        public bool riskhasfranchise(Guid seriaid, Guid riskid)
        {

            return db.RiskSerias.Any(x => x.SeriaId == seriaid && x.RiskId == riskid && x.hasFranchise > 0);
        }

        public bool ContractRecalc(Contract c, List<string> ErrMess)
        {
            var ret = true;

            bool ismulty = int.Parse(c.period_multi_type) > 0;

            var date_diff = (ismulty) ? (decimal)c.date_diff : (decimal)c.tripduration;

            if (date_diff == 0)
            {
                ErrMess.Add("Не задан период, или не указан срок поездки");

                return false;
            }


            var cter = c.Contract_territory.FirstOrDefault();

            //TODO пока скидки надбавки по всему полису, далее переделать на порисковые скидки
            //скидки надбавки

            //идем по людям
            //валидация
            var wrongsubj = c.Subjects.Where(x => x.DateOfBirth.HasValue == false);

            foreach (var s in wrongsubj)
            {
                ErrMess.Add(string.Format("У застрахованного {0} не заполнено поле 'дата рождения'", s.Name1));

                ret = false;
            }

            if (!ret)
            {
                return false;

            }



            var seriaagentid = db.getAgentSeriaId(c.UserId, c.seriaid).SingleOrDefault().Value;


            //Удалить автоскидки
            ContractFactorsDeleteAutoFactors(c, seriaagentid);
            //db.ContractFactors.RemoveRange(c.ContractFactors.Where(x => x.Factor.auto));


            //Заполним автоскидки
            ContractFactorsFillAutoFactors(c, seriaagentid);

            var vContractF = db.v_contract_factors.Where(x => x.ContractId == c.ContractId);

            decimal currate = CurrManage.getCurRate(db, c.currencyid, c.date_out);

            try
            {


                foreach (var crisk in c.ContractRisks)
                {

                    crisk.BaseTarif = 0;
                    crisk.InsPrem = 0;
                    crisk.InsFee = 0;
                    crisk.InsPremRur = 0;
                    crisk.FactorsDescr = "";

                    if (!crisk.isMandatory && !crisk.ischecked)
                    {

                        continue;
                    }


                    //найдем тариф
                    var tt = db.Tarifs.Where(
                        x => x.AgentSeriaId == seriaagentid
                            && x.RiskProgramId == crisk.RiskProgramId
                            && date_diff >= x.PeriodFrom
                            && date_diff <= x.PeriodTo
                            && crisk.InsSum >= x.InsSumFrom
                            && crisk.InsSum <= x.InsSumTo);

                    var risk_seria_type_tarif = db.RiskSerias.Where(x => x.RiskId == crisk.RiskId && x.SeriaId == c.seriaid).SingleOrDefault().TypeTarif;

                    // var t = new Tarif();



                    //многократное
                    if (ismulty)
                    {
                        tt = tt.Where(x => x.RepeatedType == c.period_multi_type
                            && x.RepeatedDays == c.tripduration);

                    }


                    //else
                    //{
                    //    t = tt.FirstOrDefault();
                    //}

                    //франшиза
                    if (riskhasfranchise(c.seriaid, (Guid)crisk.RiskId))
                    {
                        tt = tt.Where(x => x.FranshPerc == ((crisk.FranshPerc.HasValue) ? crisk.FranshPerc : 0));
                    }

                    var t = tt.FirstOrDefault();

                    if (t != null && ret)
                    {



                        crisk.BaseTarif = (decimal)t.PremSum;

                        crisk.AgentTarif = (crisk.AgentTarif.HasValue && crisk.AgentTarif != 0) ? (decimal)crisk.AgentTarif : crisk.BaseTarif;

                        crisk.InsPrem = calcprem((decimal)crisk.AgentTarif, vContractF, c.Subjects.Count(), (decimal)c.date_diff, ismulty, (int)risk_seria_type_tarif, (decimal)crisk.InsSum);
                        crisk.InsFee = calcprem((decimal)t.InsFee, vContractF, c.Subjects.Count(), (decimal)c.date_diff, ismulty, (int)risk_seria_type_tarif, (decimal)crisk.InsSum);


                        crisk.InsPremRur = crisk.InsPrem * currate;
                        crisk.InsFeeRur = crisk.InsFee * currate;


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

            if (ret)
                db.SaveChanges();

            return ret;
        }

        public ContractAgent Create_ContractAgent(Guid contractid)
        {

            ContractAgent ca = new ContractAgent();
            ca.ContractAgentId = Guid.NewGuid();
            ca.ContractId = contractid;
            ca.num = db.ContractAgents.Count(x => x.ContractId == contractid) + 1;

            db.ContractAgents.Add(ca);

            db.SaveChanges();

            return ca;
        }

        public int ContractAgent_Count(Guid contractid)
        {
            return db.ContractAgents.Count(x => x.ContractId == contractid);
        }


        void IDisposable.Dispose()
        {

            db.Dispose();

            //throw new NotImplementedException();
        }
    }
}