# Comms Project
Comms is a simplistic web messaging application built on the following tech stack:
- Frontend: Vue 3, TypeScript, Pinia, Tailwind CSS
- Backend: C# (.NET 8 Web API)
- Database: PostgreSQL

# Development
General guide to start development.
## Frontend
1. Install and setup ***Visual Studio Code IDE*** with the following extensions
   - Vue (Official)
   - Tailwind CSS IntelliSense
   - Pretty TypeScript Errors
   - ESLint
   - Stylelint
2. Run `npm install`
3. Run `npm run dev` for development
## Backend + Database
1. Install and setup ***Visual Studio IDE***
2. Install ***docker*** for running Postgres database
3. Run the following command to start a docker container for the database: `docker run -d --name comms-postgres -e POSTGRES_USER=comms_user -e POSTGRES_PASSWORD=comms_password -e POSTGRES_DB=comms_db -p 5432:5432 -v comms_postgres_data:/var/lib/postgresql/data postgres:16`
4. Any changes that update the DB schema, run `dotnet ef migrations add CommsDb_Migration_Number`, where Migration_Number is the latest migration
   - E.g `dotnet ef migrations add CommsDb_6`
