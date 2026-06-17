export interface StartGameRequest {
  playerName: string;
  shipName: string;
}

export interface ResumeGameRequest {
  resumeCode: string;
}

export interface WarpJumpRequest {
  destinationX: number;
  destinationY: number;
}

export interface GalaxySectorState {
  x: number;
  y: number;
  enemyCount: number;
  planetCount: number;
  baseCount: number;
  visited: boolean;
}

export interface GalaxyMapState {
  width: number;
  height: number;
  jumpRange: number;
  currentX: number;
  currentY: number;
  completedTurns: number;
  actionUsed: boolean;
  movementUsed: boolean;
  sectors: GalaxySectorState[];
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
  galaxyMap: GalaxyMapState;
}
