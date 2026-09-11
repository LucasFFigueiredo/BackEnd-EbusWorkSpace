# 🏢 BackEnd-EbusWorkSpace

API RESTful para **gestão e reserva de espaços de trabalho**, desenvolvida em **C# .NET 10** com arquitetura limpa (Clean Architecture), autenticação JWT + Google OAuth, persistência com PostgreSQL e suporte a Docker.

---

## 📑 Sumário

- [Visão Geral](#-visão-geral)
- [Tecnologias](#-tecnologias)
- [Arquitetura do Projeto](#-arquitetura-do-projeto)
- [Pré-requisitos](#-pré-requisitos)
- [Configuração do Ambiente](#-configuração-do-ambiente)
- [Como Rodar](#-como-rodar)
- [Endpoints da API](#-endpoints-da-api)
- [Testes](#-testes)
- [Docker](#-docker)
- [Variáveis de Ambiente](#-variáveis-de-ambiente)
- [Licença](#-licença)

---

## 🔍 Visão Geral

O sistema permite que colaboradores reservem **mesas** e **salas de reunião**, com funcionalidades como:

- ✅ **Check-in** em reservas
- 🚫 **No-show automático** — reservas sem check-in são marcadas como no-show por um worker em background
- 🔒 **Bloqueio de espaços** para manutenção com desbloqueio automático por data
- 👥 **Perfis de usuário** com níveis de permissão (Employee, Manager, Facilities, Admin)
- 📊 **Dashboard** com métricas e indicadores de uso
- 📝 **Logs de auditoria** para rastreabilidade de ações
- 🔐 **Autenticação** via JWT e Google OAuth 2.0
- ⏳ **Aprovação de reservas** para espaços que requerem autorização

---

## 🚀 Tecnologias

| Tecnologia | Versão | Finalidade |
|---|---|---|
| **.NET** | 10.0 | Framework principal |
| **C#** | 13 | Linguagem de programação |
| **ASP.NET Core** | 10.0 | Web API REST |
| **Entity Framework Core** | 10.0.11 | ORM e migrações de banco |
| **PostgreSQL** | — | Banco de dados relacional |
| **Npgsql** | 10.0.3 | Provider PostgreSQL para EF Core |
| **MediatR** | 14.2.0 | Mediator pattern (CQRS) |
| **AutoMapper** | 16.2.0 | Mapeamento de objetos (Entity ↔ DTO) |
| **JWT Bearer** | 10.0.11 | Autenticação via tokens JWT |
| **Google.Apis.Auth** | 1.75.0 | Autenticação OAuth 2.0 com Google |
| **Swashbuckle** | 6.6.2 | Documentação Swagger / OpenAPI |
| **Docker** | — | Containerização |

### 🧪 Testes

| Tecnologia | Versão | Finalidade |
|---|---|---|
| **xUnit** | 2.9.3 | Framework de testes |
| **Moq** | 4.20.72 | Mocking de dependências |
| **FluentAssertions** | 8.10.0 | Asserções expressivas |
| **Testcontainers.PostgreSql** | 4.14.0 | Testes de integração com PostgreSQL real (Docker) |
| **Microsoft.AspNetCore.Mvc.Testing** | 10.0.11 | Testes de integração da API |

---

## 🏗 Arquitetura do Projeto

O projeto segue **Clean Architecture** com separação em camadas:

```
BackEnd-EbusWorkSpace/
├── src/
│   ├── JCA.WorkSpace.Domain/                          #    Camada de Domínio
│   │   ├── Entities/                                  #    Entidades (User, Space, Reservation, AuditLog)
│   │   ├── Enums/                                     #    Enumerações (SpaceType, UserProfile, ReservationStatus)
│   │   ├── Exceptions/                                #    Exceções customizadas
│   │   └── Interfaces/                                #    Contratos (repositórios, UnitOfWork)
│   │
│   ├── JCA.WorkSpace.Application/                     #    Camada de Aplicação
│   │   ├── Commands/                                  #    Comandos (CQRS - escrita)
│   │   │   ├── Login/
│   │   │   ├── NoShows/
│   │   │   ├── Reservations/
│   │   │   ├── Spaces/
│   │   │   └── Users/
│   │   ├── Queries/                                   #    Queries (CQRS - leitura)
│   │   │   ├── AuditLogs/
│   │   │   ├── Dashboards/
│   │   │   ├── Reservations/
│   │   │   ├── Spaces/
│   │   │   └── Users/
│   │   ├── Handlers/                                  #    Handlers do MediatR
│   │   ├── Dtos/                                      #    Data Transfer Objects
│   │   └── Mappers/                                   #    Perfis do AutoMapper
│   │
│   ├── JCA.WorkSpace.Infrastructure.Data/             #    Camada de Infraestrutura (Dados)
│   │   ├── Contexts/                                  #    DbContexts (escrita e leitura)
│   │   ├── Repositories/                              #    Implementações dos repositórios
│   │   ├── Migrations/                                #    Migrações do EF Core
│   │   └── UnitOfWork/                                #    Implementação do UnitOfWork
│   │
│   ├── JCA.WorkSpace.Infrastructure.CrossCutting.IoC/ #    Inversão de Controle
│   │   └── DependencyInjection.cs                     #    Registro de todas as dependências
│   │
│   └── JCA.WorkSpace.Service.API/                     #    Camada de Apresentação (API)
│       ├── Controllers/                               #    Controllers REST
│       ├── Middlewares/                               #    Middleware de observabilidade
│       ├── Workers/                                   #    Background services (NoShowWorker)
│       └── Program.cs                                 #    Entry point da aplicação
│
├── tests/
│   ├── JCA.WorkSpace.Application.Tests/               # Testes unitários (handlers, mappers)
│   └── JCA.WorkSpace.Service.API.Tests/               # Testes de integração (controllers, API)
│
├── Dockerfile                                         # Build e deploy em container
├── JCA.WorkSpace.slnx                                 # Solution file
└── global.json                                        # Versão do SDK (.NET 10.0.400)
```

### 📐 Padrões Utilizados

| Padrão | Onde é usado |
|---|---|
| **CQRS** | Commands e Queries separados na camada Application |
| **Mediator** | MediatR para desacoplar controllers dos handlers |
| **Repository** | Abstração de acesso a dados via interfaces no Domain |
| **Unit of Work** | Transações atômicas no banco de dados |
| **Dependency Injection** | Registro centralizado no projeto CrossCutting.IoC |
| **CQRS Read/Write Split** | `WorkSpaceContext` (escrita) e `WorkSpaceContextRead` (leitura) |

---

## 📋 Pré-requisitos

Antes de rodar o projeto, certifique-se de ter instalado:

| Ferramenta | Versão mínima | Link |
|---|---|---|
| **.NET SDK** | 10.0.400 | [Download](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **PostgreSQL** | 14+ | [Download](https://www.postgresql.org/download/) |
| **Docker** *(opcional)* | 20+ | [Download](https://www.docker.com/get-started/) |
| **Git** | 2.30+ | [Download](https://git-scm.com/downloads) |

---

## ⚙ Configuração do Ambiente

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/BackEnd-EbusWorkSpace.git
cd BackEnd-EbusWorkSpace
```

### 2. Configure o banco de dados

Crie um banco de dados PostgreSQL:

```sql
CREATE DATABASE workspace_db;
```

### 3. Configure as variáveis de ambiente

Edite o arquivo `src/JCA.WorkSpace.Service.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=workspace_db;Username=postgres;Password=SUA_SENHA_AQUI"
  },
  "JwtSettings": {
    "Secret": "CHAVE_SECRETA_COM_NO_MINIMO_32_CARACTERES",
    "Issuer": "WorkSpace-API",
    "Audience": "WorkSpace-WebApp",
    "ExpirationInMinutes": 480
  },
  "GoogleAuth": {
    "ClientId": "SEU_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  }
}
```

> ⚠️ **Importante:** Nunca commite senhas ou chaves reais. Use variáveis de ambiente ou um secret manager em produção.

### 4. Aplique as migrações do banco

```bash
dotnet ef database update --project src/JCA.WorkSpace.Infrastructure.Data --startup-project src/JCA.WorkSpace.Service.API
```

---

## ▶ Como Rodar

### Rodando localmente

```bash
# Restaurar dependências
dotnet restore

# Rodar a API
dotnet run --project src/JCA.WorkSpace.Service.API
```

A API estará disponível em:
- **HTTP:** `http://localhost:5000`
- **Swagger UI:** `http://localhost:5000/swagger` *(apenas em Development)*

### Rodando com Docker

```bash
# Build da imagem
docker build -t ebus-workspace-api .

# Rodar o container
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=workspace_db;Username=postgres;Password=SUA_SENHA" \
  -e JwtSettings__Secret="CHAVE_SECRETA_COM_NO_MINIMO_32_CARACTERES" \
  -e JwtSettings__Issuer="WorkSpace-API" \
  -e JwtSettings__Audience="WorkSpace-WebApp" \
  -e JwtSettings__ExpirationInMinutes="480" \
  --name workspace-api \
  ebus-workspace-api
```

A API estará disponível em `http://localhost:8080`.

---

## 📡 Endpoints da API

### 🔐 Autenticação (`/api/auth`)

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/auth/google` | Login via Google OAuth 2.0 |

### 👤 Usuários (`/api/users`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/users` | Listar todos os usuários |
| `GET` | `/api/users/{id}` | Buscar usuário por ID |
| `POST` | `/api/users` | Criar usuário |
| `PUT` | `/api/users/{id}` | Atualizar usuário |
| `DELETE` | `/api/users/{id}` | Remover usuário |

### 🏢 Espaços (`/api/spaces`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/spaces` | Listar todos os espaços |
| `GET` | `/api/spaces/{id}` | Buscar espaço por ID |
| `POST` | `/api/spaces` | Criar espaço |
| `PUT` | `/api/spaces/{id}` | Atualizar espaço |
| `DELETE` | `/api/spaces/{id}` | Remover espaço |

### 📅 Reservas (`/api/reservations`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/reservations` | Listar reservas (com filtros) |
| `POST` | `/api/reservations` | Criar reserva |
| `PUT` | `/api/reservations/{id}` | Atualizar reserva |
| `DELETE` | `/api/reservations/{id}` | Cancelar reserva |
| `POST` | `/api/reservations/{id}/checkin` | Realizar check-in |

### 📊 Dashboard (`/api/dashboard`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/dashboard` | Métricas e indicadores de uso |

### 📝 Auditoria (`/api/audits`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/audits` | Consultar logs de auditoria |

### ❤️ Health Check

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/health` | Verificar saúde da aplicação |

> 📖 A documentação interativa completa está disponível via **Swagger UI** em `/swagger` quando a API está rodando em modo Development.

---

## 🧪 Testes

O projeto possui duas suítes de testes:

### Testes Unitários

Localizados em `tests/JCA.WorkSpace.Application.Tests/`, cobrem os **handlers** e **mappers** da camada Application usando **Moq** para mocking e **FluentAssertions** para asserções.

### Testes de Integração

Localizados em `tests/JCA.WorkSpace.Service.API.Tests/`, testam os **controllers** e fluxos completos da API usando:
- **`WebApplicationFactory`** para simular a aplicação real
- **Testcontainers** para subir um PostgreSQL real via Docker durante os testes

### Rodar os testes

```bash
# Todos os testes
dotnet test

# Apenas testes unitários
dotnet test tests/JCA.WorkSpace.Application.Tests

# Apenas testes de integração (requer Docker rodando)
dotnet test tests/JCA.WorkSpace.Service.API.Tests
```

> ⚠️ Os testes de integração utilizam **Testcontainers**, que requer o **Docker** rodando na máquina.

---

## 🐳 Docker

O projeto inclui um `Dockerfile` multi-stage otimizado:

1. **Build stage** — usa o SDK do .NET 10 para compilar e publicar
2. **Runtime stage** — usa o runtime ASP.NET 10 (imagem leve) para executar

```dockerfile
# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# ...

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
EXPOSE 8080
ENTRYPOINT ["dotnet", "JCA.WorkSpace.Service.API.dll"]
```

A aplicação roda na **porta 8080** dentro do container.

---

## 🔑 Variáveis de Ambiente

| Variável | Descrição | Obrigatória |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | String de conexão do PostgreSQL | ✅ Sim |
| `JwtSettings__Secret` | Chave secreta para assinatura do JWT (mín. 32 chars) | ✅ Sim |
| `JwtSettings__Issuer` | Emissor do token JWT | ✅ Sim |
| `JwtSettings__Audience` | Audiência do token JWT | ✅ Sim |
| `JwtSettings__ExpirationInMinutes` | Tempo de expiração do token (padrão: 480) | ❌ Não |
| `GoogleAuth__ClientId` | Client ID do Google OAuth 2.0 | ✅ Sim |

---

## 🔄 Background Services

### NoShowWorker

Worker que roda em background a cada **5 minutos** e realiza:

- **Salas** → marca como no-show reservas de sala sem check-in (a cada execução)
- **Mesas** → marca como no-show reservas de mesa sem check-in (uma vez por dia, após 10:31 horário de Brasília)
- **Conclusão automática** → finaliza reservas que passaram do horário de término
- **Desbloqueio automático** → libera espaços cuja data de manutenção expirou

### Observability Middleware

Middleware global que:
- Gera um `X-Request-Id` único para cada request
- Loga duração, endpoint e status de cada requisição
- Trata exceções de forma centralizada retornando JSON padronizado

---

## 📄 Licença

Este projeto é de uso interno da **Ebus**.

---
<p align="center">
  Desenvolvido por <strong>Lucas Figueiredo - Pedro Viveiros - Thiffany Silva</strong>
</p>
