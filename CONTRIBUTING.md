# Contributing

Thank you for your interest in contributing to Comms!

This project uses **GitHub Actions** for CI to ensure code quality and integrity.
Below are the prerequisites and the suggested workflow for development.

## Prerequisites

- **Docker** (for running Postgres)
- **.NET 8 SDK** (for the backend)
- **Node.js** (for frontend development)

> Any IDE or editor that supports the relevant tooling can be used.

## Backend & Database Setup

The database runs in docker.

1. Create a `.env` file based on `.env.template` and fill in the required variables
2. Start the database:
   ```bash
   docker compose up postgres -d
   ```
3. Update the connection string in `Comms-Server/Comms-Server/appsettings.Development.json` to match your `.env` credentials (`DATABASE_USER`, `DATABASE_PASSWORD`)
4. Run the backend:
   ```bash
   dotnet run --project Comms-Server/Comms-Server
   ```
5. Logs are written to the console. Wire up your own sink (Seq, a file, a log aggregator, etc.) in `appsettings.Development.json` if you want structured/centralized logging.

## Frontend Setup

1. Install and setup ***Visual Studio Code*** (recommended) with the following extensions
   - Vue (Official)
   - Vitest
   - Tailwind CSS IntelliSense
   - Pretty TypeScript Errors
   - ESLint
   - Stylelint
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm run dev
   ```

## Database Migrations

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
5. Address any failing checks. Once all checks pass, a maintainer will review and merge your PR
