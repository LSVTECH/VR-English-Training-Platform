from fastapi import APIRouter

router = APIRouter()

@router.get("/demo")
async def demo_response():
    return {
        "transcription": "mock transcription of the provided audio",
        "ai_response_text": "This is a mock AI response for testing purposes.",
        "audio_url": "/static/audio/mock.mp3",
        "scoring": {"grammar_score": 0.9, "vocabulary_score": 0.8, "professionalism_score": 1.0, "grammar_errors": [], "suggested_phrase": "mock transcription of the provided audio"}
    }
