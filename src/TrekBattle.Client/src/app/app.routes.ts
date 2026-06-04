import { Routes } from '@angular/router';

import { GameLaunchPageComponent } from './pages/game-launch.page';
import { GameSetupPageComponent } from './pages/game-setup.page';

export const routes: Routes = [
  {
    path: '',
    component: GameSetupPageComponent,
    title: 'TrekBattle | Game Startup',
  },
  {
    path: 'launch/:resumeCode',
    component: GameLaunchPageComponent,
    title: 'TrekBattle | Launch',
  },
  {
    path: '**',
    redirectTo: '',
  },
];
