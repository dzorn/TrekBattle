import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { GameSessionState } from '../models/game-session.model';
import { GameApiService } from '../services/game-api.service';

@Component({
  standalone: true,
  selector: 'tb-game-launch-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './game-launch.page.html',
  styleUrl: './game-launch.page.scss',
})
export class GameLaunchPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly gameApi = inject(GameApiService);

  protected readonly session = signal<GameSessionState | null>(null);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  constructor() {
    const resumeCode = this.route.snapshot.paramMap.get('resumeCode');

    if (!resumeCode) {
      void this.router.navigate(['/']);
      return;
    }

    this.gameApi.loadSession(resumeCode).subscribe({
      next: session => {
        this.session.set(session);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set('We could not load that session. Try the resume code again.');
        this.loading.set(false);
      },
    });
  }
}
