# ScrabbleSharp

ScrabbleSharp is a high-performance, feature-rich Scrabble solver and word finder. It combines a powerful .NET backend engine with an interactive React frontend, providing a fine tool for Scrabble enthusiasts and word game players.

The application supports multiple game modes, including **Classic Scrabble**, **Super Scrabble**, **Scrabble Duel**, and **Letter League Classic**.

**[Live Demo](https://scrabble-sharp.bisocm.org/)** 🚀

-----

## Features

  - **⚡ High-Performance Solver:** The core engine is written in C\# and leverages parallel processing and optimized algorithms to find the best possible moves in milliseconds.
  - **🧩 Multiple Game Modes:** Out-of-the-box support for various board layouts and rules:
      - Scrabble Classic (15x15)
      - Super Scrabble (21x21)
      - Scrabble Duel (11x11)
      - Letter League (15x15, expandable)
  - **🗺️ Expandable Board:** A unique feature for the "Letter League" mode where the board can be expanded dynamically.
  - **📡 Efficient API:** Uses gRPC-Web for fast, strongly-typed communication between the frontend and backend.

-----

## Tech Stack

ScrabbleSharp is a full-stack application composed of two main parts: a .NET backend and a React frontend.

| Component         | Technology                                                                                                  | Description                                                                 |
| ----------------- | ----------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| **Backend** | C\# 12, .NET 8, ASP.NET Core                                                                                 | Hosts the gRPC service and contains the core game engine logic.             |
| **Frontend** | TypeScript, React 18, Vite, Redux Toolkit, Tailwind CSS                                                     | The client-side application that users interact with in the browser.        |
| **API** | [gRPC-Web](https://github.com/grpc/grpc-web), [Connect-RPC](https://connectrpc.com/)                          | Defines the contract for efficient, low-latency client-server communication. |
| **State Mgt** | [Redux Toolkit](https://redux-toolkit.js.org/)                                                              | Manages the frontend application state, persisted to `localStorage`.         |
| **Styling** | [Tailwind CSS](https://tailwindcss.com/)                                                                    | A utility-first CSS framework for rapid UI development.                     |

-----

## Project Structure

This is a mono-repository, housing both the frontend and the backend of the application. It is organized into several key projects:

```
/
├── ScrabbleSharp.Contracts/   # Protobuf definitions for the gRPC API.
├── ScrabbleSharp.Engine/      # The core solver logic, game rules, and board layouts.
├── ScrabbleSharp.Gateway/     # The ASP.NET Core backend hosting the gRPC-Web service.
└── ScrabbleSharp.Frontend/    # The React/Vite frontend application.
```

Every major component - **Gateway** and **Frontend** feature dedicated Dockerfiles for ease of deployment.

-----

## Getting Started

Follow these instructions to get a local copy up and running for development and testing.

### Prerequisites

  - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  - [Node.js and npm](https://nodejs.org/en/) (LTS version recommended)

### Backend Setup (Gateway)

1.  Navigate to the Gateway directory:
    ```bash
    cd ScrabbleSharp.Gateway
    ```
2.  Restore dependencies:
    ```bash
    dotnet restore
    ```
3.  Run the backend service:
    ```bash
    dotnet run
    ```
    The gRPC service will be running at `http://localhost:5144`.

### Frontend Setup

1.  Navigate to the frontend directory:
    ```bash
    cd ScrabbleSharp.Frontend/scrabblesharp-frontend
    ```
2.  Install npm packages:
    ```bash
    npm install
    ```
3.  Run the development server:
    ```bash
    npm run dev
    ```
    The frontend application will be available at `http://localhost:5173` (or another port if 5173 is in use). The app is configured to connect to the local backend by default. It uses `env.production` if compiled to Docker via the provided Dockerfile.

## API Overview

Communication is handled via a gRPC service defined in `ScrabbleSharp.proto`.

### Services

  - `rpc Solve(SolveRequest) returns (SolveResponse)`
      - The primary endpoint for finding moves.
      - **Request:** Contains the current `board` state as a newline-separated string and the player's `rack`.
      - **Response:** A list of all valid `Move` objects, including word, position, score, and definition.
  - `rpc GetLayout(LayoutRequest) returns (LayoutResponse)`
      - Fetches the initial board size and multiplier layout for a given game mode.
  - `rpc Expand(ExpandRequest) returns (ExpandDelta)`
      - Requests the expansion of the board in a specific `Direction`.
      - **Response:** Returns only the *newly added slice* of the board (`ExpandDelta`), including its dimensions, offset, and multipliers, along with the new total board dimensions. This minimizes payload size.

### Custom Headers

The gRPC context is enriched with custom headers to manage game state without complex request bodies:

  - `x-mode`: Specifies the active game mode (e.g., `letterleague_classic`).
  - `x-up`, `x-down`, `x-left`, `x-right`: Tell the server how many times the board has already been expanded in each direction, ensuring a correct layout is generated for the `Solve` call.

-----

## Contributing

Contributions are welcome\! Whether you're fixing a bug, improving a feature, or suggesting a new idea, your help is appreciated. Please feel free to open an issue or submit a pull request.

-----

## License

This project is distributed under the MIT License. See `LICENSE.txt` for more information.