select ag.AgentId, ag.Name, 
cr.InsPrem,
cr.InsPremRur,
ca.[Percent],
ca.[Percent] *cr.InsPremRur/100 InsPremRur
from contract c
join ContractStatus cs on cs.ContractStatusId = c.ContractStatusId
join Status st on st.StatusId = cs.StatusId
join ContractAgent ca on ca.ContractId = c.ContractId
join Agent ag on ag.AgentId = ca.AgentId
join ContractRisk cr on cr.ContractId = c.ContractId
where st.Code in ('confirmed','annul')


update

