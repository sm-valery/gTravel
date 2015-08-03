using gTravel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace gTravel
{


    public static class CurrManage
    {
        public static decimal getCurRate(goDbEntities db, Guid curid, DateTime? dt, bool updateifnotfound = true)
        {
            decimal ret = 0;

            if (dt == null)
                dt = DateTime.Now;

            if (curid == Guid.Parse("{00000000-0000-0000-0000-000000000000}"))
                return 1;
            
            dt = dt.Value.Date;

            var retv = db.CurRates.FirstOrDefault(x => x.CurrencyId == curid && x.RateDate ==  dt);
            if (retv != null)
                return retv.Rate;

            if (updateifnotfound)
            {
                updateCurRate(db, dt.Value);
                ret = getCurRate(db, curid, dt, false);
            }

            return ret;
        }

        public static void updateCurRate(goDbEntities db, DateTime dt)
        {
            XmlDocument xml = new XmlDocument();

            var curlist = db.Currencies.Where(x => x.cbrId != null).ToList();

            xml.Load(string.Format("http://www.cbr.ru/scripts/XML_daily.asp?date_req={0}",dt.ToShortDateString()));
            
            XmlNodeList vals = xml.GetElementsByTagName("Valute");

            decimal ratevaluenew=0;
            
            foreach(XmlNode v in vals)
            {
                if (v.HasChildNodes)
                {

                    string valid = v.Attributes[0].Value;
                    var curone = curlist.FirstOrDefault(x => x.cbrId.Trim() == valid);

                    if (curone!=null)
                    {
                        if (!db.CurRates.Any(x => x.CurrencyId == curone.CurrencyId && x.RateDate == dt))
                        {

                            foreach (XmlNode attr in v.ChildNodes)
                            {
                                if (attr.Name.ToLower() == "value")
                                {
                                    ratevaluenew = 0;
                                    if (decimal.TryParse(attr.FirstChild.Value, out ratevaluenew))
                                    {
                                        CurRate ratenew = new CurRate();
                                        ratenew.CurRateId = Guid.NewGuid();
                                        ratenew.CurrencyId = curone.CurrencyId;
                                        ratenew.RateDate = dt;
                                        ratenew.Rate = ratevaluenew;
                                        db.CurRates.Add(ratenew);
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }

            db.SaveChanges();

        }
    }
}