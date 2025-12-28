# 阶段1：构建
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

# 1. 设置工作目录并复制项目文件
WORKDIR /src
COPY User.Identity/User.Identity.csproj ./User.Identity/
COPY BuildingBlocks/Resilience/Resilience.csproj ./BuildingBlocks/Resilience/

# 2. 恢复依赖
WORKDIR /src/User.Identity
RUN dotnet restore

# 3. 回到根目录，复制所有源代码
WORKDIR /src
# 复制User.Identity源代码（不包括项目文件，因为已经复制了）
COPY User.Identity/ ./User.Identity/
# 复制Resilience源代码（不包括项目文件，因为已经复制了）
COPY BuildingBlocks/Resilience/ ./BuildingBlocks/Resilience/

# 4. 清理所有obj/bin目录（关键步骤）
RUN find /src -name "obj" -type d -exec rm -rf {} + || true
RUN find /src -name "bin" -type d -exec rm -rf {} + || true

# 5. 发布项目
WORKDIR /src/User.Identity
RUN dotnet publish -c Release -o /app/out

# 阶段2：运行时
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out ./
# 注意：这里需要从构建上下文复制cert.pfx
COPY User.Identity/cert.pfx /app/

# 安全警告：不要在Dockerfile中硬编码密码
# 建议使用Docker secrets或环境变量传入
ARG CERT_PASSWORD
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/cert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD:-YourPassword123}

EXPOSE 80
EXPOSE 445

ENTRYPOINT ["dotnet", "User.Identity.dll"]