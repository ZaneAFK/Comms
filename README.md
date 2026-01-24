# Comms Project
Comms is a simplistic web messaging application built on the following tech stack:
- Frontend: Vue 3, TypeScript, Pinia, Tailwind CSS
- Backend: C# (.NET ASP Web API)
- Database: PostgreSQL

# Development
General guide to start development.
## Workflow
GitHub Actions is used for CI to ensure code quality and integrity. Below is the suggested workflow of creating a change.
1. Create a feature branch off `master` with your changes
2. Open a PR as a **draft** while your feature is in progress
3. Mark the PR as *Ready for review* once you have finished your work. This will trigger the CI pipeline and runs:
   - Unit tests
   - Build checks
   - Linting and style checks (where applicable)
4. Address failing checks and merge into `master` once reviewed
## Frontend
1. Install and setup ***Visual Studio Code IDE*** with the following extensions
   - Vue (Official)
   - Vitest
   - Tailwind CSS IntelliSense
   - Pretty TypeScript Errors
   - ESLint
   - Stylelint
2. Run `npm install`
3. Run `npm run dev` for development
## Backend + Database
1. Install and setup ***Visual Studio IDE***
2. Install ***docker*** for running Postgres database
3. Create a `.env` file based on `.env.template` and fill in the relevant variables
4. Start the Postgres database using Docker Compose:
   - `docker compose up -d`
5. Any changes that update the database schema, run migrations locally with:
   - `dotnet ef migrations add CommsDb_<Migration_Number>`. Replace <Migration_Number> with the latest migration identifier
   - E.g `dotnet ef migrations add CommsDb_6`
