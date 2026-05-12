from openai import AsyncOpenAI 
import json 
import os

client = AsyncOpenAI() 
MOCK_MODE = os.getenv("MOCK_MODE", "false").lower() == "true"
 
EVALUATOR_PROMPT = """ 
Eres un evaluador de inglés para profesionales de hostelería. 
Recibirás una frase dicha por un estudiante de nivel {level}. 
Evalúa en formato JSON con estas claves: 
  - grammar_score: float 0.0-1.0 
  - vocabulary_score: float 0.0-1.0 
  - professionalism_score: float 0.0-1.0 
  - grammar_errors: lista de strings con los errores (máx 3) 
  - suggested_phrase: versión mejorada de la frase del estudiante 
Responde SOLO con el JSON. Sin texto adicional. 
""" 
 
async def evaluate_turn(student_text: str, state: str, level: str = "B1") -> dict: 
    if MOCK_MODE:
        return {
            "grammar_score": 0.9,
            "vocabulary_score": 0.8,
            "professionalism_score": 1.0,
            "grammar_errors": [],
            "suggested_phrase": student_text
        }
    
    response = await client.chat.completions.create( 
        model="gpt-4o-mini", 
        response_format={"type": "json_object"}, 
        messages=[ 
            {"role": "system", "content": EVALUATOR_PROMPT.format(level=level)}, 
            {"role": "user", "content": f"Frase del estudiante: '{student_text}'\nEstado del escenario: {state}"} 
        ] 
    ) 
    return json.loads(response.choices[0].message.content) 
