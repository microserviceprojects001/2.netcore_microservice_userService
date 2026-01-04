# 阶段1：构建
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

# 1. 设置工作目录并复制项目文件
WORKDIR /src
COPY Recommend.API/Recommend.API.csproj ./Recommend.API/
COPY BuildingBlocks/Resilience/Resilience.csproj ./BuildingBlocks/Resilience/

# 2. 恢复依赖
WORKDIR /src/Recommend.API
RUN dotnet restore

# 3. 回到根目录，复制所有源代码
WORKDIR /src
# 复制Recommend.API源代码
COPY Recommend.API/ ./Recommend.API/

# 复制Resilience源代码
COPY BuildingBlocks/Resilience/ ./BuildingBlocks/Resilience/

# 4. 清理所有obj/bin目录（避免编译冲突）
RUN find /src -name "obj" -type d -exec rm -rf {} + || true
RUN find /src -name "bin" -type d -exec rm -rf {} + || true

# 5. 发布项目
WORKDIR /src/Recommend.API
RUN dotnet publish -c Release -o /app/out

# 阶段2：运行时
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out ./

# 复制证书文件
COPY Recommend.API/cert.pfx /app/

# 安全警告：不要在Dockerfile中硬编码密码
# 建议使用Docker secrets或环境变量传入
ARG CERT_PASSWORD
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/cert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD:-YourPassword123}

EXPOSE 80
EXPOSE 446

ENTRYPOINT ["dotnet", "Recommend.API.dll"]