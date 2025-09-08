# Real‑time Multilingual Train Announcements

This project demonstrates how to build a real‑time, multilingual train announcement system using a **.NET** backend, a **Python** microservice for speech‑to‑text and a simple web frontend.  Passengers scan a QR code in their carriage to receive live announcements translated into their preferred language.

## Key Features

* **.NET API and SignalR** — The core of the system is an ASP.NET Core web API.  It exposes an endpoint to accept recorded audio of train announcements.  After transcribing and translating, it uses SignalR to push messages in real‑time to all passengers subscribed to the relevant train/carriage group.  Groups are named using a simple convention (`train.{trainId}[.car.{carId}]`) so each carriage receives only its own announcements.
* **Python speech‑to‑text microservice** — A separate FastAPI service runs Whisper (an automatic speech‑to‑text model) with optional voice activity detection.  The .NET API calls this service through an interface (`ISttClient`) and adapter (`FastApiSttClient`) so the STT implementation can be swapped without changing business logic.
* **Translation via Azure Cognitive Services** — The recognised text is translated into the passenger’s chosen language using Azure Translator.  The translation service is encapsulated in a separate `Translator` class, making it easy to replace with another provider or to add caching.
* **Port/adapter architecture** — The backend code follows a hexagonal (port‑adapter) pattern.  A clear boundary is drawn between domain logic (`AnnouncementsService`) and infrastructure (STT and translation).  This separation simplifies testing and future enhancements.
* **Client‑side web app** — A static web page served from the API’s `wwwroot` folder subscribes to train announcement groups via SignalR.  Passengers access the page by scanning a QR code that encodes the train/car IDs.

## Repository Layout

```
echotravel/
├── EchoTravel.Api/                # ASP.NET Core backend
│   ├── Controllers/
│   │   └── IngestController.cs    # HTTP endpoint for uploading audio
│   ├── Hubs/
│   │   └── AnnouncementsHub.cs    # SignalR hub to join train groups and receive messages
│   ├── Ports/
│   │   └── ISttClient.cs          # Abstraction for speech‑to‑text clients
│   ├── Adapters/
│   │   └── FastApiSttClient.cs    # HTTP adapter calling the Python STT service
│   ├── Services/
│   │   ├── AnnouncementsService.cs # Business logic: transcribe, translate and broadcast
│   │   └── Translator.cs          # Encapsulates translation via Azure Cognitive Services
│   ├── wwwroot/
│   │   └── index.html             # Simple PWA‑style client using SignalR
│   ├── Program.cs                 # Application entry point and dependency registration
│   ├── EchoTravel.Api.csproj      # Minimal .NET project file
│   └── appsettings.Development.json # Configuration for STT and Translator endpoints/keys
├── stt_service/
│   ├── main.py                    # FastAPI service wrapping Whisper for speech‑to‑text
│   └── requirements.txt           # Python dependencies
└── .gitignore
```

## Running the Project Locally

1. **Start the Python STT service.**  The service depends on [`ffmpeg`](https://ffmpeg.org/) and Python 3.10+.  Create a virtual environment, install dependencies and run the service:

   ```bash
   cd stt_service
   python -m venv .venv
   source .venv/bin/activate
   pip install -r requirements.txt
   uvicorn main:app --host 0.0.0.0 --port 8001
   ```

   Whisper can run on CPU for small demos but you can switch to a GPU model (e.g. `small` or `medium`) by passing `device=\"cuda\"` in `main.py`.

2. **Configure and run the .NET API.**  Install the .NET 8 SDK and set up your Azure Translator key in `EchoTravel.Api/appsettings.Development.json`.  Then build and run:

   ```bash
   cd EchoTravel.Api
   dotnet run
   ```

   The API listens on `http://localhost:5000`.  It forwards uploaded audio to the STT service (`http://localhost:8001`) and broadcasts the translated announcements via SignalR.

3. **Open the client in a browser.**  Navigate to `http://localhost:5000/?trainId=IC1234&car=12` (replace with your own train and car IDs).  When you `POST` an audio file to `/api/ingest/{trainId}?car={car}&to=en`, the recognised and translated text appears instantly on the page.

## Skills Demonstrated

This project highlights several important technologies and concepts:

* **ASP.NET Core & C#** — building RESTful APIs, dependency injection, configuration, middleware and SignalR for real‑time messaging.
* **Python & FastAPI** — creating a microservice around a machine‑learning model (Whisper) with minimal overhead.
* **Microservices & integration** — decoupling services via HTTP, designing contracts (JSON over HTTP) and using adapters to isolate infrastructure changes.
* **Azure Translator** — consuming external SaaS APIs responsibly, including key management and asynchronous calls.
* **Port/adapter (hexagonal) architecture** — emphasising clean separation between core business logic and external dependencies, enabling easier testing and evolution.
* **Real‑time web clients** — using SignalR in JavaScript to receive server‑pushed messages and keep multiple clients in sync.
* **Multilingual domain focus** — solving a real problem for passengers by translating dynamic content into multiple languages in near real‑time.
