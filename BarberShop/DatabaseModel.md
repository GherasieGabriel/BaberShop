# BarberShop - Database Model (Code First)

## Relational Model Diagram

```mermaid
erDiagram
    CLIENT ||--o{ APPOINTMENT : books
    BARBER ||--o{ APPOINTMENT : serves
    SERVICE ||--o{ APPOINTMENT : includes

    CLIENT ||--|| CLIENT_NOTIFICATION_SETTINGS : has

    APPOINTMENT ||--o{ REVIEW : receives
    CLIENT ||--o{ REVIEW : writes
    BARBER ||--o{ REVIEW : gets

    APPOINTMENT ||--o{ PAYMENT : paid_by

    CLIENT {
      int client_id PK
      string full_name
      string email
      string phone
      date member_since
    }

    BARBER {
      int barber_id PK
      string first_name
      string last_name
      string phone
      string email
      date hire_date
      bool is_active
    }

    SERVICE {
      int service_id PK
      string name
      string description
      int base_duration
      decimal base_price
      bool is_active
    }

    APPOINTMENT {
      int appointment_id PK
      int client_id FK
      int barber_id FK
      int service_id FK
      datetime start_datetime
      datetime end_datetime
      string status
    }

    PRODUCT {
      int product_id PK
      string name
      string category
      decimal price
      int stock_quantity
      bool is_active
    }

    CLIENT_NOTIFICATION_SETTINGS {
      int client_id PK,FK
      bool notify_email
      bool notify_sms
    }

    REVIEW {
      int review_id PK
      int appointment_id FK
      int client_id FK
      int barber_id FK
      int rating
      string comment
    }

    PAYMENT {
      int payment_id PK
      int appointment_id FK
      decimal amount
      string currency
      string method
      string status
    }
```

## Tables included (excluding user/auth tables)

- `CLIENT`
- `BARBER`
- `SERVICE`
- `APPOINTMENT`
- `PRODUCT`
- `CLIENT_NOTIFICATION_SETTINGS`
- `REVIEW`
- `PAYMENT`

## Code First notes

- Entity classes are in `Models/`
- EF Core context is `Data/BarberShopDbContext.cs`
- Connection string is configured in `appsettings.json`
- EF Core is registered in `Program.cs` via `AddDbContext`

### Migration commands (run locally)

- `dotnet tool install --global dotnet-ef`
- `dotnet ef migrations add InitialCreate`
- `dotnet ef database update`
