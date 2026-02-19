## Tech Stack

- .NET 10 (ASP.NET Core MVC)
- Entity Framework Core 10
- PostgreSQL 16
- Bootstrap 5
- Docker Compose

## โครงสร้างไฟล์สำคัญ

- `ReservationTable/Controllers/ReservationController.cs` จัดการ flow การจองโต๊ะ
- `ReservationTable/Controllers/TableManagementController.cs` จัดการโซน/โต๊ะ/สถานะโต๊ะ
- `ReservationTable/Data/ApplicationDbContext.cs` EF Core DbContext
- `ReservationTable/Data/DbInitializer.cs` สร้างฐานข้อมูลและ seed ข้อมูลเริ่มต้น
- `ReservationTable/Views/Reservation/Index.cshtml` หน้าจองโต๊ะ
- `ReservationTable/Views/TableManagement/Index.cshtml` หน้าจัดการโต๊ะ
- `docker-compose.yml` สำหรับรัน Web + PostgreSQL

## Seed Data เริ่มต้น
- โซน: `A1, A2, A3, B1, B2, B3`
- โต๊ะ: โซนละ 6 โต๊ะ (`1-6`) สถานะเริ่มต้นเป็น `Available`

## การใช้งานด้วย Docker Compose

### Build และ Start

```bash
docker compose up --build -d
```

### เข้าใช้งาน

- Web: `http://localhost:5000`
- PostgreSQL: `localhost:5432`
  - Database: `reservation_db`
  - Username: `postgres`
  - Password: `postgres`

### หยุดระบบ

```bash
docker compose down
```

### หยุดระบบและลบฐานข้อมูล

```bash
docker compose down -v
```

## Run Local

1. ติดตั้ง .NET 10 SDK และ PostgreSQL
2. สร้างฐานข้อมูลชื่อ `reservation_db`
3. ตรวจสอบ connection string ใน `ReservationTable/appsettings.json`
4. รันคำสั่ง:

```bash
dotnet restore
dotnet run --project ReservationTable/ReservationTable.csproj
```