export interface StartGameRequest {
  playerName: string;
  shipName: string;
}

export interface ResumeGameRequest {
  resumeCode: string;
}

export interface GameSessionState {
  sessionId: string;
  playerName: string;
  shipName: string;
  resumeCode: string;
  missionTitle: string;
  missionBrief: string;
  missionObjective: string;
  currentScreen: string;
  createdUtc: string;
}
