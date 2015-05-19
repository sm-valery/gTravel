﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace gTravel.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class goDbEntities : DbContext
    {
        public goDbEntities()
            : base("name=goDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ConditionSeria> ConditionSerias { get; set; }
        public virtual DbSet<ContractCondition> ContractConditions { get; set; }
        public virtual DbSet<Subject> Subjects { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<RiskSeria> RiskSerias { get; set; }
        public virtual DbSet<Contract_territory> Contract_territory { get; set; }
        public virtual DbSet<Risk> Risks { get; set; }
        public virtual DbSet<ContractRisk> ContractRisks { get; set; }
        public virtual DbSet<ContractStatu> ContractStatus { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Territory> Territories { get; set; }
        public virtual DbSet<v_contract> v_contract { get; set; }
        public virtual DbSet<CurRate> CurRates { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<import_settings> import_settings { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<ImportLog> ImportLogs { get; set; }
        public virtual DbSet<ImportLogContract> ImportLogContracts { get; set; }
        public virtual DbSet<v_importlog> v_importlog { get; set; }
        public virtual DbSet<Agent> Agents { get; set; }
        public virtual DbSet<AgentUser> AgentUsers { get; set; }
        public virtual DbSet<AddRef> AddRefs { get; set; }
        public virtual DbSet<Condition> Conditions { get; set; }
        public virtual DbSet<BorderoContract> BorderoContracts { get; set; }
        public virtual DbSet<Bordero> Borderoes { get; set; }
        public virtual DbSet<v_bordero> v_bordero { get; set; }
        public virtual DbSet<v_contract_history> v_contract_history { get; set; }
        public virtual DbSet<v_bordero_new> v_bordero_new { get; set; }
        public virtual DbSet<v_contract_ag> v_contract_ag { get; set; }
        public virtual DbSet<v_contract_risk> v_contract_risk { get; set; }
        public virtual DbSet<Tarif> Tarifs { get; set; }
        public virtual DbSet<seria> serias { get; set; }
        public virtual DbSet<v_agentseria> v_agentseria { get; set; }
        public virtual DbSet<ContractFactor> ContractFactors { get; set; }
        public virtual DbSet<TerritoryGrp> TerritoryGrps { get; set; }
        public virtual DbSet<TarifPlan> TarifPlans { get; set; }
        public virtual DbSet<TarifPlanAgent> TarifPlanAgents { get; set; }
        public virtual DbSet<AgentSeria> AgentSerias { get; set; }
        public virtual DbSet<Factor> Factors { get; set; }
    
        public virtual ObjectResult<v_contract> spContract(string userId, Nullable<decimal> contractnumber, Nullable<System.Guid> importLogId, Nullable<System.Guid> contractid, Nullable<System.Guid> borderoId)
        {
            var userIdParameter = userId != null ?
                new ObjectParameter("UserId", userId) :
                new ObjectParameter("UserId", typeof(string));
    
            var contractnumberParameter = contractnumber.HasValue ?
                new ObjectParameter("contractnumber", contractnumber) :
                new ObjectParameter("contractnumber", typeof(decimal));
    
            var importLogIdParameter = importLogId.HasValue ?
                new ObjectParameter("ImportLogId", importLogId) :
                new ObjectParameter("ImportLogId", typeof(System.Guid));
    
            var contractidParameter = contractid.HasValue ?
                new ObjectParameter("contractid", contractid) :
                new ObjectParameter("contractid", typeof(System.Guid));
    
            var borderoIdParameter = borderoId.HasValue ?
                new ObjectParameter("BorderoId", borderoId) :
                new ObjectParameter("BorderoId", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<v_contract>("spContract", userIdParameter, contractnumberParameter, importLogIdParameter, contractidParameter, borderoIdParameter);
        }
    
        public virtual ObjectResult<v_contract> spContract(string userId, Nullable<decimal> contractnumber, Nullable<System.Guid> importLogId, Nullable<System.Guid> contractid, Nullable<System.Guid> borderoId, MergeOption mergeOption)
        {
            var userIdParameter = userId != null ?
                new ObjectParameter("UserId", userId) :
                new ObjectParameter("UserId", typeof(string));
    
            var contractnumberParameter = contractnumber.HasValue ?
                new ObjectParameter("contractnumber", contractnumber) :
                new ObjectParameter("contractnumber", typeof(decimal));
    
            var importLogIdParameter = importLogId.HasValue ?
                new ObjectParameter("ImportLogId", importLogId) :
                new ObjectParameter("ImportLogId", typeof(System.Guid));
    
            var contractidParameter = contractid.HasValue ?
                new ObjectParameter("contractid", contractid) :
                new ObjectParameter("contractid", typeof(System.Guid));
    
            var borderoIdParameter = borderoId.HasValue ?
                new ObjectParameter("BorderoId", borderoId) :
                new ObjectParameter("BorderoId", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<v_contract>("spContract", mergeOption, userIdParameter, contractnumberParameter, importLogIdParameter, contractidParameter, borderoIdParameter);
        }
    
        public virtual int BorderoCreate()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("BorderoCreate");
        }
    
        public virtual int BorderoUpdPrepare()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("BorderoUpdPrepare");
        }
    
        public virtual ObjectResult<spMonthPrem_Result1> spMonthPrem(string userId)
        {
            var userIdParameter = userId != null ?
                new ObjectParameter("UserId", userId) :
                new ObjectParameter("UserId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spMonthPrem_Result1>("spMonthPrem", userIdParameter);
        }
    }
}
