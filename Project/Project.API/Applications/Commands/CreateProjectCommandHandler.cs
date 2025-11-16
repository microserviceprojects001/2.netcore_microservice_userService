using MediatR;
using Project.Domain.AggregatesModel;
using ProjectAggregate = Project.Domain.AggregatesModel.Project;
namespace Project.API.Applications.Commands
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, int>
    {
        private readonly IProjectRepository _projectRepository;

        public CreateProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<int> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            // 使用工厂方法创建项目实体
            var project = ProjectAggregate.Create(
                userId: request.UserId,
                company: request.CreateProjectDto.Company,
                introduction: request.CreateProjectDto.Introduction,
                avatar: request.CreateProjectDto.Avatar,
                originBPFile: request.CreateProjectDto.OriginBPFile,
                formatBPFile: request.CreateProjectDto.FormatBPFile,
                provice: request.CreateProjectDto.Provice,
                city: request.CreateProjectDto.City,
                areaName: request.CreateProjectDto.AreaName,
                finStage: request.CreateProjectDto.FinStage,
                userName: request.CreateProjectDto.UserName,
                tags: request.CreateProjectDto.Tags,
                finPercentage: request.CreateProjectDto.FinPercentage,
                provinceId: request.CreateProjectDto.ProvinceId,
                cityId: request.CreateProjectDto.CityId,
                areaId: request.CreateProjectDto.AreaId,
                finMoney: request.CreateProjectDto.FinMoney,
                income: request.CreateProjectDto.Income,
                revenue: request.CreateProjectDto.Revenue,
                valuation: request.CreateProjectDto.Valuation,
                brokerageOptions: request.CreateProjectDto.BrokerageOptions,
                onPlatform: request.CreateProjectDto.OnPlatform,
                registerTime: request.CreateProjectDto.RegisterTime
            );
            await _projectRepository.AddAsync(project);
            await _projectRepository.UnitOfWork.SaveEntitiesAsync();


            return project.Id;
        }
    }
}