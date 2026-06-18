import { Routes } from '@angular/router';

import { GameLaunchPageComponent } from './pages/game-launch.page';
import { GalaxyNavigationPageComponent } from './pages/galaxy-navigation.page';
import { GameSetupPageComponent } from './pages/game-setup.page';

export const routes: Routes = [
  {
    path: '',
    component: GameSetupPageComponent,
    title: 'Galaxy Battle | Game Startup',
  },
  {
    path: 'launch/:resumeCode',
    component: GameLaunchPageComponent,
    title: 'Galaxy Battle | Launch',
  },
  {
    path: 'galaxy/:resumeCode',
    component: GalaxyNavigationPageComponent,
    title: 'Galaxy Battle | Galaxy',
  },
  {
    path: '**',
    redirectTo: '',
  },
];
