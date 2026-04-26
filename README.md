# Comms Project

Comms is a simple web-based messaging application built with the following tech stack:
- **Frontend**: Vue 3, TypeScript, Pinia, Tailwind CSS
- **Backend**: C# (.NET 8 ASP.NET Web API)
- **Database**: PostgreSQL
- **Logging**: Serilog + Seq
- **Containerization**: Docker

---

## Contributing

Thank you for your interest in contributing to Comms!

This project uses **GitHub Actions** for CI to ensure code quality and integrity.
Below are the prerequisites and the suggested workflow for development.

### Prerequisites

- **Docker** (for running the backend and database)
- **Node.js** (for frontend development)

> Any IDE or editor that supports the relevant tooling can be used.

---

### Backend & Database Setup

The backend and database both run in Docker containers.

1. Create a `.env` file based on `.env.template` and fill in the required variables
2. Start the containers:
   - `docker compose up -d backend`
   - This also starts `postgres` and `seq` automatically
3. The backend runs with hot reload via `dotnet watch` — changes to the source are picked up automatically
4. Structured logs are viewable in Seq at `http://localhost:5341`

### Frontend - Suggested Setup

1. Install and setup ***Visual Studio Code*** (recommended) with the following extensions
   - Vue (Official)
   - Vitest
   - Tailwind CSS IntelliSense
   - Pretty TypeScript Errors
   - ESLint
   - Stylelint
2. Install dependencies:
   - Run `npm install`
3. Start the development server:
   - Run `npm run dev`
4. The Vite dev server proxies `/api` and `/hubs` to the backend container automatically

### Database Migrations

If you make changes that update the database schema, create a new migration:

```
dotnet ef migrations add CommsDb_<Migration_Number>
```

Replace `<Migration_Number>` with the next migration number (e.g. `CommsDb_6`).
## Suggested Workflow
1. Fork the repository
1. Create a feature branch on your fork
2. Open a Pull Request **as a draft** while your feature is in progress
3. Mark the PR as *Ready for review* once your work is complete
4. When a PR is marked ready, the CI pipeline will run:
   - Unit tests
   - Build checks
   - Linting and style checks (where applicable)
5. Address failing checks and merge once all checks are passed
