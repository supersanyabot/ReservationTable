## Techstack

- .NET 10 (ASP.NET Core MVC)
- Entity Framework Core 10
- PostgreSQL 16
- Bootstrap 5
- Docker Compose

## โครงสร้างไฟล์สำคัญ

- `ReservationTable/Controllers/ReservationController.cs` ระบบจองโต๊ะ
- `ReservationTable/Controllers/TableManagementController.cs` ระบบจัดการโต๊ะ
- `ReservationTable/Data/ApplicationDbContext.cs` EF Core DbContext
- `ReservationTable/Data/DbInitializer.cs` Seed โต๊ะเริ่มต้น
- `ReservationTable/Views/Reservation/Index.cshtml` หน้าแสดงโต๊ะ + ฟอร์มจอง
- `ReservationTable/Views/TableManagement/Index.cshtml` หน้าจัดการโต๊ะ
- `docker-compose.yml` รันเว็บ + PostgreSQL

## การใช้งานด้วย Docker Compose

### Build และ Start

```bash
docker compose up --build -d
```

### เข้าใช้งาน

- Web: `http://localhost:5000`
- PostgreSQL: `localhost:5432`
  - DB: `reservation_db`
  - User: `postgres`
  - Password: `postgres`

### หยุดระบบ

```bash
docker compose down
```

### หยุดและลบข้อมูลฐานข้อมูล

```bash
docker compose down -v
```

## Run Local

1. ติดตั้ง .NET 10 SDK และ PostgreSQL
2. สร้างฐานข้อมูล `reservation_db`
3. ปรับ Connection String ที่ `ReservationTable/appsettings.json`
4. รันคำสั่ง:

```bash
dotnet restore
dotnet run --project ReservationTable/ReservationTable.csproj
```

## หมายเหตุ

- ระบบใช้ `EnsureCreated()` เพื่อสร้างตารางอัตโนมัติเมื่อเริ่มระบบครั้งแรก
- โต๊ะเริ่มต้นที่ seed: `A1, A2, A3, B1, B2, B3`
