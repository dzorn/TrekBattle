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

  protected performLongRangeScan(): void {
    const game = this.session();

    if (!game || this.busy() || game.galaxyMap.actionUsed) {
      return;
    }

    this.busy.set(true);
    this.statusMessage.set(null);

    this.gameApi.performLongRangeScan(game.resumeCode).subscribe({
      next: session => {
        this.session.set(session);
        this.statusMessage.set('Long range scan updated the map.');
        this.busy.set(false);
      },
      error: error => {
        console.error('[TrekBattle] Long range scan failed.', error);
        this.statusMessage.set('Long range scan failed. Try again.');
        this.busy.set(false);
      },
    });
  }

  protected warpTo(destinationX: number, destinationY: number): void {
    const game = this.session();

    if (!game || this.busy() || game.galaxyMap.movementUsed || !this.canWarpTo(destinationX, destinationY)) {
      return;
    }

    this.busy.set(true);
    this.statusMessage.set(null);

    this.gameApi.warpJump(game.resumeCode, { destinationX, destinationY }).subscribe({
      next: session => {
        this.session.set(session);
        this.statusMessage.set(`Warp jump complete. Arrived at ${this.locationLabel(session.galaxyMap.currentX, session.galaxyMap.currentY)}.`);
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

    this.gameApi.endTurn(game.resumeCode).subscribe({
      next: session => {
        this.session.set(session);
        this.statusMessage.set(`Turn ${session.galaxyMap.completedTurns} completed.`);
        this.busy.set(false);
      },
      error: error => {
        console.error('[TrekBattle] End turn failed.', error);
        this.statusMessage.set('End turn failed. The ship computer did not commit the turn.');
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

  protected get actionReady(): boolean {
    return !(this.map?.actionUsed ?? true);
  }

  protected get movementReady(): boolean {
    return !(this.map?.movementUsed ?? true);
  }

  protected get turnCounter(): number {
    return this.map?.completedTurns ?? 0;
  }

  protected get currentLocationText(): string {
    const map = this.map;

    return map ? this.locationLabel(map.currentX, map.currentY) : 'Unknown';
  }

  protected get navigationGrid(): Array<Array<{ x: number; y: number; inBounds: boolean }>> {
    const map = this.map;

    if (!map) {
      return [];
    }

    return this.navigationOffsets.map(offsetY =>
      this.navigationOffsets.map(offsetX => {
        const x = map.currentX + offsetX;
        const y = map.currentY + offsetY;

        return {
          x,
          y,
          inBounds: this.isInBounds(x, y),
        };
      })
    );
  }

  protected getGalaxySector(x: number, y: number): GalaxySectorState | null {
    return this.getSector(x, y);
  }

  protected canWarpTo(x: number, y: number): boolean {
    const map = this.map;

    if (!map) {
      return false;
    }

    if (!this.isInBounds(x, y)) {
      return false;
    }

    const deltaX = Math.abs(x - map.currentX);
    const deltaY = Math.abs(y - map.currentY);

    return Math.max(deltaX, deltaY) <= map.jumpRange && !(x === map.currentX && y === map.currentY);
  }

  protected isCurrentSector(x: number, y: number): boolean {
    const map = this.map;
    return map ? map.currentX === x && map.currentY === y : false;
  }

  protected sectorTitle(sector: GalaxySectorState | null): string {
    if (!sector) {
      return 'Uncharted';
    }

    if (!sector.visited) {
      return 'Fog';
    }

    return `K${sector.enemyCount} P${sector.planetCount} B${sector.baseCount}`;
  }

  protected sectorSubtitle(sector: GalaxySectorState | null): string {
    if (!sector) {
      return 'Outside boundary';
    }

    if (!sector.visited) {
      return 'No scan data';
    }

    return `Sector ${this.locationLabel(sector.x, sector.y)}`;
  }

  private getSector(x: number, y: number): GalaxySectorState | null {
    const map = this.map;

    if (!map) {
      return null;
    }

    return map.sectors.find(sector => sector.x === x && sector.y === y) ?? null;
  }

  private isInBounds(x: number, y: number): boolean {
    return x >= 0 && x < GalaxyWidth && y >= 0 && y < GalaxyHeight;
  }

  private locationLabel(x: number, y: number): string {
    return `${x + 1},${y + 1}`;
  }
}
