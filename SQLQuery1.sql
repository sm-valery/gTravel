select ag.AgentId, ag.Name, count(*) sold,
sum(cr.InsPrem) Prem,
sum(cr.InsPremRur) PremRur,
sum(ca.percent*
from contract c
join ContractStatus cs on cs.ContractStatusId = c.ContractStatusId
join Status st on st.StatusId = cs.StatusId
join ContractAgent ca on ca.ContractId = c.ContractId
join Agent ag on ag.AgentId = ca.AgentId
join ContractRisk cr on cr.ContractId = c.ContractId
where st.Code in ('confirmed','annul')
group by ag.AgentId, ag.Name
order by ag.Name


