# PROXY_TODB API

### ⚠️ Important Notice

This program has been largely generated with the assistance of AI and is part of a broader experimental project.

The primary goal of this project is to simulate a real-world DevOps workflow, including building, deploying, and maintaining applications within a controlled environment. As such, the focus is placed on infrastructure, automation, and process implementation rather than on code quality or adherence to software engineering best practices.

Please note that:
- The codebase may not follow established design patterns or coding standards
- Certain implementations are intentionally simplified or non-optimal
- The structure and logic are designed to support DevOps experimentation rather than production use

This project should be treated as a **learning and testing environment**, not as a reference for production-grade development.
A simple RESTful API for managing temporary databases and executing SQL scripts. Built with ASP.NET Core (.NET 8, C# 12).

## Endpoints

### 1. `GET /Home/CreateDB`

Creates a new temporary database.

- **Response:**
{ "Message": "Database created successfully.", "Hash": "unique-database-hash" }

- **Usage:**  
  Call this endpoint to create a new database. The response includes a unique hash to reference the database in future requests.

---

### 2. `DELETE /Home/DeleteDB?uniquehash={hash}`

Deletes a database identified by its unique hash.

- **Parameters:**
  - `uniquehash` (string): The hash of the database to delete.
- **Response:**  
  Returns a message indicating the result of the deletion.
- **Usage:**  
  Use the hash received from `CreateDB` to delete the corresponding database.

---

### 3. `POST /Home/ExecuteScript`

Executes a SQL script against a database identified by its hash.

- **Request Body:**
  { "Hash": "unique-database-hash", "Script": "SQL script to execute" }
  - **Response:**
  - { "Message": "Result of script execution.", "Success": true }
- **Usage:**  
  Send the hash and the SQL script you want to execute. The response indicates if the execution was successful and provides any output or error message.

---

### 4. `GET /Home/Hello`

Simple health check endpoint.

- **Response:**  
  `"I am working"`
- **Usage:**  
  Use this endpoint to verify that the API is running.

---
### 📄 `settings.env` structure

```env
masterconn=Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres;
host=localhost
port=5432
managementdb=ManagementDB
```

## 🔑 Parameters explained

- **`masterconn`**  
  Connection string for the main (head) PostgreSQL database.  
  ⚠️ Must use **admin-level credentials**, as it is required for creating and managing databases.

- **`host`**  
  Database host.  
  Must match the host specified in `masterconn`.

- **`port`**  
  Database port (default: `5432`).

- **`managementdb`**  
  Name of the management database where all service-related data is stored.

---
gitlab-ci and etc files for ci-cd pipeline, dont touch them
TEST_PROXY is a integration test code 

## How to run locally? (docker-compose.yaml file)

1. Build code locally

```bash
dotnet publish -c Release -o publish
```

2. Build docker image using Dockerfile. Dockerfile should be placed where the `publish` folder is.

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app

COPY publish/ ./

RUN apk add --no-cache curl

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "YOUR_PROJ_DLL"]
```

Example:

```bash
docker build -t proxyapp .
```

3. Create `docker-compose.yaml`

```yaml
services:
  proxydb:
    container_name: proxy_app_deploy
    image: proxyapp
    env_file:
      - settings.env
    depends_on:
      pgdb:
        condition: service_healthy
    networks:
      - deploy_net
    ports:
      - 8080:8080

  pgdb:
    container_name: db_postgres_deploy
    image: postgres:14.22
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: postgres
    healthcheck:
      test: ["CMD", "pg_isready", "-q", "-d", "postgres", "-U", "postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    volumes:
      - db_data:/var/lib/postgresql/data
    networks:
      - deploy_net

volumes:
  db_data:

networks:
  deploy_net:
```

4. Run containers

```bash
docker compose up -d
```

⚠️ Make sure you changed the database hostname in `settings.env`.

Other files and full proj structure are explained in the **[Main project](https://github.com/StealLine/GITLAB-CI-PROJECT)**.
