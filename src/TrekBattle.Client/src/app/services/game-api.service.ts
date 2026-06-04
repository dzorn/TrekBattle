import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
  GameSessionState,
  ResumeGameRequest,
  StartGameRequest,
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
}
