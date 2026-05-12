from fastapi import APIRouter, UploadFile, Form, HTTPException 
from services.whisper_service import transcribe_audio 
from services.openai_service import get_ai_response 
from services.tts_service import text_to_speech 
from services.scoring_service import evaluate_turn 
from scenario.engine import ScenarioEngine 
from database.redis_client import get_session, update_session 
import asyncio 
 
router = APIRouter() 
 
@router.post("/conversation") 
async def process_turn( 
    audio: UploadFile, 
    session_id: str = Form(...), 
    turn_number: int = Form(...), 
): 
    # 1. Obtener contexto de Redis 
    session = await get_session(session_id) 
    if not session: 
        raise HTTPException(404, "Sesión no encontrada") 
 
    # 2. Transcribir audio en paralelo con otras tareas 
    audio_bytes = await audio.read() 
    transcription = await transcribe_audio(audio_bytes) 
 
    # 3. Construir historial de conversación 
    history = session["history"] 
    history.append({"role": "user", "content": transcription}) 
 
    # 4. Llamar a GPT y al scoring en paralelo 
    ai_text, score = await asyncio.gather( 
        get_ai_response(history, session["level"], session["scenario_state"]), 
        evaluate_turn(transcription, session["scenario_state"]) 
    ) 
 
    # 5. Convertir respuesta a audio (streaming) 
    audio_url = await text_to_speech(ai_text, session_id) 
 
    # 6. Actualizar estado del escenario 
    engine = ScenarioEngine(session["scenario_state"]) 
    new_state = engine.advance(transcription) 
 
    # 7. Guardar contexto actualizado en Redis 
    history.append({"role": "assistant", "content": ai_text}) 
    await update_session(session_id, { 
        "history": history, 
        "scenario_state": new_state, 
        "last_score": score 
    }) 
 
    return { 
        "transcription": transcription, 
        "ai_response_text": ai_text, 
        "audio_url": audio_url, 
        "score": score.get("points", 0), 
        "feedback": score.get("feedback", ""),
        "scenario_state": new_state 
    } 

@router.post("/conversation_text") 
async def process_turn_text( 
    text: str = Form(...), 
    session_id: str = Form(...), 
    turn_number: int = Form(...), 
): 
    try:
        session = await get_session(session_id) 
        if not session: 
            # Stub para pruebas si no hay sesion real
            session = {"history": [], "level": "B1", "scenario_state": "greeting"}
            
        history = session["history"] 
        history.append({"role": "user", "content": text}) 
        
        ai_text, score = await asyncio.gather( 
            get_ai_response(history, session.get("level", "B1"), session.get("scenario_state", "greeting")), 
            evaluate_turn(text, session.get("scenario_state", "greeting")) 
        ) 
        
        audio_url = await text_to_speech(ai_text, session_id) 
        
        engine = ScenarioEngine(session.get("scenario_state", "greeting")) 
        new_state = engine.advance(text) 
        
        history.append({"role": "assistant", "content": ai_text}) 
        await update_session(session_id, { 
            "history": history, 
            "scenario_state": new_state, 
            "last_score": score 
        }) 
        
        return { 
            "transcription": text, 
            "ai_response_text": ai_text, 
            "audio_url": audio_url, 
            "score": score.get("points", 0), 
            "feedback": score.get("feedback", ""),
            "scenario_state": new_state 
        }
    except Exception as e:
        return {"error": str(e)}
