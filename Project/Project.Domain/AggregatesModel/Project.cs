using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Project.Domain.Events;
using Project.Domain.SeedWork;

namespace Project.Domain.AggregatesModel
{
    public class Project : Entity, IAggregateRoot
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 项目Logo
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 原始的BP文件地址
        /// </summary>
        public string OriginBPFile { get; set; }

        /// <summary>
        /// 转换后的BP文件地址
        /// </summary>
        public string FormatBPFile { get; set; }

        /// <summary>
        /// 是否显示敏感信息
        /// </summary>
        public bool ShowSecurityInfo { get; set; }

        /// <summary>
        /// 公司所在省Id
        /// </summary>
        public int ProvinceId { get; set; }


        /// <summary>
        /// 公司所在省名称
        /// </summary>
        public string Provice { get; set; }

        /// <summary>
        /// 公司所在城市Id
        /// </summary>
        public int CityId { get; set; }
        /// <summary>
        /// 公司所在城市名称
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 区域Id
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 公司成立时间
        /// </summary>
        public DateTime RegisterTime { get; set; }
        /// <summary>
        /// 项目基本信息
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// 出让股份比例
        /// </summary>
        public string FinPercentage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FinStage { get; set; }
        /// <summary>
        /// 收入单位(万)
        /// </summary>
        public int Income { get; set; }
        /// <summary>
        /// 利润 单位（万）
        /// </summary>
        public int Revenue { get; set; }
        /// <summary>
        /// 估值 单位（万）
        /// </summary>
        public int Valuation { get; set; }
        /// <summary>
        /// 佣金分配方式
        /// </summary>
        public int BrokerageOptions { get; set; }
        /// <summary>
        /// 是否委托各finbook平台
        /// </summary>
        public bool OnPlatform { get; set; }

        /// <summary>
        /// 可以范围设置
        /// </summary>
        public ProjectVisibleRule VisibleRule { get; set; }

        /// <summary>
        /// 根引用项目Id
        /// </summary>
        public int SourceId { get; set; }
        /// <summary>
        /// 上级引用项目Id
        /// </summary>
        public int ReferenceId { get; set; }

        /// <summary>
        /// 项目标签
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 项目属性: 行业领域，融资币种
        /// </summary>
        public List<ProjectProperty> Properties { get; set; }

        /// <summary>
        /// 贡献者列表
        /// </summary>
        public List<ProjectContributor> Contributors { get; set; }
        /// <summary>
        /// 查看者列表
        /// </summary>
        public List<ProjectViewer> Viewers { get; set; }

        public DateTime CreateTime { get; private set; }

        public DateTime UpdateTime { get; set; }

        private Project CloneProject(Project source = null)
        {
            if (source == null)
            {
                source = this;
            }

            var newProject = new Project
            {
                UserId = source.UserId,
                Avatar = source.Avatar,
                Company = source.Company,
                OriginBPFile = source.OriginBPFile,
                FormatBPFile = source.FormatBPFile,
                ShowSecurityInfo = source.ShowSecurityInfo,
                ProvinceId = source.ProvinceId,
                Provice = source.Provice,
                CityId = source.CityId,
                City = source.City,
                AreaId = source.AreaId,
                AreaName = source.AreaName,
                RegisterTime = source.RegisterTime,
                Introduction = source.Introduction,
                FinPercentage = source.FinPercentage,
                FinStage = source.FinStage,
                Income = source.Income,
                Revenue = source.Revenue,
                Valuation = source.Valuation,
                BrokerageOptions = source.BrokerageOptions,
                OnPlatform = source.OnPlatform,
                VisibleRule = source.VisibleRule,
                SourceId = source.SourceId,
                ReferenceId = source.ReferenceId,
                Tags = source.Tags,
                Properties = source.Properties?.Select(p => new ProjectProperty
                {
                    Key = p.Key,
                    Text = p.Text,
                    Value = p.Value
                }).ToList(),
                Contributors = new List<ProjectContributor>(),
                Viewers = new List<ProjectViewer>(),
            };
            return newProject;
        }

        public Project ContributorFork(int contributeId, Project source = null)
        {
            var newProject = CloneProject(source);
            newProject.UserId = contributeId;
            newProject.ReferenceId = source != null ? source.ReferenceId : this.ReferenceId;
            newProject.SourceId = source != null ? source.SourceId : this.SourceId;
            newProject.UpdateTime = DateTime.Now;
            // 添加贡献者
            // newProject.Contributors.Add(new ProjectContributor
            // {
            //     UserId = contributeId,
            //     ProjectId = 0, // 新项目，ID待生成
            //     JoinTime = DateTime.Now
            // });
            return newProject;
        }

        public Project()
        {
            this.Viewers = new List<ProjectViewer>();
            this.Contributors = new List<ProjectContributor>();

            this.AddDomainEvent(new ProjectCreatedEvent { project = this });
        }
        public void AddViewer(int userId, string userName, string avatar)
        {
            if (Viewers == null)
            {
                Viewers = new List<ProjectViewer>();
            }
            var projectViewer = new ProjectViewer
            {
                UserId = userId,
                UserName = userName,
                Avatar = avatar,
                CreatedTime = DateTime.Now
            };
            if (Viewers.Any(v => v.UserId == userId))
            {
                return;
            }
            Viewers.Add(projectViewer);

            AddDomainEvent(new ProjectViewerEvent { viewer = projectViewer });
        }

        public void AddContributor(ProjectContributor projectContributor)
        {
            if (Contributors == null)
            {
                Contributors = new List<ProjectContributor>();
            }

            if (!Contributors.Any(v => v.UserId == projectContributor.UserId))
            {
                Contributors.Add(projectContributor);
                AddDomainEvent(new ProjectJoinedEvent { contributor = projectContributor });
            }


        }
    }
}