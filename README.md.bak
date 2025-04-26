## Running the Project with Docker

This project consists of two main .NET 8.0 services, each with its own Dockerfile:

- **ChessFiguresClassification_Api** (exposes port `5000`)
- **ChessNotationsGenerator** (exposes port `5002`)

Both services are orchestrated using Docker Compose and are connected via a custom bridge network (`chessnet`).

### Requirements
- **Docker** and **Docker Compose** installed
- **.NET 8.0** is specified in the Dockerfiles (images: `mcr.microsoft.com/dotnet/aspnet:8.0` and `mcr.microsoft.com/dotnet/sdk:8.0`)

### Environment Variables
- No required environment variables are specified by default in the Dockerfiles or `docker-compose.yml`.
- If you need to use environment variables, you can create `.env` files in each service directory and uncomment the `env_file` lines in the compose file.

### Build and Run Instructions
1. From the root of the repository, run:
   ```sh
   docker compose up --build
   ```
   This will build and start both services.

2. Access the services at:
   - **ChessFiguresClassification_Api**: http://localhost:5000
   - **ChessNotationsGenerator**: http://localhost:5002

### Ports
- `5000`: ChessFiguresClassification_Api
- `5002`: ChessNotationsGenerator

### Special Configuration
- Both services run as non-root users inside their containers for improved security.
- Both services are attached to the `chessnet` Docker network for internal communication.
- If you need to add service dependencies, use the `depends_on` section in the compose file.

---

_This section was updated to reflect the current Docker-based setup for this project. For further details, see the `docker-compose.yml` and individual Dockerfiles in each service directory._
