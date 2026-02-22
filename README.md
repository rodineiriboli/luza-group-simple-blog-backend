# Teste Pratico C# - Blog Simples

Este workspace contem dois projetos separados:

- `backend/`: API ASP.NET Core (.NET 8) + PostgreSQL + SignalR
- `frontend/`: cliente React/Vite para testar autenticacao, CRUD e notificacoes em tempo real

Este README foi escrito para um avaliador executar o projeto com o menor atrito possivel.

## 1) Pre-requisitos

- Docker Desktop + Docker Compose
- Node.js LTS (18+)
- (Opcional) .NET SDK 8+ para rodar a API fora de container

## 2) Execucao rapida (recomendada)

### 2.1 Subir backend (API + banco)

```bash
cd backend
docker compose up -d --build
```

Validar backend:

- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`

### 2.2 Subir frontend

Em outro terminal:

```bash
cd frontend
npm install
```

Copiar variaveis de ambiente:

Linux/macOS:
```bash
cp .env.example .env
```

PowerShell:
```powershell
Copy-Item .env.example .env
```

Rodar frontend:

```bash
npm run dev
```

Abrir: `http://localhost:5173`

## 3) Como validar o teste rapidamente

1. Acesse `http://localhost:5173`
2. Registre um usuario novo
3. Faca login
4. Crie um post
5. Edite o mesmo post
6. Exclua o post
7. Abra duas abas do frontend, crie um novo post em uma e verifique notificacao em tempo real na outra

## 4) Endpoints principais (backend)

Auth:
- `POST /api/auth/register`
- `POST /api/auth/login`

Posts:
- `GET /api/posts?page=1&pageSize=10` (publico)
- `GET /api/posts/{id}` (publico)
- `POST /api/posts` (auth)
- `PUT /api/posts/{id}` (auth)
- `DELETE /api/posts/{id}` (auth)

SignalR:
- Hub: `/hubs/notifications`
- Evento: `PostCreated`

## 5) Teste por arquivo .http

Existe um arquivo pronto em:

- `backend/requests/blog.http`

Pode ser usado no VS Code (REST Client) para validar registro/login/CRUD.

## 6) Observacoes importantes para avaliacao

- Arquitetura monolitica em camadas no backend (`Domain`, `Application`, `Infrastructure`, `Api`)
- EF Core com migrations e indices
- JWT para autenticacao
- Regra de autorizacao para modificar postagens
- Notificacao realtime via SignalR ao criar postagem

## 7) Parar ambiente

```bash
cd backend
docker compose down
```

Para remover volumes do banco:

```bash
cd backend
docker compose down -v
```

## 8) Troubleshooting rapido

- Se `8080` ou `5432` estiverem em uso, pare processos/containers que ocupam essas portas.
- Se frontend nao conectar na API, confirme que `frontend/.env` aponta para `http://localhost:8080`.
- Se Swagger nao abrir, confirme que `backend` subiu sem erro:
  - `docker compose ps` dentro de `backend/`
