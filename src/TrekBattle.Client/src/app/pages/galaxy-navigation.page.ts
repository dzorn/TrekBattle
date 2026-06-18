import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { GalaxyMapState, GalaxySectorState, GameSessionState } from '../models/game-session.model';
import { GameApiService } from '../services/game-api.service';

const GalaxyWidth = 12;
const GalaxyHeight = 12;
const JumpRange = 5;
const GridOffsets = Array.from({ length: JumpRange * 2 + 1 }, (_, index) => index - JumpRange);

@Component({
  standalone: true,
  selector: 'tb-galaxy-navigation-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './galaxy-navigation.page.html',
  styleUrl: './galaxy-navigation.page.scss',
})
export class GalaxyNavigationPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly gameApi = inject(GameApiService);

  protected readonly session = signal<GameSessionState | null>(null);
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly selectedDestination = signal<{ x: number; y: number } | null>(null);
  protected readonly navigationRadius = JumpRange;

  protected readonly galaxyRows = Array.from({ length: GalaxyHeight }, (_, index) => index);
  protected readonly galaxyCols = Array.from({ length: GalaxyWidth }, (_, index) => index);
  protected readonly navigationOffsets = GridOffsets;

  constructor() {
    const resumeCode = this.route.snapshot.paramMap.get('resumeCode');

    if (!resumeCode) {
      void this.router.navigate(['/']);
      return;
    }

    this.gameApi.activateGalaxyView(resumeCode).subscribe({
      next: session => {
        this.session.set(session);
        this.loading.set(false);
      },
      error: () => {
        this.statusMessage.set('We could not load the galaxy view for that session.');
        this.loading.set(false);
      },
    });
  }

  protected toggleLongRangeScan(): void {
    const game = this.session();

    if (!game || this.busy()) {
      return;
    }

    this.busy.set(true);
    this.statusMessage.set(null);

    this.gameApi.toggleLongRangeScan(game.resumeCode).subscribe({
      next: session => {
        this.session.set(session);
        this.statusMessage.set(session.galaxyMap.actionUsed ? 'Long range scan queued.' : 'Long range scan cleared.');
        this.busy.set(false);
      },
      error: error => {
        console.error('[TrekBattle] Long range scan failed.', error);
        this.statusMessage.set('Unable to update the action toggle. Try again.');
        this.busy.set(false);
      },
    });
  }

  protected selectWarpDestination(gridX: number, gridY: number): void {
    const game = this.session();

    if (!game || this.busy()) {
      return;
    }

    const selected = this.selectedDestination();

    if (selected && selected.x === gridX && selected.y === gridY) {
      this.selectedDestination.set(null);
      this.statusMessage.set('Movement destination cleared.');
      return;
    }

    if (this.isNavigationCenter(gridX, gridY)) {
      this.selectedDestination.set(null);
      this.statusMessage.set('Movement destination cleared.');
      return;
    }

    const offsetX = gridX - JumpRange;
    const offsetY = gridY - JumpRange;
    const destinationX = game.galaxyMap.currentX + offsetX;
    const destinationY = game.galaxyMap.currentY + offsetY;

    if (game.galaxyMap.movementUsed) {
      this.selectedDestination.set({ x: gridX, y: gridY });
      this.statusMessage.set(`Movement destination selected at range ${offsetX},${offsetY}.`);
      return;
    }

    this.busy.set(true);
    this.statusMessage.set(null);

    this.gameApi.warpJump(game.resumeCode, {
      destinationX,
      destinationY,
    }).subscribe({
      next: session => {
        this.session.set(session);
        this.selectedDestination.set({ x: gridX, y: gridY });
        this.statusMessage.set(`Warp jump complete at ${this.locationLabel(session.galaxyMap.currentX, session.galaxyMap.currentY)}.`);
        this.busy.set(false);
      },
      error: error => {
        console.error('[TrekBattle] Warp jump failed.', error);
        this.statusMessage.set('Warp jump failed. Check the destination and try again.');
        this.busy.set(false);
      },
    });
  }

  protected endTurn(): void {
    const game = this.session();

    if (!game || this.busy()) {
      return;
    }

    this.busy.set(true);
    this.statusMessage.set(null);

    const hadQueuedAction = game.galaxyMap.actionUsed;

    this.gameApi.endTurn(game.resumeCode).subscribe({
      next: session => {
        this.session.set(session);
        this.selectedDestination.set(null);

        if (hadQueuedAction) {
          this.statusMessage.set(`Long range scan resolved. Turn ${session.galaxyMap.completedTurns} completed.`);
        } else {
          this.statusMessage.set(`Turn ${session.galaxyMap.completedTurns} completed.`);
        }

        this.busy.set(false);
      },
      error: error => {
        console.error('[TrekBattle] End turn failed.', error);
        this.statusMessage.set('End turn failed. Check the selected destination and try again.');
        this.busy.set(false);
      },
    });
  }

  protected get sectorRows(): number[] {
    return this.galaxyRows;
  }

  protected get sectorCols(): number[] {
    return this.galaxyCols;
  }

  protected get map(): GalaxyMapState | null {
    return this.session()?.galaxyMap ?? null;
  }

  protected get currentSector(): GalaxySectorState | null {
    const map = this.map;

    if (!map) {
      return null;
    }

    return this.getSector(map.currentX, map.currentY);
  }

  protected get actionQueued(): boolean {
    return this.map?.actionUsed ?? false;
  }

  protected get movementQueued(): boolean {
    return this.selectedDestination() !== null;
  }

  protected get turnCounter(): number {
    return this.map?.completedTurns ?? 0;
  }

  protected get currentLocationText(): string {
    const map = this.map;

    return map ? this.locationLabel(map.currentX, map.currentY) : 'Unknown';
  }

  protected get navigationGrid(): Array<Array<{ x: number; y: number; offsetX: number; offsetY: number }>> {
    return this.navigationOffsets.map((offsetY, rowIndex) =>
      this.navigationOffsets.map((offsetX, columnIndex) => ({
        x: columnIndex,
        y: rowIndex,
        offsetX,
        offsetY,
      }))
    );
  }

  protected getGalaxySector(x: number, y: number): GalaxySectorState | null {
    return this.getSector(x, y);
  }

  protected isCurrentSector(x: number, y: number): boolean {
    const map = this.map;
    return map ? map.currentX === x && map.currentY === y : false;
  }

  protected sectorTitle(sector: GalaxySectorState | null): string {
    if (!sector) {
      return 'Uncharted';
    }

    return `K${sector.enemyCount} P${sector.planetCount} B${sector.baseCount}`;
  }

  protected sectorHasScanInfo(sector: GalaxySectorState | null): boolean {
    if (!sector) {
      return false;
    }

    return sector.visited || sector.scanned || sector.enemyCount !== 0 || sector.planetCount !== 0 || sector.baseCount !== 0;
  }

  private getSector(x: number, y: number): GalaxySectorState | null {
    const map = this.map;

    if (!map) {
      return null;
    }

    return map.sectors.find(sector => sector.x === x && sector.y === y) ?? null;
  }

  private locationLabel(x: number, y: number): string {
    return `${x + 1},${y + 1}`;
  }

  protected isNavigationCenter(x: number, y: number): boolean {
    return x === JumpRange && y === JumpRange;
  }

  protected isSelectedDestination(x: number, y: number): boolean {
    const selected = this.selectedDestination();
    return selected ? selected.x === x && selected.y === y : false;
  }

}
