# Chess Game Image Recognition

This project provides a modular and functional system that automatically converts a sequence of chessboard images into standard algebraic notation. It combines computer vision, machine learning, and rule-based logic to interpret moves from a series of static images. The system is divided into two main services:

- **API for Chess Figure Classification** using an ML.NET model.

- **Web app Generator for Chess Notation,** which processes sequences of board states, detects changes,
and reconstructs moves with user-friendly *UI*.

# Dockerized Deployment

This project consists of two main services that work together:
- **ChessFiguresClassification_Api** – .NET 8.0 API for chess figure recognition (port `5000`)
- **ChessNotationsGenerator** – .NET 8.0 WebApp for move generation using board images (port `8080`)

These are packaged and deployed using Docker and Docker Compose, with prebuilt images hosted on Docker Hub.

---

## Quick Start 

### Prerequisites
- [Docker](https://docs.docker.com/get-docker/) installed on your system with linux containers

### 1. Clone the Repository
In commandline navigate to the directory you want this repo to be cloned.
```bash
cd <path_to_cloned_repo>
```
Then: Clone the repo.  
```bash
git clone https://github.com/lukasmodry02/BP_full_solution.git
```
It can look like the cloning is stuck.
It is running in the background give it a while.
You can check it by pressing top arrow if you see your resent commands you all set!

### 2. Start the Services
Double check if you are in the correct directory.
Folder structure should look like this.
```
ChessFiguresClassification_Api/
ChessNotationsGenerator/
docker-compose.yml
README.md
```

If not, try to go one directory deeper.

The use following command.
This pulls the published images from Docker Hub and starts the containers:

```bash
docker-compose up
```
or 
```bash
docker-compose up -d
```
if you want to start all the services in the background (detached from your terminal).

### 3. Access the App
- **Generator WebApp**: http://localhost:8080
- You can test the app with these [images](https://www.dropbox.com/scl/fo/7tarqzily7hu284d60oss/AMQnKUn6khhB9gzWWQTH_3o?rlkey=7lfmosfezsyf1wl5em06wxem7&st=jj9tpt0u&dl=0) . Every sub dir is a chess game ready to be tested.

## Compose Configuration

### Docker Compose (`docker-compose.yml`)
By default, the project uses these **Docker Hub** images:

```yaml
services:
  csharp-chessfiguresclassification_api:
    image: lukasmodry02/bp_chessgame_image_recognition:api
    ports:
      - "5000:5000"
    networks:
      - chessnet

  csharp-chessnotationsgenerator:
    image: lukasmodry02/bp_chessgame_image_recognition:generator
    ports:
      - "8080:8080"
    depends_on:
      - csharp-chessfiguresclassification_api
    networks:
      - chessnet

networks:
  chessnet:
    driver: bridge
```


---

## Docker Hub Images
- [Images](https://hub.docker.com/r/lukasmodry02/bp_chessgame_image_recognition/tags)
These are built and pushed from the latest code.

---

## Contributors
- Lukáš Modrý

---

## License
This project is licensed under the MIT License.