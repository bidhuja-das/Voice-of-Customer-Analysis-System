import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { FeedbackListComponent } from './pages/feedback-list/feedback-list.component';
import { TopicsInsightsComponent } from './pages/topics-insights/topics-insights.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'feedback',
    component: FeedbackListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'insights',
    component: TopicsInsightsComponent,
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: 'login' }
];