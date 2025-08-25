# 🚀 TechSub - SaaS Subscription Management API

API completa para gerenciamento de assinaturas SaaS desenvolvida em .NET 8 com arquitetura limpa e práticas de segurança.

## 📋 Funcionalidades

- ✅ **Autenticação JWT** - Sistema de login seguro
- ✅ **Gerenciamento de Usuários** - CRUD completo com roles (Admin/User)
- ✅ **Planos de Assinatura** - Criação e gestão de planos (Free, Basic, Pro)
- ✅ **Sistema de Assinaturas** - Ciclo completo com trials e renovações
- ✅ **Processamento de Pagamentos** - Simulação de gateway de pagamento
- ✅ **Relatórios** - Listagem de usuários ativos por plano
- ✅ **Testes Unitários** - Cobertura com xUnit e Moq
- ✅ **Configuração Segura** - Dados sensíveis protegidos via .gitignore

## 🏗️ Arquitetura

```
TechSub/
├── 📁 TechSub.Dominio/          # Entidades e interfaces
├── 📁 TechSub.Aplicacao/        # Services e DTOs
├── 📁 TechSub.Infraestrutura/   # Repositórios e EF Core
├── 📁 TechSub.WebAPI/           # Controllers e configurações
└── 📁 TechSub.Tests/            # Testes unitários
```

## ⚡ Como Executar

### Pré-requisitos
- .NET 8 SDK
- PostgreSQL
- Visual Studio ou VS Code

### 1. Clone o Repositório
```bash
git clone <url-do-repositorio>
cd TechSub
```

### 2. Configure as Variáveis de Ambiente
```bash
# Copie o arquivo de exemplo
cp appsettings.example.json src/TechSub.WebAPI/appsettings.json

# Configure suas credenciais no appsettings.json:
# - ConnectionString do PostgreSQL
# - JWT SecretKey (mínimo 32 caracteres)
```

### 3. Execute as Migrations
```bash
dotnet ef database update --project src/TechSub.Infraestrutura --startup-project src/TechSub.WebAPI
```

### 4. Execute a API
```bash
dotnet run --project src/TechSub.WebAPI
```

A API estará disponível em: `https://localhost:7000`

## 🧪 Como Testar a API de Relatórios

### Passo 1: Execute a API
```bash
dotnet run --project src/TechSub.WebAPI
```

### Passo 2: Registre um Usuário Admin
```http
POST https://localhost:7000/api/auth/register
Content-Type: application/json

{
  "nome": "Admin User",
  "email": "admin@techsub.com",
  "senha": "Admin123!",
  "role": "Admin"
}
```

### Passo 3: Faça Login
```http
POST https://localhost:7000/api/auth/login
Content-Type: application/json

{
  "email": "admin@techsub.com",
  "senha": "Admin123!"
}
```

### Passo 4: Teste via Swagger
1. Abra: `https://localhost:7000/swagger`
2. Copie o `token` da resposta do login
3. Clique em **"Authorize"** no Swagger
4. Digite: `Bearer {seu-token}`
5. Clique em **"Authorize"**

### Passo 5: Teste o Endpoint de Relatórios
```http
GET /api/relatorios/assinaturas-por-plano
Authorization: Bearer {seu-token}
```

**Resposta Esperada:**
```json
[
  {
    "planoNome": "Premium",
    "totalUsuariosAtivos": 15,
    "usuariosAtivos": [
      {
        "usuarioId": "guid",
        "planoNome": "Premium",
        "status": "Ativa",
        "emTrial": false,
        "dataInicio": "2024-01-15T10:30:00Z",
        "dataProximaCobranca": "2024-09-15T10:30:00Z"
      }
    ]
  }
]
```

## 🔧 Testando com Postman/Insomnia

### 1. Registrar Admin
```http
POST https://localhost:7000/api/auth/register
Content-Type: application/json

{
  "nome": "Admin User",
  "email": "admin@techsub.com",
  "senha": "Admin123!",
  "role": "Admin"
}
```

### 2. Login
```http
POST https://localhost:7000/api/auth/login
Content-Type: application/json

{
  "email": "admin@techsub.com",
  "senha": "Admin123!"
}
```

### 3. Relatório de Assinaturas
```http
GET https://localhost:7000/api/relatorios/assinaturas-por-plano
Authorization: Bearer {token-do-login}
```

## 📊 Endpoints Principais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/auth/login` | Fazer login |
| `POST` | `/api/auth/register` | Registrar usuário |
| `GET` | `/api/usuarios` | Listar usuários |
| `GET` | `/api/planos` | Listar planos |
| `GET` | `/api/assinaturas` | Listar assinaturas |
| `GET` | `/api/relatorios/assinaturas-por-plano` | **Relatório de usuários por plano** |

## 🧪 Executar Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Dados de Teste

Para ter dados nos relatórios, você pode:

1. **Registrar usuários** via `/api/auth/register`
2. **Criar planos** via `/api/planos` (Admin)
3. **Criar assinaturas** via `/api/assinaturas`
4. **Processar pagamentos** via `/api/pagamentos`

## 🏢 Para a Empresa Testar

### Cenário de Teste Completo:

1. **Execute a API**: `dotnet run --project src/TechSub.WebAPI`
2. **Registre um usuário Admin** via `/api/auth/register` (dados acima)
3. **Faça login** para obter o token JWT
4. **Acesse**: `https://localhost:7000/swagger` e autorize com o token
5. **Teste o endpoint**: `GET /api/relatorios/assinaturas-por-plano`
6. **Verifique** a listagem de usuários ativos agrupados por plano

### ⚠️ Importante:
- **Não há usuários pré-cadastrados** - você precisa registrar primeiro
- **Apenas planos são criados automaticamente** (Free, Basic, Pro)
- **Para ter dados no relatório**, crie assinaturas via API após o registro

### Validação:
- ✅ API executa sem erros
- ✅ Autenticação JWT funciona
- ✅ Endpoint retorna dados estruturados
- ✅ Usuários são agrupados por plano
- ✅ Apenas assinaturas ativas são exibidas

## 🛠️ Tecnologias

- **.NET 8** - Framework principal
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **JWT** - Autenticação
- **Swagger** - Documentação da API
- **xUnit + Moq** - Testes unitários

## ⚙️ Configuração de Segurança

### Arquivo de Configuração
- ✅ `appsettings.json` está no `.gitignore` para segurança
- ✅ Use `appsettings.example.json` como template
- ✅ Configure suas próprias credenciais localmente

### Exemplo de Configuração:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=techsub_db;Username=postgres;Password=SUA_SENHA;Port=5432"
  },
  "Authentication": {
    "JwtSettings": {
      "SecretKey": "sua-chave-jwt-secreta-com-pelo-menos-32-caracteres",
      "Issuer": "TechSub",
      "Audience": "TechSubUsers",
      "ExpirationHours": 24
    }
  }
}
```

## 📞 Suporte

Para dúvidas ou problemas, verifique:
1. Se o .NET 8 está instalado
2. Se o PostgreSQL está rodando
3. Se copiou `appsettings.example.json` para `appsettings.json`
4. Se configurou suas credenciais no `appsettings.json`
5. Se as migrations foram executadas

---

**Desenvolvido para demonstrar competências em .NET, Clean Architecture e APIs RESTful.**
