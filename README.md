# KERP

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Google OAuth Credentials

### Setup

1. **Clone repository**
```bash
   git clone https://github.com/your-org/KERP.git
   cd KERP
```

2. **Configure Google OAuth**
   
   a. Get credentials:
   - Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
   - Create OAuth 2.0 Client ID (Web application)
   - Add authorized redirect URIs:
     - `https://localhost:7219/signin-google`
     - `http://localhost:5100/signin-google`
   - Copy Client ID and Client Secret

   b. Set up User Secrets (Development):
```bash
   cd src/KERP.BlazorUI
   dotnet user-secrets init
   dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_SECRET_HERE"
```

3. **Database setup**
```bash
   dotnet ef database update --project src/KERP.Infrastructure --startup-project src/KERP.BlazorUI
```

4. **Run application**
```bash
   cd src/KERP.BlazorUI
   dotnet run
```

5. **Open browser**
```
   https://localhost:7219
```

## 🔒 Security - Secrets Management

### Development (Local Machine)
Use **User Secrets** - secrets stored outside repository:
```bash
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-secret"
```

### Production / Staging

## 📁 Project Structure
```
KERP/
├── src/
│   ├── KERP.Domain/          # Domain entities, value objects, domain events
│   ├── KERP.Application/     # Business logic, CQRS, validation
│   ├── KERP.Infrastructure/  # Data access, external services
│   ├── KERP.BlazorUI/        # Presentation layer (Blazor Server)
│   └── KERP.API/             # REST API (future)
├── tests/
│   └── KERP.UnitTests/       # Unit tests
├── docs/                     # Documentation
└── README.md
```

## 🏗️ Architecture

- **Clean Architecture** (Domain-centric)
- **CQRS** (Command Query Responsibility Segregation)
- **Result Pattern** (no exceptions for business logic)
- **Validation Chain** (Chain of Responsibility)
- **Factory Pattern** (domain entity creation)

## 🧪 Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📚 Documentation

- [Validation Chain](docs/VALIDATION_CHAIN.md)
- [CQRS Implementation](docs/CQRS.md)
- [Security Guidelines](docs/SECURITY.md)

## 👥 Contributing

1. Create feature branch: `git checkout -b feature/my-feature`
2. Commit changes: `git commit -m "feat: add my feature"`
3. Push: `git push origin feature/my-feature`
4. Create Pull Request

## 📄 License

[??]