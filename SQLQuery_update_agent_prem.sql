update ca 
set ca.insprem = r.insprem
from ContractAgent ca
join 
(
select sum(cr.insprem) insprem, cr.ContractId from contractrisk cr group by cr.ContractId
) r on r.ContractId = ca.ContractId
where ca.insprem = 0 or ca.insprem is null



update ca
set ca.inspremrur = round( ca.InsPrem * (select top 1 rate from CurRate where CurRate.RateDate = cast( c.date_out as date)),2)
from ContractAgent ca
join Contract c on c.ContractId = ca.ContractId
where ca.InsPremRur is null


select * from ContractAgent where insprem is not null