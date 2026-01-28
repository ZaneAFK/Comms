# Comms Project

Comms is a simple web-based messaging application built with the following tech stack:
- **Frontend**: Vue 3, TypeScript, Pinia, Tailwind CSS
- **Backend**: C# (.NET 8 ASP.NET Web API)
- **Database**: PostgreSQL

---

## Contributing

Thank you for your interest in contributing to Comms!

This project uses **GitHub Actions** for CI to ensure code quality and integrity.
Below are the prerequisites and the suggested workflow for development.

### Prequisities

- **.NET SDK 8.0**
- **Docker** (for running container with database)
- **Node.js** (for frontend development)

> Any IDE or editor that supports the relevant tooling can be used.

---

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
3. Start the development server
   - Run `npm run dev`
### Backend - Suggested Setup
1. Ensure **.NET SDK 8.0** and **Docker** is installed
2. Create a `.env` file based on `.env.template` and fill in the required variables
3. Start the Postgres database using Docker Compose:
   - `docker compose up -d`
4. If you make changes that update the database schema, create a new migration:
   - `dotnet ef migrations add CommsDb_<Migration_Number>`
   - Replace <Migration_Number> with the latest migration identifier
   - E.g `dotnet ef migrations add CommsDb_6`
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
