import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
  GameSessionState,
  ResumeGameRequest,
  StartGameRequest,
  WarpJumpRequest,
} from '../models/game-session.model';

@Injectable({ providedIn: 'root' })
export class GameApiService {
  private readonly http = inject(HttpClient);

  createSession(request: StartGameRequest): Observable<GameSessionState> {
    return this.http.post<GameSessionState>('/api/sessions', request);
  }

  resumeSession(request: ResumeGameRequest): Observable<GameSessionState> {
    return this.http.post<GameSessionState>('/api/sessions/resume', request);
  }

  loadSession(resumeCode: string): Observable<GameSessionState> {
    return this.resumeSession({ resumeCode });
  }

  activateGalaxyView(resumeCode: string): Observable<GameSessionState> {
    return this.http.post<GameSessionState>(`/api/sessions/${encodeURIComponent(resumeCode)}/activate-galaxy`, {});
  }

  toggleLongRangeScan(resumeCode: string): Observable<GameSessionState> {
    return this.http.post<GameSessionState>(`/api/sessions/${encodeURIComponent(resumeCode)}/lrs`, {});
  }

  warpJump(resumeCode: string, request: WarpJumpRequest): Observable<GameSessionState> {
    return this.http.post<GameSessionState>(`/api/sessions/${encodeURIComponent(resumeCode)}/warp`, request);
  }

  endTurn(resumeCode: string): Observable<GameSessionState> {
    return this.http.post<GameSessionState>(`/api/sessions/${encodeURIComponent(resumeCode)}/end-turn`, {});
  }
}
