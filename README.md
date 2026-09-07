# Sistema de Gestão de Consultas UVV

Aplicação Web ASP.NET Core MVC (.NET 8) para gerenciamento de usuários e
registro de consultas médicas/profissionais, usando Entity Framework Core
(Code First) e autenticação por cookies.

## Estrutura do projeto

```
UVVConsultas/
├── Controllers/
│   ├── ContaController.cs      -> Cadastro, Login e Logout
│   ├── ConsultasController.cs  -> CRUD de consultas ([Authorize])
│   └── HomeController.cs
├── Models/
│   ├── Usuario.cs
│   ├── Consulta.cs
│   └── LoginViewModel.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Views/
│   ├── Home/, Conta/, Consultas/, Shared/
├── Program.cs
└── appsettings.json
```

## Pré-requisitos

- .NET 8 SDK
- SQL Server (LocalDB, Express ou completo)
- Ferramenta `dotnet-ef` (para gerar/aplicar migrations)

```bash
dotnet tool install --global dotnet-ef
```

## Configuração

1. Ajuste a connection string em `appsettings.json` conforme o seu ambiente
   (por padrão está configurada para o LocalDB do Visual Studio):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UVVConsultasDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

2. Restaure os pacotes:

```bash
dotnet restore
```

## Criando o banco (Code First / Migrations)

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Isso cria as tabelas `Usuarios` e `Consultas`, com a chave estrangeira
`UsuarioId` em `Consultas` e um índice único em `Usuarios.Email`.

## Executando

```bash
dotnet run
```

Acesse `https://localhost:{porta}` exibida no console.

## Fluxo da aplicação

1. **Cadastro** (`/Conta/Cadastro`): cria um novo `Usuario`. A senha é
   transformada em hash (via `PasswordHasher<Usuario>`) antes de ser salva —
   nunca é gravada em texto puro.
2. **Login** (`/Conta/Login`): valida e-mail/senha contra o hash armazenado e
   grava um cookie de autenticação com os claims do usuário.
3. **Consultas** (`/Consultas`, protegido por `[Authorize]`): usuário logado
   pode criar, listar, editar e excluir suas próprias consultas. Todas as
   consultas exibidas/editadas são filtradas pelo `UsuarioId` do usuário
   logado (obtido do claim `ClaimTypes.NameIdentifier`).

## Pontos de arquitetura atendidos

- **Code First + Migrations**: entidades `Usuario` e `Consulta` mapeadas via
  EF Core; banco gerado a partir do modelo.
- **Data Annotations**: `[Required]`, `[EmailAddress]`, `[StringLength]` nas
  entidades, validadas tanto no cliente (jQuery Unobtrusive Validation)
  quanto no servidor (`ModelState.IsValid`).
- **Injeção de Dependência**: `ApplicationDbContext` registrado via
  `AddDbContext` em `Program.cs`.
- **Pipeline de Middleware**: ordem correta —
  `UseRouting → UseAuthentication → UseAuthorization → MapControllerRoute`.
- **Segurança**: `[Authorize]` no `ConsultasController`, senha com hash,
  `[ValidateAntiForgeryToken]` em todos os POSTs, e cada operação de consulta
  restrita ao dono do registro (evita que um usuário edite/exclua consultas
  de outro usuário apenas manipulando o ID na URL).
