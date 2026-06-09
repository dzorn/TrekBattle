import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { GameApiService } from '../services/game-api.service';

@Component({
  standalone: true,
  selector: 'tb-game-setup-page',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './game-setup.page.html',
  styleUrl: './game-setup.page.scss',
})
export class GameSetupPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly gameApi = inject(GameApiService);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly newGameForm = this.fb.nonNullable.group({
    playerName: ['', [Validators.required, Validators.maxLength(100)]],
    shipName: ['', [Validators.required, Validators.maxLength(100)]],
  });

  protected readonly resumeForm = this.fb.nonNullable.group({
    resumeCode: ['', [Validators.required, Validators.maxLength(32)]],
  });

  protected startNewGame(): void {
    if (this.newGameForm.invalid) {
      this.newGameForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { playerName, shipName } = this.newGameForm.getRawValue();

    this.gameApi.createSession({ playerName, shipName }).subscribe({
      next: session => {
        console.info('[TrekBattle] Created session', {
          sessionId: session.sessionId,
          resumeCode: session.resumeCode,
        });
        void this.router.navigate(['/launch', session.resumeCode]).catch(error => {
          console.error('[TrekBattle] Navigation to launch screen failed.', error);
          this.errorMessage.set('The session was created, but the launch screen could not open.');
          this.isSubmitting.set(false);
        });
      },
      error: error => {
        console.error('[TrekBattle] Failed to create a new session.', error);
        this.errorMessage.set('We could not start the new mission. Please try again.');
        this.isSubmitting.set(false);
      },
    });
  }

  protected resumeExistingGame(): void {
    if (this.resumeForm.invalid) {
      this.resumeForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { resumeCode } = this.resumeForm.getRawValue();

    this.gameApi.resumeSession({ resumeCode }).subscribe({
      next: session => {
        console.info('[TrekBattle] Resumed session', {
          sessionId: session.sessionId,
          resumeCode: session.resumeCode,
        });
        void this.router.navigate(['/launch', session.resumeCode]).catch(error => {
          console.error('[TrekBattle] Navigation to launch screen failed.', error);
          this.errorMessage.set('The session was restored, but the launch screen could not open.');
          this.isSubmitting.set(false);
        });
      },
      error: error => {
        console.error('[TrekBattle] Failed to resume session.', error);
        this.errorMessage.set('We could not find a saved session for that resume code.');
        this.isSubmitting.set(false);
      },
    });
  }

  protected get playerNameControl() {
    return this.newGameForm.controls.playerName;
  }

  protected get shipNameControl() {
    return this.newGameForm.controls.shipName;
  }

  protected get resumeCodeControl() {
    return this.resumeForm.controls.resumeCode;
  }
}
