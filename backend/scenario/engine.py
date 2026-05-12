from enum import Enum 
from typing import Optional 
 
class CheckinState(str, Enum): 
    GREETING          = "greeting" 
    ID_REQUESTED      = "id_requested" 
    ID_VERIFIED       = "id_verified" 
    RESERVATION_FOUND = "reservation_found" 
    COMPLICATION      = "complication_active" 
    RESOLVED          = "complication_resolved" 
    COMPLETE          = "checkin_complete" 
 
TRANSITIONS = { 
    CheckinState.GREETING:          [CheckinState.ID_REQUESTED], 
    CheckinState.ID_REQUESTED:       [CheckinState.ID_VERIFIED], 
    CheckinState.ID_VERIFIED:        [CheckinState.RESERVATION_FOUND], 
    CheckinState.RESERVATION_FOUND:  [CheckinState.COMPLICATION, 
                                      CheckinState.COMPLETE], 
    CheckinState.COMPLICATION:       [CheckinState.RESOLVED], 
    CheckinState.RESOLVED:           [CheckinState.COMPLETE], 
} 
 
# Palabras clave que indican qué hizo el estudiante 
STATE_TRIGGERS = { 
    CheckinState.ID_REQUESTED:  ["passport", "id", "identification", "document"], 
    CheckinState.ID_VERIFIED:   ["found", "confirmed", "reservation", "booking"], 
    CheckinState.RESOLVED:      ["sorry", "upgrade", "complimentary", "include", "add"], 
    CheckinState.COMPLETE:      ["enjoy", "room", "key", "floor", "elevator"], 
} 
 
class ScenarioEngine: 
    def __init__(self, current_state: str): 
        self.state = CheckinState(current_state) 
 
    def advance(self, student_text: str) -> str: 
        text_lower = student_text.lower() 
        valid_next = TRANSITIONS.get(self.state, []) 
        for next_state in valid_next: 
            triggers = STATE_TRIGGERS.get(next_state, []) 
            if any(t in text_lower for t in triggers): 
                self.state = next_state 
                break 
        return self.state.value 
