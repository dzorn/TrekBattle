export interface StartGameRequest {
  playerName: string;
  shipName: string;
}

export interface ResumeGameRequest {
  resumeCode: string;
}

export interface WarpJumpRequest {
  destinationX: number | null;
  destinationY: number | null;
}

export interface GalaxySectorState {
  x: number;
  y: number;
  enemyCount: number;
  planetCount: number;
  baseCount: number;
  visited: boolean;
  scanned: boolean;
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
  queuedDestinationX: number | null;
  queuedDestinationY: number | null;
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
