# Biblioteca — Gestão de Biblioteca

Aplicação Web desenvolvida em **ASP.NET Core MVC** no âmbito da UFCD 5417 (CET 105 — CINEL) para gerir o acervo de uma biblioteca: livros, autores, categorias, leitores e empréstimos.

## Funcionalidades

- **Autenticação**: login, logout, registo, alteração do perfil, alteração da palavra-passe e recuperação da palavra-passe por email (ASP.NET Core Identity com `AccountController` próprio).
- **CRUD completo** de Autores, Categorias, Livros, Leitores e Empréstimos.
- **Empréstimos e devoluções** com regras de negócio:
  - só é possível emprestar livros com exemplares disponíveis;
  - cada leitor pode ter no máximo 3 empréstimos ativos;
  - um leitor não pode ter dois exemplares do mesmo livro em simultâneo;
  - prazo máximo de 60 dias; deteção automática de empréstimos em atraso.
- **Consultas relacionadas**: livros por autor e por categoria, histórico de empréstimos por livro e por leitor, painel com estatísticas e empréstimos em atraso.
- **Pesquisa e filtros** nas listagens.
- **Tratamento de erros**: registos inexistentes, dados inválidos, eliminação de registos com dependências e erros de gravação — sem mostrar os ecrãs de erro de desenvolvimento.

## Modelo de dados

| Entidade   | Relações |
|------------|----------|
| Autor      | 1 Autor → N Livros |
| Categoria  | 1 Categoria → N Livros |
| Livro      | N:1 Autor, N:1 Categoria |
| Leitor     | — |
| Emprestimo | Entidade associativa: **Livro N ↔ N Leitor** (com datas do empréstimo) |
| User       | Utilizadores da aplicação (Identity) |

## Tecnologias

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10 (Code First + Migrations)
- SQL Server (LocalDB)
- ASP.NET Core Identity
- Repository Pattern (repositório genérico + repositórios específicos)
- Bootstrap 5 + Bootstrap Icons
- MailKit (envio de emails)

## Estrutura

```
Controllers/        Controladores MVC
Data/
  Entities/         Entidades (modelo da BD)
  Repositories/     IGenericRepository, GenericRepository e repositórios específicos
  DataContext.cs    DbContext (IdentityDbContext)
  SeedDb.cs         Dados iniciais
Helpers/            UserHelper (Identity) e MailHelper (email)
Models/             ViewModels
Migrations/         Migrations do EF Core
Views/              Vistas Razor
```

## Como executar

1. Clonar o repositório e abrir `Biblioteca.sln` no Visual Studio 2026.
2. Confirmar a connection string em `appsettings.json` (por omissão usa o LocalDB):
   ```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BibliotecaDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   ```
3. No **Package Manager Console** executar:
   ```
   Update-Database
   ```
   (a aplicação também aplica as migrations automaticamente ao arrancar).
4. Executar a aplicação (F5).

### Utilizador de demonstração

| Email | Palavra-passe |
|-------|---------------|
| admin@biblioteca.pt | Admin123 |

### Configurar o email (recuperação da palavra-passe)

Preencher a secção `Mail` em `appsettings.json` (ex.: Gmail com palavra-passe de aplicação):

```json
"Mail": {
  "NameFrom": "Biblioteca",
  "From": "email@gmail.com",
  "Smtp": "smtp.gmail.com",
  "Port": "587",
  "Password": "palavra-passe-de-aplicacao"
}
```

Se o email não estiver configurado, em ambiente de desenvolvimento a ligação de recuperação é apresentada no ecrã para efeitos de demonstração.
