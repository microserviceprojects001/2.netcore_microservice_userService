using System.ComponentModel.DataAnnotations;

namespace Project.API.Dtos
{
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "项目介绍不能为空")]
        public string Introduction { get; set; }

        [Required(ErrorMessage = "公司名称不能为空")]
        public string Company { get; set; }

        // 其他可选字段
        public string? City { get; set; }
        public string? Tags { get; set; }
        public string? Avatar { get; set; }
        public string? Provice { get; set; }
        public string? AreaName { get; set; }
        public string? FinStage { get; set; }
        public string? UserName { get; set; }
        public string? FormatBPFile { get; set; }
        public string? OriginBPFile { get; set; }
        public string? FinPercentage { get; set; }
        public decimal? FinMoney { get; set; }
        public int? Income { get; set; }
        public int? Revenue { get; set; }
        public int? Valuation { get; set; }
        public int? BrokerageOptions { get; set; }
        public bool? OnPlatform { get; set; }
        public DateTime? RegisterTime { get; set; }
        public int? ProvinceId { get; set; }
        public int? CityId { get; set; }
        public int? AreaId { get; set; }
    }
}