"""
FastAPI service wrapping the Whisper speech‑to‑text model.  The service exposes a single
`POST /transcribe` endpoint that accepts an audio file and returns the recognised text.  It
demonstrates how to integrate a Python machine‑learning model into a microservice that can
be called from a .NET application.

For simplicity this example uses Whisper's CPU model.  For production use you should
consider running on a GPU and applying VAD (voice activity detection) to trim silence.
"""

from fastapi import FastAPI, UploadFile, File
from pydub import AudioSegment
import io
import whisper

app = FastAPI(title="EchoTravel STT")

# Load the Whisper model once at startup.  You can choose another size, e.g. "small" or "medium".
model = whisper.load_model("base")


@app.post("/transcribe")
async def transcribe(file: UploadFile = File(...)):
    """Transcribe an uploaded audio file into text.  Returns JSON with the recognised text and
    language code.
    """
    raw = await file.read()
    # Convert any audio format into 16 kHz mono WAV using pydub
    audio = AudioSegment.from_file(io.BytesIO(raw))
    audio = audio.set_channels(1).set_frame_rate(16000)
    buf = io.BytesIO()
    audio.export(buf, format="wav")
    wav_data = buf.getvalue()
    # Write to a temp file because whisper.load_model expects a file path
    tmp_path = "tmp.wav"
    with open(tmp_path, "wb") as f:
        f.write(wav_data)
    result = model.transcribe(tmp_path, language=None)  # auto‑detect language
    text = result.get("text", "").strip()
    return {"text": text, "lang": result.get("language", "unknown")}
