
# 🏋️‍♂️ FitTrack - Sistema de Acompanhamento Fitness

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%2523-11.0-239120?logo=c-sharp)
![MongoDB](https://img.shields.io/badge/MongoDB-7.0-47A248?logo=mongodb)
![JWT Authentication](https://img.shields.io/badge/JWT-Authentication-000000?logo=json-web-tokens)
![Swagger Documentation](https://img.shields.io/badge/Swagger-Documentation-85EA2D?logo=swagger)

Sistema completo de acompanhamento físico com geração automática de planos de treino

## 📋 Sobre o Projeto
O FitTrack é uma API RESTful desenvolvida em .NET 9 para acompanhamento físico completo. O sistema oferece desde cadastro de usuários até geração automática de planos de treino personalizados, integração com catálogo de exercícios e dashboard analítico.

## FRONTEND
[fit-track](https://github.com/mauridf/fit-track)

## ✨ Destaques
- Geração automática de planos de treino baseados no perfil do usuário
- Integração com ExerciseDB - Catálogo com +500 exercícios
- Cálculos inteligentes - IMC, peso ideal, progresso automático
- Dashboard completo - Métricas, estatísticas e recomendações
- Autenticação segura - JWT com refresh tokens
- API documentada - Swagger/OpenAPI integrado

## 🛠 Tecnologias
- Backend: .NET 9, C# 11
- Database: MongoDB 7.0+
- Autenticação: JWT + BCrypt
- API Externa: ExerciseDB (RapidAPI)
- Documentação: Swagger/OpenAPI
- Containerização: Docker-ready

## 📊 Funcionalidades

### 🔐 Autenticação & Segurança
- Registro e login de usuários
- JWT com refresh tokens
- Proteção de endpoints com [Authorize]
- Hash de senhas com BCrypt

### 👤 Gestão de Usuários
- Perfil completo com dados físicos
- Cálculos automáticos de IMC e peso ideal
- Histórico de medidas (peso, etc.)
- Atualização e exclusão de conta

### 💪 Sistema de Exercícios
- Catálogo com integração ExerciseDB API
- Exercícios customizados por usuário
- Busca por equipamento, parte do corpo, músculo
- Sincronização automática com API externa

### 📅 Planos de Treino
- Geração automática baseada no perfil
- Progressão semanal inteligente
- Adaptação por equipamentos disponíveis
- Múltiplas sessões por semana

### 🏋️‍♂️ Sistema de Sessões
- Registro de treinos executados
- Cálculo automático de calorias
- Estatísticas de performance
- Histórico completo de treinos

### 📈 Dashboard & Analytics
- Visão geral do progresso
- Estatísticas de treino
- Progresso de metas
- Recomendações personalizadas
- Métricas em tempo real

## 🚀 Começando

### Pré-requisitos
- .NET 9 SDK
- MongoDB (local ou Atlas)
- Git

### Instalação
Clone o repositório:

```bash
git clone https://github.com/seu-usuario/fittrack.git
cd fittrack
```

Configure o MongoDB (`appsettings.json`):

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "fittrack"
  }
}
```

Configure a API Key do ExerciseDB (Opcional):

```bash
# Obtenha uma API Key gratuita em:
# https://rapidapi.com/justin-WFnsXH_t6/api/exercisedb
```

Execute a aplicação:

```bash
dotnet run --project src/FitTrack.API
```

Acesse a documentação: `https://localhost:7070`

## 📚 Documentação

### 🔐 Autenticação
- Registro de Usuário: `POST /api/v1/auth/register`
- Login: `POST /api/v1/auth/login`

### 💪 Exercícios
- Buscar Exercícios: `GET /api/v1/exercises?equipment=esteira&bodyPart=cardio`
- Sincronizar com ExerciseDB: `POST /api/v1/exercises/sync`

### 📅 Planos de Treino
- Gerar Plano Automático: `POST /api/v1/training-plans/generate`

### 🏋️‍♂️ Sessões de Treino
- Registrar Sessão: `POST /api/v1/training-sessions`

### 📊 Dashboard
- Visão Geral: `GET /api/v1/dashboard/overview`
- Progresso de Peso: `GET /api/v1/dashboard/weight-progress?days=30`

## 🏗 Estrutura do Projeto
```
FitTrack/
├── src/
│   ├── FitTrack.API/              # Camada de Apresentação
│   │   ├── Controllers/           # Controladores REST
│   │   ├── Program.cs             # Configuração da App
│   │   └── Properties/
│   ├── FitTrack.Core/             # Camada de Domínio
│   │   ├── Entities/              # Entidades de Domínio
│   │   ├── DTOs/                  # Data Transfer Objects
│   │   └── Interfaces/            # Contratos de Serviços
│   └── FitTrack.Infrastructure/   # Camada de Infraestrutura
│       ├── Repositories/          # Implementações MongoDB
│       ├── Services/              # Serviços de Domínio
│       └── Data/                  # Contexto do MongoDB

```

## 🔧 Configuração

### Variáveis de Ambiente
```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "fittrack"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-super-segura-aqui",
    "Issuer": "fittrack-api",
    "Audience": "fittrack-users",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "ExerciseDb": {
    "ApiKey": "sua-chave-rapidapi"
  }
}
```

### Desenvolvimento
```bash
# Desenvolvimento com hot reload
dotnet watch run --project src/FitTrack.API

# Executar testes
dotnet test

# Build de produção
dotnet publish -c Release
```

## 🤝 Contribuindo
1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença
Este projeto está sob a licença MIT. Veja o arquivo LICENSE para detalhes.

## 👥 Autores
Maurício Oliveira - Desenvolvimento Inicial - mauridf

## 🙏 Agradecimentos
- ExerciseDB - API de exercícios
- MongoDB - Banco de dados
- .NET - Framework

Feito com 💪 e ☕
