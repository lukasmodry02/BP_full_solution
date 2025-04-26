# ChessNotionsWebApp
Web application for generating chess notation from images of the chess game/ its states.

## Running the Project with Docker

To run this project using Docker, follow these steps:

### Prerequisites
- Ensure Docker and Docker Compose are installed on your system.
- The project uses .NET version 8.0 as specified in the Dockerfile.

### Build and Run Instructions
1. Clone the repository and navigate to the project directory.
2. Build and start the services using Docker Compose:
   ```bash
   docker-compose up --build
   ```
3. Access the application at `http://localhost:5002` in your web browser.

### Configuration
- The application exposes port `5002` as defined in the Docker Compose file.
- Modify the `docker-compose.yml` file to adjust configurations if necessary.

For further details, refer to the Dockerfile and Docker Compose file included in the project.