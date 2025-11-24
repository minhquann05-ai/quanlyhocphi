# 1. Dùng hình ảnh SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy toàn bộ code vào
COPY . ./

# Restore và Build project API
RUN dotnet restore
RUN dotnet publish QLHocPhi.API/QLHocPhi.API.csproj -c Release -o out

# 2. Dùng hình ảnh Runtime để chạy (nhẹ hơn)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Cài đặt thư viện hỗ trợ font chữ cho QuestPDF (để xuất PDF không lỗi)
RUN apt-get update && apt-get install -y libfontconfig1

# Mở cổng 80
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Chạy file DLL
ENTRYPOINT ["dotnet", "QLHocPhi.API.dll"]