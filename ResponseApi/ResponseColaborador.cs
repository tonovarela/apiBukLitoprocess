using System.Text.Json.Serialization;
using apiBukLitoprocess.helpers;

namespace apiBukLitoprocess.responseApi
{
    public partial class ResponseColaborador
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("data")]
        public ColaboradorResponse? data { get; set; }
    }

    public partial class ColaboradorResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("person_id")]
        public long? person_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public long? id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("picture_url")]
        public string? picture_url { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("first_name")]
        public string? first_name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("surname")]
        public string? surname { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("second_surname")]
        public string? second_surname { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("full_name")]
        public string? full_name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("document_type")]
        public string? document_type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("document_number")]
        public string? document_number { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("curp")]
        public string? curp { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("rfc")]
        public string? rfc { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("code_sheet")]
        public string? code_sheet { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("social_security")]
        public string? social_security { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("gender")]
        public string? gender { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("birthday")]
        public DateTimeOffset? birthday { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("nationality")]
        public string? nationality { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("civil_status")]
        public string? civil_status { get; set; }

        [JsonPropertyName("office_phone")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? office_phone { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("celular")]
        public string? celular { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("phone")]
        public string? phone { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("email")]
        public string? email { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("personal_email")]
        public string? personal_email { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("address")]
        public string? address { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("location_id")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? location_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("postal_code")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? postal_code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("active_since")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? active_since { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("status")]
        public string? status { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("payment_method")]
        public string? payment_method { get; set; }

        [JsonPropertyName("bank")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? bank { get; set; }

        [JsonPropertyName("account_number")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? account_number { get; set; }

        [JsonPropertyName("clabe")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? clabe { get; set; }

        [JsonPropertyName("client_number")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? client_number { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("country_code")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? country_code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("private_role")]
        
        public bool? private_role { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("health_company")]

        public string? health_company { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("pension_regime")]
        public string? pension_regime { get; set; }

        [JsonPropertyName("pension_fund")]
        public object? pension_fund { get; set; }

        [JsonPropertyName("afc")]
        public object? afc { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("retired")]
        public bool? retired { get; set; }

        [JsonPropertyName("retirement_regime")]
        public object? retirement_regime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("payment_currency")]
        public string? payment_currency { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("period_type")]
        public string? period_type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("custom_attributes")]
        public DataCustomAttributes? custom_attributes { get; set; }

        [JsonPropertyName("active_until")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? active_until { get; set; }

        [JsonPropertyName("termination_reason")]
        public string? termination_reason { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("current_job")]
        public CurrentJob? current_job { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("family_responsabilities")]
        public List<FamilyResponsability>? family_responsabilities { get; set; }
    }

    public partial class CurrentJob
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("periodicity")]
        public string? periodicity { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("frequency")]
        public string? frequency { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("working_schedule_type")]
        public string? working_schedule_type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("zone_assignment")]
        public bool? zone_assignment { get; set; }

        [JsonPropertyName("union")]
        public object? union { get; set; }

        [JsonPropertyName("project")]
        public object? project { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("days")]
        public List<string>? days { get; set; }

        [JsonPropertyName("previous_job_id")]
        public object? previous_job_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public long? id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("company_id")]
        public long? company_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("start_date")]
        public DateTimeOffset? start_date { get; set; }

        [JsonPropertyName("end_date")]
        public object? end_date { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("area_id")]
        public long? area_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("cost_center")]
        public string? cost_center { get; set; }

        [JsonPropertyName("active_until")]
        public object? active_until { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("currency_code")]
        public string? currency_code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("boss")]
        public Boss? boss { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("employee_id")]
        public long? employee_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("wage")]
        public double? wage { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("type_of_regime")]
        public string? type_of_regime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("type_of_contract")]
        public string? type_of_contract { get; set; }

        [JsonPropertyName("end_of_contract")]
        public object? end_of_contract { get; set; }

        [JsonPropertyName("notice_date")]
        public object? notice_date { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("type_of_working_day")]
        public string? type_of_working_day { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("regular_hours")]
        public double? regular_hours { get; set; }

        [JsonPropertyName("risk")]
        public object? risk { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("wage_type")]
        public string? wage_type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("without_wage")]
        public bool? without_wage { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("location_id")]
        public long? location_id { get; set; }

        [JsonPropertyName("union_id")]
        public object? union_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("company_registration")]
        public long? company_registration { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("status")]
        public string? status { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("worker_kind")]
        public string? worker_kind { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("role")]
        public Role? role { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("custom_attributes")]
        public CurrentJobCustomAttributes? custom_attributes { get; set; }
    }

    public partial class Boss
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        
        public long? id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("document_type")]
        public string? document_type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("document_number")]
        public string? document_number { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("rfc")]
        public string? rfc { get; set; }
    }

    public partial class CurrentJobCustomAttributes
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("centroCostos")]
        public string? centroCostos { get; set; }

        [JsonPropertyName("ctrlit_recinto")]
        public object? ctrlit_recinto { get; set; }
    }

    public partial class Role
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public long? id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("code")]
        public string? code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("requirements")]
        public string? requirements { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("area_ids")]
        public List<long>? area_ids { get; set; }

        [JsonPropertyName("role_family")]
        public object? role_family { get; set; }
    }

    public partial class DataCustomAttributes
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("idColaborador")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? idColaborador { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("# Hijos")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? NumeroHijos { get; set; }


        

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Factor jornada")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? FactorJornada { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Tipo Contrato")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? TipoContrato { get; set; }
         

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("numInt")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? numInt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("numExt")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? numExt { get; set; }

        [JsonPropertyName("Alergias")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? Alergias { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Beneficiario1")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? Beneficiario1 { get; set; }

        [JsonPropertyName("Beneficiario2")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? Beneficiario2 { get; set; }

        [JsonPropertyName("Beneficiario3")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? Beneficiario3 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Colonia")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? Colonia { get; set; }

        [JsonPropertyName("contactoEmergencia1")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? contactoEmergencia1 { get; set; }

        [JsonPropertyName("contactoEmergencia2")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? contactoEmergencia2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("Delegacion")]
        public string? Delegacion { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("EstadoCivil")]
        public string? EstadoCivil { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]        
        [JsonPropertyName("Ext")]        
        public string? Ext { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("fecNacBeneficiario1")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? fecNacBeneficiario1 { get; set; }

        [JsonPropertyName("fecNacBeneficiario2")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? fecNacBeneficiario2 { get; set; }

        [JsonPropertyName("fecNacBeneficiario3")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? fecNacBeneficiario3 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("NivelAcademico")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? NivelAcademico { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Pais")]
        [JsonConverter(typeof(StringCustomConverter))]
        public string? Pais { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("parentescoBeneficiario1")]
        public string? parentescoBeneficiario1 { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("parentescoBeneficiario2")]
        public string? parentescoBeneficiario2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("parentescoBeneficiario3")]
        public string? parentescoBeneficiario3 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("parentestoContactoEmerg1")]
        public string? parentestoContactoEmerg1 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("parentestoContactoEmerg2")]
        public string? parentestoContactoEmerg2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("porcBeneficiario1")]
        public string? porcBeneficiario1 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("porcBeneficiario2")]
        public string? porcBeneficiario2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("porcBeneficiario3")]
        public string? porcBeneficiario3 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("telContactoEmerg1")]
        public string? telContactoEmerg1 { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("telContactoEmerg2")]
        public string? telContactoEmerg2 { get; set; }



        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("Lugar Nacimiento")]
        public string? lugarNacimiento { get; set; }


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("TipoColaborador")]
        public string? tipoColaborador { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(StringCustomConverter))]
        [JsonPropertyName("tipoSangre")]
        public string? tipoSangre { get; set; }
    }

    public partial class FamilyResponsability
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public long? id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("family_allowance_section")]
        public string? family_allowance_section { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("simple_family_responsability")]
        public long? simple_family_responsability { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("maternity_family_responsability")]
        public long? maternity_family_responsability { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("invalid_family_responsability")]
        public long? invalid_family_responsability { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("start_date")]
        public DateTimeOffset? start_date { get; set; }

        [JsonPropertyName("end_date")]
        public object? end_date { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("responsability_details")]
        public List<object>? responsability_details { get; set; }
    }

}